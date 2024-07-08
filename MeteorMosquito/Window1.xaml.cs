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
        public event EventHandler? OnAudioDisableToggle;

        public readonly record struct CaptureDevice(string DeviceName, bool IsDefault, int Channels, int SampleRate, int BitsPerSample, int Volume);

        public void UpdateTelemetry(int sampleCount, int filterCount, int timing)
        {
            Dispatcher.BeginInvoke(() =>
            {
                SampleCountLabel.Content = sampleCount + " samples/s";
                TimingLabel.Content = timing + "µs/sample";
                FilterCountLabel.Content = filterCount + " filters";
            });
        }

        public void LoadDeviceNames(List<CaptureDevice> captureDevices)
        {
            _devices = captureDevices;

            int i = captureDevices.Count - 1;
            foreach (CaptureDevice captureDevice in captureDevices)
            {
                if (!captureDevice.IsDefault) i--;
                InputDeviceList.Items.Add(captureDevice.DeviceName);
            }
            InputDeviceList.SelectedIndex = i;
            _selectedIndex = i;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedIndex = InputDeviceList.SelectedIndex;
            OnInputDeviceSet?.Invoke(_selectedIndex);
            InputApplyButton.IsEnabled = false;
        }

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InputDeviceInfoLabel.Content =
                $"{_devices[InputDeviceList.SelectedIndex].DeviceName}\n{_devices[InputDeviceList.SelectedIndex].Channels}" +
                $"{(_devices[InputDeviceList.SelectedIndex].Channels > 1 ? " channels" : " channel")}" +
                $", {_devices[InputDeviceList.SelectedIndex].SampleRate}Hz, {_devices[InputDeviceList.SelectedIndex].BitsPerSample}bit, {_devices[InputDeviceList.SelectedIndex].Volume}%";

            if (InputDeviceList.SelectedIndex != _selectedIndex && _selectedIndex != -1)
                InputApplyButton.IsEnabled = true;
            else InputApplyButton.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnFilterDisableToggle?.Invoke(sender, e);
        }

        public void ToggleFilter(bool Enable, bool disableButton = false)
        {
            FilterToggle.Content = Enable ? "Disable filter" : "Enable filter";
            FilterToggle.IsEnabled = !disableButton;
        }

        internal void AudioToggle_Click(object sender, RoutedEventArgs e)
        {
            OnAudioDisableToggle?.Invoke(sender, e);
        }

        internal void ToggleAudio(bool Enable)
        {
            AudioToggle.Content = Enable ? "Stop audio" : "Start audio";
            StatPanel.Visibility = Enable ? Visibility.Visible : Visibility.Collapsed;
            ToggleFilter(Enable, !Enable);
        }

        public MeteorMosquitoWindow()
        {
            InitializeComponent();
        }
    }
}
