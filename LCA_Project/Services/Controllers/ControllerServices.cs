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
