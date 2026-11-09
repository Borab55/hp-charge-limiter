using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

[assembly: AssemblyTitle("HP Battery Charge Limiter (v2.4 Native GPU Edition)")]
[assembly: AssemblyDescription("Victus / OMEN / Pavilion Smart Battery & Charge Manager")]
[assembly: AssemblyCompany("hp-charge-limit-rs & HPChargeLimiter Contributors")]
[assembly: AssemblyProduct("HP Battery Charge Limiter")]
[assembly: AssemblyCopyright("Copyright (c) 2026 hp-charge-limit-rs contributors (MIT OR Apache-2.0)")]
[assembly: AssemblyVersion("2.4.0.0")]
[assembly: AssemblyFileVersion("2.4.0.0")]

namespace HPChargeLimiter
{
    public static class Program
    {
        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int value);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHChangeNotify(int wEventId, uint uFlags, string dwItem1, IntPtr dwItem2);

        private static Mutex _mutex;

        [STAThread]
        public static void Main(string[] args)
        {
            try { SetProcessDpiAwareness(2); }
            catch
            {
                try { SetProcessDPIAware(); } catch { }
            }

            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                // SHCNE_UPDATEITEM (0x00002000) with SHCNF_PATHW (0x0005) refreshes Explorer's icon for this .exe
                SHChangeNotify(0x00002000, 0x0005, exePath, IntPtr.Zero);
            }
            catch { }

            bool isRestarted = false;
            bool startMinimized = false;
            if (args != null)
            {
                foreach (string a in args)
                {
                    if (string.Equals(a, "--restarted", StringComparison.OrdinalIgnoreCase))
                        isRestarted = true;
                    else if (string.Equals(a, "--minimized", StringComparison.OrdinalIgnoreCase))
                        startMinimized = true;
                }
            }

            bool createdNew = false;
            int attempts = isRestarted ? 30 : 1;
            for (int i = 0; i < attempts; i++)
            {
                _mutex = new Mutex(true, @"Global\HP_CHARGE_LIMITER_GUI_MUTEX", out createdNew);
                if (createdNew) break;
                _mutex.Dispose();
                _mutex = null;
                if (isRestarted) Thread.Sleep(100);
            }

            if (!createdNew)
                return;

            Application app = new Application();
            MainWindow win = new MainWindow(startMinimized);
            app.Run(win);
            ReleaseMutex();
        }

        public static void ReleaseMutex()
        {
            if (_mutex != null)
            {
                try { _mutex.ReleaseMutex(); } catch { }
                try { _mutex.Dispose(); } catch { }
                _mutex = null;
            }
        }
    }
}
