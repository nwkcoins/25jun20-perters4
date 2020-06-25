using System;
namespace CryptoTraderScanner.Bot.Interfaces
{
    public interface IRebuy
    {
        decimal Investment { get; set; }
        decimal Coins { get; set; }
        decimal Price { get; set; }
        DateTime Date { get; set; }
    }
}
