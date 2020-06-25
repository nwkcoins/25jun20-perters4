using System;

namespace CryptoTraderStandard.Indicators
{
    public class MacdIND
    {
        public MacdIND()
        {
            MacdValue = MacdSignal = MacdHistogram = Decimal.Zero;
        }

        public Decimal MacdValue { get; set; }
        public Decimal MacdSignal { get; set; }
        public Decimal MacdHistogram { get; set; }
    }
}
