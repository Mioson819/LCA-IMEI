using Bottom_Sorting.Services.Controllers;
using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Utilities;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.UI;
using System.Windows.Forms;
namespace LCA_Project.Form.Teaching
{
    public partial class frmTeaching2 : System.Windows.Forms.Form
    {
        private System.Timers.Timer timer;
        private List<System.Windows.Forms.Control> _control;
        private string _namePort { get; set; }
        private KeyenceHostLinkTcpClient _plc { get; set; }
        private Dictionary<Guna.UI2.WinForms.Guna2TextBox, PLCRegisterDataControl> _txtControl = new Dictionary<Guna.UI2.WinForms.Guna2TextBox, PLCRegisterDataControl>();
        private Dictionary<Guna.UI2.WinForms.Guna2GradientButton, PLCBitControl> _btnControl = new Dictionary<Guna.UI2.WinForms.Guna2GradientButton, PLCBitControl>();
        List<PLCRadioControl> _radioBindings = new List<PLCRadioControl>();
        private PLCLabelControl _lblControl;
        private static Type type { get; set; }
        private DataforTagControl dataforController { get; set; }
        public frmTeaching2(string namePort, KeyenceHostLinkTcpClient plc)
        {
            InitializeComponent();
            _namePort = namePort;
            _plc = plc;
            lblNamePort.Text = lblNamePort.Text + "  " + NamePort(namePort);
        }
        #region Init
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _control = new List<System.Windows.Forms.Control>()
            {
                txtSavedPosX,
                txtSavedPosY,
                txtSavedPosZ,
                txtSavedPosRI,
                txtSavedPosRO,
                txtSavedPosF
            };
            _lblControl = new PLCLabelControl(_plc, 200);
            BindTagsOnce();
            btnUnloadSocket.CheckedChanged += CheckedChanged;
            btnLoadTrayOK1.CheckedChanged += CheckedChanged;
            btnLoadTrayOK2.CheckedChanged += CheckedChanged;
            btnLoadTrayOK3.CheckedChanged += CheckedChanged;
            btnUnLoadTrayNG1.CheckedChanged += CheckedChanged;
            btnUnLoadTrayNG2.CheckedChanged += CheckedChanged;
            btnUnLoadTrayNG3.CheckedChanged += CheckedChanged;
            btnUnLoadTrayNG41.CheckedChanged += CheckedChanged;
            btnUnLoadTrayNG42.CheckedChanged += CheckedChanged;
            btnUnLoadTrayNG43.CheckedChanged += CheckedChanged;
            btnVisionPosition.CheckedChanged += CheckedChanged;
            btnLoadSocketRIX.CheckedChanged += CheckedChanged;
            //btnLoadSocketRIY.CheckedChanged += CheckedChanged;
            btnUnLoadSocketZ.CheckedChanged += CheckedChanged;
            btnLoadSocketZ.CheckedChanged += CheckedChanged;
            btnLoadSocketF.CheckedChanged += CheckedChanged;
            btnUnLoadSocketF.CheckedChanged += CheckedChanged;
            btnLoadTrayOKZ.CheckedChanged += CheckedChanged;
            btnUnLoadTrayNGZ.CheckedChanged += CheckedChanged;
            btnVisionPositionZ.CheckedChanged += CheckedChanged;
            btnRunReady.CheckedChanged += CheckedChanged;
            btnRunReadyIn.CheckedChanged += CheckedChanged;
            btnRunReadyOut.CheckedChanged += CheckedChanged;
            Hight.CheckedChanged -= CheckedChanged2;
            Low.CheckedChanged -= CheckedChanged2;
            Mid.CheckedChanged -= CheckedChanged2;
            Micro.CheckedChanged -= CheckedChanged2;
            Hight.CheckedChanged += CheckedChanged2;
            Low.CheckedChanged += CheckedChanged2;
            Mid.CheckedChanged += CheckedChanged2;
            Micro.CheckedChanged += CheckedChanged2;
            Init();
            btnLoadTrayOK1.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosX,"PosStartOKX"},
                {txtSavedPosY,"PosStartOKY"},
                {btnSaveCurrentPos,"LoadPointStart"},
                {btnMoveCurrentPos,"RunPointStart"}
            };
            btnLoadTrayOK2.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosX,"PosEndXOKX"},
                {txtSavedPosY,"PosEndXOKY"},
                {btnSaveCurrentPos,"LoadPointEndX"},
                {btnMoveCurrentPos,"RunPointEndX"},
            };
            btnLoadTrayOK3.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosX,"PosEndYOKX"},
                {txtSavedPosY,"PosEndYOKY"},
                {btnSaveCurrentPos,"LoadPointEndY"},
                {btnMoveCurrentPos,"RunPointEndY"},
            };
            btnUnLoadTrayNG1.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosX,"PosStartNGX"},
                { txtSavedPosY,"PosStartNGY"},
                {btnSaveCurrentPos,"LoadPointStartNG"},
                {btnMoveCurrentPos,"RunPointStartNG"},
            };
            btnUnLoadTrayNG2.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosX,"PosEndXNGX"},
                { txtSavedPosY,"PosEndXNGY"},
                {btnSaveCurrentPos,"LoadPointEndXNG"},
                {btnMoveCurrentPos,"RunPointEndXNG"},
            };
            btnUnLoadTrayNG3.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosX,"PosEndYNGX"},
                { txtSavedPosY,"PosEndYNGY"},
                {btnSaveCurrentPos,"LoadPointEndYNG"},
                {btnMoveCurrentPos,"RunPointEndYNG"},
            };
            btnUnLoadTrayNG41.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosX,"PosStartNG4X"},
                { txtSavedPosY,"PosStartNG4Y"},
                {btnSaveCurrentPos,"LoadPointStartNG4"},
                {btnMoveCurrentPos,"RunPointStartNG4"},
            };
            btnUnLoadTrayNG42.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosX,"PosEndXNG4X"},
                { txtSavedPosY,"PosEndXNG4Y"},
                {btnSaveCurrentPos,"LoadPointEndXNG4"},
                {btnMoveCurrentPos,"RunPointEndXNG4"},
            };
            btnUnLoadTrayNG43.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosX,"PosEndYNG4X1_2"},
                { txtSavedPosY,"PosEndYNG4Y1_2"},
                {btnSaveCurrentPos,"LoadPointEndYNG4"},
                {btnMoveCurrentPos,"RunPointEndYNG4"},
            };
            btnVisionPosition.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosX,"PosCX"},
                { txtSavedPosY,"PosCY"},
                {btnSaveCurrentPos,"LoadPosCam"},
                {btnMoveCurrentPos,"RunPosCam"},
            };
            btnLoadSocketRIX.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosX,"PosSKRIX"},
                {txtSavedPosY,"PosSKRIY"},
                {btnSaveCurrentPos,"LoadPointSK"},
                {btnMoveCurrentPos,"RunPointSK"},
            };
            //btnLoadSocketRIY.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            //{
            //    { txtSavedPosRI,"PosSKRIY"},
            //    {btnSaveCurrentPos,"LoadPointSK"},
            //    {btnMoveCurrentPos,"RunPointSK"},
            //};
            btnUnLoadSocketZ.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosZ,"PosULP3Z"},
                {btnSaveCurrentPos,"LoadULP3Z"},
                {btnMoveCurrentPos,"RunULP3Z"},
            };
            btnLoadSocketZ.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosZ,"PosLP4Z"},
                {btnSaveCurrentPos,"LoadLP4Z"},
                {btnMoveCurrentPos,"RunLP4Z"},
            };
            btnLoadSocketF.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosF,"PosOpenF2"},
                {btnSaveCurrentPos,"LoadOpenF2"},
                {btnMoveCurrentPos,"RunOpenF2"}
            };
            btnUnLoadSocketF.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosF,"PosCloseF2"},
                {btnSaveCurrentPos,"LoadCloseF2"},
                {btnMoveCurrentPos,"RunCloseF2"}
            };
            btnLoadTrayOKZ.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                {txtSavedPosZ,"PosLP1Z"},
                {btnSaveCurrentPos,"LoadLP1Z"},
                {btnMoveCurrentPos,"RunLP1Z"}
            };
            btnUnLoadTrayNGZ.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosZ,"PosULP0Z"},
                {btnSaveCurrentPos,"LoadULP0Z"},
                {btnMoveCurrentPos,"RunULP0Z"}
            };
            btnVisionPositionZ.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosZ,"PosP2Z"},
                {btnSaveCurrentPos,"LoadCP2Z"},
                {btnMoveCurrentPos,"RunCP2Z"}
            };
            btnLoadSocketZ.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosZ,"PosLP4Z"},
                {btnSaveCurrentPos,"LoadLP4Z"},
                {btnMoveCurrentPos,"RunLP4Z"}
            };
            btnUnLoadSocketZ.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosZ,"PosULP3Z"},
                {btnSaveCurrentPos,"LoadULP3Z"},
                {btnMoveCurrentPos,"RunULP3Z"}
            };
            btnRunReady.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosZ,"PosrdZ00"},
                {btnSaveCurrentPos,"LoadReadyZ00"},
                {btnMoveCurrentPos,"RunReadyZ00"}
            };
            btnRunReadyIn.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosZ,"PosrdZI1O0"},
                {btnSaveCurrentPos,"LoadReadyZ10"},
                {btnMoveCurrentPos,"RunReadyZ10"}
            };
            btnRunReadyOut.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosZ,"PosrdZI0O1"},
                {btnSaveCurrentPos,"LoadReadyZ01"},
                {btnMoveCurrentPos,"RunReadyZ01"}
            };
            btnUnloadSocket.Tag = new Dictionary<System.Windows.Forms.Control, string>()
            {
                { txtSavedPosX,"UnloadSocketX"},
                { txtSavedPosY,"UnloadSocketY"},
                {btnSaveCurrentPos,"LoadUnloadSocket"},
                {btnMoveCurrentPos,"RunUnloadSocket"},
            };
            //RqOpenSK.CheckedChanged -= CheckedChanged2;
            //RqCloseSK.CheckedChanged -= CheckedChanged2;
            //RqOpenSK.CheckedChanged += CheckedChanged2;
            //RqCloseSK.CheckedChanged += CheckedChanged2;
        }
        #endregion
        private void LoadTags(object sender, EventArgs e)
        {
            Refresh();
            Guna.UI2.WinForms.Guna2GradientButton btn = sender as Guna.UI2.WinForms.Guna2GradientButton;
            foreach (var item in btn.Tag as Dictionary<System.Windows.Forms.Control, String>)
            {
                var prop = type.GetProperty((item.Value ?? string.Empty).Trim(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || dataforController == null) continue;
                var tagObj = prop.GetValue(dataforController);
                var tag = tagObj != null ? tagObj.ToString().Trim() : null;
                if (!string.IsNullOrEmpty(tag))
                {
                    item.Key.Tag = tag;
                    if (item.Key is Guna.UI2.WinForms.Guna2TextBox txt)
                    {
                        if (_txtControl.TryGetValue(txt, out var value))
                        {
                            value.Address = tag;
                            txt.Text = value.Refresh();
                            _control.Remove(txt);
                        }
                        ;
                    }
                    else if (item.Key is Guna.UI2.WinForms.Guna2GradientButton button)
                    {
                        if (_btnControl.TryGetValue(button, out var value))
                        {
                            value.WordAddress = tag.Split('.')[0];
                            value.BitIndex = int.Parse(tag.Split('.')[1]);
                        }
                        ;
                    }
                }
            }
            foreach (var controls in _control)
            {
                controls.Enabled = false;
                controls.Text = "";
            }
        }
        private void Refresh()
        {
            _control = new List<System.Windows.Forms.Control>()
            {
                txtSavedPosX,
                txtSavedPosY,
                txtSavedPosZ,
                txtSavedPosRI,
                txtSavedPosRO,
                txtSavedPosF
            };
            foreach (var controls in _control)
            {
                controls.Enabled = true;
            }
        }
        private void BindTagsOnce()
        {
            dataforController = DatabaseControllers.Instance.GetDataByKey(_namePort);
            type = typeof(DataforTagControl);
            foreach (System.Windows.Forms.Control control in GetAllControls(this))
            {
                var prop = type.GetProperty((control.Name ?? string.Empty).Trim(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || dataforController == null) continue;
                var tagObj = prop.GetValue(dataforController);
               // Console.WriteLine(control.Name+"----"+ $"{tagObj}");
                var tag = tagObj != null ? tagObj.ToString().Trim() : null;
                if (!string.IsNullOrEmpty(tag))
                {
                    control.Tag = tag;
                }
            }
        }
        private void Init()
        {
            foreach (System.Windows.Forms.Control ctrl in GetAllControls(this))
            {
                if (ctrl is Guna.UI2.WinForms.Guna2TextBox)
                {
                    Guna.UI2.WinForms.Guna2TextBox txt = ctrl as Guna.UI2.WinForms.Guna2TextBox;
                    if (txt.Tag?.ToString() == null || txt.Tag?.ToString() == string.Empty)
                    {
                        continue;
                    }
                    int dataType = DataTypeTextBox(txt.Name.ToString(), txt.Tag?.ToString());
                    if (dataType == 0)
                    {
                        continue;
                    }
                    PLCRegisterDataControl ctrTxt = new PLCRegisterDataControl(_plc, txt.Tag.ToString(), dataType);
                    txt.Text = ctrTxt.LoadOnce();
                    _txtControl.Add(txt, ctrTxt);
                    txt.KeyDown -= Textbox_KeyDown;
                    txt.KeyDown += Textbox_KeyDown;
                }
                else if (ctrl is Guna.UI2.WinForms.Guna2GradientButton)
                {
                    Guna.UI2.WinForms.Guna2GradientButton btn = ctrl as Guna.UI2.WinForms.Guna2GradientButton;
                    if (btn.Tag == null || btn.Tag?.ToString() == string.Empty)
                    {
                        continue;
                    }
                    var _dataType = DataTypeButton(btn.Name.ToString(), btn.Tag?.ToString());
                    if (_dataType == DataTypeEvent.None)
                    {
                        if (btn.ButtonMode == Guna.UI2.WinForms.Enums.ButtonMode.RadioButton)
                        {
                            if (_plc.ReadBitFromWord(btn.Tag?.ToString().Split('.')[0], int.Parse(btn.Tag?.ToString().Split('.')[1])))
                            {
                                btn.Checked = true;
                            }
                        }
                        continue;
                    }
                    PLCBitControl ctrButton = new PLCBitControl(_plc, btn.Tag?.ToString(), btn);
                    bool state = ctrButton.LoadOnce();
                    btn.FillColor = state ? System.Drawing.Color.Lime : Color.Silver;
                    _btnControl.Add(btn, ctrButton);
                    if (_dataType == DataTypeEvent.Click)
                    {
                        btn.Click -= Button_Click;
                        btn.Click += Button_Click;
                    }
                    else if (_dataType == DataTypeEvent.Mounse)
                    {
                        btn.MouseDown -= Button_MouseDown;
                        btn.MouseDown += Button_MouseDown;
                        btn.MouseUp -= Button_MouseUp;
                        btn.MouseUp += Button_MouseUp;
                    }
                    else if (_dataType == DataTypeEvent.ClickMessenger)
                    {
                        btn.Click -= Button_ClickMessengerbox;
                        btn.Click += Button_ClickMessengerbox;
                    }
                }
                else if (ctrl is System.Windows.Forms.Label)
                {
                    Label lbl = ctrl as Label;
                    if (lbl.Tag?.ToString() == null || lbl.Tag?.ToString() == string.Empty)
                    {
                        continue;
                    }
                    if (ContainsAny(lbl.Name, "ModelSaveAs"))
                    {
                        //_lblControl.Register(lbl, 1);
                    }
                    else
                    {
                        _lblControl.Register(lbl, 100);
                    }
                }
                else if (ctrl is RadioButton radio)
                {
                    if (radio.Tag == null || string.IsNullOrWhiteSpace(radio.Tag?.ToString()) || ContainsAny(radio.Name.ToString(), "rdoAxis"))
                    {
                        continue;
                    }
                    var vm = new PLCRadioControl(_plc, radio.Tag.ToString());
                    _radioBindings.Add(vm);
                    radio.DataBindings.Add(
                        "Checked",
                        vm,
                        nameof(vm.IsChecked),
                        true,
                        DataSourceUpdateMode.OnPropertyChanged
                    );
                    bool state = vm.LoadOnce();
                    radio.ForeColor = state ? System.Drawing.Color.Lime : System.Drawing.Color.Silver;
                }
            } 
            PLCBitControl.Start();
            // _lblControl.Start();
            timer = new System.Timers.Timer(3000);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
        }
        private  void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _lblControl.Start();
            timer.Elapsed -= Timer_Elapsed;
            timer.Stop();
        }
        #region Event
        private void Textbox_Changed(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2TextBox txt = sender as Guna.UI2.WinForms.Guna2TextBox;
            if (_txtControl.TryGetValue(txt, out var instance))
            {
                instance.Send(txt);
            }
        }
        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            Guna.UI2.WinForms.Guna2TextBox txt = sender as Guna.UI2.WinForms.Guna2TextBox;
            if (_txtControl.TryGetValue(txt, out var instance))
            {
                instance.OnEnter(txt);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        private void Textbox_KeyDown2(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            TextBox txt = sender as TextBox;
            if (MessageBox.Show("Do you Confirm?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (TryNormalize(txt.Text.ToString(), out int newPlcValue))
                {
                    _plc.WriteInt32(txt.Tag.ToString(), newPlcValue);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else
                {
                    MessageBox.Show("Invalid Input", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private bool TryNormalize(string input, out int plcValue)
        {
            plcValue = 0;
            if (!decimal.TryParse(input,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal uiValue))
                return false;
            plcValue = (int)Math.Round(uiValue * 100,
                MidpointRounding.AwayFromZero);
            return true;
        }
        private void Button_ClickMessengerbox(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2GradientButton btn = sender as Guna.UI2.WinForms.Guna2GradientButton;
            if (_btnControl.TryGetValue(btn, out var instance))
            {
                bool newState = instance.ToggleMessenger(out string error1);
                if (error1 != null)
                {
                    MessageBox.Show(error1, "PLC Error");
                    return;
                }
                else if (error1 == string.Empty)
                {
                    return;
                }
                // btn.FillColor = newState ? System.Drawing.Color.Lime : Color.Silver;
                // await Task.Delay(1000);
                //newState = instance.ToggleMessengerReset(out string error2);
                //if (error2 != string.Empty && error2 != null)
                //{
                //    MessageBox.Show(error2, "PLC Error");
                //    return;
                //}
                //btn.FillColor = newState ? System.Drawing.Color.Lime : Color.Silver;
                foreach (var item in _txtControl)
                {
                    if (item.Key.Enabled == true)
                        item.Key.Text = item.Value.Refresh();
                }
            }
        }
        private void Button_Click(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2GradientButton btn = sender as Guna.UI2.WinForms.Guna2GradientButton;
            if (_btnControl.TryGetValue(btn, out var instance))
            {
                bool newState = instance.Toggle(out string error);
                if (error != null)
                {
                    MessageBox.Show(error, "PLC Error");
                    return;
                }
                btn.FillColor = newState ? System.Drawing.Color.Lime : Color.Silver;
            }
        }
        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            Guna.UI2.WinForms.Guna2GradientButton btn = sender as Guna.UI2.WinForms.Guna2GradientButton;
            if (_btnControl.TryGetValue(btn, out var instacne))
            {
                bool state = instacne.PressDown(out string error);
                if (error != null)
                {
                    MessageBox.Show(error, "PLC Error");
                    return;
                }
                btn.FillColor = state ? System.Drawing.Color.Lime : Color.Silver;
            }
        }
        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            Guna.UI2.WinForms.Guna2GradientButton btn = sender as Guna.UI2.WinForms.Guna2GradientButton;
            if (_btnControl.TryGetValue(btn, out var instacne))
            {
                bool state = instacne.Release(out string error);
                if (error != null)
                {
                    MessageBox.Show(error, "PLC Error");
                    return;
                }
                btn.FillColor = state ? System.Drawing.Color.Lime : Color.Silver;
            }
        }
        #endregion
        #region DataType
        private int DataTypeTextBox(string input, string tag)
        {
            if (ContainsAny(input, "WaitP0F1", "WaitP0F2", "WaitP0H", "txtTime", "txtSpStart", "txtAcc", "txtDec") && tag != null && tag != string.Empty)
            {
                return 10;
            }
            else if (tag == null || tag == string.Empty)
            {
                return 0;
            }
            else if (ContainsAny(input, "IdJob") && tag != null && tag != string.Empty)
            {
                return 1;
            }
            else
            {
                return 100;
            }
        }
        private DataTypeEvent DataTypeButton(string input, string tag)
        {
            if (ContainsAny(input, "RqSaveModel", "btnSaveCurrentPos", "btnMoveCurrentPos", "LoadMasterCam", "Retrig"))
            {
                return DataTypeEvent.ClickMessenger;
            }
            else if (ContainsAny(input, "JOG", "RESET",  "RqOpenSK", "RqCloseSK", "ORG") && tag != null && tag != string.Empty)
            {
                return DataTypeEvent.Mounse;
            }
            else if (ContainsAny(input, "Rq", "Chart", "OFF", "Limit", "Sensor", "SWs") && tag != null && tag != string.Empty)
            {
                return DataTypeEvent.Click;
            }
            else
            {
                return DataTypeEvent.None;
            }
        }
        private static bool ContainsAny(string input, params string[] keys)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            for (int i = 0; i < keys.Length; i++)
            {
                if (input.IndexOf(keys[i], StringComparison.OrdinalIgnoreCase) >= 0) { return true; }
            }
            return false;
        }
        #endregion
        public IEnumerable<System.Windows.Forms.Control> GetAllControls(System.Windows.Forms.Control parent)
        {
            foreach (System.Windows.Forms.Control ctrl in parent.Controls)
            {
                yield return ctrl;
                foreach (System.Windows.Forms.Control child in GetAllControls(ctrl))
                    yield return child;
            }
        }
        private void CheckedChanged(object sender, EventArgs e)
        {
            var btn = sender as Guna.UI2.WinForms.Guna2GradientButton;
            if (btn.Checked)
            {
                btnSaveCurrentPos.Enabled = true;
                btnMoveCurrentPos.Enabled=true;
                btn.FillColor = Color.Lime;
                LoadTags(sender, e);
            }
            else
            {
                btn.FillColor = Color.Silver;
            }
        }
        private void CheckedChanged2(object sender, EventArgs e)
        {
            var btn = sender as Guna.UI2.WinForms.Guna2GradientButton;
            if (btn.Checked)
            {
                btn.FillColor = Color.Lime;
                _plc.SetBitInWord(btn.Tag.ToString().Split('.')[0], int.Parse(btn.Tag.ToString().Split('.')[1]));
            }
            else
            {
                _plc.ResetBitInWord(btn.Tag.ToString().Split('.')[0], int.Parse(btn.Tag.ToString().Split('.')[1]));
                btn.FillColor = Color.Silver;
            }
        }
        private void IdJob_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(IdJob.Text.ToString().Split('.')[0], out int rs))
                {
                    lblCurrentModel.Text = DatabaseControllers.Instance.NameModel(NamePort(this._namePort), rs);
                    ModelSaveAs.Text = "";
                }
            }
            catch { }
        }
        private string NamePort(string s)
        {
            if (s == "Station1")
            {
                return "Port1";
            }
            else if (this._namePort == "Station2")
            {
                return "Port2";
            }
            else if (this._namePort == "Station3")
            {
                return "Port3";
            }
            else
            {
                return "Port4";
            }
        }
        private void cbxModelSaveAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(DatabaseControllers.Instance.IdModel(NamePort(this._namePort), cbxModelSaveAs.Text.ToString()), out int rs))
            {
                ModelSaveAs.Text = rs.ToString();
            }
            else
            {
                ModelSaveAs.Text = "";
            }
        }
        private void ModelSaveAs_TextChanged(object sender, EventArgs e)
        {
            if (ModelSaveAs.Text == "")
            {
                return;
            }
            if (short.TryParse(ModelSaveAs.Text.ToString(), out short value))
            {
                _plc.WriteInt16(ModelSaveAs.Tag.ToString(), value);
            }
        }
        private void frmTeaching2_FormClosing(object sender, FormClosingEventArgs e)
        {
            PLCBitControl.Stop();
            _lblControl.Stop();
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
                    DatabaseControllers.Instance.LoadDataNameModel(cbxModelSaveAs, NamePort(this._namePort));
                    cbxModelSaveAs.Items.Remove(lblCurrentModel.Text.ToString());
                }
            }
            catch { }
        }
        private void frmTeaching2_Load(object sender, EventArgs e)
        {
        }
    }
    public enum DataTypeEvent
    {
        Click,
        Mounse,
        ClickMessenger,
        ClickMessengerComdition,
        None
    }
}
