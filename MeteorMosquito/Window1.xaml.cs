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
        private int _selectedInputIndex = -1;
        private int _selectedOutputIndex = -1;
        private (List<Device> input, List<Device> output) _devices = new();

        public delegate void DeviceSetEventHandler(int Index);
        public event DeviceSetEventHandler? OnInputDeviceSet;
        public event DeviceSetEventHandler? OnOutputDeviceSet;

        public event EventHandler? OnFilterDisableToggle;
        public event EventHandler? OnAudioDisableToggle;

        public readonly record struct Device(string DeviceName, string ID, bool IsDefault, int Channels, int SampleRate, int BitsPerSample, int Volume);

        public void UpdateTelemetry(int sampleCount, int filterCount, int timing)
        {
            Dispatcher.BeginInvoke(() =>
            {
                SampleCountLabel.Content = sampleCount + " samples/s";
                TimingLabel.Content = timing + "µs/sample";
                FilterCountLabel.Content = filterCount + " filters";
            });
        }

        public void LoadDeviceNames((List<Device> input, List<Device> ouput) captureDevices)
        {
            _devices = captureDevices;

            int i = captureDevices.input.Count - 1;
            foreach (Device captureDevice in captureDevices.input)
            {
                if (!captureDevice.IsDefault) i--;
                InputDeviceList.Items.Add(captureDevice.DeviceName);
            }
            InputDeviceList.SelectedIndex = i;
            _selectedInputIndex = i;

            int y = captureDevices.ouput.Count - 1;
            foreach (Device renderDevice in captureDevices.ouput)
            {
                if (!renderDevice.IsDefault) y--;
                OutputDeviceList.Items.Add(renderDevice.DeviceName);
            }
            OutputDeviceList.SelectedIndex = y;
            _selectedOutputIndex = y;
        }

        private void InputApplyButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedInputIndex = InputDeviceList.SelectedIndex;
            OnInputDeviceSet?.Invoke(_selectedInputIndex);
            InputApplyButton.IsEnabled = false;
        }
        private void OutputApplyButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedOutputIndex = OutputDeviceList.SelectedIndex;
            OnOutputDeviceSet?.Invoke(_selectedOutputIndex);
            OutputApplyButton.IsEnabled = false;
        }

        private void InputDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InputDeviceInfoLabel.Content =
                $"{_devices.input[InputDeviceList.SelectedIndex].DeviceName}\n{_devices.input[InputDeviceList.SelectedIndex].Channels}" +
                $"{(_devices.input[InputDeviceList.SelectedIndex].Channels > 1 ? " channels" : " channel")}" +
                $", {_devices.input[InputDeviceList.SelectedIndex].SampleRate}Hz, {_devices.input[InputDeviceList.SelectedIndex].BitsPerSample}bit, {_devices.input[InputDeviceList.SelectedIndex].Volume}%";

            if (InputDeviceList.SelectedIndex != _selectedInputIndex && _selectedInputIndex != -1)
                InputApplyButton.IsEnabled = true;
            else InputApplyButton.IsEnabled = false;
        }

        private void OutputDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputDeviceList.SelectedIndex != _selectedOutputIndex && _selectedOutputIndex != -1)
                OutputApplyButton.IsEnabled = true;
            else OutputApplyButton.IsEnabled = false;
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
