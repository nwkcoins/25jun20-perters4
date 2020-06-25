//using ExchangeSharp;
using CryptoTraderStandard.ScannerSQLiteDB;
using System;

namespace CryptoTraderScanner.Bot.Implementation
{
    public class DayTradingStrategy : IStrategy
    {
        /*private enum StrategyState
        {
            Scanning,
            Opened,
            Rebuy1,
            Rebuy2,
            Waiting
        }
        private const Decimal RebuyPercentage = 0.9825m; // 1.75%
        private const Decimal FirstRebuyFactor = 2m;     // 2x
        private const Decimal SecondRebuyFactor = 4m;    // 4x 
        private const Decimal ThirdRebuyFactor = 8m;     // 8x 
        private readonly int MaxTimesRebuy = 2;
        
        private ITrade _trade = null;
        private Decimal _bundleSize;
        //private StrategyState _state = StrategyState.Scanning;
        */
        //private readonly Settings _settings;

        /*public DayTradingStrategy(/*string symbol, Settings settings*//*)
        {
            // Symbol = symbol;
            //_settings = settings;
        }*/

        //public string Symbol { get; set; }
        public Decimal BolingerBandwidth { get; set; }
        public Decimal RSI { get; set; }
        public decimal MFI { get; set; }
        public Decimal ClosePrice { get; set; }
        public Decimal BuyPrice { get; set; }
        public Decimal TakeProfit { get; set; }
        public Decimal StopLoss { get; set; }
        public Decimal PositionSize { get; set; }
        public SignalsTBL.Strategies Strategy { get; set; }


        /// <summary>
        /// Perform strategy
        /// </summary>
        /// <param name="candles">candle history</param>
        /// <param name="bar">currentcandle</param>
        /// <param name="tradeManager">tradeManager</param>
        /// <param name="Message">message</param>
        /*public void OnCandle(List<MarketCandleCMD> candles, int bar, ITradeManager tradeManager, MarketPairsCMD MarketPair, out string Message)
        {
            Message = "";
            try
            {
                switch (_state)
                {
                    case StrategyState.Scanning:
                        ScanForEntry(candles, bar, tradeManager, MarketPair, out Message);
                        break;

                    case StrategyState.Opened:
                        CloseTradeIfPossible(candles, bar, tradeManager);
                        if (_trade == null) return;

                        if (MaxTimesRebuy >= 1)
                        {
                            if (CanRebuy(candles, bar))
                            {
                                MarketCandleCMD candle = candles[bar];
                                if (DoRebuy(candle, FirstRebuyFactor, tradeManager))
                                {

                                    _state = StrategyState.Rebuy1;
                                }
                            }
                        }
                        break;

                    case StrategyState.Rebuy1:
                        CloseTradeIfPossible(candles, bar, tradeManager);
                        if (_trade == null) return;

                        if (MaxTimesRebuy >= 2)
                        {
                            if (CanRebuy(candles, bar))
                            {
                                MarketCandleCMD candle = candles[bar];
                                if (DoRebuy(candle, SecondRebuyFactor, tradeManager))
                                {
                                    _state = StrategyState.Rebuy2;
                                }
                            }
                        }
                        break;

                    case StrategyState.Rebuy2:
                        CloseTradeIfPossible(candles, bar, tradeManager);
                        if (_trade == null) return;

                        if (MaxTimesRebuy >= 3)
                        {
                            if (CanRebuy(candles, bar))
                            {
                                MarketCandleCMD candle = candles[bar];
                                if (DoRebuy(candle, ThirdRebuyFactor, tradeManager))
                                {
                                    _state = StrategyState.Waiting;
                                }
                            }
                        }
                        break;

                    case StrategyState.Waiting:
                        CloseTradeIfPossible(candles, bar, tradeManager);
                        break;
                }
            }
            catch (Exception E)
            {
                Message = E.Message;
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR OnCandle: {E.Message}");
            }
        }*/

        /// <summary>
        /// Adds more to the current position by buying (or selling) more coins.
        /// </summary>
        /// <param name="tradeManager">tradeManager</param>
        /// <returns><c>true</c>, if rebuy was done, <c>false</c> otherwise.</returns>
        /*private bool DoRebuy(MarketCandleCMD candle, decimal factor, ITradeManager tradeManager)
        {
            Decimal investment = factor * _bundleSize;
            Decimal coins = investment / candle.ClosePrice;

            switch (_trade.TradeType)
            {
                case TradeType.Long:
                    return tradeManager.BuyMore(_trade, coins);

                case TradeType.Short:
                    return tradeManager.SellMore(_trade, coins);
            }
            return false;
        }*/


