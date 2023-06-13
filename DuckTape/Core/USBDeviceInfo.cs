using System.Management;

namespace DuckTape.MVVM.Model
{
    public class USBDeviceInfo
    {
        public string DeviceID { get; private set; }
        public string PNPDeviceID { get; private set; }
        public string Description { get; private set; }
        public string Caption { get; private set; }

        public USBDeviceInfo(string deviceId)
        {
            DeviceID = deviceId;

            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID='" + deviceId.Replace("\\", "\\\\") + "'");
            foreach (var device in searcher.Get())
            {
                PNPDeviceID = (string)device.GetPropertyValue("PNPDeviceID");
                Description = (string)device.GetPropertyValue("Description");
                Caption = (string)device.GetPropertyValue("Caption");
            }
        }
    }
}
