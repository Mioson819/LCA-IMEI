using Bottom_Sorting.Services.Utilities;
using DocumentFormat.OpenXml.Wordprocessing;
using Guna.UI2.AnimatorNS;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Form;
using LCA_Project.Form.Devices.Controllers;
using LCA_Project.Form.frmAlarm;
using LCA_Project.Form.frmResult;
using LCA_Project.Form.Signal;
using LCA_Project.Form.Teaching;
using LCA_Project.Form.TesterComunication;
using LCA_Project.Utilities;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
namespace LCA_Project
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        // ====== Forms / UI ======
        private frmOldResult _frmOldResult;
        //private Guna2Transition transition;
        //private frmControllersParameter frmControllers;
        private frmControllers frmControllersMain;
        //private frmMultiSignal frmMultiSignal;
        //private FrmSettingController frmSettingController;
        private ucButtonDisplayGrid<DataforUnload> _Unload;
        private ucButtonDisplayGrid<Dataforload> _Load;
        private ucButtonDisplayGrid<DataforloadImei> _Load2;
        private ucButtonDisplayGrid<DataforNG4> _NG4;
        //private CameraAS cam;
        private frmAlarm _frmAlarm;
        private frmControl _frmControl;           // frmControl đang mở (nếu có)
        private KeyenceHostLinkTcpClient plc;
        private CancellationTokenSource _cts;
        private System.Timers.Timer timer;
        private System.Timers.Timer timerUPH;
        private readonly SemaphoreSlim _pollGate = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _pressGate = new SemaphoreSlim(1, 1);
        private Task _batchPollTask;
        private readonly SemaphoreSlim _batchGate = new SemaphoreSlim(1, 1);
        // BUG-3 FIX: giới hạn ReadAlarm chỉ 1 task chạy cùng lúc
        // Không có gate → mỗi 2500ms spawn 1 Task.Run mới bất kể task cũ xong chưa
        // → tích lũy task → threadpool cạn → UI đơ
        private readonly SemaphoreSlim _alarmGate = new SemaphoreSlim(1, 1);
        private readonly List<LabelMonitor> _labelMonitors = new List<LabelMonitor>();
        private readonly List<BitMonitor> _bitMonitors = new List<BitMonitor>();
        private readonly TimeSpan _labelScanPeriod = TimeSpan.FromSeconds(11);
        private readonly TimeSpan _bitScanPeriod = TimeSpan.FromSeconds(5);
        private event EventHandler Nodata;
        private CameraAS _cam;
        private class LabelMonitor
        {
            public System.Windows.Forms.Label Label { get; set; }
            public string Address { get; set; }
            public int LastValue { get; set; } = int.MinValue;
        }
        private class BitMonitor
        {
            public Guna2Button Button { get; set; }
            public string Word { get; set; }
            public int Bit { get; set; }
            public bool State { get; set; } = false;
        }
        public delegate void SendNameStation(string s);
        public event SendNameStation _sendName;
        private string nameStation;
        private string namePort;
        private Type type;
        private string _NameStation;
        private string _ModifyWork;
        private string _ModifyWork2;
        private string _beforStatus = "";
        private string _beforAlarmText = "";
        private ushort _prevAlarmCode = 0;
        private string _ReStartNoData { get; set; }
        private string _Loto { get; set; }
        private bool statusReset = false;
        private bool statusResetNG4 = false, statusResetNG = false;
        private bool statusResult = true;
        private List<Guna2Button> oldResultNG123;
        private List<Guna2Button> oldResultNG4;
        private int _Input = 0, _OK = 0, _NG = 0;
        private int OK = 0, NG = 0;
        private Stopwatch RunTime1, ErrTime1, IdleTime1;
        private string OldRunTime, OldErrTime, OldIdleTime;
        private TimeSpan _OldRunTime, _OldErrTime, _OldIdleTime;
        private readonly object _timeLock = new object();
        private ushort _lastStatusCodeSnapshot = 65535;
        private string ResetALL;
        private string ResetTrayNG4;
        private string ResetTrayNG;
        private string _changeModeTag;       // Tag PLC cho ChangeModeTrayInput (dạng "WORD.BIT")
        private string _tray1InputSignal;   // Tag PLC xác nhận Tray1 đang active (cột Tray1InputSignal trong DB)
        private string _tray2InputSignal;   // Tag PLC xác nhận Tray2 đang active (cột Tray2InputSignal trong DB)
        private int nX;
        private int ny;
        private int nYNG4;
        private int CountDelete = 0;
        private string folder { get; set; }
        public LogFileWatcher logWatcher;
        private readonly Dictionary<string, (string sendDm, string statusTm, string alarmTm, string triggerMrBase)> _addr
            = new Dictionary<string, (string sendDm, string statusTm, string alarmTm, string triggerMrBase)>(StringComparer.OrdinalIgnoreCase)
            {
                { "Station1", ("DM1920", "TM82", "TM74", "MR10100") },
                { "Station2", ("DM1120", "TM80", "TM70", "MR10100") },
                { "Station3", ("DM2320", "TM83", "TM76", "MR10400") },
                { "Station4", ("DM1520", "TM81", "TM72", "MR10400") },
            };
        private readonly Dictionary<ushort, string> _alarm = new Dictionary<ushort, string>
        {
            { 1,  "Lỗi Trục X" },
            { 2,  "Quá Thời Gian Khởi Tạo X" },
            { 3,  "Dừng Khẩn Cấp" },
            { 4,  "Lỗi Driver X" },
            { 5,  "Lỗi Trục Y" },
            { 6,  "Quá Thời Gian Khởi Tạo Y" },
            { 7,  "Lỗi Driver Y" },
            { 8,  "Lỗi Trục Z" },
            { 9,  "Quá Thời Gian Khởi Tạo Z" },
            { 10, "Lỗi Driver Z" },
            { 11, "Lỗi Trục RI" },
            { 12, "Lỗi Driver RI" },
            { 13, "Lỗi Trục R0" },
            { 14, "Lỗi Driver R0" },
            { 15, "Lỗi Trục F" },
            { 16, "Quá Thời Gian Khởi Tạo F" },
            { 17, "Lỗi Driver F" },
            { 18, "Lỗi Cảm Biến Khí Vào" },
            { 19, "Lỗi Cảm Biến Vin Hút Sản Phẩm Vào" },
            { 20, "Lỗi Cảm Biến Viout Hút Sản Phẩm Ra" },
            { 21, "Lỗi Cảm Biến VCa: XyLanh Di Chuyển Camera" },
            { 22, "Lỗi Cảm Biến Xylanh Chart ON" },
            { 23, "Lỗi Cảm Biến Xylanh Chart OFF" },
            { 24, "Lỗi Cảm Biến Cửa" },
            { 25, "Lỗi Cảm Biến JIG/TRAY OK" },
            { 26, "Lỗi Cảm Biến JIG/TRAY NG" },
            { 27, "Lỗi Cảm Biến JIG/TRAY NG4" },
            { 28, "Lỗi MACHINE TEST LCA" },
            { 29, "Lỗi Time Out Test LCA" },
            { 30, "Lỗi Cảm Biến Vsk: Hút Sản Phẩm Tại Socket" },
            { 31, "Complete" },
            { 32, "Full NG1" },
            { 33, "Full NG2" },
            { 34, "Full NG3" },
            { 35, "Full NG4" },
            { 36, "Vaccum Fail" },
            { 40, "No Data. Kiểm tra đường dẫn log hoặc phần mềm Handler/Test của máy Tester." },
            { 41, "Time Out Camera , Hãy Init và chạy lại " },
            { 42, "Timeout không mở được lắp socket, kiểm tra lắp socket hoặc điểm teach F" },
            { 43, "Cửa Đang Mở,Bạn Có Muốn Chạy Tiếp Không" },
            { 44, "Chart đang OFF" },
            { 45, "Thiết Bị Đang Sửa Chữa" },
            { 50, "Axis Alarm: Limit+ X" },
            { 51, "Axis Alarm: Limit- X" },
            { 52, "Axis Alarm: Limit- Y" },
            { 53, "Axis Alarm: Limit- Z" },
            { 54, "Kênh hàng trong Socket" },
            { 55, "PC không kết nối với Handler máy Test, kiểm tra dây kết nối " },
            { 56, "Sensor Cyclinder IN Socket Off" },
            { 57, "Sensor Cyclinder OUT Socket Off" },
            { 58, "Sensor Cyclinder UP Socket Off" },
            { 59, "Sensor Cyclinder DOWN Socket Off" },
            { 60, "Sensor Cyclinder UnClip Socket Off" },
            { 61, "Sensor Cyclinder Clip Socket Off" },
        };
        private readonly Dictionary<ushort, string> _Status = new Dictionary<ushort, string>
        {
            { 0, "Yêu Cầu Về Gốc" },
            { 1, "Đang Về Gốc" },
            { 2, "Lỗi Yêu Cầu Xóa Lỗi" },
            { 3, "Đang Tạm Dừng" },
            { 4, "Sẵn Sàng Chạy Thủ Công" },
            { 5, "Đang Chạy Thủ Công" },
            { 6, "Chưa Đủ Điều Kiện Chạy Tự Động" },
            { 7, "Sẵn Sàng Chạy Tự Động" },
            { 8, "Đang Chạy Tự Động" },
            { 9, "Vô Hiệu Hóa" }
        };
        public string Nametation
        {
            get { return _NameStation; }
            set
            {
                if (value == "Station1") _NameStation = "Port1";
                else if (value == "Station2") _NameStation = "Port2";
                else if (value == "Station3") _NameStation = "Port3";
                else if (value == "Station4") _NameStation = "Port4";
                else _NameStation = value;
            }
        }
        public Form1(KeyenceHostLinkTcpClient plc, string name, int triggerIndex, string NameModel, int nx, int ny, int nYNG4, CameraAS cam)
        {
            InitializeComponent();
            this.nX = nx;
            this.ny = ny;
            this.nYNG4 = nYNG4;
            this.plc = plc;
            this.nameStation = name;
            this.Nametation = name;
            _cts = new CancellationTokenSource();
            string _NameModel = NameModel;
            namePort = name;
            ErrTime1 = new Stopwatch();
            RunTime1 = new Stopwatch();
            IdleTime1 = new Stopwatch();
            string[] value = DatabaseControllers.Instance.GetCurrentValue(this.Nametation);
            if (value != null)
            {
                _Input = int.Parse(value[0]);
                _OK = int.Parse(value[1]);
                _NG = int.Parse(value[2]);
            }
            NG = DatabaseControllers.Instance.SelectDataPortSummaNG(this.Nametation);
            OK = DatabaseControllers.Instance.SelectDataPortSummaOk(this.Nametation);
            OldRunTime = DatabaseControllers.Instance.SelectTimerRunTime(this.Nametation);
            OldErrTime = DatabaseControllers.Instance.SelectTimerErrTime(this.Nametation);
            OldIdleTime = DatabaseControllers.Instance.SelectTimerIdleTime(this.Nametation);
            TimeSpan rt;
            if (!string.IsNullOrWhiteSpace(OldRunTime) && TimeSpan.TryParse(OldRunTime, out rt)) _OldRunTime = rt;
            TimeSpan et;
            if (!string.IsNullOrWhiteSpace(OldErrTime) && TimeSpan.TryParse(OldErrTime, out et)) _OldErrTime = et;
            TimeSpan it;
            if (!string.IsNullOrWhiteSpace(OldIdleTime) && TimeSpan.TryParse(OldIdleTime, out it)) _OldIdleTime = it;
            StartTrigger(triggerIndex);
            this._cam = cam;
            // Đăng ký FormClosing để dừng tất cả background task khi đóng form
            this.FormClosing += Form1_FormClosing;
        }
        // ====== SAFE INVOKE HELPERS ======
        // Dùng thay cho this.Invoke(...) — an toàn khi form đang đóng
        private void SafeInvoke(MethodInvoker action)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try
            {
                this.Invoke(action);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }
        // Dùng thay cho this.BeginInvoke(...) — an toàn khi form đang đóng
        private void SafeBeginInvoke(MethodInvoker action)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try
            {
                this.BeginInvoke(action);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }
        // ====== Helpers ======
        private static bool TryParseTag(string tag, out string word, out int bit)
        {
            word = null; bit = -1;
            if (string.IsNullOrWhiteSpace(tag)) return false;
            string[] parts = tag.Split('.');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[1], out bit)) return false;
            word = parts[0];
            return true;
        }
        private async Task<bool> TrySetBitWithVerify(string word, int bit, int timeoutMs = 3000, int stepMs = 100)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    if (plc.SetBitInWord(word, bit))
                    {
                        if (plc.ReadBitFromWord(word, bit)) return true;
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("SetBit error " + word + "." + bit + ": " + ex, this.Nametation);
                }
                await Task.Delay(stepMs);
            }
            return false;
        }
        private async Task<bool> TryResetBitWithVerify(string word, int bit, int timeoutMs = 3000, int stepMs = 100)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    if (plc.ResetBitInWord(word, bit))
                    {
                        if (!plc.ReadBitFromWord(word, bit)) return true;
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("ResetBit error " + word + "." + bit + ": " + ex, this.Nametation);
                }
                await Task.Delay(stepMs);
            }
            return false;
        }
        public void UpdateModel(Dictionary<string, string> s)
        {
            var value = s.Where(name => name.Key == this.nameStation).FirstOrDefault().Value;
            lblModel.Text = value;
        }
        public void StartRead(Dictionary<string, string> stationToFolder)
        {
            try
            {
                try
                {
                    logWatcher.OnNewLineRead -= HandleLogLine;
                    logWatcher.Dispose();
                }
                catch { }
                string value = stationToFolder.Where(p => p.Key == this.nameStation).FirstOrDefault().Value;
                string serverFolder = value;
                if (string.IsNullOrWhiteSpace(serverFolder)) return;
                string tempPath1;
                switch (this.nameStation)
                {
                    case "Station1": tempPath1 = @"C:\LOGLCA\log1.txt"; break;
                    case "Station2": tempPath1 = @"C:\LOGLCA\log2.txt"; break;
                    case "Station3": tempPath1 = @"C:\LOGLCA\log3.txt"; break;
                    case "Station4": tempPath1 = @"C:\LOGLCA\log4.txt"; break;
                    default: tempPath1 = @"C:\LOGLCA\log.txt"; break;
                }
                this.folder = serverFolder;
                logWatcher = new LogFileWatcher(serverFolder, tempPath1);
                // Đọc PcType từ DB (Nano / Pamtech) thay cho OffMess static
                logWatcher.PcType = DatabaseControllers.Instance.GetPcType(
                    lblModel.Text?.ToString(), this.Nametation);
                logWatcher.OnNewLineRead -= HandleLogLine;
                logWatcher.OnNewLineRead += HandleLogLine;
                logWatcher.Start();
                Nodata -= logWatcher.DeleteFile;
                Nodata += logWatcher.DeleteFile;
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog("StartRead error: " + ex, this.Nametation);
            }
        }
        public void OnMess(object sender, EventArgs e)
        {
            if (logWatcher == null) return;
            // Cập nhật PcType từ DB mỗi khi OnMess được gọi
            // (model hoặc port có thể đã thay đổi)
            logWatcher.PcType = DatabaseControllers.Instance.GetPcType(
                lblModel.Text?.ToString(), this.Nametation);
            if (logWatcher.PcType == "Nano")
            {
                var s = DatabaseControllers.Instance.LoadDataFolder(
                    lblModel.Text.ToString(), this.Nametation);
                if (string.IsNullOrEmpty(s))
                    MessageBox.Show(
                        $"Đường Dẫn ON MESS {this.Nametation} Chưa Có Trong Cơ Sở Dữ Liệu , Yêu Cầu nhập thêm đường dẫn ON MESS ",
                        "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    logWatcher.serverFolder = s;
            }
            else  // Pamtech
            {
                var s = DatabaseControllers.Instance.LoadDataFolderPathOFFMESS(
                    lblModel.Text.ToString(), this.Nametation);
                if (string.IsNullOrEmpty(s))
                    MessageBox.Show(
                        $"Đường Dẫn ON MESS {this.Nametation} Chưa Có Trong Cơ Sở Dữ Liệu , Yêu Cầu nhập thêm đường dẫn OFF MESS ",
                        "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    logWatcher.serverFolder = s;
            }
        }
        private void HandleLogLine(ushort value)
        {
            // Nếu form đã đóng thì bỏ qua, không BeginInvoke nữa
            if (_cts.IsCancellationRequested) return;
            Interlocked.Increment(ref _Input);
            (string sendDm, string statusTm, string alarmTm, string triggerMrBase) map;
            if (!_addr.TryGetValue(nameStation, out map)) return;
            try
            {
                this.plc.WriteUInt16(map.sendDm, value);
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog("PLC write UInt16 " + map.sendDm + " failed: " + ex, this.Nametation);
            }
            // NOTE: KHÔNG gọi _Load2.MarkCurrentAsPass() ở đây.
            // LogWatcher fire trong pnLoad phase (sau test, TRƯỚC khi robot di chuyển sang pnUnload).
            // Lúc này nXULoadNGNow/mYULoadNGNow vẫn chứa tọa độ cycle trước → đổi xanh sai vị trí.
            // Việc đổi xanh _Load2 được xử lý bởi LoopLoad (ucButtonDisplayGrid) khi
            // TakePointNG signal fire — đúng thời điểm robot đặt sản phẩm vào pnlUnload.
            // ← SafeBeginInvoke thay cho this.BeginInvoke
            SafeBeginInvoke((MethodInvoker)(() =>
            {
                lblResultTester.Text = value.ToString();
                if (value == 0)
                {
                    Interlocked.Increment(ref _OK);
                    lblStatus2.Text = "Pass";
                    lblStatus2.ForeColor = System.Drawing.Color.Green;
                    OK++;
                    NberPrdOKs.Text = _OK.ToString();
                    DatabaseControllers.Instance.InsertDataPortSummaOK(this.Nametation, OK);
                    LogProgram.WriteLog(this.nameStation + " PC send Pass : " + value, this.Nametation);
                }
                else
                {
                    lblStatus2.Text = "NG";
                    lblStatus2.ForeColor = System.Drawing.Color.Red;
                    Interlocked.Increment(ref _NG);
                    NG++;
                    NberPrdNGs.Text = _NG.ToString();
                    DatabaseControllers.Instance.InsertDataPortSummaNG(this.Nametation, NG);
                    LogProgram.WriteLog(this.nameStation + " PC send NG : " + value, this.Nametation);
                }
                NberPrdIns.Text = _Input.ToString();
            }));
            CountDelete++;
            // FIX: guard null/_ReStartNoData trước khi Split để tránh NullReferenceException / FormatException
            if (!string.IsNullOrWhiteSpace(this._ReStartNoData) && this._ReStartNoData.Contains('.'))
            {
                var _rsParts = this._ReStartNoData.Split('.');
                if (_rsParts.Length >= 2 && int.TryParse(_rsParts[1], out int _rsBit))
                    this.plc.SetBitInWord(_rsParts[0], _rsBit);
                else
                    LogProgram.WriteLog("HandleLogLine: _ReStartNoData format sai: " + this._ReStartNoData, this.Nametation);
            }
        }
        private void ReadStatus()
        {
            (string sendDm, string statusTm, string alarmTm, string triggerMrBase) map;
            if (!_addr.TryGetValue(nameStation, out map)) return;
            try
            {
                ushort code = plc.ReadUInt16(map.statusTm);
                if (code == _lastStatusCodeSnapshot && _beforStatus != null)
                    return;
                string s;
                if (!_Status.TryGetValue(code, out s) || s == null) return;
                if (s != _beforStatus)
                {
                    _beforStatus = s;
                    // ← SafeBeginInvoke thay cho this.BeginInvoke
                    SafeBeginInvoke((MethodInvoker)(() => { lblStatus.Text = s; }));
                }
                lock (_timeLock)
                {
                    if (code == 5 || code == 8)
                    {
                        if (!RunTime1.IsRunning) RunTime1.Start();
                        if (ErrTime1.IsRunning) ErrTime1.Stop();
                        if (IdleTime1.IsRunning) IdleTime1.Stop();
                    }
                    else if (code == 2)
                    {
                        if (RunTime1.IsRunning) RunTime1.Stop();
                        if (!ErrTime1.IsRunning) ErrTime1.Start();
                        if (IdleTime1.IsRunning) IdleTime1.Stop();
                    }
                    else
                    {
                        if (RunTime1.IsRunning) RunTime1.Stop();
                        if (ErrTime1.IsRunning) ErrTime1.Stop();
                        if (!IdleTime1.IsRunning) IdleTime1.Start();
                    }
                }
                _lastStatusCodeSnapshot = code;
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog("ReadStatus error: " + ex, this.Nametation);
            }
        }
        private void ReadAlarm()
        {
            (string sendDm, string statusTm, string alarmTm, string triggerMrBase) map;
            if (!_addr.TryGetValue(nameStation, out map)) return;
            // BUG-3 FIX: nếu task ReadAlarm trước chưa xong thì bỏ qua tick này.
            // Không có gate cũ → mỗi 2500ms tạo Task.Run mới → tích lũy → threadpool cạn.
            if (!_alarmGate.Wait(0)) return;
            Task.Run(() =>
            {
                try
                {
                    if (_cts.IsCancellationRequested) return;
                    ushort value = plc.ReadUInt16(map.alarmTm);
                    if (value == 0)
                    {
                        try
                        {
                            if (_frmAlarm != null)
                            {
                                SafeBeginInvoke((MethodInvoker)(() =>
                                {
                                    if (_frmAlarm != null && !_frmAlarm.IsDisposed)
                                    {
                                        _frmAlarm.Close();
                                        _frmAlarm.Dispose();
                                        _frmAlarm = null;
                                    }
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            LogProgram.WriteLog("Close frmAlarm error: " + ex, this.Nametation);
                        }
                        _beforAlarmText = "";
                        statusResult = true;
                        _prevAlarmCode = 0;
                        return;
                    }
                    string s;
                    if (!_alarm.TryGetValue(value, out s)) return;
                    if (value == 31 || value == 32 || value == 33 || value == 34 || value == 35 && statusResult)
                    {
                        oldResultNG123 = _Unload != null ? _Unload.OldResult() : null;
                        oldResultNG4 = _NG4 != null ? _NG4.OldResult() : null;
                        statusResult = false;
                    }
                    else if (value != 31 || value != 32 || value != 33 || value != 34 || value != 35)
                    {
                        statusResult = true;
                    }
                    _prevAlarmCode = value;
                    if (s != _beforAlarmText)
                    {
                        _beforAlarmText = s;
                        // BUG-8 FIX: dispose instance cũ trước khi tạo mới,
                        // tránh tích lũy GDI handle sau nhiều giờ chạy máy.
                        SafeBeginInvoke((MethodInvoker)(() =>
                        {
                            if (_frmAlarm != null && !_frmAlarm.IsDisposed)
                            {
                                _frmAlarm.Close();
                                _frmAlarm.Dispose();
                            }
                            _frmAlarm = new frmAlarm(s, this.plc, this.nameStation);
                            _frmAlarm.Location = new Point(this.Location.X, this.Location.Y);
                            _frmAlarm.Model = lblModel.Text.ToString();
                            _frmAlarm.PcType = logWatcher?.PcType ?? "Nano";
                            _frmAlarm.Show();
                        }));
                        LogProgram.MesWriteLog("Alarm: " + s, this.Nametation);
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("ReadAlarm error: " + ex, this.Nametation);
                }
                finally
                {
                    try { _alarmGate.Release(); } catch (ObjectDisposedException) { }
                }
            });
        }
        // ====== Timers ======
        private void TimerUPH_Elapsed(object sender, ElapsedEventArgs e)
        {
        }
        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Nếu form đã cancel (đang đóng) thì bỏ qua tick này
            if (_cts.IsCancellationRequested) return;
            // Bọc WaitAsync trong try-catch vì semaphore có thể đã bị Dispose
            // do race condition giữa Timer callback và FormClosing
            bool acquired;
            try
            {
                acquired = await _pollGate.WaitAsync(0);
            }
            catch (ObjectDisposedException) { return; }
            if (!acquired) return;
            try
            {
                ReadAlarm();
                ReadStatus();
                Rset(ResetALL);
                RsetNG(ResetTrayNG);
                RsetNG4(ResetTrayNG4);
                TimeSpan run, err, idle;
                lock (_timeLock)
                {
                    run = (!string.IsNullOrEmpty(OldRunTime)) ? (_OldRunTime + RunTime1.Elapsed) : RunTime1.Elapsed;
                    err = (!string.IsNullOrEmpty(OldErrTime)) ? (_OldErrTime + ErrTime1.Elapsed) : ErrTime1.Elapsed;
                    idle = (!string.IsNullOrEmpty(OldIdleTime)) ? (_OldIdleTime + IdleTime1.Elapsed) : IdleTime1.Elapsed;
                }
                // SafeBeginInvoke (async) thay vì SafeInvoke (sync/blocking).
                // Timer_Elapsed chạy trên threadpool và giữ _pollGate.
                // Nếu dùng SafeInvoke (this.Invoke) mà UI thread đang bận → timer thread
                // chờ vô thời hạn, _pollGate không được release → mọi tick tiếp theo bị bỏ qua.
                var runCopy = run; var errCopy = err; var idleCopy = idle;
                var input = _Input; var ok = _OK; var ng = _NG;
                SafeBeginInvoke((MethodInvoker)(() =>
                {
                    NberPrdNGs.Text = ng.ToString();
                    NberPrdOKs.Text = ok.ToString();
                    NberPrdIns.Text = input.ToString();
                    if (input > 0 && ok >= 0)
                    {
                        float value = ((float)ok / (float)input) * 100;
                        percentOKs.Text = value.ToString("0.00");
                    }
                    RunTime.Text = runCopy.ToString(@"hh\:mm\:ss");
                    ErrTime.Text = errCopy.ToString(@"hh\:mm\:ss");
                    IdleTime.Text = idleCopy.ToString(@"hh\:mm\:ss");
                    var h = (float)runCopy.TotalHours;
                    if (h > 0)
                    {
                        INUPHs.Text = (input / h).ToString("0.00");
                        PASSUPHs.Text = (ok / h).ToString("0.00");
                    }
                }));
            }
            finally
            {
                try
                {
                    DatabaseControllers.Instance.UpdateCurrentValue(this.Nametation, _Input, _OK, _NG);
                    DatabaseControllers.Instance.InsertDataPortSummaNG(this.Nametation, NG);
                    DatabaseControllers.Instance.InsertDataPortSummaOK(this.Nametation, OK);
                    DatabaseControllers.Instance.InsertDataPortSummaInput(this.Nametation,
                        DatabaseControllers.Instance.SelectDataPortSummaNG(this.Nametation) +
                        DatabaseControllers.Instance.SelectDataPortSummaOk(this.Nametation));
                }
                catch { }
                try { _pollGate.Release(); } catch (ObjectDisposedException) { }
            }
        }
        // ====== Load ======
        /// <summary>
        /// Gọi từ frmMaincs sau khi xác thực mật khẩu thành công để mở/khóa các nút admin.
        /// </summary>
        public void SetAdminButtons(bool enabled)
        {
            btnTeaching.Enabled = enabled;
            btnSetting.Enabled = enabled;
            // Lan truyền trạng thái xuống frmControl đang mở (nếu có)
            if (_frmControl != null && !_frmControl.IsDisposed)
                _frmControl.SetSensorDoor(enabled);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Khóa các nút admin cho đến khi user xác thực thành công qua frmLotoImei
            btnTeaching.Enabled = false;
            btnSetting.Enabled = false;
            AutoRead();
            StartBatchPoller();
            ReadDataforLoad();
            ReadDataForNG4();
            // Đọc bit ChangeModeTrayInput từ PLC để quyết định panel Unload khi form load
            // Tag hỗ trợ 2 dạng: "DM2512" (word, dùng bit 0) hoặc "MR2512.1" (word.bit)
            {
                bool isImeiMode = false;
                if (!string.IsNullOrWhiteSpace(_changeModeTag))
                {
                    try
                    {
                        string word; int bit;
                        if (TryParseTag(_changeModeTag, out word, out bit))
                            isImeiMode = plc.ReadBitFromWord(word, bit);   // dạng MR2512.1
                        else
                            isImeiMode = plc.ReadBitFromWord(_changeModeTag, 0); // dạng DM2512
                    }
                    catch (Exception ex)
                    {
                        LogProgram.WriteLog("Form1_Load – Read ChangeModeTrayInput error: " + ex, this.Nametation);
                    }
                }
                if (isImeiMode)
                {
                    ReadDataforUnloadImei();   // bit = 1 → Mode IMEI
                    ChangeModeTrayInput.BackColor = System.Drawing.Color.LimeGreen;
                    ChangeModeTrayInput.Text = "Mode 2 Tray Input";
                }
                else
                {
                    ReadDataforUnload();        // bit = 0 → Mode LCA
                    ModifyWork2.Visible = false;
                    ChangeModeTrayInput.BackColor = System.Drawing.Color.Gray;
                    ChangeModeTrayInput.Text = "Mode 1 Tray Input";
                }
            }
            timer = new System.Timers.Timer(2500);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
            lblStation.Text = this.Nametation;
            if (!frmMaincs.modeimei)
            {
                ModifyWork2.Visible = false;
                ChangeModeTrayInput.Visible = false;
            }
        }
        private void AutoRead()
        {
            type = typeof(DataforInputResults);
            // ResetALL
            PropertyInfo pReset = type.GetProperty("ResetData", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object data = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            ResetALL = pReset != null ? (pReset.GetValue(data) + "") : null;
            // ResetNG4
            PropertyInfo rs4 = type.GetProperty("ResetNG4", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object data2 = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            ResetTrayNG4 = rs4 != null ? (rs4.GetValue(data2) + "").Trim() : null;
            // ModifyWork
            PropertyInfo pModify = type.GetProperty("ModifyWork", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object data3 = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            _ModifyWork = pModify != null ? (pModify.GetValue(data3) + "").Trim() : null;
            // ModifyWork 2
            PropertyInfo pModify2 = type.GetProperty("ModifyWork2", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object data33 = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            _ModifyWork2 = pModify2 != null ? (pModify2.GetValue(data33) + "").Trim() : null;
            // ResetTrayNG
            PropertyInfo pResetTrayNG = type.GetProperty("ResetTrayNG", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object data4 = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            this.ResetTrayNG = pResetTrayNG != null ? (pResetTrayNG.GetValue(data4) + "").Trim() : null;
            // ReStartNoData
            PropertyInfo ReStartNoData = type.GetProperty("ReStartNoData", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object data5 = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            this._ReStartNoData = ReStartNoData != null ? (ReStartNoData.GetValue(data5) + "").Trim() : null;
            PropertyInfo Loto = type.GetProperty("Loto", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object data6 = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            this._Loto = Loto != null ? (Loto.GetValue(data6) + "").Trim() : null;
            // ChangeModeTrayInput — lưu địa chỉ tag (WORD.BIT) để Form1_Load đọc bit quyết định chế độ Unload
            PropertyInfo pChangeModeInput = type.GetProperty("ChangeModeTrayInput", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object dataChangeModeInput = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            _changeModeTag = pChangeModeInput != null ? (pChangeModeInput.GetValue(dataChangeModeInput) + "").Trim() : null;
            // Tray1InputSignal — bit PLC xác nhận Tray1 đang active (dùng để routing _NG4._takPointLoad)
            PropertyInfo pTray1Signal = type.GetProperty("Tray1InputSignal", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object dataTray1Signal = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            _tray1InputSignal = pTray1Signal != null ? (pTray1Signal.GetValue(dataTray1Signal) + "").Trim() : null;
            // Tray2InputSignal — bit PLC xác nhận Tray2 đang active
            PropertyInfo pTray2Signal = type.GetProperty("Tray2InputSignal", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            object dataTray2Signal = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
            _tray2InputSignal = pTray2Signal != null ? (pTray2Signal.GetValue(dataTray2Signal) + "").Trim() : null;
            _labelMonitors.Clear();
            _bitMonitors.Clear();
            foreach (System.Windows.Forms.Control control in GetAllControls(this))
            {
                if (control is System.Windows.Forms.Label)
                {
                    System.Windows.Forms.Label label = (System.Windows.Forms.Label)control;
                    PropertyInfo prop = type.GetProperty(label.Name.Trim(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null) continue;
                    object dataObj = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
                    if (dataObj == null) continue;
                    string addr = (prop.GetValue(dataObj) + "").Trim();
                    if (string.IsNullOrEmpty(addr)) continue;
                    _labelMonitors.Add(new LabelMonitor
                    {
                        Label = label,
                        Address = addr
                    });
                }
                else if (control is Guna2Button)
                {
                    Guna2Button button = (Guna2Button)control;
                    PropertyInfo prop = type.GetProperty(button.Name.Trim(),
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null) continue;
                    object dataObj = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
                    if (dataObj == null) continue;
                    string tag = (prop.GetValue(dataObj) + "").Trim();
                    string word; int bit;
                    if (string.IsNullOrEmpty(tag) || !TryParseTag(tag, out word, out bit)) continue;
                    _bitMonitors.Add(new BitMonitor
                    {
                        Button = button,
                        Word = word,
                        Bit = bit
                    });
                }
                else if (control is Guna2GradientButton)
                {
                    Guna2GradientButton buttonGradion = (Guna2GradientButton)control;
                    string nameUpper = buttonGradion.Name.ToUpperInvariant();
                    PropertyInfo prop = type.GetProperty(buttonGradion.Name.Trim(),
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null) continue;
                    object dataObj = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
                    if (dataObj == null) continue;
                    string tag = (prop.GetValue(dataObj) + "").Trim();
                    if (string.IsNullOrEmpty(tag)) continue;
                    // ← SafeBeginInvoke thay cho this.BeginInvoke
                    SafeBeginInvoke((MethodInvoker)(() => { buttonGradion.Tag = tag; }));
                    if (nameUpper.Contains("RESETDATA") || nameUpper.Contains("SKIP") || nameUpper.Contains("RETRI")
                        || nameUpper.IndexOf("Reset", StringComparison.OrdinalIgnoreCase) >= 0
                        || nameUpper.IndexOf("Set", StringComparison.OrdinalIgnoreCase) >= 0
                        || nameUpper.IndexOf("EndAuto", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        buttonGradion.MouseDown += MouseDowns;
                        buttonGradion.MouseUp += MouseUps;
                    }
                    else if (nameUpper.Contains("MODI"))
                    {
                        continue;
                    }
                    else if (nameUpper == "CHANGEMODETRAYINPUT")
                    {
                        // Handler riêng: toggle bit rồi đóng form
                        buttonGradion.Click -= ChangeModeTrayInput_Click;
                        buttonGradion.Click += ChangeModeTrayInput_Click;
                    }
                    else
                    {
                        buttonGradion.Click += Clicks;
                    }
                }
            }
        }
        private void StartBatchPoller()
        {
            if (_batchPollTask != null && !_batchPollTask.IsCompleted) return;
            _batchPollTask = Task.Run(async () =>
            {
                Stopwatch swLabel = Stopwatch.StartNew();
                Stopwatch swBit = Stopwatch.StartNew();
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        if (swLabel.Elapsed >= _labelScanPeriod && _labelMonitors.Count > 0)
                        {
                            swLabel.Restart();
                            foreach (LabelMonitor m in _labelMonitors)
                            {
                                if (_cts.IsCancellationRequested) break;
                                try
                                {
                                    int valueNow = plc.ReadInt32(m.Address);
                                    if (valueNow != m.LastValue)
                                    {
                                        m.LastValue = valueNow;
                                        string text = ((double)valueNow).ToString();
                                        // ← SafeBeginInvoke thay cho this.BeginInvoke
                                        SafeBeginInvoke((MethodInvoker)(() => m.Label.Text = text));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogProgram.WriteLog("Batch ReadInt32 " + m.Address + " error: " + ex, this.Nametation);
                                }
                            }
                        }
                        if (swBit.Elapsed >= _bitScanPeriod && _bitMonitors.Count > 0)
                        {
                            swBit.Restart();
                            foreach (BitMonitor b in _bitMonitors)
                            {
                                if (_cts.IsCancellationRequested) break;
                                try
                                {
                                    bool now = plc.ReadBitFromWord(b.Word, b.Bit);
                                    if (now != b.State)
                                    {
                                        b.State = now;
                                        // ← SafeBeginInvoke thay cho this.BeginInvoke
                                        SafeBeginInvoke((MethodInvoker)(() =>
                                        {
                                            b.Button.FillColor = now
                                                ? System.Drawing.Color.FromArgb(192, 255, 192)
                                                : System.Drawing.Color.FromArgb(255, 128, 128);
                                        }));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogProgram.WriteLog("Batch ReadBit " + b.Word + "." + b.Bit + " error: " + ex, this.Nametation);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogProgram.WriteLog("BatchPoller loop error: " + ex, this.Nametation);
                    }
                    await Task.Delay(300);
                }
            }, _cts.Token);
        }
        // ====== Control trees ======
        public IEnumerable<System.Windows.Forms.Control> GetAllControls(System.Windows.Forms.Control parent)
        {
            foreach (System.Windows.Forms.Control ctrl in parent.Controls)
            {
                yield return ctrl;
                foreach (System.Windows.Forms.Control child in GetAllControls(ctrl))
                    yield return child;
            }
        }
        private void NG999(Object sender, EventArgs e)
        {
            lblResultTester.Text = "999";
            Interlocked.Increment(ref _Input);
            Interlocked.Increment(ref _NG);
        }
        // ====== Data Grids ======
        private void ReadDataforUnload()
        {
            _Unload = new ucButtonDisplayGrid<DataforUnload>(this.nX, this.ny, this.plc,
                DatabaseControllers.Instance.GetDataForUnload(this.nameStation), this.nameStation, "9000", "UnLoad");
            pnlUnload.Controls.Clear();
            pnlUnload.Controls.Add(_Unload);
            _Unload.NG999 -= NG999;
            _Unload.NG999 += NG999;
            _Unload._takPointLoad -= _Load.UpdateLabel;
            _Unload._takPointLoad += _Load.UpdateLabel;
            _Unload.CheckDelete = () => { DeleteFile(); };
        }
        private void ReadDataforUnloadImei()
        {
            _Load2 = new ucButtonDisplayGrid<DataforloadImei>(this.nX, this.ny, this.plc,
                DatabaseControllers.Instance.GetDataloadImei(this.nameStation), this.nameStation, _ModifyWork2, "Load");
            pnlUnload.Controls.Clear();
            pnlUnload.Controls.Add(_Load2);
            if (_NG4 != null)
            {
                // _tray1InputSignal đọc từ DB cột Tray1InputSignal trong AutoRead()
                // _tray2InputSignal đọc từ DB cột Tray2InputSignal trong AutoRead()
                // Cả 2 đã sẵn có trước khi hàm này được gọi (AutoRead chạy trước Form1_Load)
                string tray1Tag = _tray1InputSignal;
                // Xóa toàn bộ subscription cũ trên _NG4._takPointLoad
                _NG4._takPointLoad -= _Load.UpdateLabel;
                _NG4._takPointLoad -= _Load2.UpdateLabel;
                // Routing lambda: đọc Tray1InputSignal bit lúc NG4 fire
                _NG4._takPointLoad += (x, y, classify, outVal) =>
                {
                    bool tray1Active = false;
                    try
                    {
                        string word; int bit;
                        if (!string.IsNullOrWhiteSpace(tray1Tag)
                            && TryParseTag(tray1Tag, out word, out bit))
                        {
                            tray1Active = plc.ReadBitFromWord(word, bit);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogProgram.WriteLog("ReadDataforUnloadImei routing lambda error: " + ex.Message, this.Nametation);
                    }
                    if (tray1Active)
                        _Load.UpdateLabel(x, y, classify, outVal);   // Tray1 đang active
                    else
                        _Load2.UpdateLabel(x, y, classify, outVal);  // Tray2 đang active
                };
            }
            _Load2.CheckDelete = () => { DeleteFile(); };
        }
        private void ReadDataforLoad()
        {
            _Load = new ucButtonDisplayGrid<Dataforload>(this.nX, this.ny, this.plc,
                DatabaseControllers.Instance.GetDataload(this.nameStation), this.nameStation, _ModifyWork, "Load");
            pnlLoad.Controls.Clear();
            pnlLoad.Controls.Add(_Load);
            _Load.CheckDelete = () =>
            {
                DeleteFile();
            };
        }
        private void ReadDataForNG4()
        {
            _NG4 = new ucButtonDisplayGrid<DataforNG4>(2, nYNG4, this.plc,
                DatabaseControllers.Instance.GetDataNG4(this.nameStation), this.nameStation, "9000", "UnLoad");
            pnlNG4.Controls.Clear();
            pnlNG4.Controls.Add(_NG4);
            _NG4.NG999 -= NG999;
            _NG4.NG999 += NG999;
            _NG4.CheckDelete -= DeleteFile;
            _NG4.CheckDelete += DeleteFile;
            _NG4._takPointLoad -= _Load.UpdateLabel;
            _NG4._takPointLoad += _Load.UpdateLabel;
        }
        // ====== Form Closing — dừng tất cả background task trước khi form bị hủy ======
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 1. Dừng timer TRƯỚC TIÊN và hủy đăng ký event
                //    để không có tick mới nào được phát sinh
                if (timer != null)
                {
                    timer.Stop();
                    timer.Elapsed -= Timer_Elapsed;
                }
                if (timerUPH != null) { timerUPH.Stop(); }
                // 2. Cancel token để tất cả Task/loop thoát
                //    Guard: chỉ Cancel nếu chưa bị dispose hoặc đã cancel rồi
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    try { _cts.Cancel(); } catch (ObjectDisposedException) { }
                }
                // 3. Dừng và lưu thời gian
                lock (_timeLock)
                {
                    if (RunTime1.IsRunning) RunTime1.Stop();
                    if (ErrTime1.IsRunning) ErrTime1.Stop();
                    if (IdleTime1.IsRunning) IdleTime1.Stop();
                }
                DatabaseControllers.Instance.InsertTimerRunTime(this.Nametation, (_OldRunTime + RunTime1.Elapsed).ToString(@"hh\:mm\:ss"));
                DatabaseControllers.Instance.InsertTimerErrTime(this.Nametation, (_OldErrTime + ErrTime1.Elapsed).ToString(@"hh\:mm\:ss"));
                DatabaseControllers.Instance.InsertTimerIdleTime(this.Nametation, (_OldIdleTime + IdleTime1.Elapsed).ToString(@"hh\:mm\:ss"));
                DatabaseControllers.Instance.UpdateCurrentValue(this.Nametation, _Input, _OK, _NG);
                DatabaseControllers.Instance.InsertDataPortSummaNG(this.Nametation, NG);
                DatabaseControllers.Instance.InsertDataPortSummaOK(this.Nametation, OK);
                DatabaseControllers.Instance.InsertDataPortSummaInput(this.Nametation,
                DatabaseControllers.Instance.SelectDataPortSummaNG(this.Nametation) +
                DatabaseControllers.Instance.SelectDataPortSummaOk(this.Nametation));
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog("FormClosing save times error: " + ex, this.Nametation);
            }
            finally
            {
                // 4. Giải phóng tài nguyên — bọc từng cái riêng để cái này lỗi không ảnh hưởng cái kia
                try { timer?.Dispose(); } catch { }
                try { timerUPH?.Dispose(); } catch { }
                try { logWatcher?.Dispose(); } catch { }
                try { _pollGate.Dispose(); } catch { }
                try { _pressGate.Dispose(); } catch { }
                try { _batchGate.Dispose(); } catch { }
                try { _alarmGate.Dispose(); } catch { }
                try { _cts.Dispose(); } catch { }
            }
        }
        public void StopRead(object sender, EventArgs e)
        {
            try
            {
                if (logWatcher != null) logWatcher.Dispose();
                lblModel.Text = "Model : Null";
            }
            catch { }
        }
        // ====== Buttons / Events ======
        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            frmControllersMain = new frmControllers();
            frmControllersMain.Show();
        }
        private void guna2GradientButton14_Click(object sender, EventArgs e)
        {
            frmTeaching2 frmTeaching2 = new frmTeaching2(this.nameStation, this.plc);
            frmTeaching2.Show();
        }
        private void ResetWork_Click(object sender, EventArgs e)
        {
            if (_Load != null) _Load.ResetLoad();
        }
        private async void MouseUps(object sender, MouseEventArgs e)
        {
            Guna2GradientButton btn = sender as Guna2GradientButton;
            string word; int bit;
            if (!TryParseTag(btn != null ? btn.Tag as string : null, out word, out bit)) return;
            await TryResetBitWithVerify(word, bit);
            btn.FillColor = System.Drawing.Color.Gray;
        }
        private async void MouseDowns(object sender, MouseEventArgs e)
        {
            Guna2GradientButton btn = sender as Guna2GradientButton;
            string word; int bit;
            if (!TryParseTag(btn != null ? btn.Tag as string : null, out word, out bit)) return;
            await TrySetBitWithVerify(word, bit);
            btn.FillColor = System.Drawing.Color.Green;
        }
        private void SetCompWork_Click(object sender, EventArgs e)
        {
            if (_Load != null) _Load.SetComp();
        }
        private void SetEmptyWork_Click(object sender, EventArgs e)
        {
            if (_Load != null) _Load.SetEmptyLoad();
        }
        private void ResetCompelete_Click(object sender, EventArgs e)
        {
            if (_Unload != null) _Unload.ResetUnLoad();
            if (_Load2 != null) _Load2.ResetLoad();    // IMEI mode 2 tray: reset pnlUnload về vàng
        }
        private void SetCompCompelete_Click(object sender, EventArgs e)
        {
            if (_Unload != null) _Unload.SetComp();
        }
        private void SetEmptyCompelete_Click(object sender, EventArgs e)
        {
            if (_Unload != null) _Unload.SetEmptyUnLoad();
            if (_Load2 != null) _Load2.SetEmptyLoad();  // IMEI mode 2 tray: pnlUnload về vàng
        }
        private void ModifyWork_Click(object sender, EventArgs e)
        {
            if (_Load != null)
            {
                _Load.Modify(_ModifyWork);
                if (_Load.isSend == true)
                {
                    SetCompWork.Enabled = false;
                    ResetWork.Enabled = false;
                    SetEmptyWork.Enabled = false;
                }
                else
                {
                    SetCompWork.Enabled = true;
                    ResetWork.Enabled = true;
                    SetEmptyWork.Enabled = true;
                }
            }
        }
        private void ModifyWork2_Click(object sender, EventArgs e)
        {
            if (_Load2 != null)
            {
                _Load2.Modify(_ModifyWork2);
                if (_Load2.isSend == true)
                {
                    SetEmptyCompelete.Enabled = false;
                    ResetCompelete.Enabled = false;
                    SetCompCompelete.Enabled = false;
                }
                else
                {
                    SetEmptyCompelete.Enabled = true;
                    ResetCompelete.Enabled = true;
                    SetCompCompelete.Enabled = true;
                }
            }
        }
        // ====== ChangeModeTrayInput: toggle bit PLC rồi đóng form ======
        private async void ChangeModeTrayInput_Click(object sender, EventArgs e)
        {
            Guna2GradientButton btn = sender as Guna2GradientButton;
            string tag = btn?.Tag as string;
            if (string.IsNullOrWhiteSpace(tag)) return;
            // Hỗ trợ 2 dạng tag: "DM2512" (word, bit 0) hoặc "MR2512.1" (word.bit)
            string word; int bit;
            if (!TryParseTag(tag, out word, out bit))
            {
                word = tag;   // dạng DM2512 → dùng bit 0
                bit = 0;
            }
            // Đọc trạng thái hiện tại rồi toggle
            bool current = false;
            try { current = plc.ReadBitFromWord(word, bit); }
            catch (Exception ex)
            {
                LogProgram.WriteLog("ChangeModeTrayInput read bit error: " + ex, this.Nametation);
            }
            try
            {
                if (current)
                    await TryResetBitWithVerify(word, bit);   // đang ON → tắt
                else
                    await TrySetBitWithVerify(word, bit);     // đang OFF → bật
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog("ChangeModeTrayInput toggle error: " + ex, this.Nametation);
            }
            // Đóng form sau khi toggle
            SafeBeginInvoke((MethodInvoker)(() =>
            {
                try { this.Close(); } catch { }
            }));
        }
        private async void Clicks(object sender, EventArgs e)
        {
            Guna2GradientButton btn = sender as Guna2GradientButton;
            if (btn == null) return;
            string tag = btn.Tag as string;
            string word; int bit;
            if (!TryParseTag(tag, out word, out bit)) return;
            bool turningOn = btn.FillColor != System.Drawing.Color.Green;
            if (turningOn)
            {
                if (await TrySetBitWithVerify(word, bit))
                    // ← SafeBeginInvoke thay cho this.BeginInvoke
                    SafeBeginInvoke((MethodInvoker)(() => btn.FillColor = System.Drawing.Color.Green));
            }
            else
            {
                if (await TryResetBitWithVerify(word, bit))
                    // ← SafeBeginInvoke thay cho this.BeginInvoke
                    SafeBeginInvoke((MethodInvoker)(() => btn.FillColor = System.Drawing.Color.Gray));
            }
        }
        private void guna2GradientButton9_Click(object sender, EventArgs e)
        {
            DatabaseControllers.Instance.DeleteCurrentValue(this.Nametation);
            Interlocked.Exchange(ref _Input, 0);
            Interlocked.Exchange(ref _OK, 0);
            Interlocked.Exchange(ref _NG, 0);
            INUPHs.Text = string.Empty;
            PASSUPHs.Text = string.Empty;
            NberPrdIns.Text = "0";
            NberPrdOKs.Text = "0";
            NberPrdNGs.Text = "0";
            _OldRunTime = TimeSpan.Parse("00:00:00");
            _OldErrTime = TimeSpan.Parse("00:00:00");
            _OldIdleTime = TimeSpan.Parse("00:00:00");
            bool runTime = RunTime1.IsRunning, errTime = ErrTime1.IsRunning, idleIime = IdleTime1.IsRunning;
            RunTime1.Reset();
            ErrTime1.Reset();
            IdleTime1.Reset();
            if (runTime) RunTime1.Start();
            if (errTime) ErrTime1.Start();
            if (idleIime) IdleTime1.Start();
            string[] value = DatabaseControllers.Instance.GetCurrentValue(this.Nametation);
            if (value != null)
            {
                _Input = int.Parse(value[0]);
                _OK = int.Parse(value[1]);
                _NG = int.Parse(value[2]);
            }
        }
        public void Retrig()
        {
            if (_sendName != null) _sendName.Invoke(nameStation);
        }
        private void btnLoto_Click(object sender, EventArgs e)
        {
            frmLoto _Loto = new frmLoto(this.plc, this._Loto);
            _Loto.Location = new Point(this.Location.X, this.Location.Y);
            _Loto.Show();
            _Loto.BringToFront();
        }
        private void tableLayoutPanel24_Paint(object sender, PaintEventArgs e)
        {
        }
        private void Retrig1_Click(object sender, EventArgs e)
        {
            if (_sendName != null) _sendName.Invoke(nameStation);
        }
        public void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            this.Close(); // Close() tự trigger FormClosing → Dispose() đúng thứ tự
        }
        private void btnOldResult_Click(object sender, EventArgs e)
        {
            _frmOldResult = new frmOldResult(this.nX, this.ny, this.nYNG4, this.nameStation);
            if (oldResultNG123 != null && oldResultNG4 != null)
            {
                _frmOldResult.OldResultNG123 = oldResultNG123;
                _frmOldResult.OldResultNG4 = oldResultNG4;
                _frmOldResult.Location = new Point(this.Location.X, this.Location.Y);
                _frmOldResult.Show();
            }
        }
        // ====== Reset theo bit ======
        public void Rset(string s) // Reset Tray Load
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            Task.Run(() =>
            {
                try
                {
                    string word; int bit;
                    if (!TryParseTag(s, out word, out bit)) return;
                    bool b = plc.ReadBitFromWord(word, bit);
                    if (b == false && statusReset == false)
                    {
                        if (_Load != null) _Load.RsetLoad();
                        // Mode 2 tray IMEI: reset _Load2 (pnlUnload) về vàng cùng lúc với _Load
                        if (_Load2 != null) _Load2.RsetLoad();
                        statusReset = true;
                        LogProgram.WriteLog("Reset Tray Load :" + this.Nametation, this.Nametation);
                    }
                    else if (b == true && statusReset == true)
                    {
                        statusReset = false;
                        if (_Load != null) _Load.FullTray();
                        // Mode 2 tray IMEI: FullTray cho _Load2 khi tray mới được đặt vào
                        if (_Load2 != null) _Load2.FullTray();
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("Rset error: " + ex, this.Nametation);
                }
            });
        }
        public void RsetNG4(string s) // Reset Tray NG4
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            Task.Run(() =>
            {
                try
                {
                    string word; int bit;
                    if (!TryParseTag(s, out word, out bit)) return;
                    bool b = plc.ReadBitFromWord(word, bit);
                    if (b == false && statusResetNG4 == false)
                    {
                        statusResetNG4 = true;
                    }
                    else if (b == true && statusResetNG4 == true)
                    {
                        if (_NG4 != null) _NG4.Rset();
                        LogProgram.WriteLog("Reset Tray NG4 :" + this.Nametation, this.Nametation);
                        statusResetNG4 = false;
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("RsetNG4 error: " + ex, this.Nametation);
                }
            });
        }
        public void RsetNG(string s) // Reset Tray NG (Unload)
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            Task.Run(() =>
            {
                try
                {
                    string word; int bit;
                    if (!TryParseTag(s, out word, out bit)) return;
                    bool b = plc.ReadBitFromWord(word, bit);
                    if (b == false && statusResetNG == false)
                    {
                        statusResetNG = true;
                    }
                    else if (b == true && statusResetNG == true)
                    {
                        if (_Unload != null) _Unload.Rset();
                        LogProgram.WriteLog("Reset Tray NG :" + this.Nametation, this.Nametation);
                        statusResetNG = false;
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("RsetNG error: " + ex, this.Nametation);
                }
            });
        }
        // ====== Trigger ======
        private void StartTrigger(int s)
        {
            (string sendDm, string statusTm, string alarmTm, string triggerMrBase) map;
            if (!_addr.TryGetValue(nameStation, out map)) return;
            string mrBase = map.triggerMrBase;
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        if (this.plc.ReadBitFromWord(mrBase, s) == true)
                        {
                            LogProgram.WriteLog(nameStation + " Trigger " + s, this.Nametation);
                            if (_sendName != null) _sendName.Invoke(nameStation);
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogProgram.WriteLog("StartTrigger read error: " + ex, this.Nametation);
                    }
                    await Task.Delay(400);
                }
            }, _cts.Token);
        }
        public void Reset(object sender, EventArgs e)
        {
            timer.Stop();
            DatabaseControllers.Instance.DeleteNG(this.Nametation);
            NG = DatabaseControllers.Instance.SelectDataPortSummaNG(this.Nametation);
            OK = DatabaseControllers.Instance.SelectDataPortSummaOk(this.Nametation);
            timer.Start();
        }
        public void DeleteFile()
        {
            if (CountDelete >= 3)
            {
                try
                {
                    CountDelete = 0;
                    Nodata?.Invoke(this, new EventArgs());
                    switch (this.nameStation)
                    {
                        case "Station1": File.WriteAllText(@"C:\LOGLCA\log1.txt", ""); break;
                        case "Station2": File.WriteAllText(@"C:\LOGLCA\log2.txt", ""); break;
                        case "Station3": File.WriteAllText(@"C:\LOGLCA\log3.txt", ""); break;
                        case "Station4": File.WriteAllText(@"C:\LOGLCA\log4.txt", ""); break;
                    }
                }
                catch (IOException) { }
                catch (Exception) { }
            }
        }
        private void btnSetting_Click(object sender, EventArgs e)
        {
            frmSetting _frmSetting = new frmSetting(this.nameStation, this.plc);
            _frmSetting.Show();
        }
        private void btnControl_Click(object sender, EventArgs e)
        {
            _frmControl = new frmControl(this.nameStation, this.plc);
            // Xoá reference khi frmControl đóng để tránh memory leak
            _frmControl.FormClosed += (s, args) => _frmControl = null;
            _frmControl.Show();
        }
        public void ResetChangeModeTag()
        {
            if (string.IsNullOrWhiteSpace(_changeModeTag)) return;
            string word; int bit;
            if (!TryParseTag(_changeModeTag, out word, out bit))
            {
                word = _changeModeTag;  // dạng DM2512 → bit 0
                bit = 0;
            }
            plc.ResetBitInWord(word, bit);  // set = 0
        }
    }
}