        /// <summary>
        /// Checks if a valid entry appeard
        /// </summary>
        /// <returns><c>true</c>, if valid entry was found, <c>false</c> otherwise.</returns>
        /// <param name="candles">History of candles</param>
        /// <param name="bar">The current candle</param>
        /// <param name="tradeType">returns trade type.</param>
        /// <param name="Message">message</param>
        /*public bool IsValidEntry_MOVED_(List<MarketCandleCMD> candles, int bar, MarketPairsCMD MarketPair, out SignalsTBL.TradeTypes tradeType, out string Message)
        {
            // beschikbaar (ook andere waarden van MarketPair
            //MarketPair.CalculatedMaxBuyAmount

            Message = "";
            tradeType = SignalsTBL.TradeTypes.Long;
            try
            {
                Settings Settings = SettingsStore.Load();

                if (candles.Count < bar + 20)
                {
                    return false;
                }

                MarketCandleCMD candle = candles[bar];
                //var bb = new BollingerBands(candles, bar);
                BollingerBandsIND bb = new BollingerBandsIND();
                bb = BollingerBands.Calculate(candles, bar);

                //var stocasticRSI1 = new CalculateTaStochRSI(candles);
                //var stocasticRSI = new StochRSI(candles);
                StochRSIIND stocasticRSI = new StochRSIIND();
                stocasticRSI = StochRSI.Calculate(candles);

                //var mfi = new MoneyFlow(candles, bar);
                decimal mfi = CalculateTaMFI.Calculate(candles); // new CalculateTaMFI(candles);

                //var stoch = new Stochastics(candles, 14);
                StochasticsIND stoch = new StochasticsIND();
                stoch = Stochastics.Calculate(candles, 14);

                //var macd = new CalculateTaMacd(candles);
                MacdIND macd = new MacdIND();
                macd = CalculateTaMacd.Calculate(candles);

                //var rsi = new RSI(candles, bar);
                //var ma20 = new CalculateTaMA20(candles, bar);
                DateTime enddate = new DateTime(2020, 5, 1);

                if (DateTime.Now < enddate)
                {
                    // more then 5 flat candles in last 15 candles ?
                    int flatCandles = 0;
                    for (int i = 0; i < Settings.MaxFlatCandleCount; ++i)
                    {
                        MarketCandleCMD candleStick = candles[bar + i];
                        if (candleStick.QuoteCurrencyVolume <= 0) flatCandles++;
                    }
                    if (flatCandles > Settings.MaxFlatCandles)
                    {
                        Message = $"flatcandles > {Settings.MaxFlatCandles}";
                        return false;
                    }
                    // check for panic
                    Double panic = (double)((candle.ClosePrice / candle.OpenPrice) * 100m);
                    Double LowValue = (100 - Settings.MaxPanic);
                    Double HighValue = (100 + Settings.MaxPanic);
                    if ((panic < LowValue) || (panic > HighValue))
                    {
                        // more then 5% panic
                        Message = $"Panic: {panic} < {LowValue} OR > {HighValue}";
                        return false;
                    }

                    if (Settings.ChMiddleUP)
                    {
                        // is bolling bands width check
                        if ((candle.ClosePrice > Settings.LowSatBTC)
                            && (candle.ClosePrice > Settings.LowSatBTC)
                            && (candle.ClosePrice > bb.Middle)
                            && (candle.ClosePrice < bb.MiddleUp)
                            && (bb.Bandwidth > Settings.MinBollingerBandWidth)
                            && (bb.Bandwidth < Settings.MaxBollingerBandWidth))
                        {
                            if ((Settings.ChStoch) && (!Settings.ChMACDUPMU))
                            {
                                //if ((candle.ClosePrice < bb.Upper))
                                if ((stoch.K < Settings.StochK)
                                    && (stoch.D < Settings.StochD)
                                    && (stoch.D2 < stoch.D)
                                    && (stoch.D3 < stoch.D2)
                                    && (stoch.D4 < stoch.D3)
                                    && (bb.Middle1 < bb.Middle)
                                    && (bb.Middle2 < bb.Middle1)
                                    && (bb.Middle3 < bb.Middle2)
                                    && (bb.Middle4 < bb.Middle3)
                                    && Settings.AllowLong)
                                {
                                    // open buy order when price closes above Middle bollinger bands
                                    // and stochastics K & D are both below 45
                                    // K3 smaller K2 and K2 smaller K
                                    //NumberFormatInfo nfi = new NumberFormatInfo();
                                    //nfi.NumberDecimalSeparator = ".";
                                    BolingerBandwidth = bb.Bandwidth;
                                    //					var buyprice = ((bb.Middle / 1000) * 1001);
                                    BuyPrice = candle.ClosePrice; //.ToString(nfi);
                                    //					var limitprice = ((bb.Middle / 10000) * 10005);
                                    //					LimitPrice = limitprice.ToString(nfi);
                                    //Decimal takeProfit = ((bb.Upper) * 0.998m);
                                    Decimal TakeProfit = ((bb.Upper) * 0.998m);
                                    //Decimal stopLoss = (bb.Lower * 0.999m);
                                    Decimal StopLoss = (bb.Lower * 0.999m);
                                    RSI = stocasticRSI.RSIValue;
                                    MFI = mfi;
                                    ClosePrice = candle.ClosePrice;
                                    tradeType = SignalsTBL.TradeTypes.Long;
                                    Strategy = SignalsTBL.Strategies.MiddleUp; //  "Middle-UP";
                                    return true;
                                }
                                //Message = "No Middle-UP (Stochastic1) trigger";
                            }
                            if ((Settings.ChStoch) && (Settings.ChMACDUPMU))
                            {
                                //if ((candle.ClosePrice < bb.Upper))
                                if ((stoch.K < Settings.StochK)
                                    && (stoch.D < Settings.StochD)
                                    && (stoch.D2 < stoch.D)
                                    && (stoch.D3 < stoch.D2)
                                    && (stoch.D4 < stoch.D3)
                                    && (bb.Middle1 < bb.Middle)
                                    && (bb.Middle2 < bb.Middle1)
                                    && (bb.Middle3 < bb.Middle2)
                                    && (bb.Middle4 < bb.Middle3)
                                    && (macd.macdsig1 < macd.macdsig)
                                    && (macd.macdsig2 < macd.macdsig1)
                                    && Settings.AllowLong)
                                {
                                    // open buy order when price closes above Middle bollinger bands
                                    // and stochastics K & D are both below 45
                                    // K3 smaller K2 and K2 smaller K
                                    //NumberFormatInfo nfi = new NumberFormatInfo();
                                    //nfi.NumberDecimalSeparator = ".";
                                    BolingerBandwidth = bb.Bandwidth;
                                    //					var buyprice = ((bb.Middle / 1000) * 1001);
                                    BuyPrice = candle.ClosePrice; //.ToString(nfi);
                                                                //					var limitprice = ((bb.Middle / 10000) * 10005);
                                                                //					LimitPrice = limitprice.ToString(nfi);
                                    Decimal takeProfit = ((bb.Upper / 1000) * 998);
                                    TakeProfit = takeProfit; //.ToString(nfi);
                                    Decimal stopLoss = (bb.Lower / 1000) * 999;
                                    StopLoss = stopLoss; //.ToString(nfi);
                                    RSI = stocasticRSI.RSIValue;
                                    MFI = mfi;
                                    ClosePrice = candle.ClosePrice;
                                    tradeType = SignalsTBL.TradeTypes.Long;
                                    Strategy = SignalsTBL.Strategies.MiddleUp; // "Middle-UP";
                                    return true;
                                }

                                else if ((candle.ClosePrice > bb.Upper)
                                    && (stoch.K > 80)
                                    && (stoch.D > 80)
                                    && Settings.AllowShort)
                                {
                                    // open sell order when price closes above upper bollinger bands
                                    // and stochastics K & D are both above 80
                                    tradeType = SignalsTBL.TradeTypes.Short;
                                    return true;
                                }
                                Message = "No Middle-UP (Stochastic2) trigger";
                            }
                            //Message = "No Middle-UP (Stochastic2) trigger";
                        }

                        if ((Settings.ChStochRSI) && (!Settings.ChMACDUPMU))
                        {
                            //if ((candle.ClosePrice < bb.Upper))
                            if ((stocasticRSI.KValue < (decimal)Settings.StochRSIK)
                                && (stocasticRSI.DValue < (decimal)Settings.StochRSID)
                                && (stocasticRSI.DValue3 < stocasticRSI.DValue2)
                                && (stocasticRSI.DValue2 < stocasticRSI.DValue1)
                                && (stocasticRSI.DValue1 < stocasticRSI.DValue)
                                && (bb.Middle1 < bb.Middle)
                                && (bb.Middle2 < bb.Middle1)
                                && (bb.Middle3 < bb.Middle2)
                                && (bb.Middle4 < bb.Middle3)
                                && Settings.AllowLong)
                            {
                                // open buy order when price closes above Middle bollinger bands
                                // and stochastics K & D are both below 45
                                // K3 smaller K2 and K2 smaller K
                                //NumberFormatInfo nfi = new NumberFormatInfo();
                                //nfi.NumberDecimalSeparator = ".";
                                BolingerBandwidth = bb.Bandwidth;
                                //					var buyprice = ((bb.Middle / 1000) * 1001);
                                BuyPrice = candle.ClosePrice; //.ToString(nfi);
                                //					var limitprice = ((bb.Middle / 10000) * 10005);
                                //					LimitPrice = limitprice.ToString(nfi);
                                Decimal takeProfit = ((bb.Upper / 1000) * 998);
                                TakeProfit = takeProfit; //.ToString(nfi);
                                var stopLoss = ((bb.Lower / 1000) * 999);
                                StopLoss = stopLoss; //.ToString(nfi);
                                RSI = stocasticRSI.RSIValue;
                                MFI = mfi;
                                ClosePrice = candle.ClosePrice;
                                tradeType = SignalsTBL.TradeTypes.Long;
                                Strategy = SignalsTBL.Strategies.MiddleUp; // "Middle-UP";
                                return true;
                            }
                            Message = "No Middle-UP (Stochastic) trigger";
                        }
                        if ((Settings.ChStochRSI) && (Settings.ChMACDUPMU))
                        {
                            //if ((candle.ClosePrice < bb.Upper))
                            if ((stocasticRSI.KValue < (decimal)Settings.StochRSIK)
                                && (stocasticRSI.DValue < (decimal)Settings.StochRSID)
                                && (stocasticRSI.DValue3 < stocasticRSI.DValue2)
                                && (stocasticRSI.DValue2 < stocasticRSI.DValue1)
                                && (stocasticRSI.DValue1 < stocasticRSI.DValue)
                                && (bb.Middle1 < bb.Middle)
                                && (bb.Middle2 < bb.Middle1)
                                && (bb.Middle3 < bb.Middle2)
                                && (bb.Middle4 < bb.Middle3)
                                && (macd.macdsig1 < macd.macdsig)
                                && (macd.macdsig2 < macd.macdsig1)
                                && Settings.AllowLong)
                            {
                                // open buy order when price closes above Middle bollinger bands
                                // and stochastics K & D are both below 45
                                // K3 smaller K2 and K2 smaller K
                                //NumberFormatInfo nfi = new NumberFormatInfo();
                                //nfi.NumberDecimalSeparator = ".";
                                BolingerBandwidth = bb.Bandwidth;
                                //					var buyprice = ((bb.Middle / 1000) * 1001);
                                BuyPrice = candle.ClosePrice; //.ToString(nfi);
                                                            //					var limitprice = ((bb.Middle / 10000) * 10005);
                                                            //					LimitPrice = limitprice.ToString(nfi);
                                Decimal takeProfit = ((bb.Upper / 1000) * 998);
                                TakeProfit = takeProfit; //.ToString(nfi);
                                var stopLoss = ((bb.Lower / 1000) * 999);
                                StopLoss = stopLoss; //.ToString(nfi);
                                RSI = stocasticRSI.RSIValue;
                                MFI = mfi;
                                ClosePrice = candle.ClosePrice;
                                tradeType = SignalsTBL.TradeTypes.Long;
                                Strategy = SignalsTBL.Strategies.MiddleUp; // "Middle-UP";
                                return true;
                            }
                            Message = "No Middle-UP (Stochastic) trigger";
                        }

                        else if ((candle.ClosePrice > bb.Upper)
                        && (stoch.K > 80)
                        && (stoch.D > 80)
                        && (Settings.AllowShort))
                        {
                            // open sell order when price closes above upper bollinger bands
                            // and stochastics K & D are both above 80
                            tradeType = SignalsTBL.TradeTypes.Short;
                            return true;
                        }
                    }
                    else if (Settings.ChBottumUp)
                    {
                        if ((candle.ClosePrice > Settings.LowSatBTC)
                        && (bb.Bandwidth > (decimal)Settings.BUMinBollingerBandWidth)
                        && (bb.Bandwidth < (decimal)Settings.BUMaxBollingerBandWidth))
                        {
                            if ((candle.ClosePrice < bb.Lower)
                                && (stoch.K < 20 && stoch.D < 20)
                                && (mfi < Settings.BUMFI)
                                && (stocasticRSI.RSIValue < (decimal)Settings.BURSI)
                                && Settings.AllowLong)
                            {
                                // open buy order when price closes below lower bollinger bands
                                // and stochastics K & D are both below 20
                                //NumberFormatInfo nfi = new NumberFormatInfo();
                                //nfi.NumberDecimalSeparator = ".";
                                BolingerBandwidth = bb.Bandwidth;
                                BuyPrice = candle.ClosePrice; //.ToString(nfi);
                                //Decimal takeProfit = (bb.Bandwidth / 2);
                                if (bb.Bandwidth > 1.8m)
                                {
                                    TakeProfit = 0.9m;
                                }
                                if (bb.Bandwidth < 0.9m)
                                {
                                    TakeProfit = 0.4m;
                                }
                                else
                                {
                                    TakeProfit = Math.Round(((bb.Bandwidth) / 2), 2);
                                }
                                StopLoss = ((bb.Lower) * 0.999m); // stopLoss; //.ToString(nfi);
                                RSI = stocasticRSI.RSIValue;
                                MFI = mfi;
                                var MaxPosSize = (2.10m * 0.045m); // 30% van maximal budget (2,10) in 15% 1e instap (15,20,25,40) instap 
                                var PosSize = ((MarketPair.CalculatedMaxBuyAmount) * 0.1m); // MaxBuyAmount / 10 is Instap
                                if (PosSize > MaxPosSize)
                                {
                                    PositionSize = MaxPosSize;
                                }
                                else
                                {
                                    PositionSize = PosSize;
                                }
                                ClosePrice = candle.ClosePrice;
                                tradeType = SignalsTBL.TradeTypes.Long;
                                Strategy = SignalsTBL.Strategies.BottomUp; // "Bottom-UP";
                                return true;
                            }
                            else if ((candle.ClosePrice > bb.Upper)
                                && (stoch.K > 80)
                                && (stoch.D > 80)
                                && (Settings.AllowShort))
                            {
                                // open sell order when price closes above upper bollinger bands
                                // and stochastics K & D are both above 80
                                tradeType = SignalsTBL.TradeTypes.Short;
                                return true;
                            }
                        }
                        Message = "No BottomUp trigger";
                    }
                    else if (Settings.ChRSIFamily)
                    {
                        if ((candle.ClosePrice > Settings.LowSatBTC))
                        //&& (bb.Bandwidth > (decimal)Settings.MinBollingerBandWidth)
                        //&& (bb.Bandwidth < (decimal)Settings.MaxBollingerBandWidth))
                        {
                            if ((mfi < Settings.RsiMFI)
                                && (stocasticRSI.KValue < (decimal)Settings.RsiStochRSI)
                                && (stocasticRSI.DValue < (decimal)Settings.RsiStochRSI)
                                && (stocasticRSI.RSIValue < (decimal)Settings.RsiRSI)
                                && Settings.AllowLong)
                            {
                                //NumberFormatInfo nfi = new NumberFormatInfo();
                                //nfi.NumberDecimalSeparator = ".";
                                BolingerBandwidth = bb.Bandwidth;
                                BuyPrice = candle.ClosePrice; //.ToString(nfi);
                                if (bb.Bandwidth > 1.8m)
                                {
                                    TakeProfit = 0.9m;
                                }
                                if (bb.Bandwidth < 0.8m)
                                {
                                    TakeProfit = 0.4m;
                                }
                                else
                                {
                                    TakeProfit = Math.Round(((bb.Bandwidth) / 2), 2);
                                }
                                StopLoss = ((bb.Lower) * 0.999m);
                                //StopLoss = stopLoss; //.ToString(nfi);
                                RSI = stocasticRSI.RSIValue;
                                MFI = mfi;
                                var MaxPosSize = (2.10m / 35); // max 35 tegelijktijdige open trades 
                                var PosSize = ((MarketPair.CalculatedMaxBuyAmount) * 0.5m); // MaxBuyAmount 50% is Instap
                                if (PosSize > MaxPosSize)
                                {
                                    PositionSize = ((MaxPosSize));
                                }
                                else
                                {
                                    PositionSize = PosSize;
                                }
                                ClosePrice = candle.ClosePrice;
                                Strategy = SignalsTBL.Strategies.Family; // "Family";
                                tradeType = SignalsTBL.TradeTypes.Long;
                                return true;
                            }
                            if ((mfi < Settings.RsiMFISell)
                                && (stocasticRSI.KValue < (decimal)Settings.RsiStochRSISell
                                && stocasticRSI.DValue < (decimal)Settings.RsiStochRSISell)
                                && (stocasticRSI.RSIValue < (decimal)Settings.RsiRSISell)
                                && Settings.AllowShort)
                            {
                                // open sell order when price closes above upper bollinger bands
                                // and stochastics K & D are both above 80
                                tradeType = SignalsTBL.TradeTypes.Short;
                                return true;
                            }
                        }
                        Message = "No Family trigger";
                    }
                }
         *//*
                if (Settings.ChMiddleUP) Strategy = "Middle-UP";
                else if (Settings.ChBottumUp) Strategy = "Bottom-UP";
                else if (Settings.ChRSIFamily) Strategy = "RSI Famlily";

                if
                   (
                       (
                           (Settings.ChBottumUp)
                           && (candle.ClosePrice > Settings.LowSatBTC)
                           && (bb.Bandwidth > (decimal)Settings.BUMinBollingerBandWidth)
                           && (bb.Bandwidth < (decimal)Settings.BUMaxBollingerBandWidth)
                           && (candle.ClosePrice < bb.Lower)
                           && (stoch.K < 20 && stoch.D < 20)
                           && (mfi < Settings.BUMFI)
                           && (stocasticRSI.RSIValue < (decimal)Settings.BURSI))
                       ||
                           ((Settings.ChMiddleUP) && (Settings.ChStoch) && (Settings.ChMACDUPMU))
                           && (candle.ClosePrice > Settings.LowSatBTC)
                           && (bb.Bandwidth > Settings.MinBollingerBandWidth)
                           && (bb.Bandwidth < Settings.MaxBollingerBandWidth)
                           && (candle.ClosePrice > bb.Middle)
                           && (candle.ClosePrice < bb.MiddleUp)
                           && (stoch.K < Settings.StochK)
                           && (stoch.D < Settings.StochD)
                           && (stoch.D2 < stoch.D)
                           && (stoch.D3 < stoch.D2)
                           && (stoch.D4 < stoch.D3)
                           && (bb.Middle1 < bb.Middle)
                           && (bb.Middle2 < bb.Middle1)
                           && (bb.Middle3 < bb.Middle2)
                           && (bb.Middle4 < bb.Middle3)
                           && (macd.macdsig1 < macd.macdsig)
                           && (macd.macdsig2 < macd.macdsig1)
                        ||
                            ((Settings.ChMiddleUP) && (Settings.ChStoch) && (!Settings.ChMACDUPMU))
                            && (candle.ClosePrice > Settings.LowSatBTC)
                            && (bb.Bandwidth > Settings.MinBollingerBandWidth)
                            && (bb.Bandwidth < Settings.MaxBollingerBandWidth)
                            && (candle.ClosePrice > bb.Middle)
                            && (candle.ClosePrice < bb.MiddleUp)
                            && (stoch.K < Settings.StochK)
                            && (stoch.D < Settings.StochD)
                            && (stoch.D2 < stoch.D)
                            && (stoch.D3 < stoch.D2)
                            && (stoch.D4 < stoch.D3)
                            && (bb.Middle1 < bb.Middle)
                            && (bb.Middle2 < bb.Middle1)
                            && (bb.Middle3 < bb.Middle2)
                            && (bb.Middle4 < bb.Middle3)
                        ||
                            ((Settings.ChMiddleUP) && (Settings.ChStochRSI) && (Settings.ChMACDUPMU))
                            && (candle.ClosePrice > Settings.LowSatBTC)
                            && (bb.Bandwidth > Settings.MinBollingerBandWidth)
                            && (bb.Bandwidth < Settings.MaxBollingerBandWidth)
                            && (candle.ClosePrice > bb.Middle)
                            && (candle.ClosePrice < bb.MiddleUp)
                            && (stocasticRSI.KValue < (decimal)Settings.StochRSIK)
                            && (stocasticRSI.DValue < (decimal)Settings.StochRSID)
                            && (stocasticRSI.DValue3 < stocasticRSI.DValue2)
                            && (stocasticRSI.DValue2 < stocasticRSI.DValue1)
                            && (bb.Middle1 < bb.Middle)
                            && (bb.Middle2 < bb.Middle1)
                            && (bb.Middle3 < bb.Middle2)
                            && (bb.Middle4 < bb.Middle3)
                            && (macd.macdsig1 < macd.macdsig)
                            && (macd.macdsig2 < macd.macdsig1)
                        ||
                            ((Settings.ChMiddleUP) && (Settings.ChStochRSI) && (!Settings.ChMACDUPMU))
                            && (candle.ClosePrice > Settings.LowSatBTC)
                            && (bb.Bandwidth > Settings.MinBollingerBandWidth)
                            && (bb.Bandwidth < Settings.MaxBollingerBandWidth)
                            && (candle.ClosePrice > bb.Middle && candle.ClosePrice < bb.MiddleUp)
                            && (stocasticRSI.KValue < (decimal)Settings.StochRSIK)
                            && (stocasticRSI.DValue < (decimal)Settings.StochRSID)
                            && (stocasticRSI.DValue3 < stocasticRSI.DValue2)
                            && (stocasticRSI.DValue2 < stocasticRSI.DValue1)
                            && (bb.Middle1 < bb.Middle)
                            && (bb.Middle2 < bb.Middle1)
                            && (bb.Middle3 < bb.Middle2)
                            && (bb.Middle4 < bb.Middle3)
                        ||
                            (Settings.ChRSIFamily)
                            && (candle.ClosePrice > Settings.LowSatBTC)
                            && (mfi < Settings.RsiMFI)
                            && (stocasticRSI.KValue < (decimal)Settings.RsiStochRSI)
                            && (stocasticRSI.DValue < (decimal)Settings.RsiStochRSI)
                            && (stocasticRSI.RSIValue < (decimal)Settings.RsiRSI))
                {
                    // open buy order when price closes above Middle bollinger bands
                    // and stochastics K & D are both below 45
                    // K3 smaller K2 and K2 smaller K
                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = ".";
                    BolingerBandwidth = bb.Bandwidth;
                    //					var buyprice = ((bb.Middle / 1000) * 1001);
                    BuyPrice = candle.ClosePrice.ToString(nfi);
                    //					var limitprice = ((bb.Middle / 10000) * 10005);
                    //					LimitPrice = limitprice.ToString(nfi);
                    var takeProfit = ((bb.Upper / 1000) * 998);
                    TakeProfit = takeProfit.ToString(nfi);
                    var stopLoss = ((bb.Lower / 1000) * 999);
                    StopLoss = stopLoss.ToString(nfi);
                    RSI = stocasticRSI.RSIValue;
                    MFI = mfi;
                    ClosePrice = candle.ClosePrice;
                    tradeType = TradeType.Long;
                    return true;
                }
                else
                {
                    if ((Settings.ChBottumUp)
                        && (candle.ClosePrice > Settings.LowSatBTC)
                        && (candle.ClosePrice > bb.Upper)
                        && (stoch.K > 80)
                        && (stoch.D > 80)
                        && (Settings.AllowShorts))
                    {
                        // open sell order when price closes above upper bollinger bands
                        // and stochastics K & D are both above 80
                        tradeType = TradeType.Short;
                        return true;
                    }
                    else if ((Settings.ChRSIFamily)
                        && (candle.ClosePrice > Settings.LowSatBTC)
                        && (mfi < Settings.RsiMFISell)
                        && (stocasticRSI.KValue < (decimal)Settings.RsiStochRSISell)
                        && (stocasticRSI.DValue < (decimal)Settings.RsiStochRSISell)
                        && (stocasticRSI.RSIValue < (decimal)Settings.RsiRSISell))
                    {
                        // open sell order when MFI Stoch RSI and RSI are Overbought
                        // and stochastics K & D are both above 80
                        tradeType = TradeType.Short;
                        return true;
                    }
                    else
                    {
                        Message = "No Valid Signal" + " StockRSI K: " + Math.Round((stocasticRSI.KValue), 2) + "  StochRSI D: " + Math.Round((stocasticRSI.DValue), 2) + "  RSI: " + Math.Round((stocasticRSI.RSIValue), 2) + "  MFI: " + Math.Round((mfi), 2);
                    }
                }*//*
                return false;
            }
            catch (Exception E)
            {
                Message = E.Message;
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ERROR IsValidEntry: {E.Message}");
                return false;
            }
        }*/

