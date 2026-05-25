using LCA_Project.Services.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Form.Devices.Controllers
{
    public partial class frmControllers : System.Windows.Forms.Form
    {
        private DrawDashboard drawDashboard;
        public frmControllers()
        {
            InitializeComponent();
            guna2Panel1.HandleCreated += (s, e) =>
            {
                drawDashboard = new DrawDashboard("Controllers", guna2Panel1);
            };
        }
        public frmControllers(string Condition)
        {
            InitializeComponent();
            guna2Panel1.HandleCreated += (s, e) =>
            {
                drawDashboard = new DrawDashboard("ControllerTableSetting", guna2Panel1,Condition);
            };
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
        }
        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {
        }
        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
        }
    }
}
