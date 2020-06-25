using System;

namespace Tulip
{
    internal static partial class Tinet
    {
        private const int TI_OKAY = 0;
        private const int TI_INVALID_OPTION = 1;

        private static (int size, int pushes, int index, double sum, double[] vals) BufferDoubleFactory(int size) =>
            (size, 0, 0, 0.0, new double[size]);

        private static (int size, int pushes, int index, decimal sum, decimal[] vals) BufferDecimalFactory(int size) =>
            (size, 0, 0, Decimal.Zero, new decimal[size]);

        private static void BufferPush(ref (int size, int pushes, int index, double sum, double[] vals) buffer, double val)
        {
            if (buffer.pushes >= buffer.size)
            {
                buffer.sum -= buffer.vals[buffer.index];
            }

            buffer.sum += val;
            buffer.vals[buffer.index++] = val;
            ++buffer.pushes;
            if (buffer.index >= buffer.size)
            {
                buffer.index = 0;
            }
        }

        private static void BufferPush(ref (int size, int pushes, int index, decimal sum, decimal[] vals) buffer, decimal val)
        {
            if (buffer.pushes >= buffer.size)
            {
                buffer.sum -= buffer.vals[buffer.index];
            }

            buffer.sum += val;
            buffer.vals[buffer.index++] = val;
            ++buffer.pushes;
            if (buffer.index >= buffer.size)
            {
                buffer.index = 0;
            }
        }

        private static void BufferQPush(ref (int size, int pushes, int index, double sum, double[] vals) buffer, double val)
        {
            buffer.vals[buffer.index++] = val;
            if (buffer.index >= buffer.size)
            {
                buffer.index = 0;
            }
        }

        private static void BufferQPush(ref (int size, int pushes, int index, decimal sum, decimal[] vals) buffer, decimal val)
        {
            buffer.vals[buffer.index++] = val;
            if (buffer.index >= buffer.size)
            {
                buffer.index = 0;
            }
        }

        private static double BufferGet((int, int, int, double, double[]) buffer, double val)
        {
            var (size, _, index, _, vals) = buffer;
            //return vals[(Index)((index + size - 1 + val) % size)];
            return vals[(int)((index + size - 1 + val) % size)];
        }

        private static decimal BufferGet((int, int, int, decimal, decimal[]) buffer, decimal val)
        {
            var (size, _, index, _, vals) = buffer;
            //return vals[(Index)((index + size - 1 + val) % size)];
            return vals[(int)((index + size - 1 + val) % size)];
        }

        private static void CalcTrueRange(double[] low, double[] high, double[] close, int i, out double trueRange)
        {
            double l = low[i];
            double h = high[i];
            double c = close[i - 1];
            double ych = Math.Abs(h - c);
            double ycl = Math.Abs(l - c);
            double v = h - l;
            if (ych > v)
            {
                v = ych;
            }

            if (ycl > v)
            {
                v = ycl;
            }

            trueRange = v;
        }

        private static void CalcTrueRange(decimal[] low, decimal[] high, decimal[] close, int i, out decimal trueRange)
        {
            decimal l = low[i];
            decimal h = high[i];
            decimal c = close[i - 1];
            decimal ych = Math.Abs(h - c);
            decimal ycl = Math.Abs(l - c);
            decimal v = h - l;
            if (ych > v)
            {
                v = ych;
            }

            if (ycl > v)
            {
                v = ycl;
            }

            trueRange = v;
        }

        private static void CalcDirection(double[] high, double[] low, int i, out double up, out double down)
        {
            up = high[i] - high[i - 1];
            down = low[i - 1] - low[i];

            if (up < 0.0)
            {
                up = 0.0;
            }
            else if (up > down)
            {
                down = 0.0;
            }

            if (down < 0.0)
            {
                down = 0.0;
            }
            else if (down > up)
            {
                up = 0.0;
            }
        }

        private static void CalcDirection(decimal[] high, decimal[] low, int i, out decimal up, out decimal down)
        {
            up = high[i] - high[i - 1];
            down = low[i - 1] - low[i];

            if (up < Decimal.Zero)
            {
                up = Decimal.Zero;
            }
            else if (up > down)
            {
                down = Decimal.Zero;
            }

            if (down < Decimal.Zero)
            {
                down = Decimal.Zero;
            }
            else if (down > up)
            {
                up = Decimal.Zero;
            }
        }

        private static void Simple1(int size, double[] input, double[] output, Func<double, double> op)
        {
            for (var i = 0; i < size; ++i)
            {
                output[i] = op(input[i]);
            }
        }

        private static void Simple1(int size, decimal[] input, decimal[] output, Func<decimal, decimal> op)
        {
            for (var i = 0; i < size; ++i)
            {
                output[i] = op(input[i]);
            }
        }

        private static void Simple2(int size, double[] input1, double[] input2, double[] output, Func<double, double, double> op)
        {
            for (var i = 0; i < size; ++i)
            {
                output[i] = op(input1[i], input2[i]);
            }
        }

        private static void Simple2(int size, decimal[] input1, decimal[] input2, decimal[] output, Func<decimal, decimal, decimal> op)
        {
            for (var i = 0; i < size; ++i)
            {
                output[i] = op(input1[i], input2[i]);
            }
        }

