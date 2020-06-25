using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTraderStandard.ScannerLiteDB
{
    public class ScannerLiteDbStore
    {
        private string pDBPath = "";
        private string pDbLocation = "";
        private LiteDatabase pDB;
        private ILiteCollection<SignalsTBL> pcolSignals;
        private ILiteCollection<OrdersTBL> pcolOrders;
        private LiteDatabase pDb = null;

        public ScannerLiteDbStore(string DataMap)
        {
            try
            {
                //Not set in DataMap, better fixed location
                //pDbLocation = (!String.IsNullOrEmpty(DataMap) ? DataMap : "C:\\ScannerDB") + "\\ScannerData.LiteDB";
                pDBPath = ""; //  "C:\\ScannerDB\\";
                pDbLocation = $"{pDBPath}ScannerData.LiteDB";
                if (!String.IsNullOrEmpty(pDBPath)) System.IO.Directory.CreateDirectory(pDBPath);

                if (pDB == null)
                {
                    pDb = new LiteDatabase(pDbLocation);
                }
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ScannerLiteDbStore: ERROR {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
            }
        }

        public SignalsTBL LiteDbGetSignal(int Id, out string Message)
        {
            Message = "";
            SignalsTBL Signal = new SignalsTBL();

            try
            {
                if (pDb == null)
                {
                    pDb = new LiteDatabase(pDbLocation);
                }
                //using (LiteDatabase Db = new LiteDatabase(@pDbLocation))
                //{
                ILiteCollection<SignalsTBL> colSignals = pDb.GetCollection<SignalsTBL>("Signals");
                SignalsTBL FoundSignal = colSignals.FindById(Id);
                //}
            }
            catch (Exception E)
            {
                Message = $"GetSignal: ERROR {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return Signal;
        }

        public int InsertSignal(SignalsTBL Signal, out string Message)
        {
            Message = "";
            int Id = -1;

            try
            {
                if (pDb == null)
                {
                    pDb = new LiteDatabase(pDbLocation);
                }

                ILiteCollection<SignalsTBL> colSignals = pDb.GetCollection<SignalsTBL>("Signals");
                Signal.Id = 0; // for sure
                Signal.UpdateDateTime = DateTime.Now;
                Id = colSignals.Insert(Signal);
                if (Id > -1)
                {
                    if (Signal.FirstBuyId == 0)
                    {
                        Signal.FirstBuyId = Id;
                        if (!colSignals.Update(Signal))
                        {
                            Message = $"LiteDbInsertSignal Id={Id}: ERROR updating to set FirstBuyId to Id";
                        }
                    }
                }
                else
                {
                    Message = $"LiteDbInsertSignal ERROR: Invalid Id returned";
                }
            }
            catch (Exception E)
            {
                Message = $"LiteDbInsertSignal: ERROR InsertSignal (Id={Id}): {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return Id;
        }

        public SignalsTBL UpdateSignal(int id, Dictionary<string, dynamic> Data, out string Message)
        {
            Message = "";
            SignalsTBL Signal = new SignalsTBL();

            try
            {
                if (id > 0)
                {
                    if (pDb == null)
                    {
                        pDb = new LiteDatabase(pDbLocation);
                    }
                    //using (LiteDatabase Db = new LiteDatabase(@pDbLocation))
                    //{
                    ILiteCollection<SignalsTBL> colSignals = pDb.GetCollection<SignalsTBL>("Signals");
                    Signal = colSignals.FindById(id);
                    if (Signal != null)
                    {
                        foreach (KeyValuePair<string, dynamic> Entry in Data)
                        {
                            Signal.GetType().GetProperty(Entry.Key).SetValue(Signal, Entry.Value);
                        }
                        Signal.UpdateDateTime = DateTime.Now;
                        colSignals.Update(Signal);
                    }
                    else
                    {
                        Message = $"UpdateSignal: id={id} not found in DB, not updated!";
                    }
                    //}
                }
                else
                {
                    Message = "UpdateSignal: id=0, nothing to update!";
                }
            }
            catch (Exception E)
            {
                Message = $"UpdateSignal: ERROR id={id}: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return Signal;
        }

        public void DeleteSignal(int id, out string Message)
        {
            Message = "";
            try
            {
                if (id > 0)
                {
                    if (pDb == null)
                    {
                        pDb = new LiteDatabase(pDbLocation);
                    }
                    ILiteCollection<SignalsTBL> colSignals = pDb.GetCollection<SignalsTBL>("Signals");
                    if (!colSignals.Delete(id))
                    {
                        Message = $"DeleteSignal: id={id} not found in DB, not deleted!";
                    }
                }
            }
            catch (Exception E)
            {
                Message = $"DeleteSignal: ERROR id={id}: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }
        }

        /* public void DeleteManySignal(int id, out string Message)
         {
             Message = "";
             try
             {
                 if (id > 0)
                 {
                     if (pDb == null)
                     {
                         pDb = new LiteDatabase(pDbLocation);
                     }
                     ILiteCollection<SignalsTBL> colSignals = pDb.GetCollection<SignalsTBL>("Signals");
                     if (!colSignals.DeleteMany())
                     {
                         Message = $"DeleteSignal: id={id} not found in DB, not deleted!";
                     }
                 }
             }
             catch (Exception E)
             {
                 Message = $"DeleteSignal: ERROR id={id}: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
             }
         }*/

        public List<SignalsTBL> GetAllSignals(out string Message)
        {
            Message = "";
            List<SignalsTBL> AllSignals = new List<SignalsTBL>();

            int maxTries = 10;
            while (maxTries > 0)
            {
                maxTries--;
                Message = "";
                try
                {
                    if (pDb == null)
                    {
                        pDb = new LiteDatabase(pDbLocation);
                    }
                    //using (LiteDatabase Db = new LiteDatabase(@pDbLocation))
                    //{
                    ILiteCollection<SignalsTBL> colSignals = pDb.GetCollection<SignalsTBL>("Signals");
                    foreach (SignalsTBL Signal in colSignals.FindAll().OrderByDescending(s => s.StartDateTime))
                    {
                        AllSignals.Add(Signal);
                    }
                    //}
                    if (AllSignals.Count == 0)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} WARNING GetAllSignals: No Signals in DB");
                    }
                    maxTries = 0;
                }
                catch (Exception E)
                {
                    Message = $"GetAllSignals: ERROR trying {maxTries} times: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
                    System.Threading.Thread.Sleep(500);
                }
            }

            return AllSignals;
        }

        public static SignalsTBL CopySignal(SignalsTBL Signal)
        {
            SignalsTBL CopySignal = new SignalsTBL();

            try
            {
                CopySignal.StartDateTime = Signal.StartDateTime;
                CopySignal.UpdateDateTime = Signal.UpdateDateTime;
                CopySignal.SignalState = Signal.SignalState;
                CopySignal.TimeframeName = Signal.TimeframeName;
                CopySignal.Exchange = Signal.Exchange;
                CopySignal.Symbol = Signal.Symbol;
                CopySignal.Message = Signal.Message;
                CopySignal.Strategy = Signal.Strategy;
                CopySignal.TradeType = Signal.TradeType;
                CopySignal.Id = Signal.Id;
                CopySignal.ExchangeId = Signal.ExchangeId;
                CopySignal.FirstBuyId = Signal.FirstBuyId;
                CopySignal.BuyCount = Signal.BuyCount;
                CopySignal.CandleCount = Signal.CandleCount;
                CopySignal.TimeframeMinutes = Signal.TimeframeMinutes;
                CopySignal.Valid = Signal.Valid;
                CopySignal.BBWidth = Signal.BBWidth;
                CopySignal.RSI = Signal.RSI;
                CopySignal.ClosePrice = Signal.ClosePrice;
                CopySignal.BuyPrice = Signal.BuyPrice;
                CopySignal.TakeProfit = Signal.TakeProfit;
                CopySignal.StopLoss = Signal.StopLoss;
                CopySignal.MFI = Signal.MFI;
                CopySignal.Bm1Hr = Signal.Bm1Hr;
                CopySignal.Bm4Hr = Signal.Bm4Hr;
                CopySignal.Bm1Day = Signal.Bm1Day;
                CopySignal.Quote100Volume = Signal.Quote100Volume;
                CopySignal.MaxBuyAmount = Signal.MaxBuyAmount;
                CopySignal.PositionSize = Signal.PositionSize;
                CopySignal.SignalAction = Signal.SignalAction;
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CopySignal ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
            }

            return CopySignal;
        }
    }
}
