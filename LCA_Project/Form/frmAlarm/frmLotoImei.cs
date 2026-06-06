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
        private Action _onLock;
        // Constructor dùng khi cần set bit PLC sau khi xác thực
        public frmLotoImei(KeyenceHostLinkTcpClient plc, string address)
        {
            InitializeComponent();
            _plc = plc;
            _address = address;
            _onSuccess = null;
            _onLock = null;
            this.Shown += (s, e) => { this.ActiveControl = txtPassword; txtPassword.Focus(); };
        }
        // Constructor dùng khi xác thực để mở quyền admin, và có thể truyền thêm action để khoá lại
        public frmLotoImei(Action onSuccess, Action onLock = null)
        {
            InitializeComponent();
            _plc = null;
            _address = null;
            _onSuccess = onSuccess;
            _onLock = onLock;
            // Chỉ hiện btnLock khi có action khoá
            btnLock.Visible = (_onLock != null);
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
        private void btnLock_Click(object sender, EventArgs e)
        {
            _onLock?.Invoke();
            this.Close();
        }
    }
}