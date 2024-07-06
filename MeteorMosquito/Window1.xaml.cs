using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MeteorMosquito
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MeteorMosquitoWindow : Window
    {
        private int _selectedIndex = -1;
        private List<CaptureDevice> _devices = new();

        public delegate void InputDeviceSetEventHandler(int Index);
        public event InputDeviceSetEventHandler? OnInputDeviceSet;
        public event EventHandler? OnFilterDisableToggle;


        public readonly record struct CaptureDevice(string DeviceName, bool IsDefault, int Channels, int SampleRate, int BitsPerSample, int Volume);

        public void SetTiming(uint t)
        {
            Dispatcher.BeginInvoke(() =>
            {
                TimingLabel.Content = t + "µs";
            });
        }

        public void LoadDeviceNames(List<CaptureDevice> captureDevices)
        {
            _devices = captureDevices;

            int i = captureDevices.Count - 1;
            foreach (CaptureDevice captureDevice in captureDevices)
            {
                if (!captureDevice.IsDefault) i--;
                DeviceList.Items.Add(captureDevice.DeviceName);
            }
            DeviceList.SelectedIndex = i;
            _selectedIndex = i;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedIndex = DeviceList.SelectedIndex;
            OnInputDeviceSet?.Invoke(_selectedIndex);
            ApplyButton.IsEnabled = false;
        }

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceInfoLabel.Content =
                $"{_devices[DeviceList.SelectedIndex].DeviceName}\n{_devices[DeviceList.SelectedIndex].Channels}" +
                $"{(_devices[DeviceList.SelectedIndex].Channels > 1 ? " channels" : " channel")}" +
                $", {_devices[DeviceList.SelectedIndex].SampleRate}Hz, {_devices[DeviceList.SelectedIndex].BitsPerSample}bit, {_devices[DeviceList.SelectedIndex].Volume}%";

            if (DeviceList.SelectedIndex != _selectedIndex && _selectedIndex != -1)
                ApplyButton.IsEnabled = true;
            else ApplyButton.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnFilterDisableToggle?.Invoke(sender, e);
        }

        public void ToggleFilter(bool toggle)
        {
            FilterToggle.Content = toggle ? "Disable filter" : "Enable filter";
        }

        public MeteorMosquitoWindow()
        {
            InitializeComponent();
        }
    }
}
