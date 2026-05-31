using Guna.UI2.AnimatorNS;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Form.Signal;
using LCA_Project.Services.Controllers;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Guna.UI2.Native.WinApi;
using LCA_Project.Utilities;
using System.Collections;
using System.ComponentModel;
namespace LCA_Samsung.Process
{
    public class ProcessMain
    {
        public Dictionary<string, Dictionary<string, ButtonSignal<Guna2CircleButton>>> _Signal
            = new Dictionary<string, Dictionary<string, ButtonSignal<Guna2CircleButton>>>();
        // Map: station ("Station1".."Station4") → danh sách địa chỉ PLC
        // Build từ register.Key (chứa tên station) — không dùng NameSignal.get
        private readonly Dictionary<string, List<string>> _stationAddressMap
            = new Dictionary<string, List<string>>();
        private static readonly object _lock = new object();
        private static ProcessMain _instance;
        // Mỗi station có CTS riêng — dừng đúng station, không ảnh hưởng station khác
        private readonly Dictionary<string, CancellationTokenSource> _ctsMap
            = new Dictionary<string, CancellationTokenSource>();
        // Giữ tương thích với code cũ còn tham chiếu _cts
        public CancellationTokenSource _cts => _ctsMap.Values.FirstOrDefault();
        public static ProcessMain Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new ProcessMain();
                    return _instance;
                }
            }
        }
        private ProcessMain() { }
        public async Task Initialize()
        {
            await InitializeSignalTable();
        }
        private async Task InitializeSignalTable()
        {
            StopAllSignalRead();
            _Signal.Clear();
            _stationAddressMap.Clear();
            // BUG 1 FIX: gọi DB một lần duy nhất, dùng lại cho cả _stationAddressMap và _Signal
            var registers = DatabaseControllers.Instance.GetRegister_ControllerParameterInputs();
            // Build _stationAddressMap từ register.Key (chứa "Station1".."Station4")
            // register.Key   = tên signal, vd "Station1_Input_X"
            // register.Value = địa chỉ PLC, vd "DM100.1"
            foreach (var register in registers)
            {
                string stationKey = null;
                if (register.Key.Contains("Station1")) stationKey = "Station1";
                else if (register.Key.Contains("Station2")) stationKey = "Station2";
                else if (register.Key.Contains("Station3")) stationKey = "Station3";
                else if (register.Key.Contains("Station4")) stationKey = "Station4";
                if (stationKey == null) continue;
                if (!_stationAddressMap.ContainsKey(stationKey))
                    _stationAddressMap[stationKey] = new List<string>();
                if (!_stationAddressMap[stationKey].Contains(register.Value))
                    _stationAddressMap[stationKey].Add(register.Value);
            }
            // Build _Signal — mỗi controller dùng cùng tập registers đã đọc
            List<Task> allTasks = new List<Task>();
            foreach (var kv in ControllerServices.Instance.Controllers)
            {
                string controllerKey = kv.Key;
                // Capture registers vào closure để tránh gọi DB lần 2
                var registersCopy = registers;
                Task runInitialize = Task.Run(() =>
                {
                    var signalForController = new Dictionary<string, ButtonSignal<Guna2CircleButton>>();
                    foreach (var register in registersCopy)
                    {
                        if (signalForController.ContainsKey(register.Value)) continue;
                        signalForController.Add(
                            register.Value,
                            new ButtonSignal<Guna2CircleButton>
                            {
                                NameSignal = register.Key,
                                RegisterSignal = register.Value
                            });
                    }
                    lock (_lock)
                    {
                        if (!_Signal.ContainsKey(controllerKey))
                            _Signal.Add(controllerKey, signalForController);
                    }
                });
                allTasks.Add(runInitialize);
            }
            await Task.WhenAll(allTasks);
        }
        // Đọc signal chỉ cho station đang hiển thị
        // controllerName: key trong _Signal (vd "LCA")
        // station: "Station1" | "Station2" | "Station3" | "Station4"
        public void StartReadStation(string controllerName, string station)
        {
            StopReadStation(station);
            // BUG 2 FIX: _Signal được build với key = controllerName từ ControllerServices
            // btnAlarm_Click dùng key "LCA" — phải khớp với Name trong bảng Controllers DB
            if (!_Signal.TryGetValue(controllerName, out var signalDict) || signalDict == null)
            {
                LogProgram.WriteLog($"[Signal] Controller '{controllerName}' không tìm thấy trong _Signal");
                return;
            }
            var plcEntry = ControllerServices.Instance.Controllers
                .Where(x => x.Key == controllerName).FirstOrDefault();
            if (plcEntry.Value == null)
            {
                LogProgram.WriteLog($"[Signal] PLC '{controllerName}' không tìm thấy trong ControllerServices");
                return;
            }
            var plc = plcEntry.Value;
            if (!_stationAddressMap.TryGetValue(station, out var addressList) || addressList.Count == 0)
            {
                LogProgram.WriteLog($"[Signal] Không có địa chỉ nào cho {station}");
                return;
            }
            var signalsForStation = addressList
                .Where(addr => signalDict.ContainsKey(addr))
                .Select(addr => new KeyValuePair<string, ButtonSignal<Guna2CircleButton>>(addr, signalDict[addr]))
                .ToList();
            if (signalsForStation.Count == 0)
            {
                LogProgram.WriteLog($"[Signal] Không match signal nào cho {station}");
                return;
            }
            var cts = new CancellationTokenSource();
            lock (_lock) { _ctsMap[station] = cts; }
            // Capture token cục bộ để Task.Delay không ném exception ra ngoài catch
            var token = cts.Token;
            Task.Run(async () =>
            {
                LogProgram.WriteLog($"[Signal] Bắt đầu đọc {station} ({signalsForStation.Count} signal)");
                while (!token.IsCancellationRequested)
                {
                    // Đọc giá trị PLC trên background thread (không động UI)
                    var readings = new List<(ButtonSignal<Guna2CircleButton> btn, bool val)>();
                    try
                    {
                        foreach (var signal in signalsForStation)
                        {
                            if (token.IsCancellationRequested) break;
                            var parts = signal.Key.Split('.');
                            if (parts.Length < 2 || !int.TryParse(parts[1], out int bitIndex))
                                continue;
                            bool val = plc.ReadBitFromWord(parts[0], bitIndex);
                            readings.Add((signal.Value, val));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogProgram.WriteLog($"[Signal] Lỗi đọc {station}: {ex.Message}");
                        await Task.Delay(500).ConfigureAwait(false);
                        continue;
                    }
                    // BUG 7 FIX: UpdateSignal cập nhật Guna2CircleButton — phải marshal về UI thread
                    // Dùng Application.OpenForms để lấy form đang hiển thị mà không giữ reference cứng
                    if (readings.Count > 0)
                    {
                        var uiForm = System.Windows.Forms.Application.OpenForms
                            .Cast<System.Windows.Forms.Form>()
                            .FirstOrDefault(f => !f.IsDisposed && f.IsHandleCreated);
                        if (uiForm != null)
                        {
                            try
                            {
                                uiForm.BeginInvoke((System.Windows.Forms.MethodInvoker)(() =>
                                {
                                    foreach (var (btn, val) in readings)
                                        btn.UpdateSignal(val);
                                }));
                            }
                            catch (Exception) { /* form đóng giữa chừng — bỏ qua */ }
                        }
                    }
                    await Task.Delay(200).ConfigureAwait(false);
                }
                LogProgram.WriteLog($"[Signal] Dừng đọc {station}");
            });
        }
        public void StopReadStation(string station)
        {
            lock (_lock)
            {
                if (_ctsMap.TryGetValue(station, out var old))
                {
                    try { old.Cancel(); old.Dispose(); } catch { }
                    _ctsMap.Remove(station);
                }
            }
        }
        public void StopAllSignalRead()
        {
            lock (_lock)
            {
                foreach (var cts in _ctsMap.Values)
                    try { cts.Cancel(); cts.Dispose(); } catch { }
                _ctsMap.Clear();
            }
        }
        // Tương thích với code cũ — mặc định đọc Station1
        public void StartAutoReadSignalPLC(string name)
        {
            StartReadStation(name, "Station1");
        }
    }
}