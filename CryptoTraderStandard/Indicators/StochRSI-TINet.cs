using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tulip;

// https://tulipindicators.org/stochrsi
// the oldest data in the time series to be in index 0
// Check

namespace CryptoTraderStandard.Indicators
{
    public class StochRSI_TINet
    {
        public static List<StochRSIIND> Calculate(List<MarketCandleCMD> candles, int length = 0, string LogIndicatorCalculationsForSymbol = "", string DataMap = "")
        {
            List<StochRSIIND> StochRSIData = new List<StochRSIIND>();

            string errmsg = "";
            try
            {
                if (length == 0) length = candles.Count;
                double[] close = candles.Select(e => (double)e.ClosePrice).Take(length).Reverse().ToArray();
                double[] options = new double[] { 14 }; // period
                //int start = tinet.indicators.stochrsi.start(options); -- not implemented!
                int start = Tinet.IndicatorStart(77, options);
                int output_length = close.Length - start;
                double[] stochrsi = new double[output_length];

                double[][] inputs = { close };
                double[][] outputs = { stochrsi };
                //int success = tinet.indicators.rsi.run(inputs, options, outputs);
                int success = Tinet.IndicatorRun(77, inputs, options, outputs);

                for (int i = output_length - 1; i >= 0; i--) // loop (i=output_length-1 is most recent candle) until (i=0 is most oldest candle) => Add KValue,DValue in order now to past
                {
                    errmsg = $"i={i} rsi={stochrsi[i]}";
                    StochRSIData.Add(new StochRSIIND()
                    {
                        //rsi = toDecimal(rsi[i]),
                        //RSIValue = 0, //?
                        KValue = toDecimal(100 * stochrsi[i]), // rsi
                        DValue = toDecimal(100 * ((i > 1) ? ((stochrsi[i] + stochrsi[i - 1] + stochrsi[i - 2]) / 3) : 0))    // sum(last 3 rsi) / 3
                    });
                }

                if (candles[0].Name == LogIndicatorCalculationsForSymbol)
                {
                    string csvline = "";
                    string Filename = DataMap + $"\\calculateTiNet-StochRSI-{LogIndicatorCalculationsForSymbol}-{(candles[0].PeriodSeconds / 60)}-{DateTime.Now.ToString("ddMMyy.HHmm")}.csv";
                    StreamWriter swCSVTINetFile = new StreamWriter(Filename);
                    swCSVTINetFile.WriteLine("Nr;Time;CandleClose;;K;D");
                    for (int i = 0; i < output_length; i++)
                    {
                        NumberFormatInfo nfi = new NumberFormatInfo();
                        nfi.NumberDecimalSeparator = ".";
                        csvline = $"{i};{candles[i].Timestamp.ToString("HH:mm")};{candles[i].ClosePrice};;{StochRSIData[i].KValue};{StochRSIData[i].DValue}";
                        swCSVTINetFile.WriteLine(csvline);
                    }
                    swCSVTINetFile.Close();
                    Console.WriteLine($"TULIPIndicator file written: {Filename}");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR StochRSI_TINet.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}' {errmsg}");
            }

            return StochRSIData;
        }
        private static decimal toDecimal(double d)
        {
            return ((!Double.IsNaN(d)) ? (decimal)d : 0);
        }

    }
}
