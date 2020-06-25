using System;

namespace CryptoTraderStandard.Indicators
{
    public class BollingerBandsIND
    {
        public BollingerBandsIND()
        {
            // Initial / Default values
            Bandwidth = Lower = Middle = Upper = Decimal.Zero;
            //MiddleUp = Middle1 = Middle2 = Middle3 = Middle4 = Decimal.Zero;
        }

        public Decimal Bandwidth { get; set; }
        public Decimal Lower { get; set; }
        public Decimal Middle { get; set; }
        //public Decimal Middle1 { get; set; }
        //public Decimal Middle2 { get; set; }
        //public Decimal Middle3 { get; set; }
        //public Decimal Middle4 { get; set; }
        public Decimal Upper { get; set; }
        //public Decimal MiddleUp { get; set; }
    }
}
