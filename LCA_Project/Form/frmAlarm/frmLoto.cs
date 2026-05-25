using LCA_Project.Database;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Forms;
namespace LCA_Project.Form.frmAlarm
{
    public partial class frmLoto : System.Windows.Forms.Form
    {
        private KeyenceHostLinkTcpClient _plc;
        private string Address;
        public frmLoto(KeyenceHostLinkTcpClient plc,string address)
        {
            InitializeComponent();
            _plc = plc;
            Address = address;
        }
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            Guna.UI2.WinForms.Guna2TextBox txt = sender as Guna.UI2.WinForms.Guna2TextBox;
            if (txt.Text != string.Empty || txt.Text != null)
            {
                if (DatabaseControllers.Instance.GetPasswordMaster(txt.Text))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    if (_plc.ReadBitFromWord(Address.Split('.')[0], int.Parse(Address.Split('.')[1])))
                    {
                        _plc.ResetBitInWord(Address.Split('.')[0], int.Parse(Address.Split('.')[1]));
                    }
                    else
                    {
                        _plc.SetBitInWord(Address.Split('.')[0], int.Parse(Address.Split('.')[1]));
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Sai Password");
                }
            }
        }
    }
}
