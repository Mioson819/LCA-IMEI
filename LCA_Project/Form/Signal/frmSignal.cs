using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Utilities;
using LCA_Samsung.Process;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Form.Signal
{
    public partial class frmSignal : System.Windows.Forms.Form
    {
        private Dictionary<string, ButtonSignal<Guna2CircleButton>> _controller = new Dictionary<string, ButtonSignal<Guna2CircleButton>>();
        private Action<string> action;
        private string _name { get; set; }
        public frmSignal(Dictionary<string, ButtonSignal<Guna2CircleButton>> controller,string name )
        {
            InitializeComponent();
            _controller = controller;
            this.Load += frmSignal_Load;
            flpSignal.AutoScroll = true;
            flpp2.AutoScroll = true;
            flpp3.AutoScroll = true;
            flpp4.AutoScroll = true;
            flpSignal.FlowDirection = FlowDirection.LeftToRight;
            _name = name;
            action += ReadData;
        }
        private void frmSignal_Load(object sender, EventArgs e)
        {
            foreach (var item in _controller)
            {
                foreach (var register in DatabaseControllers.Instance.GetRegister_ControllerParameterInputs())
                {
                    if (register.Key.Contains("Station1"))
                    {
                        if (item.Key == register.Value)
                        {
                            flpSignal.Controls.Add(item.Value);
                            break;
                        }
                    }
                    else if (register.Key.Contains("Station2"))
                    {
                        if (item.Key == register.Value)
                        {
                            flpp2.Controls.Add(item.Value);
                            break;
                        }
                    }
                    else if (register.Key.Contains("Station3"))
                    {
                        if (item.Key == register.Value)
                        {
                            flpp3.Controls.Add(item.Value);
                            break;
                        }
                    }
                    else if (register.Key.Contains("Station4"))
                    {
                        if (item.Key == register.Value)
                        {
                            flpp4.Controls.Add(item.Value);
                            break;
                        }
                    }
                }
            }
            action?.Invoke(_name);
        }
        private void ReadData(string name)
        {
            Task.Run(() =>
            {
                ProcessMain.Instance.StartAutoReadSignalPLC(name);
            });
        }
        private void flpSignal_Paint(object sender, PaintEventArgs e)
        {
        }
        private void label1_Click(object sender, EventArgs e)
        {
        }
        private void frmSignal_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Controls.Clear();
           ProcessMain.Instance._cts.Cancel();
        }
        private void guna2ResizeBox1_Click(object sender, EventArgs e)
        {
        }
    }
}
