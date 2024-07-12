using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;

namespace MeteorMosquito
{
    internal class Program
    {
        private static readonly MeteorMosquitoWindow window1 = new();

        private static (List<MMDevice> input, List<MMDevice> output) _devices = new();

        private static WaveInEvent? waveIn;
        private static WaveOutEvent? waveOut;
        private static NotchFilterProvider? notchFilterProvider;
        private static System.Timers.Timer? statTimer;
        private static MMDevice? selectedInputDevice;
        private static MMDevice? selectedOutputDevice;
        private static MMDevice? defaultInputDevice;
        private static MMDevice? defaultOutputDevice;

        [STAThread]
        private static void Main(string[] args)
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
            selectedInputDevice = _devices.input[Index];

            if (notchFilterProvider is not null && notchFilterProvider.AudioEnabled)
            {
                Initialize(Enumerate: false);
            }
        }
        private static void Window1_OnOutputDeviceSet(int Index)
        {
            waveIn?.StopRecording();
            waveOut?.Stop();
            statTimer?.Stop();
            selectedOutputDevice = _devices.output[Index];

            if (notchFilterProvider is not null && notchFilterProvider.AudioEnabled)
            {
                Initialize(Enumerate: false);
            }
        }

        private static void Initialize(bool Enumerate = true, bool CreateProvider = true)
        {
            if (Enumerate)
            {
                _devices = EnumerateDevices();
                window1.LoadDeviceNames(_devices);

                if (selectedInputDevice is null)
                {
                    selectedInputDevice = defaultInputDevice;
                }
            }

            if (CreateProvider && selectedInputDevice is not null)
            {
                InitProvider(selectedInputDevice);
            }
        }

        private static (List<MMDevice>, List<MMDevice>) EnumerateDevices() => (EnumerateInputDevices(), EnumerateOutputDevices());

        private static List<MMDevice> EnumerateInputDevices()
        {
            var en = new MMDeviceEnumerator();
            List<MMDevice> devices = new();

            defaultInputDevice = en.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);

            foreach (var wasapi in en.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                devices.Add(wasapi);

                Debug.WriteLine($"{wasapi.FriendlyName} {wasapi.DeviceTopology.DeviceId}");
            }

            return devices;
        }

        private static List<MMDevice> EnumerateOutputDevices()
        {
            var en = new MMDeviceEnumerator();
            List<MMDevice> devices = new();

            defaultOutputDevice = en.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            foreach (var wasapi in en.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                devices.Add(wasapi);

                Debug.WriteLine($"{wasapi.FriendlyName} {wasapi.DeviceTopology.DeviceId}");

            }

            return devices.Reverse<MMDevice>().ToList();
        }

        private static void InitProvider(MMDevice Device)
        {
            var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            int id = 0;
            foreach (MMDevice d in devices)
            {
                if (d == Device)
                {
                    id++;
                    continue;
                }
                break;
            }

            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(Device.AudioClient.MixFormat.SampleRate,
                Device.AudioClient.MixFormat.BitsPerSample,
                Device.AudioClient.MixFormat.Channels),
                BufferMilliseconds = 20,
                DeviceNumber = id
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

            notchFilterProvider = new NotchFilterProvider(waveProvider, filterParams);

            var en = new MMDeviceEnumerator();

            var wasapiOut = new WasapiOut(selectedOutputDevice is not null ? selectedOutputDevice : defaultOutputDevice, AudioClientShareMode.Shared, false, 30);

            wasapiOut.Init(notchFilterProvider);
            waveIn.StartRecording();

            wasapiOut.Play();

            statTimer = new(1000);
            statTimer.Elapsed += (sender, e) =>
            {
                window1.UpdateTelemetry(notchFilterProvider.GetSampleCount(true), filterParams.Length, notchFilterProvider.GetTiming());
            };
            statTimer.Start();
        }
    }
}
