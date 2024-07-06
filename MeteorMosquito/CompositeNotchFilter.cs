using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorMosquito
{
    internal class CompositeNotchFilter
    {
        private readonly List<NotchFilter> filters;

        public CompositeNotchFilter(double sampleRate, params (float frequency, float q)[] filterParams)
        {
            filters = filterParams.Select(param => new NotchFilter(param.frequency, sampleRate, param.q)).ToList();
        }

        public float Process(float input)
        {
            return filters.Aggregate(input, (current, filter) => filter.Process(current));
        }
    }
}
