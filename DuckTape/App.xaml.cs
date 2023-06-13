using System.Windows;
using System.Threading;
using System;
using System.Runtime.InteropServices;

namespace DuckTape
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static int CaptchaLength { get; internal set; }


        private Mutex mutex;
        private const string MutexName = "YourAppNameUniqueMutexName";

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;

            mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                // Mutex already exists, another instance is running
                BringRunningInstanceToFront();
                Shutdown();
                return;
            }

            base.OnStartup(e);

        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.ReleaseMutex();
            mutex?.Close();
            mutex = null;

            base.OnExit(e);
        }

        private void BringRunningInstanceToFront()
        {
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            var processes = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (var process in processes)
            {
                // Find the running instance with the same process ID but different MainWindowHandle
                if (process.Id != currentProcess.Id && process.MainWindowHandle != IntPtr.Zero)
                {
                    NativeMethods.ShowWindow(process.MainWindowHandle, NativeMethods.SW_RESTORE);
                    NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                    NativeMethods.CenterWindow(process.MainWindowHandle);
                    break;
                }
            }
        }


        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);
            internal const int SW_RESTORE = 9;

            [DllImport("user32.dll")]
            internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

            internal static void CenterWindow(IntPtr hWnd)
            {
                if (GetWindowRect(hWnd, out var rect))
                {
                    int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                    int screenHeight = (int)SystemParameters.PrimaryScreenHeight;
                    int windowWidth = rect.right - rect.left;
                    int windowHeight = rect.bottom - rect.top;
                    int left = Math.Max((screenWidth - windowWidth) / 2, 0);
                    int top = Math.Max((screenHeight - windowHeight) / 2, 0);

                    MoveWindow(hWnd, left, top, windowWidth, windowHeight, true);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

        }
    }
}
