//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTraderScanner
{
    public class BollingerBands
    {
        public static BollingerBandsIND Calculate(List<MarketCandleCMD> candles, int length = 20, int deviations = 2)
        {
            BollingerBandsIND bollingerBandsData = new BollingerBandsIND();

            try
            {
                List<Decimal> prices = candles.Select(e => e.ClosePrice).Take(length).ToList();
                Decimal sd = GetStandardDeviation(prices);

                bollingerBandsData.Middle = candles.Select(e => e.ClosePrice).Take(length).ToList().Average();
                //    bollingerBandsData.Middle1 = candles.Skip(1).Select(e => e.ClosePrice).Take(length).ToList().Average();
                //    bollingerBandsData.Middle2 = candles.Skip(2).Select(e => e.ClosePrice).Take(length).ToList().Average();
                //    bollingerBandsData.Middle3 = candles.Skip(3).Select(e => e.ClosePrice).Take(length).ToList().Average();
                //    bollingerBandsData.Middle4 = candles.Skip(4).Select(e => e.ClosePrice).Take(length).ToList().Average();
                bollingerBandsData.Lower = bollingerBandsData.Middle - deviations * sd;
                bollingerBandsData.Upper = bollingerBandsData.Middle + deviations * sd;
                //    bollingerBandsData.MiddleUp = (bollingerBandsData.Middle + bollingerBandsData.Upper) / 2.0m;

                bollingerBandsData.Bandwidth = ((bollingerBandsData.Upper - bollingerBandsData.Lower) / bollingerBandsData.Middle) * 100.0m;

                //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} BollingerBands: {(candles[0].PeriodSeconds / 60)},{candles[0].Name}: Upper={bollingerBandsData.Upper} Middle={bollingerBandsData.Middle} Lower={bollingerBandsData.Lower}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")}  ERROR BollingerBands.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }
            return bollingerBandsData;
        }

        private static Decimal GetStandardDeviation(List<decimal> numberSet)
        {
            try
            {
                Decimal mean = numberSet.Average();

                return (Decimal)Math.Sqrt(numberSet.Sum(x => Math.Pow((double)(x - mean), 2)) / numberSet.Count);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR BollingerBands.GetStandardDeviation: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
                return Decimal.Zero;
            }
        }
    }
}