        /// <summary>
        /// Scans for possibility to open a new trade.
        /// </summary>
        /// <param name="candles">candle history</param>
        /// <param name="bar">currentcandle</param>
        /// <param name="tradeManager">tradeManager</param>
        /// <param name="Message">message</param>
        /*private void ScanForEntry(List<MarketCandleCMD> candles, int bar, ITradeManager tradeManager, MarketPairsCMD MarketPair, out string Message)
        {
            Message = "";
            SignalsTBL.TradeTypes tradeType;
            if (IsValidEntry(candles, bar, MarketPair, out tradeType, out Message))
            {
                if (bar < candles.Count)
                {
                    MarketCandleCMD candle = candles[bar];
                    Decimal totalRebuy = 1m;
                    if (MaxTimesRebuy >= 1) totalRebuy += FirstRebuyFactor;
                    if (MaxTimesRebuy >= 2) totalRebuy += SecondRebuyFactor;
                    if (MaxTimesRebuy >= 3) totalRebuy += ThirdRebuyFactor;
                    _bundleSize = tradeManager.AccountBalance / totalRebuy;
                    Decimal coins = _bundleSize / candle.ClosePrice;
                    switch (tradeType)
                    {
                        case SignalsTBL.TradeTypes.Long:
                            _trade = tradeManager.BuyMarket(Symbol, coins);
                            break;

                        case SignalsTBL.TradeTypes.Short:
                            _trade = tradeManager.SellMarket(Symbol, coins);
                            break;
                    }
                    if (_trade != null)
                    {
                        _state = StrategyState.Opened;
                    }
                }
                else
                {
                    Message = $"ERROR: ";
                }
            }
        }*/

