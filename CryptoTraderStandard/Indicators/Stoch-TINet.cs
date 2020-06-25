using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// https://tulipindicators.org/stoch
// the oldest data in the time series to be in index 0
// Check 25mai20 tinet dll: ok
// Check 26mei20 Tinet Tulip: ok

namespace CryptoTraderStandard.Indicators
{
    public class Stoch_TINet
    {
        public static List<StochasticsIND> Calculate(List<MarketCandleCMD> candles, int length = 0, string LogIndicatorCalculationsForSymbol = "", string DataMap = "")
        {
            List<StochasticsIND> StochData = new List<StochasticsIND>();

            string errmsg = "";
            try
            {
                if (length == 0) length = candles.Count;
                double[] close = candles.Select(e => (double)e.ClosePrice).Take(length).Reverse().ToArray();
                double[] high = candles.Select(e => (double)e.HighPrice).Take(length).Reverse().ToArray();
                double[] low = candles.Select(e => (double)e.LowPrice).Take(length).Reverse().ToArray();
                double[] options = new double[] { 14, 3, 3 }; // %k period, %k slowing period, %d period
                int start = tinet.indicators.stoch.start(options);
                //int start = Tinet.IndicatorStart(76, options);
                int output_length = close.Length - start;
                double[] stoch_k = new double[output_length];
                double[] stoch_d = new double[output_length];

                double[][] inputs = { high, low, close };
                double[][] outputs = { stoch_k, stoch_d };
                int success = tinet.indicators.stoch.run(inputs, options, outputs);
                //int success = Tinet.IndicatorRun(76, inputs, options, outputs);

                for (int i = output_length - 1; i >= 0; i--)
                {
                    errmsg = $"i={i} K={stoch_k[i]} D={stoch_d[i]}";
                    StochData.Add(new StochasticsIND()
                    {
                        //ClosePrice = candles[i].ClosePrice,
                        //HighPrice = candles[i].HighPrice,
                        //LowPrice = candles[i].LowPrice,
                        KValue = toDecimal(stoch_k[i]),
                        DValue = toDecimal(stoch_d[i])
                    });
                }

                // Create csv file with candle time, closeprice and K, D values if and for Symbol given
                if (candles[0].Name == LogIndicatorCalculationsForSymbol)
                {
                    string csvline = "";
                    string Filename = DataMap + $"\\calculateTiNet-Stoch-{LogIndicatorCalculationsForSymbol}-{(candles[0].PeriodSeconds / 60)}-{DateTime.Now.ToString("ddMMyy.HHmm")}.csv";
                    StreamWriter swCSVTINetFile = new StreamWriter(Filename);
                    swCSVTINetFile.WriteLine("Nr;Time;Close;Low;High;;K;D");
                    for (int i = 0; i < output_length; i++)
                    {
                        NumberFormatInfo nfi = new NumberFormatInfo();
                        nfi.NumberDecimalSeparator = ".";
                        csvline = $"{i};{candles[i].Timestamp.ToString("HH:mm")};{candles[i].ClosePrice};{candles[i].LowPrice};{candles[i].HighPrice};;{StochData[i].KValue};{StochData[i].DValue}";
                        swCSVTINetFile.WriteLine(csvline);
                    }
                    swCSVTINetFile.Close();
                    Console.WriteLine($"TULIPIndicator file written: {Filename}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR Stoch_TINet.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}' {errmsg}");
            }

            return StochData;
        }

        private static decimal toDecimal(double d)
        {
            return ((!Double.IsNaN(d)) ? (decimal)d : 0);
        }

    }
}
