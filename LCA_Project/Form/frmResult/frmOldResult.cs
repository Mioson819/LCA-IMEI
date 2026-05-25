using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LCA_Project.Utilities;
using System.Runtime.InteropServices;
namespace LCA_Project.Form.frmResult
{
    public partial class frmOldResult : System.Windows.Forms.Form
    {
        private DrawOldResult _NG123;
        private DrawOldResult _NG4;
        private int _NX { get; set; }
             private int _NY { get; set; }
        private int _NYNG4 { get; set; }
        private List<Guna2Button> _oldResultNG123 {  get; set; }
        private List<Guna2Button> _oldResultNG4 { get; set; }
        private string NameStation { get; set; }    
        //public List<Guna2Button> OldResult
        //{
        //    get => _oldResult?.Select(x => new Guna2Button()
        //    {
        //        Text = x.Text,
        //        Name = x.Name,
        //        FillColor = x.FillColor,
        //        Dock = x.Dock,
        //        ForeColor = x.ForeColor
        //    }).ToList();
        //    set => _oldResult = value?.Select(x => new Guna2Button()
        //    {
        //        Text = x.Text,
        //        Name = x.Name,
        //        FillColor = x.FillColor,
        //        Dock = x.Dock,
        //        ForeColor = x.ForeColor
        //    }).ToList();
        //}
        public List<Guna2Button> OldResultNG123
        {
            get => _oldResultNG123;
            set => _oldResultNG123 = value?.Select(x => new Guna2Button()
            {
                Text = x.Text,
                Name = x.Name,
                FillColor = x.FillColor,
                Dock = x.Dock,
                ForeColor = x.ForeColor
            }).ToList();
        }
        public List<Guna2Button> OldResultNG4
        {
            get => _oldResultNG4;
            set => _oldResultNG4 = value?.Select(x => new Guna2Button()
            {
                Text = x.Text,
                Name = x.Name,
                FillColor = x.FillColor,
                Dock = x.Dock,
                ForeColor = x.ForeColor
            }).ToList();
        }
        public frmOldResult(int nX, int nY,int _nYNG4,string nameStation)
        {
            InitializeComponent();
            this.NameStation = nameStation;
            _NX = nX;
            _NY = nY;
            _NYNG4 = _nYNG4;
        }
        private void pnlNG4_Paint(object sender, PaintEventArgs e)
        {
        }
        private void pnlNG123_Paint(object sender, PaintEventArgs e)
        {
        }
        private void frmOldResult_Load(object sender, EventArgs e)
        {
            _NG123 = new DrawOldResult(this._NX, this._NY, this.OldResultNG123, this.NameStation);
            _NG4 = new DrawOldResult(2, this._NYNG4, this.OldResultNG4, this.NameStation);
            pnlNG123.Controls.Add(_NG123);
            pnlNG4.Controls.Add(_NG4);
        }
        public void addNG123(List<Guna2Button> _buttons)
        {
            pnlNG123.Controls.Add(_NG123);
        }
        public void addNG4(List<Guna2Button> _buttons)
        {
            pnlNG4.Controls.Add(_NG4);
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}
