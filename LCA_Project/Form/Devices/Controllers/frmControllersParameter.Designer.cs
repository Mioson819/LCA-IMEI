namespace LCA_Project.Form.Devices.Controllers
{
    partial class frmControllersParameter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            this.guna2ControlBox3 = new Guna.UI2.WinForms.Guna2ControlBox();
            this.guna2ControlBox1 = new Guna.UI2.WinForms.Guna2ControlBox();
            this.guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2DragControl1 = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.tabFolderPort = new System.Windows.Forms.TabControl();
            this.tabOutputs = new System.Windows.Forms.TabPage();
            this.pnlOutputs = new Guna.UI2.WinForms.Guna2Panel();
            this.tabInputResults = new System.Windows.Forms.TabPage();
            this.guna2Panel4 = new Guna.UI2.WinForms.Guna2Panel();
            this.tabInputSignal = new System.Windows.Forms.TabPage();
            this.guna2Panel5 = new Guna.UI2.WinForms.Guna2Panel();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.guna2Panel3 = new Guna.UI2.WinForms.Guna2Panel();
            this.guna2Panel2 = new Guna.UI2.WinForms.Guna2Panel();
            this.tbModel = new System.Windows.Forms.TabPage();
            this.guna2Panel6 = new Guna.UI2.WinForms.Guna2Panel();
            this.guna2Panel1.SuspendLayout();
            this.tabFolderPort.SuspendLayout();
            this.tabOutputs.SuspendLayout();
            this.tabInputResults.SuspendLayout();
            this.tabInputSignal.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.guna2Panel2.SuspendLayout();
            this.tbModel.SuspendLayout();
            this.SuspendLayout();
            // 
            // guna2Panel1
            // 
            this.guna2Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(44)))), ((int)(((byte)(54)))));
            this.guna2Panel1.Controls.Add(this.guna2ControlBox3);
            this.guna2Panel1.Controls.Add(this.guna2ControlBox1);
            this.guna2Panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.guna2Panel1.Location = new System.Drawing.Point(0, 0);
            this.guna2Panel1.Name = "guna2Panel1";
            this.guna2Panel1.Size = new System.Drawing.Size(800, 45);
            this.guna2Panel1.TabIndex = 0;
            this.guna2Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.guna2Panel1_Paint);
            // 
            // guna2ControlBox3
            // 
            this.guna2ControlBox3.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox;
            this.guna2ControlBox3.Dock = System.Windows.Forms.DockStyle.Right;
            this.guna2ControlBox3.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(44)))), ((int)(((byte)(54)))));
            this.guna2ControlBox3.IconColor = System.Drawing.Color.White;
            this.guna2ControlBox3.Location = new System.Drawing.Point(679, 0);
            this.guna2ControlBox3.Name = "guna2ControlBox3";
            this.guna2ControlBox3.Size = new System.Drawing.Size(59, 45);
            this.guna2ControlBox3.TabIndex = 4;
            // 
            // guna2ControlBox1
            // 
            this.guna2ControlBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.guna2ControlBox1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(44)))), ((int)(((byte)(54)))));
            this.guna2ControlBox1.IconColor = System.Drawing.Color.White;
            this.guna2ControlBox1.Location = new System.Drawing.Point(738, 0);
            this.guna2ControlBox1.Name = "guna2ControlBox1";
            this.guna2ControlBox1.Size = new System.Drawing.Size(62, 45);
            this.guna2ControlBox1.TabIndex = 1;
            // 
            // guna2Elipse1
            // 
            this.guna2Elipse1.TargetControl = this;
            // 
            // guna2DragControl1
            // 
            this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2DragControl1.TargetControl = this.guna2Panel1;
            this.guna2DragControl1.UseTransparentDrag = true;
            // 
            // tabFolderPort
            // 
            this.tabFolderPort.Controls.Add(this.tabPage1);
            this.tabFolderPort.Controls.Add(this.tbModel);
            this.tabFolderPort.Controls.Add(this.tabOutputs);
            this.tabFolderPort.Controls.Add(this.tabInputResults);
            this.tabFolderPort.Controls.Add(this.tabInputSignal);
            this.tabFolderPort.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabFolderPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabFolderPort.Location = new System.Drawing.Point(0, 0);
            this.tabFolderPort.Margin = new System.Windows.Forms.Padding(2);
            this.tabFolderPort.Name = "tabFolderPort";
            this.tabFolderPort.SelectedIndex = 0;
            this.tabFolderPort.Size = new System.Drawing.Size(800, 405);
            this.tabFolderPort.TabIndex = 1;
            this.tabFolderPort.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabOutputs
            // 
            this.tabOutputs.Controls.Add(this.pnlOutputs);
            this.tabOutputs.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.tabOutputs.Location = new System.Drawing.Point(4, 22);
            this.tabOutputs.Margin = new System.Windows.Forms.Padding(2);
            this.tabOutputs.Name = "tabOutputs";
            this.tabOutputs.Padding = new System.Windows.Forms.Padding(2);
            this.tabOutputs.Size = new System.Drawing.Size(792, 379);
            this.tabOutputs.TabIndex = 5;
            this.tabOutputs.Text = "Outputs";
            this.tabOutputs.UseVisualStyleBackColor = true;
            // 
            // pnlOutputs
            // 
            this.pnlOutputs.Cursor = System.Windows.Forms.Cursors.Default;
            this.pnlOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOutputs.Location = new System.Drawing.Point(2, 2);
            this.pnlOutputs.Name = "pnlOutputs";
            this.pnlOutputs.Size = new System.Drawing.Size(788, 375);
            this.pnlOutputs.TabIndex = 0;
            this.pnlOutputs.Paint += new System.Windows.Forms.PaintEventHandler(this.guna2Panel3_Paint);
            // 
            // tabInputResults
            // 
            this.tabInputResults.Controls.Add(this.guna2Panel4);
            this.tabInputResults.Location = new System.Drawing.Point(4, 22);
            this.tabInputResults.Margin = new System.Windows.Forms.Padding(2);
            this.tabInputResults.Name = "tabInputResults";
            this.tabInputResults.Padding = new System.Windows.Forms.Padding(2);
            this.tabInputResults.Size = new System.Drawing.Size(792, 379);
            this.tabInputResults.TabIndex = 3;
            this.tabInputResults.Text = "Inputs Results";
            this.tabInputResults.UseVisualStyleBackColor = true;
            // 
            // guna2Panel4
            // 
            this.guna2Panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2Panel4.Location = new System.Drawing.Point(2, 2);
            this.guna2Panel4.Name = "guna2Panel4";
            this.guna2Panel4.Size = new System.Drawing.Size(788, 375);
            this.guna2Panel4.TabIndex = 1;
            // 
            // tabInputSignal
            // 
            this.tabInputSignal.Controls.Add(this.guna2Panel5);
            this.tabInputSignal.Location = new System.Drawing.Point(4, 22);
            this.tabInputSignal.Margin = new System.Windows.Forms.Padding(2);
            this.tabInputSignal.Name = "tabInputSignal";
            this.tabInputSignal.Padding = new System.Windows.Forms.Padding(2);
            this.tabInputSignal.Size = new System.Drawing.Size(792, 379);
            this.tabInputSignal.TabIndex = 4;
            this.tabInputSignal.Text = "Inputs Signal";
            this.tabInputSignal.UseVisualStyleBackColor = true;
            // 
            // guna2Panel5
            // 
            this.guna2Panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2Panel5.Location = new System.Drawing.Point(2, 2);
            this.guna2Panel5.Name = "guna2Panel5";
            this.guna2Panel5.Size = new System.Drawing.Size(788, 375);
            this.guna2Panel5.TabIndex = 1;
            this.guna2Panel5.Paint += new System.Windows.Forms.PaintEventHandler(this.guna2Panel5_Paint);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.guna2Panel3);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(792, 379);
            this.tabPage1.TabIndex = 6;
            this.tabPage1.Text = "Folder Port";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // guna2Panel3
            // 
            this.guna2Panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2Panel3.Location = new System.Drawing.Point(3, 3);
            this.guna2Panel3.Name = "guna2Panel3";
            this.guna2Panel3.Size = new System.Drawing.Size(786, 373);
            this.guna2Panel3.TabIndex = 2;
            // 
            // guna2Panel2
            // 
            this.guna2Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(44)))), ((int)(((byte)(54)))));
            this.guna2Panel2.Controls.Add(this.tabFolderPort);
            this.guna2Panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2Panel2.Location = new System.Drawing.Point(0, 45);
            this.guna2Panel2.Name = "guna2Panel2";
            this.guna2Panel2.Size = new System.Drawing.Size(800, 405);
            this.guna2Panel2.TabIndex = 1;
            // 
            // tbModel
            // 
            this.tbModel.Controls.Add(this.guna2Panel6);
            this.tbModel.Location = new System.Drawing.Point(4, 22);
            this.tbModel.Name = "tbModel";
            this.tbModel.Padding = new System.Windows.Forms.Padding(3);
            this.tbModel.Size = new System.Drawing.Size(792, 379);
            this.tbModel.TabIndex = 7;
            this.tbModel.Text = "Model";
            this.tbModel.UseVisualStyleBackColor = true;
            // 
            // guna2Panel6
            // 
            this.guna2Panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2Panel6.Location = new System.Drawing.Point(3, 3);
            this.guna2Panel6.Name = "guna2Panel6";
            this.guna2Panel6.Size = new System.Drawing.Size(786, 373);
            this.guna2Panel6.TabIndex = 3;
            // 
            // frmControllersParameter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.guna2Panel2);
            this.Controls.Add(this.guna2Panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmControllersParameter";
            this.Text = "frmControllersParameter";
            this.Load += new System.EventHandler(this.frmControllersParameter_Load);
            this.guna2Panel1.ResumeLayout(false);
            this.tabFolderPort.ResumeLayout(false);
            this.tabOutputs.ResumeLayout(false);
            this.tabInputResults.ResumeLayout(false);
            this.tabInputSignal.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.guna2Panel2.ResumeLayout(false);
            this.tbModel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse1;
        private Guna.UI2.WinForms.Guna2DragControl guna2DragControl1;
        private Guna.UI2.WinForms.Guna2ControlBox guna2ControlBox1;
        private Guna.UI2.WinForms.Guna2ControlBox guna2ControlBox3;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel2;
        private System.Windows.Forms.TabControl tabFolderPort;
        private System.Windows.Forms.TabPage tabOutputs;
        private Guna.UI2.WinForms.Guna2Panel pnlOutputs;
        private System.Windows.Forms.TabPage tabInputResults;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel4;
        private System.Windows.Forms.TabPage tabInputSignal;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel5;
        private System.Windows.Forms.TabPage tabPage1;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel3;
        private System.Windows.Forms.TabPage tbModel;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel6;
    }
}