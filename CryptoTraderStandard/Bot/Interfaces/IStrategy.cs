//using ExchangeSharp;

namespace CryptoTraderScanner.Bot
{
    public interface IStrategy
    {
        /// <summary>
        /// Perform strategy
        /// </summary>
        /// <param name="candles">history of candles</param>
        /// <param name="candle">current candle</param>
        /// <param name="tradeManager">tradeManager</param>
        /// <param name="Message">message</param>
    //    void OnCandle(List<MarketCandleCMD> candles, int candle, ITradeManager tradeManager, MarketPairsCMD MarketPair, out string Message);

        /// <summary>
        /// Checks if a valid entry appeard
        /// </summary>
        /// <returns><c>true</c>, if valid entry was found, <c>false</c> otherwise.</returns>
        /// <param name="candles">History of candles</param>
        /// <param name="candle">The current candle</param>
        /// <param name="tradeType">returns trade type.</param>
        /// <param name="Message">message</param>
    //    bool IsValidEntry(List<MarketCandleCMD> candles, int candle, MarketPairsCMD MarketPair, out TradeType tradeType, out string Message);

        /// <summary>
        /// Symbol to test
        /// </summary>
        /// <value>The symbol.</value>
	//	string Symbol { get; }
    }
}
