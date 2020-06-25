using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CryptoTraderScanner.Bot.Implementation
{
    public class DayTradingStrategy : IStrategy
    {
        private enum StrategyState
        {
            Scanning,
            Opened,
            Rebuy1,
            Rebuy2,
            Waiting
        }
        private const decimal RebuyPercentage = 0.9825m; // 1.75%
        private const decimal FirstRebuyFactor = 2m;     // 2x
        private const decimal SecondRebuyFactor = 4m;    // 4x 
        private const decimal ThirdRebuyFactor = 8m;     // 8x 
        private readonly int MaxTimesRebuy = 2;


        private ITrade _trade = null;
        private decimal _bundleSize;
        private StrategyState _state = StrategyState.Scanning;
        private readonly Settings _settings;

        public DayTradingStrategy(string symbol, Settings settings)
        {
            Symbol = symbol;
            _settings = settings;
        }

        public string Symbol { get; set; }
        public decimal Bbands { get; set; }
        public decimal RelativeSI { get; set; }
        public double MoneyFI { get; set; }
        public decimal ClPrice { get; set; }
        public string BPrice { get; set; }
        public string LPrice { get; set; }
        public string TakeP { get; set; }
        public string StLoss { get; set; }
        public string Strategy { get; set; }

        /// <summary>
        /// Perform strategy
        /// </summary>
        /// <param name="candles">candle history</param>
        /// <param name="bar">currentcandle</param>
        /// <param name="tradeManager">tradeManager</param>
        /// <param name="ErrorMessage">Errormessage</param>
        public void OnCandle(List<MarketCandle> candles, int bar, ITradeManager tradeManager, out string ErrorMessage)
        {
            ErrorMessage = "";
            switch (_state)
            {
                case StrategyState.Scanning:
                    ScanForEntry(candles, bar, tradeManager, out ErrorMessage);
                    break;

                case StrategyState.Opened:
                    CloseTradeIfPossible(candles, bar, tradeManager);
                    if (_trade == null) return;

                    if (MaxTimesRebuy >= 1)
                    {
                        if (CanRebuy(candles, bar))
                        {
                            var candle = candles[bar];
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
                            var candle = candles[bar];
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
                            var candle = candles[bar];
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

        /// <summary>
        /// Adds more to the current position by buying (or selling) more coins.
        /// </summary>
        /// <param name="tradeManager">tradeManager</param>
        /// <returns><c>true</c>, if rebuy was done, <c>false</c> otherwise.</returns>
        private bool DoRebuy(MarketCandle candle, decimal factor, ITradeManager tradeManager)
        {
            var investment = factor * _bundleSize;
            var coins = investment / candle.ClosePrice;

            switch (_trade.TradeType)
            {
                case TradeType.Long:
                    return tradeManager.BuyMore(_trade, coins);

                case TradeType.Short:
                    return tradeManager.SellMore(_trade, coins);
            }
            return false;
        }

        /// <summary>
        /// Checks if a valid entry appeard
        /// </summary>
        /// <returns><c>true</c>, if valid entry was found, <c>false</c> otherwise.</returns>
        /// <param name="candles">History of candles</param>
        /// <param name="bar">The current candle</param>
        /// <param name="tradeType">returns trade type.</param>
        /// <param name="ErrorMessage">Errormessage</param>
		public bool IsValidEntry(List<MarketCandle> candles, int bar, out TradeType tradeType, out string ErrorMessage)
		{
			ErrorMessage = "";
			tradeType = TradeType.Long;
			if (candles.Count < bar + 20)
			{
				return false;
			}
			var candle = candles[bar];
			var bbands = new BollingerBands(candles, bar);
			//var stocasticRSI = new StocasticRSI(candles);
			var stocasticRSI = new StochRSI(candles);
			var mfi = new MoneyFlow(candles, 12);
			var stoch = new Stochastics(candles, 14);
			//var rsi = new RSI(candles, 0);
			var macd = new CalculateTaMacd(candles);
			var enddate = new DateTime(2020, 5, 1);

			if (DateTime.Now < enddate)
			{
				// more then 5 flat candles in last 15 candles ?
				var flatCandles = 0;
				for (int i = 0; i < _settings.MaxFlatCandleCount; ++i)
				{
					var candleStick = candles[bar + i];
					if (candleStick.QuoteCurrencyVolume <= 0) flatCandles++;
				}
				if (flatCandles > _settings.MaxFlatCandles)
				{
					ErrorMessage = $"flatcandles > {_settings.MaxFlatCandles}";
					return false;
				}
				// check for panic
				double panic = (double)((candle.ClosePrice / candle.OpenPrice) * 100m);
				double LowValue = (100 - _settings.MaxPanic);
				double HighValue = (100 + _settings.MaxPanic);
				if ((panic < LowValue)
					|| (panic > HighValue))
				{
					// more then 5% panic
					ErrorMessage = $"Panic: {panic} < {LowValue} OR > {HighValue}";
					return false;
				}

				if (_settings.ChMiddleUP)
				{
					// is bolling bands width check
					if ((candle.ClosePrice > bbands.Middle && candle.ClosePrice < bbands.MiddleUp)
						&& (candle.ClosePrice > _settings.LowSatBTC)
						&& (bbands.Bandwidth > _settings.MinBollingerBandWidth)
						&& (bbands.Bandwidth < _settings.MaxBollingerBandWidth))
					{
						if (_settings.ChStoch)
						{
							//if ((candle.ClosePrice < bbands.Upper))
							if ((stoch.K < _settings.StochK)
								&& (stoch.D < _settings.StochD)
								&& (stoch.D2 < stoch.D)
								&& (stoch.D3 < stoch.D2)
								&& (stoch.D4 < stoch.D3)
								&& (bbands.Middle1 < bbands.Middle)
								&& (bbands.Middle2 < bbands.Middle1)
								&& (bbands.Middle3 < bbands.Middle2)
								&& (bbands.Middle4 < bbands.Middle3))
							{
								if (_settings.ChMACDUPMU)
								{
									if ((macd.macdsig2 < macd.macdsig1) && (macd.macdsig1 < macd.macdsig))
									{
										// open buy order when price closes above Middle bollinger bands
										// and stochastics K & D are both below 45
										// K3 smaller K2 and K2 smaller K
										NumberFormatInfo nfi = new NumberFormatInfo();
										nfi.NumberDecimalSeparator = ".";
										Bbands = bbands.Bandwidth;
										//					var bprice = ((bbands.Middle / 1000) * 1001);
										BPrice = candle.ClosePrice.ToString(nfi);
										//					var lprice = ((bbands.Middle / 10000) * 10005);
										//					LPrice = lprice.ToString(nfi);
										var takeP = ((bbands.Upper / 1000) * 998);
										TakeP = takeP.ToString(nfi);
										var stLoss = ((bbands.Lower / 1000) * 999);
										StLoss = stLoss.ToString(nfi);
										RelativeSI = stocasticRSI.RSIValue;
										MoneyFI = mfi.MFI;
										ClPrice = candle.ClosePrice;
										tradeType = TradeType.Long;
										Strategy = "Middle-UP";
										return true;
									}
								}
								else if (!_settings.ChMACDUPMU)
								{
									// open buy order when price closes above Middle bollinger bands
									// and stochastics K & D are both below 45
									// K3 smaller K2 and K2 smaller K
									NumberFormatInfo nfi = new NumberFormatInfo();
									nfi.NumberDecimalSeparator = ".";
									Bbands = bbands.Bandwidth;
									//					var bprice = ((bbands.Middle / 1000) * 1001);
									BPrice = candle.ClosePrice.ToString(nfi);
									//					var lprice = ((bbands.Middle / 10000) * 10005);
									//					LPrice = lprice.ToString(nfi);
									var takeP = ((bbands.Upper / 1000) * 998);
									TakeP = takeP.ToString(nfi);
									var stLoss = ((bbands.Lower / 1000) * 999);
									StLoss = stLoss.ToString(nfi);
									RelativeSI = stocasticRSI.RSIValue;
									MoneyFI = mfi.MFI;
									ClPrice = candle.ClosePrice;
									tradeType = TradeType.Long;
									Strategy = "Middle-UP";
									return true;
								}
							}
							else if ((candle.ClosePrice > bbands.Upper)
								&& (stoch.K > 80)
								&& (stoch.D > 80)
								&& (_settings.AllowShorts))
							{
								// open sell order when price closes above upper bollinger bands
								// and stochastics K & D are both above 80
								tradeType = TradeType.Short;
								return true;
							}
						}
						ErrorMessage = "No Stochastic trigger";
					}
					if (_settings.ChStochRSI)
					{
						//if ((candle.ClosePrice < bbands.Upper))
						if ((stocasticRSI.KValue < (decimal)_settings.StochRSIK)
							&& (stocasticRSI.DValue < (decimal)_settings.StochRSID)
							&& (stocasticRSI.DValue3 < stocasticRSI.DValue2)
							&& (stocasticRSI.DValue2 < stocasticRSI.DValue1)
							&& (stocasticRSI.DValue1 < stocasticRSI.DValue)
							&& (bbands.Middle4 < bbands.Middle3)
							&& (bbands.Middle3 < bbands.Middle2)
							&& (bbands.Middle2 < bbands.Middle1)
							&& (bbands.Middle1 < bbands.Middle))
						{
							if (_settings.ChMACDUPMU)
							{
								if ((macd.macdsig2 < macd.macdsig1) && (macd.macdsig1 < macd.macdsig))
								{
									// open buy order when price closes above Middle bollinger bands
									// and stochastics K & D are both below 45
									// K3 smaller K2 and K2 smaller K
									NumberFormatInfo nfi = new NumberFormatInfo();
									nfi.NumberDecimalSeparator = ".";
									Bbands = bbands.Bandwidth;
									//					var bprice = ((bbands.Middle / 1000) * 1001);
									BPrice = candle.ClosePrice.ToString(nfi);
									//					var lprice = ((bbands.Middle / 10000) * 10005);
									//					LPrice = lprice.ToString(nfi);
									var takeP = ((bbands.Upper / 1000) * 998);
									TakeP = takeP.ToString(nfi);
									var stLoss = ((bbands.Lower / 1000) * 999);
									StLoss = stLoss.ToString(nfi);
									RelativeSI = stocasticRSI.RSIValue;
									MoneyFI = mfi.MFI;
									ClPrice = candle.ClosePrice;
									tradeType = TradeType.Long;
									Strategy = "Middle-UP";
									return true;
								}
							}
							else if (!_settings.ChMACDUPMU)
							{
								// open buy order when price closes above Middle bollinger bands
								// and stochastics K & D are both below 45
								// K3 smaller K2 and K2 smaller K
								NumberFormatInfo nfi = new NumberFormatInfo();
								nfi.NumberDecimalSeparator = ".";
								Bbands = bbands.Bandwidth;
								//					var bprice = ((bbands.Middle / 1000) * 1001);
								BPrice = candle.ClosePrice.ToString(nfi);
								//					var lprice = ((bbands.Middle / 10000) * 10005);
								//					LPrice = lprice.ToString(nfi);
								var takeP = ((bbands.Upper / 1000) * 998);
								TakeP = takeP.ToString(nfi);
								var stLoss = ((bbands.Lower / 1000) * 999);
								StLoss = stLoss.ToString(nfi);
								RelativeSI = stocasticRSI.RSIValue;
								MoneyFI = mfi.MFI;
								ClPrice = candle.ClosePrice;
								tradeType = TradeType.Long;
								Strategy = "Middle-UP";
								return true;
							}
						}
						else if ((candle.ClosePrice > bbands.Upper)
							&& (stoch.K > 80)
							&& (stoch.D > 80)
							&& (_settings.AllowShorts))
						{
							// open sell order when price closes above upper bollinger bands
							// and stochastics K & D are both above 80
							tradeType = TradeType.Short;
							return true;
						}
					}
					ErrorMessage = "No StochasticRSI trigger";
				}
				else if (_settings.ChBottumUp)
				{
					if ((candle.ClosePrice > _settings.LowSatBTC)
					&& (bbands.Bandwidth > (decimal)_settings.BUMinBollingerBandWidth)
					&& (bbands.Bandwidth < (decimal)_settings.BUMaxBollingerBandWidth))
					{
						if ((candle.ClosePrice < bbands.Lower)
							&& (stoch.K < 20 && stoch.D < 20)
							&& (mfi.MFI < _settings.BUMFI)
							&& (stocasticRSI.RSIValue < (decimal)_settings.BURSI))
						{
							// open buy order when price closes below lower bollinger bands
							// and stochastics K & D are both below 20
							NumberFormatInfo nfi = new NumberFormatInfo();
							nfi.NumberDecimalSeparator = ".";
							Bbands = bbands.Bandwidth;
							BPrice = candle.ClosePrice.ToString(nfi);
							var takeP = ((bbands.Upper / 1000) * 998);
							TakeP = takeP.ToString(nfi);
							var stLoss = ((bbands.Lower / 1000) * 999);
							StLoss = stLoss.ToString(nfi);
							RelativeSI = stocasticRSI.RSIValue;
							MoneyFI = mfi.MFI;
							ClPrice = candle.ClosePrice;
							tradeType = TradeType.Long;
							Strategy = "Bottom-UP";
							return true;
						}
						else if ((candle.ClosePrice > bbands.Upper)
							&& (stoch.K > 80)
							&& (stoch.D > 80)
							&& (_settings.AllowShorts))
						{
							// open sell order when price closes above upper bollinger bands
							// and stochastics K & D are both above 80
							tradeType = TradeType.Short;
							return true;
						}
					}
					ErrorMessage = "No BottomUp trigger";
				}
				else if (_settings.ChRSIFamily)
				{
					if (candle.ClosePrice > _settings.LowSatBTC)
					//						&& (bbands.Bandwidth > (decimal)_settings.MinBollingerBandWidth)
					//						&& (bbands.Bandwidth < (decimal)_settings.MaxBollingerBandWidth))
					{
						if ((mfi.MFI < _settings.RsiMFI)
							&& (stocasticRSI.KValue < (decimal)_settings.RsiStochRSI)
							&& (stocasticRSI.DValue < (decimal)_settings.RsiStochRSI)
							&& (stocasticRSI.RSIValue < (decimal)_settings.RsiRSI)
							&& (stocasticRSI.KValue > stocasticRSI.DValue))
						{
							NumberFormatInfo nfi = new NumberFormatInfo();
							nfi.NumberDecimalSeparator = ".";
							Bbands = bbands.Bandwidth;
							BPrice = candle.ClosePrice.ToString(nfi);
							var takeP = ((bbands.Upper / 1000) * 998);
							TakeP = takeP.ToString(nfi);
							var stLoss = ((bbands.Lower / 100) * 90);
							StLoss = stLoss.ToString(nfi);
							RelativeSI = stocasticRSI.RSIValue;
							MoneyFI = mfi.MFI;
							ClPrice = candle.ClosePrice;
							Strategy = "RSI Famlily";
							tradeType = TradeType.Long;
							return true;
						}
						else if ((mfi.MFI < _settings.RsiMFISell)
							&& (stocasticRSI.KValue < (decimal)_settings.RsiStochRSISell
							&& stocasticRSI.DValue < (decimal)_settings.RsiStochRSISell)
							&& (stocasticRSI.RSIValue < (decimal)_settings.RsiRSISell))
						{
							// open sell order when price closes above upper bollinger bands
							// and stochastics K & D are both above 80
							tradeType = TradeType.Short;
							return true;
						}
					}
					//					ErrorMessage = "No RSIFamily trigger";
				}
				ErrorMessage = "No RSIFamily trigger";
			}
			//if (ErrorMessage.Length == 0) ErrorMessage = "No trigger";
			return false;
		}

        /// <summary>
        /// Scans for possibility to open a new trade.
        /// </summary>
        /// <param name="candles">candle history</param>
        /// <param name="bar">currentcandle</param>
        /// <param name="tradeManager">tradeManager</param>
        /// <param name="Errormessage">Errormessage</param>
        private void ScanForEntry(List<MarketCandle> candles, int bar, ITradeManager tradeManager, out string ErrorMessage)
        {
            ErrorMessage = "";
            TradeType tradeType;
            if (IsValidEntry(candles, bar, out tradeType, out ErrorMessage))
            {
                var candle = candles[bar];
                var totalRebuy = 1m;
                if (MaxTimesRebuy >= 1) totalRebuy += FirstRebuyFactor;
                if (MaxTimesRebuy >= 2) totalRebuy += SecondRebuyFactor;
                if (MaxTimesRebuy >= 3) totalRebuy += ThirdRebuyFactor;
                _bundleSize = tradeManager.AccountBalance / totalRebuy;
                var coins = _bundleSize / candle.ClosePrice;
                switch (tradeType)
                {
                    case TradeType.Long:
                        _trade = tradeManager.BuyMarket(Symbol, coins);
                        break;

                    case TradeType.Short:
                        _trade = tradeManager.SellMarket(Symbol, coins);
                        break;
                }
                if (_trade != null)
                {
                    _state = StrategyState.Opened;
                }
            }
        }

        /// <summary>
        /// Returns if the conditions for a rebuy are met.
        /// </summary>
        /// <returns><c>true</c>, if rebuy is possible, <c>false</c> otherwise.</returns>
        /// <param name="candles">candle history</param>
        /// <param name="bar">currentcandle</param>
        private bool CanRebuy(List<MarketCandle> candles, int bar)
        {
            var bbands = new BollingerBands(candles, bar);
            var stoch = new Stochastics(candles, bar);
            //var RSI = new RSI(candles, bar);
            var candle = candles[bar];

            // for long we do a rebuy when price closes under the lower bollinger bands and both stochastics are below 20
            // and price is 1.75% below the previous buy
            if (_trade.TradeType == TradeType.Long && candle.ClosePrice < bbands.Lower && stoch.K < 20 && stoch.D < 20)
            {
                var price = _trade.OpenPrice * RebuyPercentage;
                if (_trade.Rebuys.Count > 0) price = _trade.Rebuys.Last().Price * RebuyPercentage;
                return candle.ClosePrice < price;
            }

            // for short we do a rebuy when price closes above the upper bollinger bands and both stochastics are above 80
            // and price is 1.75% above the previous sell
            if (_trade.TradeType == TradeType.Short && candle.ClosePrice > bbands.Upper && stoch.K > 80 && stoch.D > 80)
            {
                var factor = 1m + (1m - RebuyPercentage);
                var price = _trade.OpenPrice * factor;
                if (_trade.Rebuys.Count > 0) price = _trade.Rebuys.Last().Price * factor;
                return candle.ClosePrice > price;
            }
            return false;
        }

        /// <summary>
        /// Closes the trade if price crosses the upper/lower bollinger band.
        /// </summary>
        /// <param name="candles">candle history</param>
        /// <param name="bar">currentcandle</param>
        /// <param name="tradeManager">tradeManager</param>
        private void CloseTradeIfPossible(List<MarketCandle> candles, int bar, ITradeManager tradeManager)
        {
            var candle = candles[bar];
            var bbands = new BollingerBands(candles, bar);


            // for long we close the trade if price  gets above the upper bollinger bands
            if (_trade.TradeType == TradeType.Long && candle.HighPrice > bbands.Upper)
            {
                if (tradeManager.Close(_trade, candle.HighPrice))
                {
                    _state = StrategyState.Scanning;
                    _trade = null;
                    return;
                }
            }

            // for short we close the trade if price  gets below the lowe bollinger bands
            if (_trade.TradeType == TradeType.Short && candle.LowPrice < bbands.Lower)
            {
                if (tradeManager.Close(_trade, candle.LowPrice))
                {
                    _state = StrategyState.Scanning;
                    _trade = null;
                    return;
                }
            }
        }
    }
}
