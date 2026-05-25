using Bottom_Sorting.Services.Utilities;
using DocumentFormat.OpenXml.Wordprocessing;
using Guna.UI2.AnimatorNS;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Form;
using LCA_Project.Form.Devices.Cameras;
using LCA_Project.Form.Devices.Controllers;
using LCA_Project.Form.frmAlarm;
using LCA_Project.Form.frmResult;
using LCA_Project.Form.Resources;
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
        private Guna2Transition transition;
        private frmControllersParameter frmControllers;
        private frmControllers frmControllersMain;
        private frmMultiSignal frmMultiSignal;
        private frmResources frmResources;
        private FrmSettingController frmSettingController;
        private ucButtonDisplayGrid<DataforUnload> _Unload;
        private ucButtonDisplayGrid<Dataforload> _Load;
        private ucButtonDisplayGrid<Dataforload> _Load2;
        private ucButtonDisplayGrid<DataforNG4> _NG4;
        private CameraAS cam;
        private frmTeaching frmTeaching;
        private frmTeachingCamera frmTeachingCamera;
        private frmCurPos _frmCurPos;
        private frmSekectPos _frmSelected;
        private frmAlarm _frmAlarm;
        private KeyenceHostLinkTcpClient plc;
        private CancellationTokenSource _cts;
        private System.Timers.Timer timer;
        private System.Timers.Timer timerUPH;
        private readonly SemaphoreSlim _pollGate = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _pressGate = new SemaphoreSlim(1, 1);
        private Task _batchPollTask;
        private readonly SemaphoreSlim _batchGate = new SemaphoreSlim(1, 1);
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
            {42, "Timeout không mở được lắp socket, kiểm tra lắp socket hoặc điểm teach F" },
            {43, "Cửa Đang Mở,Bạn Có Muốn Chạy Tiếp Không" },
            {44, "Chart đang OFF" },
            {45, "Thiết Bị Đang Sửa Chữa" },
            {50, "Axis Alarm: Limit+ X" },
            {51, "Axis Alarm: Limit- X" },
            {52, "Axis Alarm: Limit- Y" },
            {53, "Axis Alarm: Limit- Z" },
            {54, "Kênh hàng trong Socket" },
            {55, "PC không kết nối với Handler máy Test, kiểm tra dây kết nối " },
            {56, "Sensor Cyclinder IN Socket Off" },
            {57, "Sensor Cyclinder OUT Socket Off" },
            {58, "Sensor Cyclinder UP Socket Off" },
            {59, "Sensor Cyclinder DOWN Socket Off" },
            {60, "Sensor Cyclinder UnClip Socket Off" },
            {61, "Sensor Cyclinder Clip Socket Off" },
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
                if (value == "Station1") _NameStation = "PORT1";
                else if (value == "Station2") _NameStation = "PORT2";
                else if (value == "Station3") _NameStation = "PORT3";
                else if (value == "Station4") _NameStation = "PORT4";
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
                //string filePattern = DateTime.Now.ToString("yyyy") + "*.log"; 
                //string filePattern = "P1" + "*.txt";
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
            if (logWatcher != null)
            {
                if (LogFileWatcher.OffMess)
                {
                    var s = DatabaseControllers.Instance.LoadDataFolder(lblModel.Text.ToString(), this.Nametation);
                    if (s == "")
                    {
                        MessageBox.Show($"Đường Dẫn ON MESS {this.Nametation} Chưa Có Trong Cơ Sở Dữ Liệu , Yêu Cầu nhập thêm đường dẫn ON MESS ", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        logWatcher.serverFolder = s;
                    }
                }
                else
                {
                    var s = DatabaseControllers.Instance.LoadDataFolderPathOFFMESS(lblModel.Text.ToString(), this.Nametation);
                    if (s == "")
                    {
                        MessageBox.Show($"Đường Dẫn ON MESS {this.Nametation} Chưa Có Trong Cơ Sở Dữ Liệu , Yêu Cầu nhập thêm đường dẫn OFF MESS ", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        logWatcher.serverFolder = s;
                    }
                }
            }
        }
        private void HandleLogLine(ushort value)
        {
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
            this.BeginInvoke((MethodInvoker)(() =>
            {
                lblResultTester.Text = value.ToString();
                if (value == 0)
                {
                    Interlocked.Increment(ref _OK);
                    lblStatus2.Text = "Pass";
                    lblStatus2.ForeColor = System.Drawing.Color.Green;
                    OK++;
                    // cập nhật ngay like code cũ
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
                    // cập nhật ngay like code cũ
                    NberPrdNGs.Text = _NG.ToString();
                    DatabaseControllers.Instance.InsertDataPortSummaNG(this.Nametation, NG);
                    LogProgram.WriteLog(this.nameStation + " PC send NG : " + value, this.Nametation);
                }
                // cập nhật Input realtime nếu muốn
                NberPrdIns.Text = _Input.ToString();
            }));
            CountDelete++;
            this.plc.SetBitInWord(this._ReStartNoData.Split('.')[0], int.Parse(this._ReStartNoData.Split('.')[1]));
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
                    this.BeginInvoke((MethodInvoker)(() => { lblStatus.Text = s; }));
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
                    //else
                    //{
                    //    if (RunTime1.IsRunning) RunTime1.Stop();
                    //    if (ErrTime1.IsRunning) ErrTime1.Stop();
                    //    if (IdleTime1.IsRunning) IdleTime1.Stop();
                    //}
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
            Task.Run(() =>
            {
                try
                {
                    ushort value = plc.ReadUInt16(map.alarmTm);
                    if (value == 0)
                    {
                        try
                        {
                            if (_frmAlarm != null)
                            {
                                this.BeginInvoke((MethodInvoker)(() => { _frmAlarm.Close(); }));
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
                    if (value == 31 /*&& _prevAlarmCode != 31 &&*/ || value == 32 || value == 33 || value == 34 || value == 35 && statusResult)
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
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            _frmAlarm = new frmAlarm(s, this.plc, this.nameStation);
                            _frmAlarm.Location = new Point(this.Location.X, this.Location.Y);
                            _frmAlarm.Model = lblModel.Text.ToString();
                            _frmAlarm.Show();
                        }));
                        LogProgram.MesWriteLog("Alarm: " + s, this.Nametation);
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("ReadAlarm error: " + ex, this.Nametation);
                }
            });
        }
        // ====== Timers ======
        private void TimerUPH_Elapsed(object sender, ElapsedEventArgs e)
        {
        }
        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!await _pollGate.WaitAsync(0)) return;
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
                this.Invoke((MethodInvoker)(() =>
                {
                    NberPrdNGs.Text = _NG.ToString();
                    NberPrdOKs.Text = _OK.ToString();
                    NberPrdIns.Text = _Input.ToString();
                    if (NberPrdIns.Text != "" && NberPrdOKs.Text != "")
                    {
                        float value = ((float)_OK / (float)_Input) * 100;
                        percentOKs.Text = value.ToString("0.00");
                    }
                    RunTime.Text = run.ToString(@"hh\:mm\:ss");
                    ErrTime.Text = err.ToString(@"hh\:mm\:ss");
                    IdleTime.Text = idle.ToString(@"hh\:mm\:ss");
                    var s = int.Parse(run.ToString(@"hh"));
                    INUPHs.Text = ((float)_Input / float.Parse(run.ToString(@"hh"))).ToString("0.00");
                    PASSUPHs.Text = ((float)_OK / float.Parse(run.ToString(@"hh"))).ToString("0.00");
                }));
            }
            finally
            {
                DatabaseControllers.Instance.UpdateCurrentValue(this.Nametation, _Input, _OK, _NG);
                DatabaseControllers.Instance.InsertDataPortSummaNG(this.Nametation, NG);
                DatabaseControllers.Instance.InsertDataPortSummaOK(this.Nametation, OK);
                DatabaseControllers.Instance.InsertDataPortSummaInput(this.Nametation, DatabaseControllers.Instance.SelectDataPortSummaNG(this.Nametation) + DatabaseControllers.Instance.SelectDataPortSummaOk(this.Nametation));
                _pollGate.Release();
            }
        }
        // ====== Load ======
        private void Form1_Load(object sender, EventArgs e)
        {
            AutoRead();
            StartBatchPoller();
            ReadDataforLoad();
            ReadDataForNG4();
            timer = new System.Timers.Timer(2500);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
            //timerUPH = new System.Timers.Timer(3600000);
            //timerUPH.Elapsed += TimerUPH_Elapsed;
            //timerUPH.AutoReset = true;
            //timerUPH.Start();
            lblStation.Text = this.Nametation;
            if (frmMaincs.conimiei)
            {
                label37.Text = "PASS";
                label37.BackColor = System.Drawing.Color.Lime;
                ReadDataforUnloadImei();
            }
            else
            {
                ReadDataforUnload();
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
                    this.BeginInvoke(new Action(() => { buttonGradion.Tag = tag; }));
                    if (nameUpper.Contains("RESETDATA") || nameUpper.Contains("SKIP") || nameUpper.Contains("RETRI") || nameUpper.IndexOf("Reset", StringComparison.OrdinalIgnoreCase) >= 0 || nameUpper.IndexOf("Set", StringComparison.OrdinalIgnoreCase) >= 0 || nameUpper.IndexOf("EndAuto", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        buttonGradion.MouseDown += MouseDowns;
                        buttonGradion.MouseUp += MouseUps;
                    }
                    else if (nameUpper.Contains("MODI"))
                    {
                        continue;
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
                                try
                                {
                                    int valueNow = plc.ReadInt32(m.Address);
                                    if (valueNow != m.LastValue)
                                    {
                                        m.LastValue = valueNow;
                                        string text = ((double)valueNow).ToString();
                                        this.BeginInvoke((MethodInvoker)(() => m.Label.Text = text));
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
                                try
                                {
                                    bool now = plc.ReadBitFromWord(b.Word, b.Bit);
                                    if (now != b.State)
                                    {
                                        b.State = now;
                                        this.BeginInvoke((MethodInvoker)(() =>
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
                DatabaseControllers.Instance.GetDataForUnload(this.nameStation), this.nameStation, "9000","UnLoad");
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
            _Load2 = new ucButtonDisplayGrid<Dataforload>(this.nX, this.ny, this.plc,
                DatabaseControllers.Instance.GetDataload(this.nameStation), this.nameStation, _ModifyWork,"Load");
            pnlUnload.Controls.Clear();
            pnlUnload.Controls.Add(_Load2);
            if (_NG4 != null)
            {
                _NG4._takPointLoad -= _Load2.UpdateLabel;
                _NG4._takPointLoad += _Load2.UpdateLabel;
            }
            _Load2.CheckDelete = () => { DeleteFile(); };
        }
        private void ReadDataforLoad()
        {
            _Load = new ucButtonDisplayGrid<Dataforload>(this.nX, this.ny, this.plc,
                DatabaseControllers.Instance.GetDataload(this.nameStation), this.nameStation, _ModifyWork,"Load");
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
                DatabaseControllers.Instance.GetDataNG4(this.nameStation), this.nameStation, "9000","UnLoad");
            pnlNG4.Controls.Clear();
            pnlNG4.Controls.Add(_NG4);
            _NG4.NG999 -= NG999;
            _NG4.NG999 += NG999;
            _NG4.CheckDelete -= DeleteFile;
            _NG4.CheckDelete += DeleteFile;
            _NG4._takPointLoad -= _Load.UpdateLabel;
            _NG4._takPointLoad += _Load.UpdateLabel;
        }
        // ====== Form closing ======
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (timer != null) timer.Stop();
                if (timerUPH != null) timerUPH.Stop();
                _cts.Cancel();
                // Dừng đồng hồ trong lock trước khi lưu
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
                DatabaseControllers.Instance.InsertDataPortSummaInput(this.Nametation, DatabaseControllers.Instance.SelectDataPortSummaNG(this.Nametation) + DatabaseControllers.Instance.SelectDataPortSummaOk(this.Nametation));
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog("FormClosing save times error: " + ex, this.Nametation);
            }
            finally
            {
                try { if (timer != null) timer.Dispose(); } catch { }
                try { if (timerUPH != null) timerUPH.Dispose(); } catch { }
                try { if (logWatcher != null) logWatcher.Dispose(); } catch { }
                try { _pollGate.Dispose(); } catch { }
                try { _pressGate.Dispose(); } catch { }
                try { _batchGate.Dispose(); } catch { }
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
        private void guna2ImageButton3_Click(object sender, EventArgs e)
        {
            frmResources = new frmResources();
            frmResources.Show();
        }
        private void guna2GradientButton14_Click(object sender, EventArgs e)
        {
            var frm = new frmLotoImei(() =>
            {
                frmTeaching2 frmTeaching2 = new frmTeaching2(this.nameStation, this.plc);
                frmTeaching2.Show();
            });
            frm.Show();
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
        }
        private void SetCompCompelete_Click(object sender, EventArgs e) 
        { 
            if (_Unload != null) _Unload.SetComp(); 
        }
        private void SetEmptyCompelete_Click(object sender, EventArgs e)
        { 
            if (_Unload != null) _Unload.SetEmptyUnLoad();
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
        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            frmTeachingCamera = new frmTeachingCamera();
            frmTeachingCamera.Show();
        }
        private void Guna2GradientButton21_Click(object sender, EventArgs e, int i)
        {
            _frmCurPos = new frmCurPos(this.nameStation, this.plc);
            _frmCurPos.Show();
        }
        private void guna2GradientButton21_Click(object sender, EventArgs e)
        {
            _frmCurPos = new frmCurPos(this.nameStation, this.plc);
            _frmCurPos.Show();
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
                    this.BeginInvoke((MethodInvoker)(() => btn.FillColor = System.Drawing.Color.Green));
            }
            else
            {
                if (await TryResetBitWithVerify(word, bit))
                    this.BeginInvoke((MethodInvoker)(() => btn.FillColor = System.Drawing.Color.Gray));
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
            if (runTime)
            {
                RunTime1.Start();
            }
            if (errTime)
            {
                ErrTime1.Start();
            }
            if (idleIime)
            {
                IdleTime1.Start();
            }
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
            try { this.Dispose(); this.Close(); } catch { }
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
                        statusReset = true;
                        LogProgram.WriteLog("Reset Tray Load :" + this.Nametation, this.Nametation);
                    }
                    else if (b == true && statusReset == true)
                    {
                        statusReset = false;
                        if (_Load != null) _Load.FullTray();
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
                    await Task.Delay(300);
                }
            }, _cts.Token);
        }
        public void Reset(object sender, EventArgs e)
        {
            timer.Stop();
            DatabaseControllers.Instance.DeleteNG(this.Nametation);
            NG = DatabaseControllers.Instance.SelectDataPortSummaNG(this.Nametation);
            OK = DatabaseControllers.Instance.SelectDataPortSummaOk(this.Nametation);
            // _pollGate.Release();
            timer.Start();
        }
        public void DeleteFile()
        {
            if (CountDelete >= 3)
            {
                try
                {
                    //DirectoryInfo dir = new DirectoryInfo(file);
                    //FileInfo value = dir.GetFiles(this.filePattern).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                    //if (value == null)
                    //{
                    //    MessageBox.Show("Không Tìm Thấy File");
                    //    return;
                    //}
                    CountDelete = 0;
                    Nodata?.Invoke(this, new EventArgs());
                    //string file;
                    switch (this.nameStation)
                    {
                        case "Station1": File.WriteAllText(@"C:\LOGLCA\log1.txt", ""); break;
                        case "Station2": File.WriteAllText(@"C:\LOGLCA\log2.txt", ""); break;
                        case "Station3": File.WriteAllText(@"C:\LOGLCA\log3.txt", ""); break;
                        case "Station4": File.WriteAllText(@"C:\LOGLCA\log4.txt", ""); break;
                            //  default: File.Delete(@"C:\LOGLCA\log.txt"); break;
                    }
                }
                catch (IOException ex)
                {
                    //  MessageBox.Show("Không Thể Xóa Do File Đang Được Truy Cập");
                }
                catch (Exception e)
                {
                    //  MessageBox.Show($"ERROR : {e}");
                }
            }
        }
        private void btnSetting_Click(object sender, EventArgs e)
        {
            var frm = new frmLotoImei(() =>
            {
                frmSetting _frmSetting = new frmSetting(this.nameStation, this.plc);
                _frmSetting.Show();
            });
            frm.Show();
        }
        private void btnControl_Click(object sender, EventArgs e)
        {
            frmControl _frmControl = new frmControl(this.nameStation, this.plc);
            _frmControl.Show();
        }
    }
}
