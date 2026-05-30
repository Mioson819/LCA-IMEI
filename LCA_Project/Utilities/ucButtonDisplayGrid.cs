using DocumentFormat.OpenXml.Wordprocessing;
using Guna.UI2.WinForms;
using LCA_Project.Database;
using LCA_Project.Form.TesterComunication;
using LCA_Project.Utilities;
using Project_Visionpro.Program.PLC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Bottom_Sorting.Services.Utilities
{
    public class ucButtonDisplayGrid<T> : TableLayoutPanel where T : class
    {
        public event EventHandler NG999;
        private string NameStation { get; set; }
        private delegate void Updatelabel(int row, int colums, int classify, int Out);
        private Updatelabel _UpdateLabel;
        private readonly List<Guna2Button> _buttons = new List<Guna2Button>();
        public event EventHandler UpdateStatusButton;
        public Action CheckDelete;
        private Dictionary<string, bool> _statusButton = new Dictionary<string, bool>();
        private readonly Dictionary<Guna2Button, string> _Modify = new Dictionary<Guna2Button, string>();
        private readonly Dictionary<string, Guna2Button> _byRowCol = new Dictionary<string, Guna2Button>(); // "r-c"
        private readonly Dictionary<string, Guna2Button> _byColRow = new Dictionary<string, Guna2Button>(); // "c-r" 
        private PropertyInfo[] props;
        private T Value;
        private KeyenceHostLinkTcpClient plc;
        private CancellationTokenSource _cts;
        private readonly List<Task> _runningTasks = new List<Task>();
        public delegate void TakePointLoad(int nX, int nY, int Classify, int Out);
        public TakePointLoad _takPointLoad;
        private System.Windows.Forms.Timer timer;
        private OldButton oldButton;
        private List<string> values;
        public bool isSend = false;
        private int Input, NG;
        private int nX { get; set; }
        private int nY { get; set; }
        private int nXNG { get; set; }
        private int nYNG { get; set; }
        private int classify { get; set; }
        private int Out { get; set; }
        private int row { get; set; }
        private int col { get; set; }
        private string _registerModify;
        private string _NameStation { get; set; }
        public string Nametation
        {
            get => _NameStation;
            set
            {
                if (value == "Station1") _NameStation = "PORT1";
                else if (value == "Station2") _NameStation = "PORT2";
                else if (value == "Station3") _NameStation = "PORT3";
                else if (value == "Station4") _NameStation = "PORT4";
            }
        }
        public ucButtonDisplayGrid(int nX, int nY, KeyenceHostLinkTcpClient plc, T item, string nameStation, string RegisterModify, string Tray)
        {
            if (nX == 0 || nY == 0 || item == null) return;
            if (nX > 16) throw new InvalidOperationException("Mỗi row = 1 word, tối đa 16 bit (nX <= 16).");
            this.NameStation = nameStation;
            this.Nametation = nameStation;
            this.col = nX;
            this.row = nY;
            this._registerModify = RegisterModify;
            this.Dock = DockStyle.Fill;
            this.BackColor = System.Drawing.Color.White;
            timer = new System.Windows.Forms.Timer { Interval = 9000 };
            timer.Tick += Timer_Tick;
            if (Tray == "Load")
            {
                InitializeControllerforLoad(nX, nY, plc, item, RegisterModify);
            }
            else if (Tray == "UnLoad")
            {
                InitializeControllerforUnLoad(nX, nY, plc, item, RegisterModify);
            }
            this.SizeChanged += UcButtonDisplayGrid_SizeChanged;
        }
        public void InitializeControllerforLoad(int nX, int nY, KeyenceHostLinkTcpClient plc, T item, string RegisterModify)
        {
            StopAllBackgroundWork();
            this.plc = plc;
            this.Value = item;
            _UpdateLabel += UpdateLabel;
            _buttons.Clear();
            _byRowCol.Clear();
            _byColRow.Clear();
            Type type = typeof(T);
            props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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
                for (int i = 0; i < cols; i++)
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
                for (int j = 0; j < rows; j++)
                    this.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
                var buttons = new Guna2Button[nX * nY];
                for (int i = 0; i < nX * nY; i++)
                {
                    int index = i;
                    int r = (index / cols) + 1;
                    int c = (index % cols) + 1;
                    buttons[index] = new Guna2Button
                    {
                        Name = $"{r}{c}".Trim(),
                        Dock = DockStyle.Fill,
                        Font = new System.Drawing.Font("Arial", 7, FontStyle.Bold),
                        ForeColor = System.Drawing.Color.Black,
                        Tag = 1,
                        FillColor = System.Drawing.Color.FromArgb(255, 213, 95)            };
                    _Modify[buttons[index]] = $"{r}-{c}";
                    _byRowCol[$"{r}-{c}"] = buttons[index];
                    _byColRow[$"{c}-{r}"] = buttons[index];
                    _buttons.Add(buttons[index]);
                    if (this.NameStation == "Station1" || this.NameStation == "Station3")
                    {
                        this.Controls.Add(buttons[index], index % cols, index / cols);
                    }
                    else
                    {
                        this.Controls.Add(buttons[index], (cols - 1) - (index % cols), index / cols);
                    }
                }
                SetRowHeightsEvenly();
                SetColumnWidthsEvenly();
                AutoReadStatusButton(this.plc, this.Value);
            }
        }
        public void InitializeControllerforUnLoad(int nX, int nY, KeyenceHostLinkTcpClient plc, T item, string RegisterModify)
        {
            StopAllBackgroundWork();
            this.plc = plc;
            this.Value = item;
            _UpdateLabel += UpdateLabel;
            _buttons.Clear();
            _byRowCol.Clear();
            _byColRow.Clear();
            Type type = typeof(T);
            props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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
                for (int i = 0; i < cols; i++)
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
                for (int j = 0; j < rows; j++)
                    this.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
                var buttons = new Guna2Button[nX * nY];
                for (int i = 0; i < nX * nY; i++)
                {
                    int index = i;
                    int r = (index / cols) + 1;
                    int c = (index % cols) + 1;
                    buttons[index] = new Guna2Button
                    {
                        Name = $"{r}{c}".Trim(),
                        Dock = DockStyle.Fill,
                        Font = new System.Drawing.Font("Arial", 7, FontStyle.Bold),
                        ForeColor = System.Drawing.Color.Black,
                        Tag = 1,
                        FillColor = System.Drawing.Color.Gray
                    };
                    _Modify[buttons[index]] = $"{r}-{c}";
                    _byRowCol[$"{r}-{c}"] = buttons[index];
                    _byColRow[$"{c}-{r}"] = buttons[index];
                    _buttons.Add(buttons[index]);
                    if (this.NameStation == "Station1" || this.NameStation == "Station3")
                    {
                        this.Controls.Add(buttons[index], index % cols, index / cols);
                    }
                    else
                    {
                        this.Controls.Add(buttons[index], (cols - 1) - (index % cols), index / cols);
                    }
                }
                SetRowHeightsEvenly();
                SetColumnWidthsEvenly();
                AutoReadStatusButton(this.plc, this.Value);
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
                int thisRowHeight = baseHeight + (i < remaining ? 1 : 0);
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
                int thisColWidth = baseWidth + (i < remaining ? 1 : 0);
                this.ColumnStyles[i].SizeType = SizeType.Absolute;
                this.ColumnStyles[i].Width = thisColWidth;
            }
        }
        private void UcButtonDisplayGrid_SizeChanged(object sender, EventArgs e)
        {
            SetRowHeightsEvenly();
            SetColumnWidthsEvenly();
        }
        private void AutoReadStatusButton(KeyenceHostLinkTcpClient plc, T item)
        {
            values = new List<string>();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            foreach (var prop in props)
            {
                var s = prop.GetValue(item)?.ToString();
                if (!string.IsNullOrEmpty(s)) values.Add(s);
            }
            if (values.Count == 8)
            {
                _runningTasks.Add(Task.Run(() => LoopNG123(values[0], values[1], values[2], values[3], values[4], values[6], values[7], token), token));
            }
            else if (values.Count == 4)
            {
                _runningTasks.Add(Task.Run(() => LoopLoad(values[0], values[1], values[2], token), token));
            }
            else if (values.Count == 9)
            {
                _runningTasks.Add(Task.Run(() => LoopNG4(values[0], values[1], values[2], values[3], values[4], values[7], values[8], token), token));
            }
            else
            {
                LogProgram.WriteLog($"[AutoRead] Unsupported T mapping with values.Count={values.Count}");
            }
        }
        private async Task LoopNG123(string _nX1, string _nY1, string _classify1, string _out1, string _Signal, string nXLoadnow, string nYLoadnow, CancellationToken token)
        {
            ResetLocalState();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (TryReadBitFromWord(_Signal, out bool trig) && trig)
                    {
                        //  await Task.Delay(500, token);
                        LogProgram.WriteLog($"{this.NameStation} Plc send start read NG123");
                        if (TryReadInt32(_nX1, out int s1) &&
                            TryReadInt32(_nY1, out int s2) &&
                            TryReadInt32(_out1, out int s4))
                        {
                            LogProgram.WriteLog($"{this.NameStation} Plc send start NG123 : {s1} : {s2} : {s4}");
                            if (s4 > 0)
                            {
                                SafeUI(() => UpdateLabel(s1, s2, s4, 0));
                                if (TryReadInt32(nXLoadnow, out int s6) && TryReadInt32(nYLoadnow, out int s7))
                                {
                                    _takPointLoad?.Invoke(s6, s7, -1, -1);
                                }
                                await Task.Delay(1000, token);
                            }
                        }
                    }
                    await Task.Delay(300, token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { LogProgram.WriteLog($"[NG123] {ex.Message}"); }
            }
        }
        private async Task LoopNG4(string _nX1, string _nY1, string _classify1, string _out1, string _signal, string nXLoadnow, string nYLoadnow, CancellationToken token)
        {
            ResetLocalState();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (TryReadBitFromWord(_signal, out bool trig) && trig)
                    {
                        LogProgram.WriteLog($"{this.NameStation} Plc send start read NG4");
                        // await Task.Delay(500, token);
                        if (TryReadInt32(_nX1, out int s1) &&
                            TryReadInt32(_nY1, out int s2) &&
                            TryReadInt32(_out1, out int s4))
                        {
                            LogProgram.WriteLog($"{this.NameStation} Plc send start NG4 : {s1} : {s2} : {s4}");
                            if (s4 > 0)
                            {
                                SafeUI(() => UpdateLabel(s1, s2, s4, 0));
                                if (TryReadInt32(nXLoadnow, out int s7) && TryReadInt32(nYLoadnow, out int s8))
                                {
                                    _takPointLoad?.Invoke(s7, s8, -1, -1);
                                }
                                await Task.Delay(1000, token);
                            }
                        }
                    }
                    await Task.Delay(300, token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { LogProgram.WriteLog($"[NG4] {ex.Message}"); }
            }
        }
        private async Task LoopLoad(string _nX1, string _nY1, string _Signal, CancellationToken token)
        {
            this.nX = 0;
            this.nY = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (TryReadBitFromWord(_Signal, out bool trig) && trig)
                    {
                        // await Task.Delay(500, token);
                        if (TryReadInt16(_nX1, out short s1) && TryReadInt16(_nY1, out short s2))
                        {
                            SafeUI(() => UpdateLabel(s1, s2, 0, 0));
                            await Task.Delay(1000, token);
                        }
                    }
                    await Task.Delay(300, token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { LogProgram.WriteLog($"[Load] {ex.Message}"); }
            }
        }
        private void ResetLocalState()
        {
            this.nX = 0; this.nY = 0; this.classify = 0; this.Out = 0; this.nYNG = 0; this.nXNG = 0;
        }
        /// <summary>
        /// Gọi từ Form1.HandleLogLine khi logwatcher đọc về Pass (value == 0) trong mode 2 tray IMEI.
        /// Đọc vị trí hiện tại của sản phẩm từ PLC (values[0]=X, values[1]=Y) rồi tô màu xanh.
        /// SafeUI dùng BeginInvoke nên an toàn gọi từ background thread (logwatcher callback).
        /// </summary>
        public void MarkCurrentAsPass()
        {
            if (values == null || values.Count < 2) return;
            if (TryReadInt16(values[0], out short x) && TryReadInt16(values[1], out short y))
            {
                LogProgram.WriteLog($"{NameStation} MarkCurrentAsPass: vị trí [{x}-{y}] → xanh");
                SafeUI(() => UpdateLabel((int)x, (int)y, 0, 0));
            }
        }

        public void UpdateLabel(int row, int colums, int classify, int Out)
        {
            try
            {
                //if (row < 1 || row > this.row || colums < 1 || colums > this.col)
                //{
                //    LogProgram.WriteLog($"[{NameStation}] Out-of-range UpdateLabel r={row}, c={colums}");
                //    return;
                //   }
                LogProgram.WriteLog($"{this.NameStation} Plc send : {classify} | Location : [{row}-{colums}]");
                var keyColRow = $"{row}-{colums}";
                if (!_byColRow.TryGetValue(keyColRow, out var btn) || btn == null)
                {
                    LogProgram.WriteLog($"No Find Button {keyColRow} (col-row) {this.NameStation}");
                    return;
                }
                if (classify == 0)
                {
                    timer.Stop();
                    oldButton = new OldButton() { btnName = keyColRow, Classify = 0, Out = Out };
                    btn.FillColor = System.Drawing.Color.Green;
                    btn.Image = null;
                    btn.Text = "";
                    LogProgram.WriteLog($" {this.NameStation} UpdateLabel : Pass | Location : [{keyColRow}]");
                    timer.Start();
                }
                if (classify == -1 && Out == -1)
                {
                    timer.Stop();
                    oldButton = new OldButton() { btnName = keyColRow, Classify = -1, Out = -1 };
                    btn.FillColor = System.Drawing.Color.Red;
                    btn.Image = null;
                    btn.Text = "";
                    LogProgram.WriteLog($" {this.NameStation} UpdateLabel : Load_NG | Location : [{keyColRow}]");
                    timer.Start();
                }
                else if (classify > 0)
                {
                    timer.Stop();
                    oldButton = new OldButton() { btnName = keyColRow, Classify = classify, Out = 0 };
                    btn.FillColor = System.Drawing.Color.Red;
                    btn.Text = $"{classify}";
                    btn.Image = null;
                    LogProgram.WriteLog($"{this.NameStation} UpdateLabel : NG {classify} | Location : [{keyColRow}]");
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            string s = this.Nametation; // PORT1..4
                            DatabaseControllers.Instance.InsertDataNG("NG" + $"{classify}", s);
                        }
                        catch (Exception ex) { LogProgram.WriteLog($"DB NG err: {ex.Message}"); }
                    });
                    timer.Start();
                }
                else
                {
                    LogProgram.WriteLog($"No Find Button {keyColRow} NG/OK {classify} {this.NameStation}");
                }
                if (classify > 900)
                {
                    int value = DatabaseControllers.Instance.SelectDataPortSummaNG(this.Nametation) + 1;
                    DatabaseControllers.Instance.InsertDataPortSummaNG(this.Nametation, value);
                    NG999?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog("Error " + ex.Message);
            }
            finally
            {
                CheckDelete?.Invoke();
            }
        }
        public void Rset()
        {
            SafeUI(() =>
            {
                foreach (var btn in _buttons)
                {
                    btn.FillColor = System.Drawing.Color.Gray;
                    btn.Text = "";
                    btn.Image = null;
                    btn.Image = null;
                }
            });
        }
        public void RsetLoad()
        {
            SafeUI(() =>
            {
                foreach (var btn in _buttons)
                {
                    btn.FillColor = System.Drawing.Color.FromArgb(255, 213, 95) ;
                    btn.Text = "";
                    btn.Image = null;
                    btn.Image = null;
                }
            });
        }
        public void FullTray()
        {
            SafeUI(() =>
            {
                foreach (var btn in _buttons)
                {
                    if (btn.Tag.ToString() == "1")
                    {
                       // btn.Image = LCA_Project.Properties.Resources.z7034644005131_de3582810b09347c4ea4d794a3e8a39c;
                    }
                    btn.FillColor = System.Drawing.Color.FromArgb(255, 213, 95) ;
                    btn.Text = "";
                }
            });
        }
        public void ResetUnLoad()
        {
            SafeUI(() =>
            {
                foreach (var btn in _buttons)
                {
                    btn.Text = "";
                    btn.Image = LCA_Project.Properties.Resources.z7034644005131_de3582810b09347c4ea4d794a3e8a39c;
                    btn.FillColor = System.Drawing.Color.Gray;
                }
            });
        }
        public void ResetLoad()
        {
            SafeUI(() =>
            {
                foreach (var btn in _buttons)
                {
                    btn.Text = "";
                    btn.Image = LCA_Project.Properties.Resources.z7034644005131_de3582810b09347c4ea4d794a3e8a39c;
                    btn.FillColor = System.Drawing.Color.FromArgb(255, 213, 95) ;
                }
            });
        }
        public void SetComp()
        {
            SafeUI(() =>
            {
                foreach (var btn in _buttons)
                {
                    btn.Image = null;
                    btn.FillColor = System.Drawing.Color.Green;
                    btn.Text = "";
                }
            });
        }
        public void SetEmptyUnLoad()
        {
            SafeUI(() =>
            {
                foreach (var btn in _buttons)
                {
                    btn.Image = null;
                    btn.Text = "";
                    btn.FillColor = System.Drawing.Color.Gray;
                }
            });
        }
        public void SetEmptyLoad()
        {
            SafeUI(() =>
            {
                foreach (var btn in _buttons)
                {
                    btn.Image = null;
                    btn.Text = "";
                    btn.FillColor = System.Drawing.Color.FromArgb(255, 213, 95) ;
                }
            });
        }
        public void Modify(string register)
        {
            if (string.IsNullOrWhiteSpace(register)) return;
            if (!isSend)
            {
                PrimeAllOnes(register);
                if (this.col > 16)
                {
                    MessageBox.Show("Số cột > 16. Một row chỉ hỗ trợ 16 bit/word.");
                    return;
                }
                foreach (var btn in _buttons)
                {
                    btn.Text = "";
                    btn.FillColor = System.Drawing.Color.Gray;
                    btn.Image = null;
                    btn.MouseDown -= Clicks; btn.MouseDown -= MousRight;
                    btn.MouseDown += Clicks;
                    btn.MouseDown += MousRight;
                }
                StopAllBackgroundWork();
                isSend = true;
                return;
            }
            try
            {
                for (int r = 0; r < this.row; r++)
                {
                    string addr = ResolveRegister(register, r);
                    if (!TryReadUInt16(addr, out ushort word))
                    {
                        TryReadInt16(addr, out short val16);
                        word = unchecked((ushort)val16);
                    }
                    for (int c = 0; c < this.col; c++)
                    {
                        var keyRowCol = $"{r + 1}-{c + 1}";
                        if (_byRowCol.TryGetValue(keyRowCol, out var btn) && btn.Image != null)
                        {
                            word = (ushort)(word & ~(1 << c));
                            btn.Tag = 1;
                        }
                        else
                        {
                            btn.Tag = 0;
                        }
                    }
                    TryWriteUInt16(addr, word);
                }
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog($"Modify save error: {ex.Message}");
            }
            foreach (var b in _buttons)
            {
                b.MouseDown -= Clicks;
                b.MouseDown -= MousRight;
            }
            isSend = false;
            FullTray();
            AutoReadStatusButton(this.plc, this.Value);
            MessageBox.Show("Modify Success");
        }
        private void Clicks(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;
            var btn = (Guna2Button)sender;
            if (btn.Image == null)
            {
                btn.Image = LCA_Project.Properties.Resources.z7034644005131_de3582810b09347c4ea4d794a3e8a39c;
            }
            else
            {
                btn.Image = null;
                btn.FillColor = System.Drawing.Color.Gray;
            }
        }
        public List<Guna2Button> OldResult()
        {
            return _buttons.Select(x => new Guna2Button()
            {
                Text = x.Text,
                Name = x.Name,
                FillColor = x.FillColor,
                Dock = x.Dock,
                ForeColor = x.ForeColor
            }).ToList();
        }
        private ushort GetAllOnesMaskForCols()
        {
            if (this.col > 16)
                throw new InvalidOperationException("Mỗi row = 1 word, tối đa 16 bit.");
            return (ushort)((this.col >= 16) ? 0xFFFF : ((1 << this.col) - 1));
        }
        public void PrimeAllOnes(string register)
        {
            if (string.IsNullOrWhiteSpace(register)) return;
            ushort mask = GetAllOnesMaskForCols();
            for (int r = 0; r < this.row; r++)
            {
                string addr = ResolveRegister(register, r);
                TryWriteUInt16(addr, mask);
            }
        }
        private string ResolveRegister(string baseReg, int offset)
        {
            var m = Regex.Match(baseReg, @"^(?<prefix>[A-Za-z]+)(?<num>\d+)$");
            if (m.Success)
            {
                string prefix = m.Groups["prefix"].Value;
                int num = int.Parse(m.Groups["num"].Value);
                return $"{prefix}{num + offset}";
            }
            return baseReg + offset.ToString();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (oldButton == null) { timer.Stop(); return; }
            SafeUI(() =>
            {
                try
                {
                    if (!_byColRow.TryGetValue(oldButton.btnName, out var btn) || btn == null)
                    {
                        LogProgram.WriteLog($"Recheck miss {this.NameStation} Key : [{oldButton.btnName}]");
                    }
                    else if (btn.FillColor == System.Drawing.Color.Gray || btn.Image != null)
                    {
                        if (oldButton.Classify == 0)
                        {
                            btn.FillColor = System.Drawing.Color.Green;
                            btn.Text = "";
                            btn.Image = null;
                            LogProgram.WriteLog($"{this.NameStation} ReUpdate : Pass | Location : [{oldButton.btnName}]");
                        }
                        else if (oldButton.Classify > 0)
                        {
                            btn.FillColor = System.Drawing.Color.Red;
                            btn.Text = $"{oldButton.Classify}";
                            btn.Image = null;
                            LogProgram.WriteLog($"{this.NameStation} ReUpdate : NG {oldButton.Classify} | Location : [{oldButton.btnName}]");
                        }
                    }
                }
                catch { }
                finally
                {
                    oldButton = null;
                    timer.Stop();
                }
            });
        }
        private void MousRight(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var btn = sender as Guna2Button;
            if (btn == null) return;
            //if (string.IsNullOrWhiteSpace(btn.Name) || btn.Name.Length < 2) return;
            if (_Modify.TryGetValue(btn, out string s))
            {
                bool _isFirstRows = int.Parse(s.Split('-')[0]) <= 1 ? true : false;
                string colStr = btn.Name.Substring(btn.Name.Length - 1);
                if (!int.TryParse(colStr, out int colIdx)) return;
                SafeUI(() =>
                {
                    if (_isFirstRows)
                    {
                        _byRowCol.TryGetValue($"{1}-{colIdx}", out var btnFirst);
                        if (btnFirst.Image == null)
                        {
                            btnFirst.Image = LCA_Project.Properties.Resources.z7034644005131_de3582810b09347c4ea4d794a3e8a39c;
                        }
                        else
                        {
                            btnFirst.Image = null;
                        }
                        for (int r = 1; r <= this.row; r++)
                        {
                            var keyRowCol = $"{r}-{colIdx}";
                            if (_byRowCol.TryGetValue(keyRowCol, out var btn2) && btn2 != null && btnFirst != null)
                            {
                                if (btnFirst.Image != null)
                                {
                                    btn2.Image = LCA_Project.Properties.Resources.z7034644005131_de3582810b09347c4ea4d794a3e8a39c;
                                }
                                else
                                {
                                    btn2.Image = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        int value = int.Parse(btn.Name) / 10;
                        _byRowCol.TryGetValue($"{value}-{1}", out var btnFirst);
                        if (btnFirst.Image == null)
                        {
                            btnFirst.Image = LCA_Project.Properties.Resources.z7034644005131_de3582810b09347c4ea4d794a3e8a39c;
                        }
                        else
                        {
                            btnFirst.Image = null;
                        }
                        for (int c = 1; c <= this.col; c++)
                        {
                            var keyRowCol = $"{value}-{c}";
                            if (_byRowCol.TryGetValue(keyRowCol, out var btn2) && btn2 != null && btnFirst != null)
                            {
                                if (btnFirst.Image != null)
                                {
                                    btn2.Image = LCA_Project.Properties.Resources.z7034644005131_de3582810b09347c4ea4d794a3e8a39c;
                                }
                                else
                                {
                                    btn2.Image = null;
                                }
                            }
                        }
                    }
                });
            }
        }
        // -------------- Helpers --------------
        private void SafeUI(Action act)
        {
            try
            {
                if (!IsHandleCreated) return;
                if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)(() => act()));
                else act();
            }
            catch { }
        }
        private bool TryReadBitFromWord(string dotted, out bool bit)
        {
            bit = false;
            try
            {
                var parts = dotted.Split('.');
                if (parts.Length != 2) return false;
                string reg = parts[0];
                if (!int.TryParse(parts[1], out int bitno)) return false;
                bit = plc.ReadBitFromWord(reg, bitno);
                return true;
            }
            catch (Exception ex)
            {
                LogProgram.WriteLog($"TryReadBitFromWord error: {ex.Message}");
                return false;
            }
        }
        private bool TryReadInt16(string reg, out short val)
        {
            val = 0;
            try { val = plc.ReadInt16(reg); return true; }
            catch (Exception ex) { LogProgram.WriteLog($"ReadInt16 {reg} err: {ex.Message}"); return false; }
        }
        private bool TryReadUInt16(string reg, out ushort val)
        {
            val = 0;
            try { val = plc.ReadUInt16(reg); return true; }
            catch (Exception ex) { LogProgram.WriteLog($"ReadUInt16 {reg} err: {ex.Message}"); return false; }
        }
        private bool TryWriteUInt16(string reg, ushort val)
        {
            try { plc.WriteUInt16(reg, val); return true; }
            catch (Exception ex) { LogProgram.WriteLog($"WriteUInt16 {reg} err: {ex.Message}"); return false; }
        }
        private bool TryReadInt32(string reg, out int val)
        {
            val = 0;
            try { val = plc.ReadInt32(reg); return true; }
            catch (Exception ex) { LogProgram.WriteLog($"ReadInt32 {reg} err: {ex.Message}"); return false; }
        }
        private void StopAllBackgroundWork()
        {
            try
            {
                _cts?.Cancel();
                foreach (var t in _runningTasks.ToArray())
                {
                    try { t.Wait(50); } catch { }
                }
            }
            catch { }
            finally
            {
                _runningTasks.Clear();
                _cts?.Dispose();
                _cts = null;
            }
            if (timer != null) { timer.Stop(); oldButton = null; }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopAllBackgroundWork();
                if (timer != null)
                {
                    timer.Tick -= Timer_Tick;
                    timer.Dispose();
                    timer = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
public class OldButton
{
    public string btnName { get; set; }
    public int Classify { get; set; }
    public int Out { get; set; }
}
