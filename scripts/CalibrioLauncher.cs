using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

internal static class CalibrioLauncher
{
    private const string Port = "3001";
    private const string AppUrl = "http://localhost:3001/";
    private const string MutexName = "CalibrioDesktopLauncher";

    [STAThread]
    private static int Main(string[] args)
    {
        using (new Mutex(true, MutexName))
        {
            try
            {
                if (args.Length > 0 && string.Equals(args[0], "--self-check", StringComparison.OrdinalIgnoreCase))
                {
                    RunSelfCheck();
                    return 0;
                }

                StartServer();
                WaitForServer(TimeSpan.FromSeconds(12));
                OpenAppWindow();
                return 0;
            }
            catch (Exception ex)
            {
                WriteLog("Launcher failed: " + ex);
                MessageBox.Show(
                    "Calibrio could not start.\n\n" + ex.Message,
                    "Calibrio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }
        }
    }

    private static void RunSelfCheck()
    {
        string root = AppDomain.CurrentDomain.BaseDirectory;
        RequireFile(Path.Combine(root, "app", "start-server.cmd"), "application launcher");
        RequireFile(Path.Combine(root, "app", "desktop-server.mjs"), "application server");
        RequireFile(Path.Combine(root, "runtime", "node.exe"), "bundled runtime");
    }

    private static void RequireFile(string path, string label)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Missing " + label + ": " + path, path);
        }
    }

    private static void StartServer()
    {
        string root = AppDomain.CurrentDomain.BaseDirectory;
        string appDir = Path.Combine(root, "app");
        string serverScript = Path.Combine(appDir, "start-server.cmd");
        string nodePath = Path.Combine(root, "runtime", "node.exe");

        RequireFile(serverScript, "application launcher");
        RequireFile(Path.Combine(appDir, "desktop-server.mjs"), "application server");
        RequireFile(nodePath, "bundled runtime");

        StopBundledServer(nodePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = serverScript,
            Arguments = Port,
            WorkingDirectory = appDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        Process.Start(startInfo);
    }

    private static void StopBundledServer(string nodePath)
    {
        foreach (int processId in FindListeningProcesses(Port))
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    string processPath = null;
                    try
                    {
                        processPath = process.MainModule.FileName;
                    }
                    catch
                    {
                        processPath = null;
                    }

                    if (processPath != null && string.Equals(Path.GetFullPath(processPath), Path.GetFullPath(nodePath), StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                        process.WaitForExit(2500);
                    }
                }
            }
            catch
            {
            }
        }
    }

    private static int[] FindListeningProcesses(string port)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, "netstat.exe"),
                Arguments = "-ano -p tcp",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(3000);

                var ids = new System.Collections.Generic.List<int>();
                foreach (string rawLine in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string line = rawLine.Trim();
                    if (!line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase)) continue;
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 5) continue;
                    if (!parts[1].EndsWith(":" + port, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!string.Equals(parts[3], "LISTENING", StringComparison.OrdinalIgnoreCase)) continue;
                    int id;
                    if (int.TryParse(parts[4], out id)) ids.Add(id);
                }
                return ids.ToArray();
            }
        }
        catch
        {
            return new int[0];
        }
    }

    private static void WaitForServer(TimeSpan timeout)
    {
        DateTime stopAt = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < stopAt)
        {
            if (IsCalibrioReady()) return;
            Thread.Sleep(250);
        }
    }

    private static bool IsCalibrioReady()
    {
        try
        {
            var request = (HttpWebRequest)WebRequest.Create(AppUrl);
            request.Timeout = 1000;
            request.ReadWriteTimeout = 1000;
            request.UserAgent = "CalibrioLauncher";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return (int)response.StatusCode >= 200 && (int)response.StatusCode < 500;
            }
        }
        catch
        {
            return false;
        }
    }

    private static void OpenAppWindow()
    {
        string brave = FindBrave();
        if (!string.IsNullOrEmpty(brave))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = brave,
                Arguments = "--new-window " + AppUrl,
                UseShellExecute = false
            });
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = AppUrl,
            UseShellExecute = true
        });
    }

    private static string FindBrave()
    {
        string[] candidates =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "BraveSoftware", "Brave-Browser", "Application", "brave.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "BraveSoftware", "Brave-Browser", "Application", "brave.exe")
        };

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private static void WriteLog(string message)
    {
        try
        {
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(Path.Combine(logDir, "launcher.log"), DateTime.Now.ToString("s") + " " + message + Environment.NewLine);
        }
        catch
        {
        }
    }
}
