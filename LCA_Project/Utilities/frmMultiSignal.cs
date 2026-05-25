using Guna.UI2.WinForms;
using LCA_Project.Database;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Guna.UI2.AnimatorNS;
using LCA_Project.Form.Signal;
using LCA_Samsung.Process;
namespace LCA_Project.Utilities
{
    public class frmMultiSignal : System.Windows.Forms.FlowLayoutPanel
    {
        private frmSignal frmSignal;
        private Guna2GradientButton btnSignal;
        private Guna2Separator separator;
        private Guna2Elipse elipse;
        private Guna2DragControl dragControl;
        private Guna2Transition transition;
        public frmMultiSignal()
        {
            transition = new Guna2Transition();
            this.Width = 210;
            this.Height = 130;
            this.AutoScroll = true;
            this.FlowDirection = FlowDirection.TopDown;
            this.BackColor = System.Drawing.Color.FromArgb(59, 68, 70);
            this.WrapContents = false;
            elipse = new Guna2Elipse();
            dragControl = new Guna2DragControl();
            var names = DatabaseControllers.Instance.GetName_Controller();
            foreach (var name in names)
            {
                if(name==null||name =="")
                    return;
                separator = new Guna2Separator();
                btnSignal = new Guna2GradientButton();
                btnSignal.Tag = name.ToString();
                btnSignal.Text = name;
                btnSignal.Width = 180;
                btnSignal.Height = 45;
                btnSignal.FillColor = System.Drawing.Color.FromArgb(41, 44, 54);
                btnSignal.FillColor2 = System.Drawing.Color.FromArgb(41, 44, 54);
                btnSignal.HoverState.FillColor = System.Drawing.Color.FromArgb(241, 84, 127);
                btnSignal.HoverState.FillColor2 = System.Drawing.Color.FromArgb(41, 44, 54);
                btnSignal.CustomizableEdges = new Guna.UI2.WinForms.Suite.CustomizableEdges()
                {
                    BottomLeft = true,
                    BottomRight = false,
                    TopLeft = true,
                    TopRight = false
                };
                btnSignal.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Segoe UI Semilight"), 12, System.Drawing.FontStyle.Italic);
                btnSignal.ForeColor = System.Drawing.Color.White;
                btnSignal.BorderRadius = 8;
                separator.FillColor = System.Drawing.Color.FromArgb(41, 44, 54);
                separator.Width = 180;
                separator.Height = 10;
                btnSignal.Click += btnSignal_Click;
                this.Controls.Add(btnSignal);
                this.Controls.Add(separator);
                elipse.TargetControl = this;
                elipse.BorderRadius = 8;
                dragControl.TargetControl = this;
                dragControl.DragMode = Guna.UI2.WinForms.Enums.DragMode.Control;
            }
        }
        private void btnSignal_Click(object sender, EventArgs e)
        {
            var button = sender as Guna2GradientButton;
            if (button != null)
            {
                string controllerName = button.Tag.ToString();
                //  ProcessMain.Instance._Signal.TryGetValue(System.Windows.Forms.ControllerName, out var Name);
                if (Name == null)
                {
                    MessageBox.Show("No signal data available for this controller.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                //frmSignal = new frmSignal(Name, controllerName);
                //frmSignal.StartPosition = FormStartPosition.Manual;
                //Point locationOnForm = this.Parent.PointToScreen(this.Location);
                //frmSignal.StartPosition = FormStartPosition.CenterScreen;
                //frmSignal.Show();
                //frmSignal.BringToFront();
            }
        }
    }
}
