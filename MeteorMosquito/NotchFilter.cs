using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorMosquito
{
    internal class NotchFilter
    {
        private readonly double[] a = new double[3];
        private readonly double[] b = new double[3];
        private readonly double[] x = new double[3];
        private readonly double[] y = new double[3];

        public NotchFilter(double frequency, double sampleRate, double q = 1.0)
        {
            if (frequency <= 0 || frequency >= sampleRate / 2)
                throw new ArgumentException("Frequency must be between 0 and Nyquist frequency");

            double w0 = 2 * Math.PI * frequency / sampleRate;
            double alpha = Math.Sin(w0) / (2 * q);

            double a0 = 1 + alpha;
            b[0] = 1 / a0;
            b[1] = -2 * Math.Cos(w0) / a0;
            b[2] = 1 / a0;

            a[1] = -2 * Math.Cos(w0) / a0;
            a[2] = (1 - alpha) / a0;
        }

        public float Process(float input)
        {
            // Shift x and y arrays
            x[2] = x[1];
            x[1] = x[0];
            x[0] = input;

            y[2] = y[1];
            y[1] = y[0];

            // Calculate output
            double output = b[0] * x[0] + b[1] * x[1] + b[2] * x[2]
                          - a[1] * y[1] - a[2] * y[2];

            y[0] = output;

            return (float)output;
        }
    }
}
