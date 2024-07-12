using NAudio.CoreAudioApi;
using System.Windows;
using System.Windows.Controls;

namespace MeteorMosquito
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MeteorMosquitoWindow : Window
    {
        private (List<MMDevice> input, List<MMDevice> output) _devices = new();

        private MMDevice? selectedInputDevice;
        private MMDevice? selectedOutputDevice;
        public delegate void DeviceSetEventHandler(MMDevice Index);
        public event DeviceSetEventHandler? OnInputDeviceSet;
        public event DeviceSetEventHandler? OnOutputDeviceSet;

        public event EventHandler? OnFilterDisableToggle;
        public event EventHandler? OnAudioDisableToggle;

        public void UpdateTelemetry(int sampleCount, int filterCount, int timing)
        {
            Dispatcher.BeginInvoke(() =>
            {
                SampleCountLabel.Content = sampleCount + " samples/s";
                TimingLabel.Content = timing + "µs/sample";
                FilterCountLabel.Content = filterCount + " filters";
            });
        }

        public void LoadDeviceNames((List<MMDevice> input, List<MMDevice> ouput) captureDevices)
        {
            var default_capture = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            var default_render = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            _devices = captureDevices;

            foreach (MMDevice captureDevice in captureDevices.input)
            {
                InputDeviceList.Items.Add(captureDevice.FriendlyName);
            }
            InputDeviceList.SelectedItem = default_capture.FriendlyName;
            selectedInputDevice = default_capture;

            int y = captureDevices.ouput.Count - 1;
            foreach (MMDevice renderDevice in captureDevices.ouput)
            {
                OutputDeviceList.Items.Add(renderDevice.FriendlyName);
            }
            OutputDeviceList.SelectedItem = default_render.FriendlyName;
            selectedOutputDevice = default_render;
        }

        private void InputApplyButton_Click(object sender, RoutedEventArgs e)
        {
            selectedInputDevice = _devices.input[InputDeviceList.SelectedIndex];
            OnInputDeviceSet?.Invoke(_devices.input[InputDeviceList.SelectedIndex]);
            InputApplyButton.IsEnabled = false;
        }
        private void OutputApplyButton_Click(object sender, RoutedEventArgs e)
        {
            selectedOutputDevice = _devices.output[OutputDeviceList.SelectedIndex];
            OnOutputDeviceSet?.Invoke(_devices.output[OutputDeviceList.SelectedIndex]);
            OutputApplyButton.IsEnabled = false;
        }

        private void InputDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDevice = _devices.input[InputDeviceList.SelectedIndex];
            var audioClient = selectedDevice.AudioClient;
            var mixFormat = audioClient.MixFormat;

            var deviceId = selectedDevice.FriendlyName;
            var channelCount = mixFormat.Channels;
            var channelLabel = channelCount > 1 ? "channels" : "channel";
            var sampleRate = mixFormat.SampleRate;
            var bitsPerSample = mixFormat.BitsPerSample;
            var volume = _devices.input[InputDeviceList.SelectedIndex].AudioEndpointVolume.MasterVolumeLevelScalar * 100;

            InputDeviceInfoLabel.Content =
                $"{deviceId}\n{channelCount} {channelLabel}, {sampleRate}Hz, {bitsPerSample}bit, {volume}%";


            if (selectedInputDevice is not null && _devices.input[InputDeviceList.SelectedIndex] != selectedInputDevice)
            {
                InputApplyButton.IsEnabled = true;
            }
            else
            {
                InputApplyButton.IsEnabled = false;
            }
        }

        private void OutputDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedOutputDevice is not null && _devices.output[OutputDeviceList.SelectedIndex] != selectedOutputDevice)
            {
                OutputApplyButton.IsEnabled = true;
            }
            else
            {
                OutputApplyButton.IsEnabled = false;
            }
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
