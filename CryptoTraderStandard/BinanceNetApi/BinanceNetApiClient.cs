using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using CryptoTraderScanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;

namespace CryptoTraderStandard.BinanceNetApi
{
    public class BinanceNetApiClient
    {
        BinanceClient httpBinanceClient = new BinanceClient();

        public enum BinancePlaceOrderTypes
        {
            MarginOrder,
            MarginOrderAsync,
            OCOOrder,
            OCOOrderAsync,
            Order,
            OrderAsync,
            TestOrder,
            TestOrderAsync
        }

        public BinanceNetApiClient()
        {
            //
        }

        public void SetApiCredentials(string ApiKey, string ApiSecret)
        {
            httpBinanceClient.SetApiCredentials(ApiKey, ApiSecret);
        }

        public void PlaceOrder(BinancePlaceOrderTypes OrderType, string PostData, out string Message)
        {
            Message = "";
            try
            {
                Settings Settings = SettingsStore.Load();
                if (!String.IsNullOrEmpty(Settings.ApikeyBinance) && !String.IsNullOrEmpty(Settings.ApiSecretBinance))
                {
                    SetApiCredentials(Settings.ApikeyBinance, Settings.ApiSecretBinance);

                    /*using (var client = httpBinanceClient)
                    {
                        switch (OrderType)
                        {
                            case BinancePlaceOrderTypes.TestOrderAsync: WebCallResult<BinancePlacedOrder> result = client.PlaceTestOrderAsync(); break;
                        }
                    }*/
                    Message = "To be Implemented!";
                }
                else
                {
                    Message = $"ERROR PlaceOrder: No Binance API Key/Secret set!";
                }
            }
            catch (Exception E)
            {
                Message = $"ERROR PlaceOrder: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }
        }

        public string ConvertDust2BNB(out string Message)
        {
            Message = "";
            string ConvertDust2BNBResult = "";

            try
            {
                BinanceAccountInfo AccountInfo = _GetAccountInfo(out Message);
                if (String.IsNullOrEmpty(Message))
                {
                    string[] Assets = AccountInfo.Balances.Where(b => ((b.Total > 0) && (!"BTC,BNB".Contains(b.Asset.ToUpper())))).Select(b => b.Asset).ToArray();
                    if (Assets.Length > 0)
                    {
                        Settings Settings = SettingsStore.Load();
                        SetApiCredentials(Settings.ApikeyBinance, Settings.ApiSecretBinance);

                        using (var client = httpBinanceClient)
                        {
                            WebCallResult<BinanceDustTransferResult> DustTransferResult = client.DustTransfer(Assets);
                            if (DustTransferResult.Success)
                            {
                                BinanceDustTransferResult DustResult = DustTransferResult.Data;
                                ConvertDust2BNBResult = $"Success: Result={DustResult.TransferResult} Total={DustResult.TotalTransferred} Charge={DustResult.TotalServiceCharge}";
                            }
                            else
                            {
                                Message = $"ConvertDust2BNB: Error {DustTransferResult.Error.Message}";
                            }
                        }
                    }
                    else
                    {
                        Message = $"INFO ConvertDust2BNB: No Assets to convert (excluding BNB, BTC)";
                    }
                }
                else
                {
                    Message = $"ERROR ConvertDust2BNB: GetAccountInfo: {Message}";
                }
            }
            catch (Exception E)
            {
                Message = $"ERROR ConvertDust2BNB: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return ConvertDust2BNBResult;
        }

        public Decimal GetCoinAmount(string Symbol, out string Message)
        {
            Message = "";
            Decimal CoinAmount = Decimal.Zero;

            try
            {
                BinanceAccountInfo AccountInfo = _GetAccountInfo(out Message);
                if (String.IsNullOrEmpty(Message))
                {
                    BinanceBalance Asset = AccountInfo.Balances.SingleOrDefault(balance => balance.Asset.ToUpper() == Symbol.ToUpper());
                    if (Asset != null)
                    {
                        CoinAmount = Asset.Total;
                    }
                }
                else
                {
                    Message = $"ERROR GetCoinAmount: GetAccountInfo: {Message}";
                }
            }
            catch (Exception E)
            {
                Message = $"ERROR getCoinAmount: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return CoinAmount;
        }

        public BinanceAccountInfo _GetAccountInfo(out string Message)
        {
            Message = "";
            BinanceAccountInfo AccountInfo = new BinanceAccountInfo();

            try
            {
                Settings Settings = SettingsStore.Load();
                if (!String.IsNullOrEmpty(Settings.ApikeyBinance) && !String.IsNullOrEmpty(Settings.ApiSecretBinance))
                {
                    SetApiCredentials(Settings.ApikeyBinance, Settings.ApiSecretBinance);

                    using (var client = httpBinanceClient)
                    {
                        // See https://github.com/JKorf/Binance.Net/issues/23
                        WebCallResult<BinanceAccountInfo> AccountInfoResult = client.GetAccountInfo();
                        if (AccountInfoResult.Success)
                        {
                            AccountInfo = AccountInfoResult.Data;
                        }
                        else
                        {
                            Message = "_GetAccountInfo ERROR getting AccountInfo: " + AccountInfoResult.Error.Message;
                        }
                    }
                }
                else
                {
                    Message = $"ERROR _GetAccountInfo: No Binance API Key / Secret set!";
                }
            }
            catch (Exception E)
            {
                Message = $"ERROR _GetAccountInfo: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return AccountInfo;
        }

        public /*BinancePlacedOrder*/ String BuyMarketOrder(string CoinPair, Decimal Amount, out string Message)
        {
            Message = "";
            //BinancePlacedOrder BinanceOrder = new BinancePlacedOrder();
            String SuccessMessage = "";

            try
            {
                if (!String.IsNullOrEmpty(CoinPair) && Amount > 0)
                {
                    Settings Settings = SettingsStore.Load();
                    if (!String.IsNullOrEmpty(Settings.ApikeyBinance) && !String.IsNullOrEmpty(Settings.ApiSecretBinance))
                    {
                        SetApiCredentials(Settings.ApikeyBinance, Settings.ApiSecretBinance);

                        using (var client = httpBinanceClient)
                        {
                            WebCallResult<BinancePlacedOrder> OrderResult = client.PlaceOrder(CoinPair, OrderSide.Buy, OrderType.Market, Amount);
                            if (OrderResult.Success)
                            {
                                //BinanceOrder = OrderResult.Data;
                                BinancePlacedOrder BinanceOrder = OrderResult.Data;
                                SuccessMessage = $"Success: Id={BinanceOrder.OrderId} Executed={BinanceOrder.ExecutedQuantity} Price={BinanceOrder.Price} Status={BinanceOrder.Status.ToString()}";
                            }
                            else
                            {
                                Message = "BuyMarketOrder: NO SUCCESS: " + OrderResult.Error.Message;
                            }
                        }
                    }
                    else
                    {
                        Message = $"ERROR BuyMarketOrder: No Binance API Key / Secret set!";
                    }
                }
                else
                {
                    Message = $"ERROR BuyMarketOrder: CoinPair={CoinPair} or Amount={Amount} not valid";
                }
            }
            catch (Exception E)
            {
                Message = $"ERROR BuyMarketOrder: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return SuccessMessage; // BinanceOrder;
        }

        public List<OrderBIN> GetOrders(string Symbol, out string Message)
        {
            Message = "";
            List<OrderBIN> Orders = new List<OrderBIN>();

            try
            {
                Settings Settings = SettingsStore.Load();
                if (!String.IsNullOrEmpty(Settings.ApikeyBinance) && !String.IsNullOrEmpty(Settings.ApiSecretBinance))
                {
                    SetApiCredentials(Settings.ApikeyBinance, Settings.ApiSecretBinance);

                    using (var client = httpBinanceClient)
                    {
                        WebCallResult<IEnumerable<BinanceOrder>> result = client.GetAllOrders(Symbol); // no non-blocking version available
                        if (result.Success)
                        {
                            Orders = new List<OrderBIN>(result.Data.OrderByDescending(d => d.Time).Select(o => new OrderBIN()
                            {
                                Valid = true,
                                Id = o.OrderId,
                                ExecutedQuantity = o.ExecutedQuantity,
                                OriginalQuantity = o.OriginalQuantity,
                                Price = o.Price,
                                Side = o.Side,
                                Status = o.Status,
                                Symbol = o.Symbol,
                                Time = o.Time,
                                Type = o.Type
                            }));
                        }
                        else
                        {
                            Orders = new List<OrderBIN>();
                            Orders.Add(new OrderBIN()
                            {
                                Valid = false,
                                Message = result.Error.Message
                            });
                        }
                    }
                }
                else
                {
                    Message = $"ERROR GetOrders: No Binance API Key / Secret set!";
                }
            }
            catch (Exception E)
            {
                Message = $"ERROR GetOrders: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return Orders;
        }

    }
}
