using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.Windows.Threading;

namespace MeteorMosquito
{
    internal class Program
    {
        static MeteorMosquitoWindow window1 = new();

        static private WaveInEvent? waveIn;
        static private WaveOutEvent? waveOut;
        static private System.Timers.Timer? statTimer;

        [STAThread]
        static void Main(string[] args)
        {
            window1.OnInputDeviceSet += Window1_OnInputDeviceSet;
            window1.Closed += Window1_Closed;

            Initialize();
            window1.ShowDialog();
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
            var devices = EnumerateDevices();
            window1.LoadDeviceNames(devices);
            var default_i = devices.IndexOf(devices.Where(d => d.IsDefault).First());
            Debug.WriteLine($"Setting device [{default_i}] {devices[default_i]} as default");
            InitProvider(default_i);
        }

        private static List<MeteorMosquitoWindow.CaptureDevice> EnumerateDevices()
        {
            var en = new MMDeviceEnumerator();
            List<MeteorMosquitoWindow.CaptureDevice> devices = new();

            var defaultDeviceID = en.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia).ID;

            foreach (var wasapi in en.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                devices.Add(new MeteorMosquitoWindow.CaptureDevice(wasapi.FriendlyName, wasapi.ID == defaultDeviceID ? true : false));
            }

            return devices;
        }

        private static void InitProvider(int deviceNumber)
        {
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(48000, 16, 1),
                BufferMilliseconds = 20,
                DeviceNumber = deviceNumber
            };

            var waveProvider = new WaveInProvider(waveIn).ToSampleProvider();

            var filterParams = new[]
            {
                (frequency: 1000.0f, q: 20.0f),
                (frequency: 2000.0f, q: 30.0f),
                (frequency: 3000.0f, q: 40.0f),
                (frequency: 4000.0f, q: 50.0f),
                (frequency: 5000.0f, q: 60.0f),
                (frequency: 7000.0f, q: 80.0f),
                //(frequency: 8000.0, q: 90.0), // can't hear these
                //(frequency: 9000.0, q: 100.0),
                //(frequency: 10000.0, q: 110.0)
            };

            var notchFilter = new NotchFilterProvider(waveProvider, filterParams);
            waveOut = new WaveOutEvent
            {
                DesiredLatency = 30,
                NumberOfBuffers = 4
            };
            waveOut.Init(notchFilter);
            waveIn.StartRecording();
            waveOut.Play();

            statTimer = new(1000);
            statTimer.Elapsed += (sender, e) =>
            {
                window1.SetTiming((uint)notchFilter.GetTiming());
            };
            statTimer.Start();
        }
    }
}
