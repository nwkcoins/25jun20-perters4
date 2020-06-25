namespace CryptoTraderScanner
{
    /*public class Signal
    {
        public Signal()
        {
            Date = TimeframeName = Exchange = Symbol = Message = "";
            Strategy = Strategies.None;
            Trade = Trades.None;
            Id = TimeframeMinutes = 0;
            Valid = false;
            BollingerBandwidth = RSI = ClosePrice = BuyPrice = LimitPrice = TakeProfit = StopLoss = Decimal.Zero;
            MFI = 0d;
        }

        public enum SignalStates
        {
            New = 0,                 // Signal is new created/generated; if Valid=true then ready for Exchange
            SendToExchange = 1,      // Signal is send to Exchange (ie Zignaly), waiting to be filled / processed
            Rejected = 2,            // Signal is not accepted by Exchange
            InProcessAtExchange = 3, // Signal is accepted, InProcess at Exchange (at least partially filled till done): see Order(s)
            Canceled = 4,            // Signal is removed from Exchange, without being processed
            CompletedProfit = 5,     // Signal has been processed at Exchange: with profit (green)
            CompletedLoss = 6,       // Signal has been processed at Exchange: with loss (red)
        }
        public enum Strategies
        {
            None = 0,

        }
        public enum Trades
        {
            None = 0,

        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string TimeframeName { get; set; }
        public int TimeframeMinutes { get; set; }
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public bool Valid { get; set; }
        public string Message { get; set; }
        public Strategies Strategy { get; set; }
        public Trades Trade { get; set; }
        public Decimal BollingerBandwidth { get; set; }
        public Decimal RSI { get; set; }
        public decimal MFI { get; set; }
        public Decimal ClosePrice { get; set; } // was Price1
        public Decimal BuyPrice { get; set; }
        public Decimal LimitPrice { get; set; }
        public Decimal TakeProfit { get; set; }
        public Decimal StopLoss { get; set; }
        public Decimal Bm1Hr { get; set; }
        public Decimal Bm4Hr { get; set; }
        public Decimal Bm1Day { get; set; }
        public Decimal Quote100Volume { get; set; }
        public Decimal MaxBuyAmount { get; set; }
        public Decimal PositionSize { get; set; }


        public string GetCsvHeaderRow()
        {
            string[] HeaderRow = {
                "Datum",
                "Periode",
                "Strategie",
                "Pair",
                "Geldig",
                "Koopprijs",
                "Limitprijs",
                "Stoploss",
                "ClosePrice",
                "BollingerBandwidth",
                "RSI",
                "MFI",
                "Opmerking"
            };
            return string.Join(";", HeaderRow);
        }

        public string GetCsvDataRow()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            string[] DataRow = {
                Date,
                TimeframeName,
                Strategy,
                Symbol.ToUpper(),
                (Valid ? "1" : "0"),
                BuyPrice.ToString(nfi),
                LimitPrice.ToString(nfi),
                StopLoss.ToString(nfi),
                ClosePrice.ToString(nfi),
                BollingerBandwidth.ToString(nfi),
                RSI.ToString(nfi),
                MFI.ToString(nfi),
                Message
            };
            return string.Join(";", DataRow);
        }
    }*/
}