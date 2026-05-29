using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Utilities;
using LCA_Samsung.Process;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LCA_Project.Form.Signal
{
    public partial class frmSignal : System.Windows.Forms.Form
    {
        private Dictionary<string, ButtonSignal<Guna2CircleButton>> _controller
            = new Dictionary<string, ButtonSignal<Guna2CircleButton>>();

        private string _name { get; set; }

        // Map Name của TabPage (từ Designer) → tên station
        private readonly Dictionary<string, string> _tabToStation = new Dictionary<string, string>
        {
            { "Port1", "Station1" },
            { "Port2", "Station2" },
            { "Port3", "Station3" },
            { "Port4", "Station4" },
        };

        // Station đang được đọc — dùng để StopCurrentTab chỉ dừng đúng 1 station
        private string _activeStation = null;

        public frmSignal(Dictionary<string, ButtonSignal<Guna2CircleButton>> controller, string name)
        {
            InitializeComponent();
            _controller = controller;
            _name = name;

            flpp1in.AutoScroll = true;
            flpp2in.AutoScroll = true;
            flpp3in.AutoScroll = true;
            flpp4in.AutoScroll = true;
            flpp1out.AutoScroll = true;
            flpp2out.AutoScroll = true;
            flpp3out.AutoScroll = true;
            flpp4out.AutoScroll = true;
            flpp1in.FlowDirection = FlowDirection.LeftToRight;

            this.Load += frmSignal_Load;
        }

        private void frmSignal_Load(object sender, EventArgs e)
        {
            // Format cột Name trong DB: "<TênBiến>_Input_Station1" hoặc "<TênBiến>_Output_Station2"
            // Ví dụ: "SpReady_Input_Station1", "LoadNG_Output_Station4"
            // Dùng Contains để match bất kể vị trí — dễ mở rộng khi DB thêm cột mới
            var registers = DatabaseControllers.Instance.GetRegister_ControllerParameterInputs();

            // Build lookup: địa chỉ PLC (register.Value) → register.Key (tên signal)
            // Để tránh vòng lặp O(n²) khi _controller lớn
            var addressToName = new Dictionary<string, string>();
            foreach (var register in registers)
            {
                if (!addressToName.ContainsKey(register.Value))
                    addressToName[register.Value] = register.Key;
            }

            foreach (var item in _controller)
            {
                // item.Key = địa chỉ PLC (register.Value), item.Value = button
                if (!addressToName.TryGetValue(item.Key, out string signalName))
                    continue;   // địa chỉ này không có trong DB — bỏ qua

                // Xác định loại (Input / Output) từ tên signal
                // Format: "<Tên>_Input_StationN" hoặc "<Tên>_Output_StationN"
                bool isInput = signalName.Contains("_Input_");
                bool isOutput = signalName.Contains("_Output_");

                // Nếu DB chưa update (chưa có _Input_/_Output_) → mặc định vào Input
                // để button vẫn hiển thị thay vì ẩn hoàn toàn
                if (!isInput && !isOutput)
                    isInput = true;

                // Phân loại theo station và thêm vào đúng FlowLayoutPanel
                if (signalName.Contains("Station1"))
                {
                    if (isInput) flpp1in.Controls.Add(item.Value);
                    else flpp1out.Controls.Add(item.Value);
                }
                else if (signalName.Contains("Station2"))
                {
                    if (isInput) flpp2in.Controls.Add(item.Value);
                    else flpp2out.Controls.Add(item.Value);
                }
                else if (signalName.Contains("Station3"))
                {
                    if (isInput) flpp3in.Controls.Add(item.Value);
                    else flpp3out.Controls.Add(item.Value);
                }
                else if (signalName.Contains("Station4"))
                {
                    if (isInput) flpp4in.Controls.Add(item.Value);
                    else flpp4out.Controls.Add(item.Value);
                }
                else
                {
                    // Tên signal không chứa StationN → log để debug
                    LogProgram.WriteLog($"[Signal] Không xác định được station cho signal '{signalName}' (addr={item.Key})");
                }
            }

            // BUG 4 FIX: đăng ký event SAU khi load xong — tránh fire sớm khi
            // TabControl vẽ lại trong quá trình khởi tạo form
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;

            // Bắt đầu đọc tab đang active (mặc định Port1 = Station1)
            StartReadCurrentTab();
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // BUG 5 FIX: chỉ dừng đúng station đang đọc, không dừng hết 4 station
            StopActiveStation();
            StartReadCurrentTab();
        }

        private void StartReadCurrentTab()
        {
            string tabName = tabControl1.SelectedTab?.Name;
            if (tabName == null) return;
            if (!_tabToStation.TryGetValue(tabName, out string station)) return;

            _activeStation = station;
            ProcessMain.Instance.StartReadStation(_name, station);
        }

        // BUG 5 FIX: chỉ dừng station đang active thay vì vòng lặp dừng hết 4
        private void StopActiveStation()
        {
            if (_activeStation != null)
            {
                ProcessMain.Instance.StopReadStation(_activeStation);
                _activeStation = null;
            }
        }

        private void flpSignal_Paint(object sender, PaintEventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }

        private void frmSignal_FormClosing(object sender, FormClosingEventArgs e)
        {
            // KHÔNG gọi Controls.Clear() — button thuộc ProcessMain._Signal
            // Chỉ dừng vòng đọc PLC đang chạy
            ProcessMain.Instance.StopAllSignalRead();
            _activeStation = null;
        }

        private void guna2ResizeBox1_Click(object sender, EventArgs e) { }
    }
}