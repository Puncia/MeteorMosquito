using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.Windows.Threading;

namespace MeteorMosquito
{
    internal class Program
    {
        static MeteorMosquitoWindow window1 = new();

        static private (List<MeteorMosquitoWindow.Device> input, List<MeteorMosquitoWindow.Device> output) _devices = new();

        static private WaveInEvent? waveIn;
        static private WaveOutEvent? waveOut;
        static private NotchFilterProvider? notchFilterProvider;
        static private System.Timers.Timer? statTimer;
        static private string selectedInputDevice = "";
        static private string selectedOutputDevice = "";

        [STAThread]
        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            window1.OnInputDeviceSet += Window1_OnInputDeviceSet;
            window1.OnOutputDeviceSet += Window1_OnOutputDeviceSet;
            window1.Closed += Window1_Closed;
            window1.OnFilterDisableToggle += Window1_OnFilterDisableToggle;
            window1.OnAudioDisableToggle += Window1_OnAudioDisableToggle;

            Initialize(CreateProvider: false);
            window1.ShowDialog();
        }


        private static void Window1_OnAudioDisableToggle(object? sender, EventArgs e)
        {
            bool toggle = notchFilterProvider is not null;
            if (toggle)
            {
                statTimer?.Stop();
                statTimer = null;

                waveIn?.StopRecording();
                waveIn = null;

                waveOut?.Stop();
                waveOut = null;
                notchFilterProvider?.Dispose();
                notchFilterProvider = null;
            }
            else
            {
                statTimer?.Start();
                Initialize(Enumerate: false);
            }
            window1.ToggleAudio(!toggle);
        }

        private static void Window1_OnFilterDisableToggle(object? sender, EventArgs e)
        {
            if (notchFilterProvider is not null)
            {
                var tggl = !notchFilterProvider.FilterEnabled;
                notchFilterProvider.FilterEnabled = tggl;

                window1.ToggleFilter(tggl);
            }
        }

        private static void Window1_Closed(object? sender, EventArgs e)
        {
            waveIn?.StopRecording();
            waveOut?.Stop();
        }

        private static void Window1_OnInputDeviceSet(int Index)
        {
            waveIn?.StopRecording();
            waveOut?.Stop();
            statTimer?.Stop();
            selectedInputDevice = _devices.input[Index].ID;

            if (notchFilterProvider is not null && notchFilterProvider.AudioEnabled)
                Initialize(Enumerate: false);
        }
        private static void Window1_OnOutputDeviceSet(int Index)
        {
            waveIn?.StopRecording();
            waveOut?.Stop();
            statTimer?.Stop();
            selectedOutputDevice = _devices.output[Index].ID;

            if (notchFilterProvider is not null && notchFilterProvider.AudioEnabled)
                Initialize(Enumerate: false);
        }

        private static void Initialize(bool Enumerate = true, bool CreateProvider = true)
        {
            if (Enumerate)
            {
                _devices = EnumerateDevices();
                window1.LoadDeviceNames(_devices);

                if (selectedInputDevice == string.Empty)
                    selectedInputDevice = _devices.input.First(d => d.IsDefault).ID;
            }

            if (CreateProvider)
                InitProvider(selectedInputDevice);
        }

        private static (List<MeteorMosquitoWindow.Device>, List<MeteorMosquitoWindow.Device>) EnumerateDevices()
        {

            return (EnumerateInputDevices(), EnumerateOutputDevices());
        }
        private static List<MeteorMosquitoWindow.Device> EnumerateInputDevices()
        {
            var en = new MMDeviceEnumerator();
            List<MeteorMosquitoWindow.Device> devices = new();

            var defaultDeviceID = en.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia).ID;

            foreach (var wasapi in en.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                devices.Add(new MeteorMosquitoWindow.Device(
                    wasapi.FriendlyName,
                    wasapi.DeviceTopology.DeviceId,
                    wasapi.ID == defaultDeviceID ? true : false,
                    wasapi.AudioClient.MixFormat.Channels,
                    wasapi.AudioClient.MixFormat.SampleRate,
                    wasapi.AudioClient.MixFormat.BitsPerSample,
                    (int)(wasapi.AudioEndpointVolume.MasterVolumeLevelScalar * 100)));

                Debug.WriteLine($"{wasapi.FriendlyName} {wasapi.DeviceTopology.DeviceId}");
            }

