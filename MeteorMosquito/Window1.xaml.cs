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

        public delegate void InputDeviceSetEventHandler(int Index);
        public event InputDeviceSetEventHandler? OnInputDeviceSet;

        public readonly record struct CaptureDevice(string DeviceName, bool IsDefault);

        public void SetTiming(uint t)
        {
            Dispatcher.BeginInvoke(() =>
            {
                TimingLabel.Content = t + "µs";
            });
        }

        public void LoadDeviceNames(List<CaptureDevice> captureDevices)
        {
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
            if (DeviceList.SelectedIndex != _selectedIndex && _selectedIndex != -1)
                ApplyButton.IsEnabled = true;
            else ApplyButton.IsEnabled = false;
        }

        public MeteorMosquitoWindow()
        {
            InitializeComponent();
        }
    }
}
