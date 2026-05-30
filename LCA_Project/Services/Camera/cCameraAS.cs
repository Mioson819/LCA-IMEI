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
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(_ip), _port));
            // FIX Bug A: cấp phát buffer cục bộ mới cho mỗi lần BeginReceive
            byte[] localBuffer = new byte[1024];
            try
            {
                _clientSocket.BeginReceive(localBuffer, 0, localBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveData), localBuffer);
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
            Console.WriteLine("TCP disconnected from camera!");
        }
        catch { }
    }
    private void ReceiveData(IAsyncResult ar)
    {
        // FIX Bug A: lấy buffer từ AsyncState (cục bộ của lần BeginReceive này)
        byte[] localBuffer = (byte[])ar.AsyncState;
        try
        {
            lock (_lockReice)
            {
                var sock = _clientSocket;
                if (sock == null) return;
                int bytesRead;
                try
                {
                    bytesRead = sock.EndReceive(ar);
                }
                catch (ObjectDisposedException)
                {
                    DisconnectSocketOnly();
                    return;
                }
                catch (SocketException)
                {
                    DisconnectSocketOnly();
                    return;
                }
                catch (Exception)
                {
                    DisconnectSocketOnly();
                    return;
                }
                if (bytesRead == 0)
                {
                    // TCP FIN nhận được: camera chủ động đóng kết nối.
                    // Phải disconnect để IsConnected = false và ping timer sẽ reconnect.
                    // Không xử lý sẽ khiến IsConnected = true nhưng không còn BeginReceive
                    // nào được queue → camera "sống" nhưng bỏ lỡ mọi trigger response.
                    DisconnectSocketOnly();
                    return;
                }

                string stringData = Encoding.ASCII.GetString(localBuffer, 0, bytesRead);
                Console.WriteLine($"{stringData}");
                if (stringData.Contains("Welcome") && Idmodel != "" && IdPLC != "")
                {
                }
                else if (stringData == "User: " && Idmodel != "" && IdPLC != "")
                {
                    SendCommand($"admin\r\n");
                }
                else if (stringData == "Password: " && Idmodel != "" && IdPLC != "")
                {
                    SendCommand($"\r\n");
                }
                else if (stringData == "User Logged In\r\n" && Idmodel != "" && IdPLC != "")
                {
                    SendCommand($"SO0\r\n");
                }
                else if (stringData == "1\r\n" && Idmodel != "" && IdPLC != " ")
                {
                    if (StatusChangeJob)
                    {
                        StatusChangeJob = false;
                        StatusSO = true;
                        SendCommand($"LF{Idmodel}_{IdPLC}.job\r\n");
                        _StatusJob?.Invoke("Watting", NamePort);
                    }
                    else if (StatusSO)
                    {
                        StatusSO = false;
                        SendCommand($"SO1\r\n");
                    }
                    else if (StatusChangeJob == false && StatusSO == false)
                    {
                        StatusChangeJob = true;
                        StatusSO = true;
                        Reconnect(Idmodel, IdPLC, 7890, "", "");
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

                                    // Dequeue lấy đúng station của lệnh GCP này (FIFO).
                                    // Dùng Queue thay vì nameStation volatile để tránh race
                                    // khi Station1 và Station2 cùng trigger cam1 gần nhau.
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
                                // Bỏ lệnh này ra khỏi queue để không lệch thứ tự
                                lock (_pendingLock)
                                {
                                    if (_pendingStations.Count > 0) _pendingStations.Dequeue();
                                }
                            }
                        }
                        else if (lines[0] == "GS" && lines[1] == "1")
                        {
                            // KHÔNG dùng MessageBox ở đây — ReceiveData chạy trên socket async callback
                            // (threadpool), đang giữ _lockReice. MessageBox block thread + lock →
                            // BeginReceiveNext không được gọi → camera điếc cho đến khi user click OK.
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
                BeginReceiveNext(sock);
            }
        }
        catch
        {
            DisconnectSocketOnly();
        }
    }
    // FIX Bug A: helper tạo buffer mới cho mỗi lần BeginReceive
    private void BeginReceiveNext(Socket sock)
    {
        try
        {
            if (sock != null && sock.Connected)
            {
                byte[] newBuffer = new byte[1024];
                sock.BeginReceive(newBuffer, 0, newBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveData), newBuffer);
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