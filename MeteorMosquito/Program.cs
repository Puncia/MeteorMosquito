using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.Windows.Threading;

namespace MeteorMosquito
{
    internal class Program
    {
        static MeteorMosquitoWindow window1 = new();

        static private List<MeteorMosquitoWindow.CaptureDevice> _devices = new();

        static private WaveInEvent? waveIn;
        static private WaveOutEvent? waveOut;
        static private NotchFilterProvider? notchFilterProvider;
        static private System.Timers.Timer? statTimer;

        [STAThread]
        static void Main(string[] args)
        {
            window1.OnInputDeviceSet += Window1_OnInputDeviceSet;
            window1.Closed += Window1_Closed;
            window1.OnFilterDisableToggle += Window1_OnFilterDisableToggle;

            Initialize();
            window1.ShowDialog();
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
            InitProvider(Index);
        }

        private static void Initialize()
        {
            _devices = EnumerateDevices();
            window1.LoadDeviceNames(_devices);
            var default_i = _devices.IndexOf(_devices.Where(d => d.IsDefault).First());
            Debug.WriteLine($"Setting device [{default_i}] {_devices[default_i]} as default");
            InitProvider(default_i);
        }

        private static List<MeteorMosquitoWindow.CaptureDevice> EnumerateDevices()
        {
            var en = new MMDeviceEnumerator();
            List<MeteorMosquitoWindow.CaptureDevice> devices = new();

            var defaultDeviceID = en.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia).ID;

            foreach (var wasapi in en.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                devices.Add(new MeteorMosquitoWindow.CaptureDevice(
                    wasapi.FriendlyName,
                    wasapi.ID == defaultDeviceID ? true : false,
                    wasapi.AudioClient.MixFormat.Channels,
                    wasapi.AudioClient.MixFormat.SampleRate,
                    wasapi.AudioClient.MixFormat.BitsPerSample,
                    (int)(wasapi.AudioEndpointVolume.MasterVolumeLevelScalar * 100)));
            }

            return devices;
        }

        private static void InitProvider(int deviceNumber)
        {
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(_devices[deviceNumber].SampleRate, _devices[deviceNumber].BitsPerSample, _devices[deviceNumber].Channels),
                BufferMilliseconds = 20,
                DeviceNumber = deviceNumber
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
            };

            notchFilterProvider = new NotchFilterProvider(waveProvider, filterParams);
            waveOut = new WaveOutEvent
            {
                DesiredLatency = 30,
                NumberOfBuffers = 4
            };
            waveOut.Init(notchFilterProvider);
            waveIn.StartRecording();
            waveOut.Play();

            statTimer = new(1000);
            statTimer.Elapsed += (sender, e) =>
            {
                window1.SetTiming((uint)notchFilterProvider.GetTiming());
            };
            statTimer.Start();
        }
    }
}
