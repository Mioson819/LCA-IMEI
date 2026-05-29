using Guna.UI2.AnimatorNS;
using LCA_Project.Database;
using LCA_Project.Form.Devices.Controllers;
using LCA_Project.Form.frmAlarm;
using LCA_Project.Form.Signal;
using LCA_Project.Form.TesterComunication;
using LCA_Project.Services.Controllers;
using LCA_Project.Services.Logs;
using LCA_Project.Utilities;
using LCA_Samsung.Process;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
namespace LCA_Project
{
    public partial class frmMaincs : System.Windows.Forms.Form
    {
        // private CameraAS cam;
        private frmExport _frExport;
        private Form1 form1;
        public static string[] arraySetiing;
        private static string file_path_setting = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\setting.txt";
        private frmControllers frmControllersMain;
        private KeyenceHostLinkTcpClient plc;
        public delegate void SendResult(string s);
        public event SendResult _sendResult;
        private frmSignal _frmSignal;
        private CameraAS cam1;
        private CameraAS cam2;
        private delegate void StartRead(Dictionary<string, string> s);
        private event StartRead _startRead;
        private event EventHandler _stopRead;
        private delegate void SendModel(Dictionary<string, string> s);
        private event StartRead sendlabel;
        private event EventHandler OnMess;
        private Action _action;
        private frmUser _frmUser;
        private Form1 _form1;
        private Form1 _form2;
        private Form1 _form3;
        private Form1 _form4;
        private object _lock = new object();
        private string Port1, Port2, Port3, Port4;
        private frmWatting _frmWatting;
        private System.Timers.Timer timer;
        public static bool modeimei = true;
        public frmMaincs()
        {
            InitializeComponent();
            ControllerServices.Instance.InitializeController();
            foreach (var plc in ControllerServices.Instance.Controllers)
            {
                this.plc = plc.Value;
                break;
            }
            this.plc.Open();
            this.plc.StartCommunication();
            CamOnline1();
            CamOnline2();
            _frExport = new frmExport(this.plc);
            _frmWatting = new frmWatting();
            timer = new System.Timers.Timer();
            timer.Interval = 1200000;
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
            _action = () =>
            {
                _frExport.Delete();
            };
            if (this.plc.ReadBitFromWord("DM60500", 0))
            {
                SWImei.FillColor = System.Drawing.Color.Green;
                SWImei.Text = "Mode: IMEI Actuator";
                modeimei = true;
            }
            else
            {
                SWImei.FillColor = System.Drawing.Color.Red;
                SWImei.Text = "Mode: LCA Module";
                modeimei = false;
            }
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 8 || DateTime.Now.Hour == 20)
            {
                _action?.Invoke();
            }
        }
        private void CamOnline1()
        {
            cam1 = new CameraAS("192.168.0.77", 7890);
            cam1.Connect();
            cam1._send += SendDataToPLC;
            cam1._StatusJob += ChangeJob;
        }
        private void ChangeJob(string s1, string s2)
        {
            this.Invoke(new Action(() =>
            {
                _frmWatting.Watch(s1, s2);
            }));
        }
        private void CamOnline2()
        {
            cam2 = new CameraAS("192.168.0.78", 7890);
            cam2.Connect();
            cam2._send += SendDataToPLC;
            cam2._StatusJob += ChangeJob;
        }
        private void frmMaincs_Load(object sender, EventArgs e)
        {
            DatabaseControllers.Instance.LoadDataNameModel(cbxPort1, "Port1");
            DatabaseControllers.Instance.LoadDataNameModel(cbxPort2, "Port2");
            DatabaseControllers.Instance.LoadDataNameModel(cbxPort3, "Port3");
            DatabaseControllers.Instance.LoadDataNameModel(cbxPort4, "Port4");
        }
        private void btnDevices_Click(object sender, EventArgs e)
        {
            frmControllersMain = new frmControllers();
            frmControllersMain.Show();
        }
        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            _frExport.Show();
        }
        private async void btnHelp_Click(object sender, EventArgs e)
        {
            await frmshowall();
            //if (btnStation1.Visible == false)
            //{
            //    guna2Transition1.ShowSync(btnStation1);
            //    guna2Transition1.ShowSync(btnStation2);
            //    guna2Transition1.ShowSync(btnStation3);
            //    guna2Transition1.ShowSync(btnStation4);
            //}
            //else
            //{
            //    guna2Transition1.HideSync(btnStation1);
            //    guna2Transition1.HideSync(btnStation2);
            //    guna2Transition1.HideSync(btnStation3);
            //    guna2Transition1.HideSync(btnStation4);
            //}
        }
        private void btnStation1_Click(object sender, EventArgs e)
        {
            //if (cbxPort1.Text?.ToString() != "")
            //{
            //    var s = DatabaseControllers.Instance.GetnXnY("Station1", cbxPort1.Text.ToString());
            //    if (s == null)
            //    {
            //        System.Windows.Forms.MessageBox.Show("Ban Chua Nhap so luong  hang va cot cho model");
            //        return;
            //    }
            //    this._form1 = new Form1(plc, "Station1", 1, cbxPort1.Text?.ToString(), int.Parse(s[0]), int.Parse(s[1]));
            //    this._form1.Show();
            //    _form1._sendName += StartSend;
            //    _startRead += _form1.StartRead;
            //}
            //else
            //{
            //    System.Windows.Forms.MessageBox.Show("Ban Chua Chon Model");
            //}
        }
        private void btnStation2_Click(object sender, EventArgs e)
        {
            // if (cbxPort2.Text?.ToString() !="") {
            //     var s = DatabaseControllers.Instance.GetnXnY("Station2", cbxPort2.Text?.ToString());
            //     if (s== null)
            //     {
            //       System.Windows.Forms.MessageBox.Show("Ban Chua Nhap so luong  hang va cot cho model");
            //         return;
            //     }
            //_form2 = new Form1(plc, "Station2", 0, cbxPort2.Text?.ToString(), int.Parse(s[0]), int.Parse(s[1]));
            //    _form2.Show();
            //     _form2._sendName += StartSend;
            //     _startRead += _form2.StartRead;
            // }
            // else
            // {
            //     System.Windows.Forms.MessageBox.Show("Ban Chua Chon Model");
            // }
        }
        private void btnStation3_Click(object sender, EventArgs e)
        {
            //if (cbxPort3.Text?.ToString() != "")
            //{
            //    var s = DatabaseControllers.Instance.GetnXnY("Station3", cbxPort3.Text?.ToString());
            //    if (s == null)
            //    {
            //        System.Windows.Forms.MessageBox.Show("Ban Chua Nhap so luong  hang va cot cho model");
            //        return;
            //    }
            //    _form3 = new Form1(plc, "Station3", 1, cbxPort3.Text?.ToString(), int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]));
            //    _form3.Show();
            //    _form3._sendName += StartSend;
            //    _startRead += _form3.StartRead;
            //}
            //else
            //{
            //    System.Windows.Forms.MessageBox.Show("Ban Chua Chon Model");
            //}
        }
        private void btnStation4_Click(object sender, EventArgs e)
        {
            //if (cbxPort4.Text?.ToString() != "")
            //{
            //    var s = DatabaseControllers.Instance.GetnXnY("Station4", cbxPort4.Text?.ToString());
            //    if (s == null)
            //    {
            //        System.Windows.Forms.MessageBox.Show("Ban Chua Nhap so luong  hang va cot cho model");
            //        return;
            //    }
            //    _form4 = new Form1(plc, "Station4", 0, cbxPort4.Text?.ToString(), int.Parse(s[0]), int.Parse(s[1]));
            //    _form4.Show();
            //    _form4._sendName += StartSend;
            //    _startRead += _form4.StartRead;
            //}
            //else
            //{
            //    System.Windows.Forms.MessageBox.Show("Ban Chua Chon Model");
            //}
        }
        private async void btnAlarm_Click(object sender, EventArgs e)
        {
            // BUG 3 FIX: nếu form đã mở thì chỉ BringToFront, không tạo thêm instance
            if (_frmSignal != null && !_frmSignal.IsDisposed)
            {
                _frmSignal.BringToFront();
                return;
            }

            await ProcessMain.Instance.Initialize();

            // BUG 2 FIX: key của _Signal = Name từ bảng Controllers trong DB
            // Nếu DB lưu tên là "LCA" thì khớp; nếu không tìm thấy → log và thoát
            var kvSignal = ProcessMain.Instance._Signal
                .Where(x => x.Key == "LCA").FirstOrDefault();
            if (kvSignal.Value == null)
            {
                MessageBox.Show("Không tìm thấy controller 'LCA' trong Signal table.Kiểm tra bảng Controllers trong DB.",
                    "Signal Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _frmSignal = new frmSignal(kvSignal.Value, "LCA");
            _frmSignal.FormClosed += (s, v) => _frmSignal = null;
            _frmSignal.Show();
            _frmSignal.BringToFront();
        }
        private void StartSend(string NAME)
        {
            if (NAME == "Station1")
            {
                cam1.ChangFearture(NAME);
            }
            else if (NAME == "Station2")
            {
                cam1.TriggerResult(NAME);
            }
            else if (NAME == "Station4")
            {
                cam2.TriggerResult(NAME);
            }
            else
            {
                cam2.ChangFearture(NAME);
            }
        }
        private void SendDataToPLC(string[] result, string s)
        {
            // FIX: validate result trước khi parse tránh exception crash silent
            if (result == null || result.Length < 3)
            {
                LogProgram.WriteLog($"[SendDataToPLC] {s}: result null hoặc thiếu phần tử (nhận {result?.Length ?? 0})");
                return;
            }
            if (!Int32.TryParse(result[0], out Int32 v0) ||
                !Int32.TryParse(result[1], out Int32 v1) ||
                !Int32.TryParse(result[2], out Int32 v2))
            {
                LogProgram.WriteLog($"[SendDataToPLC] {s}: không parse được result: [{result[0]},{result[1]},{result[2]}]");
                return;
            }
            // FIX: Station4 tường minh thay vì dùng else (tránh ghi nhầm khi station không xác định)
            if (s == "Station1")
            {
                bool ok = this.plc.WriteInt32("DM780", v0)
                        & this.plc.WriteInt32("DM782", v1)
                        & this.plc.WriteInt32("DM784", v2);
                if (ok) plc.SetBitInWord("MR10200", 1);
                else LogProgram.WriteLog($"[SendDataToPLC] {s}: WriteInt32 thất bại, bỏ qua SetBit");
                LogProgram.WriteLog($"{s} EndTrigger ok={ok}");
            }
            else if (s == "Station2")
            {
                bool ok = this.plc.WriteInt32("DM720", v0)
                        & this.plc.WriteInt32("DM722", v1)
                        & this.plc.WriteInt32("DM724", v2);
                if (ok) plc.SetBitInWord("MR10200", 1);
                else LogProgram.WriteLog($"[SendDataToPLC] {s}: WriteInt32 thất bại, bỏ qua SetBit");
                LogProgram.WriteLog($"{s} EndTrigger ok={ok}");
            }
            else if (s == "Station3")
            {
                bool ok = this.plc.WriteInt32("DM810", v0)
                        & this.plc.WriteInt32("DM812", v1)
                        & this.plc.WriteInt32("DM814", v2);
                if (ok) plc.SetBitInWord("MR10500", 0);
                else LogProgram.WriteLog($"[SendDataToPLC] {s}: WriteInt32 thất bại, bỏ qua SetBit");
                LogProgram.WriteLog($"{s} EndTrigger ok={ok}");
            }
            else if (s == "Station4")
            {
                bool ok = this.plc.WriteInt32("DM800", v0)
                        & this.plc.WriteInt32("DM802", v1)
                        & this.plc.WriteInt32("DM804", v2);
                if (ok) plc.SetBitInWord("MR10500", 0);
                else LogProgram.WriteLog($"[SendDataToPLC] {s}: WriteInt32 thất bại, bỏ qua SetBit");
                LogProgram.WriteLog($"{s} EndTrigger ok={ok}");
            }
            else
            {
                LogProgram.WriteLog($"[SendDataToPLC] Station không xác định: '{s}' — bỏ qua");
            }
        }
        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (_form1 == null || _form2 == null || _form3 == null || _form4 == null || _form1.Created == false || _form2.Created == false || _form3.Created == false || _form4.Created == false)
            {
                btnStart.Enabled = false;
                System.Windows.Forms.MessageBox.Show("Please Open All Form");
                return;
            }
            if (btnStart.ForeColor != System.Drawing.Color.Green)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("Station1", DatabaseControllers.Instance.LoadDataFolder(cbxPort1.Text?.ToString(), "Port1"));
                values.Add("Station2", DatabaseControllers.Instance.LoadDataFolder(cbxPort2.Text?.ToString(), "Port2"));
                values.Add("Station3", DatabaseControllers.Instance.LoadDataFolder(cbxPort3.Text?.ToString(), "Port3"));
                values.Add("Station4", DatabaseControllers.Instance.LoadDataFolder(cbxPort4.Text?.ToString(), "Port4"));
                Dictionary<string, string> label = new Dictionary<string, string>();
                label.Add("Station1", cbxPort1.SelectedItem?.ToString());
                label.Add("Station2", cbxPort2.SelectedItem?.ToString());
                label.Add("Station3", cbxPort3.SelectedItem?.ToString());
                label.Add("Station4", cbxPort4.SelectedItem?.ToString());
                sendlabel?.Invoke(label);
                _startRead?.Invoke(values);
                btnStart.ForeColor = System.Drawing.Color.Green;

            }
            else
            {
                btnStart.ForeColor = System.Drawing.Color.Gray;
                _stopRead?.Invoke(this, new EventArgs());
            }
        }
        private void cbxPort1_SelectedValueChanged(object sender, EventArgs e)
        {
            Port1 = DatabaseControllers.Instance.IdModel("Port1", cbxPort1.Text.ToString());
            lblIdPort1.Text = Port1;

            if ((_form1 == null || _form1.IsDisposed) && !string.IsNullOrEmpty(Port1))
            {
                try
                {
                    var tag = DatabaseControllers.Instance.GetDataByKey("Station1");
                    if (tag != null && !string.IsNullOrWhiteSpace(tag.IdJob))
                    {
                        this.plc.WriteInt32(tag.IdJob, int.Parse(Port1));
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("cbxPort1 write IdJob to PLC error: " + ex.Message);
                }
            }
        }
        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Port2 = DatabaseControllers.Instance.IdModel("Port2", cbxPort2.Text.ToString());
            lblIdPort2.Text = Port2;

            if ((_form2 == null || _form2.IsDisposed) && !string.IsNullOrEmpty(Port2))
            {
                try
                {
                    var tag = DatabaseControllers.Instance.GetDataByKey("Station2");
                    if (tag != null && !string.IsNullOrWhiteSpace(tag.IdJob))
                    {
                        this.plc.WriteInt32(tag.IdJob, int.Parse(Port2));
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("cbxPort2 write IdJob to PLC error: " + ex.Message);
                }
            }
        }
        private void guna2ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Port3 = DatabaseControllers.Instance.IdModel("Port3", cbxPort3.Text.ToString());
            lblIdPort3.Text = Port3;

            if ((_form3 == null || _form3.IsDisposed) && !string.IsNullOrEmpty(Port3))
            {
                try
                {
                    var tag = DatabaseControllers.Instance.GetDataByKey("Station3");
                    if (tag != null && !string.IsNullOrWhiteSpace(tag.IdJob))
                    {
                        this.plc.WriteInt32(tag.IdJob, int.Parse(Port3));
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("cbxPort3 write IdJob to PLC error: " + ex.Message);
                }
            }
        }
        private void cbxPort4_SelectedIndexChanged(object sender, EventArgs e)
        {
            Port4 = DatabaseControllers.Instance.IdModel("Port4", cbxPort4.Text.ToString());
            lblIdPort4.Text = Port4;

            if ((_form4 == null || _form4.IsDisposed) && !string.IsNullOrEmpty(Port4))
            {
                try
                {
                    var tag = DatabaseControllers.Instance.GetDataByKey("Station4");
                    if (tag != null && !string.IsNullOrWhiteSpace(tag.IdJob))
                    {
                        this.plc.WriteInt32(tag.IdJob, int.Parse(Port4));
                    }
                }
                catch (Exception ex)
                {
                    LogProgram.WriteLog("cbxPort4 write IdJob to PLC error: " + ex.Message);
                }
            }
        }
        private void frmMaincs_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                cam1.Disconnect();
                cam2.Disconnect();
                timer.Stop();
            }
            catch { }
        }
        private async Task frmshowall()
        {
            System.Drawing.Rectangle work = Screen.PrimaryScreen.WorkingArea;
            int W = work.Width / 2;
            int H = work.Height / 2;
            if (cbxPort1.Text?.ToString() != "" && cbxPort2.Text?.ToString() != "" && cbxPort3.Text?.ToString() != "" && cbxPort4.Text?.ToString() != "")
            {
                var s1 = DatabaseControllers.Instance.GetnXnY("Port1", cbxPort1.Text.ToString());
                var s2 = DatabaseControllers.Instance.GetnXnY("Port2", cbxPort2.Text?.ToString());
                var s3 = DatabaseControllers.Instance.GetnXnY("Port3", cbxPort3.Text?.ToString());
                var s4 = DatabaseControllers.Instance.GetnXnY("Port4", cbxPort4.Text?.ToString());
                if (s1 == null && s2 == null && s3 == null && s4 == null)
                {
                    System.Windows.Forms.MessageBox.Show("Ban Chua Nhap so luong  hang va cot cho model");
                    return;
                }
                if (_form1 == null || _form1.IsDisposed)
                {
                    this._form1 = new Form1(plc, "Station1", 1, cbxPort1.Text?.ToString(), int.Parse(s1[0]), int.Parse(s1[1]), int.Parse(s1[2]), cam1);
                    this._form1.StartPosition = FormStartPosition.Manual;
                    this._form1.Bounds = new System.Drawing.Rectangle(work.Left, work.Top, W, H);
                    _form1.Show();
                    _form1._sendName -= StartSend;
                    _form1._sendName += StartSend;
                    sendlabel -= _form1.UpdateModel;
                    sendlabel += _form1.UpdateModel;
                    _startRead -= _form1.StartRead;
                    _startRead += _form1.StartRead;
                    _stopRead -= _form1.StopRead;
                    _stopRead += _form1.StopRead;
                    OnMess -= _form1.OnMess;
                    OnMess += _form1.OnMess;
                    _frExport.Reset -= _form1.Reset;
                    _frExport.Reset += _form1.Reset;
                    _form1.FormClosed += (s, v) =>
                    {
                        btnStart.Enabled = false;
                        btnStart.ForeColor = System.Drawing.Color.Yellow;
                    };
                    await Task.Delay(500);
                }
                //-----------------------------------------------------------------------------------------------------------------------
                if (_form2 == null || _form2.IsDisposed)
                {
                    _form2 = new Form1(plc, "Station2", 0, cbxPort2.Text?.ToString(), int.Parse(s2[0]), int.Parse(s2[1]), int.Parse(s2[2]), cam1);
                    this._form2.StartPosition = FormStartPosition.Manual;
                    this._form2.Bounds = new System.Drawing.Rectangle(work.Left + W, work.Top, W, H);
                    _form2.Show();
                    _form2._sendName -= StartSend;
                    _form2._sendName += StartSend;
                    sendlabel -= _form2.UpdateModel;
                    sendlabel += _form2.UpdateModel;
                    _startRead -= _form2.StartRead;
                    _startRead += _form2.StartRead;
                    _stopRead -= _form2.StopRead;
                    _stopRead += _form2.StopRead;
                    OnMess -= _form2.OnMess;
                    OnMess += _form2.OnMess;
                    _frExport.Reset -= _form2.Reset;
                    _frExport.Reset += _form2.Reset;
                    _form2.FormClosed += (s, v) =>
                    {
                        btnStart.Enabled = false;
                        btnStart.ForeColor = System.Drawing.Color.Yellow;
                    };
                    await Task.Delay(500);
                }
                //-----------------------------------------------------------------------------------------------------------------------
                if (_form3 == null || _form3.IsDisposed)
                {
                    _form3 = new Form1(plc, "Station3", 1, cbxPort3.Text?.ToString(), int.Parse(s3[0]), int.Parse(s3[1]), int.Parse(s3[2]), cam2);
                    this._form3.StartPosition = FormStartPosition.Manual;
                    this._form3.Bounds = new System.Drawing.Rectangle(work.Left, work.Top + H, W, H);
                    _form3.Show();
                    _form3._sendName -= StartSend;
                    _form3._sendName += StartSend;
                    sendlabel -= _form3.UpdateModel;
                    sendlabel += _form3.UpdateModel;
                    _startRead -= _form3.StartRead;
                    _startRead += _form3.StartRead;
                    _stopRead -= _form3.StopRead;
                    _stopRead += _form3.StopRead;
                    OnMess -= _form3.OnMess;
                    OnMess += _form3.OnMess;
                    _frExport.Reset -= _form3.Reset;
                    _frExport.Reset += _form3.Reset;
                    _form3.FormClosed += (s, v) =>
                    {
                        btnStart.Enabled = false;
                        btnStart.ForeColor = System.Drawing.Color.Yellow;
                    };
                    await Task.Delay(500);
                }
                //-----------------------------------------------------------------------------------------------------------------------
                if (_form4 == null || _form4.IsDisposed)
                {
                    _form4 = new Form1(plc, "Station4", 0, cbxPort4.Text?.ToString(), int.Parse(s4[0]), int.Parse(s4[1]), int.Parse(s4[2]), cam2);
                    this._form4.StartPosition = FormStartPosition.Manual;
                    this._form4.Bounds = new System.Drawing.Rectangle(work.Left + W, work.Top + H, W, H);
                    _form4.Show();
                    _form4._sendName -= StartSend;
                    _form4._sendName += StartSend;
                    sendlabel -= _form4.UpdateModel;
                    sendlabel += _form4.UpdateModel;
                    _startRead -= _form4.StartRead;
                    _startRead += _form4.StartRead;
                    _stopRead -= _form4.StopRead;
                    _stopRead += _form4.StopRead;
                    OnMess -= _form4.OnMess;
                    OnMess += _form4.OnMess;
                    _frExport.Reset -= _form4.Reset;
                    _frExport.Reset += _form4.Reset;
                    _form4.FormClosed += (s, v) =>
                    {
                        btnStart.Enabled = false;
                        btnStart.ForeColor = System.Drawing.Color.Yellow;
                    };
                }
                btnStart.Enabled = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Ban Chua Chon Model Port ");
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (_form1 != null && _form2 != null && _form3 != null && _form4 != null && _form1.Created && _form2.Created && _form3.Created && _form4.Created && _form1.logWatcher != null && _form2.logWatcher != null && _form3.logWatcher != null && _form4.logWatcher != null)
            {
                if (LogFileWatcher.OffMess == false)
                {
                    LogFileWatcher.OffMess = true;
                    //btnStatusMes.ForeColor = System.Drawing.Color.Green;
                    //btnStatusMes.Text = "PAMTEK";
                    OnMess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    LogFileWatcher.OffMess = false;
                    btnStatusMes.ForeColor = System.Drawing.Color.Green;
                    btnStatusMes.Text = "NANO";
                    OnMess?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please Open All Form");
            }
        }
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            cbxPort1.Items.Clear();
            cbxPort2.Items.Clear();
            cbxPort3.Items.Clear();
            cbxPort4.Items.Clear();
            DatabaseControllers.Instance.LoadDataNameModel(cbxPort1, "Port1");
            DatabaseControllers.Instance.LoadDataNameModel(cbxPort2, "Port2");
            DatabaseControllers.Instance.LoadDataNameModel(cbxPort3, "Port3");
            DatabaseControllers.Instance.LoadDataNameModel(cbxPort4, "Port4");
            lblIdPort1.Text = "";
            lblIdPort2.Text = "";
            lblIdPort3.Text = "";
            lblIdPort4.Text = "";
            Port1 = "";
            Port2 = "";
            Port3 = "";
            Port4 = "";
        }
        private void panelContain_Paint(object sender, PaintEventArgs e)
        {
        }
        private void btnUser_Click(object sender, EventArgs e)
        {
            var loto = new frmLotoImei(
                onSuccess: () =>
                {
                    // Sau khi xác thực thành công: enable admin buttons trên tất cả các form
                    Form1[] stations = { _form1, _form2, _form3, _form4 };
                    foreach (var f in stations)
                    {
                        if (f != null && !f.IsDisposed)
                            f.SetAdminButtons(true);
                    }
                    btnUser.Text = "Master";
                },
                onLock: () =>
                {
                    // Khoá lại quyền admin trên tất cả các form
                    Form1[] stations = { _form1, _form2, _form3, _form4 };
                    foreach (var f in stations)
                    {
                        if (f != null && !f.IsDisposed)
                            f.SetAdminButtons(false);
                    }
                    btnUser.Text = "User";
                }
            );
            loto.Show();
        }

        // Helper: đóng an toàn 1 Form1, bỏ qua nếu null hoặc đã bị dispose
        private void CloseForm(Form1 form)
        {
            if (form == null || form.IsDisposed) return;
            try { form.Close(); } catch { }
        }

        private void SWImei_Click(object sender, EventArgs e)
        {
            // Kiểm tra còn form nào đang mở không
            bool anyOpen = (_form1 != null && !_form1.IsDisposed)
                        || (_form2 != null && !_form2.IsDisposed)
                        || (_form3 != null && !_form3.IsDisposed)
                        || (_form4 != null && !_form4.IsDisposed);

            if (anyOpen)
            {
                var confirmClose = System.Windows.Forms.MessageBox.Show(
                    "Đang có Form đang mở. Đóng tất cả Form trước khi đổi Mode?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmClose != DialogResult.Yes) return;

                // Đóng cả 4 form (dùng if riêng biệt, KHÔNG dùng else if)
                CloseForm(_form1);
                CloseForm(_form2);
                CloseForm(_form3);
                CloseForm(_form4);
            }

            // Hỏi xác nhận đổi Mode
            var rsSWimei = System.Windows.Forms.MessageBox.Show(
                "Do you confirm Change Mode Imei ?" + Environment.NewLine +
                "Yes: Change to Mode Imei" + Environment.NewLine +
                "No: Change to Mode LCA Module",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            switch (rsSWimei)
            {
                case DialogResult.Yes:
                    try
                    {
                        this.plc.SetBitInWord("DM60500", 0);
                        if (this.plc.ReadBitFromWord("DM60500", 0))
                        {
                            SWImei.FillColor = System.Drawing.Color.Green;
                            SWImei.Text = "Mode: IMEI Actuator";
                            modeimei = true;
                            btnUser.Enabled = true;
                        }
                        else
                        {
                            SWImei.FillColor = System.Drawing.Color.Red;
                            SWImei.Text = "Mode: LCA Module";
                            modeimei = false;
                            System.Windows.Forms.MessageBox.Show("Failed to Set SWImei.");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogProgram.WriteLog("Fail To Set SWImei: " + ex.Message);
                    }
                    break;

                case DialogResult.No:
                    try
                    {
                        this.plc.ResetBitInWord("DM60500", 0);
                        if (!this.plc.ReadBitFromWord("DM60500", 0))
                        {
                            SWImei.FillColor = System.Drawing.Color.Red;
                            SWImei.Text = "Mode: LCA Module";
                            modeimei = false;
                            btnUser.Enabled = false;
                        }
                        else
                        {
                            SWImei.FillColor = System.Drawing.Color.Green;
                            SWImei.Text = "Mode: IMEI Actuator";
                            modeimei = true;
                            System.Windows.Forms.MessageBox.Show("Failed to Reset SWImei.");
                        }
                        // Reset bit ChangeModeTrayInput về 0 trên tất cả form đã đóng
                        foreach (string st in new[] { "Station1", "Station2", "Station3", "Station4" })
                        {
                            try
                            {
                                var d = DatabaseControllers.Instance.GetDataByInputResults(st);
                                if (d == null) continue;
                                string tag = (d.ChangeModeTrayInput + "").Trim();
                                if (string.IsNullOrWhiteSpace(tag)) continue;
                                string[] parts = tag.Split('.');
                                string word = parts[0];
                                int bit = parts.Length > 1 && int.TryParse(parts[1], out int b) ? b : 0;
                                this.plc.ResetBitInWord(word, bit);
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogProgram.WriteLog("Fail To Reset SWImei: " + ex.Message);
                    }
                    break;
            }
        }
        //private void btnChangeModel1_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (Port1 == null || Port2 == null)
        //        {
        //            System.Windows.MessageBox.Show("Please Select Full Model Port1 and Port2");
        //            return;
        //        }
        //        else
        //        {
        //            if (System.Windows.Forms.MessageBox.Show("Do you confirm Change Model ?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
        //            this.plc.SetBitInWord(lblIdPort1.Tag.ToString().Split('.')[0], int.Parse(lblIdPort1.Tag.ToString().Split('.')[1]));
        //            this.plc.SetBitInWord(lblIdPort2.Tag.ToString().Split('.')[0], int.Parse(lblIdPort2.Tag.ToString().Split('.')[1]));
        //            string value = DatabaseControllers.Instance.GetIdModel("Port1", "Port2", cbxPort1.Text.ToString(), cbxPort2.Text.ToString());
        //            if (string.IsNullOrEmpty(value))
        //            {
        //                return;
        //            }
        //            if (btnUser.Text == "Master")
        //            {
        //                cam1.Reconnect("00" + value, "00" + Port1 + "_" + "00" + Port2, 23, cbxPort1.Text.ToString(), cbxPort2.Text.ToString());
        //                _frmWatting.Show();
        //            }
        //        }
        //        btnChangeModel1.FillColor = System.Drawing.Color.Green;
        //    }
        //    finally
        //    {
        //        btnChangeModel1.FillColor = System.Drawing.Color.Gray;
        //    }
        //}
        //private void lblIdPort1_Click(object sender, EventArgs e)
        //{
        //}
        //private void btnChangeModel2_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (Port3 == null || Port4 == null)
        //        {
        //            System.Windows.MessageBox.Show("Please Select Full Model Port3 and Port4");
        //            return;
        //        }
        //        else
        //        {
        //            if (System.Windows.Forms.MessageBox.Show("Do you confirm Change Model ?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
        //            this.plc.SetBitInWord(lblIdPort3.Tag.ToString().Split('.')[0], int.Parse(lblIdPort3.Tag.ToString().Split('.')[1]));
        //            this.plc.SetBitInWord(lblIdPort4.Tag.ToString().Split('.')[0], int.Parse(lblIdPort4.Tag.ToString().Split('.')[1]));
        //            btnChangeModel2.FillColor = System.Drawing.Color.Green;
        //            string value = DatabaseControllers.Instance.GetIdModel("Port3", "Port4", cbxPort3.Text.ToString(), cbxPort4.Text.ToString());
        //            if (string.IsNullOrEmpty(value))
        //            {
        //                return;
        //            }
        //            if (btnUser.Text == "Master")
        //            {
        //                cam2.Reconnect("00" + value, "00" + Port3 + "_" + "00" + Port4, 23, cbxPort3.Text.ToString(), cbxPort4.Text.ToString());
        //                _frmWatting.Show();
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        btnChangeModel2.FillColor = System.Drawing.Color.Gray;
        //    }
        //}
    }
}