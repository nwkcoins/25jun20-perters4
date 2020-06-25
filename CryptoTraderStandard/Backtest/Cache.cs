//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;

namespace CryptoTraderScanner
{
    [MessagePackObject]
    public class CacheCandle
    {
        [Key(0)]
        public DateTime Date { get; set; }
        [Key(1)]
        public decimal Open { get; set; }
        [Key(2)]
        public decimal Close { get; set; }
        [Key(3)]
        public decimal High { get; set; }
        [Key(4)]
        public decimal Low { get; set; }

        public CacheCandle()
        {
        }

        public CacheCandle(MarketCandleCMD candle)
        {
            Date = candle.Timestamp;
            Open = candle.OpenPrice;
            Close = candle.ClosePrice;
            High = candle.HighPrice;
            Low = candle.LowPrice;
        }

        public MarketCandleCMD ToMarketCandleCMD()
        {
            return new MarketCandleCMD()
            {
                Timestamp = Date,
                OpenPrice = Open,
                ClosePrice = Close,
                HighPrice = High,
                LowPrice = Low
            };
        }
    }

    [MessagePackObject]
    public class CacheCandleList
    {
        [Key(0)]
        public CacheCandle[] Candles;
    }

    public class Cache
    {
        public List<MarketCandleCMD> Load(string symbol)
        {
            if (File.Exists($"{symbol}-candles.dat"))
            {
                // load candles from disk
                using (var file = File.OpenRead($"{symbol}-candles.dat"))
                {
                    var bytes = new byte[file.Length];
                    file.Read(bytes, 0, bytes.Length);
                    var cache = MessagePackSerializer.Deserialize<CacheCandleList>(bytes);

                    var result = new List<MarketCandleCMD>();
                    foreach (var candle in cache.Candles)
                    {
                        result.Add(candle.ToMarketCandleCMD());
                    }
                    return result;
                }
            }
            return null;
        }

        public void Save(string symbol, List<MarketCandleCMD> marketCandlesCMD)
        {
            // store candles on disk
            var cache = new CacheCandleList();
            int idx = 0;
            cache.Candles = new CacheCandle[marketCandlesCMD.Count];
            foreach (var candle in marketCandlesCMD)
            {
                cache.Candles[idx++] = new CacheCandle(candle);

            }
            var bytes = MessagePackSerializer.Serialize(cache);
            using (var file = File.OpenWrite($"{symbol}-candles.dat"))
            {
                file.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
