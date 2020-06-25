//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Linq;
using TicTacTec.TA.Library;

namespace CryptoTraderScanner
{
    public class CalculateTaRSI
    {
        public static decimal Calculate(List<MarketCandleCMD> candles, int startCandle = 0, int length = 14) //   IEnumerable<Double> closePrices)
        {
            try
            {
                var partCandles = candles.Take(length).ToArray();
                Double[] closePrices = partCandles.Select(e => (Double)e.ClosePrice).ToArray();  // closePrices.ToList();

                int endIdx = closePrices.Count() - 1;

                Core.RetCode resCode = Core.RetCode.InternalError;
                int outBegIdx;
                int outNBElement;
                Double[] outReal = new Double[endIdx - startCandle + 1];

                resCode = Core.Rsi(startCandle, endIdx, closePrices, length, out outBegIdx, out outNBElement, outReal);

                /*for (int i = 0; i < outNBElement; i++)
                {
                    int j = outBegIdx + i;
                    rsi = Math.Round(outReal[i], 2);
                }*/
                //return outReal;
                return (decimal)Math.Round(outReal[outNBElement - 1], 2);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR TaRSI.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
                return 0;
            }
        }
    }
}