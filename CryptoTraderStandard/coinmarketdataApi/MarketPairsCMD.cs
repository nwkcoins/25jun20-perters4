using System;

namespace CryptoTraderStandard.coinmarketdataApi
{
    public class MarketPairsCMD
    {
        public MarketPairsCMD()
        {
            // Initial / Default values
            Exchangename = Pairname = BaseCurrency = QuoteCurrency = "";
            StartDateTime24Hr = EndDateTime24Hr = NowDateTime = DateTime.MinValue;
            QuoteCurrencyVolume24Hr = BaseCurrencyVolume24Hr = Decimal.Zero;
            CountCandles24Hr = 0;
        }

        public string Exchangename { get; set; }
        public string Pairname { get; set; }
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set; }
        public DateTime StartDateTime24Hr { get; set; }
        public DateTime EndDateTime24Hr { get; set; }
        public DateTime NowDateTime { get; set; }
        public Decimal QuoteCurrencyVolume24Hr { get; set; }
        public Decimal BaseCurrencyVolume24Hr { get; set; }
        public Decimal Quote100Last1MinuteVolume { get; set; }
        public Decimal CalculatedMaxBuyAmount { get; set; }
        public int CountCandles24Hr { get; set; }
    }
}
