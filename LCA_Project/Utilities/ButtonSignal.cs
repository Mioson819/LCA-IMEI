using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
namespace LCA_Project.Utilities
{
    public class ButtonSignal<T> : System.Windows.Forms.Panel where T : Control, new()
    {
        //    private Guna2CircleButton _buttonSignal;
        private Guna2VSeparator _vseparator;
        private System.Windows.Forms.Label _labelSignal;
        private System.Windows.Forms.Label _labelRegister;
        private Guna2Panel _panel;
        private bool oldState = false;
        public string NameSignal
        {
            set
            {
                int idx = value?.IndexOf('(') ?? -1;
                string trimmed = idx >= 0 ? value.Substring(0, idx).Trim() : value;
                // Bỏ hậu tố " Input" / " Output" ở cuối tên hiển thị
                trimmed = trimmed
                    .Replace(" Input", "")
                    .Replace(" Output", "")
                    .Trim();
                _labelSignal.Text = trimmed;
            }
        }
        public string RegisterSignal
        {
            set { _labelRegister.Text = value; }
        }
        private string _imgSignal;
        public string ImgSignal
        {
            get
            {
                if (_imgSignal == null)
                {
                    return "Control";
                }
                return _imgSignal;
            }
            set
            {
                _imgSignal = value;
            }
        }
        public ButtonSignal()
        {
            // var basebutton;
            _panel = new Guna2Panel();
            //    _buttonSignal = new Guna2CircleButton();
            _vseparator = new Guna2VSeparator();
            _labelSignal = new System.Windows.Forms.Label();
            _labelRegister = new System.Windows.Forms.Label();
            this.Controls.Add(_panel);
            _panel.Dock = DockStyle.Fill;
            this.Size = new System.Drawing.Size(301, 89);
            if (new T() is Guna2CircleButton _button)
            {
                _button.Size = new System.Drawing.Size(89, 81);
                _button.Location = new System.Drawing.Point(23, 4);
                _button.BackColor = System.Drawing.Color.LightSkyBlue;
                _button.FillColor = System.Drawing.Color.Red;
                _panel.Controls.Add(_button);
            }
            else if (new T() is Guna2Button button)
            {
                button.Size = new System.Drawing.Size(89, 81);
                button.Location = new System.Drawing.Point(23, 4);
                button.BackColor = System.Drawing.Color.LightSkyBlue;
                button.FillColor = System.Drawing.Color.Red;
                button.Image = (Bitmap)LCA_Project.Properties.Resources.ResourceManager.GetObject(ImgSignal);
                button.ImageSize = new System.Drawing.Size(42, 42);
                _panel.Controls.Add(button);
            }
            else
            {
                throw new InvalidOperationException("Unsupported control type. No  supported.");
            }
            _panel.BorderRadius = 18;
            _panel.BorderThickness = 0;
            _panel.BorderColor = System.Drawing.Color.Black;
            _panel.FillColor = System.Drawing.Color.LightSkyBlue;
            _vseparator.Size = new System.Drawing.Size(14, 89);
            _labelSignal.Size = new System.Drawing.Size(113, 41);
            _labelSignal.Font = new System.Drawing.Font(new FontFamily("Segoe UI Semilight"), 9.6f, FontStyle.Bold | FontStyle.Italic);
            _labelSignal.ForeColor = System.Drawing.Color.Black;
            _labelSignal.Text = "Signal";
            _labelSignal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _labelRegister.Size = new System.Drawing.Size(113, 36);
            _labelRegister.Font = new System.Drawing.Font(new FontFamily("Segoe UI Semilight"), 8.4f, FontStyle.Bold);
            _labelRegister.ForeColor = System.Drawing.Color.DarkSlateGray;
            _labelRegister.Text = "";
            _labelRegister.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _panel.Controls.Add(_vseparator);
            _panel.Controls.Add(_labelSignal);
            _panel.Controls.Add(_labelRegister);
            _panel.BackColor = this.BackColor;
            _vseparator.Location = new System.Drawing.Point(132, 4);
            _labelSignal.Location = new System.Drawing.Point(156, 4);
            _labelRegister.Location = new System.Drawing.Point(156, 48);
            _labelSignal.BackColor = _panel.FillColor;
            _labelRegister.BackColor = _panel.FillColor;
            _vseparator.BackColor = _panel.FillColor;
        }
        public void UpdateSignal(bool state)
        {
            if (state != oldState)
            {
                if (state)
                {
                    this.BeginInvoke((MethodInvoker)(() =>
                    {
                        foreach (System.Windows.Forms.Control ctrl in GetAllControls(_panel))
                        {
                            if (ctrl is Guna2Button btn)
                            {
                                btn.FillColor = System.Drawing.Color.Green;
                            }
                            else if (ctrl is Guna2CircleButton btn2)
                            {
                                btn2.FillColor = System.Drawing.Color.Green;
                            }
                        }
                    }));
                }
                else
                {
                    this.BeginInvoke((MethodInvoker)(() =>
                    {
                        foreach (System.Windows.Forms.Control ctrl in GetAllControls(_panel))
                        {
                            if (ctrl is Guna2Button btn)
                            {
                                btn.FillColor = System.Drawing.Color.Red;
                            }
                            else if (ctrl is Guna2CircleButton btn2)
                            {
                                btn2.FillColor = System.Drawing.Color.Red;
                            }
                        }
                    }));
                }
                oldState = state;
            }
        }
        public IEnumerable<Control> GetAllControls(System.Windows.Forms.Control parent)
        {
            foreach (System.Windows.Forms.Control ctrl in parent.Controls)
            {
                yield return ctrl;
                foreach (System.Windows.Forms.Control child in GetAllControls(ctrl))
                {
                    yield return child;
                }
            }
        }
    }
}