using System;

namespace Tulip
{
    internal static partial class Tinet
    {
        private static int VarStart(double[] options) => (int)options[0] - 1;

        private static int VarStart(decimal[] options) => (int)options[0] - 1;

        private static int Var(int size, double[][] inputs, double[] options, double[][] outputs)
        {
            var period = (int)options[0];

            if (period < 1)
            {
                return TI_INVALID_OPTION;
            }

            if (size <= VarStart(options))
            {
                return TI_OKAY;
            }

            double[] input = inputs[0];
            double[] output = outputs[0];

            double sum = default;
            double sum2 = default;
            for (var i = 0; i < period; ++i)
            {
                sum += input[i];
                sum2 += input[i] * input[i];
            }

            double scale = 1.0 / period;
            int outputIndex = default;
            output[outputIndex++] = sum2 * scale - sum * scale * (sum * scale);
            for (var i = period; i < size; ++i)
            {
                sum += input[i];
                sum2 += input[i] * input[i];

                sum -= input[i - period];
                sum2 -= input[i - period] * input[i - period];

                output[outputIndex++] = sum2 * scale - sum * scale * (sum * scale);
            }

            return TI_OKAY;
        }

        private static int Var(int size, decimal[][] inputs, decimal[] options, decimal[][] outputs)
        {
            var period = (int)options[0];

            if (period < 1)
            {
                return TI_INVALID_OPTION;
            }

            if (size <= VarStart(options))
            {
                return TI_OKAY;
            }

            decimal[] input = inputs[0];
            decimal[] output = outputs[0];

            decimal sum = default;
            decimal sum2 = default;
            for (var i = 0; i < period; ++i)
            {
                sum += input[i];
                sum2 += input[i] * input[i];
            }

            decimal scale = Decimal.One / period;
            int outputIndex = default;
            output[outputIndex++] = sum2 * scale - sum * scale * sum * scale;
            for (var i = period; i < size; ++i)
            {
                sum += input[i];
                sum2 += input[i] * input[i];

                sum -= input[i - period];
                sum2 -= input[i - period] * input[i - period];

                output[outputIndex++] = sum2 * scale - sum * scale * sum * scale;
            }

            return TI_OKAY;
        }
    }
}
