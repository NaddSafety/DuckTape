using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using DuckTape.MVVM.Model;


namespace DuckTape.MVVM.View
{
    public partial class HomeView : UserControl
    {
        private const int WH_KEYBOARD_LL = 13;

        private IntPtr hookId = IntPtr.Zero;
        private LowLevelKeyboardProc keyboardProc;

        private readonly DeviceConnectionManager _deviceConnectionManager;
        public HomeView()
        {
            InitializeComponent();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "slider_value.txt");

            if (File.Exists(filePath))
            {
                string sliderValueString = File.ReadAllText(filePath);
                if (double.TryParse(sliderValueString, out double sliderValue))
                {
                    slider1.Value = sliderValue;
                }
            }


            _deviceConnectionManager = new DeviceConnectionManager();
            _deviceConnectionManager.DevicesConnected += OnDevicesConnected;
            CaptchaLength = (int)slider1.Value;
            slider1.ValueChanged += Slider_ValueChanged;
            slider1.ValueChanged += CaptchaSlider_ValueChanged;
            _deviceConnectionManager.Start();
            keyboardProc = KeyboardHookCallback;
            Button_Click_2(null, null);
            Button_Click(null, null);
        }


        private void CaptchaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CaptchaLength = (int)e.NewValue;
        }
       
        private bool _isWindow1Open = false;

        private void OnDevicesConnected(object sender, EventArgs e)
        {
            
            if (!_isWindow1Open)
            {

                Button_Click_1(null, null);
                _isWindow1Open = true;
            }
         
        }

     
        public void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "slider_value.txt");
            File.WriteAllText(filePath, slider1.Value.ToString());
            Button_Click_2(null, null);
        }


        public List<USBDeviceInfo> GetConnectedUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            var searcher = new ManagementObjectSearcher("SELECT DeviceID FROM Win32_USBHub");
            foreach (var device in searcher.Get())
            {
                var deviceID = (string)device.GetPropertyValue("DeviceID");
                var deviceInfo = new USBDeviceInfo(deviceID);
                devices.Add(deviceInfo);
            }

            return devices;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Update the ListBox's ItemsSource with the connected USB devices
            List<USBDeviceInfo> devices = GetConnectedUSBDevices();
            DeviceList.ItemsSource = devices;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {


            Application.Current.Dispatcher.Invoke(() =>
            {
                Window2 window2 = new Window2
                {
                    Owner = Window.GetWindow(this)
                };
                window2.Show();
                window2.IsChecked = checkBox.IsChecked;
            });
        }



        private int _captchaLength = 3;

        public int CaptchaLength
        {

            get { return _captchaLength; }
            set
            {
                _captchaLength = value;
                label21.Content = $"Captcha Length: {_captchaLength}";
                byte grayValue = (byte)(255 * (1 - (_captchaLength - 3) / 5.0));
                label21.Foreground = new SolidColorBrush(Color.FromRgb(grayValue, grayValue, grayValue));
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _captchaLength = (int)slider1.Value;
            App.CaptchaLength = CaptchaLength;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {


            try
            {
                RegistryKey regkey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
                regkey.SetValue("NoWinKeys", 1, RegistryValueKind.DWord);
                regkey.Close();

                ManagementObject groupPolicyObject = new ManagementObject("root\\CIMV2", "Win32_GroupPolicyObject.Name='LocalGPO'", null);
                ManagementBaseObject inParams = groupPolicyObject.GetMethodParameters("GetRegistryValue");
                inParams["RegistryKey"] = @"SOFTWARE\Policies\Microsoft\Windows\Explorer";
                inParams["RegistryValue"] = "NoWinKeys";
                inParams["UsePrecedence"] = false;
                ManagementBaseObject outParams = groupPolicyObject.InvokeMethod("GetRegistryValue", inParams, null);
                outParams = groupPolicyObject.InvokeMethod("SetRegistryValue", inParams, null);
                MessageBox.Show("Windows key hotkeys have been disabled.Such changes require a restart to function", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
                const string keyName = "ConsentPromptBehaviorAdmin";

                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(keyPath, true);

                if (regKey != null)
                {
                    try
                    {
                        // Set the value to 1
                        regKey.SetValue(keyName, 1, RegistryValueKind.DWord);

                        MessageBox.Show("Registry value updated successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to update registry value: " + ex.Message);
                    }
                    finally
                    {
                        regKey.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to open registry key: " + keyPath);
                }
            }
            catch (SecurityException)
            {
                // Handle the SecurityException
                MessageBox.Show("Registry access is not allowed. Please contact your system administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        // ...

        private void AutoStartButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the current executable path
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            // Create or open the registry key for the current user's "Run" key
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // Check if the application is already set to start with Windows startup
            if (IsAutoStartEnabled(registryKey))
            {
                // Application is already set to start with Windows startup, so remove it
                registryKey.DeleteValue("MyApp");
                MessageBox.Show("Application removed from Windows startup.");
            }
            else
            {
                // Application is not set to start with Windows startup, so add it
                registryKey.SetValue("MyApp", appPath);
                MessageBox.Show("Application added to Windows startup.");
            }
        }

        // Helper method to check if the application is set to start with Windows startup
        private bool IsAutoStartEnabled(RegistryKey registryKey)
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string value = registryKey.GetValue("MyApp") as string;
            return !string.IsNullOrEmpty(value) && value.Equals(appPath, StringComparison.InvariantCultureIgnoreCase);
        }

        private void EnableDisableButton_Click(object sender, RoutedEventArgs e)
        {

            if (hookId == IntPtr.Zero)
            {
                // Enable the keyboard hook
                hookId = SetKeyboardHook(keyboardProc);
                EnableDisableButton.Content = "Enable ⊞ ";
            }
            else
            {
                // Disable the keyboard hook
                UnhookKeyboardHook();
                hookId = IntPtr.Zero;
                EnableDisableButton.Content = "Disable ⊞ ";
            }
        }

        private IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module.ModuleName), 0);
            }
        }

        private void UnhookKeyboardHook()
        {
            UnhookWindowsHookEx(hookId);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (key == Key.LWin || key == Key.RWin)
                {
                    // Windows key was pressed, prevent further processing
                    return new IntPtr(1);
                }
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

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

        private void Policyeditor(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("gpedit.msc");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while opening the Group Policy Editor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            string message = "To enable the 'Disable Windows Key hotkeys' feature, please follow these steps:\n\n";
            message += " In the Group Policy Editor, navigate to the following path:\n";
            message += "'User Configuration' > 'Administrative Templates' > 'Windows Components' > 'File Explorer'\n";
            message += " Double-click on the 'Turn off Windows Key hotkeys' policy.\n";
            message += " Select the 'Enabled' option and click 'OK'.\n";
            message += " Close the Group Policy Editor.\n";
            message += "\nOnce you have enabled the feature, restart your computer for the changes to take effect.";

            MessageBox.Show(message, "Enable 'Disable Windows Key hotkeys'", MessageBoxButton.OK, MessageBoxImage.Information);


        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Por(object sender, RoutedEventArgs e)
        {

        }

        private void reduction(object sender, RoutedEventArgs e)
        {
            // Change the slider value to 8
            slider1.Value = 8;

            // Simulate button click of "Apply"
            Button_Click_2(null, null);

            // Simulate check the checkbox
            checkBox.IsChecked = true;

            // Simulate button click of "Disable ⊞Win Key"
            EnableDisableButton_Click(null, null);

            // Simulate button click of "Auto Start"
            AutoStartButton_Click(null, null);

            // Simulate button click of "Secure UAC"
            Button_Click_5(null, null);

            // Simulate button click of "Policy Editor"
            Policyeditor(null, null);

            // Simulate button click of "Por"
            Por(null, null);

        }
    }
}




