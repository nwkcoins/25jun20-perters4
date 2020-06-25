using System;

namespace Tulip
{
    internal static partial class Tinet
    {
        private static int FoscStart(double[] options) => (int)options[0];

        private static int FoscStart(decimal[] options) => (int)options[0];

        private static int Fosc(int size, double[][] inputs, double[] options, double[][] outputs)
        {
            var period = (int)options[0];

            if (period < 1)
            {
                return TI_INVALID_OPTION;
            }

            if (size <= FoscStart(options))
            {
                return TI_OKAY;
            }

            double[] input = inputs[0];
            double[] output = outputs[0];

            double x = default; // Sum of Xs.
            double x2 = default; // Sum of square of Xs.
            double y = default; // Flat sum of previous numbers.
            double xy = default; // Weighted sum of previous numbers.
            double tsf = default;
            for (var i = 0; i < period - 1; ++i)
            {
                x += i + 1;
                x2 += (i + 1) * (i + 1);
                xy += input[i] * (i + 1);
                y += input[i];
            }

            x += period;
            x2 += period * period;

            double p = 1.0 / period;
            double bd = 1.0 / (period * x2 - x * x);
            int outputIndex = default;
            for (var i = period - 1; i < size; ++i)
            {
                xy += input[i] * period;
                y += input[i];
                double b = (period * xy - x * y) * bd;
                double a = (y - b * x) * p;
                if (i >= period)
                {
                    output[outputIndex++] = 100 * (input[i] - tsf) / input[i];
                }

                tsf = a + b * (period + 1);
                xy -= y;
                y -= input[i - period + 1];
            }

            return TI_OKAY;
        }

        private static int Fosc(int size, decimal[][] inputs, decimal[] options, decimal[][] outputs)
        {
            var period = (int)options[0];

            if (period < 1)
            {
                return TI_INVALID_OPTION;
            }

            if (size <= FoscStart(options))
            {
                return TI_OKAY;
            }

            decimal[] input = inputs[0];
            decimal[] output = outputs[0];

            decimal x = default; // Sum of Xs.
            decimal x2 = default; // Sum of square of Xs.
            decimal y = default; // Flat sum of previous numbers.
            decimal xy = default; // Weighted sum of previous numbers.
            decimal tsf = default;
            for (var i = 0; i < period - 1; ++i)
            {
                x += i + 1;
                x2 += (i + 1) * (i + 1);
                xy += input[i] * (i + 1);
                y += input[i];
            }

            x += period;
            x2 += period * period;

            decimal p = Decimal.One / period;
            decimal bd = Decimal.One / (period * x2 - x * x);
            int outputIndex = default;
            for (var i = period - 1; i < size; ++i)
            {
                xy += input[i] * period;
                y += input[i];
                decimal b = (period * xy - x * y) * bd;
                decimal a = (y - b * x) * p;
                if (i >= period)
                {
                    output[outputIndex++] = 100 * (input[i] - tsf) / input[i];
                }

                tsf = a + b * (period + 1);
                xy -= y;
                y -= input[i - period + 1];
            }

            return TI_OKAY;
        }
    }
}
