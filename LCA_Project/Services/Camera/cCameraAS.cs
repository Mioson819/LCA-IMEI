using Guna.UI2.AnimatorNS;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using LCA_Project.Utilities;
public class CameraAS : IDisposable
{
    private string _ip;
    private int _port;
    private Socket _clientSocket;
    // FIX Bug A: buffer cục bộ trong ReceiveData thay vì field dùng chung
    private System.Threading.Timer pingTimer;
    public bool IsConnected { get; private set; }
    public bool isCalib = true;
    public string[] NeedleData;
    public int TriggerOn = 0;
    public int CalibStep = 1;
    // FIX Bug C: xya không còn là field dùng chung — data được copy trước khi invoke
    public delegate void SendResult(string[] s, string NameStaton);
    public event SendResult _send;
    public event EventHandler _StartCalib;
    public bool changejob = true;
    // Queue FIFO ghép mỗi lệnh GCP gửi đi với station tương ứng.
    // Khi camera phản hồi (ReceiveData), Dequeue lấy đúng station của lệnh đó.
    // Bắt buộc vì cam1 dùng chung cho Station1 và Station2: nếu cả hai trigger
    // cùng lúc và dùng volatile string thì Station1 sẽ nhận kết quả của Station2.
    private readonly Queue<string> _pendingStations = new Queue<string>();
    private readonly object _pendingLock = new object();
    private volatile string nameStation; // fallback khi queue rỗng (không nên xảy ra)
    private readonly object _lock = new object();
    private readonly object _lockReice = new object();
    // FIX Bug E: _connectLock bảo vệ Connect/Disconnect khỏi race với PingTimer
    private readonly object _connectLock = new object();
    // BUG-5 FIX: generation counter — mỗi khi ConnectInternal tạo socket mới, tăng generation.
    // ReceiveData callback truyền generation lúc BeginReceive; nếu không khớp → stale callback
    // từ socket cũ, bỏ qua để tránh DisconnectSocketOnly() nhầm lên kết nối mới.
    private volatile int _socketGeneration = 0;
    // Helper class: gom buffer + generation vào 1 object để truyền qua AsyncState
    private class ReceiveState
    {
        public byte[] Buffer;
        public int Generation;
    }
    private bool StatusChangeJob = true;
    private bool StatusSO = false;
    private string Idmodel { get; set; }
    private string IdPLC { get; set; }
    public delegate void StatusJob(string s1, string s2);
    public StatusJob _StatusJob;
    public string NamePort;
    public CameraAS(string ip, int port)
    {
        _ip = ip;
        _port = port;
        // Ping mỗi 15 giây — đủ để phát hiện mất kết nối mà không tạo tải network.
        // Nếu camera mất kết nối, sẽ phát hiện và reconnect trong tối đa 15 giây.
        pingTimer = new System.Threading.Timer(PingTimerCallback, null, 15000, 15000);
    }
    private void PingTimerCallback(object state)
    {
        // FIX Bug E: dùng lock để tránh race Connect/Disconnect với ReceiveData
        lock (_connectLock)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(_ip, 500);
                    if (reply.Status == IPStatus.Success)
                    {
                        if (!IsConnected)
                            ConnectInternal();
                    }
                    else
                    {
                        if (IsConnected)
                            DisconnectSocketOnly();
                    }
                }
            }
            catch
            {
                if (IsConnected)
                    DisconnectSocketOnly();
            }
        }
    }
    public void Connect()
    {
        lock (_connectLock)
        {
            ConnectInternal();
        }
    }
    // Phần thực sự kết nối — gọi bên trong _connectLock
    private void ConnectInternal()
    {
        try
        {
            DisconnectSocketOnly();
            // BUG-5 FIX: tăng generation mỗi khi tạo socket mới.
            // Callback ReceiveData từ socket cũ sẽ thấy generation không khớp và tự bỏ qua.
            _socketGeneration++;
            int gen = _socketGeneration;
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // BUG-1 (camera): dùng BeginConnect + WaitOne(3s) thay vì Connect() đồng bộ
            // tránh block _connectLock trong lúc PingTimerCallback đang chờ kết nối.
            IAsyncResult connectAr = _clientSocket.BeginConnect(
                new IPEndPoint(IPAddress.Parse(_ip), _port), null, null);
            bool ok = connectAr.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
            if (!ok)
            {
                try { _clientSocket.Close(); } catch { }
                IsConnected = false;
                Console.WriteLine("Camera connect timeout");
                return;
            }
            _clientSocket.EndConnect(connectAr);
            var state = new ReceiveState { Buffer = new byte[1024], Generation = gen };
            try
            {
                _clientSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveData), state);
            }
            catch
            {
                DisconnectSocketOnly();
                throw;
            }
            IsConnected = true;
            Console.WriteLine("TCP connected to camera!");
        }
        catch (Exception ex)
        {
            IsConnected = false;
            Console.WriteLine("Connect failed: " + ex.Message);
        }
    }
    public void Disconnect()
    {
        lock (_connectLock)
        {
            DisconnectSocketOnly();
            pingTimer?.Dispose();
        }
    }
    private void DisconnectSocketOnly()
    {
        try
        {
            if (_clientSocket != null && _clientSocket.Connected)
            {
                try { _clientSocket.Shutdown(SocketShutdown.Both); } catch { }
                try { _clientSocket.Close(); } catch { }
                try { _clientSocket.Dispose(); } catch { }
            }
            IsConnected = false;
            // Clear queue khi mất kết nối để tránh dequeue nhầm station
            // cho trigger tiếp theo sau khi reconnect.
            lock (_pendingLock) { _pendingStations.Clear(); }
            Console.WriteLine("TCP disconnected from camera!");
        }
        catch { }
    }
    private void ReceiveData(IAsyncResult ar)
    {
        // BUG-5 FIX: lấy ReceiveState (buffer + generation) thay vì chỉ byte[]
        var state = (ReceiveState)ar.AsyncState;
        // Lệnh cần gửi ra ngoài lock — xác định trong lock, gửi sau khi release.
        // BUG-6 FIX: không giữ _lockReice trong lúc gọi SendCommand (có thể block
        // nếu TCP send-buffer đầy → camera điếc cho đến khi timeout).
        string commandToSend = null;
        bool shouldInvokeSuccess = false;
        string successIdmodel = null, successIdPLC = null;

        try
        {
            lock (_lockReice)
            {
                // BUG-5 FIX: callback từ socket thế hệ cũ (bị đóng bởi PingTimer) → bỏ qua.
                // Tránh DisconnectSocketOnly() nhầm lên kết nối MỚI đang hoạt động.
                if (state.Generation != _socketGeneration) return;

                var sock = _clientSocket;
                if (sock == null) return;
                int bytesRead;
                try
                {
                    bytesRead = sock.EndReceive(ar);
                }
                catch (ObjectDisposedException) { DisconnectSocketOnly(); return; }
                catch (SocketException)          { DisconnectSocketOnly(); return; }
                catch (Exception)                { DisconnectSocketOnly(); return; }

                if (bytesRead == 0)
                {
                    // TCP FIN — camera chủ động đóng kết nối, PingTimer sẽ reconnect.
                    DisconnectSocketOnly();
                    return;
                }

                string stringData = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
                Console.WriteLine($"{stringData}");

                if (stringData.Contains("Welcome") && Idmodel != "" && IdPLC != "")
                {
                    // không cần gửi gì
                }
                else if (stringData == "User: " && Idmodel != "" && IdPLC != "")
                {
                    // BUG-4 FIX: bỏ \r\n trong string — SendCommand tự thêm \r\n
                    commandToSend = "admin";
                }
                else if (stringData == "Password: " && Idmodel != "" && IdPLC != "")
                {
                    commandToSend = "";   // mật khẩu rỗng → SendCommand gửi "\r\n"
                }
                else if (stringData == "User Logged In\r\n" && Idmodel != "" && IdPLC != "")
                {
                    commandToSend = "SO0";
                }
                else if (stringData == "1\r\n" && Idmodel != "" && IdPLC != " ")
                {
                    if (StatusChangeJob)
                    {
                        StatusChangeJob = false;
                        StatusSO = true;
                        commandToSend = $"LF{Idmodel}_{IdPLC}.job";
                        _StatusJob?.Invoke("Watting", NamePort);
                    }
                    else if (StatusSO)
                    {
                        StatusSO = false;
                        commandToSend = "SO1";
                    }
                    else if (!StatusChangeJob && !StatusSO)
                    {
                        StatusChangeJob = true;
                        StatusSO = true;
                        // Lưu lại trước khi clear — dùng trong Reconnect bên ngoài lock
                        shouldInvokeSuccess = true;
                        successIdmodel = Idmodel;
                        successIdPLC = IdPLC;
                        Idmodel = "";
                        IdPLC = "";
                        _StatusJob?.Invoke("Success", NamePort);
                    }
                }
                else if (stringData == "2\r\n")
                {
                    _StatusJob?.Invoke("Error", NamePort);
                }
                else
                {
                    string stringSeparators = stringData.TrimEnd('\r', '\n');
                    string[] lines = stringSeparators.Split(',');
                    if (lines.Length > 1)
                    {
                        Console.WriteLine("CHECK");
                        if (lines[0] == "GCP" && lines[1] == "1")
                        {
                            if (lines.Length >= 5)
                            {
                                try
                                {
                                    string[] xyaLocal = new string[3];
                                    var s = lines[2].Split('.');
                                    xyaLocal[0] = s[0].Trim() + (s.Length > 1 ? s[1].Trim() : "");
                                    var s2 = lines[3].Split('.');
                                    xyaLocal[1] = s2[0].Trim() + (s2.Length > 1 ? s2[1].Trim() : "");
                                    var s3 = lines[4].Split('.');
                                    xyaLocal[2] = s3[0].Trim() + (s3.Length > 1 ? s3[1].Trim() : "");
                                    string stationSnapshot;
                                    lock (_pendingLock)
                                    {
                                        stationSnapshot = _pendingStations.Count > 0
                                            ? _pendingStations.Dequeue()
                                            : nameStation;
                                    }
                                    _send?.Invoke(xyaLocal, stationSnapshot);
                                    LogProgram.WriteLog($"PC send Data to {stationSnapshot} Position: {xyaLocal[0]} {xyaLocal[1]} {xyaLocal[2]}");
                                }
                                catch (Exception ex)
                                {
                                    LogProgram.WriteLog($"[CameraAS] Parse GCP data error: {ex.Message}");
                                }
                            }
                            else
                            {
                                LogProgram.WriteLog($"[CameraAS] GCP packet thiếu fields: '{stringSeparators}' (cần >= 5, nhận {lines.Length})");
                                lock (_pendingLock)
                                {
                                    if (_pendingStations.Count > 0) _pendingStations.Dequeue();
                                }
                            }
                        }
                        else if (lines[0] == "GS" && lines[1] == "1")
                        {
                            LogProgram.WriteLog("[CameraAS] Calib Hand Eye is running (GS,1 received)");
                        }
                        else if (lines[0] == "HE" && lines[1] == "1")
                        {
                            if (CalibStep == 10)
                                LogProgram.WriteLog("[CameraAS] Calib HE Step 10 — Please choose Rotate -");
                            else if (CalibStep == 11)
                                LogProgram.WriteLog("[CameraAS] Calib HE Step 11 — Please choose Rotate +");
                            CalibStep++;
                        }
                        BeginReceiveNext(sock);
                        return;
                    }
                }
                // Arm receive tiếp theo TRONG lock — trước khi release để không bỏ sót gói
                BeginReceiveNext(sock);
            }

            // BUG-6 FIX: gửi lệnh SAU KHI release _lockReice
            // → không block camera receive trong lúc TCP send chờ buffer
            if (commandToSend != null)
                SendCommand(commandToSend);

            // Reconnect (thay đổi job) cũng sau khi release lock
            if (shouldInvokeSuccess)
                Reconnect(successIdmodel, successIdPLC, 7890, "", "");
        }
        catch
        {
            DisconnectSocketOnly();
        }
    }
    // Helper tạo ReceiveState mới cho mỗi lần BeginReceive
    private void BeginReceiveNext(Socket sock)
    {
        try
        {
            if (sock != null && sock.Connected)
            {
                // BUG-5 FIX: snapshot generation hiện tại để callback sau biết thuộc socket nào
                var newState = new ReceiveState
                {
                    Buffer = new byte[1024],
                    Generation = _socketGeneration
                };
                sock.BeginReceive(newState.Buffer, 0, newState.Buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveData), newState);
            }
        }
        catch
        {
            DisconnectSocketOnly();
        }
    }
    public void SendCommand(string command)
    {
        lock (_lock)
        {
            if (_clientSocket != null && _clientSocket.Connected)
            {
                try
                {
                    Console.WriteLine($"---{command}");
                    byte[] data = Encoding.ASCII.GetBytes(command + "\r\n");
                    _clientSocket.Send(data);
                }
                catch
                {
                    DisconnectSocketOnly();
                }
            }
        }
    }
    public void SendToServer(byte[] data)
    {
        if (_clientSocket != null && _clientSocket.Connected)
        {
            _clientSocket.Send(data);
        }
    }
    private void HandleData(string[] data)
    {
        NeedleData = data;
        TriggerOn = 2;
    }
    public void Trigger()
    {
        SendCommand("GCP,1,Cam2D,0,0,0,0,0,0");
    }
    public void CalibCamAS()
    {
        isCalib = true;
        SendCommand("TA,1");
    }
    public void Dispose()
    {
        Disconnect();
    }
    public void StepCalib(float x, float y, float z, float a, float b, float c)
    {
        SendCommand($"HE,1,1,{x},{y},{z},{a},{b},{c}");
    }
    public void EndCalib()
    {
        SendCommand($"HEE,1");
        this.CalibStep = 1;
        isCalib = true;
    }
    public void SGP(int i)
    {
        SendCommand($"SGP,{i},Home2D,0,0,0");
    }
    public void StartCalib()
    {
        if (isCalib)
        {
            SendCommand("GS,1");
            isCalib = false;
            MessageBox.Show("Calib Hand Eye is running");
        }
        else
        {
            var result = MessageBox.Show("You need to finish the previous calibration first!\r\nClick OK to continue the current process or click NO to redo the cycle", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                isCalib = true;
                CalibStep = 1;
                SendCommand("GS,1");
            }
        }
    }
    public void TriggerResult(string s)
    {
        // Enqueue TRƯỚC SendCommand: camera có thể phản hồi rất nhanh (< 1ms trên LAN)
        // nếu enqueue sau thì ReceiveData có thể chạy trước khi station được queue.
        lock (_pendingLock) { _pendingStations.Enqueue(s); }
        nameStation = s;
        SendCommand("GCP,1,HOME2D,0,0,0,0,0,0");
    }
    public void ChangeJob(int Idjob)
    {
        SendCommand($"LF{Idjob}_{Idjob}.job");
    }
    public void ChangFearture(string s)
    {
        lock (_pendingLock) { _pendingStations.Enqueue(s); }
        nameStation = s;
        SendCommand("GCP,2,HOME2D,0,0,0,0,0,0");
    }
    public void Reconnect(string IdModel, string IdPLC, int port, string NamePort1, string NamePort2)
    {
        _port = port;
        this.Idmodel = IdModel;
        this.IdPLC = IdPLC;
        NamePort = NamePort1 + "         " + NamePort2;
        Connect();
    }
}