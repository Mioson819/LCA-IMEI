namespace LCA_Project.Form.Teaching
{
    partial class frmCurPos
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
            this.guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2DragControl1 = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.guna2ControlBox2 = new Guna.UI2.WinForms.Guna2ControlBox();
            this.guna2ControlBox1 = new Guna.UI2.WinForms.Guna2ControlBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.CurPosX = new System.Windows.Forms.Label();
            this.CurPosY = new System.Windows.Forms.Label();
            this.CurPosZ = new System.Windows.Forms.Label();
            this.CurPosF = new System.Windows.Forms.Label();
            this.CurPosRI = new System.Windows.Forms.Label();
            this.CurPosRO = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // guna2Elipse1
            // 
            this.guna2Elipse1.TargetControl = this;
            // 
            // guna2DragControl1
            // 
            this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2DragControl1.TargetControl = this.label3;
            this.guna2DragControl1.UseTransparentDrag = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(52)))), ((int)(((byte)(88)))));
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.guna2ControlBox2);
            this.panel1.Controls.Add(this.guna2ControlBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(365, 45);
            this.panel1.TabIndex = 2;
            // 
            // guna2ControlBox2
            // 
            this.guna2ControlBox2.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox;
            this.guna2ControlBox2.Dock = System.Windows.Forms.DockStyle.Right;
            this.guna2ControlBox2.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(52)))), ((int)(((byte)(88)))));
            this.guna2ControlBox2.IconColor = System.Drawing.Color.White;
            this.guna2ControlBox2.Location = new System.Drawing.Point(275, 0);
            this.guna2ControlBox2.Name = "guna2ControlBox2";
            this.guna2ControlBox2.Size = new System.Drawing.Size(45, 45);
            this.guna2ControlBox2.TabIndex = 7;
            this.guna2ControlBox2.Click += new System.EventHandler(this.Guna2ControlBox2_Click);
            // 
            // guna2ControlBox1
            // 
            this.guna2ControlBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.guna2ControlBox1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(52)))), ((int)(((byte)(88)))));
            this.guna2ControlBox1.IconColor = System.Drawing.Color.White;
            this.guna2ControlBox1.Location = new System.Drawing.Point(320, 0);
            this.guna2ControlBox1.Name = "guna2ControlBox1";
            this.guna2ControlBox1.Size = new System.Drawing.Size(45, 45);
            this.guna2ControlBox1.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("Arial", 21.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.label3.ForeColor = System.Drawing.Color.Lime;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(275, 45);
            this.label3.TabIndex = 4;
            this.label3.Text = "Current Pos";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label3.Click += new System.EventHandler(this.Label3_Click);
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.BackColor = System.Drawing.Color.Silver;
            this.label39.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label39.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label39.Font = new System.Drawing.Font("Segoe UI", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label39.Location = new System.Drawing.Point(3, 0);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(176, 55);
            this.label39.TabIndex = 1;
            this.label39.Text = "Current Pos X ";
            this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Silver;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.CurPosRO, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.CurPosRI, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.CurPosF, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.CurPosZ, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.CurPosY, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.CurPosX, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label15, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label13, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label11, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label39, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 45);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(365, 331);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Silver;
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(3, 55);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(176, 55);
            this.label7.TabIndex = 3;
            this.label7.Text = "Current Pos Y";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.Silver;
            this.label9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(3, 110);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(176, 55);
            this.label9.TabIndex = 5;
            this.label9.Text = "Current Pos Z";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.Color.Silver;
            this.label11.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Font = new System.Drawing.Font("Segoe UI", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(3, 165);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(176, 55);
            this.label11.TabIndex = 7;
            this.label11.Text = "Current Pos F";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.Color.Silver;
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Font = new System.Drawing.Font("Segoe UI", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(3, 220);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(176, 55);
            this.label13.TabIndex = 9;
            this.label13.Text = "Current Pos RI";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.BackColor = System.Drawing.Color.Silver;
            this.label15.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Font = new System.Drawing.Font("Segoe UI", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(3, 275);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(176, 56);
            this.label15.TabIndex = 11;
            this.label15.Text = "Current Pos RO";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CurPosX
            // 
            this.CurPosX.AutoSize = true;
            this.CurPosX.BackColor = System.Drawing.Color.WhiteSmoke;
            this.CurPosX.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CurPosX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurPosX.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.CurPosX.Location = new System.Drawing.Point(185, 0);
            this.CurPosX.Name = "CurPosX";
            this.CurPosX.Size = new System.Drawing.Size(177, 55);
            this.CurPosX.TabIndex = 41;
            this.CurPosX.Text = "0.00";
            this.CurPosX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CurPosX.Click += new System.EventHandler(this.CurPosX_Click);
            // 
            // CurPosY
            // 
            this.CurPosY.AutoSize = true;
            this.CurPosY.BackColor = System.Drawing.Color.WhiteSmoke;
            this.CurPosY.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CurPosY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurPosY.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.CurPosY.Location = new System.Drawing.Point(185, 55);
            this.CurPosY.Name = "CurPosY";
            this.CurPosY.Size = new System.Drawing.Size(177, 55);
            this.CurPosY.TabIndex = 42;
            this.CurPosY.Text = "0.00";
            this.CurPosY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CurPosY.Click += new System.EventHandler(this.Label2_Click);
            // 
            // CurPosZ
            // 
            this.CurPosZ.AutoSize = true;
            this.CurPosZ.BackColor = System.Drawing.Color.WhiteSmoke;
            this.CurPosZ.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CurPosZ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurPosZ.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.CurPosZ.Location = new System.Drawing.Point(185, 110);
            this.CurPosZ.Name = "CurPosZ";
            this.CurPosZ.Size = new System.Drawing.Size(177, 55);
            this.CurPosZ.TabIndex = 43;
            this.CurPosZ.Text = "0.00";
            this.CurPosZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CurPosF
            // 
            this.CurPosF.AutoSize = true;
            this.CurPosF.BackColor = System.Drawing.Color.WhiteSmoke;
            this.CurPosF.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CurPosF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurPosF.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.CurPosF.Location = new System.Drawing.Point(185, 165);
            this.CurPosF.Name = "CurPosF";
            this.CurPosF.Size = new System.Drawing.Size(177, 55);
            this.CurPosF.TabIndex = 44;
            this.CurPosF.Text = "0.00";
            this.CurPosF.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CurPosF.Click += new System.EventHandler(this.Label5_Click);
            // 
            // CurPosRI
            // 
            this.CurPosRI.AutoSize = true;
            this.CurPosRI.BackColor = System.Drawing.Color.WhiteSmoke;
            this.CurPosRI.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CurPosRI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurPosRI.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.CurPosRI.Location = new System.Drawing.Point(185, 220);
            this.CurPosRI.Name = "CurPosRI";
            this.CurPosRI.Size = new System.Drawing.Size(177, 55);
            this.CurPosRI.TabIndex = 45;
            this.CurPosRI.Text = "0.00";
            this.CurPosRI.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CurPosRO
            // 
            this.CurPosRO.AutoSize = true;
            this.CurPosRO.BackColor = System.Drawing.Color.WhiteSmoke;
            this.CurPosRO.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CurPosRO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurPosRO.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.CurPosRO.Location = new System.Drawing.Point(185, 275);
            this.CurPosRO.Name = "CurPosRO";
            this.CurPosRO.Size = new System.Drawing.Size(177, 56);
            this.CurPosRO.TabIndex = 46;
            this.CurPosRO.Text = "0.00";
            this.CurPosRO.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CurPosRO.Click += new System.EventHandler(this.CurPosRO_Click);
            // 
            // frmCurPos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 376);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmCurPos";
            this.Text = "frmCurPos";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmCurPos_FormClosing);
            this.Load += new System.EventHandler(this.FrmCurPos_Load);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
        }
        #endregion
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse1;
        private Guna.UI2.WinForms.Guna2DragControl guna2DragControl1;
        private System.Windows.Forms.Panel panel1;
        private Guna.UI2.WinForms.Guna2ControlBox guna2ControlBox2;
        private Guna.UI2.WinForms.Guna2ControlBox guna2ControlBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label CurPosRO;
        private System.Windows.Forms.Label CurPosRI;
        private System.Windows.Forms.Label CurPosF;
        private System.Windows.Forms.Label CurPosZ;
        private System.Windows.Forms.Label CurPosY;
        private System.Windows.Forms.Label CurPosX;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label39;
    }
}