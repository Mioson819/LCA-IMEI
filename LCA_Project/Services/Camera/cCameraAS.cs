using Guna.UI2.AnimatorNS;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
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
    private byte[] buffer = new byte[1024];
    private System.Threading.Timer pingTimer;
    public bool IsConnected { get; private set; }
    public bool isCalib = true;
    public string[] NeedleData;
    public int TriggerOn = 0;
    public int CalibStep = 1;
    public string[] xya;
    public delegate void SendResult(string[] s,string NameStaton);
    public event SendResult _send;
    public event EventHandler _StartCalib;
    public bool changejob = true;
    private string nameStation { get; set; }
    private object _lock = new object();
    private object _lockReice = new object();
    private bool StatusChangeJob=true;
    private bool StatusSO = false;
    private string Idmodel{  get; set; }
    private string IdPLC { get; set; }
    public delegate void StatusJob(string s1,string s2);
    public StatusJob _StatusJob;
    public string NamePort;
    public CameraAS(string ip, int port)
    {
        _ip = ip;
        _port = port;
        // Ping mỗi 2 giây
        pingTimer = new System.Threading.Timer(PingTimerCallback, null, 0, 2000);
    }
    private void PingTimerCallback(object state)
    {
        try
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(_ip, 500);
            if (reply.Status == IPStatus.Success)
            {
                if (!IsConnected)
                    Connect();
            }
            else
            {
                if (IsConnected)
                    DisconnectSocketOnly();
            }
        }
        catch
        {
            if (IsConnected)
                DisconnectSocketOnly();
        }
    }
    public void Connect()
    {
        try
        {
            DisconnectSocketOnly(); 
            xya = new string[3];
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(_ip), _port));
            try
            {
                _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), null);
            }
            catch {
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
        DisconnectSocketOnly();
        pingTimer?.Dispose();
    }
    private void DisconnectSocketOnly()
    {
        try
        {
            if (_clientSocket != null && _clientSocket.Connected)
            {
                try { _clientSocket.Shutdown(SocketShutdown.Both); } catch { }
                try { _clientSocket.Close(); } catch { }
                try { _clientSocket.Dispose();} catch { }
            }
            IsConnected = false;
            Console.WriteLine("TCP disconnected from camera!");
        }
        catch { }
    }
    private void ReceiveData(IAsyncResult ar)
    {
        try
        {
            lock (_lockReice)
            {
                var sock = _clientSocket;
                if (sock == null) return;
                int bytesRead;
                try
                {
                    bytesRead = _clientSocket.EndReceive(ar);
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
                if (bytesRead > 0)
                {
                    string stringData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"{stringData}");
                    //------------------
                    if (stringData.Contains("Welcome")&& Idmodel != ""&&IdPLC!="")
                    {
                    }
                    else if (stringData == "User: "&&Idmodel!= "" && IdPLC != "")
                    {
                        SendCommand($"admin\r\n");
                    }
                    else if (stringData == "Password: " && Idmodel!= "" && IdPLC != "")
                    {
                        SendCommand($"\r\n");
                    }
                    else if (stringData == "User Logged In\r\n" && Idmodel!= "" && IdPLC != "")
                    {
                         SendCommand($"SO0\r\n");
                    }
                    else if (stringData == "1\r\n" && Idmodel!= "" && IdPLC != " ")
                    {
                        if (StatusChangeJob)
                        {
                            StatusChangeJob=false;
                            StatusSO=true;
                            SendCommand($"LF{Idmodel}_{IdPLC}.job\r\n");
                            _StatusJob?.Invoke("Watting",NamePort);
                        }
                        else if(StatusSO)
                        {
                            StatusSO=false;
                            SendCommand($"SO1\r\n");
                            //Console.WriteLine("SO1");
                        }
                        else if(StatusChangeJob==false&&StatusSO==false)
                        {
                            StatusChangeJob = true;
                            StatusSO = true;
                            Reconnect(Idmodel, IdPLC,7890,"","");
                            Idmodel= "";
                            IdPLC = "";
                            _StatusJob?.Invoke("Success",NamePort);
                        }
                    }
                    else if (stringData=="2\r\n")
                    {
                        _StatusJob?.Invoke("Error",NamePort);
                    }
                    else
                    {
                        string stringSeparators = stringData.TrimEnd('\r', '\n');
                        string[] lines = stringSeparators.Split(',');
                        if (lines.Length > 1)
                        {
                            // FLOW TỰ ĐỘNG CALIB
                            Console.WriteLine("CHECK");
                            if (lines[0] == "GCP" && lines[1] == "1")
                            {
                                if (lines.Length >= 2)
                                {
                                    var s = lines[2].Split('.');
                                    xya[0] = s[0].Trim() + s[1].Trim();
                                    var s2 = lines[3].Split('.');
                                    xya[1] = s2[0].Trim() + s2[1].Trim();
                                    var s3 = lines[4].Split('.');
                                    xya[2] = s3[0].Trim() + s3[1].Trim();
                                    // Console.WriteLine("Received: " + xya[2]);
                                    _send?.Invoke(xya, nameStation);
                                    LogProgram.WriteLog($"PC send Data to " + $"{nameStation} " +"Postion :" + $"{xya[0]} " + $"{xya[1]} " + $"{xya[2]} ");
                                }
                            }
                            else if (lines[0] == "GS" && lines[1] == "1")
                            {
                                MessageBox.Show("Calib Hand Eye is running");
                            }
                            else if (lines[0] == "HE" && lines[1] == "1")
                            {
                                if (CalibStep == 10)
                                {
                                    MessageBox.Show("Calib Hand Eye is Step 10, Please chose Rotato - ");
                                }
                                else if (CalibStep == 11)
                                {
                                    MessageBox.Show("Calib Hand Eye is Step 11, Please chose Rotato + ");
                                }
                                CalibStep++;
                            }
                            try
                            {
                                if (_clientSocket != null && _clientSocket.Connected)
                                    _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), null);
                            }
                            catch
                            {
                                DisconnectSocketOnly();
                            }
                            return;
                        }
                        //  Console.WriteLine("Received: " + stringData.Trim());
                    }
                    //  Thread.Sleep(300);
                    try
                    {
                        if (_clientSocket != null && _clientSocket.Connected)
                            _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), null);
                    }
                    catch
                    {
                        DisconnectSocketOnly();
                    }
                }
            }
        }
        catch
        {
            DisconnectSocketOnly();
        }
    }
    // Gửi lệnh dạng string
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
                catch { 
                    DisconnectSocketOnly() ;
                }
            }
        }
    }
    // Gửi lệnh dạng byte[] nếu cần
    public void SendToServer(byte[] data)
    {
        if (_clientSocket != null && _clientSocket.Connected)
        {
            _clientSocket.Send(data);
        }
    }
    // Hàm xử lý data nhận được (tuỳ biến theo nhu cầu)
    private void HandleData(string[] data)
    {
        NeedleData = data;
        TriggerOn = 2;
        // Bạn có thể xử lý/hiển thị/log data tại đây
    }
    // Trigger camera
    public void Trigger()
    {
        SendCommand("GCP,1,Cam2D,0,0,0,0,0,0");
    }
    // Bắt đầu hiệu chuẩn
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
            isCalib=false;
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
        SendCommand("GCP,1,HOME2D,0,0,0,0,0,0");
        nameStation = s;
    }
    public void ChangeJob(int Idjob)
    {
        SendCommand($"LF{Idjob}_{Idjob}.job");
    }
    public void ChangFearture(string s)
    {
        SendCommand("GCP,2,HOME2D,0,0,0,0,0,0");
        nameStation = s;
    }
    public void Reconnect(string IdModel,string IdPLC,int port,string NamePort1,string NamePort2)
    {
        _port = port;
        this.Idmodel= IdModel;
        this.IdPLC = IdPLC;
        NamePort = NamePort1 + "         " + NamePort2;
        Connect();
    }
}
