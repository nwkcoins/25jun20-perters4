//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTraderScanner
{
    public class MoneyFlow
    {
        public static decimal Calculate(List<MarketCandleCMD> Candels, int length = 12)
        {
            try
            {
                Candels.Take(24).Reverse();
                var TypicalimitPrice = Candels.Select(c => (c.ClosePrice + c.HighPrice + c.LowPrice) / 3).ToList();
                var MoneyFlow = Candels.Select(c => ((c.ClosePrice + c.HighPrice + c.LowPrice) / 3) * (Decimal)c.BaseCurrencyVolume).ToList();
                List<decimal> PositiveMoneyFlow = new List<decimal>();
                List<decimal> NegativeMoneyFlow = new List<decimal>();

                for (int i = 1; i <= length; ++i)
                {
                    if (TypicalimitPrice[i] > TypicalimitPrice[i - 1])
                    {
                        PositiveMoneyFlow.Add((decimal)MoneyFlow[i]);
                    }
                    else
                    {
                        NegativeMoneyFlow.Add((decimal)MoneyFlow[i]);
                    }
                }

                decimal MoneyRatio = PositiveMoneyFlow.Sum() / NegativeMoneyFlow.Sum();
                decimal MoneyFlowIndex = 100 - (100 / (MoneyRatio + 1));

                return Math.Round(MoneyFlowIndex, 2);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR MoneyFlow.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
                return 0;
            }
        }
    }
}

