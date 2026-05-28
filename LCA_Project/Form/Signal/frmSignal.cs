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

        public frmSignal(Dictionary<string, ButtonSignal<Guna2CircleButton>> controller, string name)
        {
            InitializeComponent();
            _controller = controller;
            this.Load += frmSignal_Load;
            flpp1in.AutoScroll = true;
            flpp2in.AutoScroll = true;
            flpp3in.AutoScroll = true;
            flpp4in.AutoScroll = true;
            flpp1out.AutoScroll = true;
            flpp2out.AutoScroll = true;
            flpp3out.AutoScroll = true;
            flpp4out.AutoScroll = true;
            flpp1in.FlowDirection = FlowDirection.LeftToRight;
            _name = name;
            action += ReadData;
        }

        private void frmSignal_Load(object sender, EventArgs e)
        {
            // Cả Input và Output đều nằm trong cùng bảng ControllerParameterInputsSignal
            // Phân biệt bằng từ "Input" / "Output" trong register.Key
            var registers = DatabaseControllers.Instance.GetRegister_ControllerParameterInputs();

            foreach (var item in _controller)
            {
                foreach (var register in registers)
                {
                    if (item.Key != register.Value) continue;

                    bool isInput = register.Key.Contains("Input");
                    bool isOutput = register.Key.Contains("Output");

                    if (register.Key.Contains("Station1"))
                    {
                        if (isInput) { flpp1in.Controls.Add(item.Value); break; }
                        if (isOutput) { flpp1out.Controls.Add(item.Value); break; }
                    }
                    else if (register.Key.Contains("Station2"))
                    {
                        if (isInput) { flpp2in.Controls.Add(item.Value); break; }
                        if (isOutput) { flpp2out.Controls.Add(item.Value); break; }
                    }
                    else if (register.Key.Contains("Station3"))
                    {
                        if (isInput) { flpp3in.Controls.Add(item.Value); break; }
                        if (isOutput) { flpp3out.Controls.Add(item.Value); break; }
                    }
                    else if (register.Key.Contains("Station4"))
                    {
                        if (isInput) { flpp4in.Controls.Add(item.Value); break; }
                        if (isOutput) { flpp4out.Controls.Add(item.Value); break; }
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