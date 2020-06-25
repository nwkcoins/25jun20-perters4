namespace CryptoTraderScanner
{
    public class BackTester
    {
        //private Cache _cache = new Cache();

        /// <summary>
        /// Downloads 5min. candle sticks data for the symbol from 1/1/2018 until now
        /// </summary>
        /// <returns>list of 5m candle sticks</returns>
        /// <param name="api">ExchangeBinanceAPI</param>
        /// <param name="symbol">symbol to get the candlesticks from</param>
        /*private List<MarketCandleCMD> DownloadCandlesFromExchange(ExchangeAPI api, string symbol, DateTime startDate, DateTime endDate)
		{
			// get candle stick data
			Console.WriteLine($"Downloading 5m candlesticks for {symbol} from {api.Name}");
			var allCandles = new List<MarketCandleCMD>();
			while (true)
			{
/ *				try 
				{
					var candles = api.GetCandlesAsyncs(symbol, 5 * 60, startDate, DateTime.Now, 1000).ToList();
					if (candles == null) break;
					if (candles.Count == 0) break;
					allCandles.AddRange(candles);
					startDate = candles.Last().Timestamp;
					Console.WriteLine($"date:{candles[0].Timestamp} - {startDate}  / total:{allCandles.Count}");
					if (candles.Count < 1000) break;
					if (startDate > endDate) break;
				}
				catch (Exception)
* /				{
					Console.WriteLine("-");
				}

				//if (allCandles.Count > 4000) break;
			}
			allCandles.Reverse();
			return allCandles;
		}*/

        /// <summary>
        /// Backtest strategy for the specified symbol.
        /// </summary>
        /// <param name="api">ExchangeBinanceAPI</param>
        /// <param name="strategy">strategy to test</param>
        /*public void Test(ExchangeAPI api, IStrategy strategy, DateTime startTime)
		{
		    string Errormessage = "";
			// Get candle sticks
			var allCandles = _cache.Load(strategy.Symbol);
			if (allCandles == null)
			{
				allCandles = DownloadCandlesFromExchange(api, strategy.Symbol, startTime, DateTime.Now);

				_cache.Save(strategy.Symbol, allCandles);
			}

            // convert from utc->local date/teim
			foreach (var candle in allCandles) candle.Timestamp = candle.Timestamp.AddHours(2);


			// backtest strategy
			var virtualTradeManager = new VirtualTradeManager();
			for (int i = allCandles.Count - 50; i > 0; i--)
			{
				virtualTradeManager.Candle = allCandles[i];
				strategy.OnCandle(allCandles, i, virtualTradeManager, out Errormessage);
				if (ErrorMessage.Length > 0) {
					Console.WriteLine($"ERROR: {ErrorMessage}");
					ErrorMessage = "";
				}
			}
            

            // dump all trades to console
			foreach (var trade in virtualTradeManager.Trades)
			{
				((VirtualTrade)trade).Dump();
			}

			// show results
			Console.WriteLine("");
			Console.WriteLine("Backtesting results");
			Console.WriteLine($"Symbol                : {strategy.Symbol} on {api.Name}");
			Console.WriteLine($"Period                : {allCandles[allCandles.Count - 1].Timestamp:dd-MM-yyyy HH:mm} / {allCandles[0].Timestamp:dd-MM-yyyy HH:mm}");

			Console.WriteLine($"Starting capital      : $ 1000");
			Console.WriteLine($"Ending capital        : $ {virtualTradeManager.AccountBalance:0.00}");
			virtualTradeManager.DumpStatistics();
			Console.ReadLine();
		}*/
    }
}
