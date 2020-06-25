using System;

namespace CryptoTraderStandard.coinmarketdataApi
{
    public class MarketCandleCMD
    {
        public MarketCandleCMD()
        {
            // Initial / Default values
            ExchangeName = Name = "";
            Timestamp = DateTime.MinValue;
            PeriodSeconds = 0;
            OpenPrice = ClosePrice = LowPrice = HighPrice = BaseCurrencyVolume = QuoteCurrencyVolume = Decimal.Zero;
            WeightedAverage = Decimal.Zero;
        }

        public enum TimeFrames
        {
            Min1 = 0,
            Min3 = 1,
            Min5 = 2,
            Min15 = 3,
            Min30 = 4,
            Hr1 = 5,
            Hr4 = 6,
            Day1 = 7
        }

        public string ExchangeName { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public int PeriodSeconds { get; set; }
        public Decimal OpenPrice { get; set; }
        public Decimal HighPrice { get; set; }
        public Decimal LowPrice { get; set; }
        public Decimal ClosePrice { get; set; }
        public Decimal BaseCurrencyVolume { get; set; }
        public Decimal QuoteCurrencyVolume { get; set; }
        public Decimal WeightedAverage { get; set; }

        //public override string ToString();
    }
}
