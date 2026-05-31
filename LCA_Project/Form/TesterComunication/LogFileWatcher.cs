using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LCA_Project.Database;
namespace LCA_Project.Form.TesterComunication
{
    public class LogFileWatcher
    {
        public string serverFolder { get; set; }
        // ── PcType ─────────────────────────────────────────────────────────────
        // Thay thế biến static OffMess.
        // "Nano"    → OffMess = true  (subfolder năm/tháng, pattern *.log)
        // "Pamtech" → OffMess = false (path gốc, pattern *.txt)
        // Giá trị được truyền vào từ ngoài (Form1 đọc từ DB qua DatabaseControllers.GetPcType)
        // rồi set vào property này trước khi gọi Start().
        private string _pcType = "Nano";    // mặc định Nano nếu chưa set
        public string PcType
        {
            get => _pcType;
            set => _pcType = (value ?? "Nano").Trim();
        }
        // OffMess tương đương: true khi Nano, false khi Pamtech
        private bool IsNano => string.Equals(_pcType, "Nano", StringComparison.OrdinalIgnoreCase);
        // ── ServerFolder & filePattern — logic giữ nguyên, chỉ đổi nguồn biến ──
        private string ServerFolder(string path)
        {
            if (!IsNano) return path;   // Pamtech → path gốc
            return Path.Combine(path,
                DateTime.Now.Year.ToString(),
                DateTime.Now.Month.ToString("D2"));  // Nano → path/năm/tháng
        }
        private string filePattern
        {
            get
            {
                if (!IsNano)
                    return $"-{DateTime.Now.Year.ToString().Substring(2)}" + "*.txt";  // Pamtech
                return $"{DateTime.Now.Year}" + "*.log";                               // Nano
            }
        }
        private readonly string localPath;
        private long lastPosition = 0;
        private readonly object lockObj = new object();
        private CancellationTokenSource ctsCopy;
        private Task copyTask;
        private static readonly object Lock = new object();
        private string[] lastValidLine;
        public event Action<ushort> OnNewLineRead;
        public LogFileWatcher(string serverFolder, string localPath)
        {
            this.serverFolder = serverFolder;
            this.localPath = localPath;
            DeleteFile(this, EventArgs.Empty);
        }
        // Constructor tiện lợi — truyền PcType thẳng khi khởi tạo
        public LogFileWatcher(string serverFolder, string localPath, string pcType)
            : this(serverFolder, localPath)
        {
            PcType = pcType;
        }
        public void Start()
        {
            // BUG 5 FIX: cancel task cũ nếu Start() bị gọi nhiều lần
            ctsCopy?.Cancel();
            try { copyTask?.Wait(500); } catch { }
            if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(localPath));
            ctsCopy = new CancellationTokenSource();
            copyTask = Task.Run(async () =>
            {
                while (!ctsCopy.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!Directory.Exists(ServerFolder(serverFolder)))
                        {
                            await Task.Delay(2000, ctsCopy.Token);
                            continue;
                        }
                        string[] files;
                        try
                        {
                            files = Directory.GetFiles(ServerFolder(serverFolder), filePattern);
                        }
                        catch
                        {
                            await Task.Delay(2000, ctsCopy.Token);
                            continue;
                        }
                        if (files.Length == 0)
                        {
                            await Task.Delay(2000, ctsCopy.Token);
                            continue;
                        }
                        var newestFile = files.Select(f => new FileInfo(f))
                                              .OrderByDescending(f => f.CreationTime)
                                              .First();
                        bool copied = false;
                        while (!copied && !ctsCopy.Token.IsCancellationRequested)
                        {
                            try
                            {
                                using (FileStream sourceStream = new FileStream(newestFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                using (FileStream destStream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                                {
                                    destStream.SetLength(0);
                                    await sourceStream.CopyToAsync(destStream, 81920, ctsCopy.Token);
                                }
                                copied = true;
                                ParseNewLines();
                            }
                            catch (IOException) { await Task.Delay(1000, ctsCopy.Token); }
                            catch (UnauthorizedAccessException) { await Task.Delay(2000, ctsCopy.Token); }
                            catch { await Task.Delay(2000, ctsCopy.Token); }
                        }
                    }
                    catch { }
                    await Task.Delay(2000, ctsCopy.Token);
                }
            }, ctsCopy.Token);
        }
        private void ParseNewLines()
        {
            lock (lockObj)
            {
                try
                {
                    using (FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        if (fs.Length < lastPosition) lastPosition = 0;
                        fs.Seek(lastPosition, SeekOrigin.Begin);
                        string newText = sr.ReadToEnd();
                        lastPosition = fs.Position;
                        if (string.IsNullOrWhiteSpace(newText)) return;
                        string[] newLines = newText.Split(
                            new[] { Environment.NewLine },
                            StringSplitOptions.RemoveEmptyEntries);
                        for (int i = newLines.Length - 1; i >= 0; i--)
                        {
                            string line = newLines[i];
                            string[] array = line.Split(
                                new[] { ' ' },
                                StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (IsNano)   // Nano (OffMess = true)
                                {
                                    if (array[2].IndexOf('=') >= 0)
                                    {
                                        if (array.Length >= 3 &&
                                            ushort.TryParse(array[2].Split('=')[1].Split(',')[0], out ushort s1))
                                        {
                                            lastValidLine = array;
                                            OnNewLineRead?.Invoke(s1);
                                            return;
                                        }
                                        else if (array.Length >= 3 &&
                                                 ushort.TryParse(array[3].Split(',')[0], out ushort s2))
                                        {
                                            lastValidLine = array;
                                            OnNewLineRead?.Invoke(s2);
                                            return;
                                        }
                                    }
                                }
                                else          // Pamtech (OffMess = false)
                                {
                                    if (ushort.TryParse(array[3], out ushort s1))
                                    {
                                        lastValidLine = array;
                                        OnNewLineRead?.Invoke(s1);
                                        return;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }
        public void DeleteFile(object sender, EventArgs ev)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(ServerFolder(serverFolder));
                var values = dir.GetFiles(filePattern)
                                .OrderByDescending(f => f.LastWriteTime);
                foreach (var value in values)
                    File.Delete(value.FullName);
            }
            catch (IOException) { }
            catch (Exception) { }
        }
        public void Dispose()
        {
            ctsCopy?.Cancel();
            try { copyTask?.Wait(1000); } catch { }
        }
    }
}