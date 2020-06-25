//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using TicTacTec.TA.Library;

namespace CryptoTraderScanner
{
    public class CalculateTaMacd
    {
        public static List<MacdIND> Calculate(List<MarketCandleCMD> candles, int length = 31) //   IEnumerable<Double> closePrices)
        {
            List<MacdIND> MacdData = new List<MacdIND>();

            try
            {
                //            var partCandles = candles.Take(2 * length).Reverse().ToArray();
                //            decimal[] data = partCandles.Select(e => (decimal)e.ClosePrice).ToArray();  // closePrices.ToList();

                Double[] data = candles.Select(p => (double)p.ClosePrice).Reverse().ToArray();

                int beginIndex;
                int outNBElements;
                Double[] outMACD = new Double[data.Length];
                Double[] outMACDSignal = new Double[data.Length];
                Double[] outMACDHist = new Double[data.Length];

                var status = Core.MacdFix(0, data.Length - 1, data, 2, out beginIndex, out outNBElements, outMACD, outMACDSignal, outMACDHist);


                if (status == Core.RetCode.Success && outNBElements > 0)
                {
                    /*
                    for (int i = 0; i < outNBElements; i++)
                    {
                        var macds = outMACDSignal[i];
                        //var macds1 = outMACDSignal[1];
                        //var macds2 = outMACDSignal[2];
                        //macd = (decimal)macds;
                        Macdsig = (decimal)macds;
                        //Macdsig1 = (decimal)macds1;
                        //Macdsig2 = (decimal)macds2;
                    }
                    */
                    if (outNBElements > 21)
                    {
                        MacdData.Add(new MacdIND() { MacdValue = (decimal)outMACDSignal[22] });
                        MacdData.Add(new MacdIND() { MacdValue = (decimal)outMACDSignal[21] });
                        MacdData.Add(new MacdIND() { MacdValue = (decimal)outMACDSignal[20] });
                    }
                }

                // Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} TaMacd.Calculate: {(candles[0].PeriodSeconds / 60)},{candles[0].Name}: Macd={MacdData.Macdsig} Macd1={MacdData.Macdsig1} Macd2={MacdData.Macdsig2}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR TaMacd.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }
            return MacdData;
        }
    }
}



