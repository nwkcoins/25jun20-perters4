//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CryptoTraderScanner
{
    public class RSI
    {
        public static List<Decimal> Calculate(List<MarketCandleCMD> candles, int length = 0, string LogIndicatorCalculationsForSymbol = "", string DataMap = "")
        {
            List<Decimal> RSIData = new List<Decimal>();
            try
            {
                if (length == 0) length = candles.Count;
                int period = 14;
                List<decimal> prices = candles.Select(e => e.ClosePrice).Take(length).Reverse().ToList();
                int output_length = prices.Count;
                decimal[] rsi = new decimal[output_length];
                decimal gain = 0;
                decimal loss = 0;
                rsi[0] = 0;
                for (int i = 1; i <= period; ++i)
                {
                    decimal diff = prices[i] - prices[i - 1];

                    if (diff >= 0)
                    {
                        gain += diff;
                    }
                    else
                    {
                        loss -= diff;
                    }
                }

                decimal avrg = gain / period;
                decimal avrl = loss / period;
                decimal rs = (loss != 0) ? gain / loss : 0;

                rsi[period] = (rs != -1) ? 100 - (100 / (1 + rs)) : 0;

                for (int i = period + 1; i < output_length; ++i)
                {
                    decimal diff = prices[i] - prices[i - 1];

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
                    rsi[i] = (rs != -1) ? 100 - (100 / (1 + rs)) : 0;
                }

                for (int i = 0; i < rsi.Length; i++)
                {
                    RSIData.Add(rsi[i]);
                }

                if (candles[0].Name == LogIndicatorCalculationsForSymbol)
                {
                    string csvline = "";
                    string Filename = DataMap + $"\\calculate-RSI_zelf-{LogIndicatorCalculationsForSymbol}-{(candles[0].PeriodSeconds / 60)}-{DateTime.Now.ToString("ddMMyy.HHmm")}.csv";
                    StreamWriter swCSVTINetFile = new StreamWriter(Filename);
                    swCSVTINetFile.WriteLine("Nr;Time;CandleClose;;RSI");
                    for (int i = 0; i < output_length; i++)
                    {
                        NumberFormatInfo nfi = new NumberFormatInfo();
                        nfi.NumberDecimalSeparator = ".";
                        csvline = $"{i};{candles[i].Timestamp.ToString("HH:mm")};{candles[i].ClosePrice};;{RSIData[i]}";
                        swCSVTINetFile.WriteLine(csvline);
                    }
                    swCSVTINetFile.Close();
                    Console.WriteLine($"TULIPIndicator file written: {Filename}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR RSI.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }
            return RSIData;
        }
    }
}

