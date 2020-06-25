using System;

namespace CryptoTraderStandard.Indicators
{
    public class StochIND
    {
        public StochIND()
        {
            // Initial / Default values
            FastK = FastD = 0d;
        }

        public Double FastK { get; set; }
        public Double FastD { get; set; }
    }
}
