using CryptoTraderStandard.ScannerSQLiteDB;
using System;

namespace CryptoTraderStandard.Strategy
{
    public class StrategyDTA
    {
        public StrategyDTA()
        {
            Symbol = Message = "";
            Valid = false;
            BolingerBandwidth = MFI = RSI = ClosePrice = BuyPrice = TakeProfit = StopLoss = PositionSize = Decimal.MinValue;
            SignalState = SignalsTBL.SignalStates.None;
            Strategy = SignalsTBL.Strategies.None;
            TradeType = SignalsTBL.TradeTypes.None;
            SignalAction = SignalsTBL.SignalActions.None;
        }

        public string Symbol { get; set; }
        public bool Valid { get; set; }
        public string Message { get; set; }
        public SignalsTBL.SignalStates SignalState { get; set; }
        public SignalsTBL.Strategies Strategy { get; set; }
        public SignalsTBL.TradeTypes TradeType { get; set; }
        public SignalsTBL.SignalActions SignalAction { get; set; }
        public Decimal BolingerBandwidth { get; set; }
        public Decimal RSI { get; set; }
        public Decimal MFI { get; set; }
        public Decimal ClosePrice { get; set; }
        public Decimal BuyPrice { get; set; }
        public Decimal TakeProfit { get; set; }
        public Decimal StopLoss { get; set; }
        public Decimal PositionSize { get; set; }

    }
}
