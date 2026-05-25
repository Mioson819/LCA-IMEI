using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Form.frmAlarm
{
    public partial class frmWatting : System.Windows.Forms.Form
    {
        private Stopwatch StartWatch = new Stopwatch();
        public frmWatting()
        {
            InitializeComponent();
        }
        private void frmWatting_Load(object sender, EventArgs e)
        {
        }
        public void Watch(string value,string NamePort)
        {
            if (value=="Watting")
            {
                StartWatch.Start();
                lblError.Text = "Đang đổi Model " + NamePort;
            }
           else if (value == "Success")
            {
                lblError.Text = "Đổi Thành Công";
                lblError.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblError.Text = "Lỗi Đổi Model Camera , Kiểm Tra Model Camera đã được khởi tạo hay không ?";
            }
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
