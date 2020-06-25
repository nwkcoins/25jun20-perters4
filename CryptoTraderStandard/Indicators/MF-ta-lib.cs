//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Linq;
using TicTacTec.TA.Library;

namespace CryptoTraderScanner
{
    public class CalculateTaMFI
    {
        public static Decimal Calculate(List<MarketCandleCMD> candles, int startCandle = 0, int length = 24)
        {
            try
            {
                var partCandles = candles.Take(length).Reverse().ToArray();
                Double[] closePrices = partCandles.Select(e => (Double)e.ClosePrice).ToArray();  // closePrices.ToList();
                Double[] HighPrices = partCandles.Select(e => (Double)e.HighPrice).ToArray();
                Double[] LowPrices = partCandles.Select(e => (Double)e.LowPrice).ToArray();
                Double[] Volumes = partCandles.Select(e => (Double)e.BaseCurrencyVolume).ToArray();

                int endIdx = closePrices.Count() - 1;

                Core.RetCode resCode = Core.RetCode.InternalError;
                int outBegIdx;
                int outNBElement;
                int optInTimePeriod = 12;
                Double[] outReal = new Double[endIdx - startCandle + 1];

                resCode = Core.Mfi(startCandle, endIdx, HighPrices, LowPrices, closePrices, Volumes, optInTimePeriod, out outBegIdx, out outNBElement, outReal);

                Decimal ReturnValue = (decimal)((outNBElement > 10) ? Math.Round(outReal[11], 2) : -1);

                //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} Mfi: {(candles[0].PeriodSeconds / 60)},{candles[0].Name}: {ReturnValue}");

                return ReturnValue;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR TaMFI.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
                return 0;
            }
        }
    }
}