        /// <summary>
        /// Returns if the conditions for a rebuy are met.
        /// </summary>
        /// <returns><c>true</c>, if rebuy is possible, <c>false</c> otherwise.</returns>
        /// <param name="candles">candle history</param>
        /// <param name="bar">currentcandle</param>
        /*private bool CanRebuy(List<MarketCandleCMD> candles, int bar)
        {
            //var bb = new BollingerBands();
            BollingerBandsIND bb = new BollingerBandsIND();
            bb = BollingerBands.Calculate(candles, bar);

            //var stoch = new Stochastics(candles, bar);
            StochasticsIND stoch = new StochasticsIND();
            stoch = Stochastics.Calculate(candles, 14);

            //var RSI = new RSI(candles, bar);
            MarketCandleCMD candle = candles[bar];

            // for long we do a rebuy when price closes under the lower bollinger bands and both stochastics are below 20
            // and price is 1.75% below the previous buy
            if (_trade.TradeType == TradeType.Long && candle.ClosePrice < bb.Lower && stoch.K < 20 && stoch.D < 20)
            {
                Decimal price = _trade.OpenPrice * RebuyPercentage;
                if (_trade.Rebuys.Count > 0) price = _trade.Rebuys.Last().Price * RebuyPercentage;
                return candle.ClosePrice < price;
            }

            // for short we do a rebuy when price closes above the upper bollinger bands and both stochastics are above 80
            // and price is 1.75% above the previous sell
            if (_trade.TradeType == TradeType.Short && candle.ClosePrice > bb.Upper && stoch.K > 80 && stoch.D > 80)
            {
                Decimal factor = 1m + (1m - RebuyPercentage);
                Decimal price = _trade.OpenPrice * factor;
                if (_trade.Rebuys.Count > 0) price = _trade.Rebuys.Last().Price * factor;
                return candle.ClosePrice > price;
            }
            return false;
        }*/

