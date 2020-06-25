using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// https://tulipindicators.org/stochrsi
// the oldest data in the time series to be in index 0
// checked: 25mei20 tinet dll: ? as waarden in tradingview (binance) slechts 4 dec.
// checked: 26 mei20 Tinet Tulip: same

namespace CryptoTraderStandard.Indicators
{
    public class MACD_TINet
    {
        public static List<MacdIND> Calculate(List<MarketCandleCMD> candles, int length = 0, string LogIndicatorCalculationsForSymbol = "", string DataMap = "")
        {
            List<MacdIND> MacdData = new List<MacdIND>();

            string errmsg = "";
            try
            {
                if (length == 0) length = candles.Count;
                double[] close = candles.Select(e => (double)e.ClosePrice).Take(length).Reverse().ToArray();
                double[] options = new double[] { 12, 26, 9 }; // shortperiod, longperiod, signalperiod
                int start = tinet.indicators.macd.start(options);
                //int start = Tinet.IndicatorStart(48, options);
                int output_length = close.Length - start;
                double[] Macd_Value = new double[output_length];
                double[] Macd_Signal = new double[output_length];
                double[] Macd_Histogram = new double[output_length];

                double[][] inputs = { close };
                double[][] outputs = { Macd_Value, Macd_Signal, Macd_Histogram };
                int success = tinet.indicators.macd.run(inputs, options, outputs);
                //int success = Tinet.IndicatorRun(48, inputs, options, outputs);

                for (int i = output_length - 1; i >= 0; i--)
                {
                    errmsg = $"i={i} macd={Macd_Value[i]} Signal={Macd_Signal[i]} Histo={Macd_Histogram[i]}";
                    MacdData.Add(new MacdIND()
                    {
                        MacdValue = toDecimal(Macd_Value[i]),
                        MacdSignal = toDecimal(Macd_Signal[i]),
                        MacdHistogram = toDecimal(Macd_Histogram[i])
                    });
                }

                if (candles[0].Name == LogIndicatorCalculationsForSymbol)
                {
                    string csvline = "";
                    string Filename = DataMap + $"\\calculateTiNet-Macd-{LogIndicatorCalculationsForSymbol}-{(candles[0].PeriodSeconds / 60)}-{DateTime.Now.ToString("ddMMyy.HHmm")}.csv";
                    StreamWriter swCSVTINetFile = new StreamWriter(Filename);
                    swCSVTINetFile.WriteLine("Nr;Time;CandleClose;;Macd;Signal;Histogram");
                    for (int i = 0; i < output_length; i++)
                    {
                        NumberFormatInfo nfi = new NumberFormatInfo();
                        nfi.NumberDecimalSeparator = ".";
                        csvline = $"{i};{candles[i].Timestamp.ToString("HH:mm")};{candles[i].ClosePrice};;{MacdData[i].MacdValue};{MacdData[i].MacdSignal};{MacdData[i].MacdHistogram}";
                        swCSVTINetFile.WriteLine(csvline);
                    }
                    swCSVTINetFile.Close();
                    Console.WriteLine($"TULIPIndicator file written: {Filename}");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR MACD_TINet.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}' {errmsg}");
            }

            return MacdData;
        }

        private static decimal toDecimal(double d)
        {
            return ((!Double.IsNaN(d)) ? (decimal)d : 0);
        }

    }
}
