using System;
using System.Collections.Generic;
using System.Management;

public class DeviceConnectionManager
{
    private readonly List<string> _connectedDevices = new List<string>();
    private readonly ManagementEventWatcher _watcher;
    private bool _isWatcherStarted = false;
    public event EventHandler DevicesConnected;

    public DeviceConnectionManager()
    {
        var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
        _watcher = new ManagementEventWatcher(query);
        _watcher.EventArrived += WatcherOnEventArrived;
        UpdateConnectedDevices();
    }

    public void Start()
    {
        if (!_isWatcherStarted)
        {
            _watcher.Start();
            _isWatcherStarted = true;
        }
    }

    public void Stop()
    {
        if (_isWatcherStarted)
        {
            _watcher.Stop();
            _isWatcherStarted = false;
        }
    }

    private void WatcherOnEventArrived(object sender, EventArrivedEventArgs e)
    {
        UpdateConnectedDevices();
        if (_connectedDevices.Count > 0)
        {
            DevicesConnected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateConnectedDevices()
    {
        _connectedDevices.Clear();
        var searcher = new ManagementObjectSearcher("SELECT DeviceID FROM Win32_USBHub");
        foreach (var device in searcher.Get())
        {
            _connectedDevices.Add(device["DeviceID"].ToString());
        }
    }
}
