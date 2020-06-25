using System;

namespace CryptoTraderStandard.Indicators
{
    public class StochRSIIND
    {
        public StochRSIIND()
        {
            // Initial / Default values
            //SRSI = RSIValue = rsi = Decimal.Zero;
            KValue = DValue = Decimal.Zero;
            //DValue1 = DValue2 = DValue3 = Decimal.Zero;
        }

        //public Decimal SRSI { get; set; }
        //public Decimal RSIValue { get; set; }
        public Decimal KValue { get; set; }
        public Decimal DValue { get; set; }
        //public Decimal DValue1 { get; set; }
        //public Decimal DValue2 { get; set; }
        //public Decimal DValue3 { get; set; }
        //public Decimal rsi { get; set; }
    }
}
