using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// https://tulipindicators.org/stochrsi
// the oldest data in the time series to be in index 0
// Checked 25mei20: tinet dll: ok
// Checked 26mei20: Tinet Tulip: ok

namespace CryptoTraderStandard.Indicators
{
    public class MFI_TINet
    {
        public static List<Decimal> Calculate(List<MarketCandleCMD> candles, int length = 0, string LogIndicatorCalculationsForSymbol = "", string DataMap = "")
        {
            List<Decimal> MFIData = new List<Decimal>();

            string errmsg = "";
            try
            {
                if (length == 0) length = candles.Count;
                double[] close = candles.Select(e => (double)e.ClosePrice).Take(length).Reverse().ToArray();
                double[] high = candles.Select(e => (double)e.HighPrice).Take(length).Reverse().ToArray();
                double[] low = candles.Select(e => (double)e.LowPrice).Take(length).Reverse().ToArray();
                double[] volume = candles.Select(e => (double)e.BaseCurrencyVolume).Take(length).Reverse().ToArray();
                double[] options = new double[] { 12 }; // period
                int start = tinet.indicators.mfi.start(options);
                //int start = Tinet.IndicatorStart(54, options);
                int output_length = close.Length - start;
                double[] mfi = new double[output_length];

                double[][] inputs = { high, low, close, volume };
                double[][] outputs = { mfi };
                int success = tinet.indicators.mfi.run(inputs, options, outputs);
                //int success = Tinet.IndicatorRun(54, inputs, options, outputs);

                for (int i = output_length - 1; i >= 0; i--)
                {
                    errmsg = $"i={i} mfi={mfi[i]}";
                    MFIData.Add(
                        toDecimal(mfi[i])
                    );
                }

                if (candles[0].Name == LogIndicatorCalculationsForSymbol)
                {
                    string csvline = "";
                    string Filename = DataMap + $"\\calculateTiNet-MFI-{LogIndicatorCalculationsForSymbol}-{(candles[0].PeriodSeconds / 60)}-{DateTime.Now.ToString("ddMMyy.HHmm")}.csv";
                    StreamWriter swCSVTINetFile = new StreamWriter(Filename);
                    swCSVTINetFile.WriteLine("Nr;Time;Close;High;Low;BaseVolume;;MFI");
                    for (int i = 0; i < output_length; i++)
                    {
                        NumberFormatInfo nfi = new NumberFormatInfo();
                        nfi.NumberDecimalSeparator = ".";
                        csvline = $"{i};{candles[i].Timestamp.ToString("HH:mm")};{candles[i].ClosePrice};{candles[i].HighPrice};{candles[i].LowPrice};{candles[i].BaseCurrencyVolume};;{MFIData[i]}";
                        swCSVTINetFile.WriteLine(csvline);
                    }
                    swCSVTINetFile.Close();
                    Console.WriteLine($"TULIPIndicator file written: {Filename}");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR MFI_TINet.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}' {errmsg}");
            }

            return MFIData;
        }

        private static decimal toDecimal(double d)
        {
            return ((!Double.IsNaN(d)) ? (decimal)d : 0);
        }

    }
}
