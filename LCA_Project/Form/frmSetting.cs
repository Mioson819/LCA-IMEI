using Bottom_Sorting.Services.Controllers;
using DocumentFormat.OpenXml.Wordprocessing;
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
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Forms;
namespace LCA_Project.Form
{
    public partial class frmSetting : System.Windows.Forms.Form
    {
        private string _namePort { get; set; }
        private KeyenceHostLinkTcpClient _plc { get; set; }
        private Dictionary<Guna.UI2.WinForms.Guna2TextBox, PLCRegisterDataControl> _txtControl = new Dictionary<Guna.UI2.WinForms.Guna2TextBox, PLCRegisterDataControl>();
        private Dictionary<Guna.UI2.WinForms.Guna2GradientButton, PLCBitControl> _btnControl = new Dictionary<Guna.UI2.WinForms.Guna2GradientButton, PLCBitControl>();
        List<PLCRadioControl> _radioBindings = new List<PLCRadioControl>();
        private PLCLabelControl _lblControl;
        private static Type type { get; set; }
        private DataforTagControl dataforController { get; set; }
        public frmSetting(string namePort, KeyenceHostLinkTcpClient plc)
        {
            InitializeComponent();
            _namePort = namePort;
            _plc = plc;
            lblNamePort.Text = lblNamePort.Text + "  " + NamePort(namePort);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            BindTagsOnce();
            Init();
        }
        private void BindTagsOnce()
        {
            dataforController = DatabaseControllers.Instance.GetDataByKey(_namePort);
            type = typeof(DataforTagControl);
            foreach (System.Windows.Forms.Control control in GetAllControls(this))
            {
                var prop = type.GetProperty((control.Name ?? string.Empty).Trim(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                Console.WriteLine(control.Name);
                if (prop == null || dataforController == null) continue;
                var tagObj = prop.GetValue(dataforController);
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
                        continue;
                    }
                    PLCBitControl ctrButton = new PLCBitControl(_plc, btn.Tag?.ToString(), btn);
                    bool state = ctrButton.LoadOnce();
                    btn.FillColor = state ? System.Drawing.Color.Lime : System.Drawing.Color.Silver;
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
           // PLCBitControl.Start();
            _lblControl?.Start();
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
            if (MessageBox.Show("Do you Confirm ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
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
        private async void Button_ClickMessengerbox(object sender, EventArgs e)
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
                else if (error1 != string.Empty)
                {
                    return;
                }
                btn.FillColor = newState ? System.Drawing.Color.Lime : System.Drawing.Color.Silver;
                await Task.Delay(1000);
                //newState = instance.ToggleMessengerReset(out string error2);
                //if (error2 != string.Empty && error2 != null)
                //{
                //    MessageBox.Show(error2, "PLC Error");
                //    return;
                //}
                btn.FillColor = newState ? System.Drawing.Color.Lime : System.Drawing.Color.Silver;
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
                btn.FillColor = newState ? System.Drawing.Color.Lime : System.Drawing.Color.Silver;
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
                btn.FillColor = state ? System.Drawing.Color.Lime : System.Drawing.Color.Silver;
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
                btn.FillColor = state ? System.Drawing.Color.Lime : System.Drawing.Color.Silver;
            }
        }
        #endregion
        #region DataType
        private int DataTypeTextBox(string input, string tag)
        {
            if (ContainsAny(input, "Time","AngeRO") && tag != null && tag != string.Empty)
            {
                return 10;
            }
            else if (tag == null || tag == string.Empty)
            {
                return 0;
            }
            else if (ContainsAny(input, "mY", "nX", "nY", "SpStart", "Acc", "Dec", "RsNG", "IdJob", "ReCheck", "NumberReCheck") && tag != null && tag != string.Empty)
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
            else if (ContainsAny(input, "Rq") && tag != null && tag != string.Empty)
            {
                return DataTypeEvent.Click;
            }
            else if (ContainsAny(input, "JOG", "RESET", "ORG") && tag != null && tag != string.Empty)
            {
                return DataTypeEvent.Mounse;
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
        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            _lblControl?.Stop();
        }
        private void SpCP2Z_TextChanged(object sender, EventArgs e)
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
