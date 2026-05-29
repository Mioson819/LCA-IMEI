using DocumentFormat.OpenXml.Office.Word;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Form.Devices.Controllers;
using LCA_Project.Form.Resources;
using LCA_Project.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Forms;
namespace LCA_Project.Services.Controllers
{
    public class DrawDashboard
    {
        private TableLayoutPanel tableLayoutPanel;
        private Guna2GradientButton btnAddRegister;
        private Guna2GradientButton btnRemoveRegisters;
        private Guna2DataGridView dgvtParameter;
        private Guna2GradientButton btnSave;
        private Guna2GradientButton btnFix;
        private Guna2GradientButton btnSetting;
        private frmControllersParameter _frmControllersParameter;
        private FrmSettingController _frmSettingController;
        private frmResources _frmResources;
        private TypeDatabase _typeDatabase;
        public delegate void sendController(System.Windows.Forms.Control control);
        public event sendController onSendController;
        private frmControllers frmControllers;
        private string _condition { get; set; }
        private int indexRows { get; set; }
        private int indexCell { get; set; }
        private string _nameDatabase { get; set; }

        // Giá trị hợp lệ cho cột PcType — thêm/bớt ở đây nếu sau này có loại mới
        private static readonly string[] PcTypeValues = { "Nano", "Pamtech" };

        public DrawDashboard(string NameDatabase, Guna2Panel panel)
        {
            CreateDashboard(NameDatabase, panel, null);
            _nameDatabase = NameDatabase;
        }
        public DrawDashboard(string NameDatabase, Guna2Panel panel, string condition)
        {
            CreateDashboard(NameDatabase, panel, condition);
            _nameDatabase = NameDatabase;
        }
        private void CreateDashboard(string NameDatabase, Guna2Panel panel, string condition)
        {
            panel.Controls.Clear();
            _condition = condition;
            tableLayoutPanel = new TableLayoutPanel();
            btnAddRegister = new Guna2GradientButton();
            btnRemoveRegisters = new Guna2GradientButton();
            dgvtParameter = new Guna2DataGridView();
            btnSave = new Guna2GradientButton();
            btnFix = null;
            btnSetting = null;
            // Xác định loại database
            if (Enum.TryParse(NameDatabase, out TypeDatabase typeDatabase))
            {
                _typeDatabase = typeDatabase;
                switch (typeDatabase)
                {
                    case TypeDatabase.Controllers:
                        btnFix = new Guna2GradientButton();
                        break;
                    case TypeDatabase.ControllerTag:
                        dgvtParameter.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dgvtParameter.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                        break;
                    case TypeDatabase.ControllerParameterInputs:
                        break;
                    case TypeDatabase.ControllerSetting:
                        dgvtParameter.CellDoubleClick += (s, ev) =>
                        {
                            if (ev.RowIndex >= 0 && ev.ColumnIndex == 2)
                            {
                                _frmResources = new frmResources();
                                _frmResources.Show();
                                indexRows = ev.RowIndex;
                                indexCell = ev.ColumnIndex;
                                _frmResources.onimgselected += (img) =>
                                {
                                    dgvtParameter.Rows[indexRows].Cells[indexCell].Value = img;
                                };
                            }
                        };
                        btnSave.Click += sendControls;
                        break;
                    case TypeDatabase.ControllerParameterInputsResults:
                        dgvtParameter.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dgvtParameter.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                        break;
                    case TypeDatabase.ControllerTableSetting:
                        btnSetting = new Guna2GradientButton();
                        break;
                    case TypeDatabase.FolderPort:
                        break;
                }
            }
            panel.Dock = DockStyle.Fill;
            dgvtParameter.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
            dgvtParameter.SelectionMode = DataGridViewSelectionMode.CellSelect;
            btnSave.FillColor = Color.White;
            btnAddRegister.FillColor = Color.White;
            btnRemoveRegisters.FillColor = Color.White;
            btnAddRegister.FillColor2 = Color.White;
            btnRemoveRegisters.FillColor2 = Color.White;
            btnSave.FillColor2 = Color.White;
            btnSave.HoverState.FillColor = Color.FromArgb(241, 84, 127);
            btnAddRegister.HoverState.FillColor = Color.FromArgb(241, 84, 127);
            btnAddRegister.HoverState.FillColor2 = Color.White;
            btnRemoveRegisters.HoverState.FillColor = Color.FromArgb(241, 84, 127);
            btnRemoveRegisters.HoverState.FillColor2 = Color.White;
            btnSave.HoverState.FillColor2 = Color.White;
            btnAddRegister.CustomizableEdges.BottomRight = false;
            btnAddRegister.CustomizableEdges.TopRight = false;
            btnRemoveRegisters.CustomizableEdges.BottomRight = false;
            btnRemoveRegisters.CustomizableEdges.TopRight = false;
            panel.Controls.Add(tableLayoutPanel);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 92F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel.Controls.Add(btnAddRegister, 1, 0);
            tableLayoutPanel.Controls.Add(btnRemoveRegisters, 1, 1);
            tableLayoutPanel.Controls.Add(btnSave, 1, 2);
            if (btnFix != null)
            {
                btnFix.FillColor = Color.White;
                btnFix.FillColor2 = Color.White;
                btnFix.HoverState.FillColor = Color.FromArgb(241, 84, 127);
                btnFix.HoverState.FillColor2 = Color.White;
                btnFix.Dock = DockStyle.Fill;
                btnFix.Image = Properties.Resources.Menu_Tools;
                btnFix.ImageSize = new Size(35, 35);
                btnFix.ImageAlign = HorizontalAlignment.Center;
                tableLayoutPanel.Controls.Add(btnFix, 1, 3);
                btnFix.Click += FixDatabase;
            }
            btnSave.Dock = DockStyle.Fill;
            btnAddRegister.Dock = DockStyle.Fill;
            btnRemoveRegisters.Dock = DockStyle.Fill;
            btnSave.Image = Properties.Resources.Button_Save;
            btnAddRegister.Image = Properties.Resources.Button_Add;
            btnRemoveRegisters.Image = Properties.Resources.Button_Minus;
            btnAddRegister.ImageSize = new Size(35, 35);
            btnRemoveRegisters.ImageSize = new Size(35, 35);
            btnSave.ImageSize = new Size(35, 35);
            btnSave.ImageAlign = HorizontalAlignment.Center;
            btnAddRegister.ImageAlign = HorizontalAlignment.Center;
            btnRemoveRegisters.ImageAlign = HorizontalAlignment.Center;
            dgvtParameter.AllowUserToAddRows = false;
            tableLayoutPanel.Controls.Add(dgvtParameter, 0, 0);
            tableLayoutPanel.SetRowSpan(dgvtParameter, 5);
            dgvtParameter.Dock = DockStyle.Fill;
            dgvtParameter.DataError += (s, e) => { e.ThrowException = false; };
            Database.DatabaseControllers.Instance.LoadDatabase(dgvtParameter, NameDatabase, null);
            if (_typeDatabase == TypeDatabase.FolderPort)
            {
                SetupFolderPortVisualCombos();
            }
            else if (_typeDatabase == TypeDatabase.Model)
            {
                SetupModel();
                dgvtParameter.EditingControlShowing -= Changed;
                dgvtParameter.EditingControlShowing += Changed;
                dgvtParameter.CellClick += dgvtParameter_CellClick;
            }
            else if (_typeDatabase == TypeDatabase.ControllerTag)
            {
                // PcType cũng xuất hiện trong bảng ControllerTag
                SetupPcTypeCombo();
            }
            btnAddRegister.Click += AddRegister;
            btnRemoveRegisters.Click += RemoveRegisters;
            btnSave.Click += SaveDatabase;
        }

