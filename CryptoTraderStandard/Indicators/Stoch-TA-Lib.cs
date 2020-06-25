//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using TicTacTec.TA.Library;

namespace CryptoTraderScanner
{

    class CalculateTaStoch
    {
        public static StochIND Calculate(List<MarketCandleCMD> candles, int startCandle = 1, int length = 44) //   IEnumerable<Double> closePrices)
        {
            StochIND StochData = new StochIND();

            try
            {
                var partCandles = candles.Take(2 * length).ToArray();
                Double[] closePrices = partCandles.Select(e => (Double)e.ClosePrice).ToArray();// closePrices.ToList();
                Double[] highPrices = partCandles.Select(e => (Double)e.HighPrice).ToArray();// HighPrice.ToList();
                Double[] lowPrices = partCandles.Select(e => (Double)e.LowPrice).ToArray();// HighPrice.ToList();

                int endIdx = closePrices.Count() - 1;

                Core.RetCode resCode = Core.RetCode.InternalError;
                int outBegIdx;
                int outNBElement;
                double[] outFastK = new double[endIdx - startCandle + 1];
                double[] outFastD = new double[endIdx - startCandle + 1];

                resCode = Core.Stoch(startCandle, endIdx, highPrices, lowPrices, closePrices, 5, 3, Core.MAType.Sma, 3, Core.MAType.Sma, out outBegIdx, out outNBElement, outFastK, outFastD);

                /*for (int i = 0; i < outNBElement; i++)
                {
                    int j = outBegIdx + i;
                    FastK = Math.Round(outFastK[i], 2);
                    FastD = Math.Round(outFastD[i], 2);
                }*/

                StochData.FastK = Math.Round(outFastK[outNBElement - 1], 2);
                StochData.FastD = Math.Round(outFastD[outNBElement - 1], 2);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR TaStoch.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }
            return StochData;
        }
    }
}