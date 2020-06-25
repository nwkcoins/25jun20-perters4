﻿using CryptoTraderScanner.Bot;
//using ExchangeSharp;
using CryptoTraderStandard.coinmarketdataApi;
using System;
using System.Collections.Generic;

namespace CryptoTraderScanner.Backtest
{
    public class VirtualTradeManager : ITradeManager
    {
        private const decimal FeesPercentage = 0.2m;// 0.1m;  // 0.2% fees

        public VirtualTradeManager(decimal initalAccountBalance = 1000)
        {
            AccountBalance = initalAccountBalance;
            Trades = new List<ITrade>();
        }

        /// <summary>
        /// Open a new buy order at market price
        /// </summary>
        /// <returns>The trade</returns>
        /// <param name="symbol">Symbol</param>
        /// <param name="amountOfCoins">Amount of coins to buy</param>
        public ITrade BuyMarket(string symbol, decimal amountOfCoins)
        {
            var investment = amountOfCoins * Candle.ClosePrice;
            return new VirtualTrade()
            {
                Symbol = symbol,
                TradeType = TradeType.Long,
                Coins = amountOfCoins,
                Investment = investment,
                OpenDate = Candle.Timestamp,
                OpenPrice = Candle.ClosePrice,
                CloseDate = DateTime.MinValue,
                ClosePrice = 0,
                ProfitDollars = 0,
                ProfitPercentage = 0,
                FeesPaid = (FeesPercentage / 100m) * (investment),
                InitialInvestment = investment,
                InitialCoins = amountOfCoins
            };
        }

        /// <summary>
        /// Open a new sell order at market price
        /// </summary>
        /// <returns>The trade</returns>
        /// <param name="symbol">Symbol</param>
        /// <param name="amountOfCoins">Amount of coins to sell</param>
        public ITrade SellMarket(string symbol, decimal amountOfCoins)
        {
            var investment = amountOfCoins * Candle.ClosePrice;
            return new VirtualTrade()
            {
                Symbol = symbol,
                TradeType = TradeType.Short,
                Coins = amountOfCoins,
                Investment = investment,
                OpenDate = Candle.Timestamp,
                OpenPrice = Candle.ClosePrice,
                CloseDate = DateTime.MinValue,
                ClosePrice = 0,
                ProfitDollars = 0,
                ProfitPercentage = 0,
                FeesPaid = (FeesPercentage / 100m) * (investment),
                InitialInvestment = investment,
                InitialCoins = amountOfCoins
            };
        }


        /// <summary>
        /// Increase the trade by buying more coins
        /// </summary>
        /// <returns><c>true</c>, if more was bought, <c>false</c> otherwise.</returns>
        /// <param name="trade">Trade.</param>
        /// <param name="coins">amount of coins to buy</param>
        public bool BuyMore(ITrade trade, decimal coins)
        {
            var vtrade = (VirtualTrade)trade;
            var investment = coins * Candle.ClosePrice;
            vtrade.Investment += investment;
            vtrade.Coins += coins;
            vtrade.FeesPaid += (FeesPercentage / 100m) * investment;

            var rebuy = new VirtualRebuy()
            {
                Date = Candle.Timestamp,
                Coins = coins,
                Investment = investment,
                Price = Candle.ClosePrice
            };
            vtrade.Rebuys.Add(rebuy);

            return true;
        }

        /// <summary>
        /// Increase the trade by selling more coins.
        /// </summary>
        /// <returns><c>true</c>, if more was sold, <c>false</c> otherwise.</returns>
        /// <param name="trade">Trade.</param>
        /// <param name="coins">Coins.</param>
        public bool SellMore(ITrade trade, decimal coins)
        {
            var vtrade = (VirtualTrade)trade;
            var investment = coins * Candle.ClosePrice;
            vtrade.Investment += investment;
            vtrade.Coins += coins;
            vtrade.FeesPaid += (FeesPercentage / 100m) * investment;

            var rebuy = new VirtualRebuy()
            {
                Date = Candle.Timestamp,
                Coins = coins,
                Investment = investment,
                Price = Candle.ClosePrice
            };
            vtrade.Rebuys.Add(rebuy);
            return true;
        }

