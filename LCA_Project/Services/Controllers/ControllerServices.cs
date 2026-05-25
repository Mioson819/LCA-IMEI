using Guna.UI2.WinForms;
using LCA_Project.Database;
using Project_Visionpro.Program.PLC;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Utilities.LogProgram;
namespace LCA_Project.Services.Controllers
{
    public class ControllerServices
    {
        private Dictionary<string,Guna2Panel> _signal = new Dictionary<string, Guna2Panel>();
        public delegate void SetBitHandler(string keys, object sender, EventArgs e);
        public event SetBitHandler SetBits;
        public delegate void ResetBitHandler(string keys, object sender, EventArgs e);
        public event ResetBitHandler ResetBits;
        private static readonly int Port = 8501;
        private bool _isInitialized = false;
        private static ControllerServices _instance;
        private static readonly object _lockSetBit = new object();
        private static readonly object _lockResetBit = new object();
        public static readonly object _lock = new object();
        public CancellationTokenSource _cts;
        public CancellationTokenSource _ctsDataAxis;
        private ControllerServices()
        {
        }
        public static ControllerServices Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ControllerServices();
                    }
                    return _instance;
                }
            }
        }
        private Dictionary<string, KeyenceHostLinkTcpClient> _controllers = new Dictionary<string, KeyenceHostLinkTcpClient>();
        public void InitializeController()
        {
            var values = DatabaseControllers.Instance.GetParamater();
            if (!_isInitialized)
            {
                foreach (var item in values)
                {
                    if (!_controllers.ContainsKey(item.Key))
                    {
                        var controller = new KeyenceHostLinkTcpClient(item.Value.ToString(), Port);
                        _controllers.Add(item.Key.ToString(), controller);
                    }
                }
                _isInitialized = true;
            }
        }
        public List<KeyenceHostLinkTcpClient> GetController()
        {
            return _controllers.Values.ToList();
        }
        public List<string> GetControllerNames()
        {
            return _controllers.Keys.ToList();
        }
        public Dictionary<string, KeyenceHostLinkTcpClient> Controllers
        {
            get
            {
                if (_isInitialized)
                {
                    return _controllers;
                }
                else
                {
                    throw new InvalidOperationException("Controllers have not been initialized. Call InitializeController first.");
                }
            }
        }
        //public void StartAutoWriteAllPlc(object sender, EventArgs e) // Write data  Cycle PC => PLC 
        //{
        //    foreach (var kv in Controllers)
        //    {
        //        string plcName = kv.Key;
        //        var client = kv.Value;
        //        Task.Run(async () =>
        //        {
        //            try
        //            {
        //                await Task.Delay(3000);
        //                var rowData = DatabasePLCs.Instance.GetPlcRowByName(plcName);
        //                foreach (var map in Variable_Word.ColumnToRegisterMap)
        //                {
        //                    string column = map.Key;
        //                    string register = map.Value;
        //                    if (rowData.TryGetValue(column, out object rawVal) && int.TryParse(rawVal?.ToString(), out int value))
        //                    {
        //                        client.WriteUInt16(register, (ushort)value);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Error writing to PLC {plcName}: {ex.Message}", "PLC Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            }
        //        });
        //    }
        //}
        private void UpdateStatus(object sender, bool status) // Update Alarm Cycle : frmAlarm
        {
            System.Windows.Forms.Label label = sender as System.Windows.Forms.Label;
            if (status)
            {
                label.BeginInvoke((MethodInvoker)(() =>
            {
                label.ForeColor = System.Drawing.Color.Yellow;
            }));
            }
            else
            {
                label.BeginInvoke((MethodInvoker)(() =>
                {
                    label.ForeColor = System.Drawing.Color.Red;
                }));
            }
        }
        // Data Axis View
        //public void StartAutoReadAllPlcDataAxis() // Update Alarm Cycle : frmControleersDeviceDataAxis
        //{
        //    //Console.WriteLine(Variable_Bit.StateBitMap.Count);
        //    foreach (var kv in Controllers)
        //    {
        //        if (kv.Key == name)
        //        {
        //            Task.Run(async () =>
        //            {
        //                while (!_ctsDataAxis.Token.IsCancellationRequested)
        //                {
        //                    try
        //                    {
        //                        string value = kv.Value.ReadUInt16(tag).ToString();
        //                        UpdateStatusTextBox(sender, value);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        MessageBox.Show($"Error reading from PLC: {ex.Message}", "PLC Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                    }
        //                    await Task.Delay(50);
        //                    //  Console.WriteLine(Variable_Bit.StateBitMap.Count);
        //                }
        //            }, _ctsDataAxis.Token);
        //        }
        //    }
        //}
        private void UpdateStatusTextBox(object sender, string value) // Update Alarm Cycle : frmAlarm
        {
            Guna2TextBox textBox = sender as Guna2TextBox;
            textBox.BeginInvoke((MethodInvoker)(() =>
                {
                    textBox.Text = value;
                }));
        }
    }
}