        internal static int IndicatorRun(byte index, double[][] inputs, double[] options, double[][] outputs)
        {
            int length = inputs[0].Length;

            int result;
            switch (index)
            {
                case 0: result = Abs(length, inputs, options, outputs); break;
                case 1: result = Acos(length, inputs, options, outputs); break;
                case 2: result = Ad(length, inputs, options, outputs); break;
                case 3: result = Add(length, inputs, options, outputs); break;
                case 4: result = Adosc(length, inputs, options, outputs); break;
                case 5: result = Adx(length, inputs, options, outputs); break;
                case 6: result = Adxr(length, inputs, options, outputs); break;
                case 7: result = Ao(length, inputs, options, outputs); break;
                case 8: result = Apo(length, inputs, options, outputs); break;
                case 9: result = Aroon(length, inputs, options, outputs); break;
                case 10: result = AroonOsc(length, inputs, options, outputs); break;
                case 11: result = Asin(length, inputs, options, outputs); break;
                case 12: result = Atan(length, inputs, options, outputs); break;
                case 13: result = Atr(length, inputs, options, outputs); break;
                case 14: result = AvgPrice(length, inputs, options, outputs); break;
                case 15: result = Bbands(length, inputs, options, outputs); break;
                case 16: result = Bop(length, inputs, options, outputs); break;
                case 17: result = Cci(length, inputs, options, outputs); break;
                case 18: result = Ceil(length, inputs, options, outputs); break;
                case 19: result = Cmo(length, inputs, options, outputs); break;
                case 20: result = Cos(length, inputs, options, outputs); break;
                case 21: result = Cosh(length, inputs, options, outputs); break;
                case 22: result = Crossany(length, inputs, options, outputs); break;
                case 23: result = Crossover(length, inputs, options, outputs); break;
                case 24: result = Cvi(length, inputs, options, outputs); break;
                case 25: result = Decay(length, inputs, options, outputs); break;
                case 26: result = Dema(length, inputs, options, outputs); break;
                case 27: result = Di(length, inputs, options, outputs); break;
                case 28: result = Div(length, inputs, options, outputs); break;
                case 29: result = Dm(length, inputs, options, outputs); break;
                case 30: result = Dpo(length, inputs, options, outputs); break;
                case 31: result = Dx(length, inputs, options, outputs); break;
                case 32: result = Edecay(length, inputs, options, outputs); break;
                case 33: result = Ema(length, inputs, options, outputs); break;
                case 34: result = Emv(length, inputs, options, outputs); break;
                case 35: result = Exp(length, inputs, options, outputs); break;
                case 36: result = Fisher(length, inputs, options, outputs); break;
                case 37: result = Floor(length, inputs, options, outputs); break;
                case 38: result = Fosc(length, inputs, options, outputs); break;
                case 39: result = Hma(length, inputs, options, outputs); break;
                case 40: result = Kama(length, inputs, options, outputs); break;
                case 41: result = Kvo(length, inputs, options, outputs); break;
                case 42: result = Lag(length, inputs, options, outputs); break;
                case 43: result = LinReg(length, inputs, options, outputs); break;
                case 44: result = LinRegIntercept(length, inputs, options, outputs); break;
                case 45: result = LinRegSlope(length, inputs, options, outputs); break;
                case 46: result = Ln(length, inputs, options, outputs); break;
                case 47: result = Log10(length, inputs, options, outputs); break;
                case 48: result = Macd(length, inputs, options, outputs); break;
                case 49: result = MarketFi(length, inputs, options, outputs); break;
                case 50: result = Mass(length, inputs, options, outputs); break;
                case 51: result = Max(length, inputs, options, outputs); break;
                case 52: result = Md(length, inputs, options, outputs); break;
                case 53: result = MedPrice(length, inputs, options, outputs); break;
                case 54: result = Mfi(length, inputs, options, outputs); break;
                case 55: result = Min(length, inputs, options, outputs); break;
                case 56: result = Mom(length, inputs, options, outputs); break;
                case 57: result = Msw(length, inputs, options, outputs); break;
                case 58: result = Mul(length, inputs, options, outputs); break;
                case 59: result = Natr(length, inputs, options, outputs); break;
                case 60: result = Nvi(length, inputs, options, outputs); break;
                case 61: result = Obv(length, inputs, options, outputs); break;
                case 62: result = Ppo(length, inputs, options, outputs); break;
                case 63: result = Psar(length, inputs, options, outputs); break;
                case 64: result = Pvi(length, inputs, options, outputs); break;
                case 65: result = Qstick(length, inputs, options, outputs); break;
                case 66: result = Roc(length, inputs, options, outputs); break;
                case 67: result = RocR(length, inputs, options, outputs); break;
                case 68: result = Round(length, inputs, options, outputs); break;
                case 69: result = Rsi(length, inputs, options, outputs); break;
                case 70: result = Sin(length, inputs, options, outputs); break;
                case 71: result = Sinh(length, inputs, options, outputs); break;
                case 72: result = Sma(length, inputs, options, outputs); break;
                case 73: result = Sqrt(length, inputs, options, outputs); break;
                case 74: result = StdDev(length, inputs, options, outputs); break;
                case 75: result = StdErr(length, inputs, options, outputs); break;
                case 76: result = Stoch(length, inputs, options, outputs); break;
                case 77: result = StochRsi(length, inputs, options, outputs); break;
                case 78: result = Sub(length, inputs, options, outputs); break;
                case 79: result = Sum(length, inputs, options, outputs); break;
                case 80: result = Tan(length, inputs, options, outputs); break;
                case 81: result = Tanh(length, inputs, options, outputs); break;
                case 82: result = Tema(length, inputs, options, outputs); break;
                case 83: result = ToDeg(length, inputs, options, outputs); break;
                case 84: result = ToRad(length, inputs, options, outputs); break;
                case 85: result = Tr(length, inputs, options, outputs); break;
                case 86: result = Trima(length, inputs, options, outputs); break;
                case 87: result = Trix(length, inputs, options, outputs); break;
                case 88: result = Trunc(length, inputs, options, outputs); break;
                case 89: result = Tsf(length, inputs, options, outputs); break;
                case 90: result = TypPrice(length, inputs, options, outputs); break;
                case 91: result = UltOsc(length, inputs, options, outputs); break;
                case 92: result = Var(length, inputs, options, outputs); break;
                case 93: result = Vhf(length, inputs, options, outputs); break;
                case 94: result = Vidya(length, inputs, options, outputs); break;
                case 95: result = Volatility(length, inputs, options, outputs); break;
                case 96: result = Vosc(length, inputs, options, outputs); break;
                case 97: result = Vwma(length, inputs, options, outputs); break;
                case 98: result = Wad(length, inputs, options, outputs); break;
                case 99: result = WcPrice(length, inputs, options, outputs); break;
                case 100: result = Wilders(length, inputs, options, outputs); break;
                case 101: result = WillR(length, inputs, options, outputs); break;
                case 102: result = Wma(length, inputs, options, outputs); break;
                case 103: result = ZlEma(length, inputs, options, outputs); break;
                default: result = TI_INVALID_OPTION; break;
            }
            return result;
            /*return index switch
            {
                0 => Abs(length, inputs, options, outputs),
                1 => Acos(length, inputs, options, outputs),
                2 => Ad(length, inputs, options, outputs),
                3 => Add(length, inputs, options, outputs),
                4 => Adosc(length, inputs, options, outputs),
                5 => Adx(length, inputs, options, outputs),
                6 => Adxr(length, inputs, options, outputs),
                7 => Ao(length, inputs, options, outputs),
                8 => Apo(length, inputs, options, outputs),
                9 => Aroon(length, inputs, options, outputs),
                10 => AroonOsc(length, inputs, options, outputs),
                11 => Asin(length, inputs, options, outputs),
                12 => Atan(length, inputs, options, outputs),
                13 => Atr(length, inputs, options, outputs),
                14 => AvgPrice(length, inputs, options, outputs),
                15 => Bbands(length, inputs, options, outputs),
                16 => Bop(length, inputs, options, outputs),
                17 => Cci(length, inputs, options, outputs),
                18 => Ceil(length, inputs, options, outputs),
                19 => Cmo(length, inputs, options, outputs),
                20 => Cos(length, inputs, options, outputs),
                21 => Cosh(length, inputs, options, outputs),
                22 => Crossany(length, inputs, options, outputs),
                23 => Crossover(length, inputs, options, outputs),
                24 => Cvi(length, inputs, options, outputs),
                25 => Decay(length, inputs, options, outputs),
                26 => Dema(length, inputs, options, outputs),
                27 => Di(length, inputs, options, outputs),
                28 => Div(length, inputs, options, outputs),
                29 => Dm(length, inputs, options, outputs),
                30 => Dpo(length, inputs, options, outputs),
                31 => Dx(length, inputs, options, outputs),
                32 => Edecay(length, inputs, options, outputs),
                33 => Ema(length, inputs, options, outputs),
                34 => Emv(length, inputs, options, outputs),
                35 => Exp(length, inputs, options, outputs),
                36 => Fisher(length, inputs, options, outputs),
                37 => Floor(length, inputs, options, outputs),
                38 => Fosc(length, inputs, options, outputs),
                39 => Hma(length, inputs, options, outputs),
                40 => Kama(length, inputs, options, outputs),
                41 => Kvo(length, inputs, options, outputs),
                42 => Lag(length, inputs, options, outputs),
                43 => LinReg(length, inputs, options, outputs),
                44 => LinRegIntercept(length, inputs, options, outputs),
                45 => LinRegSlope(length, inputs, options, outputs),
                46 => Ln(length, inputs, options, outputs),
                47 => Log10(length, inputs, options, outputs),
                48 => Macd(length, inputs, options, outputs),
                49 => MarketFi(length, inputs, options, outputs),
                50 => Mass(length, inputs, options, outputs),
                51 => Max(length, inputs, options, outputs),
                52 => Md(length, inputs, options, outputs),
                53 => MedPrice(length, inputs, options, outputs),
                54 => Mfi(length, inputs, options, outputs),
                55 => Min(length, inputs, options, outputs),
                56 => Mom(length, inputs, options, outputs),
                57 => Msw(length, inputs, options, outputs),
                58 => Mul(length, inputs, options, outputs),
                59 => Natr(length, inputs, options, outputs),
                60 => Nvi(length, inputs, options, outputs),
                61 => Obv(length, inputs, options, outputs),
                62 => Ppo(length, inputs, options, outputs),
                63 => Psar(length, inputs, options, outputs),
                64 => Pvi(length, inputs, options, outputs),
                65 => Qstick(length, inputs, options, outputs),
                66 => Roc(length, inputs, options, outputs),
                67 => RocR(length, inputs, options, outputs),
                68 => Round(length, inputs, options, outputs),
                69 => Rsi(length, inputs, options, outputs),
                70 => Sin(length, inputs, options, outputs),
                71 => Sinh(length, inputs, options, outputs),
                72 => Sma(length, inputs, options, outputs),
                73 => Sqrt(length, inputs, options, outputs),
                74 => StdDev(length, inputs, options, outputs),
                75 => StdErr(length, inputs, options, outputs),
                76 => Stoch(length, inputs, options, outputs),
                77 => StochRsi(length, inputs, options, outputs),
                78 => Sub(length, inputs, options, outputs),
                79 => Sum(length, inputs, options, outputs),
                80 => Tan(length, inputs, options, outputs),
                81 => Tanh(length, inputs, options, outputs),
                82 => Tema(length, inputs, options, outputs),
                83 => ToDeg(length, inputs, options, outputs),
                84 => ToRad(length, inputs, options, outputs),
                85 => Tr(length, inputs, options, outputs),
                86 => Trima(length, inputs, options, outputs),
                87 => Trix(length, inputs, options, outputs),
                88 => Trunc(length, inputs, options, outputs),
                89 => Tsf(length, inputs, options, outputs),
                90 => TypPrice(length, inputs, options, outputs),
                91 => UltOsc(length, inputs, options, outputs),
                92 => Var(length, inputs, options, outputs),
                93 => Vhf(length, inputs, options, outputs),
                94 => Vidya(length, inputs, options, outputs),
                95 => Volatility(length, inputs, options, outputs),
                96 => Vosc(length, inputs, options, outputs),
                97 => Vwma(length, inputs, options, outputs),
                98 => Wad(length, inputs, options, outputs),
                99 => WcPrice(length, inputs, options, outputs),
                100 => Wilders(length, inputs, options, outputs),
                101 => WillR(length, inputs, options, outputs),
                102 => Wma(length, inputs, options, outputs),
                103 => ZlEma(length, inputs, options, outputs),
                _ => TI_INVALID_OPTION
            };*/
        }

