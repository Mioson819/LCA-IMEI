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
        private Dictionary<string,ButtonSignal<Guna2CircleButton>> _signal = new Dictionary<string, ButtonSignal<Guna2CircleButton>>();
        public Dictionary<string , Dictionary<string, ButtonSignal<Guna2CircleButton>>> _Signal = new Dictionary<string, Dictionary<string, ButtonSignal<Guna2CircleButton>>>();
        private Form _mainForm;
        private static readonly object _lock = new object();
        private static ProcessMain _instance;
      //  private frmSignal _frmSignal;
        public CancellationTokenSource _cts;
        public static ProcessMain Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ProcessMain();
                    }
                    return _instance;
                }
            }
        }
        private ProcessMain()
        {
        }
        public async Task Initialize()
        {
          await  InitializeSignalTable();
        }
        private async Task InitializeSignalTable()
        {
            List<Task> allTasks = new List<Task>();
            _signal.Clear();
            _Signal.Clear();
            foreach (var kv in ControllerServices.Instance.Controllers)
            {
                Task runInitialize =   Task.Run(() =>
                {
                    foreach (var register in DatabaseControllers.Instance.GetRegister_ControllerParameterInputs())
                    {
                        if (_signal.ContainsKey(register.Key)) continue;
                        _signal.Add(register.Value, new ButtonSignal<Guna2CircleButton> { NameSignal = register.Key, RegisterSignal = register.Value });
                    }
                    _Signal.Add(kv.Key, _signal);
                });
                allTasks.Add(runInitialize);
            }
            await Task.WhenAll(allTasks);
        }
        private void InitializeButtonDisplayGrid()
        {
        }
        public void StartAutoReadSignalPLC(string name) // Update Alarm Cycle : frmAlarm
        {
            _cts = new CancellationTokenSource();
            var kv = _Signal.Where(x => x.Key == name).FirstOrDefault();
            var _object = ControllerServices.Instance.Controllers.Where(x => x.Key == kv.Key).FirstOrDefault().Value;
            if (_object == null) return;
            var CheckKey = kv.Value;
            Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            foreach (var read in CheckKey)
                            {
                                read.Value.UpdateSignal(_object.ReadBitFromWord(read.Key.Split('.')[0], int.Parse(read.Key.Split('.')[1])));
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error reading from PLC: {ex.Message}", "PLC Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _cts.Cancel();
                        }
                        await Task.Delay(50);
                        //  Console.WriteLine(Variable_Bit.StateBitMap.Count);
                    }
                }, _cts.Token);
        }
    }
}