        // ── PcType ComboBox ──────────────────────────────────────────────────────
        // Áp dụng cho bất kỳ bảng nào có cột "PcType" (FolderPort, ControllerTag)
        // Gọi sau khi LoadDatabase() đã bind DataSource
        private void SetupPcTypeCombo()
        {
            if (!dgvtParameter.Columns.Contains("PcType")) return;

            int index = dgvtParameter.Columns["PcType"].Index;
            dgvtParameter.Columns.Remove("PcType");

            var col = new DataGridViewComboBoxColumn();
            col.Name = "PcType";
            col.HeaderText = "PcType";
            col.DataPropertyName = "PcType";
            col.FlatStyle = FlatStyle.Flat;
            col.Items.AddRange(PcTypeValues);   // "Nano", "Pamtech"

            dgvtParameter.Columns.Insert(index, col);
        }

        private void dgvtParameter_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvtParameter.Columns[dgvtParameter.CurrentCell.ColumnIndex].Name;
            DataGridViewComboBoxCell cell =
                (DataGridViewComboBoxCell)dgvtParameter.CurrentCell;
            string port = null;
            if (colName == "ModelNamePortLeft")
            {
                port = dgvtParameter.Rows[dgvtParameter.CurrentCell.RowIndex]
                           .Cells["PortLeft"].Value?.ToString();
            }
            else if (colName == "ModelNamePortRight")
            {
                port = dgvtParameter.Rows[dgvtParameter.CurrentCell.RowIndex]
                           .Cells["PortRight"].Value?.ToString();
            }
            if (string.IsNullOrEmpty(port))
                return;
            DatabaseControllers.Instance.LoadDataNameModel2(cell, port);
        }
        private void SetupModel()
        {
            List<string> list1 = new List<string>(), list2 = new List<string>();
            DataTable dt = dgvtParameter.DataSource as DataTable;
            if (dt == null) return;
            int indexPortLeft = dgvtParameter.Columns["PortLeft"].Index, indexPortRight = dgvtParameter.Columns["PortRight"].Index, indexModelNamePortLeft = dgvtParameter.Columns["ModelNamePortLeft"].Index, indexModelNamePortRight = dgvtParameter.Columns["ModelNamePortRight"].Index, indexIdMODEL = dgvtParameter.Columns["IdModel"].Index;
            dgvtParameter.Columns.Remove("PortLeft");
            dgvtParameter.Columns.Remove("PortRight");
            dgvtParameter.Columns.Remove("ModelNamePortLeft");
            dgvtParameter.Columns.Remove("ModelNamePortRight");
            dgvtParameter.Columns.Remove("IdModel");
            var col = new DataGridViewComboBoxColumn();
            col.Name = "IdModel";
            col.HeaderText = "IdModel";
            col.DataPropertyName = "IdModel";
            col.FlatStyle = FlatStyle.Flat;
            var numbers = Enumerable.Range(0, 100).ToList();
            foreach (var n in numbers)
            {
                col.Items.Add(n);
            }
            var PortLeft = new DataGridViewComboBoxColumn();
            PortLeft.Name = "PortLeft";
            PortLeft.HeaderText = "PortLeft";
            PortLeft.DataPropertyName = "PortLeft";
            PortLeft.FlatStyle = FlatStyle.Flat;
            PortLeft.Items.AddRange("Port1", "Port3");
            var PortRight = new DataGridViewComboBoxColumn();
            PortRight.Name = "PortRight";
            PortRight.HeaderText = "PortRight";
            PortRight.DataPropertyName = "PortRight";
            PortRight.FlatStyle = FlatStyle.Flat;
            PortRight.Items.AddRange("Port2", "Port4");
            DatabaseControllers.Instance.LoadDataNameModel3(list1, "Port1");
            DatabaseControllers.Instance.LoadDataNameModel3(list1, "Port2");
            DatabaseControllers.Instance.LoadDataNameModel3(list2, "Port3");
            DatabaseControllers.Instance.LoadDataNameModel3(list2, "Port4");
            var ModelNamePortLeft = new DataGridViewComboBoxColumn();
            ModelNamePortLeft.Name = "ModelNamePortLeft";
            ModelNamePortLeft.HeaderText = "ModelNamePortLeft";
            ModelNamePortLeft.DataPropertyName = "ModelNamePortLeft";
            ModelNamePortLeft.FlatStyle = FlatStyle.Flat;
            foreach (var n in list1)
            {
                ModelNamePortLeft.Items.Add(n);
            }
            var ModelNamePortRight = new DataGridViewComboBoxColumn();
            ModelNamePortRight.Name = "ModelNamePortRight";
            ModelNamePortRight.HeaderText = "ModelNamePortRight";
            ModelNamePortRight.DataPropertyName = "ModelNamePortRight";
            ModelNamePortRight.FlatStyle = FlatStyle.Flat;
            foreach (var n in list2)
            {
                ModelNamePortRight.Items.Add(n);
            }
            dgvtParameter.Columns.Insert(1, col);
            dgvtParameter.Columns.Insert(2, PortLeft);
            dgvtParameter.Columns.Insert(3, PortRight);
            dgvtParameter.Columns.Insert(4, ModelNamePortLeft);
            dgvtParameter.Columns.Insert(5, ModelNamePortRight);
        }
        private void Changed(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ComboBox cb = e.Control as ComboBox;
            string columnName = dgvtParameter.Columns[dgvtParameter.CurrentCell.ColumnIndex].Name;
            if (cb != null)
            {
                if (columnName.IndexOf("ModelName", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                }
                else if (columnName.IndexOf("Port", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    cb.SelectedIndexChanged -= CategoryChanged;
                    cb.SelectedIndexChanged += CategoryChanged;
                }
            }
        }
        private void CategoryChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null || dgvtParameter.CurrentCell.ColumnIndex + 2 >= dgvtParameter.ColumnCount) return;
            string seleted = cb.SelectedItem.ToString();
            DataGridViewComboBoxCell cellProduct = (DataGridViewComboBoxCell)dgvtParameter.Rows[dgvtParameter.CurrentCell.RowIndex].Cells[dgvtParameter.CurrentCell.ColumnIndex + 2];
            DatabaseControllers.Instance.LoadDataNameModel2(cellProduct, seleted);
        }
        private void SetupFolderPortVisualCombos()
        {
            DataTable dt = dgvtParameter.DataSource as DataTable;
            if (dt == null) return;
            int index = dgvtParameter.Columns["IdPort"].Index;
            int indexnX = dgvtParameter.Columns["nX"].Index;
            int indexnY = dgvtParameter.Columns["nY"].Index;
            int indexnYNG4 = dgvtParameter.Columns["nYNG4"].Index;
            int indexMODEL = dgvtParameter.Columns["IdMODEL"].Index;
            dgvtParameter.Columns.Remove("IdPort");
            dgvtParameter.Columns.Remove("nX");
            dgvtParameter.Columns.Remove("nY");
            dgvtParameter.Columns.Remove("nYNG4");
            dgvtParameter.Columns.Remove("IdMODEL");
            var col = new DataGridViewComboBoxColumn();
            col.Name = "IdPort";
            col.HeaderText = "IdPort";
            col.DataPropertyName = "IdPort";
            col.FlatStyle = FlatStyle.Flat;
            col.Items.AddRange("Port1", "Port2", "Port3", "Port4");
            dgvtParameter.Columns.Insert(index, col);
            var numbers = Enumerable.Range(1, 100).ToList();
            var nX = new DataGridViewComboBoxColumn();
            nX.Name = "nX";
            nX.HeaderText = "nX";
            nX.DataPropertyName = "nX";
            nX.FlatStyle = FlatStyle.Flat;
            var nY = new DataGridViewComboBoxColumn();
            nY.Name = "nY";
            nY.HeaderText = "nY";
            nY.DataPropertyName = "nY";
            nY.FlatStyle = FlatStyle.Flat;
            var NG4 = new DataGridViewComboBoxColumn();
            NG4.Name = "nYNG4";
            NG4.HeaderText = "nYNG4";
            NG4.DataPropertyName = "nYNG4";
            NG4.FlatStyle = FlatStyle.Flat;
            var IdModel = new DataGridViewComboBoxColumn();
            IdModel.Name = "IdMODEL";
            IdModel.HeaderText = "IdMODEL";
            IdModel.DataPropertyName = "IdMODEL";
            IdModel.FlatStyle = FlatStyle.Flat;
            foreach (var n in numbers)
            {
                IdModel.Items.Add(n);
                nX.Items.Add(n);
                nY.Items.Add(n);
                NG4.Items.Add(n);
            }
            dgvtParameter.Columns.Insert(indexnX, nX);
            dgvtParameter.Columns.Insert(indexnY, nY);
            dgvtParameter.Columns.Insert(indexnYNG4, NG4);
            dgvtParameter.Columns.Insert(indexMODEL, IdModel);

            // PcType ComboBox — thêm sau khi các cột số đã insert xong
            SetupPcTypeCombo();
        }
        private void AddRegister(object sender, EventArgs e)
        {
            DataTable dataTable = (DataTable)dgvtParameter.DataSource;
            DataRow newRow = dataTable.NewRow();
            if (_typeDatabase != TypeDatabase.Controllers && _condition != null && _condition != "" && _typeDatabase != TypeDatabase.ControllerTag && _typeDatabase != TypeDatabase.ControllerParameterInputsResults && _typeDatabase != TypeDatabase.FolderPort && _typeDatabase != TypeDatabase.Model)
            {
                newRow[0] = _condition;
            }
            dataTable.Rows.Add(newRow);
        }
        private void RemoveRegisters(object sender, EventArgs e)
        {
            if (dgvtParameter.CurrentRow != null && !dgvtParameter.CurrentRow.IsNewRow)
            {
                dgvtParameter.Rows.Remove(dgvtParameter.CurrentRow);
            }
            else
            {
                MessageBox.Show("Select a valid row to delete");
            }
        }
        private void SaveDatabase(object sender, EventArgs e)
        {
            dgvtParameter.EndEdit();
            Database.DatabaseControllers.Instance.UpdateDatabase(dgvtParameter, _nameDatabase);
        }
        private void FixDatabase(object sender, EventArgs e)
        {
            _condition = dgvtParameter.CurrentRow.Cells[1].Value.ToString();
            _frmControllersParameter = new frmControllersParameter(_condition);
            _frmControllersParameter.Show();
        }
        private void SettingDatabase(object sender, EventArgs e)
        {
        }
        private void sendControls(object sender, EventArgs e)
        {
            onSendController?.Invoke(new ButtonSignal<Guna2Button>());
        }
    }
    public enum TypeDatabase
    {
        FolderPort,
        Model,
        Controllers,
        ControllerTag,
        ControllerParameterInputs,
        ControllerSetting,
        ControllerTableSetting,
        ControllerParameterInputsResults,
        ControllerParameterInputsSignal
    }
}