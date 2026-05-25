using LCA_Project.Database;
using Project_Visionpro.Program.PLC;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Form.frmAlarm
{
    public partial class frmLotoImei : System.Windows.Forms.Form
    {
        private KeyenceHostLinkTcpClient _plc;
        private string _address;
        private Action _onSuccess;

        // Constructor dùng khi cần set bit PLC sau khi xác thực
        public frmLotoImei(KeyenceHostLinkTcpClient plc, string address)
        {
            InitializeComponent();
            _plc = plc;
            _address = address;
            _onSuccess = null;
            this.Shown += (s, e) => { this.ActiveControl = txtPassword; txtPassword.Focus(); };
        }

        // Constructor dùng khi chỉ cần xác thực để mở form khác
        public frmLotoImei(Action onSuccess)
        {
            InitializeComponent();
            _plc = null;
            _address = null;
            _onSuccess = onSuccess;
            this.Shown += (s, e) => { this.ActiveControl = txtPassword; txtPassword.Focus(); };
        }

        private async void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            Guna.UI2.WinForms.Guna2TextBox txt = sender as Guna.UI2.WinForms.Guna2TextBox;
            if (string.IsNullOrEmpty(txt.Text)) return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            txtPassword.Enabled = false;

            if (txt.Text == DateTime.Now.ToString("ddMMyyyy"))
            {
                // Set bit PLC nếu có địa chỉ
                if (_plc != null && !string.IsNullOrEmpty(_address))
                {
                    _plc.SetBitInWord(_address.Split('.')[0], int.Parse(_address.Split('.')[1]));
                }

                this.Close();
                _onSuccess?.Invoke();
            }
            else
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "✗ Sai Password";
                lblStatus.Visible = true;

                await Task.Delay(1000);
                this.Close();
            }
        }
    }
}
