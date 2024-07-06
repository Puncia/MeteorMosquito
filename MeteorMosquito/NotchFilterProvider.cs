using NAudio.Dsp;
using NAudio.Wave;
using System.Diagnostics;

namespace MeteorMosquito
{
    class NotchFilterProvider : ISampleProvider
    {
        private readonly ISampleProvider sourceProvider;
        private readonly CompositeNotchFilter compositeFilter;

        private readonly Stopwatch sw = new();
        private readonly TimingsCircularBuffer<double> processingTimes = new(384);

        private readonly List<BiQuadFilter> filters = new();

        private int sampleCount = 0;

        public bool FilterEnabled
        {
            get; set;
        } = true;

        public NotchFilterProvider(ISampleProvider sourceProvider, params (float frequency, float q)[] filterParams)
        {
            this.sourceProvider = sourceProvider;
            compositeFilter = new CompositeNotchFilter(sourceProvider.WaveFormat.SampleRate, filterParams);

            for (int i = 0; i < filterParams.Length; i++)
                filters.Add(BiQuadFilter.NotchFilter(sourceProvider.WaveFormat.SampleRate, filterParams[i].frequency, filterParams[i].q));
        }

        public WaveFormat WaveFormat => sourceProvider.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            sw.Restart();
            int bytesRead = sourceProvider.Read(buffer, offset, count);

            if (FilterEnabled)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    float sample = buffer[offset + i];
                    foreach (var f in filters)
                        sample = f.Transform(sample);
                    buffer[offset + i] = sample;
                }
            }

            processingTimes.Push(sw.Elapsed.TotalMicroseconds);

            sw.Stop();
            return bytesRead;
        }

        public long GetTiming()
        {
            return processingTimes.Average();
        }

        public int GetSampleCount(bool Reset = true)
        {
            var s = sampleCount;

            if (Reset)
                sampleCount = 0;

            return s;
        }
    }
}
