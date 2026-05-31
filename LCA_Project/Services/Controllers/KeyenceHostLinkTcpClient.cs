using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities.LogProgram;
namespace Project_Visionpro.Program.PLC
{
    public class KeyenceHostLinkTcpClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // FIX PLC-1/PLC-3: dùng một lock duy nhất cho toàn bộ TCP I/O
        // Thay vì nhiều lock riêng dễ gây deadlock và race, một _ioLock serialize
        // tất cả Send/Receive trên stream. SetBitInWord/ResetBitInWord (RMW) cũng
        // dùng chính lock này để toàn bộ chuỗi Read→OR/AND→Write là atomic.
        private readonly object _ioLock = new object();
        private volatile bool _isConnected = false;
        private volatile bool _isSessionStarted = false;
        // Chống timer-reset liên tiếp khi 4 Form1 đều báo lỗi cùng lúc:
        // chỉ khởi động timer reconnect một lần cho đến khi reconnect thành công.
        private bool _reconnectPending = false;
        protected virtual void PropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private TcpClient _client;
        private NetworkStream _stream;
        private string _ip;
        private int _port;
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private event EventHandler _reconnect;
        public bool IsSessionStarted
        {
            get { return _isSessionStarted; }
            private set { _isSessionStarted = value; }
        }
        public KeyenceHostLinkTcpClient(string ipAddress, int port)
        {
            _ip = ipAddress;
            _port = port;
            _timer.Interval = 3000;
            _timer.AutoReset = false;
        }
        public void Open()
        {
            try
            {
                IsSessionStarted = true;
                _client = new TcpClient();
                // FIX PLC-3: đặt timeout cho kết nối và đọc để tránh treo thread vô hạn
                _client.SendTimeout = 2000;
                _client.ReceiveTimeout = 2000;
                _client.Connect(_ip, _port);
                _stream = _client.GetStream();
                _stream.ReadTimeout = 2000;
                _stream.WriteTimeout = 2000;
            }
            catch (Exception ex)
            {
                IsSessionStarted = false;
                PropertyChangedEvent($"{Tcpstatus.disconnected}");
                LogProgram.WriteLog($"[HostLinkTCP] Connection failed: {ex.Message}");
            }
        }
        public void Close()
        {
            _stream?.Close();
            _client?.Close();
            PropertyChangedEvent($"{Tcpstatus.disconnected}");
            IsSessionStarted = false;
            LogProgram.WriteLog($"[HostLinkTCP] Disconnected from {_ip}:{_port}");
        }
        // FIX PLC-1/PLC-3: SendCommand KHÔNG giữ lock — caller phải giữ _ioLock
        // Điều này cho phép SetBitInWord (RMW) gọi SendCommand nhiều lần trong 1 lock
        private string SendCommandUnlocked(string command)
        {
            if (!IsSessionStarted || _stream == null)
                return "EX:Client is not connected";
            try
            {
                string fullCommand = command.Trim() + "\r";
                byte[] sendBytes = Encoding.ASCII.GetBytes(fullCommand);
                _stream.Write(sendBytes, 0, sendBytes.Length);
                byte[] buffer = new byte[256];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                string raw = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                var parts = raw.Split(new[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return "EX:NO RESPONSE";
                string status = parts[0];
                string data = parts.Length > 1 ? parts[1] : null;
                if (data == null) return status;
                return data;
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog($"[HostLinkTCP] Error sending command: {ex.Message}");
                IsSessionStarted = false;
                PropertyChangedEvent($"{Tcpstatus.disconnected}");
                return $"EX:{ex.Message}";
            }
        }
        // API công khai: acquire lock rồi gọi unlocked variant
        public string SendCommand(string command)
        {
            lock (_ioLock)
            {
                return SendCommandUnlocked(command);
            }
        }
        private TcpReceiveCMD ParseResponse(string response)
        {
            if (response == "OK" || response == "CC" || response == "00000") return TcpReceiveCMD.OK;
            if (response == "E0") return TcpReceiveCMD.E0;
            if (response == "E1") return TcpReceiveCMD.E1;
            if (response.StartsWith("EX:")) return TcpReceiveCMD.CheckCodeWrong;
            return TcpReceiveCMD.Unknow;
        }
        public bool StartCommunication()
        {
            string response = SendCommand("CR");
            var result = ParseResponse(response);
            IsSessionStarted = result == TcpReceiveCMD.OK;
            return IsSessionStarted;
        }
        // Gọi bên trong _ioLock. Chỉ khởi động timer lần đầu để tránh reset đếm ngược
        // mỗi khi một trong 4 Form1 gặp lỗi liên tiếp → reconnect bị trì hoãn vô hạn.
        private void StartReconnectIfNeeded()
        {
            if (_isConnected && !_reconnectPending)
            {
                _reconnectPending = true;
                _timer.Elapsed -= Reconnect;
                _timer.Elapsed += Reconnect;
                _timer.Start();
            }
        }
        private void MarkConnected()
        {
            _reconnectPending = false;
            _isConnected = true;
            _timer.Elapsed -= Reconnect;
            _timer.Stop();
            PropertyChangedEvent($"{Tcpstatus.connected}");
            IsSessionStarted = true;
        }
        // FIX PLC-2: helper xử lý response disconnect/reconnect — dùng chung cho tất cả Write
        private void HandleWriteResponse(string response)
        {
            if (response.Contains("not") || response.Contains("EX"))
            {
                PropertyChangedEvent($"{Tcpstatus.disconnected}");
                IsSessionStarted = false;
                StartReconnectIfNeeded();
            }
            else
            {
                MarkConnected();
            }
        }
        public bool WriteUInt16(string address, ushort value)
        {
            lock (_ioLock)
            {
                string response = SendCommandUnlocked($"WR {address}.U {value}");
                HandleWriteResponse(response);
                return ParseResponse(response) == TcpReceiveCMD.OK;
            }
        }
        public bool WriteInt32(string address, Int32 value)
        {
            lock (_ioLock)
            {
                string response = SendCommandUnlocked($"WR {address}.L {value}");
                HandleWriteResponse(response);
                return ParseResponse(response) == TcpReceiveCMD.OK;
            }
        }
        public ushort ReadUInt16(string address)
        {
            lock (_ioLock)
            {
                try
                {
                    string response = SendCommandUnlocked($"RD {address}.U");
                    if (ushort.TryParse(response, out ushort value))
                    {
                        MarkConnected();
                        return value;
                    }
                    if (response == "OK" || response == "00000")
                    {
                        LogProgram.WriteLog($"[PLC] ReadUInt16 {address}: phản hồi OK nhưng không có data");
                        return 0;
                    }
                    if (response.Contains("not") || response.Contains("EX"))
                    {
                        LogProgram.WriteLog($"[PLC] ReadUInt16 {address} lỗi kết nối: {response}");
                        PropertyChangedEvent($"{Tcpstatus.disconnected}");
                        StartReconnectIfNeeded();
                    }
                    else
                    {
                        MarkConnected();
                    }
                    return 0;
                }
                catch
                {
                    LogProgram.WriteLog($"[PLC] ReadUInt16 exception tại {address}");
                    return 0;
                }
            }
        }
        public short ReadInt16(string address)
        {
            lock (_ioLock)
            {
                try
                {
                    string response = SendCommandUnlocked($"RD {address}.S");
                    if (short.TryParse(response, out short value))
                    {
                        MarkConnected();
                        return value;
                    }
                    if (response == "OK" || response == "00000")
                    {
                        LogProgram.WriteLog($"[PLC] ReadInt16 {address}: phản hồi OK nhưng không có data");
                        return 0;
                    }
                    if (response.Contains("not") || response.Contains("EX"))
                    {
                        LogProgram.WriteLog($"[PLC] ReadInt16 {address} lỗi kết nối: {response}");
                        PropertyChangedEvent($"{Tcpstatus.disconnected}");
                        StartReconnectIfNeeded();
                    }
                    else
                    {
                        MarkConnected();
                    }
                    return 0;
                }
                catch
                {
                    LogProgram.WriteLog($"[PLC] ReadInt16 exception tại {address}");
                    return 0;
                }
            }
        }
        public bool WriteInt16(string address, short value)
        {
            lock (_ioLock)
            {
                try
                {
                    string response = SendCommandUnlocked($"WR {address}.S {value}");
                    HandleWriteResponse(response);
                    return ParseResponse(response) == TcpReceiveCMD.OK;
                }
                catch
                {
                    return false;
                }
            }
        }
        public Int32 ReadInt32(string address)
        {
            lock (_ioLock)
            {
                try
                {
                    string response = SendCommandUnlocked($"RD {address}.L");
                    if (Int32.TryParse(response, out Int32 value))
                    {
                        MarkConnected();
                        return value;
                    }
                    if (response == "OK" || response == "00000")
                    {
                        LogProgram.WriteLog($"[PLC] ReadInt32 {address}: phản hồi OK nhưng không có data");
                        return 0;
                    }
                    if (response.Contains("not") || response.Contains("EX"))
                    {
                        LogProgram.WriteLog($"[PLC] ReadInt32 {address} lỗi kết nối: {response}");
                        PropertyChangedEvent($"{Tcpstatus.disconnected}");
                        StartReconnectIfNeeded();
                    }
                    else
                    {
                        MarkConnected();
                    }
                    return 0;
                }
                catch
                {
                    LogProgram.WriteLog($"[PLC] ReadInt32 exception tại {address}");
                    return 0;
                }
            }
        }
        public bool ReadBit(string address)
        {
            lock (_ioLock)
            {
                string response = SendCommandUnlocked($"RD {address}");
                if (response == "1") return true;
                if (response == "0") return false;
                LogProgram.WriteLog($"[PLC] Bit không hợp lệ tại {address}: {response}");
                return false;
            }
        }
        public bool SetBit(string address)
        {
            lock (_ioLock)
            {
                string response = SendCommandUnlocked($"ST {address}");
                return ParseResponse(response) == TcpReceiveCMD.OK;
            }
        }
        public bool ResetBit(string address)
        {
            lock (_ioLock)
            {
                string response = SendCommandUnlocked($"RS {address}");
                return ParseResponse(response) == TcpReceiveCMD.OK;
            }
        }
        public bool ReadBitFromWord(string wordAddress, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex > 15)
            {
                LogProgram.WriteLog($"[PLC] Bit index {bitIndex} không hợp lệ (0-15)");
                return false;
            }
            // FIX: dùng lock(_ioLock) + SendCommandUnlocked để tránh double-lock
            // (ReadUInt16 public cũng lock _ioLock → nếu gọi từ đây sẽ deadlock trên non-reentrant lock)
            lock (_ioLock)
            {
                string response = SendCommandUnlocked($"RD {wordAddress}.U");
                if (!ushort.TryParse(response, out ushort value))
                {
                    LogProgram.WriteLog($"[PLC] ReadBitFromWord: đọc {wordAddress} thất bại: {response}");
                    return false;
                }
                return (value & (1 << bitIndex)) != 0;
            }
        }
        // FIX PLC-1 & PLC-4: toàn bộ RMW (Read-Modify-Write) trong một lock duy nhất
        // Không còn nested lock — SendCommandUnlocked được gọi trực tiếp bên trong _ioLock
        public bool SetBitInWord(string wordAddress, int bitIndex)
        {
            lock (_ioLock)
            {
                // Đọc giá trị hiện tại (unlocked vì đã trong _ioLock)
                string readResp = SendCommandUnlocked($"RD {wordAddress}.U");
                if (!ushort.TryParse(readResp, out ushort original))
                {
                    LogProgram.WriteLog($"[PLC] SetBitInWord: đọc {wordAddress} thất bại: {readResp}");
                    return false;
                }
                ushort updated = (ushort)(original | (1 << bitIndex));
                string writeResp = SendCommandUnlocked($"WR {wordAddress}.U {updated}");
                HandleWriteResponse(writeResp);
                return ParseResponse(writeResp) == TcpReceiveCMD.OK;
            }
        }
        public bool ResetBitInWord(string wordAddress, int bitIndex)
        {
            lock (_ioLock)
            {
                string readResp = SendCommandUnlocked($"RD {wordAddress}.U");
                if (!ushort.TryParse(readResp, out ushort original))
                {
                    LogProgram.WriteLog($"[PLC] ResetBitInWord: đọc {wordAddress} thất bại: {readResp}");
                    return false;
                }
                ushort updated = (ushort)(original & ~(1 << bitIndex));
                string writeResp = SendCommandUnlocked($"WR {wordAddress}.U {updated}");
                HandleWriteResponse(writeResp);
                return ParseResponse(writeResp) == TcpReceiveCMD.OK;
            }
        }
        private void Reconnect(object sender, System.Timers.ElapsedEventArgs e)
        {
            // _ioLock ngăn Reconnect() chạy đồng thời với bất kỳ Read/Write nào từ 4 Form1.
            // Monitor.Enter reentrant nên StartCommunication() → SendCommand() → lock(_ioLock)
            // bên trong vẫn hoạt động đúng (cùng thread).
            lock (_ioLock)
            {
                _reconnectPending = false;
                try
                {
                    Close();
                    Open();
                    StartCommunication();
                    LogProgram.WriteLog($"[HostLinkTCP] Reconnected to {_ip}:{_port}");
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog($"[HostLinkTCP] Reconnection failed: {ex.Message}");
                    PropertyChangedEvent($"{Tcpstatus.disconnected}");
                    _reconnectPending = true;
                    _timer.Interval = 3000;
                    _timer.AutoReset = false;
                    _timer.Elapsed -= Reconnect;
                    _timer.Elapsed += Reconnect;
                    _timer.Start();
                }
            }
        }
    }
    public enum TcpReceiveCMD
    {
        OK,
        E0,
        E1,
        Unknow,
        CheckCodeWrong
    }
    public enum Tcpstatus
    {
        connected,
        disconnected,
    }
}