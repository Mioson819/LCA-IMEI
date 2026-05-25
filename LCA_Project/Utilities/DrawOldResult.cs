using Guna.UI2.WinForms;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Utilities
{
    public class DrawOldResult : TableLayoutPanel
    {
        private string NameStation {  get; set; }
        public DrawOldResult(int nX, int nY, List<Guna2Button> _buttons,string NameStation)
        {
            if (nX == 0 || nY == 0 ||_buttons.Count<=0) return;
            this.NameStation = NameStation;
            InitializeController(nX, nY, _buttons);
            this.SizeChanged += UcButtonDisplayGrid_SizeChanged;
        }
        public void InitializeController(int nX, int nY, List<Guna2Button> _buttons)
        {
            this.Controls.Clear();
            this.ColumnStyles.Clear();
            this.RowStyles.Clear();
            bool vertical = false;
            if (!vertical)
            {
                int cols = nX;
                int rows = nY;
                this.ColumnCount = cols;
                this.RowCount = rows;
                //for (int i = 0; i < cols; i++)
                //{
                //    this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
                //}
                //for (int j = 0; j < rows; j++)
                //{
                //    this.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
                //}
                for (int i = 0; i < cols; i++)
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
                for (int j = 0; j < rows; j++)
                    this.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
                for (int i = 0; i < nX * nY; i++)
                {
                    int index = i;
                    if (this.NameStation == "Station1" || this.NameStation == "Station3")
                    {
                         this.Controls.Add(_buttons[index], index % cols, index / cols);
                    }
                    else
                    {
                        this.Controls.Add(_buttons[index], (cols - 1) - (index % cols), index / cols);
                    }
                }
                this.Dock = DockStyle.Fill;
                this.BackColor = System.Drawing.Color.WhiteSmoke;
                SetRowHeightsEvenly();
                SetColumnWidthsEvenly();
            }
        }
        private void SetRowHeightsEvenly()
        {
            int rowCount = this.RowCount;
            if (rowCount == 0) return;
            int totalHeight = this.Height;
            int baseHeight = totalHeight / rowCount;
            int remaining = totalHeight - (baseHeight * rowCount);
            for (int i = 0; i < rowCount; i++)
            {
                int thisRowHeight = baseHeight;
                if (i < remaining)
                    thisRowHeight += 1;
                this.RowStyles[i].SizeType = SizeType.Absolute;
                this.RowStyles[i].Height = thisRowHeight;
            }
        }
        private void SetColumnWidthsEvenly()
        {
            int colCount = this.ColumnCount;
            if (colCount == 0) return;
            int totalWidth = this.Width;
            int baseWidth = totalWidth / colCount;
            int remaining = totalWidth - (baseWidth * colCount);
            for (int i = 0; i < colCount; i++)
            {
                int thisColWidth = baseWidth;
                if (i < remaining)
                    thisColWidth += 1;
                this.ColumnStyles[i].SizeType = SizeType.Absolute;
                this.ColumnStyles[i].Width = thisColWidth;
            }
        }
        private void UcButtonDisplayGrid_SizeChanged(object sender, EventArgs e)
        {
            SetRowHeightsEvenly();
            SetColumnWidthsEvenly();
        }
    }
}