            return devices;
        }

        private static List<MeteorMosquitoWindow.Device> EnumerateOutputDevices()
        {
            var en = new MMDeviceEnumerator();
            List<MeteorMosquitoWindow.Device> devices = new();

            var defaultDeviceID = en.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID;

            foreach (var wasapi in en.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                devices.Add(new MeteorMosquitoWindow.Device(
                    wasapi.FriendlyName,
                    wasapi.DeviceTopology.DeviceId,
                    wasapi.ID == defaultDeviceID ? true : false,
                    wasapi.AudioClient.MixFormat.Channels,
                    wasapi.AudioClient.MixFormat.SampleRate,
                    wasapi.AudioClient.MixFormat.BitsPerSample,
                    (int)(wasapi.AudioEndpointVolume.MasterVolumeLevelScalar * 100)));

                Debug.WriteLine($"{wasapi.FriendlyName} {wasapi.DeviceTopology.DeviceId}");

            }

            return devices;
        }

        private static void InitProvider(string deviceID)
        {
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(_devices.input.First((d) => d.ID == deviceID).SampleRate,
                _devices.input.First((d) => d.ID == deviceID).BitsPerSample,
                _devices.input.First((d) => d.ID == deviceID).Channels),
                BufferMilliseconds = 20,
                DeviceNumber = _devices.input.IndexOf(_devices.input.First(d => d.ID == deviceID))
            };

            var waveProvider = new WaveInProvider(waveIn).ToSampleProvider();

            float fixedBandwith = 16.0f;

            var filterParams = new[]
            {
                (frequency: 1000.0f, q: 1000.0f / fixedBandwith),
                (frequency: 2000.0f, q: 2000.0f / fixedBandwith),
                (frequency: 3000.0f, q: 3000.0f / fixedBandwith),
                (frequency: 4000.0f, q: 4000.0f / fixedBandwith),
                (frequency: 5000.0f, q: 5000.0f / fixedBandwith),
                (frequency: 6000.0f, q: 6000.0f / fixedBandwith),
                (frequency: 8000.0f, q: 8000.0f / fixedBandwith),
                (frequency: 9000.0f, q: 9000.0f / fixedBandwith),
                (frequency: 10000.0f, q: 10000.0f / fixedBandwith),
                (frequency: 11000.0f, q: 11000.0f / fixedBandwith),
                (frequency: 12000.0f, q: 12000.0f / fixedBandwith),
                (frequency: 13000.0f, q: 13000.0f / fixedBandwith),
                (frequency: 14000.0f, q: 14000.0f / fixedBandwith),
                (frequency: 16000.0f, q: 16000.0f / fixedBandwith),
                (frequency: 17000.0f, q: 17000.0f / fixedBandwith),
                (frequency: 18000.0f, q: 18000.0f / fixedBandwith),
                (frequency: 19000.0f, q: 19000.0f / fixedBandwith),
                (frequency: 20000.0f, q: 20000.0f / fixedBandwith),
            };

            var outdev = selectedOutputDevice == string.Empty ? -1 : _devices.output.IndexOf(_devices.output.First((d) => d.ID == selectedOutputDevice));

            notchFilterProvider = new NotchFilterProvider(waveProvider, filterParams);
            waveOut = new WaveOutEvent
            {
                DesiredLatency = 30,
                NumberOfBuffers = 4,
                DeviceNumber = outdev
            };

            if (outdev != -1)
                Debug.WriteLine($"default => {_devices.output[outdev]}");

            waveOut.Init(notchFilterProvider);
            waveIn.StartRecording();
            waveOut.Play();

            statTimer = new(1000);
            statTimer.Elapsed += (sender, e) =>
            {
                window1.UpdateTelemetry(notchFilterProvider.GetSampleCount(true), filterParams.Length, notchFilterProvider.GetTiming());
            };
            statTimer.Start();
        }
    }
}
