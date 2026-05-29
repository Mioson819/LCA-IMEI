using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Bibliography;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Form.TesterComunication;
using LCA_Project.Utilities;
using Project_Visionpro.Program.PLC;
namespace LCA_Project.Form.Signal
{
    public partial class frmAlarm : System.Windows.Forms.Form
    {
        // PcType: "Nano" → *.log (subfolder năm/tháng)
        //         "Pamtech" → *.txt (path gốc)
        // Được set từ ngoài vào (Form1 gọi frmAlarm.PcType = logWatcher.PcType)
        public string PcType { get; set; } = "Nano";
        private bool IsNano => string.Equals(PcType, "Nano", StringComparison.OrdinalIgnoreCase);

        private string filePattern
        {
            get
            {
                if (!IsNano) return $"-{DateTime.Now.Year.ToString().Substring(2)}" + "*.txt";
                else
                {
                    return $"{DateTime.Now.Year}" + "*.log";
                }
            }
        }
        private string ServerFolder(string nameStation)
        {
            //if (LogFileWatcher.OffMess) return DatabaseControllers.Instance.LoadDataFolder(Model, this.Nametation); 
            //else
            //{
            return Path.Combine(DatabaseControllers.Instance.LoadDataFolder(Model, this.Nametation), $"{DateTime.Now.Year.ToString()}", $"{DateTime.Now.Month.ToString("D2")}");
            // }
        }
        private string _NameStation { get; set; }
        public string Nametation
        {
            get { return _NameStation; }
            set
            {
                if (value == "Station1") _NameStation = "Port1";
                else if (value == "Station2") _NameStation = "Port2";
                else if (value == "Station3") _NameStation = "Port3";
                else if (value == "Station4") _NameStation = "Port4";
                else _NameStation = value;
            }
        }
        public string Model { get; set; }
        private string folder { get; set; }
        private string label { get; set; }
        private KeyenceHostLinkTcpClient plc { get; set; }
        private string nameStation { get; set; }
        private Type type { get; set; }
        private CancellationTokenSource _cts;
        public frmAlarm(string s, KeyenceHostLinkTcpClient plc, string station)
        {
            this.plc = plc;
            InitializeComponent();
            this.label = s;
            this.nameStation = station;
            this.folder = folder;
            Nametation = station;
        }
        private void Label1_Click(object sender, EventArgs e)
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
        private void AutoRead()
        {
            type = typeof(DataforInputResults);
            foreach (System.Windows.Forms.Control control in GetAllControls(this))
            {
                if (control is Guna2GradientButton buttonGradion)
                {
                    var prop = type.GetProperty(buttonGradion.Name.ToString().Trim(),
       System.Reflection.BindingFlags.IgnoreCase |
       System.Reflection.BindingFlags.Public |
       System.Reflection.BindingFlags.Instance);
                    if (prop != null)
                    {
                        var dataObj = DatabaseControllers.Instance.GetDataByInputResults(this.nameStation);
                        if (dataObj == null) continue;
                        string value = prop.GetValue(dataObj)?.ToString().Trim();
                        if (!string.IsNullOrEmpty(value))
                        {
                            this.Invoke(new Action(() =>
                            {
                                buttonGradion.Tag = value;
                            }));
                            //  MessageBox.Show($"{buttonGradion.Name.ToString()}");
                            buttonGradion.MouseDown += MouseDowns;
                            buttonGradion.MouseUp += MouseUps;
                        }
                    }
                    else
                    {
                        // MessageBox.Show($"{buttonGradion.Name.ToString().Trim()}");
                    }
                }
            }
        }
        private void MouseUps(object sender, MouseEventArgs e)
        {
            var btn = sender as Guna2GradientButton;
            string tag = btn.Tag as string;
            Task.Run(() =>
            {
                this.plc.ResetBitInWord(tag.Split('.')[0].ToString(), int.Parse(tag.Split('.')[1]));
            });
            btn.FillColor = System.Drawing.Color.Gray;
        }
        private void MouseDowns(object sender, MouseEventArgs e)
        {
            var btn = sender as Guna2GradientButton;
            string tag = btn.Tag as string;
            Task.Run(() =>
            {
                this.plc.SetBitInWord(tag.Split('.')[0].ToString(), int.Parse(tag.Split('.')[1]));
            });
            btn.FillColor = System.Drawing.Color.Green;
        }
        private void FrmAlarm_Load(object sender, EventArgs e)
        {
            lblError.Text = this.label;
            AutoRead();
            if (this.label.IndexOf("Vaccum Fail", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                this.Skip.Visible = true;
                this.Retri.Visible = true;
            }
            else if (this.label.IndexOf("No Data", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                this.Skip.Visible = false;
                this.Retri.Visible = false;
                btnDelete.Visible = true;
                Restart.Visible = false;
            }
            else if (this.label.IndexOf("Cửa Đang Mở", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                this.Restart.Visible = true;
                btnDelete.Visible = false;
                this.Skip.Visible = false;
                this.Retri.Visible = false;
            }
            else
            {
                this.Restart.Visible = false;
                btnDelete.Visible = false;
                this.Skip.Visible = false;
                this.Retri.Visible = false;
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteFile(ServerFolder(nameStation));
            this.Close();
        }
        private void DeleteFile(string filename)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(filename);
                FileInfo value = dir.GetFiles(this.filePattern).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                if (value == null)
                {
                    MessageBox.Show("Không Tìm Thấy File");
                    return;
                }
                File.Delete(value.FullName);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Không Thể Xóa Do File Đang Được Truy Cập");
            }
            catch (Exception e)
            {
                MessageBox.Show($"ERROR : {e}");
            }
        }
    }
}