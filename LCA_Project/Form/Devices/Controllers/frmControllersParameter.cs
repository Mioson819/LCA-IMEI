using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using LCA_Project.Services.Controllers;
namespace LCA_Project.Form.Devices.Controllers
{
    public partial class frmControllersParameter : System.Windows.Forms.Form
    {
        private DrawDashboard drawDashboard ;
        private TypeDatabase typeDatabase;
        private string _condition { get; set; }
        public frmControllersParameter(string condition)
        {
            InitializeComponent();
            _condition = condition;
        }
        private void panelInputs_Paint(object sender, PaintEventArgs e)
        {
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            typeDatabase = (TypeDatabase)tabFolderPort.SelectedIndex;
            TabPage selectedTab = tabFolderPort.SelectedTab;
            Guna2Panel panel = selectedTab.Controls
                            .OfType<Guna2Panel>()
                            .FirstOrDefault();
            drawDashboard = new DrawDashboard(typeDatabase.ToString(), panel,_condition);
        }
        private void pnlInputs_Paint(object sender, PaintEventArgs e)
        {
        }
        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {
        }
        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {
        }
        private void frmControllersParameter_Load(object sender, EventArgs e)
        {
            typeDatabase = (TypeDatabase)tabFolderPort.SelectedIndex;
            TabPage selectedTab = tabFolderPort.SelectedTab;
            Guna2Panel panel = selectedTab.Controls
                            .OfType<Guna2Panel>()
                            .FirstOrDefault();
            drawDashboard = new DrawDashboard(typeDatabase.ToString(), panel, _condition);
        }
        private void guna2Panel6_Paint(object sender, PaintEventArgs e)
        {
        }
        private void guna2Panel3_Paint_1(object sender, PaintEventArgs e)
        {
        }
        private void guna2Panel5_Paint(object sender, PaintEventArgs e)
        {
        }
    }
    public  enum TypeDatabase
    {
        FolderPort,
        Model,
        ControllerTag,
        ControllerParameterInputsResults,
        ControllerParameterInputsSignal,
    }
}
