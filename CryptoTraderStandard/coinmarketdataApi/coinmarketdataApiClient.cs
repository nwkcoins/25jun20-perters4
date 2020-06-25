using CryptoTraderScanner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Linq;
//using WebSocketSharp;
using SocketIOClient;
//using SignalR.Client.Hubs;
//using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoTraderStandard.coinmarketdataApi
{
    class coinmarketdataApiClient
    {
        const string UrlCoinmarketdata = "https://api.coinmarketdata.eu/v1";
        const string SocketCoinmarketdata = "http://api.coinmarketdata.eu:30965";
        const string UrlBinance = "https://api.binance.com";
        const string UrlZignaly = "https://zignaly.com/api";

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        private static HttpClient _httpclient;
        private SocketIO clientSocketIO;
        //private 
        private BarometerCMD _BarometerData1Hr = new BarometerCMD(),
            _BarometerData4Hr = new BarometerCMD(),
            _BarometerData1D = new BarometerCMD();
        private List<MarketPairsCMD> _Marketpairs = new List<MarketPairsCMD>();

        public coinmarketdataApiClient(HttpClient httpclient)
        {
            _httpclient = httpclient;
        }

        private string InitWebSocket()
        {
            string Message = "";
            try
            {
                clientSocketIO = new SocketIO(SocketCoinmarketdata)
                {
                    // if server need some parameters, you can add to here
                    /*Parameters = new Dictionary<string, string>
                    {
                        { "uid", "1234uid456" },
                        { "token", "345token678" }
                    }*/
                };

                // Listen server events
                clientSocketIO.On("bm", res =>
                {
                    try
                    {
                        // Rxd: "\"{s:'BTC',m:1440,b:0.13793042,p:0.10762673,n:0.10777518,e:1587811995,a:'25-04-20 12:53:15',t:1587898395,d:'26-04-20 12:53:15',c:173}\""
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InitWebSocket on.bm: Websocket Rx 'bm': " + res.Text);
                        string jsonData = res.Text.Replace("\"", "");
                        dynamic BmData = JObject.Parse(jsonData);
                        BarometerCMD BmValues = new BarometerCMD()
                        {
                            Quotename = BmData.s,
                            PeriodeMinutes = BmData.m,
                            BmPercentage = BmData.b,
                            PreviousSum = BmData.p,
                            NowSum = BmData.n,
                            PreviousDatetime = BmData.a,
                            NowDatetime = BmData.d,
                            PairCount = BmData.c
                        };
                        switch (BmValues.PeriodeMinutes)
                        {
                            case 60: _BarometerData1Hr = BmValues; break;
                            case 240: _BarometerData4Hr = BmValues; break;
                            case 1440: _BarometerData1D = BmValues; break;
                        }
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InitWebSocket on.bm: ERROR WebSocket Rx BM json data: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
                    }
                });

                clientSocketIO.On("mpvol", res =>
                {
                    try
                    {
                        // Rxd: [{x:'Binance',p:'ETHBTC',b:'ETH',s:'BTC',v:154273.835,q:3969.31575114,m:358.52324089,e:1587854520,a:'2020-04-26 00:42:00',t:1587940860,d:'2020-04-27 00:41:00',u:'2020-04-27 00:42:12',c:1440},..]
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InitWebSocket on.mpvol: Websocket Rx 'mpvol': " + res.Text.Length + " chars");
                        string jsonData = res.Text.Replace("\"", "");
                        dynamic jsonMarketdata = JsonConvert.DeserializeObject(jsonData);

                        _Marketpairs.Clear();
                        foreach (var marketData in jsonMarketdata)
                        {
                            _Marketpairs.Add(new MarketPairsCMD()
                            {
                                Exchangename = marketData.x,
                                Pairname = marketData.p,
                                BaseCurrency = marketData.b,
                                QuoteCurrency = marketData.s,
                                StartDateTime24Hr = marketData.a,
                                EndDateTime24Hr = marketData.d,
                                NowDateTime = marketData.u,
                                QuoteCurrencyVolume24Hr = marketData.q,
                                BaseCurrencyVolume24Hr = marketData.v,
                                Quote100Last1MinuteVolume = marketData.m,
                                CalculatedMaxBuyAmount = (((Decimal)marketData.m * 14.4m) / 100),
                                CountCandles24Hr = marketData.c
                            });
                        }
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InitWebSocket on.mpvol: ERROR Websocket Rx MPVOL json data: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
                    }
                });

                clientSocketIO.OnClosed += Client_OnClosed;
                clientSocketIO.OnConnected += Client_OnConnected;

                /*clientSocketIO.OnConnected += async () =>
                {
                    Message = "SubscribeNodeBm Connected";
                    Console.WriteLine(Message);
                    // Emit test event, send string.
                    await clientSocketIO.EmitAsync("otherevent", "EmitTest to server!");

                    // Emit test event, send object.
                    //await clientSocketIO.EmitAsync("test", new { code = 200 });
                };*/
                //await clientSocketIO.ConnectAsync();
                Message = "InitWebSocket: Ok.";
            }
            catch (Exception E)
            {
                Message = $"InitWebSocket ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {Message}");
            }
            return Message;
        }

        private void Client_OnConnected()
        {
            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} Client_OnConnected: Connected to server");
        }

        private void Client_OnClosed(ServerCloseReason Reason)
        {
            string Message = "";
            switch (Reason)
            {
                case ServerCloseReason.Aborted: Message = "Aborted, port 30965 open in firewall?"; break;
                case ServerCloseReason.ClosedByClient: Message = "Abort by Client"; break;
                case ServerCloseReason.ClosedByServer: Message = "Abort by Server"; break;
            }
            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} Client_OnClosed: server closed: " + Message);

            //ConnectWebSocket();
        }

        public bool IsWebSocketConnected()
        {
            return (clientSocketIO != null && clientSocketIO.State == SocketIOState.Connected);
        }

        public async Task<string> ConnectWebSocket()
        {
            string Message = "";

            if (!IsWebSocketConnected())
            {
                if (clientSocketIO == null)
                {
                    Message = InitWebSocket();
                }
                await clientSocketIO.ConnectAsync();
                Message += " (Re)Connected.";
            }
            else
            {
                Message = "Already connected.";
            }
            return Message;
        }

        public BarometerCMD getBarometer1Hr
        {
            get
            {
                return _BarometerData1Hr;
            }
        }
        public BarometerCMD getBarometer4Hr
        {
            get
            {
                return _BarometerData4Hr;
            }
        }
        public BarometerCMD getBarometer1D
        {
            get
            {
                return _BarometerData1D;
            }
        }

        public List<MarketPairsCMD> getMarketPairs
        {
            get
            {
                return _Marketpairs;
            }
        }

        public async Task<(string Message, string Content)> getData(string Destination, string Cmd, string UrlQuery)
        {
            string Message = "";
            string ResultContent = "";
            try
            {
                Settings Settings = SettingsStore.Load();
                bool LogActive = false;
                string Uri = "";
                if (!String.IsNullOrEmpty(Cmd))
                {
                    switch (Destination.ToLower())
                    {
                        case "coinmarketdata":
                            Uri = UrlCoinmarketdata + "/" + Cmd + ".php";
                            LogActive = Settings.LogUriOwnAPI;
                            break;
                        case "binance":
                            Uri = UrlBinance + "/" + Cmd;
                            LogActive = Settings.LogUriBinance;
                            break;
                        case "zignaly":
                            Uri = UrlZignaly + "/" + Cmd + ".php";
                            LogActive = Settings.LogUriZignaly;
                            break;
                    }
                }

                if (!String.IsNullOrEmpty(Uri))
                {
                    Uri += (!String.IsNullOrEmpty(UrlQuery) ? $"?{UrlQuery}" : "");

                    /*/HttpResponseMessage response = await _httpclient.GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                    //response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    return content;*/

                    ResultContent = await _httpclient.GetStringAsync(Uri).ConfigureAwait(false);
                    if (LogActive)
                    {
                        string filepath = Settings.DirDataMap + "\\" + "LogGetData";
                        File.AppendAllLines(filepath + $"-{DateTime.Now.ToString("yyMMMdd")}.txt",
                            new string[] { $"=========={System.Environment.NewLine}{DateTime.Now.ToString("ddMMyy.HHmmss")}: {Destination} Uri=[{Uri}]{System.Environment.NewLine}==> ResultContent=[{ResultContent}]" }
                        );
                    }
                }
                else
                {
                    Message = "ERROR getData: No Uri!";
                }
            }
            catch (Exception E)
            {
                Message = $"getData ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} { Message}");
            }
            return (Message, ResultContent);
        }

        public async Task<(string Message, System.Net.HttpStatusCode StatusCode, string Content)> postAsyncData(string Destination, string Cmd, string Data)
        {
            string Message = "";
            string ResultContent = "";
            System.Net.HttpStatusCode ResultCode = System.Net.HttpStatusCode.NotImplemented;
            try
            {
                Settings Settings = SettingsStore.Load();
                bool LogActive = false;
                string Uri = "";
                if (!String.IsNullOrEmpty(Cmd))
                {
                    switch (Destination.ToLower())
                    {
                        case "coinmarketdata":
                            Uri = UrlCoinmarketdata + "/" + Cmd + ".php";
                            LogActive = Settings.LogUriOwnAPI;
                            break;
                        case "binance":
                            Uri = UrlBinance + "/" + Cmd;
                            LogActive = Settings.LogUriBinance;
                            break;
                        case "zignaly":
                            Uri = UrlZignaly + "/" + Cmd + ".php";
                            LogActive = Settings.LogUriZignaly;
                            break;
                    }
                }

                if (!String.IsNullOrEmpty(Uri))
                {
                    HttpResponseMessage Response = await _httpclient.PostAsync(Uri, new StringContent(Data, Encoding.UTF8, "application/x-www-form-urlencoded"));
                    ResultCode = Response.StatusCode;
                    if (ResultCode == System.Net.HttpStatusCode.OK)
                    {
                        //Response.EnsureSuccessStatusCode();
                        ResultContent = await Response.Content.ReadAsStringAsync();
                    }
                    if (Destination.ToLower() == "zignaly") // Tbv TESTEN !
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} postAsyncData to Zignaly: Resultcode={ResultCode.ToString()} Uri=[{Uri}] Postdata=[{Data}]");
                    }
                    if (LogActive)
                    {
                        string filepath = Settings.DirDataMap + "\\" + "LogPostData";
                        //File.AppendAllLines(filepath + $"-{DateTime.Now.ToString("yyMMMdd")}.txt",
                        //    new string[] { $"=========={System.Environment.NewLine}{DateTime.Now.ToString("ddMMyy.HHmmss")}: {Destination} To=[{Uri}] Data=[{Data}]{System.Environment.NewLine}==> ResultCode=[{ResultCode.ToString()}] ResultData=[{ResultContent}]" }
                        //);

                        WriteToFileThreadSafe(
                            filepath + $"-{DateTime.Now.ToString("yyMMMdd")}.txt",
                            $"=========={System.Environment.NewLine}{DateTime.Now.ToString("ddMMyy.HHmmss")}: {Destination} To=[{Uri}] Data=[{Data}]{System.Environment.NewLine}==> ResultCode=[{ResultCode.ToString()}] ResultData=[{ResultContent}]"
                        );
                    }
                }
                else
                {
                    Message = "ERROR postData: No Uri!";
                }
            }
            catch (Exception E)
            {
                Message = $"postAsyncData ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {Message}");
            }
            return (Message, ResultCode, ResultContent);
        }

        private void WriteToFileThreadSafe(string path, string text)
        {
            // Set Status to Locked
            _readWriteLock.EnterWriteLock();
            try
            {
                // Append text to the file
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(text);
                    sw.Close();
                }
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }

        public async Task<(string Message, List<MarketCandleCMD> Candles)> GetCandlesAsyncCMD(string marketSymbols, int periodMinutes, DateTime? startDate = null, DateTime? endDate = null, int? limit = null)
        {
            string Message = "";
            List<MarketCandleCMD> Candles = new List<MarketCandleCMD>();
            try
            {
                string PostData = $"symbols={marketSymbols}&period={periodMinutes}";
                if (startDate != null) PostData += $"&start={startDate}";
                if (endDate != null) PostData += $"&end={endDate}";
                if (limit != null) PostData += $"&limit={limit}";

                var (PostMessage, Poststatus, jsonResult) = await postAsyncData("CoinMarketData", "getcandles", PostData);
                Message = PostMessage;
                //WriteToLogFile(jsonResult, marketSymbol);

                dynamic jsonCandles = JsonConvert.DeserializeObject(jsonResult);
                foreach (var candleData in jsonCandles)
                {
                    Candles.Add(new MarketCandleCMD()
                    {
                        ExchangeName = candleData.a,
                        Name = candleData.s,
                        Timestamp = candleData.d,
                        PeriodSeconds = candleData.i,
                        OpenPrice = candleData.o,
                        HighPrice = candleData.h,
                        LowPrice = candleData.l,
                        ClosePrice = candleData.c,
                        BaseCurrencyVolume = candleData.v,
                        QuoteCurrencyVolume = candleData.q,
                        WeightedAverage = 0
                    });
                }
            }
            catch (Exception E)
            {
                Message = $"GetCandlesAsyncCMD ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {Message}");
            }

            return (Message, Candles);
        }

        public async Task<IEnumerable<MarketPairsCMD>> GetMarketSymbolsAsyncCMD()
        {
            List<MarketPairsCMD> Marketpairs = new List<MarketPairsCMD>();
            try
            {
                var (Message, jsonResult) = await getData("CoinMarketData", "getmarketpairsvolume24h", "");
                //WriteToLogFile(jsonResult, "marketpairs");

                dynamic jsonMarketdata = JsonConvert.DeserializeObject(jsonResult);
                foreach (var marketData in jsonMarketdata)
                {
                    Marketpairs.Add(new MarketPairsCMD()
                    {
                        Exchangename = marketData.x,
                        Pairname = marketData.p,
                        BaseCurrency = marketData.b,
                        QuoteCurrency = marketData.s,
                        StartDateTime24Hr = marketData.a,
                        EndDateTime24Hr = marketData.d,
                        NowDateTime = marketData.u,
                        QuoteCurrencyVolume24Hr = marketData.q,
                        BaseCurrencyVolume24Hr = marketData.v,
                        Quote100Last1MinuteVolume = marketData.m,
                        CalculatedMaxBuyAmount = (((Decimal)marketData.m * 14.4m) / 100),
                        CountCandles24Hr = marketData.c
                    });
                }

                // sort on Pairname
                //Marketpairs = Marketpairs.OrderBy(o => o.Pairname).ToList();
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} GetMarketSymbolAsyncCMD ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
            }
            return Marketpairs;
        }

        public async Task<BarometerCMD> GetBarometerAsyncCMD(int? PeriodMinutes = null)
        {
            try
            {
                if (PeriodMinutes == null) PeriodMinutes = 1;

                var (Message, jsonResult) = await getData("CoinMarketData", "getbm", $"period={PeriodMinutes}");
                WriteToLogFile(jsonResult, "BmValues");

                // 1 record: {s:'BTC',m:1,b:..}
                dynamic BmData = JObject.Parse(jsonResult);
                BarometerCMD BmValues = new BarometerCMD()
                {
                    Quotename = BmData.s,
                    PeriodeMinutes = BmData.m,
                    BmPercentage = BmData.b,
                    PreviousSum = BmData.p,
                    NowSum = BmData.n,
                    PreviousDatetime = BmData.a,
                    NowDatetime = BmData.d,
                    PairCount = BmData.c
                };

                return BmValues;
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} GetBarometerAsyncCMD ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
                return null;
            }
        }

        private void WriteToLogFile(string Text, string Symbol)
        {
            Settings Settings = SettingsStore.Load();
            try
            {
                switch (Settings.WriteLogFileType)
                {
                    case "Log4Net":
                        break;
                    case "StreamWriter":
                        string filename = Settings.DirDataMap + $"\\last_candledata_{Symbol}.txt";
                        File.WriteAllText(filename, Text);
                        break;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} WriteToLogFile ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
            }
        }

    }
}
