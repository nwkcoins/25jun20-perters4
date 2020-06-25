//using CryptoTraderScanner.Bot.Implementation;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.ScannerSQLiteDB;
//using CryptoTraderStandard.Scanner;
using CryptoTraderStandard.Strategy;
using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CryptoTraderScanner
{
    public class Scanner
    {
        //private const string SOUND_TADA_FILE = "C:\\Windows\\Media\\tada.wav";
        private int _ApiExchangeNr = 0;
        private string _Exchangename = "";
        private ExchangeAPI _Api = null;
        private coinmarketdataApiClient _Apicmd = null;
        private HttpClient _HttpClient;
        private List<MarketCandleCMD> AllCandles_1min = new List<MarketCandleCMD>();
        private List<MarketCandleCMD> AllCandles_3min = new List<MarketCandleCMD>();
        private List<MarketCandleCMD> AllCandles_5min = new List<MarketCandleCMD>();
        private List<MarketCandleCMD> AllCandles_15min = new List<MarketCandleCMD>();
        private List<MarketCandleCMD> AllCandles_30min = new List<MarketCandleCMD>();
        private List<MarketCandleCMD> AllCandles_1hr = new List<MarketCandleCMD>();
        private List<MarketCandleCMD> AllCandles_4hr = new List<MarketCandleCMD>();
        private List<MarketCandleCMD> AllCandles_1day = new List<MarketCandleCMD>();
        private List<string> _AllSymbols = new List<string>();
        private bool GettingCandleData = false;

        public Scanner(HttpClient HttpClient)
        {
            _HttpClient = HttpClient;
            _Apicmd = new coinmarketdataApiClient(_HttpClient);
        }

        public async Task<(string Message, string Response)> GetUriData(string Destination, string Cmd, string UriQuery)
        {
            string Message = "";
            string Response = "";
            if (_Apicmd != null)
            {
                var (getMessage, getResponse) = await _Apicmd.getData(Destination, Cmd, UriQuery);
                Message = getMessage;
                Response = getResponse;
            }
            return (Message, Response);
        }

        public async Task<(string Message, System.Net.HttpStatusCode StatusCode, string Response)> PostUriData(string Destination, string Cmd, string PostData)
        {
            string Message = "";
            string Response = "";
            System.Net.HttpStatusCode StatusCode = System.Net.HttpStatusCode.NotImplemented;

            if (_Apicmd != null)
            {
                var (postMessage, postStatus, postResponse) = await _Apicmd.postAsyncData(Destination, Cmd, PostData);
                Message = postMessage;
                StatusCode = postStatus;
                Response = postResponse;
            }
            return (Message, StatusCode, Response);
        }

        private bool SetExchange(string Exchange)
        {
            try
            {
                if (_Api == null || Exchange != _Exchangename)
                {
                    if (_Api != null)
                    {
                        _Api.Dispose();
                        _Api = null;
                    }

                    switch (Exchange.ToLower())
                    {
                        case "binance":
                            _ApiExchangeNr = 1;
                            _Exchangename = "Binance";
                            _Api = new ExchangeBinanceAPI();
                            break;

                        case "bitfinex":
                            _ApiExchangeNr = 2;
                            _Exchangename = "Bitfinex";
                            _Api = new ExchangeBitfinexAPI();
                            break;

                        case "bittrex":
                            _ApiExchangeNr = 3;
                            _Exchangename = "Bittrex";
                            _Api = new ExchangeBittrexAPI();
                            break;

                        case "kraken":
                            _ApiExchangeNr = 4;
                            _Exchangename = "Kraken";
                            _Api = new ExchangeKrakenAPI();
                            break;

                        case "Coinbase":
                            _ApiExchangeNr = 5;
                            _Exchangename = "Coinbase";
                            _Api = new ExchangeCoinbaseAPI();
                            break;

                        case "hitbtc":
                            _ApiExchangeNr = 6;
                            _Exchangename = "HitBTC";
                            _Api = new ExchangeHitBTCAPI();
                            break;
                        default:
                            _ApiExchangeNr = 0;
                            _Exchangename = "Unknown";
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} SetExchange ERROR: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }
            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} SetExchange: ExchangeNr={_ApiExchangeNr} => {_Exchangename}; API is " + (_Api == null ? "NOT" : "") + "Active");
            return (_ApiExchangeNr != 0);
        }

        public async Task<string> StartAndCheckSubscriptions()
        {
            string result = "";
            Settings Settings = SettingsStore.Load();

            // For now: use ownAPI here ... ;)
            if (true || Settings.DataViaOwnAPI)
            {
                result = await _Apicmd.ConnectWebSocket();
            }
            else //if (SetExchange(Settings.Exchange))
            {
                result = await _Apicmd.ConnectWebSocket();  //"NotImplemented, yet";
            }
            return result;
        }

        /// <summary>
        /// Downloads all symbols from the exchanges and filters out the coins with enough 24hr Volume
        /// </summary>
        /// <returns></returns>
        public List<MarketPairsCMD> FindCoinsWithEnoughVolume(int minutes, out string Message)
        {
            Message = "";
            List<MarketPairsCMD> UsedMarketPairs = new List<MarketPairsCMD>();
            try
            {
                Settings Settings = SettingsStore.Load();

                List<MarketPairsCMD> AllMarketpairs = new List<MarketPairsCMD>();
                if (Settings.DataViaOwnAPI)
                {
                    //AllMarketpairs = _Apicmd.GetMarketSymbolsAsyncCMD().Sync().ToList();
                    AllMarketpairs = _Apicmd.getMarketPairs;
                }
                else if (SetExchange(Settings.Exchange))
                {
                    List<string> AllSymbols = _Api.GetMarketSymbolsAsync().Sync().ToList(); // Firewall: open uitgaande 443 port (!)
                    List<KeyValuePair<string, ExchangeTicker>> AllTickers = _Api.GetTickersAsync().Sync().ToList();

                    foreach (string Symbol in AllSymbols)
                    {
                        ExchangeTicker Ticker = AllTickers.Find(s => (s.Key == Symbol)).Value;
                        if (Ticker != null)
                        {
                            AllMarketpairs.Add(new MarketPairsCMD()
                            {
                                Exchangename = _Exchangename,
                                Pairname = Symbol,
                                BaseCurrency = Ticker.Volume.BaseCurrency,
                                QuoteCurrency = Ticker.Volume.QuoteCurrency,
                                StartDateTime24Hr = Ticker.Volume.Timestamp,
                                EndDateTime24Hr = Ticker.Volume.Timestamp.AddMinutes(minutes).AddSeconds(-1),
                                NowDateTime = DateTime.Now,
                                QuoteCurrencyVolume24Hr = ((_Api is ExchangeBittrexAPI) ? Ticker.Volume.BaseCurrencyVolume : Ticker.Volume.QuoteCurrencyVolume),
                                BaseCurrencyVolume24Hr = ((_Api is ExchangeBittrexAPI) ? Ticker.Volume.QuoteCurrencyVolume : Ticker.Volume.BaseCurrencyVolume),
                                CountCandles24Hr = 1440 / minutes
                            });
                        }
                    }
                }

                _AllSymbols.Clear();

                // for each symbol
                foreach (MarketPairsCMD Marketpair in AllMarketpairs)
                {
                    if (!String.IsNullOrEmpty(Marketpair.Pairname))
                    {
                        _AllSymbols.Add(Marketpair.Pairname);
                    }

                    if (!IsValidCurrency(Marketpair.Pairname))
                    {
                        // ignore, symbol has wrong currency
                        continue;
                    }

                    // check 24hr volume
                    //var ticker = allTickers.FirstOrDefault(e => e.Key == Marketpair.Pairname).Value;
                    decimal volume = Marketpair.QuoteCurrencyVolume24Hr; //ticker.Volume.QuoteCurrencyVolume;

                    /*if (_Api is ExchangeBittrexAPI)
                    {
                        volume = Marketpair.BaseCurrencyVolume24Hr;  // ticker.Volume.BaseCurrencyVolume;
                    }*/

                    if (volume < Settings.Min24HrVolume)
                    {
                        // ignore since 24hr volume is too low
                        continue;
                    }

                    UsedMarketPairs.Add(Marketpair);
                }
                _AllSymbols = _AllSymbols.OrderBy(s => s).ToList();
                UsedMarketPairs = UsedMarketPairs.OrderBy(e => e.Pairname).ToList();
            }
            catch (Exception E)
            {
                Message = $"ERROR FindCoinsWithEnoughVolume: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }
            return UsedMarketPairs;
        }

        /// <summary>
        /// returns whether currency is allowed
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private bool IsValidCurrency(string symbol)
        {
            Settings Settings = SettingsStore.Load();
            bool validCurrency = false;
            string symbolLC = symbol.ToUpper();
            string blacklistLC = "" + Settings.BlacklistCoinpairs;
            string whitelistLC = "" + Settings.WhitelistCoinpairs;

            if ((!String.IsNullOrEmpty(whitelistLC)) && whitelistLC.ToUpper().Contains(symbolLC))
            {
                validCurrency = true;
            }
            else if (((Settings.ETH && symbolLC.EndsWith("ETH"))
                || (Settings.EUR && symbolLC.EndsWith("EUR"))
                || (Settings.USDT && symbolLC.EndsWith("USDT"))
                || (Settings.BTC && symbolLC.EndsWith("BTC"))
                || (Settings.BTC && symbolLC.Contains("XBT"))
                || (Settings.BNB && symbolLC.EndsWith("BNB")))
                && (String.IsNullOrEmpty(blacklistLC)
                || ((!String.IsNullOrEmpty(blacklistLC)) && !blacklistLC.ToUpper().Contains(symbolLC))))
            {
                validCurrency = true;
            }
            return validCurrency;
        }

        /// <summary>
        /// List of symbols
        /// </summary>
        /// <value>The symbols.</value>
        public List<string> AllSymbols
        {
            get
            {
                return _AllSymbols;
            }
        }

        public async Task<string> ShowOrderbook()
        {
            string Message = "";
            try
            {
                Settings Settings = SettingsStore.Load();
                if (SetExchange(Settings.Exchange))
                {
                    switch (_ApiExchangeNr)
                    {
                        case 1: // Binance
                            if (!String.IsNullOrEmpty(Settings.ApikeyBinance) && !String.IsNullOrEmpty(Settings.ApiSecretBinance))
                            {
                                using (IWebSocket OrderbookSocket = await _Api.GetOrderDetailsWebSocketAsync(order =>
                                {
                                    ExchangeOrderResult wsOrderresult = order;
                                })) ;
                            }
                            else
                            {
                                Message = "Binance Key / Secret not set!";
                            }
                            break;
                        default:
                            Message = "Orderbook not implemented for Exchange " + _Exchangename;
                            break;
                    }
                }
            }
            catch (Exception E)
            {
                Message = $"ERROR ShowOrderbook: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }
            return Message;
        }

        public Decimal GetLastClosePrice(string Symbol, int PeriodeMinutes, out string Message)
        {
            Message = "";
            Decimal ResultClosePrice = Decimal.Zero;

            try
            {
                //int PeriodSeconds = PeriodeMinutes * 60;
                //ResultClosePrice = AllCandles.Where(c => (c.Name == Symbol && c.PeriodSeconds == PeriodSeconds)).OrderByDescending(c => c.Timestamp).First().ClosePrice;
                switch (PeriodeMinutes)
                {
                    case 1: ResultClosePrice = AllCandles_1min.First().ClosePrice; break;
                    case 3: ResultClosePrice = AllCandles_3min.First().ClosePrice; break;
                    case 5: ResultClosePrice = AllCandles_5min.First().ClosePrice; break;
                    case 15: ResultClosePrice = AllCandles_15min.First().ClosePrice; break;
                    case 30: ResultClosePrice = AllCandles_30min.First().ClosePrice; break;
                    case 60: ResultClosePrice = AllCandles_1hr.First().ClosePrice; break;
                    case 240: ResultClosePrice = AllCandles_4hr.First().ClosePrice; break;
                    case 1440: ResultClosePrice = AllCandles_1day.First().ClosePrice; break;
                }
            }
            catch (Exception E)
            {
                Message = $"GetLastClosePrice for {Symbol} in {PeriodeMinutes} minutes ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return ResultClosePrice;
        }

        public async Task<string> UpdateCandlesData(List<MarketPairsCMD> UsedMarketPairs, int PeriodeMinutes, string TimeframeName, int CountCandles)
        {
            string Message = "";
            //List<SignalsTBL> AllSignals = new List<SignalsTBL>();

            try
            {
                List<MarketCandleCMD> AllCandles = new List<MarketCandleCMD>();
                Settings Settings = SettingsStore.Load();

                List<string> AllSymbols = UsedMarketPairs.Select(s => s.Pairname).OrderBy(id => id).ToList();

                // download new candles form all symbols
                if (Settings.DataViaOwnAPI)
                {
                    string csvSymbols = string.Join(",", AllSymbols.ToArray());
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} UpdateCandlesData OwnAPI for {PeriodeMinutes}min: to GetCandlesAsyncCMD with {AllSymbols.Count} symbols, {CountCandles} candles");
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} UpdateCandlesData OwnAPI Symbols: {csvSymbols}");
                    (Message, AllCandles) = await _Apicmd.GetCandlesAsyncCMD(csvSymbols, PeriodeMinutes, null, null, CountCandles);
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} UpdateCandlesData OwnAPI for {PeriodeMinutes}min: back GetCandlesAsyncCMD with {AllCandles.Count} candles");
                }
                else if (SetExchange(Settings.Exchange))
                {
                    int SymbolCounter = 0;
                    foreach (string Symbol in AllSymbols)
                    {
                        SymbolCounter++;
                        List<MarketCandle> SymbolCandles = (await _Api.GetCandlesAsync(Symbol, 60 * PeriodeMinutes, DateTime.Now.AddMinutes(-1 * CountCandles * PeriodeMinutes))).ToList();
                        foreach (MarketCandle SymbolCandle in SymbolCandles)
                        {
                            AllCandles.Add(new MarketCandleCMD()
                            {
                                ExchangeName = _Exchangename,
                                Name = SymbolCandle.Name,
                                Timestamp = SymbolCandle.Timestamp,
                                PeriodSeconds = SymbolCandle.PeriodSeconds,
                                OpenPrice = SymbolCandle.OpenPrice,
                                ClosePrice = SymbolCandle.ClosePrice,
                                LowPrice = SymbolCandle.LowPrice,
                                HighPrice = SymbolCandle.HighPrice,
                                BaseCurrencyVolume = (decimal)SymbolCandle.BaseCurrencyVolume,
                                QuoteCurrencyVolume = (decimal)SymbolCandle.QuoteCurrencyVolume,
                                WeightedAverage = SymbolCandle.WeightedAverage
                            });
                        }
                    }
                    //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} UpdateCandlesData {Settings.Exchange} {PeriodeMinutes}minutes: got {SymbolCounter}/{AllSymbols.Count} {Symbol}: total {AllCandles.Count} candles");
                }

                AllCandles = AddMissingCandles(AllCandles, PeriodeMinutes);

                switch (PeriodeMinutes)
                {
                    case 1: AllCandles_1min = AllCandles; break;
                    case 3: AllCandles_3min = AllCandles; break;
                    case 5: AllCandles_5min = AllCandles; break;
                    case 15: AllCandles_15min = AllCandles; break;
                    case 30: AllCandles_30min = AllCandles; break;
                    case 60: AllCandles_1hr = AllCandles; break;
                    case 240: AllCandles_4hr = AllCandles; break;
                    case 1440: AllCandles_1day = AllCandles; break;
                }

                if (AllSymbols.Count > 0)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} UpdateCandlesData {(Settings.DataViaOwnAPI ? "OWNAPI" : Settings.Exchange)} for {PeriodeMinutes}min: got {AllCandles.Count} candles for {AllSymbols.Count} symbols = {(AllCandles.Count / AllSymbols.Count).ToString("n1")} candles/symbol");
                    /*foreach (MarketCandleCMD mc in AllCandles.Where(c => c.Name == "ETHBTC").ToList())
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} UpdateCandlesData Candle={mc.Name} at {mc.Timestamp.ToString("ddMMyy.HHmmss")} secs={mc.PeriodSeconds} Open={mc.OpenPrice} close={mc.ClosePrice} ");
                    }*/
                }
            }
            catch (Exception E)
            {
                Message = $"UpdateCandlesData {TimeframeName} for {CountCandles} candles ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return Message;
        }
        /// <summary>
        /// Performs a scan for all filtered symbols
        /// </summary>
        /// <returns></returns>
        public List<SignalsTBL> FindNewSignalsToBuySellAsync(List<MarketPairsCMD> UsedMarketPairs, int PeriodeMinutes, string TimeframeName, int CountCandles, Decimal Bm1Hr, Decimal Bm4Hr, Decimal Bm1Day, bool LogInvalidSignals)
        {
            List<SignalsTBL> AllSignals = new List<SignalsTBL>();

            try
            {
                int PeriodSeconds = PeriodeMinutes * 60;
                List<MarketCandleCMD> Candles = new List<MarketCandleCMD>();

                // Per marketpair check buy strategies
                foreach (MarketPairsCMD MarketPair in UsedMarketPairs)
                {
                    SignalsTBL NewSignal = new SignalsTBL()
                    {
                        Valid = false,
                        Exchange = MarketPair.Exchangename,
                        Symbol = MarketPair.Pairname,
                        TimeframeName = TimeframeName,
                        TimeframeMinutes = PeriodeMinutes,
                        SignalState = SignalsTBL.SignalStates.New,
                        Quote100Volume = MarketPair.Quote100Last1MinuteVolume,
                        MaxBuyAmount = Math.Round(MarketPair.CalculatedMaxBuyAmount, 2),
                        Bm1Hr = Math.Round(Bm1Hr, 3),
                        Bm4Hr = Math.Round(Bm4Hr, 3),
                        Bm1Day = Math.Round(Bm1Day, 3)
                    };

                    switch (PeriodeMinutes)
                    {
                        case 1: Candles = AllCandles_1min; break;
                        case 3: Candles = AllCandles_3min; break;
                        case 5: Candles = AllCandles_5min; break;
                        case 15: Candles = AllCandles_15min; break;
                        case 30: Candles = AllCandles_30min; break;
                        case 60: Candles = AllCandles_1hr; break;
                        case 240: Candles = AllCandles_4hr; break;
                        case 1440: Candles = AllCandles_1day; break;
                    }
                    List<MarketCandleCMD> CoinCandles = Candles.Where(c => (c.Name == MarketPair.Pairname)).OrderByDescending(c => c.Timestamp).ToList();
                    if (CoinCandles.Count > 0)
                    {
                        NewSignal.CandleCount = CoinCandles.Count;

                        NewSignal.StartDateTime = CoinCandles[0].Timestamp.AddHours(0);

                        //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} FindNewSignalsToBuySellAsync: Enter CheckMarketpairFor1stBuySell {PeriodeMinutes}minutes for {MarketPair.Pairname} with {CountCandles}={cc}={ccc} candles");

                        ScannerStrategy.CheckMarketpairFor1stBuySell(MarketPair, CoinCandles, ref NewSignal); // was ProcessScannerStrategy

                        if (NewSignal.Valid || LogInvalidSignals)
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} FindNewSignalsToBuySellAsync: 1stBuyStrategy {PeriodeMinutes}minutes for {MarketPair.Pairname} with {CountCandles}={NewSignal.CandleCount} candles"
                                + $" => {(NewSignal.Valid ? "" : "IN")}Valid: " + (NewSignal.Valid ? $"{NewSignal.Strategy.ToString()}, {NewSignal.SignalAction.ToString()}, {NewSignal.TradeType.ToString()}, buy={NewSignal.BuyPrice.ToString("0.00000000")}" : NewSignal.Message));
                        }

                        /*NewSignal.Valid = StrategyResult.Valid;
                        NewSignal.SignalState = SignalsTBL.SignalStates.New;
                        NewSignal.SignalAction = StrategyResult.SignalAction;
                        NewSignal.Message = StrategyResult.Message;
                        NewSignal.TradeType = StrategyResult.TradeType;
                        NewSignal.Strategy = StrategyResult.Strategy;*/

                        /*if (StrategyResult.Valid)
                        {
                            NewSignal.BBWidth = Math.Round(StrategyResult.BolingerBandwidth, 2);
                            NewSignal.RSI = Math.Round(StrategyResult.RSI, 2);
                            NewSignal.MFI = Math.Round(StrategyResult.MFI, 2);
                            NewSignal.BuyPrice = StrategyResult.BuyPrice;
                            NewSignal.TakeProfit = StrategyResult.TakeProfit;
                            NewSignal.StopLoss = StrategyResult.StopLoss;
                            NewSignal.ClosePrice = StrategyResult.ClosePrice;
                            NewSignal.PositionSize = StrategyResult.PositionSize;
                        }*/

                        /*if (NewSignal.SignalAction != SignalsTBL.SignalActions.None)
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} FindNewSignalsToBuySellAsync: Symbol={MarketPair.Pairname} TF={PeriodeMinutes} {cc}={NewSignal.CandleCount} {(CoinCandles.Count > 0 ? CoinCandles[0].Name : "")} CoinCandles => Result {NewSignal.SignalAction.ToString()}");
                        }*/
                    }
                    else
                    {
                        NewSignal.Valid = false;
                        NewSignal.Message = "ERROR FindNewSignalsToBuySellAsync: For symbol NO candles received!";
                    }

                    AllSignals.Add(NewSignal);
                }
                //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} FindNewSignalsToBuySellAsync: {UsedMarketPairs.Count} symbols => {AllSignals.Count} signals");
            }
            catch (Exception E)
            {
                AllSignals.Add(new SignalsTBL()
                {
                    Valid = false,
                    Message = $"ERROR FindNewSignalsToBuySellAsync: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'"
                });
            }
            return AllSignals;
        }

        public SignalsTBL ProcessSignalAfterFilled(SignalsTBL Signal, int AutocloseAfterCandles, int AutocloseAfterHr, Decimal Bm1Hr, Decimal Bm4Hr, Decimal Bm1Day)
        {
            SignalsTBL NewSignal = new SignalsTBL();

            try
            {
                NewSignal = ScannerSQLiteDbStore.CopySignal(Signal); // copy all values of "Mother"-Signal into this New Signal (and if valid will be added to DB later in main)

                int AutoCloseMinutes = ((AutocloseAfterCandles > 0)
                    ? AutocloseAfterCandles * Signal.TimeframeMinutes
                    : ((AutocloseAfterHr > 0)
                        ? AutocloseAfterHr * 60
                        : 0));
                bool AutoCloseNow = ((AutoCloseMinutes > 0) && (Signal.StartDateTime.AddMinutes(AutoCloseMinutes) < DateTime.Now));

                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ProcessSignalAfterFilled id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.SignalAction.ToString()} {Signal.TimeframeName} {Signal.Symbol}: BuyTime={Signal.StartDateTime.ToString()} Now={DateTime.Now} is " + (AutoCloseNow ? "after" : "shorter then")
                    + $" {AutoCloseMinutes} minutes ago => " + (AutoCloseNow ? "AutoClose detected => State->CompletedAutoClose" : "Before AutoClose -> Scan!"));

                if (AutoCloseNow)
                {
                    NewSignal.Valid = true;
                    NewSignal.SignalAction = SignalsTBL.SignalActions.SellAutoclose;
                    NewSignal.Message = "AutoClose detected: Action->SellAutoClose";
                }
                else
                {
                    List<MarketCandleCMD> Candles = new List<MarketCandleCMD>();

                    // Get Candles for Symbol and timeframe of Signal
                    switch (Signal.TimeframeMinutes)
                    {
                        case 1: Candles = AllCandles_1min; break;
                        case 3: Candles = AllCandles_3min; break;
                        case 5: Candles = AllCandles_5min; break;
                        case 15: Candles = AllCandles_15min; break;
                        case 30: Candles = AllCandles_30min; break;
                        case 60: Candles = AllCandles_1hr; break;
                        case 240: Candles = AllCandles_4hr; break;
                        case 1440: Candles = AllCandles_1day; break;
                    }
                    List<MarketCandleCMD> SymbolCandles = Candles.Where(c => (c.Name == Signal.Symbol)).OrderByDescending(c => c.Timestamp).ToList();

                    if (SymbolCandles.Count > 0)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ProcessSignalAfterFilled: Symbol={Signal.Symbol} TF={Signal.TimeframeName} => {SymbolCandles.Count} candles selected, scanning...");

                        NewSignal.Valid = false;
                        NewSignal.Bm1Hr = Math.Round(Bm1Hr, 3);
                        NewSignal.Bm4Hr = Math.Round(Bm4Hr, 3);
                        NewSignal.Bm1Day = Math.Round(Bm1Day, 3);

                        // BuyPrice : still Signal one for BottomUp
                        ScannerStrategy.CheckSignalAfterFilled(SymbolCandles, ref NewSignal);

                        /*if (NewSignal.Message != StrategyResult.Message) NewSignal.Message = StrategyResult.Message;
                        NewSignal.Valid = StrategyResult.Valid;
                        if (StrategyResult.Valid)
                        {
                            NewSignal.SignalState = StrategyResult.SignalState;
                            NewSignal.SignalAction = StrategyResult.SignalAction;
                            if (NewSignal.SignalAction == SignalsTBL.SignalActions.Buy || NewSignal.SignalAction == SignalsTBL.SignalActions.ReBuy) NewSignal.BuyCount++;
                            if (NewSignal.SignalAction == SignalsTBL.SignalActions.Sell) NewSignal.BuyCount--;

                            NewSignal.BBWidth = StrategyResult.BolingerBandwidth;
                            NewSignal.RSI = StrategyResult.RSI;
                            NewSignal.MFI = StrategyResult.MFI;
                            NewSignal.BuyPrice = StrategyResult.BuyPrice;
                            NewSignal.TakeProfit = StrategyResult.TakeProfit;
                            NewSignal.StopLoss = StrategyResult.StopLoss;
                            NewSignal.ClosePrice = StrategyResult.ClosePrice;
                            NewSignal.PositionSize = StrategyResult.PositionSize;
                        }*/
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ProcessSignalAfterFilled NewSignal=[" + String.Join("; ", NewSignal) + "]");
                    }
                    else
                    {
                        NewSignal.Valid = false;
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ProcessSignalAfterFilled WARNING: Symbol={Signal.Symbol} TF={Signal.TimeframeName}={Signal.TimeframeMinutes} => NO candles available!");
                    }
                }
            }
            catch (Exception E)
            {
                NewSignal.Message = $"ProcessedSignalAfterFilled ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return NewSignal;
        }



        //        public static void PlaySound(string file)
        //        {
        //            Process.Start(@"powershell", $@"-c (New-Object Media.SoundPlayer '{file}').PlaySync();");
        //
        //       }

        public bool IsBaroMeterCurrent(int PeriodMinutes)
        {
            DateTime BMNow = DateTime.MinValue;
            DateTime Now = DateTime.Now;
            switch (PeriodMinutes)
            {
                case 1440: BMNow = _Apicmd.getBarometer1D.NowDatetime; break;
                case 240: BMNow = _Apicmd.getBarometer4Hr.NowDatetime; break;
                case 60: BMNow = _Apicmd.getBarometer1Hr.NowDatetime; break;
            }
            return (BMNow.AddSeconds(-BMNow.Second + 1) > Now.AddSeconds(-Now.Second - 60));
        }

        public BarometerCMD GetBarometerData(int PeriodeMinutes, bool DataViaOwnAPI, string Exchange)
        {
            BarometerCMD BarometerData = new BarometerCMD();
            //Settings Settings = SettingsStore.Load();

            // for the moment: use always ownAPI... ;)
            if (true || DataViaOwnAPI)
            {
                //BarometerData = await _Apicmd.GetBarometerAsyncCMD(PeriodeMinutes);
                switch (PeriodeMinutes)
                {
                    case 1440: BarometerData = _Apicmd.getBarometer1D; break;
                    case 240: BarometerData = _Apicmd.getBarometer4Hr; break;
                    case 60: BarometerData = _Apicmd.getBarometer1Hr; break;
                }
            }
            else if (SetExchange(Exchange))
            {
                Decimal PreviousSum = 1;
                Decimal NowSum = 1;
                // ToDo: Deze PreviousSum en NowSum via api bepalen...

                BarometerData.Quotename = "BTC";
                BarometerData.PeriodeMinutes = PeriodeMinutes;
                BarometerData.PreviousSum = PreviousSum;
                BarometerData.NowSum = NowSum;
                BarometerData.BmPercentage = ((NowSum - PreviousSum) / PreviousSum) * 100;
                BarometerData.PreviousDatetime = DateTime.Now.AddMinutes(-PeriodeMinutes);
                BarometerData.NowDatetime = DateTime.Now;
            }
            return BarometerData;
        }

        private List<MarketCandleCMD> AddMissingCandles(List<MarketCandleCMD> Candles, int PeriodMinutes)
        {
            List<MarketCandleCMD> result = new List<MarketCandleCMD>();
            try
            {
                if (Candles.Count > 0)
                {
                    result.Add(Candles[0]);
                    DateTime timeStamp = Candles[0].Timestamp;
                    for (int i = 1; i < Candles.Count; ++i)
                    {
                        MarketCandleCMD NextCandle = Candles[i];
                        decimal mins = (decimal)(timeStamp - NextCandle.Timestamp).TotalMinutes;
                        while (mins > PeriodMinutes)
                        {
                            result.Add(new MarketCandleCMD()
                            {
                                OpenPrice = NextCandle.OpenPrice,
                                ClosePrice = NextCandle.ClosePrice,
                                HighPrice = NextCandle.HighPrice,
                                LowPrice = NextCandle.LowPrice,
                                Timestamp = timeStamp.AddMinutes(-PeriodMinutes)
                            });
                            mins -= PeriodMinutes;
                        }
                        result.Add(NextCandle);
                        timeStamp = NextCandle.Timestamp;
                    }
                }
                return result;
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} AddMissingCandles ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
                return result;
            }
        }
    }
}