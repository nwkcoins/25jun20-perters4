//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTraderScanner
{

    public class Stochastics
    {
        public static List<StochasticsIND> Calculate(List<MarketCandleCMD> candles, int length = 14)
        {
            List<StochasticsIND> stochasticsData = new List<StochasticsIND>();

            try
            {
                if (length > 0 && candles.Count > (length + 2))
                {
                    Decimal K0 = Math.Round(GetK(candles, 0, length), 2);
                    Decimal K1 = Math.Round(GetK(candles, 1, length), 2);
                    Decimal K2 = Math.Round(GetK(candles, 2, length), 2);
                    Decimal K3 = Math.Round(GetK(candles, 3, length), 2);
                    Decimal K4 = Math.Round(GetK(candles, 4, length), 2);
                    Decimal K5 = Math.Round(GetK(candles, 5, length), 2);
                    Decimal K6 = Math.Round(GetK(candles, 6, length), 2);

                    stochasticsData.Add(new StochasticsIND() { KValue = K0, DValue = (K0 + K1 + K2) / 3 });
                    stochasticsData.Add(new StochasticsIND() { KValue = K1, DValue = (K1 + K2 + K3) / 3 });
                    stochasticsData.Add(new StochasticsIND() { KValue = K2, DValue = (K2 + K3 + K4) / 3 });
                    stochasticsData.Add(new StochasticsIND() { KValue = K3, DValue = (K3 + K4 + K5) / 3 });
                    stochasticsData.Add(new StochasticsIND() { KValue = K4, DValue = (K4 + K5 + K6) / 3 });
                }

                // Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} StochasticsIND.Calculate: {(candles[0].PeriodSeconds / 60)},{candles[0].Name}: K={stochasticsData.K} D={stochasticsData.D}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR Stochactics.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }

            return stochasticsData;
        }

        private static Decimal GetK(List<MarketCandleCMD> candles, int prevSkip, int length)
        {
            try
            {
                List<MarketCandleCMD> lastCandles = new List<MarketCandleCMD>(candles.Skip<MarketCandleCMD>(prevSkip).Take<MarketCandleCMD>(length).ToList());
                Decimal lowest = lastCandles.Min(e => e.LowPrice);
                Decimal highprice = lastCandles.Max(e => e.HighPrice);
                Decimal closePrice = lastCandles[0].ClosePrice; //[prevSkip].ClosePrice;
                return ((highprice != lowest) ? 100m * ((closePrice - lowest) / (highprice - lowest)) : 0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR TaStoch.GetK: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
                return Decimal.Zero;
            }
        }
    }
}
