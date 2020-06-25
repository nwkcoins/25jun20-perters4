using Binance.Net.Objects;
using System;

namespace CryptoTraderStandard.BinanceNetApi
{
    public class OrderBIN
    {
        public OrderBIN()
        {
            Id = 0;
            Symbol = String.Empty;
            Valid = false;
            Message = "";
            ExecutedQuantity = OriginalQuantity = Price = Decimal.Zero;
            Side = OrderSide.Buy;
            Status = OrderStatus.Canceled;
            Symbol = "";
            Time = DateTime.MinValue;
            Type = OrderType.Limit;
        }

        public long Id { get; set; }
        public string Symbol { get; set; }
        public bool Valid { get; set; }
        public string Message { get; set; }
        public decimal ExecutedQuantity { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal Price { get; set; }
        public OrderSide Side { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime Time { get; set; }
        public OrderType Type { get; set; }
    }
}
