using DocumentFormat.OpenXml.Wordprocessing;
using Guna.UI2.AnimatorNS;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Services.Controllers;
using LCA_Project.Utilities;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace LCA_Project.Form.Teaching
{
    public partial class frmTeaching : System.Windows.Forms.Form
    {
        // ====== Fields ======
        private string _NameStation;
        public string Nametation
        {
            get => _NameStation;
            set
            {
                if (value == "Station1")
                {
                    _NameStation = "PORT1";
                }
                else if (value == "Station2")
                {
                    _NameStation = "PORT2";
                }
                else if (value == "Station3")
                {
                    _NameStation = "PORT3";
                }
                else if (value == "Station4")
                {
                    _NameStation = "PORT4";
                }
            }
        }
        private  Dictionary<string,System.Windows.Forms.Control> _button;
        private Dictionary<string,System.Windows.Forms.Control> _buttonn;
        private List<Guna2TextBox> DataCamera, Data, DataAxisX, DataAxisY, DataAxisZ, DataAxisF, DataAxisRI, DataAxisRO, DataLoad, DataUnload1, DataUnload2, DataNG1, DataNG2, DataNG3, DataNG4;
        private List<Guna2TextBox> DataAll;
        private string _nameController { get; set; }
        private KeyenceHostLinkTcpClient _controller { get; set; }
        private bool _wiredEvents = false;
        private bool _tagsBound = false;
        private bool _groupsBuilt = false;
        private CancellationTokenSource _ctsAction;
        private CancellationTokenSource _ctsLabelPoll;
        private bool _labelPolling;
        private CameraAS _cam;
        private readonly Dictionary<Label, int> _labelLastRaw = new Dictionary<Label, int>();      // cho Int32
        private readonly Dictionary<Label, short> _labelLastRaw16 = new Dictionary<Label, short>(); // cho Int16 (Id)
        private string _RqCallModel {  get; set; }
        private string _RqSaveModel { get; set; }
        private System.Windows.Forms.Control _activeContainer;
        public frmTeaching(string NameController, KeyenceHostLinkTcpClient plc,CameraAS _cam)
        {
            InitializeComponent();
            _nameController = NameController;
            Nametation = NameController; 
            _controller = plc;
            this._cam = _cam;
            this._cam._StartCalib += (s, v) =>
            {
                EndCalib.Enabled = true;
                Move.Enabled = true;
            };
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_wiredEvents) return;
            WireControlsOnce();
            _wiredEvents = true;
#if DEBUG
            DebugDumpButtonHandlerCounts();
#endif
        }
        private void frmTeaching_Load(object sender, EventArgs e)
        {
            _button = new Dictionary<string,System.Windows.Forms.Control>
            {
                { "JOG_X",JOG_XX},
                { "JOG_Y",JOG_YY},
                { "JOG_Z",JOG_ZZ},
                { "JOG_RI",JOG_RII},
                { "JOG_RO",JOG_ROO},
                { "JOG_F",JOG_FF },
                { "JOG__X",JOG__XX},
                { "JOG__Y",JOG__YY},
                { "JOG__Z",JOG__ZZ},
                { "JOG__F",JOG__FF},
                { "JOG__RI",JOG__RII},
                { "JOG__RO",JOG__ROO},
                { "CurPosX",CurPosXX},
                { "CurPosY",CurPosYY},
                { "CurPosZ",CurPosZZ},
                { "CurPosF",CurPosFF},
                { "CurPosRI",CurPosRII},
                { "CurPosRO",CurPosROO},
                { "SWsStep2",SWsStep22},
                { "SWsHSp2",SWsHSp22},
                { "DisStep__JogX",DisStep__JogXX},
                { "DisStep__JogY",DisStep__JogYY},
                { "DisStep__JogZ",DisStep__JogZZ},
                { "DisStep__JogF",DisStep__JogFF},
                { "DisStep__JogRI",DisStep__JogRII},
                { "DisStep__JogRO",DisStep__JogROO},
                { "RESETX",RESETXX},
                { "RESETY",RESETYY},
                { "RESETZ",RESETZZ},
                { "RESETF",RESETFF},
                { "RESETRI",RESETRII},
                { "RESETROO",RESETROO},
                { "ORGX",ORGXX},
                { "ORGY",ORGYY},
                { "ORGZ",ORGZZ},
                { "ORGF",ORGFF},
                { "ORGRI",ORGRII},
                { "ORGRO",ORGROO},
            };
            _buttonn = new Dictionary<string,System.Windows.Forms.Control>
            {
                { "JOG_X",JOG_XXX},
                { "JOG_Y",JOG_YYY},
                { "JOG_Z",JOG_ZZZ},
                { "JOG_RI",JOG_RIII},
                { "JOG__X",JOG__XXX},
                { "JOG__Y",JOG__YYY},
                { "JOG__Z",JOG__ZZZ},
                { "JOG__RI",JOG__RIII},
                { "CurPosX",CurPosXXX},
                { "CurPosY",CurPosYYY},
                { "CurPosZ",CurPosZZZ},
                { "CurPosRI",CurPosRIII},
                { "SWsStep2",SWsStep222},
                { "DisStep__JogX",DisStep__JogXXX},
                { "DisStep__JogY",DisStep__JogYYY},
                { "DisStep__JogZ",DisStep__JogZZZ},
                { "DisStep__JogRI",DisStep__JogRIII},
                { "RESETX",RESETXXX},
                { "RESETY",RESETYYY},
                { "RESETZ",RESETZZZ},
                { "RESETRI",RESETRIII},
                { "ORGX",ORGXXX},
                { "ORGY",ORGYYY},
                { "ORGZ",ORGZZ},
                { "ORGRI",ORGRIII},
            };
            lblNamePort.Text = "Teaching " + $": {_NameStation}";
            if (_controller == null)
            {
                MessageBox.Show("Controller not found or Not connected.");
                Close();
                return;
            }
            if (!_tagsBound)
            {
                BindTagsOnce();
                _tagsBound = true;
            }
            if (!_groupsBuilt)
            {
                GroupDataAxisX(); GroupDataAxisY(); GroupDataAxisZ(); GroupDataAxisF();
                GroupDataAxisRI(); GroupDataAxisRO(); /*GroupDataLoad1(); GroupDataUnLoad1();*/
                /*GroupDataUnLoad2()*/; GroupDataCamera(); GroupData(); GroupDataNG1();
                /*GroupDataNG2(); GroupDataNG3(); GroupDataNG4(); */GroupDataAll();
                _groupsBuilt = true;
            }
            if (TabControl != null)
            {
                TabControl.SelectedIndexChanged -= TabControl_SelectedIndexChanged;
                TabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            }
            _activeContainer = GetActiveContainerOrForm();
            ReadTextBoxesInContainer(_activeContainer);
            StartLabelPolling();
        }
        private void frmTeaching_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopLabelPolling();
            if (_ctsAction != null)
            {
                try { _ctsAction.Cancel(); } catch { }
                try { _ctsAction.Dispose(); } catch { }
                _ctsAction = null;
            }
        }
        // ====== Wire events once ======
        private void WireControlsOnce()
        {
            foreach (System.Windows.Forms.Control control in GetAllControls(this))
            {
                var button = control as Guna2GradientButton;
                if (button != null)
                {
                    button.MouseDown -= MouseDowns;
                    button.MouseUp -= MouseUps;
                    button.Click -= Clicks;
                    button.Click -= Clicks2;
                    string btnName = (button.Name ?? string.Empty).Trim();
                    bool isSaveSendButton =
                        btnName.StartsWith("btnSave", StringComparison.OrdinalIgnoreCase) ||
                        btnName.StartsWith("btnSend", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(btnName, "btnSend", StringComparison.OrdinalIgnoreCase);
                    bool isBitHoldButton =
                        btnName.IndexOf("ABS", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btnName.IndexOf("JOG", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btnName.IndexOf("RESET", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btnName.IndexOf("INC", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btnName.IndexOf("Run", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         btnName.IndexOf("RqOpen", StringComparison.OrdinalIgnoreCase) >= 0||
                           btnName.IndexOf("RqClose", StringComparison.OrdinalIgnoreCase) >= 0; 
                    bool isBitClickToggle =
                        btnName.IndexOf("Rq", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btnName.IndexOf("SWs", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool isRqSaveModel =
                       btnName.IndexOf("RqSaveModel", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (isSaveSendButton)
                    {
                        continue;
                    }
                    else if (isBitHoldButton)
                    {
                        button.MouseDown += MouseDowns;
                        button.MouseUp += MouseUps;
                    }
                    else if (isBitClickToggle&& !string.Equals(btnName, "RqSaveModel", StringComparison.OrdinalIgnoreCase) && !string.Equals(btnName, "RqSaveModelAs", StringComparison.OrdinalIgnoreCase) && !string.Equals(btnName, "RqCallModel", StringComparison.OrdinalIgnoreCase))
                    {
                            button.Click += Clicks2;
                    }
                    else
                    {
                        if (string.Equals(btnName, "RqCallModel", StringComparison.OrdinalIgnoreCase)|| string.Equals(btnName, "RqSaveModelAs", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                            button.Click += Clicks; 
                    }
                  //bool status =  _controller.ReadBitFromWord(button.Tag.ToString().Split('.')[0], int.Parse(button.Tag.ToString().Split('.')[1]));
                  //  if (status == true)
                  //  {
                  //      button.FillColor = System.Drawing.Color.Green;
                  //  }
                }
            }
            btnSaveXdata.Click -= SendDataAxisX;
            btnSaveYdata.Click -= SendDataAxisY;
            btnSaveZdata.Click -= SendDataAxisZ;
            btnSaveFdata.Click -= SendDataAxisF;
            btnSaveRIdata.Click -= SendDataAxisRI;
            btnSaveROdata.Click -= SendDataAxisRO;
            btnSaveNG1Data.Click -= SendDataNG1;
          //  btnSaveNG2Data.Click -= SendDataNG2;
         //   btnSaveNG3Data.Click -= SendDataNG3;
           // btnSaveNG4Data.Click -= SendDataNG4;
            btnSaveModelNow.Click -= SendData;
          //  btnSaveUnload1.Click -= SendDataUnload1;
           // btnSaveUnload2.Click -= SendDataUnLoad2;
           // btnSaveLoad.Click -= SendDataLoad1;
            btnSendAll.Click -= SendAll_Click;
            btnSaveXdata.Click += SendDataAxisX;
            btnSaveYdata.Click += SendDataAxisY;
            btnSaveZdata.Click += SendDataAxisZ;
            btnSaveFdata.Click += SendDataAxisF;
            btnSaveRIdata.Click += SendDataAxisRI;
            btnSaveROdata.Click += SendDataAxisRO;
            btnSaveNG1Data.Click += SendDataNG1;
            //btnSaveNG2Data.Click += SendDataNG2;
            //btnSaveNG3Data.Click += SendDataNG3;
            //btnSaveNG4Data.Click += SendDataNG4;
            btnSaveModelNow.Click += SendData;
            //btnSaveUnload1.Click += SendDataUnload1;
            //btnSaveUnload2.Click += SendDataUnLoad2;
            //btnSaveLoad.Click += SendDataLoad1;
            btnSendAll.Click += SendAll_Click;
            RqCallModel.Click -= RqCallModel_Click;
            RqCallModel.Click += RqCallModel_Click;
            RqSaveModelAs.Click -= RqModelSaveAs_Click;
            RqSaveModelAs.Click += RqModelSaveAs_Click;
        }
        private void BindTagsOnce()
        {
            var dataforController = DatabaseControllers.Instance.GetDataByKey(_nameController);
            var type = typeof(DataforTagControl);
            foreach (System.Windows.Forms.Control control in GetAllControls(this))
            {
                var prop = type.GetProperty((control.Name ?? string.Empty).Trim(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || dataforController == null) continue;
                var tagObj = prop.GetValue(dataforController);
                var tag = tagObj != null ? tagObj.ToString().Trim() : null;
                if (!string.IsNullOrEmpty(tag))
                {
                    control.Tag = tag; 
                    if(_button.TryGetValue(control.Name, out var button))
                    {
                        button.Tag = tag;
                    }
                    if (_buttonn.TryGetValue(control.Name, out var buttonn))
                    {
                        buttonn.Tag = tag;
                    }
                    if (string.Equals(control.Name, "RqSaveModel", StringComparison.OrdinalIgnoreCase))
                    {
                        this._RqSaveModel = control.Tag.ToString();
                    }
                }
            }
        }
        private void ReadTextBoxesInContainer(System.Windows.Forms.Control container)
        {
            if (container == null) return;
            foreach (System.Windows.Forms.Control control in GetAllControls(container))
            {
                var tb = control as Guna2TextBox;
                if (control is Guna2GradientButton _button)
                {
                    if (_button.Tag == null || _button.Tag.ToString().IndexOf("Rq", StringComparison.OrdinalIgnoreCase) >= 0 || _button.Tag.ToString().IndexOf("ORG", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }
                    if (_controller.ReadBitFromWord(_button.Tag.ToString().Split('.')[0], int.Parse(_button.Tag.ToString().Split('.')[1])))
                    {
                        _button.FillColor = System.Drawing.Color.Green;
                    }
                }
                if (tb == null) continue;
                var addr = tb.Tag as string;
                if (string.IsNullOrWhiteSpace(addr)) continue;
                try
                {
                    string name = tb.Name ?? string.Empty;
                    if (name.IndexOf("ModelSaveAs", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }
                    if (ContainsAny(name, "mY", "nX", "nY", "SpStart", "Acc", "Dec", "RsNG", "IdJob", "ReCheck", "NumberReCheck"))
                    {
                        short v = _controller.ReadInt16(addr);
                        tb.Text = (v / 1f).ToString("0.0", CultureInfo.InvariantCulture);
                    }
                    else if (name.IndexOf("Time", StringComparison.OrdinalIgnoreCase) >= 0|| name.IndexOf("AngeRO", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        short v = _controller.ReadInt16(addr);
                        tb.Text = (v / 10f).ToString("0.0", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        int v = _controller.ReadInt32(addr);
                        tb.Text = (v / 100.0).ToString("0.00", CultureInfo.InvariantCulture);
                    }
                }
                catch
                {
                }
            }
        }
        private void ReadBackGroup(List<Guna2TextBox> group)
        {
            if (group == null || group.Count == 0) return;
            foreach (var tb in group)
            {
                if (tb == null) continue;
                var addr = tb.Tag as string;
                if (string.IsNullOrWhiteSpace(addr)) continue;
                try
                {
                    string name = tb.Name ?? string.Empty;
                    if (name.IndexOf("ModelSaveAs", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }
                    if (ContainsAny(name, "mY", "nX", "nY", "SpStart", "IdJob", "Acc", "Dec", "RsNG", "ReCheck", "NumberReCheck"))
                    {
                        short v = _controller.ReadInt16(addr);
                        tb.Text = (v / 1f).ToString("0.0", CultureInfo.InvariantCulture);
                    }
                    else if (name.IndexOf("Time", StringComparison.OrdinalIgnoreCase) >= 0|| name.IndexOf("AngeRO", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        short v = _controller.ReadInt16(addr);
                        tb.Text = (v / 10f).ToString("0.0", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        int v = _controller.ReadInt32(addr);
                        tb.Text = (v / 100.0).ToString("0.00", CultureInfo.InvariantCulture);
                    }
                }
                catch
                {
                }
            }
        }
        private void StartLabelPolling()
        {
            if (_labelPolling) return;
            _labelPolling = true;
            _ctsLabelPoll = new CancellationTokenSource();
            var tk = _ctsLabelPoll.Token;
            Task.Run(async () =>
            {
                try
                {
                    while (!tk.IsCancellationRequested)
                    {
                        try
                        {
                            PollLabelsInContainer(_activeContainer ?? GetActiveContainerOrForm());
                        }
                        catch {  }
                        await Task.Delay(300, tk); 
                    }
                }
                catch (TaskCanceledException) { }
                finally { _labelPolling = false; }
            }, tk);
        }
        private void StopLabelPolling()
        {
            if (_ctsLabelPoll == null) return;
            try { _ctsLabelPoll.Cancel(); } catch { }
            try { _ctsLabelPoll.Dispose(); } catch { }
            _ctsLabelPoll = null;
            _labelPolling = false;
        }
        private void PollLabelsInContainer(System.Windows.Forms.Control container)
        {
            if (container == null) return;
            foreach (System.Windows.Forms.Control control in GetAllControls(container))
            {
                var label = control as System.Windows.Forms.Label;
                if (label == null) continue;
                var addr = label.Tag as string;
                if (string.IsNullOrWhiteSpace(addr)|| label.Name.IndexOf("ModelSaveAs", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                try
                {
                    bool isId = (label.Name ?? "").IndexOf("Id", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (isId)
                    {
                        short v16 = _controller.ReadInt16(addr);
                        short last;
                        if (_labelLastRaw16.TryGetValue(label, out last) && last == v16) continue;
                        _labelLastRaw16[label] = v16;
                        var text = v16.ToString(CultureInfo.InvariantCulture);
                        label.BeginInvoke(new Action(() => label.Text = text));
                    }
                    else
                    {
                        int v32 = _controller.ReadInt32(addr);
                        int last;
                        if (_labelLastRaw.TryGetValue(label, out last) && last == v32) continue;
                        _labelLastRaw[label] = v32;
                        double s = v32 / 100.0;
                        var text = s.ToString("0.00", CultureInfo.InvariantCulture);
                        label.BeginInvoke(new Action(() => label.Text = text));
                    }
                }
                catch
                {
                }
            }
        }
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            _activeContainer = GetActiveContainerOrForm();
            ReadTextBoxesInContainer(_activeContainer);
        }
        private System.Windows.Forms.Control GetActiveContainerOrForm()
        {
            if (TabControl != null && TabControl.SelectedTab != null) return TabControl.SelectedTab;
            return this;
        }
        private static bool ContainsAny(string input, params string[] keys)
        {
            if (string.IsNullOrEmpty(input)) return false;
            for (int i = 0; i < keys.Length; i++)
                if (input.IndexOf(keys[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }
        private async void Clicks(object sender, EventArgs e)
        {
            var btn = (Guna2GradientButton)sender;
            var tagStr = btn.Tag as string;
            if (string.IsNullOrEmpty(tagStr) || tagStr.IndexOf('.') < 0)
                return; 
            var parts = tagStr.Split('.');
            var reg = parts[0];
            int bit;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out bit) || bit < 0 || bit > 15)
            {
                MessageBox.Show("Bit index không hợp lệ.");
                return;
            }
            var name = btn.Name ?? string.Empty;
            bool skipConfirm = name.IndexOf("Org", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("Rq", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("SWs", StringComparison.OrdinalIgnoreCase) >= 0;
            bool RqSaveModel = name.IndexOf("RqSaveModel", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!skipConfirm||RqSaveModel==true)
            {
                if (MessageBox.Show("Do you confirm ?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
            }
            bool isOn;
            try { isOn = _controller.ReadBitFromWord(reg, bit); }
            catch { MessageBox.Show("Không đọc được trạng thái PLC."); return; }
            int maxRetry = 25;
            int tries = 0;
            if (_ctsAction != null) { try { _ctsAction.Cancel(); } catch { } try { _ctsAction.Dispose(); } catch { } }
            _ctsAction = new CancellationTokenSource();
            var token = _ctsAction.Token;
            if (!isOn)
            {
                while (tries++ < maxRetry && !token.IsCancellationRequested)
                {
                    if (_controller.SetBitInWord(reg, bit))
                    {
                        if (_controller.ReadBitFromWord(reg, bit)) break;
                    }
                    await Task.Delay(200);
                }
                BeginInvoke(new MethodInvoker(delegate { btn.FillColor = System.Drawing.Color.Green; }));
                if (tries >= maxRetry) MessageBox.Show("SET bit timeout – kiểm tra kết nối/địa chỉ.");
            }
            //else
            //{
            //    tries = 0;
            //    while (tries++ < maxRetry && !token.IsCancellationRequested)
            //    {
            //        if (_controller.ResetBitInWord(reg, bit))
            //        {
            //            if (_controller.ReadBitFromWord(reg, bit) == false) break;
            //        }
            //        await Task.Delay(200);
            //    }
            //    BeginInvoke(new MethodInvoker(delegate { btn.FillColor = System.Drawing.Color.Silver; }));
            //    if (tries >= maxRetry) MessageBox.Show("RESET bit timeout – kiểm tra kết nối/địa chỉ.");
            //}
            await Task.Delay(200);
            BeginInvoke(new MethodInvoker(delegate { btn.FillColor = System.Drawing.Color.Silver; }));
             ReadBackGroup(DataAll);
        }
        private async void Clicks2(object sender, EventArgs e)
        {
            var btn = (Guna2GradientButton)sender;
            var tagStr = btn.Tag as string;
            if (string.IsNullOrEmpty(tagStr) || tagStr.IndexOf('.') < 0)
                return; 
            var parts = tagStr.Split('.');
            var reg = parts[0];
            int bit;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out bit) || bit < 0 || bit > 15)
            {
                MessageBox.Show("Bit index không hợp lệ.");
                return;
            }
            var name = btn.Name ?? string.Empty;
            bool skipConfirm = name.IndexOf("Org", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("Rq", StringComparison.OrdinalIgnoreCase) >= 0
                            || name.IndexOf("SWs", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!skipConfirm)
            {
                if (MessageBox.Show("Do you confirm ?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
            }
            bool isOn;
            try { isOn = _controller.ReadBitFromWord(reg, bit); }
            catch { MessageBox.Show("Không đọc được trạng thái PLC."); return; }
            int maxRetry = 25;
            int tries = 0;
            if (_ctsAction != null) { try { _ctsAction.Cancel(); } catch { } try { _ctsAction.Dispose(); } catch { } }
            _ctsAction = new CancellationTokenSource();
            var token = _ctsAction.Token;
            if (!isOn)
            {
                while (tries++ < maxRetry && !token.IsCancellationRequested)
                {
                    if (_controller.SetBitInWord(reg, bit))
                    {
                        if (_controller.ReadBitFromWord(reg, bit)) break;
                    }
                    await Task.Delay(200);
                }
                BeginInvoke(new MethodInvoker(delegate { btn.FillColor = System.Drawing.Color.Green; }));
                if (tries >= maxRetry) MessageBox.Show("SET bit timeout – kiểm tra kết nối/địa chỉ.");
            }
            else
            {
                tries = 0;
                while (tries++ < maxRetry && !token.IsCancellationRequested)
                {
                    if (_controller.ResetBitInWord(reg, bit))
                    {
                        if (_controller.ReadBitFromWord(reg, bit) == false) break;
                    }
                    await Task.Delay(200);
                }
                BeginInvoke(new MethodInvoker(delegate { btn.FillColor = System.Drawing.Color.Silver; }));
                if (tries >= maxRetry) MessageBox.Show("RESET bit timeout – kiểm tra kết nối/địa chỉ.");
            }
        }
        private void MouseDowns(object sender, EventArgs e)
        {
            var btn = (Guna2GradientButton)sender;
            var tagStr = btn.Tag as string;
            if (string.IsNullOrEmpty(tagStr) || tagStr.IndexOf('.') < 0) return;
            var parts = tagStr.Split('.');
            var reg = parts[0];
            int bit;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out bit)) return;
            Task.Run(delegate
            {
                try
                {
                    _controller.SetBitInWord(reg, bit);
                    btn.BeginInvoke(new MethodInvoker(delegate { btn.FillColor = System.Drawing.Color.Green; }));
                }
                catch { }
            });
        }
        private void MouseUps(object sender, EventArgs e)
        {
            var btn = (Guna2GradientButton)sender;
            var tagStr = btn.Tag as string;
            if (string.IsNullOrEmpty(tagStr) || tagStr.IndexOf('.') < 0) return;
            var parts = tagStr.Split('.');
            var reg = parts[0];
            int bit;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out bit)) return;
            Task.Run(delegate
            {
                try
                {
                    _controller.ResetBitInWord(reg, bit);
                    btn.BeginInvoke(new MethodInvoker(delegate { btn.FillColor = System.Drawing.Color.Silver; }));
                }
                catch { }
            });
        }
        private void RqCallModel_Click(object sender, EventArgs e)
        {
            var value = (IdJob.Text ?? string.Empty).Trim();
            var tag = IdJob.Tag as string;
            var tagRqCallModel = (sender as Guna2GradientButton).Tag.ToString();
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(tag))
            {
                MessageBox.Show("Please Id Model");
                return;
            }
            var s = value.Split('.');
            if (s.Length < 2)
            {
                MessageBox.Show("Please Enter Number with Character .");
                return;
            }
            try { _controller.ReadInt16(tag); }
            catch
            {
                MessageBox.Show("Không kết nối được PLC (probe thất bại).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Task.Run(delegate
            {
                try
                {
                    short v;
                    if (short.TryParse(s[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                    {
                        short rb; string err;
                        if (TryWriteInt16Verify(tag, v, out rb, out err))
                        {
                            MessageBox.Show("Ghi Id Model thành công (" + v + ").", "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            try {
                                _controller.SetBitInWord(tagRqCallModel.Split('.')[0], int.Parse(tagRqCallModel.Split('.')[1]));
                                ReadBackGroup(DataAll); } 
                            catch { }
                        }
                        else
                        {
                            MessageBox.Show("Ghi Id Model thất bại: " + (err ?? "verify-failed"), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Id Model không hợp lệ.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi ghi Id Model: " + ex.Message);
                }
            });
        }
        private void RqModelSaveAs_Click(object sender, EventArgs e)
        {
            var value = (ModelSaveAs.Text ?? string.Empty).Trim();
            var tag = ModelSaveAs.Tag as string;
            var RqSaveModelAs = (sender as Guna2GradientButton).Tag.ToString();
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(tag))
            {
                MessageBox.Show("Please Id Model need SaveAs ");
                return;
            }
            var s = value.Split('.');
            if (s.Length < 2)
            {
                MessageBox.Show("Please Enter Number with Character .");
                return;
            }
            try { _controller.ReadInt16(tag); }
            catch
            {
                MessageBox.Show("Không kết nối được PLC (probe thất bại).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Task.Run(delegate
            {
                try
                {
                    short v;
                    if (short.TryParse(s[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                    {
                        short rb; string err;
                        if (TryWriteInt16Verify(tag, v, out rb, out err))
                        {
                            MessageBox.Show("Save As Id Model thành công (" + v + ").", "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            try
                            {
                                ModelSaveAs.Text = _controller.ReadInt16(tag).ToString(CultureInfo.InvariantCulture);
                                _controller.SetBitInWord(RqSaveModelAs.Split('.')[0], int.Parse(RqSaveModelAs.Split('.')[1]));
                                ReadBackGroup(DataAll);
                            }
                            catch { }
                        }
                        else
                        {
                            MessageBox.Show("Save As Id Model thất bại: " + (err ?? "verify-failed"), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Save As Id Model không hợp lệ.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi ghi Id Model: " + ex.Message);
                }
            });
        }
        // ====== Groups ======
        private Task GroupDataAxisX()
        {
            DataAxisX = new List<Guna2TextBox> {
                TimeOutX, DisStep__JogX, SpeedLJogX, SpeedHJogX, PointABSX, SpABSX,
                OriginSpeedX, SpStartX, AccX, DecX, PosReadyX, SpReadyX
            };
            return Task.CompletedTask;
        }
        private Task GroupDataAxisY()
        {
            DataAxisY = new List<Guna2TextBox> {
                TimeOutY, DisStep__JogY, SpeedLJogY, SpeedHJogY, PointABSY, SpABSY,
                OriginSpeedY, SpStartY, AccY, DecY, PosReadyY, SpReadyY
            };
            return Task.CompletedTask;
        }
        private Task GroupDataAxisZ()
        {
            DataAxisZ = new List<Guna2TextBox> {
                TimeOutZ, DisStep__JogZ, SpeedLJogZ, SpeedHJogZ, PointABSZ, SpABSZ,
                OriginSpeedZ, SpStartZ, AccZ, DecZ, PosReadyZ, SpReadyZ
            };
            return Task.CompletedTask;
        }
        private Task GroupDataAxisF()
        {
            DataAxisF = new List<Guna2TextBox> {
                TimeOutF, DisStep__JogF, SpeedLJogF, SpeedHJogF,  PointABSF, SpABSF,
                OriginSpeedF, SpStartF, AccF, DecF, PosReadyF, SpReadyF
            };
            return Task.CompletedTask;
        }
        private Task GroupDataAxisRI()
        {
            DataAxisRI = new List<Guna2TextBox> {
                DisStep__JogRI, SpeedLJogRI, SpeedHJogRI, AccRI, DecRI
            };
            return Task.CompletedTask;
        }
        private Task GroupDataAxisRO()
        {
            DataAxisRO = new List<Guna2TextBox> {
                SpeedLJogRO, SpeedHJogRO,  SpStartRO, AccRO, DecRO
            };
            return Task.CompletedTask;
        }
        //private Task GroupDataLoad1()
        //{
        //    DataLoad = new List<Guna2TextBox> { /*nXOK_NGmax, nYOK_NGmax*/ };
        //    return Task.CompletedTask;
        //}
        //private Task GroupDataUnLoad1()
        //{
        //    DataUnload1 = new List<Guna2TextBox> {/* mYNG1, mYNG2, mYNG3 */};
        //    return Task.CompletedTask;
        //}
        //private Task GroupDataUnLoad2()
        //{
        //    DataUnload2 = new List<Guna2TextBox> { mYNG4max1_2 };
        //    return Task.CompletedTask;
        //}
        private Task GroupData()
        {
            Data = new List<Guna2TextBox> {
                SpLP1XY, SpLP1Z, PosrdP1X, PosrdP1Y, SprdP1XY, SprdZ, SpULP0XY, SpULP0Z,
                PosrdP0X, PosrdP0Y, SprdP0XY, SpRO, SpCP2XY, SpRunCRI, PosRdP2X, PosRdP2Y,
                SpRdP2XY, SpCP2Z, SpULP3XY, SpULP3Z, PosRdP3X, PosRdP3Y, SpRdP3XY,
                SpLP4XY, SpLP4Z, PosRdP4X, PosRdP4Y, SpRdP4XY, SpOpenF, SpCloseF,AngeRO,DisRO_RIX, DisRO_RIY,
                TimeOutTester,TimeOutData
            };
            return Task.CompletedTask;
        }
        private Task GroupDataCamera()
        {
            DataCamera = new List<Guna2TextBox> { SpCP2XY, SpRunCRI, PosRdP2X, PosRdP2Y, SpRdP2XY };
            return Task.CompletedTask;
        }
        private Task GroupDataNG1()
        {
            DataNG1 = new List<Guna2TextBox> {
                RsNG101, RsNG102, RsNG103, RsNG104, RsNG105, RsNG106, RsNG107, RsNG108, RsNG109, RsNG110,
                RsNG111, RsNG112, RsNG113, RsNG114, RsNG115, RsNG116, RsNG117, RsNG118, RsNG119, RsNG120,
                RsNG201, RsNG202, RsNG203, RsNG204, RsNG205, RsNG206, RsNG207, RsNG208, RsNG209, RsNG210,
                RsNG211, RsNG212, RsNG213, RsNG214, RsNG215, RsNG216, RsNG217, RsNG218, RsNG219, RsNG220,
                RsNG301, RsNG302, RsNG303, RsNG304, RsNG305, RsNG306, RsNG307, RsNG308, RsNG309, RsNG310,
                RsNG311, RsNG312, RsNG313, RsNG314, RsNG315, RsNG316, RsNG317, RsNG318, RsNG319, RsNG320,
                RsNG401, RsNG402, RsNG403, RsNG404, RsNG405, RsNG406, RsNG407, RsNG408, RsNG409, RsNG410,
                RsNG411, RsNG412, RsNG413, RsNG414, RsNG415, RsNG416, RsNG417, RsNG418, RsNG419, RsNG420,
                nXOK_NGmax, nYOK_NGmax,
                mYNG1, mYNG2, mYNG3,
                mYNG4max1_2,
                NumberReCheck01,NumberReCheck02,NumberReCheck03,NumberReCheck04,NumberReCheck05,NumberReCheck06,
                NumberReCheck07,NumberReCheck08,NumberReCheck09,NumberReCheck10,
                ReCheck01,ReCheck02,ReCheck03,ReCheck04,ReCheck05,ReCheck06,ReCheck07,ReCheck08,ReCheck09,ReCheck10
            };
            return Task.CompletedTask;
        }
        //private Task GroupDataNG2()
        //{
        //    DataNG2 = new List<Guna2TextBox> {
        //    };
        //    return Task.CompletedTask;
        //}
        //private Task GroupDataNG3()
        //{
        //    DataNG3 = new List<Guna2TextBox> {
        //    };
        //    return Task.CompletedTask;
        //}
        //private Task GroupDataNG4()
        //{
        //    DataNG4 = new List<Guna2TextBox> {
        //    };
        //    return Task.CompletedTask;
        //}
        private Task GroupDataAll()
        {
            DataAll = new List<Guna2TextBox> {
                PosrdZ00, PosrdZI1O0, PosrdZI0O1, PosULP0Z, PosLP1Z, PosP2Z, PosULP3Z, PosLP4Z,
                PosCloseF2, PosOpenF2,  PosCX, PosCY, PosStartOKX, PosStartOKY, PosEndXOKX, PosEndXOKY,
                PosEndYOKX, PosEndYOKY, PosStartNGX, PosStartNGY, PosEndXNGX, PosEndXNGY, PosEndYNGX, PosEndYNGY,
                PosStartNG4X, PosStartNG4Y, PosEndXNG4X, PosEndXNG4Y, PosEndYNG4X1_2,PosSKRIY,
                PosSKRIX, PosSKRIX,DisStep__JogZZ,DisStep__JogYY,DisStep__JogXX,PosEndYNG4Y1_2
            };
            return Task.CompletedTask;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        private void DebugDumpButtonHandlerCounts()
        {
            foreach (var c in GetAllControls(this))
            {
                var btn = c as Guna2GradientButton;
                if (btn != null)
                {
                    int clickCount = GetHandlerCount(btn, "Click");
                    int mdCount = GetHandlerCount(btn, "MouseDown");
                    int muCount = GetHandlerCount(btn, "MouseUp");
                    System.Diagnostics.Debug.WriteLine(
                        string.Format("[HANDLERS] {0}: Click={1}, MD={2}, MU={3}", btn.Name, clickCount, mdCount, muCount));
                }
            }
        }
        private static int GetHandlerCount(System.Windows.Forms.Control c, string eventName)
        {
            if (c == null) return 0;
            var eventsProp = typeof(Component).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            var list = eventsProp != null ? (EventHandlerList)eventsProp.GetValue(c, null) : null;
            if (list == null) return 0;
            var keyField = typeof(System.Windows.Forms.Control).GetField("Event" + eventName, BindingFlags.Static | BindingFlags.NonPublic);
            if (keyField == null) return 0;
            var key = keyField.GetValue(null);
            var del = list[key] as Delegate;
            return del != null ? del.GetInvocationList().Length : 0;
        }
        private bool TryWriteInt16Verify(string tag, short value, out short readBack, out string err)
        {
            readBack = 0; err = null;
            try
            {
                _controller.WriteInt16(tag, value);
                readBack = _controller.ReadInt16(tag);
                if (readBack != value) { err = "verify-failed"; return false; }
                return true;
            }
            catch (Exception ex) { err = ex.Message; return false; }
        }
        private bool TryWriteInt32Verify(string tag, int value, out int readBack, out string err)
        {
            readBack = 0; err = null;
            try
            {
                _controller.WriteInt32(tag, value);
                readBack = _controller.ReadInt32(tag);
                if (readBack != value) { err = "verify-failed"; return false; }
                return true;
            }
            catch (Exception ex) { err = ex.Message; return false; }
        }
        // ====== Send handlers ======
        private async void SendDataAxisX(object sender, EventArgs e) { await SendDataForGroup(DataAxisX); }
        private async void SendDataAxisY(object sender, EventArgs e) { await SendDataForGroup(DataAxisY); }
        private async void SendDataAxisZ(object sender, EventArgs e) { await SendDataForGroup(DataAxisZ); }
        private async void SendDataAxisF(object sender, EventArgs e) { await SendDataForGroup(DataAxisF); }
        private void StartCalib_Click(object sender, EventArgs e)
        {
            _cam.StartCalib();
            lblStatusCHE.Text = "Calib Hand Eye Camera Step : " +_cam.CalibStep.ToString();
        }
        private void EndCalib_Click(object sender, EventArgs e)
        {
            _cam.EndCalib();
            lblStatusCHE.Text = "Calib Hand Eye Camera";
        }
        private void Move_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you confirm sending Data", "Warning",
               MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
            HYCAxisX.Text = CurPosXXX.Text;
            HYCAxisY.Text = CurPosYYY.Text;
            HYCAxisZ.Text = CurPosZZZ.Text;
            HYCAxisRI.Text = CurPosRIII.Text;
            if (_cam.CalibStep < 10) { 
            _cam.StepCalib(float.Parse(HYCAxisX.Text), float.Parse(HYCAxisY.Text), float.Parse(HYCAxisZ.Text),0,0,0);
            }
            else
            {
                _cam.StepCalib(float.Parse(HYCAxisX.Text), float.Parse(HYCAxisY.Text), float.Parse(HYCAxisZ.Text), float.Parse(HYCAxisRI.Text), 0, 0);
            }
            lblStatusCHE.Text = "Calib Hand Eye Camera Step : " + _cam.CalibStep.ToString();
        }
        private void SetGoldenPose_Click(object sender, EventArgs e)
        {
            if(_NameStation=="PORT1"|| _NameStation == "PORT3")
            {
                _cam.SGP(2);
            }
            else if (_NameStation == "PORT2" || _NameStation == "PORT3")
            {
                _cam.SGP(1);
            }
        }
        private void IdJob_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(IdJob.Text.ToString().Split('.')[0], out int rs))
            {
                lblCurrentModel.Text = DatabaseControllers.Instance.NameModel(this.Nametation, rs);
            }
        }
        private void lblCurrentModel_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (lblCurrentModel.Text == "")
                {
                    MessageBox.Show("Model,Id Job Hiện Tại Chưa Tồn Tại Trong Cơ Sở Dữ Liêu . Để Tránh Xung Đột Dữ Liệu Hãy Tạo Tên Model và Id Model Ở FormMain , mục Device => Folder Port", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    cbxModelSaveAs.Items.Clear();
                    ModelSaveAs.Text = "";
                    cbxModelSaveAs.SelectedIndex = -1;
                    DatabaseControllers.Instance.LoadDataNameModel(cbxModelSaveAs, this.Nametation);
                   cbxModelSaveAs.Items.Remove(lblCurrentModel.Text.ToString());
                }
            }
            catch { }
        }
        private void SpRdP3XY_TextChanged(object sender, EventArgs e)
        {
        }
        private void tableLayoutPanel43_Paint(object sender, PaintEventArgs e)
        {
        }
        private void SWsStep22_Click(object sender, EventArgs e)
        {
        }
        private void cbxModelSaveAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(DatabaseControllers.Instance.IdModel(this.Nametation, cbxModelSaveAs.Text.ToString()), out int rs))
            {
                ModelSaveAs.Text = rs + ".0";
            }
            else
            {
                ModelSaveAs.Text = "";
            }
        }
        private async void SendDataAxisRI(object sender, EventArgs e) { await SendDataForGroup(DataAxisRI); }
        private async void SendDataAxisRO(object sender, EventArgs e) { await SendDataForGroup(DataAxisRO); }
        private async void SendDataLoad1(object sender, EventArgs e) { await SendDataForGroup(DataLoad);
            _controller.SetBitInWord(_RqSaveModel.Split('.')[0], int.Parse(_RqSaveModel.Split('.')[1]));
        }
        private async void SendDataUnload1(object sender, EventArgs e) { await SendDataForGroup(DataUnload1);
            _controller.SetBitInWord(_RqSaveModel.Split('.')[0], int.Parse(_RqSaveModel.Split('.')[1]));
        }
        private async void SendDataUnLoad2(object sender, EventArgs e) { await SendDataForGroup(DataUnload2);
            _controller.SetBitInWord(_RqSaveModel.Split('.')[0], int.Parse(_RqSaveModel.Split('.')[1]));
        }
        //private async void SendDataCamera(object sender, EventArgs e) { await SendDataForGroup(DataCamera);
        //    _controller.SetBitInWord(_RqCallModel.Split('.')[0], int.Parse(_RqCallModel.Split('.')[1]));
        //}
        private async void SendData(object sender, EventArgs e) { await SendDataForGroup(Data);
            _controller.SetBitInWord(_RqSaveModel.Split('.')[0], int.Parse(_RqSaveModel.Split('.')[1]));
        }
        private async void SendDataNG1(object sender, EventArgs e) { await SendDataForGroup(DataNG1);
            _controller.SetBitInWord(_RqSaveModel.Split('.')[0], int.Parse(_RqSaveModel.Split('.')[1]));
        }
        private async void SendDataNG2(object sender, EventArgs e) { await SendDataForGroup(DataNG2);
            _controller.SetBitInWord(_RqSaveModel.Split('.')[0], int.Parse(_RqSaveModel.Split('.')[1]));
        }
        private async void SendDataNG3(object sender, EventArgs e) { await SendDataForGroup(DataNG3);
            _controller.SetBitInWord(_RqSaveModel.Split('.')[0], int.Parse(_RqSaveModel.Split('.')[1]));
        }
        private async void SendDataNG4(object sender, EventArgs e) { await SendDataForGroup(DataNG4);
            _controller.SetBitInWord(_RqCallModel.Split('.')[0], int.Parse(_RqCallModel.Split('.')[1]));
        }
        private async void SendAll_Click(object sender, EventArgs e) { await SendDataForGroup(DataAll); }
        private async Task SendDataForGroup(List<Guna2TextBox> group)
        {
            if (group == null || group.Count == 0) return;
            if (MessageBox.Show("Do you confirm sending Data", "Warning",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
            // Probe kết nối
            string probeTag = null;
            foreach (var t in group)
            {
                if (t != null && t.Tag is string && !string.IsNullOrWhiteSpace((string)t.Tag))
                { probeTag = (string)t.Tag; break; }
            }
            if (_controller == null)
            {
                MessageBox.Show("PLC chưa sẵn sàng (null).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                if (!string.IsNullOrEmpty(probeTag)) _controller.ReadInt16(probeTag);
                else _controller.ReadInt16("DM0");
            }
            catch
            {
                MessageBox.Show("Không kết nối được PLC (probe thất bại).", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int total = 0;
            int ok = 0;
            var failed = new List<string>();
            try
            {
                foreach (var item in group)
                {
                    if (item == null) continue;
                    total++;
                    string name = item.Name ?? string.Empty;
                    string txt = (item.Text ?? "").Trim();
                    var tag = item.Tag as string;
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        failed.Add(name + " (tag trống)");
                        continue;
                    }
                    if (string.IsNullOrEmpty(txt))
                    {
                        short rb16; string err;
                        if (TryWriteInt16Verify(tag, 0, out rb16, out err)) ok++;
                        else failed.Add(name + " (" + (err ?? "write-failed") + ")");
                        continue;
                    }
                    var s = txt.Split('.');
                    if (s.Length < 2)
                    {
                        failed.Add(name + " (định dạng thiếu dấu '.')");
                        continue;
                    }
                    if (ContainsAny(name, "mY", "nY", "nX", "RsNG", "Acc", "Dec", "NumberReCheck", "ReCheck"))
                    {
                        short v16;
                        if (!short.TryParse(s[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out v16))
                        { failed.Add(name + " (giá trị không hợp lệ)"); continue; }
                        short rb16; string err;
                        if (TryWriteInt16Verify(tag, v16, out rb16, out err)) ok++;
                        else failed.Add(name + " (" + (err ?? "verify-failed") + ")");
                    }
                    else if (name.IndexOf("SpStart", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string scaled16 = s[0].Trim();
                        short v16s;
                        if (!short.TryParse(scaled16, NumberStyles.Integer, CultureInfo.InvariantCulture, out v16s))
                        { failed.Add(name + " (scale invalid)"); continue; }
                        short rb16; string err;
                        if (TryWriteInt16Verify(tag, v16s, out rb16, out err)) ok++;
                        else failed.Add(name + " (" + (err ?? "verify-failed") + ")");
                    }
                    else
                    {
                        string scaled32 = s[0].Trim() + s[1].Trim();
                        int v32;
                        if (!int.TryParse(scaled32, NumberStyles.Integer, CultureInfo.InvariantCulture, out v32))
                        { failed.Add(name + " (scale invalid)"); continue; }
                        int rb32; string err;
                        if (TryWriteInt32Verify(tag, v32, out rb32, out err)) ok++;
                        else failed.Add(name + " (" + (err ?? "verify-failed") + ")");
                    }
                }
                if (failed.Count == 0)
                {
                    MessageBox.Show("Ghi PLC thành công: " + ok + "/" + total + " ô.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var detail = string.Join("\r\n - ", failed);
                    MessageBox.Show(
                        "Ghi PLC: thành công " + ok + "/" + total + " ô.\r\nCác ô lỗi:\r\n - " + detail,
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                }
            }
            finally
            {
                if (ok > 0) ReadBackGroup(group);
            }
            await Task.CompletedTask;
        }
        // ====== Helpers ======
        public IEnumerable<System.Windows.Forms.Control> GetAllControls(System.Windows.Forms.Control parent)
        {
            foreach (System.Windows.Forms.Control ctrl in parent.Controls)
            {
                yield return ctrl;
                foreach (System.Windows.Forms.Control child in GetAllControls(ctrl))
                    yield return child;
            }
        }
    }
}
//if(System.Windows.Forms.Control.Name.IndexOf("JOG_X", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG_XX.Tag = tag;
//}
//else if(System.Windows.Forms.Control.Name.IndexOf("JOG__X", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG__XX.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("JOG_Y", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG_YY.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("JOG__Y", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG__YY.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("JOG_Z", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG_ZZ.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("JOG__Z", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG__ZZ.Tag = tag;
//}
//if (System.Windows.Forms.Control.Name.IndexOf("JOG_F", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG_FF.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("JOG__F", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG__FF.Tag = tag;
//}
//if (System.Windows.Forms.Control.Name.IndexOf("JOG_RI", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG_RII.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("JOG__RI", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG__RII.Tag = tag;
//}
//if (System.Windows.Forms.Control.Name.IndexOf("JOG_RO", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG_ROO.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("JOG__RO", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    JOG__ROO.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("SWsStep2", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    SWsStep22.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("SWsHSp2", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    SWsHSp22.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("CurPosX", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    CurPosXX.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("CurPosY", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    CurPosYY.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("CurPosZ", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    CurPosZZ.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("CurPosF", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    CurPosFF.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("CurPosRI", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    CurPosRII.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("CurPosRO", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    CurPosROO.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("DisStep__JogX", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    DisStep__JogXX.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("DisStep__JogY", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    DisStep__JogYY.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("DisStep__JogZ", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    DisStep__JogZZ.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("DisStep__JogF", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    DisStep__JogFF.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("DisStep__JogRI", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    DisStep__JogRII.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("DisStep__JogRO", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    DisStep__JogROO.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("ORGX", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    ORGXX.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("ORGY", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    ORGYY.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("ORGF", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    ORGFF.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("ORGRI", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    ORGRII.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("ORGRO", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    ORGROO.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("ORGZ", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    ORGZZ.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("RESETX", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    RESETXX.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("RESETY", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    RESETYY.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("RESETZ", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    RESETZZ.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("RESETF", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    RESETFF.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("RESETRI", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    RESETRII.Tag = tag;
//}
//else if (System.Windows.Forms.Control.Name.IndexOf("RESETRO", StringComparison.OrdinalIgnoreCase) >= 0)
//{
//    RESETROO.Tag = tag;
//}
