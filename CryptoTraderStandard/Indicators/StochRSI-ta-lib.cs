//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using TicTacTec.TA.Library;

namespace CryptoTraderScanner
{
    public class CalculateTaStochRSI
    {
        public static StochIND Calculate(List<MarketCandleCMD> candles, int length = 14) //   IEnumerable<Double> closePrices)
        {
            StochIND StochData = new StochIND();

            try
            {
                //var partCandles = candles.Take((2 * length) + 1).Reverse().ToArray();
                // candles order is desc (now..past); take most recent ClosePrices; Order oldest on top (past..now)
                Double[] closePrices = candles.Select(e => (Double)e.ClosePrice).Take(42).Reverse().ToArray();

                int endIdx = closePrices.Count() - 1;

                Core.RetCode resCode = Core.RetCode.InternalError;
                int outBegIdx;
                int outNBElement;
                Double[] outFastK = new double[endIdx + 1];
                Double[] outFastD = new double[endIdx + 1];

                resCode = Core.StochRsi(0, endIdx, closePrices, length, 1, 1, Core.MAType.Sma, out outBegIdx, out outNBElement, outFastK, outFastD);

                // test loop to determine right index for outFastK, outFastD to return
                for (int i = 0; i < outNBElement; i++)
                {
                    //int j = outBegIdx + i;
                    double FastK = Math.Round(outFastK[i], 2);
                    double FastD = Math.Round(outFastD[i], 2);
                }

                StochData.FastK = Math.Round(outFastK[outNBElement - 1], 2);
                StochData.FastD = Math.Round(outFastD[outNBElement - 1], 2);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR TaStochRSI.Calculate: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }

            return StochData;
        }
    }
}
