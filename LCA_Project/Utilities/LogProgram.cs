using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace LCA_Project.Utilities
{
    public class LogProgram
    {
        private static readonly string LogPath = Path.Combine(Environment.CurrentDirectory, "LogProgram");
        // --- shared (no port) queues ---
        private static readonly Queue<Tuple<string, DateTime>> LogQueue = new Queue<Tuple<string, DateTime>>();
        private static readonly Queue<Tuple<string, DateTime>> MesLogQueue = new Queue<Tuple<string, DateTime>>();
        private static readonly object LogLock = new object();
        private static readonly object MesLogLock = new object();
        // --- per-port queues: key = portName (PORT1 / PORT2 / PORT3 / PORT4) ---
        private static readonly Dictionary<string, Queue<Tuple<string, DateTime>>> PortLogQueues
            = new Dictionary<string, Queue<Tuple<string, DateTime>>>();
        private static readonly Dictionary<string, Queue<Tuple<string, DateTime>>> PortMesLogQueues
            = new Dictionary<string, Queue<Tuple<string, DateTime>>>();
        private static readonly object PortLock = new object();
        private static bool isRunning = false;
        private static Thread logThread;
        static LogProgram()
        {
            StartLogThread();
        }
        // --- shared (no port) ---
        public static void WriteLog(string log)
        {
            lock (LogLock)
            {
                LogQueue.Enqueue(new Tuple<string, DateTime>(log, DateTime.Now));
            }
        }
        public static void MesWriteLog(string log)
        {
            lock (MesLogLock)
            {
                MesLogQueue.Enqueue(new Tuple<string, DateTime>(log, DateTime.Now));
            }
        }
        // --- per-port overloads ---
        public static void WriteLog(string log, string portName)
        {
            lock (PortLock)
            {
                if (!PortLogQueues.ContainsKey(portName))
                    PortLogQueues[portName] = new Queue<Tuple<string, DateTime>>();
                PortLogQueues[portName].Enqueue(new Tuple<string, DateTime>(log, DateTime.Now));
            }
        }
        public static void MesWriteLog(string log, string portName)
        {
            lock (PortLock)
            {
                if (!PortMesLogQueues.ContainsKey(portName))
                    PortMesLogQueues[portName] = new Queue<Tuple<string, DateTime>>();
                PortMesLogQueues[portName].Enqueue(new Tuple<string, DateTime>(log, DateTime.Now));
            }
        }
        private static void StartLogThread()
        {
            if (isRunning) return;
            isRunning = true;
            logThread = new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        FlushLogs();
                        Thread.Sleep(100);
                    }
                    catch
                    {
                        // Ignore logging error
                    }
                }
            });
            logThread.IsBackground = true;
            logThread.Start();
        }
        private static void FlushLogs()
        {
            // --- shared queues ---
            List<Tuple<string, DateTime>> logsToWrite = new List<Tuple<string, DateTime>>();
            List<Tuple<string, DateTime>> mesLogsToWrite = new List<Tuple<string, DateTime>>();
            lock (LogLock)
            {
                while (LogQueue.Count > 0) logsToWrite.Add(LogQueue.Dequeue());
            }
            lock (MesLogLock)
            {
                while (MesLogQueue.Count > 0) mesLogsToWrite.Add(MesLogQueue.Dequeue());
            }
            if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
            foreach (Tuple<string, DateTime> item in logsToWrite)
            {
                string filename = Path.Combine(LogPath, "Log_" + item.Item2.ToString("dd_MM_yyyy") + ".txt");
                File.AppendAllText(filename, "---" + item.Item2.ToString("HH:mm:ss:fff") + " " + item.Item1 + Environment.NewLine);
            }
            foreach (Tuple<string, DateTime> item in mesLogsToWrite)
            {
                string filename = Path.Combine(LogPath, "Mes_" + item.Item2.ToString("dd_MM_yyyy") + ".txt");
                File.AppendAllText(filename, "---" + item.Item2.ToString("HH:mm:ss:fff") + " " + item.Item1 + Environment.NewLine);
            }
            // --- per-port queues ---
            Dictionary<string, List<Tuple<string, DateTime>>> portLogsSnapshot;
            Dictionary<string, List<Tuple<string, DateTime>>> portMesLogsSnapshot;
            lock (PortLock)
            {
                portLogsSnapshot = new Dictionary<string, List<Tuple<string, DateTime>>>();
                foreach (var kv in PortLogQueues)
                {
                    var list = new List<Tuple<string, DateTime>>();
                    while (kv.Value.Count > 0) list.Add(kv.Value.Dequeue());
                    if (list.Count > 0) portLogsSnapshot[kv.Key] = list;
                }
                portMesLogsSnapshot = new Dictionary<string, List<Tuple<string, DateTime>>>();
                foreach (var kv in PortMesLogQueues)
                {
                    var list = new List<Tuple<string, DateTime>>();
                    while (kv.Value.Count > 0) list.Add(kv.Value.Dequeue());
                    if (list.Count > 0) portMesLogsSnapshot[kv.Key] = list;
                }
            }
            foreach (var kv in portLogsSnapshot)
            {
                string portPath = Path.Combine(LogPath, kv.Key);
                if (!Directory.Exists(portPath)) Directory.CreateDirectory(portPath);
                foreach (Tuple<string, DateTime> item in kv.Value)
                {
                    string filename = Path.Combine(portPath, "Log_" + item.Item2.ToString("dd_MM_yyyy") + ".txt");
                    File.AppendAllText(filename, "---" + item.Item2.ToString("HH:mm:ss:fff") + " " + item.Item1 + Environment.NewLine);
                }
            }
            foreach (var kv in portMesLogsSnapshot)
            {
                string portPath = Path.Combine(LogPath, kv.Key);
                if (!Directory.Exists(portPath)) Directory.CreateDirectory(portPath);
                foreach (Tuple<string, DateTime> item in kv.Value)
                {
                    string filename = Path.Combine(portPath, "Mes_" + item.Item2.ToString("dd_MM_yyyy") + ".txt");
                    File.AppendAllText(filename, "---" + item.Item2.ToString("HH:mm:ss:fff") + " " + item.Item1 + Environment.NewLine);
                }
            }
        }
        public static void Stop()
        {
            isRunning = false;
            if (logThread != null) logThread.Join();
            FlushLogs();
        }
    }
}
