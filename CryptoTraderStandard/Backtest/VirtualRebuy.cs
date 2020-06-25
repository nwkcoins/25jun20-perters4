using CryptoTraderScanner.Bot.Interfaces;
using System;

namespace CryptoTraderScanner.Backtest
{
    public class VirtualRebuy : IRebuy
    {
        public decimal Investment { get; set; }
        public decimal Coins { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }
}
