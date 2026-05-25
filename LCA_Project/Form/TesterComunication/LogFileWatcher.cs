using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Form.TesterComunication
{
    public class LogFileWatcher
    {
        public string serverFolder { get; set; }
        private string ServerFolder(string path)
        {
                if (!OffMess) return path;
                else
                {
                return Path.Combine(path, $"{DateTime.Now.Year.ToString()}", $"{DateTime.Now.Month.ToString("D2")}");
            }
        }
        private string filePattern
        {
            get
            {
                if (!OffMess) return $"-{DateTime.Now.Year.ToString().Substring(2)}" + "*.txt";
                else
                {
                    return $"{DateTime.Now.Year.ToString("")}" + "*.log";
                }
            }
        }
        private readonly string localPath;
        private FileSystemWatcher watcher;
        private long lastPosition = 0;
        private bool skipFirstEvent = true;
        private readonly object lockObj = new object();
        private CancellationTokenSource ctsCopy;
        private Task copyTask;
        public static bool OffMess = true;
        private static object Lock = new object();
        private string[] lastValidLine;
        public event Action<ushort> OnNewLineRead;
        public LogFileWatcher(string serverFolder, string localPath)
        {
            this.serverFolder = serverFolder;
            this.localPath = localPath;
            DeleteFile(this, EventArgs.Empty);
        }
        public void Start()
        {
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
                            catch (IOException)
                            {
                                await Task.Delay(1000, ctsCopy.Token); 
                            }
                            catch (UnauthorizedAccessException)
                            {
                                await Task.Delay(2000, ctsCopy.Token); 
                            }
                            catch
                            {
                                await Task.Delay(2000, ctsCopy.Token); 
                            }
                        }
                    }
                    catch
                    {
                        // Ignore, loop tiếp tục retry
                    }
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
                        string[] newLines = newText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = newLines.Length - 1; i >= 0; i--)
                        {
                            string line = newLines[i];
                            string[] array = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (!OffMess == false)
                                {
                                    if (array[2].IndexOf('=') >= 0)
                                    {
                                        if (array.Length >= 3 && ushort.TryParse(array[2].Split('=')[1].Split(',')[0], out ushort s1))
                                        {
                                            lastValidLine = array;
                                            OnNewLineRead?.Invoke(s1);
                                            return;
                                        }
                                        else if (array.Length >= 3 && ushort.TryParse(array[3].Split(',')[0], out ushort s2))
                                        {
                                            lastValidLine = array;
                                            OnNewLineRead?.Invoke(s2);
                                            return;
                                        }
                                    }
                                }
                                else
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
                catch
                {
                }
            }
        }
        public void DeleteFile(object sender,EventArgs ev)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(ServerFolder(serverFolder));
            var values = dir.GetFiles(this.filePattern).OrderByDescending(f => f.LastWriteTime);
                if (values == null)
                {
                   // MessageBox.Show("Không Tìm Thấy File");
                    return;
                }
                foreach (var value in values)
                {
                    File.Delete(value.FullName);
                }
            }
            catch (IOException ex)
            {
               //Console.WriteLine("Không Thể Xóa Do File Đang Được Truy Cập");
            }
            catch (Exception e)
            {
              //  Console.WriteLine($"ERROR : {e}");
            }
        }
        public void Dispose()
        {
            ctsCopy?.Cancel();
            try { copyTask?.Wait(1000); } catch { }
        }
    }
}
