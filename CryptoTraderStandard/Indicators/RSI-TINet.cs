using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// https://tulipindicators.org/rsi
// the oldest data in the time series to be in index 0
// Checked 25mei20 tinet dll: ok
// Checked 26mei20 Tinet Tulip: iets grotere afwijking 54 -> 56

namespace CryptoTraderStandard.Indicators
{
    public class RSI_TINet
    {
        public static List<Decimal> Calculate(List<MarketCandleCMD> candles, int length = 0, string LogIndicatorCalculationsForSymbol = "", string DataMap = "")
        {
            List<Decimal> RSIData = new List<Decimal>();

            string errmsg = "";
            try
            {
                if (length == 0) length = candles.Count;
                double[] close = candles.Select(e => (double)e.ClosePrice).Take(length).Reverse().ToArray();
                double[] options = new double[] { 14 }; // period
                int start = tinet.indicators.rsi.start(options);
                //int start = Tinet.IndicatorStart(69, options);
                int output_length = close.Length - start;
                double[] rsi = new double[output_length];

                double[][] inputs = { close };
                double[][] outputs = { rsi };
                int success = tinet.indicators.rsi.run(inputs, options, outputs);
                //int success = Tinet.IndicatorRun(69, inputs, options, outputs);

                for (int i = output_length - 1; i >= 0; i--)
                {
                    errmsg = $"i={i} rsi={rsi[i]}";
                    RSIData.Add(toDecimal(rsi[i]));
                }

                if (candles[0].Name == LogIndicatorCalculationsForSymbol)
                {
                    string csvline = "";
                    string Filename = DataMap + $"\\calculateTiNet-RSI-{LogIndicatorCalculationsForSymbol}-{(candles[0].PeriodSeconds / 60)}-{DateTime.Now.ToString("ddMMyy.HHmm")}.csv";
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
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR RSI_TINet.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}' {errmsg}");
            }

            return RSIData;
        }

        private static decimal toDecimal(double d)
        {
            return ((!Double.IsNaN(d)) ? (decimal)d : 0);
        }

    }
}
