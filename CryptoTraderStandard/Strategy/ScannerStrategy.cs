using CryptoTraderScanner;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.Indicators;
using CryptoTraderStandard.ScannerSQLiteDB;
using System;
using System.Collections.Generic;
using System.IO;

namespace CryptoTraderStandard.Strategy
{
    public class ScannerStrategy
    {
        public ScannerStrategy()
        {
        }

        private static MarketCandleCMD _candle1st;
        //private static StochasticsIND _stochastics = new StochasticsIND();
        private static List<StochasticsIND> _stochastics = new List<StochasticsIND>();

        //private static BollingerBandsIND _bollingerbands = new BollingerBandsIND();
        private static List<BollingerBandsIND> _bollingerbands = new List<BollingerBandsIND>();

        //private static StochRSIIND _stochasticRSI = new StochRSIIND();
        private static List<StochRSIIND> _stochasticRSIs = new List<StochRSIIND>();
        //private static Decimal _rsi;
        //private static List<Decimal> _rsiZelf = new List<Decimal>();
        private static List<Decimal> _rsi = new List<Decimal>();

        private static List<Decimal> _mfis;

        //private static MacdIND _macd = new MacdIND();
        private static List<MacdIND> _macds = new List<MacdIND>();

        //private static MA20IND _ma20 = new MA20IND();

        //private static StochIND _stochdata = new StochIND();

        private static int _flatCandles;
        private static decimal _panic;
        private static StreamWriter swCSVIndFile = null;