        /// <summary>
        /// Closes the trade if price crosses the upper/lower bollinger band.
        /// </summary>
        /// <param name="candles">candle history</param>
        /// <param name="bar">currentcandle</param>
        /// <param name="tradeManager">tradeManager</param>
        /*private void CloseTradeIfPossible(List<MarketCandleCMD> candles, int bar, ITradeManager tradeManager)
        {
            MarketCandleCMD candle = candles[bar];

            //var bb = new BollingerBands(candles, bar);
            BollingerBandsIND bb = new BollingerBandsIND();
            bb = BollingerBands.Calculate(candles, bar);

            // for long we close the trade if price  gets above the upper bollinger bands
            if (_trade.TradeType == TradeType.Long && candle.HighPrice > bb.Upper)
            {
                if (tradeManager.Close(_trade, candle.HighPrice))
                {
                    _state = StrategyState.Scanning;
                    _trade = null;
                    return;
                }
            }

            // for short we close the trade if price  gets below the lowe bollinger bands
            if (_trade.TradeType == TradeType.Short && candle.LowPrice < bb.Lower)
            {
                if (tradeManager.Close(_trade, candle.LowPrice))
                {
                    _state = StrategyState.Scanning;
                    _trade = null;
                    return;
                }
            }
        }*/
    }
}
