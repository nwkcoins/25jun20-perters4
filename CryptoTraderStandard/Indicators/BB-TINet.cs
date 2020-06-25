using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// https://tulipindicators.org/stochrsi
// the oldest data in the time series to be in index 0
// Checked 25mei20: tinet dll: ok
// Checked 26mei Tinet Tulip: ok

namespace CryptoTraderStandard.Indicators
{
    public class BB_TINet
    {
        public static List<BollingerBandsIND> Calculate(List<MarketCandleCMD> candles, int length = 0, string LogIndicatorCalculationsForSymbol = "", string DataMap = "")
        {
            List<BollingerBandsIND> BBData = new List<BollingerBandsIND>();

            string errmsg = "";
            try
            {
                if (length == 0) length = candles.Count;
                double[] close = candles.Select(e => (double)e.ClosePrice).Take(length).Reverse().ToArray();
                double[] options = new double[] { 14, 2 }; // period = 20?, stddev
                int start = tinet.indicators.bbands.start(options);
                //int start = Tinet.IndicatorStart(15, options);
                int output_length = close.Length - start;
                double[] bb_lower = new double[output_length];
                double[] bb_middle = new double[output_length];
                double[] bb_upper = new double[output_length];

                double[][] inputs = { close };
                double[][] outputs = { bb_lower, bb_middle, bb_upper };
                int success = tinet.indicators.bbands.run(inputs, options, outputs);
                //int success = Tinet.IndicatorRun(15, inputs, options, outputs);

                for (int i = output_length - 1; i >= 0; i--)
                {
                    errmsg = $"i={i} lower={bb_lower[i]} middle={bb_middle[i]} upper={bb_upper[i]} bw={(((bb_upper[i] - bb_lower[i]) / bb_middle[i]) * 100.0)}";
                    BBData.Add(new BollingerBandsIND()
                    {
                        Lower = toDecimal(bb_lower[i]),
                        Middle = toDecimal(bb_middle[i]),
                        Upper = toDecimal(bb_upper[i]),
                        Bandwidth = toDecimal(((bb_upper[i] - bb_lower[i]) / bb_middle[i]) * 100.0)
                    });
                }

                if (candles[0].Name == LogIndicatorCalculationsForSymbol)
                {
                    string csvline = "";
                    string Filename = DataMap + $"\\calculateTiNet-BB-{LogIndicatorCalculationsForSymbol}-{(candles[0].PeriodSeconds / 60)}-{DateTime.Now.ToString("ddMMyy.HHmm")}.csv";
                    StreamWriter swCSVTINetFile = new StreamWriter(Filename);
                    swCSVTINetFile.WriteLine("Nr;Time;CandleClose;;Lower;Middle;Upper;Bandwidth");
                    for (int i = 0; i < output_length; i++)
                    {
                        NumberFormatInfo nfi = new NumberFormatInfo();
                        nfi.NumberDecimalSeparator = ".";
                        csvline = $"{i};{candles[i].Timestamp.ToString("HH:mm")};{candles[i].ClosePrice};;{BBData[i].Lower};{BBData[i].Middle};{BBData[i].Upper};{BBData[i].Bandwidth}";
                        swCSVTINetFile.WriteLine(csvline);
                    }
                    swCSVTINetFile.Close();
                    Console.WriteLine($"TULIPIndicator file written: {Filename}");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR BB_TINet.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}' {errmsg}");
            }

            return BBData;
        }

        private static decimal toDecimal(double d)
        {
            return ((!Double.IsNaN(d)) ? (decimal)d : 0);
        }
    }
}
