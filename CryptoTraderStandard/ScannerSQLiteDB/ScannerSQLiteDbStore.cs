using LiteDB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CryptoTraderStandard.ScannerSQLiteDB
{
    public class ScannerSQLiteDbStore
    {
        private string pDBPath = "";
        private string pDbLocation = "";
        //private SQLiteConnection pConn = null;

        public ScannerSQLiteDbStore(string DataMap)
        {
            try
            {
                //Not set in DataMap, better fixed location
                //pDbLocation = (!String.IsNullOrEmpty(DataMap) ? DataMap : "C:\\ScannerDB") + "\\ScannerData.LiteDB";
                pDBPath = ""; //  "C:\\ScannerDB\\";
                pDbLocation = $"{pDBPath}ScannerSQLiteDB.db";
                if (!String.IsNullOrEmpty(pDBPath)) System.IO.Directory.CreateDirectory(pDBPath);
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} ScannerSQLiteDbStore: ERROR {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
            }
        }

        private SQLiteConnection OpenConnection(out string Message)
        {
            Message = "";
            SQLiteConnection pConn = null;

            try
            {
                bool DbExists = false;
                if (pConn == null)
                {
                    DbExists = File.Exists(pDbLocation);

                    pConn = new SQLiteConnection($"Data Source={pDbLocation};Version=3;");
                }

                if (pConn.State == System.Data.ConnectionState.Closed || pConn.State == System.Data.ConnectionState.Broken)
                {
                    pConn.Open();
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} OpenConnection to {pDbLocation}");
                }

                if (!DbExists)
                {
                    string Sql = "CREATE TABLE IF NOT EXISTS [signals] ("
                        + "[Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, "
                        + "[FirstBuyId] INTEGER NOT NULL, "
                        + "[ExchangeId] INTEGER, "
                        + "[BuyCount] INTEGER, "
                        + "[SignalState] TINYINT NOT NULL ON CONFLICT IGNORE DEFAULT 0, "
                        + "[SignalAction] TINYINT NOT NULL ON CONFLICT IGNORE DEFAULT 0, "
                        + "[Strategy] TINYINT NOT NULL ON CONFLICT IGNORE DEFAULT 0, "
                        + "[TradeType] TINYINT NOT NULL ON CONFLICT IGNORE DEFAULT 0, "
                        + "[TimeframeMinutes] SMALLINT NOT NULL ON CONFLICT IGNORE DEFAULT 0, "
                        + "[TimeframeName] VARCHAR(15), "
                        + "[Exchange] VARCHAR(31), "
                        + "[Symbol] VARCHAR(31) NOT NULL ON CONFLICT IGNORE, "
                        + "[CandleCount] SMALLINT, "
                        + "[Quote100Volume] DOUBLE, "
                        + "[Valid] BOOL NOT NULL ON CONFLICT IGNORE, "
                        + "[Message] VARCHAR(255), "
                        + "[StartDateTime] DATETIME NOT NULL ON CONFLICT IGNORE, "
                        + "[UpdateDateTime] DATETIME NOT NULL ON CONFLICT IGNORE, "
                        + "[BuyPrice] DOUBLE, "
                        + "[ClosePrice] DOUBLE,"
                        + "[TakeProfit] DOUBLE, "
                        + "[StopLoss] DOUBLE, "
                        + "[MaxBuyAmount] DOUBLE, "
                        + "[PositionSize] DOUBLE, "
                        + "[BBLower] DOUBLE, "
                        + "[BBMiddle] DOUBLE, "
                        + "[BBUpper] DOUBLE, "
                        + "[BBWidth] DOUBLE, "
                        + "[MFI] DOUBLE, "
                        + "[RSI] DOUBLE, "
                        + "[RSIK] DOUBLE, "
                        + "[RSID] DOUBLE, "
                        + "[StochK] DOUBLE, "
                        + "[StochD] DOUBLE, "
                        + "[MACD] DOUBLE, "
                        + "[MACDSignal] DOUBLE, "
                        + "[MACDHistogram] DOUBLE, "
                        + "[Bm1Hr] DOUBLE, "
                        + "[Bm4Hr] DOUBLE, "
                        + "[Bm1Day] DOUBLE)";
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} OpenConnection SQL=[{Sql}]");

                    SQLiteCommand cmd = new SQLiteCommand(Sql, pConn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception E)
            {
                Message = $"OpenConnection for ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }
            return pConn;
        }

        private void CloseConnection(SQLiteConnection pConn)
        {
            if (pConn != null && pConn.State == System.Data.ConnectionState.Open)
            {
                pConn.Dispose();
                pConn.Close();
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CloseConnection: closed the connection");
            }
        }

        public List<SignalsTBL> GetSignals(int Id, int FirstBuyId, int TimeframeMinutes, List<SignalsTBL.SignalStates> States, List<SignalsTBL.SignalActions> Actions, out string Message)
        {
            Message = "";
            List<SignalsTBL> Signals = new List<SignalsTBL>();
            SQLiteConnection pConn = null;

            try
            {
                CloseConnection(pConn);

                pConn = OpenConnection(out Message);
                if (String.IsNullOrEmpty(Message))
                {
                    string Sql = "SELECT * FROM signals WHERE (1 = 1) "
                        + (Id > 0 ? $"AND (Id = {Id}) " : "")
                        + (FirstBuyId > 0 ? $"AND (FirstBuyId = {Id}) " + (Id == 0 ? "AND Id != FirstBuyId " : "") : "")
                        + (TimeframeMinutes > 0 ? $"AND (TimeframeMinutes = {TimeframeMinutes}) " : "")
                        + ((States.Count > 0) ? $"AND (SignalState in ({String.Join(",", States.Select(s => (int)s))})) " : "")
                        + ((Actions.Count > 0) ? $"AND (SignalAction in ({String.Join(",", Actions.Select(a => (int)a))})) " : "")
                        + "ORDER BY Id";
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} GetSignals SQL=[{Sql}]");

                    SQLiteCommand cmd = new SQLiteCommand(Sql, pConn);
                    SQLiteDataReader reader = cmd.ExecuteReader();

                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = ".";

                    while (reader.Read())
                    {
                        SignalsTBL Signal = new SignalsTBL();
                        Signal.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        Signal.FirstBuyId = reader.GetInt32(reader.GetOrdinal("FirstBuyId"));
                        Signal.ExchangeId = reader.GetInt32(reader.GetOrdinal("ExchangeId"));
                        Signal.BuyCount = reader.GetInt32(reader.GetOrdinal("BuyCount"));
                        Signal.SignalState = (SignalsTBL.SignalStates)reader.GetInt32(reader.GetOrdinal("SignalState")); //(SignalsTBL.SignalStates)Enum.Parse(typeof(SignalsTBL.SignalStates), reader.GetString(reader.GetOrdinal("SignalState")));
                        Signal.SignalAction = (SignalsTBL.SignalActions)reader.GetInt32(reader.GetOrdinal("SignalAction"));  //Enum.Parse(typeof(SignalsTBL.SignalActions), reader.GetString(reader.GetOrdinal("SignalAction")));
                        Signal.Strategy = (SignalsTBL.Strategies)reader.GetInt32(reader.GetOrdinal("Strategy")); //Enum.Parse(typeof(SignalsTBL.Strategies), reader.GetString(reader.GetOrdinal("Strategy")));
                        Signal.TradeType = (SignalsTBL.TradeTypes)reader.GetInt32(reader.GetOrdinal("TradeType"));  //Enum.Parse(typeof(SignalsTBL.TradeTypes), reader.GetString(reader.GetOrdinal("TradeType")));
                        Signal.TimeframeMinutes = reader.GetInt32(reader.GetOrdinal("TimeframeMinutes"));
                        Signal.TimeframeName = reader["TimeframeName"].ToString();
                        Signal.Exchange = reader.GetString(reader.GetOrdinal("Exchange"));
                        Signal.Symbol = reader.GetString(reader.GetOrdinal("Symbol"));
                        Signal.CandleCount = reader.GetInt32(reader.GetOrdinal("CandleCount"));
                        Signal.Quote100Volume = reader.GetDecimal(reader.GetOrdinal("Quote100Volume"));  //reader.GetDecimal(reader.GetOrdinal("Quote100Volume"].ToString());
                        Signal.Valid = reader.GetBoolean(reader.GetOrdinal("Valid"));
                        Signal.Message = reader.GetString(reader.GetOrdinal("Message"));
                        Signal.StartDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime"));
                        Signal.UpdateDateTime = reader.GetDateTime(reader.GetOrdinal("UpdateDateTime"));
                        Signal.BuyPrice = reader.GetDecimal(reader.GetOrdinal("BuyPrice"));
                        Signal.ClosePrice = reader.GetDecimal(reader.GetOrdinal("ClosePrice"));
                        Signal.TakeProfit = reader.GetDecimal(reader.GetOrdinal("TakeProfit"));
                        Signal.StopLoss = reader.GetDecimal(reader.GetOrdinal("StopLoss"));
                        Signal.MaxBuyAmount = reader.GetDecimal(reader.GetOrdinal("MaxBuyAmount"));
                        Signal.PositionSize = reader.GetDecimal(reader.GetOrdinal("PositionSize"));
                        Signal.BBLower = reader.GetDecimal(reader.GetOrdinal("BBLOwer"));
                        Signal.BBMiddle = reader.GetDecimal(reader.GetOrdinal("BBMiddle"));
                        Signal.BBUpper = reader.GetDecimal(reader.GetOrdinal("BBUpper"));
                        Signal.BBWidth = reader.GetDecimal(reader.GetOrdinal("BBWidth"));
                        Signal.MFI = reader.GetDecimal(reader.GetOrdinal("MFI"));
                        Signal.RSI = reader.GetDecimal(reader.GetOrdinal("RSI"));
                        Signal.RSIK = reader.GetDecimal(reader.GetOrdinal("RSIK"));
                        Signal.RSID = reader.GetDecimal(reader.GetOrdinal("RSID"));
                        Signal.StochK = reader.GetDecimal(reader.GetOrdinal("StochK"));
                        Signal.StochD = reader.GetDecimal(reader.GetOrdinal("StochD"));
                        Signal.MACD = reader.GetDecimal(reader.GetOrdinal("MACD"));
                        Signal.MACDSignal = reader.GetDecimal(reader.GetOrdinal("MACDSignal"));
                        Signal.MACDHistogram = reader.GetDecimal(reader.GetOrdinal("MACDHistogram"));
                        Signal.Bm1Hr = reader.GetDecimal(reader.GetOrdinal("Bm1Hr"));
                        Signal.Bm4Hr = reader.GetDecimal(reader.GetOrdinal("Bm4Hr"));
                        Signal.Bm1Day = reader.GetDecimal(reader.GetOrdinal("Bm1Day"));

                        Signals.Add(Signal);
                        //Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} GetSignals A: {Signal.Id} {Signal.Symbol} {Signal.TimeframeName} {Signal.StartDateTime} {Signal.BuyPrice}");
                    }
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} GetSignals: {Signals.Count} records");
                }

                //CloseConnection(pConn);
            }
            catch (Exception E)
            {
                Message = $"GetSignals ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            //test output 
            /*foreach (SignalsTBL Sig in Signals)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} GetSignals B: {Sig.Id} {Sig.Symbol} {Sig.TimeframeName} {Sig.StartDateTime} {Sig.BuyPrice}");
            }*/

            return Signals;
        }

        public int InsertSignal(SignalsTBL Signal, out string Message)
        {
            Message = "";
            SQLiteConnection pConn = null;
            int RowsAffected = 0;
            int Id = -1;

            try
            {
                CloseConnection(pConn);

                pConn = OpenConnection(out Message);
                if (String.IsNullOrEmpty(Message))
                {
                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = ".";

                    string Sql = "INSERT INTO signals ("
                        + "Id,FirstBuyId,ExchangeId,BuyCount,"
                        + "SignalState,SignalAction,Strategy,TradeType,"
                        + "TimeFrameMinutes,TimeFrameName,Exchange,Symbol,CandleCount,Quote100Volume,"
                        + "Valid,Message,StartDateTime,UpdateDateTime,"
                        + "BuyPrice,ClosePrice,TakeProfit,StopLoss,MaxBuyAmount,PositionSize,"
                        + "BBLower,BBMiddle,BBUpper,BBWidth,"
                        + "MFI,RSI,RSIK,RSID,StochK,StochD,"
                        + "MACD,MACDSignal,MACDHistogram,"
                        + "Bm1Hr,Bm4Hr,Bm1Day"
                        + ") VALUES ("
                        + $"null, {Signal.FirstBuyId}, {Signal.ExchangeId}, {Signal.BuyCount}, "
                        + $"{(int)Signal.SignalState}, {(int)Signal.SignalAction}, {(int)Signal.Strategy}, {(int)Signal.TradeType}, "
                        + $"{Signal.TimeframeMinutes}, '{Signal.TimeframeName}', '{Signal.Exchange}', '{Signal.Symbol}', {Signal.CandleCount}, {Signal.Quote100Volume.ToString(nfi)}, "
                        + $"{Signal.Valid}, '{Signal.Message}', '{Signal.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss")}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', "
                        + $"{Signal.BuyPrice.ToString(nfi)}, {Signal.ClosePrice.ToString(nfi)}, {Signal.TakeProfit.ToString(nfi)}, {Signal.StopLoss.ToString(nfi)}, {Signal.MaxBuyAmount.ToString(nfi)}, {Signal.PositionSize.ToString(nfi)}, "
                        + $"{Signal.BBLower.ToString(nfi)}, {Signal.BBMiddle.ToString(nfi)}, {Signal.BBUpper.ToString(nfi)}, {Signal.BBWidth.ToString(nfi)}, "
                        + $"{Signal.MFI.ToString(nfi)}, {Signal.RSI.ToString(nfi)}, {Signal.RSIK.ToString(nfi)}, {Signal.RSID.ToString(nfi)}, {Signal.StochK.ToString(nfi)}, {Signal.StochD.ToString(nfi)}, "
                        + $"{Signal.MACD.ToString(nfi)}, {Signal.MACDSignal.ToString(nfi)}, {Signal.MACDHistogram.ToString(nfi)}, "
                        + $"{Signal.Bm1Hr.ToString(nfi)}, {Signal.Bm4Hr.ToString(nfi)}, {Signal.Bm1Day.ToString(nfi)} "
                        + ");";
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InsertSignal SQL=[{Sql}]");

                    SQLiteCommand cmd = new SQLiteCommand(Sql, pConn);
                    cmd.ExecuteScalar();
                    Id = (int)pConn.LastInsertRowId;
                    RowsAffected = pConn.Changes;
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InsertSignal: {RowsAffected} record");

                    if (Id > 0)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InsertSignal Id={Id}");
                        if (Signal.FirstBuyId == 0)
                        {
                            Signal.FirstBuyId = Id;
                            Sql = $"UPDATE signals SET FirstBuyId = {pConn.LastInsertRowId} WHERE Id = {Id}";
                            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InsertSignal -> Update SQL=[{Sql}]");

                            cmd = new SQLiteCommand(Sql, pConn);
                            cmd.ExecuteScalar();
                            RowsAffected = pConn.Changes;
                            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} InsertSignal -> Update: {RowsAffected} record");
                            /*if (pConn.)
                            {
                                Message = $"SQLiteDbInsertSignal ({Signal.TimeframeMinutes}minutes) Id={Id} ERROR: updating to set FirstBuyId to Id";
                            }*/
                        }
                    }
                    else
                    {
                        Message = $"SQLiteDbInsertSignal ({Signal.TimeframeMinutes}minutes) ERROR: Invalid Id returned";
                    }
                }

                //CloseConnection(pConn);
            }
            catch (Exception E)
            {
                Message = $"SQLiteDbInsertSignal InsertSignal ({Signal.TimeframeMinutes}minutes, Id={Id}) ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return Id;
        }

        public List<SignalsTBL> UpdateSignal(int Id, bool UpdateChilds, Dictionary<string, dynamic> Data, out string Message)
        {
            Message = "";
            SQLiteConnection pConn = null;
            int RowsAffected = 0;
            List<SignalsTBL> Signals = new List<SignalsTBL>();

            try
            {
                CloseConnection(pConn);

                if (Id > 0)
                {
                    pConn = OpenConnection(out Message);
                    if (String.IsNullOrEmpty(Message) && Data.Count > 0)
                    {
                        bool UpdateSet = false;
                        string Sql = "";
                        foreach (KeyValuePair<string, dynamic> Entry in Data)
                        {
                            Sql += $"{(String.IsNullOrEmpty(Sql) ? "" : ", ")} {Entry.Key} = {Entry.Value}";
                            if (Entry.Key == "UpdateDateTime") UpdateSet = true;
                        }
                        if (!UpdateSet)
                        {
                            Sql += $", UpdateDateTime = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";
                        }
                        Sql = $"UPDATE signals SET {Sql} WHERE " + (UpdateChilds ? $"FirstBuyId = {Id} AND Id != FirstBuyId" : $"Id = {Id}");
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} UpdateSignal SQL=[{Sql}]");

                        SQLiteCommand cmd = new SQLiteCommand(Sql, pConn);
                        cmd.ExecuteScalar();
                        RowsAffected = pConn.Changes;
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} UpdateSignal: {RowsAffected} record(s)");
                        // Message = $"UpdateSignal {Signal.TimeframeMinutes}minutes, Id={Signal.Id} ERROR Updating Signal";
                        if (RowsAffected > 0)
                        {
                            if (UpdateChilds)
                            {
                                Signals = GetSignals(0, Id, 0, Enumerable.Empty<SignalsTBL.SignalStates>().ToList(), Enumerable.Empty<SignalsTBL.SignalActions>().ToList(), out Message);
                            }
                            else
                            {
                                Signals = GetSignals(Id, 0, 0, Enumerable.Empty<SignalsTBL.SignalStates>().ToList(), Enumerable.Empty<SignalsTBL.SignalActions>().ToList(), out Message);
                            }
                        }
                    }
                    else
                    {
                        Message = $"UpdateSignal Id={Id} ERROR {(String.IsNullOrEmpty(Message) ? "no data received to update!" : Message)}";
                    }
                }
                else
                {
                    Message = "UpdateSignal: id=0, nothing to update!";
                }

                //CloseConnection(pConn);
            }
            catch (Exception E)
            {
                Message = $"UpdateSignal: ERROR Id={Id}: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return Signals;
        }

        public int DeleteSignal(int Id, int TimeFrameMinutes, List<SignalsTBL.SignalStates> States, List<SignalsTBL.SignalActions> Actions, out string Message)
        {
            SQLiteConnection pConn = null;
            int RowsAffected = 0;
            Message = "";
            try
            {
                CloseConnection(pConn);

                pConn = OpenConnection(out Message);
                if (String.IsNullOrEmpty(Message))
                {
                    string AndWhere = ((Id > 0) ? $"AND (Id = {Id}) " : "")
                        + ((TimeFrameMinutes > 0) ? $"AND (TimeframeMinutes = {TimeFrameMinutes}) " : "")
                        + ((States.Count > 0) ? $"AND (SignalState in ({String.Join(", ", States.Select(s => (int)s))})) " : "")
                        + ((Actions.Count > 0) ? $"AND (SignalAction in ({String.Join(", ", Actions.Select(a => (int)a))})) " : "");

                    if (!String.IsNullOrEmpty(AndWhere))
                    {
                        String Sql = $"DELETE FROM signals WHERE 1=1 {AndWhere}";
                        Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} DeleteSignal SQL=[{Sql}]");

                        SQLiteCommand cmd = new SQLiteCommand(Sql, pConn);
                        cmd.ExecuteScalar();
                        RowsAffected = pConn.Changes;
                    }
                    //Message = $"DeleteSignal {Signal.TimeframeMinutes}minutes ERROR: Id={Id} not found in DB, not deleted!";
                }

                //CloseConnection(pConn);
            }
            catch (Exception E)
            {
                Message = $"DeleteSignal Id={Id} ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'";
            }

            return RowsAffected;
        }

        public static SignalsTBL CopySignal(SignalsTBL Signal)
        {
            SignalsTBL CopySignal = new SignalsTBL();

            try
            {
                CopySignal.Id = Signal.Id;
                CopySignal.FirstBuyId = Signal.FirstBuyId;
                CopySignal.ExchangeId = Signal.ExchangeId;
                CopySignal.BuyCount = Signal.BuyCount;
                CopySignal.SignalState = Signal.SignalState;
                CopySignal.SignalAction = Signal.SignalAction;
                CopySignal.Strategy = Signal.Strategy;
                CopySignal.TradeType = Signal.TradeType;
                CopySignal.TimeframeMinutes = Signal.TimeframeMinutes;
                CopySignal.TimeframeName = Signal.TimeframeName;
                CopySignal.Exchange = Signal.Exchange;
                CopySignal.Symbol = Signal.Symbol;
                CopySignal.CandleCount = Signal.CandleCount;
                CopySignal.Quote100Volume = Signal.Quote100Volume;
                CopySignal.Valid = Signal.Valid;
                CopySignal.Message = Signal.Message;
                CopySignal.StartDateTime = Signal.StartDateTime;
                CopySignal.UpdateDateTime = Signal.UpdateDateTime;
                CopySignal.BuyPrice = Signal.BuyPrice;
                CopySignal.ClosePrice = Signal.ClosePrice;
                CopySignal.TakeProfit = Signal.TakeProfit;
                CopySignal.StopLoss = Signal.StopLoss;
                CopySignal.MaxBuyAmount = Signal.MaxBuyAmount;
                CopySignal.PositionSize = Signal.PositionSize;
                CopySignal.BBWidth = Signal.BBWidth;
                CopySignal.RSI = Signal.RSI;
                CopySignal.MFI = Signal.MFI;
                CopySignal.Bm1Hr = Signal.Bm1Hr;
                CopySignal.Bm4Hr = Signal.Bm4Hr;
                CopySignal.Bm1Day = Signal.Bm1Day;
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} CopySignal ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 40))}'");
            }

            return CopySignal;
        }
    }
}
