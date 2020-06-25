using System;

namespace CryptoTraderStandard.Indicators
{
    public class StochasticsIND
    {
        public StochasticsIND()
        {
            KValue = DValue = Decimal.Zero;
            //ClosePrice = LowPrice = HighPrice = Decimal.Zero;
        }

        //public Decimal ClosePrice { get; set; }
        //public Decimal LowPrice { get; set; }
        //public Decimal HighPrice { get; set; }
        public Decimal KValue { get; set; }
        public Decimal DValue { get; set; }
    }
}
