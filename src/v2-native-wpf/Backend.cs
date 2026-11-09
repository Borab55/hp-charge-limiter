using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace HPChargeLimiter
{
    public class BatteryStatus
    {
        public string Ac = "unknown";
        public int Percent = 0;
        public string State = "Unknown";
        public string Energy = "-- Wh";
        public string Voltage = "-- V";
        public string PackModel = "--";
        public string Health = "-- %";
        public string Cycles = "--";
        public string Power = "0.00 W";
        public bool Bypass = false;
        public bool ServiceActive = false;
        public string Firmware = "--";
        public string LastOff = "--";
    }

    public class ServiceConfig
    {
        public int Stop = 80;
        public int Start = 75;
        public int Interval = 30;
        public string RunMode = "inhibit";
        public string OffMode = "hold";
        public int OffMin = 40;
        public string StopMode = "auto";
        public bool Installed = false;
    }

    public class GuiConfig
    {
        public string Language = "tr";
        public string Theme = "dark";
        public bool MinimizeToTray = true;
        public bool AutostartGui = false;
        public int LastStop = 80;
        public int LastStart = 75;
        public int LastInterval = 30;
        public string LastRunMode = "inhibit";
        public string LastOffMode = "hold";
        public string LastStopMode = "auto";
        public int LastOffMin = 40;
    }

    public static class Backend
    {
        public static readonly string UserAppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HPChargeLimiter"
        );
        public static readonly string ConfigFilePath = Path.Combine(UserAppDataDir, "gui_config.json");
        public static readonly string ExtractedBinDir = Path.Combine(UserAppDataDir, "bin");
        public static readonly string ExtractedExePath = Path.Combine(ExtractedBinDir, "hp-charge-limit-rs.exe");

        public static readonly string ProgramDataDir = @"C:\ProgramData\hp-charge-limit-rs";
        public static readonly string ProgramFilesDir = @"C:\Program Files\hp-charge-limit-rs";
        public static readonly string InstalledExePath = @"C:\Program Files\hp-charge-limit-rs\hp-charge-limit-rs.exe";
        public static readonly string LogFilePath = @"C:\ProgramData\hp-charge-limit-rs\daemon.log";
        public static readonly string OffstateFilePath = @"C:\ProgramData\hp-charge-limit-rs\offstate.last";

        private const string BypassRegKey = @"SOFTWARE\hp-charge-limit-rs-bypass";
        private static readonly UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        private const int REG_OPTION_VOLATILE = 0x00000001;
        private const int KEY_WRITE = 0x20006;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegCreateKeyExW(
            UIntPtr hKey, string lpSubKey, int Reserved, string lpClass,
            int dwOptions, int samDesired, IntPtr lpSecurityAttributes,
            out IntPtr phkResult, out int lpdwDisposition);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegDeleteKeyW(UIntPtr hKey, string lpSubKey);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHELLEXECUTEINFOW
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpVerb;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpFile;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpParameters;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool ShellExecuteExW(ref SHELLEXECUTEINFOW lpExecInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static GuiConfig _cachedConfig;

        public static bool IsAdmin()
        {
            try
            {
                using (WindowsIdentity id = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(id);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch { return false; }
        }

        public static int RunElevatedFast(string file, string parameters, bool wait = true)
        {
            const uint SEE_MASK_NOCLOSEPROCESS = 0x00000040;
            SHELLEXECUTEINFOW sei = new SHELLEXECUTEINFOW();
            sei.cbSize = Marshal.SizeOf(typeof(SHELLEXECUTEINFOW));
            sei.fMask = SEE_MASK_NOCLOSEPROCESS;
            sei.lpVerb = "runas";
            sei.lpFile = file;
            sei.lpParameters = parameters;
            sei.lpDirectory = File.Exists(file) ? Path.GetDirectoryName(file) : Environment.CurrentDirectory;
            sei.nShow = 0; // SW_HIDE

            if (!ShellExecuteExW(ref sei))
                return -1;

            if (wait && sei.hProcess != IntPtr.Zero)
            {
                WaitForSingleObject(sei.hProcess, 0xFFFFFFFF);
                uint code = 0;
                GetExitCodeProcess(sei.hProcess, out code);
                CloseHandle(sei.hProcess);
                return (int)code;
            }
            return 0;
        }

        public static string GetEmbeddedEnginePath()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream s = asm.GetManifestResourceStream("HPChargeLimiter.CoreEngine.exe"))
                {
                    if (s != null)
                    {
                        Directory.CreateDirectory(ExtractedBinDir);
                        bool needsWrite = !File.Exists(ExtractedExePath);
                        if (!needsWrite)
                        {
                            FileInfo fi = new FileInfo(ExtractedExePath);
                            if (fi.Length != s.Length) needsWrite = true;
                        }
                        if (needsWrite)
                        {
                            using (FileStream fs = new FileStream(ExtractedExePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                            {
                                s.CopyTo(fs);
                            }
                        }
                        return ExtractedExePath;
                    }
                }
            }
            catch { }

            string localNextToExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hp-charge-limit-rs.exe");
            if (File.Exists(localNextToExe))
                return localNextToExe;

            if (File.Exists(InstalledExePath))
                return InstalledExePath;

            return ExtractedExePath;
        }

        public static string GetActiveCliPath(bool preferEmbedded = false)
        {
            if (!preferEmbedded && File.Exists(InstalledExePath))
                return InstalledExePath;
            return GetEmbeddedEnginePath();
        }

        public static void EnsureEngineInstalledInProgramFiles()
        {
            if (File.Exists(InstalledExePath)) return;
            try
            {
                string src = GetEmbeddedEnginePath();
                if (File.Exists(src))
                {
                    Directory.CreateDirectory(ProgramFilesDir);
                    File.Copy(src, InstalledExePath, true);
                }
            }
            catch { }
        }

        public static int RunCli(string args, out string stdout, out string stderr, bool requireAdmin = false, bool preferEmbedded = false)
        {
            stdout = "";
            stderr = "";
            string cli = GetActiveCliPath(preferEmbedded);
            if (!File.Exists(cli))
            {
                stderr = "Core engine executable not found: " + cli;
                return -1;
            }

            if (requireAdmin && !IsAdmin())
            {
                int code = RunElevatedFast(cli, args, true);
                stdout = code == 0 ? "OK" : "";
                stderr = code != 0 ? "Operation cancelled or elevation failed." : "";
                return code;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = cli,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };
                using (Process p = Process.Start(psi))
                {
                    stdout = p.StandardOutput.ReadToEnd();
                    stderr = p.StandardError.ReadToEnd();
                    p.WaitForExit(35000);
                    return p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                stderr = ex.Message;
                return -1;
            }
        }

        public static bool SetBypassDirectWin32(bool enable)
        {
            try
            {
                if (enable)
                {
                    IntPtr hKey;
                    int disp;
                    int ret = RegCreateKeyExW(
                        HKEY_LOCAL_MACHINE, BypassRegKey, 0, null,
                        REG_OPTION_VOLATILE, KEY_WRITE, IntPtr.Zero,
                        out hKey, out disp);
                    if (ret == 0)
                    {
                        RegCloseKey(hKey);
                        return true;
                    }
                    return false;
                }
                else
                {
                    int ret = RegDeleteKeyW(HKEY_LOCAL_MACHINE, BypassRegKey);
                    return ret == 0 || ret == 2;
                }
            }
            catch { return false; }
        }

        public static bool SetBypass(bool enable, out string message)
        {
            if (IsAdmin() && SetBypassDirectWin32(enable))
            {
                message = enable ? "Bypass enabled (100%)" : "Bypass disabled";
                return true;
            }
            string stdout, stderr;
            int code = RunCli("bypass " + (enable ? "on" : "off"), out stdout, out stderr, true, false);
            message = code == 0 ? "OK" : (string.IsNullOrEmpty(stderr) ? stdout : stderr);
            return code == 0;
        }

        public static BatteryStatus GetBatteryStatus()
        {
            BatteryStatus st = new BatteryStatus();
            string stdout, stderr;
            int ret = RunCli("status", out stdout, out stderr, false, false);
            if (ret != 0 && string.IsNullOrEmpty(stdout))
                return st;

            string[] lines = stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.StartsWith("AC:"))
                {
                    st.Ac = line.IndexOf("online", StringComparison.OrdinalIgnoreCase) >= 0 ? "online" : "offline";
                }
                else if (line.StartsWith("battery:"))
                {
                    Match mPct = Regex.Match(line, @"(\d+)%");
                    if (mPct.Success) int.TryParse(mPct.Groups[1].Value, out st.Percent);

                    if (line.Contains("Not charging")) st.State = "Not charging";
                    else if (line.Contains("Charging")) st.State = "Charging";
                    else if (line.Contains("Discharging")) st.State = "Discharging";

                    Match mWh = Regex.Match(line, @"([\d\.]+\s*Wh)");
                    if (mWh.Success) st.Energy = mWh.Groups[1].Value;

                    Match mV = Regex.Match(line, @"([\d\.]+\s*V)");
                    if (mV.Success) st.Voltage = mV.Groups[1].Value;
                }
                else if (line.StartsWith("pack:"))
                {
                    string val = line.Substring(5).Trim();
                    string[] parts = val.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                        st.PackModel = string.Join(" ", parts, 1, parts.Length - 1);
                    else if (parts.Length == 1)
                        st.PackModel = parts[0];
                }
                else if (line.StartsWith("health:"))
                {
                    Match mH = Regex.Match(line, @"([\d\.]+)%");
                    if (mH.Success) st.Health = mH.Groups[1].Value + "%";

                    Match mC = Regex.Match(line, @"cycles:\s*(\d+)");
                    if (mC.Success) st.Cycles = mC.Groups[1].Value;

                    Match mP = Regex.Match(line, @"power:\s*([\d\.]+\s*W)");
                    if (mP.Success) st.Power = mP.Groups[1].Value;
                }
                else if (line.StartsWith("bypass:"))
                {
                    st.Bypass = line.Contains("enabled");
                }
                else if (line.StartsWith("firmware:"))
                {
                    string fw = line.Substring(9).Trim();
                    if (fw.IndexOf("unreadable", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        st.Firmware = InferFirmwareModeFromLogOrState(st.State, st.Ac);
                    }
                    else
                    {
                        st.Firmware = fw.Split(',')[0].Trim();
                    }
                }
                else if (line.StartsWith("service:"))
                {
                    st.ServiceActive = line.Contains("active") && !line.Contains("inactive");
                }
                else if (line.StartsWith("last off:"))
                {
                    st.LastOff = line.Substring(9).Trim();
                }
            }

            if (st.LastOff == "--" && File.Exists(OffstateFilePath))
            {
                try
                {
                    string offText = File.ReadAllText(OffstateFilePath, Encoding.UTF8).Trim();
                    if (!string.IsNullOrEmpty(offText)) st.LastOff = offText;
                }
                catch { }
            }
            return st;
        }

        private static string InferFirmwareModeFromLogOrState(string batteryState, string acState)
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    using (FileStream fs = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string lastMode = null;
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Match m = Regex.Match(line, @"->\s*(inhibit|hold|auto)\b", RegexOptions.IgnoreCase);
                            if (m.Success)
                                lastMode = m.Groups[1].Value.ToLowerInvariant();
                        }
                        if (lastMode == "inhibit") return "0x0500 (inhibit)";
                        if (lastMode == "hold") return "0x0300 (hold)";
                        if (lastMode == "auto") return "0x0000 (auto)";
                    }
                }
            }
            catch { }

            if (acState == "online" && batteryState == "Not charging")
                return "0x0500 (inhibit)";
            if (batteryState == "Charging")
                return "0x0000 (auto)";
            return "--";
        }

        public static ServiceConfig GetServiceConfig()
        {
            GuiConfig guiCfg = LoadGuiConfig();
            ServiceConfig cfg = new ServiceConfig
            {
                Stop = guiCfg.LastStop,
                Start = guiCfg.LastStart,
                Interval = guiCfg.LastInterval,
                RunMode = guiCfg.LastRunMode,
                OffMode = guiCfg.LastOffMode,
                OffMin = guiCfg.LastOffMin,
                StopMode = guiCfg.LastStopMode,
                Installed = false
            };

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = "qc hp-charge-limit-rs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (Process p = Process.Start(psi))
                {
                    string outText = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(3000);
                    if (p.ExitCode == 0)
                    {
                        cfg.Installed = true;
                        Match m = Regex.Match(outText, @"BINARY_PATH_NAME\s+:\s+(.*)");
                        if (m.Success)
                        {
                            string cmd = m.Groups[1].Value.Trim();
                            Match mStop = Regex.Match(cmd, @"--stop[=\s]+(\d+)");
                            Match mStart = Regex.Match(cmd, @"--start[=\s]+(\d+)");
                            Match mInt = Regex.Match(cmd, @"--interval[=\s]+(\d+)");
                            Match mRun = Regex.Match(cmd, @"--run-mode[=\s]+(\w+)");
                            Match mOff = Regex.Match(cmd, @"--off-mode[=\s]+(\w+)");
                            Match mOffMin = Regex.Match(cmd, @"--off-min[=\s]+(\d+)");
                            Match mStopMode = Regex.Match(cmd, @"--stop-mode[=\s]+(\w+)");

                            if (mStop.Success) int.TryParse(mStop.Groups[1].Value, out cfg.Stop);
                            if (mStart.Success) int.TryParse(mStart.Groups[1].Value, out cfg.Start);
                            if (mInt.Success) int.TryParse(mInt.Groups[1].Value, out cfg.Interval);
                            if (mRun.Success) cfg.RunMode = mRun.Groups[1].Value;
                            if (mOff.Success) cfg.OffMode = mOff.Groups[1].Value;
                            if (mOffMin.Success) int.TryParse(mOffMin.Groups[1].Value, out cfg.OffMin);
                            if (mStopMode.Success) cfg.StopMode = mStopMode.Groups[1].Value;
                        }
                    }
                }
            }
            catch { }
            return cfg;
        }

        public static bool SaveAndApplyServiceConfig(ServiceConfig cfg, out string message)
        {
            if (IsAdmin())
                EnsureEngineInstalledInProgramFiles();

            string args = string.Format(
                "service install --stop={0} --start={1} --interval={2} --run-mode={3} --off-mode={4} --off-min={5} --stop-mode={6}",
                cfg.Stop, cfg.Start, cfg.Interval, cfg.RunMode, cfg.OffMode, cfg.OffMin, cfg.StopMode
            );

            string stdout, stderr;
            int code = RunCli(args, out stdout, out stderr, true, true);
            if (code == 0)
            {
                GuiConfig g = LoadGuiConfig();
                g.LastStop = cfg.Stop;
                g.LastStart = cfg.Start;
                g.LastInterval = cfg.Interval;
                g.LastRunMode = cfg.RunMode;
                g.LastOffMode = cfg.OffMode;
                g.LastOffMin = cfg.OffMin;
                g.LastStopMode = cfg.StopMode;
                SaveGuiConfig(g);
                message = string.IsNullOrEmpty(stdout) ? "OK" : stdout;
                return true;
            }
            message = string.IsNullOrEmpty(stderr) ? stdout : stderr;
            return false;
        }

        public static bool UninstallService(out string message)
        {
            string stdout, stderr;
            int code = RunCli("service uninstall", out stdout, out stderr, true, true);
            message = code == 0 ? (string.IsNullOrEmpty(stdout) ? "Service uninstalled." : stdout) : (string.IsNullOrEmpty(stderr) ? stdout : stderr);
            return code == 0;
        }

        public static bool FullSystemCleanup(out string message)
        {
            try
            {
                SetAutostartGui(false);
                string embeddedExe = GetEmbeddedEnginePath();

                if (IsAdmin())
                {
                    string so, se;
                    RunCli("set auto", out so, out se, false, true);
                    RunCli("service uninstall", out so, out se, false, true);
                    SetBypassDirectWin32(false);

                    if (Directory.Exists(ProgramFilesDir))
                        try { Directory.Delete(ProgramFilesDir, true); } catch { }
                    if (Directory.Exists(ProgramDataDir))
                        try { Directory.Delete(ProgramDataDir, true); } catch { }
                }
                else
                {
                    string cmdChain = string.Format(
                        "\"{0}\" set auto & \"{0}\" service uninstall & reg delete \"HKLM\\{1}\" /f >nul 2>&1 & rmdir /s /q \"{2}\" >nul 2>&1 & rmdir /s /q \"{3}\" >nul 2>&1",
                        embeddedExe, BypassRegKey, ProgramFilesDir, ProgramDataDir
                    );
                    int code = RunElevatedFast("cmd.exe", "/c \"" + cmdChain + "\"", true);
                    if (code == -1)
                    {
                        message = "UAC elevation cancelled.";
                        return false;
                    }
                }

                try
                {
                    if (File.Exists(ConfigFilePath))
                        File.Delete(ConfigFilePath);
                }
                catch { }

                _cachedConfig = null;
                message = "OK";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static bool SetServiceState(string action, out string message)
        {
            if (!IsAdmin())
            {
                int code = RunElevatedFast("sc.exe", action + " hp-charge-limit-rs", true);
                message = code == 0 ? "Operation completed." : "Operation failed or cancelled.";
                return code == 0;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = action + " hp-charge-limit-rs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (Process p = Process.Start(psi))
                {
                    string so = p.StandardOutput.ReadToEnd();
                    string se = p.StandardError.ReadToEnd();
                    p.WaitForExit(5000);
                    message = string.IsNullOrEmpty(so) ? se : so;
                    return p.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static string ReadLogFile(int maxLines = 300)
        {
            if (!File.Exists(LogFilePath))
                return "Log dosyası henüz oluşmadı veya temizlendi (C:\\ProgramData\\hp-charge-limit-rs\\daemon.log)";
            try
            {
                using (FileStream fs = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    List<string> lines = new List<string>();
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        lines.Add(line);
                    int start = Math.Max(0, lines.Count - maxLines);
                    return string.Join(Environment.NewLine, lines.GetRange(start, lines.Count - start));
                }
            }
            catch (Exception ex)
            {
                return "Log error: " + ex.Message;
            }
        }

        public static void ClearLogFile()
        {
            try
            {
                if (File.Exists(LogFilePath))
                    File.WriteAllText(LogFilePath, string.Format("--- Log cleared at {0:yyyy-MM-dd HH:mm:ss} ---{1}", DateTime.Now, Environment.NewLine), Encoding.UTF8);
            }
            catch { }
        }

        public static GuiConfig LoadGuiConfig()
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            GuiConfig cfg = new GuiConfig();
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigFilePath, Encoding.UTF8);
                    Match mLang = Regex.Match(json, @"""language""\s*:\s*""(\w+)""");
                    Match mTheme = Regex.Match(json, @"""theme""\s*:\s*""(\w+)""");
                    Match mTray = Regex.Match(json, @"""minimize_to_tray""\s*:\s*(true|false)");
                    Match mAuto = Regex.Match(json, @"""autostart_gui""\s*:\s*(true|false)");
                    Match mStop = Regex.Match(json, @"""last_stop""\s*:\s*(\d+)");
                    Match mStart = Regex.Match(json, @"""last_start""\s*:\s*(\d+)");
                    Match mInt = Regex.Match(json, @"""last_interval""\s*:\s*(\d+)");
                    Match mRun = Regex.Match(json, @"""last_run_mode""\s*:\s*""(\w+)""");
                    Match mOff = Regex.Match(json, @"""last_off_mode""\s*:\s*""(\w+)""");
                    Match mStopM = Regex.Match(json, @"""last_stop_mode""\s*:\s*""(\w+)""");
                    Match mOffMin = Regex.Match(json, @"""last_off_min""\s*:\s*(\d+)");

                    if (mLang.Success) cfg.Language = mLang.Groups[1].Value;
                    if (mTheme.Success) cfg.Theme = mTheme.Groups[1].Value;
                    if (mTray.Success) cfg.MinimizeToTray = mTray.Groups[1].Value == "true";
                    if (mAuto.Success) cfg.AutostartGui = mAuto.Groups[1].Value == "true";
                    if (mStop.Success) int.TryParse(mStop.Groups[1].Value, out cfg.LastStop);
                    if (mStart.Success) int.TryParse(mStart.Groups[1].Value, out cfg.LastStart);
                    if (mInt.Success) int.TryParse(mInt.Groups[1].Value, out cfg.LastInterval);
                    if (mRun.Success) cfg.LastRunMode = mRun.Groups[1].Value;
                    if (mOff.Success) cfg.LastOffMode = mOff.Groups[1].Value;
                    if (mStopM.Success) cfg.LastStopMode = mStopM.Groups[1].Value;
                    if (mOffMin.Success) int.TryParse(mOffMin.Groups[1].Value, out cfg.LastOffMin);
                }
                catch { }
            }
            _cachedConfig = cfg;
            return cfg;
        }

        public static void SaveGuiConfig(GuiConfig cfg)
        {
            _cachedConfig = cfg;
            try
            {
                Directory.CreateDirectory(UserAppDataDir);
                string json = string.Format(
                    "{{\n  \"language\": \"{0}\",\n  \"theme\": \"{10}\",\n  \"minimize_to_tray\": {1},\n  \"autostart_gui\": {2},\n  \"last_stop\": {3},\n  \"last_start\": {4},\n  \"last_interval\": {5},\n  \"last_run_mode\": \"{6}\",\n  \"last_off_mode\": \"{7}\",\n  \"last_stop_mode\": \"{8}\",\n  \"last_off_min\": {9}\n}}",
                    cfg.Language,
                    cfg.MinimizeToTray ? "true" : "false",
                    cfg.AutostartGui ? "true" : "false",
                    cfg.LastStop,
                    cfg.LastStart,
                    cfg.LastInterval,
                    cfg.LastRunMode,
                    cfg.LastOffMode,
                    cfg.LastStopMode,
                    cfg.LastOffMin,
                    cfg.Theme
                );
                File.WriteAllText(ConfigFilePath, json, Encoding.UTF8);
            }
            catch { }
        }

        public static bool IsAutostartGuiEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    if (key == null) return false;
                    return key.GetValue("HPChargeLimiterGUI") != null;
                }
            }
            catch { return false; }
        }

        public static void SetAutostartGui(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null) return;
                    if (enable)
                    {
                        string exe = Process.GetCurrentProcess().MainModule.FileName;
                        key.SetValue("HPChargeLimiterGUI", "\"" + exe + "\" --minimized");
                    }
                    else
                    {
                        key.DeleteValue("HPChargeLimiterGUI", false);
                    }
                }
            }
            catch { }
        }

        public static void RelaunchAsAdmin()
        {
            string exe = Process.GetCurrentProcess().MainModule.FileName;
            RunElevatedFast(exe, "--restarted", false);
        }
    }
}
