using System;
using System.Globalization;

namespace CryptoTraderStandard.ScannerSQLiteDB
{
    public class SignalsTBL
    {
        public SignalsTBL()
        {
            StartDateTime = UpdateDateTime = DateTime.MinValue;
            SignalState = SignalStates.None;
            SignalAction = SignalActions.None;
            TimeframeName = Exchange = Symbol = Message = "";
            Strategy = Strategies.None;
            TradeType = TradeTypes.None;
            Id = ExchangeId = FirstBuyId = BuyCount = CandleCount = TimeframeMinutes = 0;
            Valid = false;
            BBWidth = MFI = RSI = RSIK = RSID = ClosePrice = BuyPrice = TakeProfit = StopLoss = Bm1Hr = Bm4Hr = Bm1Day = Quote100Volume = MaxBuyAmount = PositionSize = Decimal.Zero;
        }

        public enum SignalStates
        {
            None = 0,                // Not set
            New = 1,                 // Signal is new created/generated; if Valid=true then ready for Exchange
            SendToExchange = 2,      // Signal is send to Exchange (ie Zignaly), waiting to be filled / processed
            Rejected = 3,            // Signal is not accepted by Exchange
            InProcessAtExchange = 4, // Signal is accepted, InProcess at Exchange (at least partially filled till done): see Order(s)
            Canceled = 5,            // Signal is removed from Exchange, without being processed
            CompletedProfit = 6,     // Signal has been processed at Exchange: with profit (green)
            CompletedLoss = 7,       // Signal has been processed at Exchange: with loss (red)
            CompletedAutoClose = 8,  // Signal has been processed at Exchange: autoclosed after buy timeout
            Completed = 9            // Signal has been processed at Exchange: for the Rebought and Bought Signals (the Sell is a seperate signal with same FirstBuyId with CompletedProfit | -Loss)
        }

        public enum SignalActions
        {
            None = 0,
            Buy = 1,
            Bought = 2,
            Sell = 3,
            Sold = 4,
            Rebuy = 5,
            Rebought = 6,
            RebuyStoploss = 15,
            ReboughtStoplossed = 16,
            Stoploss = 13,
            Stoplossed = 14,
            Update = 7,
            Updated = 8,
            Panicsell = 9,
            Panicsold = 10,
            SellAutoclose = 11,
            SoldAutoclose = 12
        }

        public enum Strategies
        {
            None = 0,
            MiddleUp = 1,
            BottomUp = 2,
            Family = 3,
        }
        public enum TradeTypes
        {
            None = 0,
            Long = 1,
            Short = 2
        }


        public int Id { get; set; }
        public int FirstBuyId { get; set; }
        public int ExchangeId { get; set; }
        public int BuyCount { get; set; }
        public SignalStates SignalState { get; set; }
        public SignalActions SignalAction { get; set; }
        public Strategies Strategy { get; set; }
        public TradeTypes TradeType { get; set; }
        public string Symbol { get; set; }
        public string TimeframeName { get; set; }
        public Decimal BuyPrice { get; set; }
        public Decimal ClosePrice { get; set; } // was Price1
        public Decimal TakeProfit { get; set; }
        public Decimal StopLoss { get; set; }
        public Decimal MaxBuyAmount { get; set; }
        public Decimal PositionSize { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public string Message { get; set; }
        public int TimeframeMinutes { get; set; }
        public string Exchange { get; set; }
        public int CandleCount { get; set; }
        public Decimal Quote100Volume { get; set; }
        public bool Valid { get; set; }
        public Decimal BBLower { get; set; }
        public Decimal BBMiddle { get; set; }
        public Decimal BBUpper { get; set; }
        public Decimal BBWidth { get; set; }
        public Decimal MFI { get; set; }
        public Decimal RSI { get; set; }
        public Decimal RSIK { get; set; }
        public Decimal RSID { get; set; }
        public Decimal StochK { get; set; }
        public Decimal StochD { get; set; }
        public Decimal MACD { get; set; }
        public Decimal MACDSignal { get; set; }
        public Decimal MACDHistogram { get; set; }
        public Decimal Bm1Hr { get; set; }
        public Decimal Bm4Hr { get; set; }
        public Decimal Bm1Day { get; set; }

        public override string ToString()
        {
            return $"Id={Id} FBId={FirstBuyId} ExchId={ExchangeId} BuyCount={BuyCount} State={(int)SignalState}={SignalState.ToString()} Action={(int)SignalAction}={SignalAction.ToString()} Symbol={Symbol} TF={TimeframeName}"
                +$" BP={BuyPrice} CL={ClosePrice} TP={TakeProfit} SL={StopLoss} Start={StartDateTime.ToString("ddMMyy-HHmm")} Upd={UpdateDateTime.ToString("ddMMyy-HHmm")} Msg={Message}";
        }

        public string GetCsvHeaderRow()
        {
            string[] HeaderRow = {
                    "Datum",
                    "Periode",
                    "Strategie",
                    "Pair",
                    "Geldig",
                    "Koopprijs",
                    "Takeprofit",
                    "Stoploss",
                    "ClosePrice",
                    "BBWidth",
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
                    StartDateTime.ToString(),
                    TimeframeName,
                    Strategy.ToString(),
                    Symbol.ToUpper(),
                    (Valid ? "1" : "0"),
                    BuyPrice.ToString(nfi),
                    TakeProfit.ToString(nfi),
                    StopLoss.ToString(nfi),
                    ClosePrice.ToString(nfi),
                    BBWidth.ToString(nfi),
                    RSI.ToString(nfi),
                    MFI.ToString(nfi),
                    Message
                };
            return string.Join(";", DataRow);
        }
    }
}
