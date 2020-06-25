//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using TicTacTec.TA.Library;

namespace CryptoTraderScanner
{
    public class CalculateTaMA20
    {
        public static MA20IND Calculate(List<MarketCandleCMD> candles, int length = 31) //   IEnumerable<Double> closePrices)
        {
            try
            {
                Double[] closePrices = candles.Select(e => (Double)e.ClosePrice).Take(length).ToArray();  // closePrices.ToList();

                int endIdx = closePrices.Count() - 1;

                Core.RetCode resCode = Core.RetCode.InternalError;
                int outBegIdx;
                int outNBElement;
                int optInTimePeriod = 20;
                Double[] outReal = new Double[endIdx + 1];

                resCode = Core.Sma(0, endIdx, closePrices, optInTimePeriod, out outBegIdx, out outNBElement, outReal);

                /*for (int i = 0; i < outNBElement; i++)
                {
                    int j = outBegIdx + i;
                    ma20 = outReal[i];
                    ma201 = outReal[i + 1];
                    ma202 = outReal[i + 2];
                    ma203 = outReal[i + 3];
                }*/

                return new MA20IND()
                {
                    ma20 = outReal[outNBElement - 1],
                    ma201 = outReal[outNBElement], // bestaat deze en volgende +1 en +2 wel?? Moet het niet -2, -3 en -4 zijn??
                    ma202 = outReal[outNBElement + 1],
                    ma203 = outReal[outNBElement + 2],
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR TaMA20.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
                return new MA20IND();
            }
        }
    }
}