        internal static int IndicatorRun(byte index, decimal[][] inputs, decimal[] options, decimal[][] outputs)
        {
            int length = inputs[0].Length;

            int result;
            switch (index)
            {
                case 0: result = Abs(length, inputs, options, outputs); break;
                case 1: result = Acos(length, inputs, options, outputs); break;
                case 2: result = Ad(length, inputs, options, outputs); break;
                case 3: result = Add(length, inputs, options, outputs); break;
                case 4: result = Adosc(length, inputs, options, outputs); break;
                case 5: result = Adx(length, inputs, options, outputs); break;
                case 6: result = Adxr(length, inputs, options, outputs); break;
                case 7: result = Ao(length, inputs, options, outputs); break;
                case 8: result = Apo(length, inputs, options, outputs); break;
                case 9: result = Aroon(length, inputs, options, outputs); break;
                case 10: result = AroonOsc(length, inputs, options, outputs); break;
                case 11: result = Asin(length, inputs, options, outputs); break;
                case 12: result = Atan(length, inputs, options, outputs); break;
                case 13: result = Atr(length, inputs, options, outputs); break;
                case 14: result = AvgPrice(length, inputs, options, outputs); break;
                case 15: result = Bbands(length, inputs, options, outputs); break;
                case 16: result = Bop(length, inputs, options, outputs); break;
                case 17: result = Cci(length, inputs, options, outputs); break;
                case 18: result = Ceil(length, inputs, options, outputs); break;
                case 19: result = Cmo(length, inputs, options, outputs); break;
                case 20: result = Cos(length, inputs, options, outputs); break;
                case 21: result = Cosh(length, inputs, options, outputs); break;
                case 22: result = Crossany(length, inputs, options, outputs); break;
                case 23: result = Crossover(length, inputs, options, outputs); break;
                case 24: result = Cvi(length, inputs, options, outputs); break;
                case 25: result = Decay(length, inputs, options, outputs); break;
                case 26: result = Dema(length, inputs, options, outputs); break;
                case 27: result = Di(length, inputs, options, outputs); break;
                case 28: result = Div(length, inputs, options, outputs); break;
                case 29: result = Dm(length, inputs, options, outputs); break;
                case 30: result = Dpo(length, inputs, options, outputs); break;
                case 31: result = Dx(length, inputs, options, outputs); break;
                case 32: result = Edecay(length, inputs, options, outputs); break;
                case 33: result = Ema(length, inputs, options, outputs); break;
                case 34: result = Emv(length, inputs, options, outputs); break;
                case 35: result = Exp(length, inputs, options, outputs); break;
                case 36: result = Fisher(length, inputs, options, outputs); break;
                case 37: result = Floor(length, inputs, options, outputs); break;
                case 38: result = Fosc(length, inputs, options, outputs); break;
                case 39: result = Hma(length, inputs, options, outputs); break;
                case 40: result = Kama(length, inputs, options, outputs); break;
                case 41: result = Kvo(length, inputs, options, outputs); break;
                case 42: result = Lag(length, inputs, options, outputs); break;
                case 43: result = LinReg(length, inputs, options, outputs); break;
                case 44: result = LinRegIntercept(length, inputs, options, outputs); break;
                case 45: result = LinRegSlope(length, inputs, options, outputs); break;
                case 46: result = Ln(length, inputs, options, outputs); break;
                case 47: result = Log10(length, inputs, options, outputs); break;
                case 48: result = Macd(length, inputs, options, outputs); break;
                case 49: result = MarketFi(length, inputs, options, outputs); break;
                case 50: result = Mass(length, inputs, options, outputs); break;
                case 51: result = Max(length, inputs, options, outputs); break;
                case 52: result = Md(length, inputs, options, outputs); break;
                case 53: result = MedPrice(length, inputs, options, outputs); break;
                case 54: result = Mfi(length, inputs, options, outputs); break;
                case 55: result = Min(length, inputs, options, outputs); break;
                case 56: result = Mom(length, inputs, options, outputs); break;
                case 57: result = Msw(length, inputs, options, outputs); break;
                case 58: result = Mul(length, inputs, options, outputs); break;
                case 59: result = Natr(length, inputs, options, outputs); break;
                case 60: result = Nvi(length, inputs, options, outputs); break;
                case 61: result = Obv(length, inputs, options, outputs); break;
                case 62: result = Ppo(length, inputs, options, outputs); break;
                case 63: result = Psar(length, inputs, options, outputs); break;
                case 64: result = Pvi(length, inputs, options, outputs); break;
                case 65: result = Qstick(length, inputs, options, outputs); break;
                case 66: result = Roc(length, inputs, options, outputs); break;
                case 67: result = RocR(length, inputs, options, outputs); break;
                case 68: result = Round(length, inputs, options, outputs); break;
                case 69: result = Rsi(length, inputs, options, outputs); break;
                case 70: result = Sin(length, inputs, options, outputs); break;
                case 71: result = Sinh(length, inputs, options, outputs); break;
                case 72: result = Sma(length, inputs, options, outputs); break;
                case 73: result = Sqrt(length, inputs, options, outputs); break;
                case 74: result = StdDev(length, inputs, options, outputs); break;
                case 75: result = StdErr(length, inputs, options, outputs); break;
                case 76: result = Stoch(length, inputs, options, outputs); break;
                case 77: result = StochRsi(length, inputs, options, outputs); break;
                case 78: result = Sub(length, inputs, options, outputs); break;
                case 79: result = Sum(length, inputs, options, outputs); break;
                case 80: result = Tan(length, inputs, options, outputs); break;
                case 81: result = Tanh(length, inputs, options, outputs); break;
                case 82: result = Tema(length, inputs, options, outputs); break;
                case 83: result = ToDeg(length, inputs, options, outputs); break;
                case 84: result = ToRad(length, inputs, options, outputs); break;
                case 85: result = Tr(length, inputs, options, outputs); break;
                case 86: result = Trima(length, inputs, options, outputs); break;
                case 87: result = Trix(length, inputs, options, outputs); break;
                case 88: result = Trunc(length, inputs, options, outputs); break;
                case 89: result = Tsf(length, inputs, options, outputs); break;
                case 90: result = TypPrice(length, inputs, options, outputs); break;
                case 91: result = UltOsc(length, inputs, options, outputs); break;
                case 92: result = Var(length, inputs, options, outputs); break;
                case 93: result = Vhf(length, inputs, options, outputs); break;
                case 94: result = Vidya(length, inputs, options, outputs); break;
                case 95: result = Volatility(length, inputs, options, outputs); break;
                case 96: result = Vosc(length, inputs, options, outputs); break;
                case 97: result = Vwma(length, inputs, options, outputs); break;
                case 98: result = Wad(length, inputs, options, outputs); break;
                case 99: result = WcPrice(length, inputs, options, outputs); break;
                case 100: result = Wilders(length, inputs, options, outputs); break;
                case 101: result = WillR(length, inputs, options, outputs); break;
                case 102: result = Wma(length, inputs, options, outputs); break;
                case 103: result = ZlEma(length, inputs, options, outputs); break;
                default: result = TI_INVALID_OPTION; break;
            }
            return result;
            /*return index switch
            {
                0 => Abs(length, inputs, options, outputs),
                1 => Acos(length, inputs, options, outputs),
                2 => Ad(length, inputs, options, outputs),
                3 => Add(length, inputs, options, outputs),
                4 => Adosc(length, inputs, options, outputs),
                5 => Adx(length, inputs, options, outputs),
                6 => Adxr(length, inputs, options, outputs),
                7 => Ao(length, inputs, options, outputs),
                8 => Apo(length, inputs, options, outputs),
                9 => Aroon(length, inputs, options, outputs),
                10 => AroonOsc(length, inputs, options, outputs),
                11 => Asin(length, inputs, options, outputs),
                12 => Atan(length, inputs, options, outputs),
                13 => Atr(length, inputs, options, outputs),
                14 => AvgPrice(length, inputs, options, outputs),
                15 => Bbands(length, inputs, options, outputs),
                16 => Bop(length, inputs, options, outputs),
                17 => Cci(length, inputs, options, outputs),
                18 => Ceil(length, inputs, options, outputs),
                19 => Cmo(length, inputs, options, outputs),
                20 => Cos(length, inputs, options, outputs),
                21 => Cosh(length, inputs, options, outputs),
                22 => Crossany(length, inputs, options, outputs),
                23 => Crossover(length, inputs, options, outputs),
                24 => Cvi(length, inputs, options, outputs),
                25 => Decay(length, inputs, options, outputs),
                26 => Dema(length, inputs, options, outputs),
                27 => Di(length, inputs, options, outputs),
                28 => Div(length, inputs, options, outputs),
                29 => Dm(length, inputs, options, outputs),
                30 => Dpo(length, inputs, options, outputs),
                31 => Dx(length, inputs, options, outputs),
                32 => Edecay(length, inputs, options, outputs),
                33 => Ema(length, inputs, options, outputs),
                34 => Emv(length, inputs, options, outputs),
                35 => Exp(length, inputs, options, outputs),
                36 => Fisher(length, inputs, options, outputs),
                37 => Floor(length, inputs, options, outputs),
                38 => Fosc(length, inputs, options, outputs),
                39 => Hma(length, inputs, options, outputs),
                40 => Kama(length, inputs, options, outputs),
                41 => Kvo(length, inputs, options, outputs),
                42 => Lag(length, inputs, options, outputs),
                43 => LinReg(length, inputs, options, outputs),
                44 => LinRegIntercept(length, inputs, options, outputs),
                45 => LinRegSlope(length, inputs, options, outputs),
                46 => Ln(length, inputs, options, outputs),
                47 => Log10(length, inputs, options, outputs),
                48 => Macd(length, inputs, options, outputs),
                49 => MarketFi(length, inputs, options, outputs),
                50 => Mass(length, inputs, options, outputs),
                51 => Max(length, inputs, options, outputs),
                52 => Md(length, inputs, options, outputs),
                53 => MedPrice(length, inputs, options, outputs),
                54 => Mfi(length, inputs, options, outputs),
                55 => Min(length, inputs, options, outputs),
                56 => Mom(length, inputs, options, outputs),
                57 => Msw(length, inputs, options, outputs),
                58 => Mul(length, inputs, options, outputs),
                59 => Natr(length, inputs, options, outputs),
                60 => Nvi(length, inputs, options, outputs),
                61 => Obv(length, inputs, options, outputs),
                62 => Ppo(length, inputs, options, outputs),
                63 => Psar(length, inputs, options, outputs),
                64 => Pvi(length, inputs, options, outputs),
                65 => Qstick(length, inputs, options, outputs),
                66 => Roc(length, inputs, options, outputs),
                67 => RocR(length, inputs, options, outputs),
                68 => Round(length, inputs, options, outputs),
                69 => Rsi(length, inputs, options, outputs),
                70 => Sin(length, inputs, options, outputs),
                71 => Sinh(length, inputs, options, outputs),
                72 => Sma(length, inputs, options, outputs),
                73 => Sqrt(length, inputs, options, outputs),
                74 => StdDev(length, inputs, options, outputs),
                75 => StdErr(length, inputs, options, outputs),
                76 => Stoch(length, inputs, options, outputs),
                77 => StochRsi(length, inputs, options, outputs),
                78 => Sub(length, inputs, options, outputs),
                79 => Sum(length, inputs, options, outputs),
                80 => Tan(length, inputs, options, outputs),
                81 => Tanh(length, inputs, options, outputs),
                82 => Tema(length, inputs, options, outputs),
                83 => ToDeg(length, inputs, options, outputs),
                84 => ToRad(length, inputs, options, outputs),
                85 => Tr(length, inputs, options, outputs),
                86 => Trima(length, inputs, options, outputs),
                87 => Trix(length, inputs, options, outputs),
                88 => Trunc(length, inputs, options, outputs),
                89 => Tsf(length, inputs, options, outputs),
                90 => TypPrice(length, inputs, options, outputs),
                91 => UltOsc(length, inputs, options, outputs),
                92 => Var(length, inputs, options, outputs),
                93 => Vhf(length, inputs, options, outputs),
                94 => Vidya(length, inputs, options, outputs),
                95 => Volatility(length, inputs, options, outputs),
                96 => Vosc(length, inputs, options, outputs),
                97 => Vwma(length, inputs, options, outputs),
                98 => Wad(length, inputs, options, outputs),
                99 => WcPrice(length, inputs, options, outputs),
                100 => Wilders(length, inputs, options, outputs),
                101 => WillR(length, inputs, options, outputs),
                102 => Wma(length, inputs, options, outputs),
                103 => ZlEma(length, inputs, options, outputs),
                _ => TI_INVALID_OPTION
            };*/
        }