        private static string _CalculateIndicators(List<MarketCandleCMD> Candles, int MaxFlatCandleCount)
        {
            try
            {
                Settings Settings = SettingsStore.Load();

                _candle1st = Candles[0];
                //_stochastics = Stochastics.Calculate(Candles);
                _stochastics = Stoch_TINet.Calculate(Candles, 0, Settings.LogIndicatorCalculationsForSymbol, Settings.DirDataMap);

                //_bollingerbands = BollingerBands.Calculate(Candles);
                _bollingerbands = BB_TINet.Calculate(Candles, 0, Settings.LogIndicatorCalculationsForSymbol, Settings.DirDataMap);

                //_stochasticRSI = StochRSI.Calculate(Candles);
                _stochasticRSIs = StochRSI_TINet.Calculate(Candles, 0, Settings.LogIndicatorCalculationsForSymbol, Settings.DirDataMap);

                //_rsiZelf = RSI.Calculate(Candles, 0, Settings.LogIndicatorCalculationsForSymbol, Settings.DirDataMap);
                _rsi = RSI_TINet.Calculate(Candles, 0, Settings.LogIndicatorCalculationsForSymbol, Settings.DirDataMap);

                //_mfi = MoneyFlow.Calculate(Candles);
                _mfis = MFI_TINet.Calculate(Candles, 0, Settings.LogIndicatorCalculationsForSymbol, Settings.DirDataMap);

                //_macd = CalculateTaMacd.Calculate(Candles);
                _macds = MACD_TINet.Calculate(Candles, 0, Settings.LogIndicatorCalculationsForSymbol, Settings.DirDataMap);

                //_ma20 = CalculateTaMA20.Calculate(Candles);
                //_stochdata = CalculateTaStochRSI.Calculate(Candles); // klopt niet erg..

                _flatCandles = 0;
                for (int i = 0; i < MaxFlatCandleCount; i++)
                {
                    if ((i < Candles.Count) && (Candles[i].QuoteCurrencyVolume <= 0)) _flatCandles++;
                }

                _panic = (decimal)((_candle1st.OpenPrice != 0) ? ((_candle1st.ClosePrice / _candle1st.OpenPrice) * 100m) : 0);

                return "";
            }
            catch (Exception E)
            {
                return $"ERROR _CalculateIndicators: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }
        }

        public static void CheckMarketpairFor1stBuySell(MarketPairsCMD MarketPair, List<MarketCandleCMD> Candles, ref SignalsTBL NewSignal)
        {
            try
            {
                Settings Settings = SettingsStore.Load();

                if (Candles.Count >= 20)
                {
                    string Message = _CalculateIndicators(Candles, Settings.MaxFlatCandleCount);

                    try
                    {
                        if (Settings.WriteCsvFile)
                        {
                            string Filename = $"{Settings.DirDataMap}\\indicatorsdata_{_candle1st.PeriodSeconds}_{DateTime.Now.ToString("ddMMMyy")}.csv";
                            string csvline = $"{_candle1st.Timestamp.ToString("dd-MMM-yyyy HH:mm:ss")};{_candle1st.PeriodSeconds};{_candle1st.Name};"
                                + $"{_candle1st.OpenPrice};{_candle1st.ClosePrice};{_candle1st.LowPrice};{_candle1st.HighPrice};{_candle1st.BaseCurrencyVolume};{_candle1st.QuoteCurrencyVolume};"
                                + $"{_bollingerbands[0].Bandwidth};{_bollingerbands[0].Upper};{_bollingerbands[0].Middle};{_bollingerbands[0].Lower};{_stochasticRSIs[0].KValue};{_stochasticRSIs[0].DValue};"
                                + $"{_stochastics[0].KValue};{_stochastics[0].DValue};{_mfis[0]};{_macds[0].MacdValue};{_macds[0].MacdSignal};{_macds[0].MacdHistogram}";
                            if (!File.Exists(Filename))
                            {
                                csvline = "Time;Timeframe;Symbol;Openprice;Closeprice;Lowprice;Highprice;Basevolume;Quotevolume;BBWidth;BBUpper;BBMiddle;BBLower;RSIValue;RSI_K;RSI_D;Stoch_K;Stoch_D;MFI;MFITA;Macd;MacdSignal;MacdHistogram"
                                    + Environment.NewLine + csvline;
                            }
                            if (swCSVIndFile == null || Filename != ((FileStream)(swCSVIndFile.BaseStream)).Name)
                            {
                                if (swCSVIndFile != null) swCSVIndFile.Dispose();
                                swCSVIndFile = new StreamWriter(Filename);
                            }
                            swCSVIndFile.WriteLine(csvline);
                            //File.AppendAllText(Filename, csvline);
                            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckMarketpairFor1stBuySell: wrote indicator data to {Filename}");
                        }
                    }
                    catch (Exception Ex)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckMarketpairFor1stBuySell Write Indicator CSVdata ERROR: {Ex.Message}");
                    }

                    if (String.IsNullOrEmpty(Message))
                    {
                        // always updated
                        NewSignal.BuyPrice = _candle1st.ClosePrice;
                        NewSignal.ClosePrice = _candle1st.ClosePrice;
                        NewSignal.BBWidth = Math.Round(_bollingerbands[0].Bandwidth, 2);
                        NewSignal.BBLower = Math.Round(_bollingerbands[0].Lower, 2);
                        NewSignal.BBMiddle = Math.Round(_bollingerbands[0].Middle, 2);
                        NewSignal.BBUpper = Math.Round(_bollingerbands[0].Upper, 2);
                        NewSignal.RSI = Math.Round(_rsi[0], 0);
                        NewSignal.RSIK = Math.Round(_stochasticRSIs[0].KValue, 0);
                        NewSignal.RSID = Math.Round(_stochasticRSIs[0].DValue, 0);
                        NewSignal.MFI = Math.Round(_mfis[0], 0);
                        NewSignal.StochK = Math.Round(_stochastics[0].KValue, 0);
                        NewSignal.StochD = Math.Round(_stochastics[0].DValue, 0);
                        NewSignal.MACD = Math.Round(_macds[0].MacdValue, 6);
                        NewSignal.MACDSignal = Math.Round(_macds[0].MacdSignal, 6);
                        NewSignal.MACDHistogram = Math.Round(_macds[0].MacdHistogram, 6);

                        // more then 5 flat Candles in last 15 Candles ?
                        if (_flatCandles > Settings.MaxFlatCandles)
                        {
                            NewSignal.Message = $"flatCandles={_flatCandles} > {Settings.MaxFlatCandles} => No Action";
                        }

                        // check for panic
                        else if ((_panic < (100 - Settings.MaxPanic)) || (_panic > (100 + Settings.MaxPanic)))
                        {
                            // more then 5% panic
                            NewSignal.Message = $"Panic={Math.Round((_panic),2)} < {Math.Round((100 - Settings.MaxPanic),2)} OR > {Math.Round((100 + Settings.MaxPanic),2)} => No Action";
                        }

                        else if (Settings.McCheckActive && ((NewSignal.Bm1Hr < Settings.MinMC4Signal) || (NewSignal.Bm4Hr < Settings.MinMC4Hr4Signal)))
                        {
                            NewSignal.Message = $"BM active AND ( (BM1Hr={Math.Round((NewSignal.Bm1Hr),2)} < {Settings.MinMC4Signal}) OR (BM4Hr={Math.Round((NewSignal.Bm4Hr),2)} < {Settings.MinMC4Hr4Signal}) ) => No Action";
                        }

                        else if (Settings.ChMiddleUP)
                        {
                            NewSignal.Strategy = SignalsTBL.Strategies.MiddleUp;

                            // Stoch
                            if (Settings.ChStoch) // Stochastic
                            {
                                // bolling bands width check
                                if ((_candle1st.ClosePrice > Settings.LowSatBTC)
                                    && (_candle1st.ClosePrice > Settings.LowSatBTC)
                                    && (_candle1st.ClosePrice > _bollingerbands[0].Middle)
                                    && (_candle1st.ClosePrice < ((_bollingerbands[0].Middle + _bollingerbands[0].Upper) / 2.0m))
                                    && (_bollingerbands[0].Bandwidth > Settings.MinBollingerBandWidth)
                                    && (_bollingerbands[0].Bandwidth < Settings.MaxBollingerBandWidth))
                                {
                                    if (Settings.ChMACDUPMU) // MACD Signal UP
                                    {
                                        //if ((candle.ClosePrice < _bollingerbands[0].Upper))
                                        if ((_stochastics[0].KValue < Settings.StochK)
                                            && (_stochastics[0].DValue < Settings.StochD)
                                            && (_stochastics[1].DValue < _stochastics[0].DValue)
                                            && (_stochastics[2].DValue < _stochastics[1].DValue)
                                            //&& (_stochastics[3].DValue < _stochastics[2].DValue)
                                            && (_bollingerbands[1].Middle < _bollingerbands[0].Middle)
                                            && (_bollingerbands[2].Middle < _bollingerbands[1].Middle)
                                            //&& (_bollingerbands[3].Middle < _bollingerbands[2].Middle)
                                            //&& (_bollingerbands[4].Middle < _bollingerbands[3].Middle)
                                            && (_macds[1].MacdValue < _macds[0].MacdValue)
                                            && (_macds[2].MacdValue < _macds[1].MacdValue)
                                            && Settings.AllowLong)
                                        {
                                            // open buy order when price closes above Middle bollinger bands
                                            // and stochastics K & D are both below 45
                                            // K3 smaller K2 and K2 smaller K
                                            //StrategyResult.BolingerBandwidth = _bollingerbands[0].Bandwidth;
                                            // Decimal buyprice = ((_bollingerbands[0].Middle / 1000) * 1001);
                                            if (_bollingerbands[0].Bandwidth < 1.5m)
                                            {
                                                NewSignal.TakeProfit = 0.4m;
                                            }
                                            else
                                            {
                                                NewSignal.TakeProfit = Math.Round((((((_bollingerbands[0].Upper / 1000) * 998) - (_candle1st.ClosePrice)) / (_candle1st.ClosePrice)) * 100), 2);
                                            }
                                            NewSignal.StopLoss = Math.Round((((((_bollingerbands[0].Lower / 1000) * 999) - (_candle1st.ClosePrice)) / (_candle1st.ClosePrice)) * 100), 2);
                                            NewSignal.TradeType = SignalsTBL.TradeTypes.Long;
                                            NewSignal.Valid = true;
                                            NewSignal.SignalAction = SignalsTBL.SignalActions.Buy;
                                            NewSignal.Message = "Stoch,MacdUp: Stoch[012],BB[12],Macd[12] => Buy (TakeProfit,StopLoss)";
                                        }
                                        else
                                        {
                                            Message = "Stoch + 'MACD Signal UP' => BBWidth valid, NO trigger";
                                        }
                                    }
                                    else // not: MACD Signal UP
                                    {
                                        if ((_stochastics[0].KValue < Settings.StochK)
                                            && (_stochastics[0].DValue < Settings.StochD)
                                            && (_stochastics[1].DValue < _stochastics[0].DValue)
                                            && (_stochastics[2].DValue < _stochastics[1].DValue)
                                            //&& (_stochastics[3].DValue < _stochastics[2].DValue)
                                            //&& (Candle.ClosePrice < _bollingerbands[0].Upper)
                                            && (_bollingerbands[1].Middle < _bollingerbands[0].Middle)
                                            && (_bollingerbands[2].Middle < _bollingerbands[1].Middle)
                                            //&& (_bollingerbands[3].Middle < _bollingerbands[2].Middle2)
                                            //&& (_bollingerbands[4].Middle < _bollingerbands[3].Middle3)
                                            && Settings.AllowLong)
                                        {
                                            // open buy order when price closes above Middle bollinger bands
                                            // and stochastics K & D are both below 45
                                            // K3 smaller K2 and K2 smaller K
                                            // Decimal buyprice = ((_bollingerbands[0].Middle / 1000) * 1001);
                                            if (_bollingerbands[0].Bandwidth < 1.5m)
                                            {
                                                NewSignal.TakeProfit = 0.4m;
                                            }
                                            else
                                            {
                                                NewSignal.TakeProfit = Math.Round((((((_bollingerbands[0].Upper / 1000) * 998) - (_candle1st.ClosePrice)) / (_candle1st.ClosePrice)) * 100), 2);
                                            }
                                            NewSignal.StopLoss = Math.Round((((((_bollingerbands[0].Lower / 1000) * 999) - (_candle1st.ClosePrice)) / (_candle1st.ClosePrice)) * 100), 2);
                                            //StrategyResult.ClosePrice = _candle1st.ClosePrice;
                                            NewSignal.TradeType = SignalsTBL.TradeTypes.Long;
                                            NewSignal.Valid = true;
                                            NewSignal.SignalAction = SignalsTBL.SignalActions.Buy;
                                            NewSignal.Message = "Stoch,!MacdUp: Stoch[012],BB[12] => Buy (StopLoss,TakeProfit)";
                                        }
                                        else
                                        {
                                            Message = "Stoch + not-'MACD Signal UP' => BBWidth valid, NO trigger";
                                        }
                                    }
                                }
                                else
                                {
                                    Message = "Stoch => BBWidth not met => NO trigger";
                                }
                            }

                            if (Settings.ChStochRSI)
                            {
                                if (Settings.ChMACDUPMU)
                                {
                                    //if ((candle.ClosePrice < _bollingerbands[0].Upper))
                                    if ((_stochasticRSIs[0].KValue < (decimal)Settings.StochRSIK)
                                        && (_stochasticRSIs[0].DValue < (decimal)Settings.StochRSID)
                                        && (_stochasticRSIs[1].DValue < _stochasticRSIs[0].DValue)
                                        && (_stochasticRSIs[2].DValue < _stochasticRSIs[1].DValue)
                                        //&& (_stochasticRSIs[3].DValue < _stochasticRSIs[2].DValue)
                                        && (_bollingerbands[1].Middle < _bollingerbands[0].Middle)
                                        && (_bollingerbands[2].Middle < _bollingerbands[1].Middle)
                                        //&& (_bollingerbands[3].Middle < _bollingerbands[2].Middle)
                                        //&& (_bollingerbands[4].Middle < _bollingerbands[3].Middle)
                                        && (_macds[1].MacdValue < _macds[0].MacdValue)
                                        && (_macds[2].MacdValue < _macds[1].MacdValue)
                                        && Settings.AllowLong)
                                    {
                                        // open buy order when price closes above Middle bollinger bands
                                        // and stochastics K & D are both below 45
                                        // K3 smaller K2 and K2 smaller K
                                        // Decimal buyprice = ((_bollingerbands[0].Middle / 1000) * 1001);
                                        if (_bollingerbands[0].Bandwidth < 1.5m)
                                        {
                                            NewSignal.TakeProfit = 0.4m;
                                        }
                                        else
                                        {
                                            NewSignal.TakeProfit = Math.Round((((((_bollingerbands[0].Upper / 1000) * 998) - (_candle1st.ClosePrice)) / (_candle1st.ClosePrice)) * 100), 2);
                                        }
                                        NewSignal.StopLoss = Math.Round((((((_bollingerbands[0].Lower / 1000) * 999) - (_candle1st.ClosePrice)) / (_candle1st.ClosePrice)) * 100), 2);
                                        NewSignal.TradeType = SignalsTBL.TradeTypes.Long;
                                        NewSignal.Valid = true;
                                        NewSignal.SignalAction = SignalsTBL.SignalActions.Buy;
                                        NewSignal.Message = "StochRSI,MacdUp: StochRSI[012],BB[12]Macd[12] => Buy (StopLoss,TakeProfit)";
                                    }
                                    else
                                    {
                                        Message += (String.IsNullOrEmpty(Message) ? "" : "; ") + "StochRSI + 'MACD Signal UP' => NO trigger";
                                    }
                                }
                                else
                                {
                                    //if ((candle.ClosePrice < _bollingerbands[0].Upper))
                                    if ((_stochasticRSIs[0].KValue < (decimal)Settings.StochRSIK)
                                        && (_stochasticRSIs[0].DValue < (decimal)Settings.StochRSID)
                                        && (_stochasticRSIs[1].DValue < _stochasticRSIs[0].DValue)
                                        && (_stochasticRSIs[2].DValue < _stochasticRSIs[1].DValue)
                                        //&& (_stochasticRSIs[3].DValue < _stochasticRSIs[2].DValue)
                                        && (_bollingerbands[1].Middle < _bollingerbands[0].Middle)
                                        && (_bollingerbands[2].Middle < _bollingerbands[1].Middle)
                                        //&& (_bollingerbands[3].Middle < _bollingerbands[2].Middle2)
                                        //&& (_bollingerbands[4].Middle < _bollingerbands[3].Middle3)
                                        && Settings.AllowLong)
                                    {
                                        // open buy order when price closes above Middle bollinger bands
                                        // and stochastics K & D are both below 45
                                        // K3 smaller K2 and K2 smaller K
                                        // Decimal buyprice = ((_bollingerbands[0].Middle / 1000) * 1001);
                                        if (_bollingerbands[0].Bandwidth < 1.5m)
                                        {
                                            NewSignal.TakeProfit = 0.4m;
                                        }
                                        else
                                        {
                                            NewSignal.TakeProfit = Math.Round((((((_bollingerbands[0].Upper / 1000) * 998) - (_candle1st.ClosePrice)) / (_candle1st.ClosePrice)) * 100), 2);
                                        }
                                        NewSignal.StopLoss = Math.Round((((((_bollingerbands[0].Lower / 1000) * 999) - (_candle1st.ClosePrice)) / (_candle1st.ClosePrice)) * 100), 2);
                                        NewSignal.TradeType = SignalsTBL.TradeTypes.Long;
                                        NewSignal.Valid = true;
                                        NewSignal.SignalAction = SignalsTBL.SignalActions.Buy;
                                        NewSignal.Message = "StochRSI,!MacdUp: StochRSI[012],BB[12] => Buy (StopLoss,TakeProfit)";
                                    }
                                    else
                                    {
                                        Message += (String.IsNullOrEmpty(Message) ? "" : "; ") + "StochRSI + not-'MACD Signal UP' => NO trigger";
                                    }
                                }
                            }

                            if (!Settings.ChStoch && !Settings.ChStochRSI)
                            {
                                NewSignal.Message = "MiddleUP: Stochoastic + StochRSI both disabled => NO checks!";
                            }
                            else
                            {
                                NewSignal.Message = "MiddleUP: " + Message;
                            }
                        }

                        else if (Settings.ChBottumUp)
                        {
                            NewSignal.Strategy = SignalsTBL.Strategies.BottomUp;

                            if ((_candle1st.ClosePrice > Settings.LowSatBTC)
                                && (_bollingerbands[0].Bandwidth > (decimal)Settings.BUMinBollingerBandWidth)
                                && (_bollingerbands[0].Bandwidth < (decimal)Settings.BUMaxBollingerBandWidth))
                            {
                                if ((_candle1st.ClosePrice < _bollingerbands[0].Lower)
                                    && (_stochastics[0].KValue < 20 && _stochastics[0].DValue < 20)
                                    && (_mfis[0] < Settings.BUMFI)
                                    && (_rsi[0] /*_stochasticRSIs[0].RSIValue*/ < (decimal)Settings.BURSI)
                                    && Settings.AllowLong)
                                {
                                    // open buy order when price closes below lower bollinger bands
                                    // and stochastics K & D are both below 20
                                    // Decimal takeProfit = (_bollingerbands[0].Bandwidth / 2);
                                    if (_bollingerbands[0].Bandwidth > 1.8m)
                                    {
                                        NewSignal.TakeProfit = 0.9m;
                                    }
                                    if (_bollingerbands[0].Bandwidth < 0.9m)
                                    {
                                        NewSignal.TakeProfit = 0.4m;
                                    }
                                    else
                                    {
                                        NewSignal.TakeProfit = Math.Round(((_bollingerbands[0].Bandwidth) / 2), 2);
                                    }
                                    NewSignal.StopLoss = Settings.BUStopLoss; // stopLoss; //.ToString(nfi);
                                    Decimal MaxPosSize = ((Settings.TradingBudget) * 0.02m); // 30% van TradingBudget daarvan instap 15% 1e instap (15,20,25,40) = 0,045 // MAx35 opentraden met 70% budget. = (1/35)/100*70=0,02
                                    Decimal PosSize = ((MarketPair.CalculatedMaxBuyAmount) * 0.1m); // MaxBuyAmount / 10 is Instap
                                    if (PosSize > MaxPosSize)
                                    {
                                        NewSignal.PositionSize = MaxPosSize;
                                    }
                                    else
                                    {
                                        NewSignal.PositionSize = PosSize;
                                    }
                                    NewSignal.TradeType = SignalsTBL.TradeTypes.Long;
                                    NewSignal.Valid = true;
                                    NewSignal.SignalAction = SignalsTBL.SignalActions.Buy;
                                    NewSignal.Message = "BottomUp: BBWidth,BBLower,Stoch,MFI,RSI => Buy (StopLoss,TakeProfit,PosSize,MaxPos)";
                                }
                                else
                                {
                                    NewSignal.Message = $"BottumUp: ClosePrice, BBWidth OK, not met: CP={_candle1st.ClosePrice} < BBLow={_bollingerbands[0].Lower}; K={Math.Round((_stochastics[0].KValue),2)}, D={Math.Round((_stochastics[0].DValue),2)} < 20; MFI={Math.Round((_mfis[0]),2)} < {Settings.BUMFI}; RSI={Math.Round((_rsi[0]),2) /*_stochasticRSIs[0].RSIValue*/} < {Settings.BURSI}; start={_candle1st.Timestamp.ToString("ddMMyy.HHmmss")}, open={_candle1st.OpenPrice} => NO trigger";
                                }
                            }
                            else
                            {
                                NewSignal.Message = $"BottomUp: not met: ClosePrice={_candle1st.ClosePrice} > {Settings.LowSatBTC}; BBWidth={Math.Round((_bollingerbands[0].Bandwidth),2)} > {Settings.BUMinBollingerBandWidth}, < {Settings.BUMaxBollingerBandWidth} => No trigger";
                            }
                        }

                        else if (Settings.ChRSIFamily)
                        {
                            NewSignal.Strategy = SignalsTBL.Strategies.Family;

                            if ((_candle1st.ClosePrice > Settings.LowSatBTC))
                            //&& (_bollingerbands[0].Bandwidth > (decimal)Settings.MinBollingerBandWidth)
                            //&& (_bollingerbands[0].Bandwidth < (decimal)Settings.MaxBollingerBandWidth))
                            {
                                if ((_mfis[0] < Settings.RsiMFI)
                                    && (_stochasticRSIs[0].KValue < (decimal)Settings.RsiStochRSI)
                                    && (_stochasticRSIs[0].DValue < (decimal)Settings.RsiStochRSI)
                                    && (_rsi[0] /*_stochasticRSIs[0].RSIValue*/ < (decimal)Settings.RsiRSI)
                                    //&& (_stochasticRSIs[0].KValue > _stochasticRSIs[0].DValue)
                                    && Settings.AllowLong)
                                {
                                    /*if (_bollingerbands[0].Bandwidth > 1.8m)
                                    {
                                        NewSignal.TakeProfit = 0.9m;
                                    }
                                    if (_bollingerbands[0].Bandwidth < 0.8m)
                                    {
                                        NewSignal.TakeProfit = 0.4m;
                                    }
                                    else
                                    {
                                        NewSignal.TakeProfit = Math.Round(((_bollingerbands[0].Bandwidth) / 2), 2);
                                    }
                                    //StrategyResult.StopLoss = ((_bollingerbands[0].Lower) * 0.999m);
                                    Decimal MaxPosSize = (2.10m / 35); // max 35 tegelijktijdige open trades 
                                    Decimal PosSize = ((MarketPair.CalculatedMaxBuyAmount) * 0.5m); // MaxBuyAmount 50% is Instap
                                    if (PosSize > MaxPosSize)
                                    {
                                        NewSignal.PositionSize = ((MaxPosSize));
                                    }
                                    else
                                    {
                                        NewSignal.PositionSize = PosSize;
                                    }*/
                                    NewSignal.TradeType = SignalsTBL.TradeTypes.Long;
                                    NewSignal.Valid = true;
                                    NewSignal.SignalAction = SignalsTBL.SignalActions.Buy;
                                    NewSignal.Message = "RSIFamily: MFI,StochRSI,RSI => Buy (MaxPos,PosSize)";
                                }
                                else
                                {
                                    NewSignal.Message = $"Family: CandleClose ok; not met: MFI={_mfis[0]} < {Settings.RsiMFI}; K={_stochasticRSIs[0].KValue}, D={_stochasticRSIs[0].DValue} < {Settings.RsiStochRSI}; RSI={_rsi[0] /*_stochasticRSIs[0].RSIValue*/} < {Settings.RsiRSI} => NO trigger";
                                }
                            }
                            else
                            {
                                NewSignal.Message = $"Family: not met: CandleClose={_candle1st.ClosePrice} > {Settings.LowSatBTC} => NO trigger";
                            }
                        }
                        else
                        {
                            NewSignal.Message = "None strategy is enabled: nothing to check!";
                        }
                    }
                    else
                    {
                        NewSignal.Message = Message;
                        //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {StrategyResult.Message}");
                    }
                }
                else
                {
                    NewSignal.Message = $"ERROR need 20, but got {Candles.Count} candles!";
                    //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {StrategyResult.Message}");
                }
            }
            catch (Exception E)
            {
                NewSignal.Message = $"ERROR CheckMarketpairFor1stBuySell: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
                //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {StrategyResult.Message}");
            }
        }


