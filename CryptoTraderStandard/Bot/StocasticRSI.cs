using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeSharp;
using CryptoTraderScanner;

namespace CryptoTraderScanner
{
    public class StocasticRSI
    {
        public double SRSI { get; set; }
        public double KValue { get; set; }
        public double d { get; set; }
        public double DValue { get; set; }
        public StocasticRSI(List<MarketCandle> candles , int length = 43)
        {
            //candles.Reverse();
            var prices = candles.Select(c => (double)c.ClosePrice).ToList();
            prices.Reverse();
            CalculateRSi(prices);
        }
        private void CalculateRSi(List<double> Prices)
        {
            double Differences;//= new List<double>();
            List<double> RSIS = new List<double>();

            for (int j = 0; j < Prices.Count - 14; ++j)
            {

                double PositiveNumber = 0;// = new List<double>();
                double NegativeNumber = 0;// new List<double>();
                for (int i = 1; i < 15; ++i)
                {
                    Differences = Math.Round((Prices[j + i] - Prices[j + i - 1]), 2);
                    if (Differences > 0)
                    {
                        PositiveNumber += Differences;
                    }
                    else
                    {
                        NegativeNumber += Math.Abs(Differences);
                    }
                }

                var avgGain = Math.Round(PositiveNumber / 14, 2);
                var avgLoass = Math.Round(NegativeNumber / 14, 2);/// NegativeNumber / 14;
                var RS = avgGain / avgLoass;
                var RSI = 100 - (100 / (1 + RS));
                RSIS.Add(RSI);
            }
            //  Console.WriteLine(RSIS[15]);
            ///Calculate StocasticRsi
            List<double> StocRSI = new List<double>();
            for (int i = 0; i < RSIS.Count - 13; ++i)
            {
                var RSI = RSIS.Skip(i + 13).Take(1).ToArray();
                //Console.WriteLine(RSI[0]);
                var MaxRSI = RSIS.Skip(i).Take(14).Max();
                var MinRSI = RSIS.Skip(i).Take(14).Min();
                var result = (RSI[0] - MinRSI) / (MaxRSI - MinRSI);
                StocRSI.Add(Math.Round(result, 2));

            }
            //Console.ReadLine();
            // StocRSI.ForEach(c => Console.WriteLine(c));


            //Calculate K
            List<double> K = new List<double>();
            for (int i = 0; i < StocRSI.Count - 2; ++i)
            {
                var k = StocRSI.Skip(i).Take(3).Sum() / 3;
                K.Add(k);
            }

            ///Calculate D
            List<double> D = new List<double>();
            for (int i = 0; i < K.Count - 2; ++i)
            {
                d = K.Skip(i).Take(3).Sum() / 3;
                D.Add(d);
            }

            DValue = D[D.Count - 1] * 100;
            KValue = K[K.Count - 1] * 100;
            SRSI = StocRSI[StocRSI.Count - 1];
            if (double.IsNaN((double)KValue))
            {
                KValue = 0;
            }
            if (Double.IsNaN((double)DValue))
            {
                DValue = 0;
            }
            if (Double.IsNaN((double)SRSI))
            {
                SRSI = 0;
            }

        }
    }
}