        /// <summary>
        /// Close the specified trade.
        /// </summary>
        /// <returns>The close.</returns>
        /// <param name="trade">Trade.</param>
        public bool Close(ITrade trade, decimal closePrice)
        {
            trade.CloseDate = Candle.Timestamp;
            trade.ClosePrice = closePrice;

            trade.FeesPaid += (FeesPercentage / 100m) * trade.Investment;
            trade.ProfitDollars = (trade.Coins * trade.ClosePrice) - trade.Investment;

            if (trade.TradeType == TradeType.Short)
            {
                trade.ProfitDollars = -trade.ProfitDollars;
            }

            trade.ProfitDollars -= trade.FeesPaid;
            trade.ProfitPercentage = (trade.ProfitDollars / trade.Investment) * 100m;
            Trades.Add(trade);
            AccountBalance += trade.ProfitDollars;
            return true;
        }

        /// <summary>
        /// Gets the account balance.
        /// </summary>
        /// <value>The account balance.</value>
        public decimal AccountBalance { get; set; }

        /// <summary>
        /// Trade history
        /// </summary>
        /// <value>List of all trades done.</value>
        public List<ITrade> Trades { get; set; }

        public MarketCandleCMD Candle { get; set; }

        /// <summary>
        /// Write statistics for all trades to the console
        /// </summary>
        public void DumpStatistics()
        {
            // Show statistics
            double winners = 0;
            double losers = 0;
            double totalProfit = 0;
            double totalMinutes = 0;
            double reBuys0 = 0;
            double reBuys1 = 0;
            double reBuys2 = 0;
            double reBuys3 = 0;
            double reBuys4 = 0;
            double maxProfit = 0;
            double maxLoss = 0;
            double shorts = 0;
            double longs = 0;
            var shortestTrade = TimeSpan.MaxValue;
            var longestTrade = TimeSpan.MinValue;

            foreach (var trade in Trades)
            {
                var vtrade = (VirtualTrade)trade;
                var profit = (double)trade.ProfitPercentage;
                totalProfit += profit;
                if (profit < 0)
                {
                    losers++;
                    if (profit < maxLoss) maxLoss = profit;
                }
                else
                {
                    winners++;
                    if (profit > maxProfit) maxProfit = profit;
                }

                var duration = (trade.CloseDate - trade.OpenDate);
                totalMinutes += duration.TotalMinutes;
                if (duration < shortestTrade) shortestTrade = duration;
                if (duration > longestTrade) longestTrade = duration;

                if (trade.Rebuys.Count == 0) reBuys0++;
                else if (trade.Rebuys.Count == 1) reBuys1++;
                else if (trade.Rebuys.Count == 2) reBuys2++;
                else if (trade.Rebuys.Count == 3) reBuys3++;
                else if (trade.Rebuys.Count == 4) reBuys4++;

                if (trade.TradeType == TradeType.Long) longs++;
                else shorts++;
            }

            longs = 100.0 * (longs / Trades.Count);
            shorts = 100.0 * (shorts / Trades.Count);

            reBuys0 = 100.0 * (reBuys0 / Trades.Count);
            reBuys1 = 100.0 * (reBuys1 / Trades.Count);
            reBuys2 = 100.0 * (reBuys2 / Trades.Count);
            reBuys3 = 100.0 * (reBuys3 / Trades.Count);
            reBuys4 = 100.0 * (reBuys4 / Trades.Count);

            double winPercentage = winners / (winners + losers);
            winPercentage *= 100.0;
            totalMinutes /= Trades.Count;

            var averageTime = TimeSpan.FromMinutes(totalMinutes);
            var averageProfit = totalProfit / Trades.Count;

            Console.WriteLine($"Trades                : {Trades.Count} trades");
            Console.WriteLine($"Winners               : {winners} trades");
            Console.WriteLine($"Losers                : {losers} trades");
            Console.WriteLine($"Win %                 : {winPercentage:0.00} %");

            Console.WriteLine($"Trades with no rebuys : {reBuys0:0.00} %");
            Console.WriteLine($"Trades with 1 rebuy   : {reBuys1:0.00} %");
            Console.WriteLine($"Trades with 2 rebuys  : {reBuys2:0.00} %");
            Console.WriteLine($"Trades with 3 rebuys  : {reBuys3:0.00} %");
            Console.WriteLine($"Trades with 4 rebuys  : {reBuys4:0.00} %");

            Console.WriteLine($"Trades long           : {longs:0.00} %");
            Console.WriteLine($"Trades shorts         : {shorts:0.00} %");

            Console.WriteLine($"Average profit/trade  : {averageProfit:0.00} %");
            Console.WriteLine($"Max profit            : {maxProfit:0.00} %");
            Console.WriteLine($"Max loss              : {maxLoss:0.00} %");
            Console.WriteLine($"Average time/trade    : {averageTime}");
            Console.WriteLine($"Shortest trade        : {shortestTrade}");
            Console.WriteLine($"Longest trade         : {longestTrade}");
        }
    }
}