        internal static int IndicatorStart(byte index, double[] options)
        {
            int result;
            switch (index)
            {
                case 0: result = AbsStart(options); break;
                case 1: result = AcosStart(options); break;
                case 2: result = AdStart(options); break;
                case 3: result = AddStart(options); break;
                case 4: result = AdoscStart(options); break;
                case 5: result = AdxStart(options); break;
                case 6: result = AdxrStart(options); break;
                case 7: result = AoStart(options); break;
                case 8: result = ApoStart(options); break;
                case 9: result = AroonStart(options); break;
                case 10: result = AroonOscStart(options); break;
                case 11: result = AsinStart(options); break;
                case 12: result = AtanStart(options); break;
                case 13: result = AtrStart(options); break;
                case 14: result = AvgPriceStart(options); break;
                case 15: result = BbandsStart(options); break;
                case 16: result = BopStart(options); break;
                case 17: result = CciStart(options); break;
                case 18: result = CeilStart(options); break;
                case 19: result = CmoStart(options); break;
                case 20: result = CosStart(options); break;
                case 21: result = CoshStart(options); break;
                case 22: result = CrossanyStart(options); break;
                case 23: result = CrossoverStart(options); break;
                case 24: result = CviStart(options); break;
                case 25: result = DecayStart(options); break;
                case 26: result = DemaStart(options); break;
                case 27: result = DiStart(options); break;
                case 28: result = DivStart(options); break;
                case 29: result = DmStart(options); break;
                case 30: result = DpoStart(options); break;
                case 31: result = DxStart(options); break;
                case 32: result = EdecayStart(options); break;
                case 33: result = EmaStart(options); break;
                case 34: result = EmvStart(options); break;
                case 35: result = ExpStart(options); break;
                case 36: result = FisherStart(options); break;
                case 37: result = FloorStart(options); break;
                case 38: result = FoscStart(options); break;
                case 39: result = HmaStart(options); break;
                case 40: result = KamaStart(options); break;
                case 41: result = KvoStart(options); break;
                case 42: result = LagStart(options); break;
                case 43: result = LinRegStart(options); break;
                case 44: result = LinRegInterceptStart(options); break;
                case 45: result = LinRegSlopeStart(options); break;
                case 46: result = LnStart(options); break;
                case 47: result = Log10Start(options); break;
                case 48: result = MacdStart(options); break;
                case 49: result = MarketFiStart(options); break;
                case 50: result = MassStart(options); break;
                case 51: result = MaxStart(options); break;
                case 52: result = MdStart(options); break;
                case 53: result = MedPriceStart(options); break;
                case 54: result = MfiStart(options); break;
                case 55: result = MinStart(options); break;
                case 56: result = MomStart(options); break;
                case 57: result = MswStart(options); break;
                case 58: result = MulStart(options); break;
                case 59: result = NatrStart(options); break;
                case 60: result = NviStart(options); break;
                case 61: result = ObvStart(options); break;
                case 62: result = PpoStart(options); break;
                case 63: result = PsarStart(options); break;
                case 64: result = PviStart(options); break;
                case 65: result = QstickStart(options); break;
                case 66: result = RocStart(options); break;
                case 67: result = RocRStart(options); break;
                case 68: result = RoundStart(options); break;
                case 69: result = RsiStart(options); break;
                case 70: result = SinStart(options); break;
                case 71: result = SinhStart(options); break;
                case 72: result = SmaStart(options); break;
                case 73: result = SqrtStart(options); break;
                case 74: result = StdDevStart(options); break;
                case 75: result = StdErrStart(options); break;
                case 76: result = StochStart(options); break;
                case 77: result = StochRsiStart(options); break;
                case 78: result = SubStart(options); break;
                case 79: result = SumStart(options); break;
                case 80: result = TanStart(options); break;
                case 81: result = TanhStart(options); break;
                case 82: result = TemaStart(options); break;
                case 83: result = ToDegStart(options); break;
                case 84: result = ToRadStart(options); break;
                case 85: result = TrStart(options); break;
                case 86: result = TrimaStart(options); break;
                case 87: result = TrixStart(options); break;
                case 88: result = TruncStart(options); break;
                case 89: result = TsfStart(options); break;
                case 90: result = TypPriceStart(options); break;
                case 91: result = UltOscStart(options); break;
                case 92: result = VarStart(options); break;
                case 93: result = VhfStart(options); break;
                case 94: result = VidyaStart(options); break;
                case 95: result = VolatilityStart(options); break;
                case 96: result = VoscStart(options); break;
                case 97: result = VwmaStart(options); break;
                case 98: result = WadStart(options); break;
                case 99: result = WcPriceStart(options); break;
                case 100: result = WildersStart(options); break;
                case 101: result = WillRStart(options); break;
                case 102: result = WmaStart(options); break;
                case 103: result = ZlEmaStart(options); break;
                default: result = TI_INVALID_OPTION; break;
            }
            return result;
        }

