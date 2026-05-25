using Guna.UI2.AnimatorNS;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Utilities;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Form.Devices.Controllers
{
    public partial class frmSekectPos : System.Windows.Forms.Form
    {
        private string _nameController { get; set; }
        private CancellationTokenSource _cts;
        private KeyenceHostLinkTcpClient _controller { get; set; }
        public frmSekectPos(string NameController,KeyenceHostLinkTcpClient plc)
        {
            InitializeComponent();
            this._nameController = NameController;
            this._controller = plc;
        }
        private void frmSekectPos_Load(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            if (_controller == null)
            {
                MessageBox.Show("Controller not found or Not  connected .");
                this.Close();
                return;
            }
            //   _ctsTXT= new CancellationTokenSource();
            var dataforController = DatabaseControllers.Instance.GetDataByKey(_nameController);
            Type type = typeof(DataforTagControl);
            foreach (System.Windows.Forms.Control control in GetAllControls(this))
            {
                if (control is Guna2Button button)
                {
                    if (button != null)
                    {
                        var prop = type.GetProperty(button.Name.ToString().Trim(),
                System.Reflection.BindingFlags.IgnoreCase |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
                        if (prop != null)
                        {
                            string btnName = button.Name.ToString().Trim();
                            var pr = prop.GetValue(dataforController)?.ToString().Trim();
                            if (pr == null) continue;
                            button.Tag = pr;
                            button.Click += Clicks;
                        }
                    }
                }
                else if (control is Guna2TextBox textBox)
                {
                    var prop = type.GetProperty(textBox.Name.ToString().Trim(),
            System.Reflection.BindingFlags.IgnoreCase |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);
                    if (prop == null || dataforController == null) continue;
                    string name = textBox.Name.ToString().Trim();
                    string value = prop.GetValue(dataforController)?.ToString().Trim();
                    textBox.Tag = value;
                    _ = Task.Run(async () =>
                          {
                              short currentValue = 0;
                              short valueNow;
                              while (!_cts.IsCancellationRequested)
                              {
                                  valueNow = _controller.ReadInt16(value);
                                  if (valueNow != currentValue)
                                  {
                                      currentValue = valueNow;
                                      textBox.BeginInvoke(new Action(() =>
                                      {
                                          textBox.Text = value.ToString();
                                      }));
                                  }
                                  await Task.Delay(50);
                              }
                          }, _cts.Token);
                }
            }
        }
        private void Clicks(object sender, EventArgs e)
        {
            string tag = (string)((Guna2Button)sender).Tag;
            var btn = ((Guna2Button)sender);
            bool _statuscolor = btn.FillColor != System.Drawing.Color.Green ? true : false;
            Task.Run(async () =>
            {
                if (tag != null)
                {
                    if (_statuscolor)
                    {
                        while (true)
                        {
                            if (_controller.SetBitInWord(tag.Split('.')[0].ToString(), int.Parse(tag.Split('.')[1])))
                            {
                                if (_controller.ReadBitFromWord(tag.Split('.')[0].ToString(), int.Parse(tag.Split('.')[1])))
                                {
                                    break;
                                }
                            }
                            await Task.Delay(200);
                        }
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            btn.FillColor = System.Drawing.Color.Green;
                        }));
                    }
                    else
                    {
                        while (true)
                        {
                            if (_controller.ResetBitInWord(tag.Split('.')[0].ToString(), int.Parse(tag.Split('.')[1])))
                            {
                                if (_controller.ReadBitFromWord(tag.Split('.')[0].ToString(), int.Parse(tag.Split('.')[1])) == false)
                                {
                                    break;
                                }
                            }
                            await Task.Delay(200);
                        }
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            btn.FillColor = System.Drawing.Color.Gray;
                        }));
                    }
                }
            });
        }
        private void PosReadyX_TextChanged(object sender, EventArgs e)
        {
        }
        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
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
        private void frmSekectPos_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts.Cancel();
        }
    }
}
