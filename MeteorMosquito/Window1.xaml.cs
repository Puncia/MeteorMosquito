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
        private int _selectedInputIndex = -1;
        private int _selectedOutputIndex = -1;
        private (List<MMDevice> input, List<MMDevice> output) _devices = new();

        public delegate void DeviceSetEventHandler(int Index);
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
            var default_render = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var default_capture = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            _devices = captureDevices;

            int i = captureDevices.input.Count - 1;
            foreach (MMDevice captureDevice in captureDevices.input)
            {
                if (captureDevice.ID != default_capture.ID) i--;
                InputDeviceList.Items.Add(captureDevice.FriendlyName);
            }
            InputDeviceList.SelectedIndex = i;
            _selectedInputIndex = i;

            int y = captureDevices.ouput.Count - 1;
            foreach (MMDevice renderDevice in captureDevices.ouput)
            {
                if (renderDevice.ID != default_render.ID) y--;
                OutputDeviceList.Items.Add(renderDevice.FriendlyName);
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
