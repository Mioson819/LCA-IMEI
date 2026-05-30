using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
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
using System.Windows.Forms;
namespace LCA_Project.Form.frmAlarm
{
    public partial class frmUser : System.Windows.Forms.Form
    {
        //private KeyenceHostLinkTcpClient _plc;
       // private string Address;
        public  Action<string> _user; 
        public frmUser()
        {
            InitializeComponent();
        }
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (cbxUser.Text != string.Empty || cbxUser != null)
            {
                if (e.KeyCode != Keys.Enter) return;
                Guna.UI2.WinForms.Guna2TextBox txt = sender as Guna.UI2.WinForms.Guna2TextBox;
                if (txt.Text != string.Empty || txt.Text != null)
                {
                    if (cbxUser.Text == "Master") 
                    {
                        if (DatabaseControllers.Instance.GetPassword(1,txt.Text))
                        {
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            _user?.Invoke("Master");
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Sai Password");
                        }
                    }
                    else if (cbxUser.Text == "Employee")
                    {
                        if (DatabaseControllers.Instance.GetPassword(2, txt.Text))
                        {
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            _user?.Invoke("Employee");
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Sai Password");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sai Password");
                }
            }
            else
            {
                MessageBox.Show("Please Selected User");
            }
        }
        private void txtNewPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (cbxUser.Text != string.Empty || cbxUser != null)
            {
                if (e.KeyCode != Keys.Enter) return;
                Guna.UI2.WinForms.Guna2TextBox txt = sender as Guna.UI2.WinForms.Guna2TextBox;
                if (txt.Text != string.Empty || txt.Text != null)
                {
                    if (cbxUser.Text == "Master")
                    {
                        if (DatabaseControllers.Instance.GetPassword(1, txtPassword.Text))
                        {
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            DatabaseControllers.Instance.UpdatePassword(1, txt.Text);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Sai Password Hien Tai");
                        }
                    }
                    else if (cbxUser.Text == "Employee")
                    {
                        if (DatabaseControllers.Instance.GetPassword(2, txtPassword.Text))
                        {
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            DatabaseControllers.Instance.UpdatePassword(2, txt.Text);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Sai Password Hien Tai");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please Selected New PassWord");
                }
            }
            else
            {
                MessageBox.Show("Please Selected User");
            }
        }
    }
}
