using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Bottom_Sorting.Services.Controllers
{
    public class PLCRegisterDataControl
    {
        private readonly KeyenceHostLinkTcpClient _plc;
        public string Address { get; set; }
        public int Scale { get; }   
        private int _oldPlcValue;  
        private bool _loaded;
        public PLCRegisterDataControl(KeyenceHostLinkTcpClient plc,string address,int scale)
        {
            if (scale != 10 && scale != 100&& scale!=1)
                throw new ArgumentException("Scale chỉ được là 10 hoặc 100");
            _plc = plc;
            Address = address;
            Scale = scale;
        }
        public string LoadOnce()
        {
            if (_loaded)
                return FormatDisplay(_oldPlcValue);
            if (Scale == 10 || Scale == 1)
                _oldPlcValue = _plc.ReadInt16(Address);
            else 
                _oldPlcValue = _plc.ReadInt32(Address);
            _loaded = true;
            return FormatDisplay(_oldPlcValue);
        }
        public string Refresh()
         {
            if (Scale == 10 || Scale == 1)
                _oldPlcValue = _plc.ReadInt16(Address);
            else
                _oldPlcValue = _plc.ReadInt32(Address);
            return FormatDisplay(_oldPlcValue);
        }
        public void OnEnter(Guna.UI2.WinForms.Guna2TextBox tb)
        {
            if (!_loaded) return;
            if (!TryNormalize(tb.Text, out int newPlcValue))
            {
                MessageBox.Show("Giá trị nhập không hợp lệ");
                tb.Text = FormatDisplay(_oldPlcValue);
                tb.SelectAll();
                return;
            }
            var rs = MessageBox.Show("Bạn có muốn gửi giá trị này xuống PLC không?","Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (rs == DialogResult.No)
            {
                tb.Text = FormatDisplay(_oldPlcValue);
                tb.SelectAll();
                return;
            }
            if (newPlcValue == _oldPlcValue)
            {
                tb.Text = FormatDisplay(_oldPlcValue);
                return;
            }
            bool writeOk;
            if (Scale == 10)
                writeOk = _plc.WriteInt16(Address, (short)newPlcValue);
            else
                writeOk = _plc.WriteInt32(Address, newPlcValue);
            if (!writeOk)
            {
                MessageBox.Show("Ghi PLC thất bại");
                tb.Text = FormatDisplay(_oldPlcValue);
                return;
            }
            int verifyValue = (Scale == 10)
                ? _plc.ReadInt16(Address)
                : _plc.ReadInt32(Address);
            if (verifyValue != newPlcValue)
            {
                MessageBox.Show("PLC trả về giá trị không khớp");
                tb.Text = FormatDisplay(_oldPlcValue);
                return;
            }
            _oldPlcValue = verifyValue;
            tb.Text = FormatDisplay(_oldPlcValue);
        }
        public void Send(Guna.UI2.WinForms.Guna2TextBox tb)
        {
            if (!_loaded) return;
            if (!TryNormalize(tb.Text, out int newPlcValue))
            {
                MessageBox.Show("Giá trị nhập không hợp lệ");
                tb.Text = FormatDisplay(_oldPlcValue);
                tb.SelectAll();
                return;
            }
            if (newPlcValue == _oldPlcValue)
            {
                tb.Text = FormatDisplay(_oldPlcValue);
                return;
            }
            bool writeOk;
            if (Scale == 10)
                writeOk = _plc.WriteInt16(Address, (short)newPlcValue);
            else
                writeOk = _plc.WriteInt32(Address, newPlcValue);
            if (!writeOk)
            {
                MessageBox.Show("Ghi PLC thất bại");
                tb.Text = FormatDisplay(_oldPlcValue);
                return;
            }
            int verifyValue = (Scale == 10)
                ? _plc.ReadInt16(Address)
                : _plc.ReadInt32(Address);
            if (verifyValue != newPlcValue)
            {
                MessageBox.Show("PLC trả về giá trị không khớp");
                tb.Text = FormatDisplay(_oldPlcValue);
                return;
            }
            _oldPlcValue = verifyValue;
            tb.Text = FormatDisplay(_oldPlcValue);
        }
        private string FormatDisplay(int plcValue)
        {
            decimal v = (decimal)plcValue / Scale;
            return v.ToString(CultureInfo.InvariantCulture);
        }
        private bool TryNormalize(string input, out int plcValue)
        {
            plcValue = 0;
            if (!decimal.TryParse(input,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal uiValue))
                return false;
            plcValue = (int)Math.Round(uiValue * Scale,
                MidpointRounding.AwayFromZero);
            return true;
        }
    }
    public class PLCBitControl
    {
        private static KeyenceHostLinkTcpClient _plc;
        public string WordAddress { get; set; }
        public int BitIndex { get; set; }
        private static CancellationTokenSource _cts;
        private bool _currentState;
        private bool _loaded;
        private  static Dictionary<Guna.UI2.WinForms.Guna2GradientButton, ButtonBinding> _bindings
            = new Dictionary<Guna.UI2.WinForms.Guna2GradientButton, ButtonBinding>();
        private static Task _task;
        public PLCBitControl(KeyenceHostLinkTcpClient plc,string tag, Guna.UI2.WinForms.Guna2GradientButton button)
        {
            _plc = plc;
            // TAG: MR600|0
            var parts = tag.Split('.');
            WordAddress = parts[0];
            BitIndex = int.Parse(parts[1]);
            if (_bindings.ContainsKey(button))
                return;
            _bindings[button] = new ButtonBinding
            {
                _button = button,
                Address = button.Tag.ToString(),
                LastState = false
            };
        }
        public bool LoadOnce()
        {
            if (_loaded)
                return _currentState;
            _currentState = _plc.ReadBitFromWord(WordAddress, BitIndex);
            _loaded = true;
            return _currentState;
        }
        public  static void  Start()
        { 
            if (_task != null && !_task.IsCompleted)
                return;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _task = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var item in _bindings.Values)
                    {
                        try
                        {
                            if (item.LastState != _plc.ReadBitFromWord(item.Address.ToString().Split('.')[0], int.Parse(item.Address.ToString().Split('.')[1])))
                            {
                                item.LastState = _plc.ReadBitFromWord(item.Address.ToString().Split('.')[0], int.Parse(item.Address.ToString().Split('.')[1]));
                                if (item._button.IsHandleCreated)
                                {
                                    item._button.BeginInvoke(new Action(() =>
                                    {
                                        item._button.FillColor = item.LastState ? System.Drawing.Color.Lime : System.Drawing.Color.Silver;
                                    }));
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    await Task.Delay(1000, token);
                }
            }, token);
        }
        public  static void Stop()
        {
            _cts?.Cancel();
            _task = null;
        }
        public static void  Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
        public bool Toggle(out string error)
        {
            error = null;
            if (!_loaded)
            {
                error = "Bit chưa được load trạng thái ban đầu";
                return _currentState;
            }
            bool ok;
            if (_currentState)
            {
                ok = _plc.ResetBitInWord(WordAddress, BitIndex);
                if (!ok)
                {
                    error = $"Reset bit {WordAddress}|{BitIndex} thất bại";
                    return _currentState;
                }
                _currentState = false;
            }
            else
            {
                ok = _plc.SetBitInWord(WordAddress, BitIndex);
                if (!ok)
                {
                    error = $"Set bit {WordAddress}|{BitIndex} thất bại";
                    return _currentState;
                }
                _currentState = true;
            }
            return _currentState;
        }
        public bool ToggleMessenger(out string error)
        {
            if(MessageBox.Show("Do You Confirm ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                error = String.Empty;
               return _currentState;
            }
            error = null;
            if (!_loaded)
            {
                error = "Bit chưa được load trạng thái ban đầu";
                return _currentState;
            }
            bool ok;
            //if (_currentState)
            //{
            //    ok = _plc.ResetBitInWord(WordAddress, BitIndex);
            //    if (!ok)
            //    {
            //        error = $"Reset bit {WordAddress}|{BitIndex} thất bại";
            //        return _currentState;
            //    }
            //    _currentState = false;
            //}
            //else
            //{
                ok = _plc.SetBitInWord(WordAddress, BitIndex);
                if (!ok)
                {
                    error = $"Set bit {WordAddress}|{BitIndex} thất bại";
                    return _currentState;
                }
                _currentState = true;
           // }
            return _currentState;
        }
        public bool ToggleMessengerReset(out string error)
        {
            bool ok;
            if (_currentState)
            {
                ok = _plc.ResetBitInWord(WordAddress, BitIndex);
                if (!ok)
                {
                    error = $"Reset bit {WordAddress}|{BitIndex} thất bại";
                    return _currentState;
                }
                _currentState = false;
            }
            else
            {
                ok = _plc.SetBitInWord(WordAddress, BitIndex);
                if (!ok)
                {
                    error = $"Set bit {WordAddress}|{BitIndex} thất bại";
                    return _currentState;
                }
                _currentState = true;
            }
            error = "";
            return _currentState;
        }
        public bool ToggleMessengerCondition(out string error)
        {
             if (_plc.ResetBitInWord("MR600", 0))
             {
                error = "Process Auto . Please Change Process ";
                return _currentState;
             }
             else if (MessageBox.Show("Do You Confirm ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
             {
                error = "Bạn Đã Xác Nhận No";
                return _currentState;
             }
            error = null;
            if (!_loaded)
            {
                error = "Bit chưa được load trạng thái ban đầu";
                return _currentState;
            }
            bool ok;
            if (_currentState)
            {
                ok = _plc.ResetBitInWord(WordAddress, BitIndex);
                if (!ok)
                {
                    error = $"Reset bit {WordAddress}|{BitIndex} thất bại";
                    return _currentState;
                }
                _currentState = false;
            }
            else
            {
                ok = _plc.SetBitInWord(WordAddress, BitIndex);
                if (!ok)
                {
                    error = $"Set bit {WordAddress}|{BitIndex} thất bại";
                    return _currentState;
                }
                _currentState = true;
            }
            return _currentState;
        }
        public bool PressDown(out string error)
        {
            error = null;
            if (!_loaded)
            {
                error = "Bit chưa được load trạng thái ban đầu";
                return _currentState;
            }
            bool ok = _plc.SetBitInWord(WordAddress, BitIndex);
            if (!ok)
            {
                error = $"Set bit {WordAddress}|{BitIndex} thất bại";
                return _currentState;
            }
            _currentState = true;
            return _currentState;
        }
        public bool Release(out string error)
        {
            error = null;
            if (!_loaded)
            {
                error = "Bit chưa được load trạng thái ban đầu";
                return _currentState;
            }
            bool ok = _plc.ResetBitInWord(WordAddress, BitIndex);
            if (!ok)
            {
                error = $"Reset bit {WordAddress}|{BitIndex} thất bại";
                return _currentState;
            }
            _currentState = false;
            return _currentState;
        }
        public bool CurrentState => _currentState;
        private class ButtonBinding
        {
            public Guna.UI2.WinForms.Guna2GradientButton _button;
            public string Address;
            public bool LastState;
        }
    }
    public class PLCLabelControl: IDisposable
    {
        private readonly KeyenceHostLinkTcpClient _plc;
        private readonly Dictionary<System.Windows.Forms.Label, LabelBinding> _bindings
            = new Dictionary<System.Windows.Forms.Label, LabelBinding>();
        private CancellationTokenSource _cts;
        private Task _task;
        private int _intervalMs;
        public PLCLabelControl(
            KeyenceHostLinkTcpClient plc,
            int intervalMs = 200)
        {
            _plc = plc;
            _intervalMs = intervalMs;
        }
        public void Register(System.Windows.Forms.Label label, int scale)
        {
            if (label.Tag == null)
                throw new ArgumentException("Label.Tag phải chứa địa chỉ PLC");
            if (scale != 1 && scale != 10 && scale != 100)
                throw new ArgumentException("Scale chỉ cho phép 1 / 10 / 100");
            if (_bindings.ContainsKey(label))
                return;
            _bindings[label] = new LabelBinding
            {
                Label = label,
                Address = label.Tag.ToString(),
                Scale = scale,
                LastRawValue = null
            };
        }
        public void Start()
        {
            if (_task != null && !_task.IsCompleted)
                return;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _task = Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        foreach (var item in _bindings.Values)
                        {
                            int rawValue = item.Scale == 10 ? _plc.ReadInt16(item.Address) : _plc.ReadInt32(item.Address);
                            if (item.LastRawValue.HasValue &&
                                item.LastRawValue.Value == rawValue)
                                continue;
                            item.LastRawValue = rawValue;
                            string display = FormatDisplay(rawValue, item.Scale);
                            if (item.Label.IsHandleCreated)
                            {
                                item.Label.BeginInvoke(new Action(() =>
                                {
                                    item.Label.Text = display;
                                }));
                            }
                        }
                        await Task.Delay(_intervalMs, token);
                    }
                }
                catch(Exception ex) { }
            }, token);
        }
        public void Stop()
        {
            _cts?.Cancel();
            _task = null;
        }
        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
        private string FormatDisplay(int rawValue, int scale)
        {
            decimal v = (decimal)rawValue / scale;
            return v.ToString(CultureInfo.InvariantCulture);
        }
        private class LabelBinding
        {
            public System.Windows.Forms.Label Label;
            public string Address;
            public int Scale;
            public int? LastRawValue;
        }
    }
    public class PLCRadioControl : INotifyPropertyChanged
    {
        private readonly KeyenceHostLinkTcpClient _plc;
        public string WordAddress { get;}
        public int BitIndex { get;}
        private bool _isChecked;
        private bool _loaded;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value)
                {
                    return;
                }
                if (!_loaded)
                {
                    return;
                }
                bool ok;
                if (value)
                    ok = _plc.SetBitInWord(WordAddress, BitIndex);
                else
                    ok = _plc.ResetBitInWord(WordAddress, BitIndex);
                if (!ok)
                {
                    return;
                }
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public PLCRadioControl(KeyenceHostLinkTcpClient plc,string tag)
        {
            _plc = plc;
            var parts = tag.Split('|');
            WordAddress = parts[0];
            BitIndex = int.Parse(parts[1]);
        }
        public bool  LoadOnce()
        {
            if(_loaded) return  false;
            bool plcValue = _plc.ReadBitFromWord(WordAddress, BitIndex);
            _isChecked = plcValue;
            _loaded = true;
            OnPropertyChanged(nameof(IsChecked));
            return plcValue;
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
