using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Form.Resources
{
    public  class frmResources : System.Windows.Forms.Form
    {
        private Guna2Elipse Guna2Elipse;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Guna2PictureBox guna2PictureBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
       // private System.Windows.Forms.ListBox ListBox;
        private Guna2GradientButton btnAdd;
        private System.ComponentModel.IContainer components;
        private Guna2GradientButton btnImport;
        private ListBox listBox1;
        private Guna2ControlBox Guna2ControlBox;
        private Guna2DragControl guna2DragControl1;
        public delegate void imgselected(string name);
        public imgselected onimgselected;
        public frmResources()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.guna2DragControl1 = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.guna2PictureBox1 = new Guna.UI2.WinForms.Guna2PictureBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnAdd = new Guna.UI2.WinForms.Guna2GradientButton();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.Guna2ControlBox = new Guna.UI2.WinForms.Guna2ControlBox();
            this.Guna2Elipse = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.guna2PictureBox1)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // guna2DragControl1
            // 
            this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2DragControl1.TargetControl = this.tableLayoutPanel1;
            this.guna2DragControl1.UseTransparentDrag = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Gray;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.guna2PictureBox1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.Guna2ControlBox, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(626, 470);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // guna2PictureBox1
            // 
            this.guna2PictureBox1.BorderRadius = 6;
            this.guna2PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2PictureBox1.ImageRotate = 0F;
            this.guna2PictureBox1.Location = new System.Drawing.Point(316, 50);
            this.guna2PictureBox1.Name = "guna2PictureBox1";
            this.guna2PictureBox1.Size = new System.Drawing.Size(307, 417);
            this.guna2PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.guna2PictureBox1.TabIndex = 0;
            this.guna2PictureBox1.TabStop = false;
            this.guna2PictureBox1.Click += new System.EventHandler(this.guna2PictureBox1_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnAdd, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.listBox1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 50);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(307, 417);
            this.tableLayoutPanel2.TabIndex = 1;
            this.tableLayoutPanel2.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel2_Paint);
            // 
            // btnAdd
            // 
            this.btnAdd.BorderRadius = 18;
            this.btnAdd.CustomizableEdges.BottomRight = false;
            this.btnAdd.CustomizableEdges.TopRight = false;
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(44)))), ((int)(((byte)(54)))));
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI Semilight", 20.25F, System.Drawing.FontStyle.Italic);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(156, 336);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(148, 78);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // listBox1
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.listBox1, 2);
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(3, 3);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(301, 327);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // Guna2ControlBox
            // 
            this.Guna2ControlBox.BackColor = System.Drawing.Color.Transparent;
            this.Guna2ControlBox.BorderColor = System.Drawing.Color.Gray;
            this.Guna2ControlBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.Guna2ControlBox.FillColor = System.Drawing.Color.Gray;
            this.Guna2ControlBox.IconColor = System.Drawing.Color.White;
            this.Guna2ControlBox.Location = new System.Drawing.Point(578, 3);
            this.Guna2ControlBox.Name = "Guna2ControlBox";
            this.Guna2ControlBox.Size = new System.Drawing.Size(45, 41);
            this.Guna2ControlBox.TabIndex = 2;
            this.Guna2ControlBox.UseTransparentBackground = true;
            // 
            // Guna2Elipse
            // 
            this.Guna2Elipse.TargetControl = this;
            // 
            // frmResources
            // 
            this.ClientSize = new System.Drawing.Size(626, 470);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmResources";
            this.Load += new System.EventHandler(this.frmResources_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.guna2PictureBox1)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private void LoadResourceList()
        {
            listBox1.Items.Clear();
            var res = LCA_Project.Properties.Resources.ResourceManager;
            var set = res.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            foreach (System.Collections.DictionaryEntry entry in set)
            {
                // Chỉ add key là image/bitmap/icon vào ListBox
                if (entry.Value is Bitmap || entry.Value is Image || entry.Value is Icon)
                {
                    listBox1.Items.Add(entry.Key.ToString());
                }
            }
        }
        private void frmResources_Load(object sender, EventArgs e)
        {
            LoadResourceList();
        }
        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
        }
        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }
        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = listBox1.SelectedItem?.ToString();
            if (name != null)
            {
                var obj = LCA_Project.Properties.Resources.ResourceManager.GetObject(name);
                if (obj is Bitmap bmp)
                    guna2PictureBox1.Image = bmp;
                else if (obj is Image img)
                    guna2PictureBox1.Image = img;
                else if (obj is Icon icon)
                    guna2PictureBox1.Image = icon.ToBitmap();
                else
                    guna2PictureBox1.Image = null;
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            onimgselected?.Invoke(listBox1.SelectedItem?.ToString());
            this.Close();
        }
    }
}