        /*internal static int IndicatorStart(byte index, double[] options) =>
        index switch
        {
            0 => AbsStart(options),
            1 => AcosStart(options),
            2 => AdStart(options),
            3 => AddStart(options),
            4 => AdoscStart(options),
            5 => AdxStart(options),
            6 => AdxrStart(options),
            7 => AoStart(options),
            8 => ApoStart(options),
            9 => AroonStart(options),
            10 => AroonOscStart(options),
            11 => AsinStart(options),
            12 => AtanStart(options),
            13 => AtrStart(options),
            14 => AvgPriceStart(options),
            15 => BbandsStart(options),
            16 => BopStart(options),
            17 => CciStart(options),
            18 => CeilStart(options),
            19 => CmoStart(options),
            20 => CosStart(options),
            21 => CoshStart(options),
            22 => CrossanyStart(options),
            23 => CrossoverStart(options),
            24 => CviStart(options),
            25 => DecayStart(options),
            26 => DemaStart(options),
            27 => DiStart(options),
            28 => DivStart(options),
            29 => DmStart(options),
            30 => DpoStart(options),
            31 => DxStart(options),
            32 => EdecayStart(options),
            33 => EmaStart(options),
            34 => EmvStart(options),
            35 => ExpStart(options),
            36 => FisherStart(options),
            37 => FloorStart(options),
            38 => FoscStart(options),
            39 => HmaStart(options),
            40 => KamaStart(options),
            41 => KvoStart(options),
            42 => LagStart(options),
            43 => LinRegStart(options),
            44 => LinRegInterceptStart(options),
            45 => LinRegSlopeStart(options),
            46 => LnStart(options),
            47 => Log10Start(options),
            48 => MacdStart(options),
            49 => MarketFiStart(options),
            50 => MassStart(options),
            51 => MaxStart(options),
            52 => MdStart(options),
            53 => MedPriceStart(options),
            54 => MfiStart(options),
            55 => MinStart(options),
            56 => MomStart(options),
            57 => MswStart(options),
            58 => MulStart(options),
            59 => NatrStart(options),
            60 => NviStart(options),
            61 => ObvStart(options),
            62 => PpoStart(options),
            63 => PsarStart(options),
            64 => PviStart(options),
            65 => QstickStart(options),
            66 => RocStart(options),
            67 => RocRStart(options),
            68 => RoundStart(options),
            69 => RsiStart(options),
            70 => SinStart(options),
            71 => SinhStart(options),
            72 => SmaStart(options),
            73 => SqrtStart(options),
            74 => StdDevStart(options),
            75 => StdErrStart(options),
            76 => StochStart(options),
            77 => StochRsiStart(options),
            78 => SubStart(options),
            79 => SumStart(options),
            80 => TanStart(options),
            81 => TanhStart(options),
            82 => TemaStart(options),
            83 => ToDegStart(options),
            84 => ToRadStart(options),
            85 => TrStart(options),
            86 => TrimaStart(options),
            87 => TrixStart(options),
            88 => TruncStart(options),
            89 => TsfStart(options),
            90 => TypPriceStart(options),
            91 => UltOscStart(options),
            92 => VarStart(options),
            93 => VhfStart(options),
            94 => VidyaStart(options),
            95 => VolatilityStart(options),
            96 => VoscStart(options),
            97 => VwmaStart(options),
            98 => WadStart(options),
            99 => WcPriceStart(options),
            100 => WildersStart(options),
            101 => WillRStart(options),
            102 => WmaStart(options),
            103 => ZlEmaStart(options),
            _ => TI_INVALID_OPTION
        };*/