        public static void CheckSignalAfterFilled(List<MarketCandleCMD> Candles, ref SignalsTBL NewSignal)
        {
            try
            {
                Settings Settings = SettingsStore.Load();

                string Message = _CalculateIndicators(Candles, Settings.MaxFlatCandleCount);

                if (string.IsNullOrEmpty(Message))
                {
                    // Always updated
                    //NewSignal.BuyPrice = _candle1st.ClosePrice;
                    NewSignal.ClosePrice = _candle1st.ClosePrice;
                    NewSignal.BBWidth = Math.Round(_bollingerbands[0].Bandwidth, 2);
                    NewSignal.BBLower = Math.Round(_bollingerbands[0].Lower, 2);
                    NewSignal.BBMiddle = Math.Round(_bollingerbands[0].Middle, 2);
                    NewSignal.BBUpper = Math.Round(_bollingerbands[0].Upper, 2);
                    NewSignal.RSI = Math.Round(_rsi[0], 0);
                    NewSignal.RSIK = Math.Round(_stochasticRSIs[0].KValue, 0);
                    NewSignal.RSID = Math.Round(_stochasticRSIs[0].DValue, 0);
                    NewSignal.MFI = Math.Round(_mfis[0], 0);
                    NewSignal.StochK = Math.Round(_stochastics[0].KValue, 0);
                    NewSignal.StochD = Math.Round(_stochastics[0].DValue, 0);
                    NewSignal.MACD = Math.Round(_macds[0].MacdValue, 6);
                    NewSignal.MACDSignal = Math.Round(_macds[0].MacdSignal, 6);
                    NewSignal.MACDHistogram = Math.Round(_macds[0].MacdHistogram, 6);

                    switch (NewSignal.Strategy)
                    {
                        case SignalsTBL.Strategies.BottomUp:
                            // BuyCount=(1stBuy + Rebuys) == (maxRebuys+1) reached? else continue with other conditions..
                            if ((NewSignal.BuyCount == 3 /*(Settings.MaxRebuysBU + 1)*/ ) // cp > 3% BP
                                && (_candle1st.ClosePrice >= (NewSignal.BuyPrice - ((NewSignal.BuyPrice / 100) - (Settings.BUStopLoss2 + Settings.BUStopLossLast))))) // 3.00%
                            {
                                NewSignal.SignalAction = SignalsTBL.SignalActions.Sell;
                                NewSignal.Valid = true;
                                NewSignal.Message = "Sell detected (BuyCount=3): Action->Sell";
                                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: BuyCount=3 CP={_candle1st.ClosePrice} BP={NewSignal.BuyPrice} SL={Settings.BUStopLoss} => {NewSignal.Message}");
                            }
                            else if (!Settings.McCheckActive || (Settings.McCheckActive && ((NewSignal.Bm1Hr >= Settings.MinMC4Signal) || (NewSignal.Bm4Hr >= Settings.MinMC4Hr4Signal))))
                            {
                                int AcceptMinutes = (Settings.MaxCandles1stBuy * NewSignal.TimeframeMinutes) + 1;
                                if ((AcceptMinutes == 0) || (AcceptMinutes > 0 && NewSignal.StartDateTime.AddMinutes(AcceptMinutes) < DateTime.Now))
                                {
                                    string Msg = $"BuyCount={NewSignal.BuyCount} Close={_candle1st.ClosePrice} BuyP={NewSignal.BuyPrice} SL={Settings.BUStopLoss} SL1={Settings.BUStopLoss1}, SL2={Settings.BUStopLoss2}, SLLast={Settings.BUStopLossLast} => ";
                                    switch (NewSignal.BuyCount)
                                    {
                                        case 1:
                                            Msg += $"{_candle1st.ClosePrice} > {(NewSignal.BuyPrice + ((NewSignal.BuyPrice / 100) * (NewSignal.TakeProfit + 0.2m)))} => {((_candle1st.ClosePrice > (NewSignal.BuyPrice + ((NewSignal.BuyPrice / 100) * (NewSignal.TakeProfit + 0.2m)))) ? "Sold/CompletedProfit (No Zignaly action)" : "False")}"
                                                + $" -OF- "
                                                + $"{_candle1st.ClosePrice} < {(NewSignal.BuyPrice * (1 - Settings.BUStopLoss1 / 100))} && > {(NewSignal.BuyPrice * (1 - Settings.BUStopLoss / 100))}"
                                                + $" => {(((_candle1st.ClosePrice < (NewSignal.BuyPrice * (1 - Settings.BUStopLoss1 / 100))) && (_candle1st.ClosePrice > (NewSignal.BuyPrice * (1 - Settings.BUStopLoss / 100)))) ? "Rebuy" : "False")}"
                                                ;
                                            break;
                                        case 2:
                                            Msg += $"{_candle1st.ClosePrice} < {(NewSignal.BuyPrice * (1 - Settings.BUStopLoss2 / 100))} && > {(NewSignal.BuyPrice * (1 - Settings.BUStopLoss / 100))}"
                                                + $" => {(((_candle1st.ClosePrice < (NewSignal.BuyPrice * (1 - Settings.BUStopLoss2 / 100))) && (_candle1st.ClosePrice > (NewSignal.BuyPrice * (1 - Settings.BUStopLoss / 100)))) ? "Rebuy + StopLoss" : "False")}";
                                            break;
                                        /*case 3:
                                            Msg += $"{_candle1st.ClosePrice} > {(NewSignal.BuyPrice * (1 - (Settings.BUStopLoss + Settings.BUStopLoss1 + Settings.BUStopLoss2) / 100))}"
                                                + $" => {((_candle1st.ClosePrice >= (NewSignal.BuyPrice * (1 - (Settings.BUStopLoss + Settings.BUStopLoss1 + Settings.BUStopLoss2) / 100))) ? "Sell" : "False")}";
                                            break;*/
                                        default:
                                            Msg += "Not detected";
                                            break;
                                    }
                                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: FBId={NewSignal.FirstBuyId} {NewSignal.Symbol} {NewSignal.TimeframeName} BottomUp detected with " + Msg);

                                    if ((NewSignal.BuyCount == 1) // only 1stBuy bought
                                        && (_candle1st.ClosePrice > (NewSignal.BuyPrice + ((NewSignal.BuyPrice / 100) * (NewSignal.TakeProfit + 0.2m)))))
                                    {
                                        NewSignal.SignalAction = SignalsTBL.SignalActions.Sold; // In main -> mothersignal also set to Sold and same state (if changed here, change there too!)
                                        NewSignal.SignalState = SignalsTBL.SignalStates.CompletedProfit; // so no Zignaly action
                                        NewSignal.Valid = true;
                                        NewSignal.Message = "TakeProfit Filled (BuyCount=1): State->CompletedProfit Action->Sold";
                                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: BM1Hr={Math.Round((NewSignal.Bm1Hr),2)} < {Settings.PSMC1hr} and BM4Hr={Math.Round((NewSignal.Bm4Hr),2)} < {Settings.PSMC4hr} => {NewSignal.Message}");
                                    }

                                    // max rebuys reached, thus (BuyCount=(1stBuy + Rebuys) > maxrebuys then stop with this condition else next conditions..
                                    //else if (NewSignal.BuyCount > Settings.MaxRebuysBU)
                                    //{
                                    //    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: BuyCount={NewSignal.BuyCount}, maxReBuys={Settings.MaxRebuysBU} => no more rebuys!");
                                    //}

                                    // Take profit Mimimal 0,4% Maximaal 0,9%% / MiddelsteBB
                                    else if ((NewSignal.BuyCount == 1)  // 3% < cp < 1,25%
                                        && (_candle1st.ClosePrice < (NewSignal.BuyPrice * (1 - Settings.BUStopLoss1 / 100))) // 1.25%
                                        && (_candle1st.ClosePrice > (NewSignal.BuyPrice * (1 - Settings.BUStopLoss / 100)))) // 3%
                                    {
                                        NewSignal.SignalAction = SignalsTBL.SignalActions.Rebuy;
                                        NewSignal.BuyPrice = _candle1st.ClosePrice;
                                        NewSignal.BuyCount++;
                                        NewSignal.Valid = true;
                                        NewSignal.Message = "Rebuy1 detected: BuyCount=1->2, BuyPrice=ClosePrice, Action->Rebuy";
                                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: BuyCount=1 CP={_candle1st.ClosePrice} SL2={Settings.BUStopLoss2} SL={Settings.BUStopLoss} => {NewSignal.Message}");
                                    }
                                    else if ((NewSignal.BuyCount == 2) // 3% < cp < 1,65%
                                        && (_candle1st.ClosePrice < (NewSignal.BuyPrice * (1 - Settings.BUStopLoss2 / 100))) // 1.65%
                                        && (_candle1st.ClosePrice > (NewSignal.BuyPrice * (1 - Settings.BUStopLoss / 100)))) // 3%
                                    {
                                        NewSignal.SignalAction = SignalsTBL.SignalActions.RebuyStoploss;
                                        NewSignal.BuyPrice = _candle1st.ClosePrice;
                                        NewSignal.StopLoss = Settings.BUStopLossLast;
                                        NewSignal.BuyCount++;
                                        NewSignal.Valid = true;
                                        NewSignal.Message = "Rebuy + Stoploss detected: BuyCount=2->3, Buyprice=ClosePrice, Stoploss=StoplossLast Action->RebuyStoploss";
                                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: BuyCount=2 CP={_candle1st.ClosePrice} SL2={Settings.BUStopLoss2} SL={Settings.BUStopLoss} => {NewSignal.Message}");
                                    }
                                    /* 11jun20 moved outside the BM check
                                    else if ((NewSignal.BuyCount == 3) // cp > 3% BP
                                        && (_candle1st.ClosePrice >= (NewSignal.BuyPrice - ((NewSignal.BuyPrice / 100) - (Settings.BUStopLoss2 + Settings.BUStopLossLast))))) // 3.00%
                                    {
                                        NewSignal.SignalAction = SignalsTBL.SignalActions.Sell;
                                        NewSignal.Valid = true;
                                        NewSignal.Message = "Sell detected (BuyCount=3): Action->Sell";
                                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: BuyCount=3 CP={_candle1st.ClosePrice} BP={NewSignal.BuyPrice} SL={Settings.BUStopLoss} => {NewSignal.Message}");
                                    }*/
                                    else
                                    {
                                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: Not detected for BuyCount={NewSignal.BuyCount}: CP={_candle1st.ClosePrice} BP={NewSignal.BuyPrice} SL={Settings.BUStopLoss} SL1={Settings.BUStopLoss1} SL2={Settings.BUStopLoss2}");
                                    }
                                }
                                else
                                {
                                    // outside AcceptMinutes period.. already handled in main
                                }
                            }
                            else
                            {
                                if ((Settings.PanicSellActivated)
                                    && (NewSignal.Bm1Hr < Settings.PSMC1hr)
                                    && (NewSignal.Bm4Hr < Settings.PSMC4hr))
                                {
                                    // Niet hier: NewSignal.SignalState = SignalsTBL.SignalStates.CompletedLoss;
                                    NewSignal.SignalAction = SignalsTBL.SignalActions.Panicsell;
                                    NewSignal.Valid = true;
                                    NewSignal.Message = "PanicSell detected: Action->PanicSell";
                                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled BottomUP strategy: BM1Hr={Math.Round((NewSignal.Bm1Hr),2)} < {Settings.PSMC1hr} and BM4Hr={Math.Round((NewSignal.Bm4Hr),2)} < {Settings.PSMC4hr} => {NewSignal.Message}");
                                }
                            }
                            break;

                        case SignalsTBL.Strategies.MiddleUp:
                            // nothing / todo..
                            break;

                        case SignalsTBL.Strategies.Family:
                            if ((_mfis[0] > Settings.RsiMFISell)
                                && (_stochasticRSIs[0].KValue > (decimal)Settings.RsiStochRSISell)
                                && (_stochasticRSIs[0].DValue > (decimal)Settings.RsiStochRSISell)
                                && (_rsi[0] /*_stochasticRSIs[0].RSIValue*/ > (decimal)Settings.RsiRSISell)
                            //&& (_stochasticRSIs[0].KValue < _stochasticRSIs[0].DValue)
                            )
                            {
                                // Niet hier: NewSignal.SignalState = SignalsTBL.SignalStates.Completed;
                                NewSignal.SignalAction = SignalsTBL.SignalActions.Sell;
                                //NewSignal.BuyCount--;
                                NewSignal.Valid = true;
                                NewSignal.Message = "Sell detected => Action->Sell";
                                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled Family strategy => {NewSignal.Message}");
                            }
                            else
                            {
                                NewSignal.Message = "Family: Not Met";
                            }
                            break;
                    }
                }
                else
                {
                    NewSignal.Message = Message;
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled: {Message}");
                }
            }
            catch (Exception E)
            {
                NewSignal.Message = $"ERROR CheckBottumUp: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CheckSignalAfterFilled: {NewSignal.Message}");
            }
        }

    }
}