using NAudio.Dsp;
using NAudio.Wave;
using System.Diagnostics;

namespace MeteorMosquito
{
    public class NotchFilterProvider : ISampleProvider, IDisposable
    {
        private readonly ISampleProvider sourceProvider;

        private readonly Stopwatch sw = new();
        private readonly TimingsCircularBuffer<double> processingTimes;

        private readonly List<BiQuadFilter>[] filtersPerChannel;
        private int sampleCount = 0;

        public bool FilterEnabled { get; internal set; } = true;
        public bool AudioEnabled { get; internal set; } = true;

        public NotchFilterProvider(ISampleProvider sourceProvider, params (float frequency, float q)[] filterParams)
        {
            processingTimes = new(100);
            this.sourceProvider = sourceProvider;
            int channels = sourceProvider.WaveFormat.Channels;
            filtersPerChannel = new List<BiQuadFilter>[channels];

            // need to do this because otherwise we apply the same filter on both and we create harmonics
            for (int ch = 0; ch < channels; ch++)
            {
                filtersPerChannel[ch] = filterParams.Select(p => BiQuadFilter.NotchFilter(sourceProvider.WaveFormat.SampleRate, p.frequency, p.q)).ToList();
            }
        }

        public WaveFormat WaveFormat => sourceProvider.WaveFormat;


        public int Read(float[] buffer, int offset, int count)
        {
            if (AudioEnabled)
            {
                sw.Restart();

                int samplesRead = sourceProvider.Read(buffer, offset, count);
                int channels = sourceProvider.WaveFormat.Channels;

                if (FilterEnabled)
                {
                    for (int i = 0; i < samplesRead; i += channels)
                    {
                        for (int ch = i; ch < i + channels; ch++)
                        {
                            float sample = buffer[offset + ch];
                            foreach (var f in filtersPerChannel[ch % channels])
                            {
                                sample = f.Transform(sample);
                            }
                            buffer[offset + ch] = sample;
                        }
                    }
                }

                sw.Stop();
                processingTimes.Push(sw.Elapsed.TotalMicroseconds);
                sampleCount += samplesRead;

                return samplesRead;
            }

            return 0;
        }

        public int GetTiming()
        {
            return processingTimes.Average();
        }

        public int GetSampleCount(bool reset = true)
        {
            var s = sampleCount;

            if (reset)
                sampleCount = 0;

            return s;

        }

        public void Dispose()
        {
            sampleCount = 0;
        }
    }
}
