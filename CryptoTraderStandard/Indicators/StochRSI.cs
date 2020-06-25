//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;

/* 
 * https://school.stockcharts.com/doku.php?id=technical_indicators:relative_strength_index_rsi
 * candle 0 is most recent (now), last candle is most long ago (earliest datetime)
 * period = 14 candles; last is 
 * diff(1..14 with 0..13) candles Closeprices => sum gains/14 and sum losses/14
 * candles 15..last: 
*/

namespace CryptoTraderScanner
{
    public class StochRSI
    {
        public static StochRSIIND Calculate(List<MarketCandleCMD> candles, int period = 14, int length = 36)
        {
            StochRSIIND StochRSIData = new StochRSIIND();

            try
            {
                List<Decimal> prices = candles.Select(e => e.ClosePrice).Reverse().Take(length).ToList();
                Decimal[] rsi = new Decimal[prices.Count];
                List<Decimal> RSIS = new List<Decimal>();
                Decimal gain = 0;
                Decimal loss = 0;

                rsi[0] = 0;
                //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} StochRSI: {0} {prices[0]}");
                for (int i = 1; i <= period; ++i)
                {
                    Decimal diff = prices[i] - prices[i - 1];

                    if (diff >= 0)
                    {
                        gain += diff;
                    }
                    else
                    {
                        loss -= diff;
                    }
                    //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} StochRSI: {i} {prices[i]} {diff} {((diff >= 0) ? gain.ToString("0.00000000") : "")} {((diff < 0) ? "          " : loss.ToString("0.00000000"))}");
                }

                Decimal avrg = gain / period;
                Decimal avrl = loss / period;
                Decimal rs = (loss != 0) ? gain / loss : 0;

                rsi[period] = (rs != -1) ? 100 - (100 / (1 + rs)) : 0;

                for (int i = period + 1; i < prices.Count; ++i)
                {
                    Decimal diff = prices[i] - prices[i - 1];

                    if (diff >= 0)
                    {
                        avrg = ((avrg * (period - 1)) + diff) / period;
                        avrl = (avrl * (period - 1)) / period;
                    }
                    else
                    {
                        avrl = ((avrl * (period - 1)) - diff) / period;
                        avrg = (avrg * (period - 1)) / period;
                    }
                    rs = (avrl != 0) ? avrg / avrl : 0;
                    RSIS.Add(rsi[i] = (rs != -1) ? 100 - (100 / (1 + rs)) : 0);
                    //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} StochRSI: i={i} price[i]={prices[i]} diff={diff} gain={gain} loss={loss}");

                }
                //            return rsi[prices.Count - 1];
                //                RSIS.Add(rsi[i] = 100 - (100 / (1 + rs)));
                //                RSIS.Add(rsi[i]);

                //  Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {RSIS[15]}");
                ///Calculate StocasticRsi
                List<Decimal> StocRSI = new List<Decimal>();
                for (int i = 0; i < RSIS.Count; ++i)
                {
                    Decimal[] RSI = RSIS.Skip(i + 13).Take(1).ToArray();
                    //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} RSI[0]={RSI[0]}");
                    Decimal MaxRSI = RSIS.Skip(i).Take(14).Max();
                    Decimal MinRSI = RSIS.Skip(i).Take(14).Min();
                    Decimal result = (MaxRSI != MinRSI) ? (RSI[0] - MinRSI) / (MaxRSI - MinRSI) : 0;
                    StocRSI.Add(Math.Round(result, 4));
                }
                //Console.ReadLine();
                // StocRSI.ForEach(c => Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} c={c}"));

                //Calculate K
                List<Decimal> K = new List<Decimal>();
                for (int i = 0; i < StocRSI.Count - 2; ++i)
                {
                    Decimal k = StocRSI.Skip(i).Take(3).Sum() / 3;
                    K.Add(k);
                }

                ///Calculate D
                List<Decimal> D = new List<Decimal>();
                for (int i = 0; i < K.Count - 2; ++i)
                {
                    Decimal d = K.Skip(i).Take(3).Sum() / 3;
                    D.Add(d);
                }

                StochRSIData.DValue = Math.Round(((D.Count > 0) ? D[D.Count - 1] * 100 : 0), 2);
                //todo    StochRSIData.DValue1 = Math.Round(((D.Count > 1) ? D[D.Count - 2] * 100 : 0), 2);
                //todo    StochRSIData.DValue2 = Math.Round(((D.Count > 2) ? D[D.Count - 3] * 100 : 0), 2);
                //todo    StochRSIData.DValue3 = Math.Round(((D.Count > 3) ? D[D.Count - 4] * 100 : 0), 2);
                StochRSIData.KValue = Math.Round(((K.Count > 0) ? K[K.Count - 1] * 100 : 0), 2);
                //    StochRSIData.SRSI = Math.Round(((StocRSI.Count > 0) ? StocRSI[StocRSI.Count - 1] : 0), 2);
                //    StochRSIData.RSIValue = Math.Round(((RSIS.Count > 0) ? RSIS[RSIS.Count - 1] : 0), 2);
                if (double.IsNaN((double)StochRSIData.KValue))
                {
                    StochRSIData.KValue = 0;
                }
                if (double.IsNaN((double)StochRSIData.DValue))
                {
                    StochRSIData.DValue = 0;
                }
                //    if (double.IsNaN((double)StochRSIData.SRSI))
                //    {
                //        StochRSIData.SRSI = 0;
                //    }
                //return rsi[prices.Count - 1];
                //StochRSIData.rsi = rsi[prices.Count - 1];
                StochRSIData.KValue = rsi[prices.Count - 1];

                //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} StochRSI: {(candles[0].PeriodSeconds / 60)},{candles[0].Name}: K={StochRSIData.KValue} D={StochRSIData.DValue} SRSI={StochRSIData.N} RSIValue={StochRSIData.RSIValue} rsi={StochRSIData.rsi}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR StochRSI.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }

            return StochRSIData;
        }
    }
}