        internal static int IndicatorStart(byte index, decimal[] options)
        {
            int result;
            switch (index)
            {
                case 0: result = AbsStart(options); break;
                case 1: result = AcosStart(options); break;
                case 2: result = AdStart(options); break;
                case 3: result = AddStart(options); break;
                case 4: result = AdoscStart(options); break;
                case 5: result = AdxStart(options); break;
                case 6: result = AdxrStart(options); break;
                case 7: result = AoStart(options); break;
                case 8: result = ApoStart(options); break;
                case 9: result = AroonStart(options); break;
                case 10: result = AroonOscStart(options); break;
                case 11: result = AsinStart(options); break;
                case 12: result = AtanStart(options); break;
                case 13: result = AtrStart(options); break;
                case 14: result = AvgPriceStart(options); break;
                case 15: result = BbandsStart(options); break;
                case 16: result = BopStart(options); break;
                case 17: result = CciStart(options); break;
                case 18: result = CeilStart(options); break;
                case 19: result = CmoStart(options); break;
                case 20: result = CosStart(options); break;
                case 21: result = CoshStart(options); break;
                case 22: result = CrossanyStart(options); break;
                case 23: result = CrossoverStart(options); break;
                case 24: result = CviStart(options); break;
                case 25: result = DecayStart(options); break;
                case 26: result = DemaStart(options); break;
                case 27: result = DiStart(options); break;
                case 28: result = DivStart(options); break;
                case 29: result = DmStart(options); break;
                case 30: result = DpoStart(options); break;
                case 31: result = DxStart(options); break;
                case 32: result = EdecayStart(options); break;
                case 33: result = EmaStart(options); break;
                case 34: result = EmvStart(options); break;
                case 35: result = ExpStart(options); break;
                case 36: result = FisherStart(options); break;
                case 37: result = FloorStart(options); break;
                case 38: result = FoscStart(options); break;
                case 39: result = HmaStart(options); break;
                case 40: result = KamaStart(options); break;
                case 41: result = KvoStart(options); break;
                case 42: result = LagStart(options); break;
                case 43: result = LinRegStart(options); break;
                case 44: result = LinRegInterceptStart(options); break;
                case 45: result = LinRegSlopeStart(options); break;
                case 46: result = LnStart(options); break;
                case 47: result = Log10Start(options); break;
                case 48: result = MacdStart(options); break;
                case 49: result = MarketFiStart(options); break;
                case 50: result = MassStart(options); break;
                case 51: result = MaxStart(options); break;
                case 52: result = MdStart(options); break;
                case 53: result = MedPriceStart(options); break;
                case 54: result = MfiStart(options); break;
                case 55: result = MinStart(options); break;
                case 56: result = MomStart(options); break;
                case 57: result = MswStart(options); break;
                case 58: result = MulStart(options); break;
                case 59: result = NatrStart(options); break;
                case 60: result = NviStart(options); break;
                case 61: result = ObvStart(options); break;
                case 62: result = PpoStart(options); break;
                case 63: result = PsarStart(options); break;
                case 64: result = PviStart(options); break;
                case 65: result = QstickStart(options); break;
                case 66: result = RocStart(options); break;
                case 67: result = RocRStart(options); break;
                case 68: result = RoundStart(options); break;
                case 69: result = RsiStart(options); break;
                case 70: result = SinStart(options); break;
                case 71: result = SinhStart(options); break;
                case 72: result = SmaStart(options); break;
                case 73: result = SqrtStart(options); break;
                case 74: result = StdDevStart(options); break;
                case 75: result = StdErrStart(options); break;
                case 76: result = StochStart(options); break;
                case 77: result = StochRsiStart(options); break;
                case 78: result = SubStart(options); break;
                case 79: result = SumStart(options); break;
                case 80: result = TanStart(options); break;
                case 81: result = TanhStart(options); break;
                case 82: result = TemaStart(options); break;
                case 83: result = ToDegStart(options); break;
                case 84: result = ToRadStart(options); break;
                case 85: result = TrStart(options); break;
                case 86: result = TrimaStart(options); break;
                case 87: result = TrixStart(options); break;
                case 88: result = TruncStart(options); break;
                case 89: result = TsfStart(options); break;
                case 90: result = TypPriceStart(options); break;
                case 91: result = UltOscStart(options); break;
                case 92: result = VarStart(options); break;
                case 93: result = VhfStart(options); break;
                case 94: result = VidyaStart(options); break;
                case 95: result = VolatilityStart(options); break;
                case 96: result = VoscStart(options); break;
                case 97: result = VwmaStart(options); break;
                case 98: result = WadStart(options); break;
                case 99: result = WcPriceStart(options); break;
                case 100: result = WildersStart(options); break;
                case 101: result = WillRStart(options); break;
                case 102: result = WmaStart(options); break;
                case 103: result = ZlEmaStart(options); break;
                default: result = TI_INVALID_OPTION; break;
            }
            return result;
        }
        /*internal static int IndicatorStart(byte index, decimal[] options) =>
            index switch
            {
                0 => AbsStart(options),
                1 => AcosStart(options),
                2 => AdStart(options),
                3 => AddStart(options),
                4 => AdoscStart(options),
                5 => AdxStart(options),
                6 => AdxrStart(options),
                7 => AoStart(options),
                8 => ApoStart(options),
                9 => AroonStart(options),
                10 => AroonOscStart(options),
                11 => AsinStart(options),
                12 => AtanStart(options),
                13 => AtrStart(options),
                14 => AvgPriceStart(options),
                15 => BbandsStart(options),
                16 => BopStart(options),
                17 => CciStart(options),
                18 => CeilStart(options),
                19 => CmoStart(options),
                20 => CosStart(options),
                21 => CoshStart(options),
                22 => CrossanyStart(options),
                23 => CrossoverStart(options),
                24 => CviStart(options),
                25 => DecayStart(options),
                26 => DemaStart(options),
                27 => DiStart(options),
                28 => DivStart(options),
                29 => DmStart(options),
                30 => DpoStart(options),
                31 => DxStart(options),
                32 => EdecayStart(options),
                33 => EmaStart(options),
                34 => EmvStart(options),
                35 => ExpStart(options),
                36 => FisherStart(options),
                37 => FloorStart(options),
                38 => FoscStart(options),
                39 => HmaStart(options),
                40 => KamaStart(options),
                41 => KvoStart(options),
                42 => LagStart(options),
                43 => LinRegStart(options),
                44 => LinRegInterceptStart(options),
                45 => LinRegSlopeStart(options),
                46 => LnStart(options),
                47 => Log10Start(options),
                48 => MacdStart(options),
                49 => MarketFiStart(options),
                50 => MassStart(options),
                51 => MaxStart(options),
                52 => MdStart(options),
                53 => MedPriceStart(options),
                54 => MfiStart(options),
                55 => MinStart(options),
                56 => MomStart(options),
                57 => MswStart(options),
                58 => MulStart(options),
                59 => NatrStart(options),
                60 => NviStart(options),
                61 => ObvStart(options),
                62 => PpoStart(options),
                63 => PsarStart(options),
                64 => PviStart(options),
                65 => QstickStart(options),
                66 => RocStart(options),
                67 => RocRStart(options),
                68 => RoundStart(options),
                69 => RsiStart(options),
                70 => SinStart(options),
                71 => SinhStart(options),
                72 => SmaStart(options),
                73 => SqrtStart(options),
                74 => StdDevStart(options),
                75 => StdErrStart(options),
                76 => StochStart(options),
                77 => StochRsiStart(options),
                78 => SubStart(options),
                79 => SumStart(options),
                80 => TanStart(options),
                81 => TanhStart(options),
                82 => TemaStart(options),
                83 => ToDegStart(options),
                84 => ToRadStart(options),
                85 => TrStart(options),
                86 => TrimaStart(options),
                87 => TrixStart(options),
                88 => TruncStart(options),
                89 => TsfStart(options),
                90 => TypPriceStart(options),
                91 => UltOscStart(options),
                92 => VarStart(options),
                93 => VhfStart(options),
                94 => VidyaStart(options),
                95 => VolatilityStart(options),
                96 => VoscStart(options),
                97 => VwmaStart(options),
                98 => WadStart(options),
                99 => WcPriceStart(options),
                100 => WildersStart(options),
                101 => WillRStart(options),
                102 => WmaStart(options),
                103 => ZlEmaStart(options),
                _ => TI_INVALID_OPTION
            };*/
    }
}
