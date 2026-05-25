using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LCA_Project.Database;
using LCA_Project.Utilities;
using System.Threading;
using Project_Visionpro.Program.PLC;
namespace LCA_Project.Form.Teaching
{
    public partial class frmCurPos : System.Windows.Forms.Form
    {
        private CancellationTokenSource _cts;
        private KeyenceHostLinkTcpClient plc;
        private string nameStation { get; set; }
        public frmCurPos(string station,KeyenceHostLinkTcpClient plc)
        {
            this.nameStation = station;
            this.plc = plc;
            InitializeComponent();
        }
        private void Label2_Click(object sender, EventArgs e)
        {
        }
        private void Label5_Click(object sender, EventArgs e)
        {
        }
        private void CurPosX_Click(object sender, EventArgs e)
        {
        }
        private void AutoRead()
        {
            _cts = new CancellationTokenSource();
            Type type = typeof(DataforInputResults);
            foreach (System.Windows.Forms.Control control in GetAllControls(this))
            {
                if (control is System.Windows.Forms.Label label)
                {
                    double currentValue = 0;
                    double valueNow;
                    var prop = type.GetProperty(label.Name.ToString().Trim(),
            System.Reflection.BindingFlags.IgnoreCase |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);
                    if (prop != null)
                    {
                        var dataObj = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
                        if (dataObj == null) continue;
                        string value = prop.GetValue(dataObj)?.ToString().Trim();
                        if (string.IsNullOrEmpty(value)) continue;
                        //   label.Tag = value;
                        Task.Run(async () =>
                        {
                            while (!_cts.IsCancellationRequested)
                            {
                                valueNow = this.plc.ReadInt32(value) / 100.0;
                                if (valueNow != currentValue)
                                {
                                    currentValue = valueNow;
                                    this.BeginInvoke((MethodInvoker)(() =>
                                    {
                                        label.Text = currentValue.ToString("0.00");
                                    }));
                                }
                                await Task.Delay(50);
                            }
                        }, _cts.Token);
                    }
                }
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
        private void CurPosRO_Click(object sender, EventArgs e)
        {
        }
        private void FrmCurPos_Load(object sender, EventArgs e)
        {
            AutoRead();
        }
        private void Label3_Click(object sender, EventArgs e)
        {
        }
        private void Guna2ControlBox2_Click(object sender, EventArgs e)
        {
        }
        private void FrmCurPos_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts.Cancel();
        }
    }
}
