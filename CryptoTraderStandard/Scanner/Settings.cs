//using Avalonia.Input;
using MessagePack;
using System;

namespace CryptoTraderScanner
{
    [MessagePackObject]
    public class Settings
    {
        [Key(72)]
        public DateTime OperationEndDate { get { return new DateTime(2020, 8, 1); } }

        [Key(0)]
        public string Exchange { get; set; }

        [Key(1)]
        public bool USDT { get; set; }

        [Key(2)]
        public bool EUR { get; set; }

        [Key(3)]
        public bool ETH { get; set; }

        [Key(4)]
        public bool BNB { get; set; }

        [Key(5)]
        public bool BTC { get; set; }

        [Key(6)]
        public decimal Min24HrVolume { get; set; }

        [Key(7)]
        public bool AllowShort { get; set; }

        [Key(12)]
        public bool AllowLong { get; set; }

        [Key(8)]
        public decimal MinBollingerBandWidth { get; set; }

        [Key(9)]
        public int MaxFlatCandles { get; set; }

        [Key(10)]
        public int MaxFlatCandleCount { get; set; }

        [Key(11)]
        public decimal MaxPanic { get; set; }

        //[Key(12)]
        //public string TimeFrame { get; set; }

        [Key(13)]
        public bool TimeFrame1min { get; set; }

        [Key(14)]
        public bool TimeFrame3min { get; set; }

        [Key(15)]
        public bool TimeFrame5min { get; set; }

        [Key(16)]
        public bool TimeFrame15min { get; set; }

        [Key(17)]
        public bool TimeFrame30min { get; set; }

        [Key(18)]
        public bool TimeFrame1hr { get; set; }

        [Key(19)]
        public bool TimeFrame4hr { get; set; }

        [Key(20)]
        public bool TimeFrame1d { get; set; }

        [Key(21)]
        public decimal MaxMFI { get; set; }

        [Key(22)]
        public string Zignaly { get; set; }

        [Key(23)]
        public decimal MaxBollingerBandWidth { get; set; }

        [Key(24)]
        public decimal MaxRSI { get; set; }

        [Key(25)]
        public decimal StochD { get; set; }

        [Key(26)]
        public decimal StochK { get; set; }

        [Key(27)]
        public string BlacklistCoinpairs { get; set; }

        [Key(28)]
        public string WhitelistCoinpairs { get; set; }

        [Key(29)]
        public string BrowserLocation { get; set; }

        [Key(30)]
        public bool AutoTrade { get; set; }

        [Key(31)]
        public decimal LowSatBTC { get; set; }

        [Key(32)]
        public bool ChBottumUp { get; set; }

        [Key(33)]
        public bool ChMiddleUP { get; set; }

        [Key(34)]
        public bool ChRSIFamily { get; set; }

        [Key(35)]
        public decimal BUMaxBollingerBandWidth { get; set; }

        [Key(36)]
        public decimal BUMinBollingerBandWidth { get; set; }

        [Key(37)]
        public decimal BUMFI { get; set; }

        [Key(38)]
        public decimal BURSI { get; set; }

        [Key(39)]
        public bool ChStoch { get; set; }

        [Key(40)]
        public bool ChStochRSI { get; set; }

        [Key(41)]
        public decimal StochRSID { get; set; }

        [Key(42)]
        public decimal StochRSIK { get; set; }

        [Key(43)]
        public string Zignaly1min { get; set; }

        [Key(44)]
        public string Zignaly3min { get; set; }

        [Key(45)]
        public string Zignaly5min { get; set; }

        [Key(46)]
        public string Zignaly15min { get; set; }

        [Key(47)]
        public string Zignaly30min { get; set; }

        [Key(48)]
        public string Zignaly1hr { get; set; }

        [Key(49)]
        public string Zignaly4hr { get; set; }

        [Key(50)]
        public string Zignaly1day { get; set; }

        [Key(51)]
        public decimal RsiRSI { get; set; }

        [Key(52)]
        public decimal RsiRSISell { get; set; }

        [Key(53)]
        public decimal RsiStochRSI { get; set; }

        [Key(54)]
        public decimal RsiStochRSISell { get; set; }

        [Key(55)]
        public decimal RsiMFI { get; set; }

        [Key(56)]
        public decimal RsiMFISell { get; set; }

        [Key(57)]
        public bool WriteLogFile { get; set; }

        [Key(58)]
        public bool WriteCsvFile { get; set; }

        [Key(59)]
        public bool ChTradingview { get; set; }

        [Key(60)]
        public bool ChAltrady { get; set; }

        [Key(61)]
        public bool ChHyperTrader { get; set; }

        [Key(62)]
        public string DirDataMap { get; set; }

        [Key(63)]
        public bool ChMACDUPMU { get; set; }

        [Key(64)]
        public string ApikeyBinance { get; set; }

        [Key(65)]
        public string ApiSecretBinance { get; set; }

        [Key(66)]
        public bool DataViaOwnAPI { get; set; }

        [Key(67)]
        public decimal TradingBudget { get; set; }

        [Key(68)]
        public decimal ConPositions { get; set; }

        [Key(69)]
        public bool LogInvalidSignals { get; set; }

        [Key(70)]
        public bool BmValuesToFile { get; set; }

        [Key(71)]
        public bool BmValuesToFileWhenTfRun { get; set; }

        [Key(73)]
        public bool PlaySounds { get; set; }

        [Key(74)]
        public bool TradeExchangeDirect { get; set; }

        [Key(75)]
        public int MaxCandles1stBuy { get; set; }

        [Key(76)]
        public decimal MinMC4Signal { get; set; }

        [Key(77)]
        public int AutocloseAfterCandles { get; set; }

        [Key(78)]
        public int AutocloseAfterHr { get; set; }

        [Key(79)]
        public bool LogUriOwnAPI { get; set; }

        [Key(80)]
        public bool LogUriBinance { get; set; }

        [Key(81)]
        public bool LogUriZignaly { get; set; }

        [Key(82)]
        public bool McCheckActive { get; set; }

        [Key(83)]
        public bool Only1stBuy { get; set; }

        [Key(84)]
        public bool EnableConfirmDialogs { get; set; }

        [Key(85)]
        public decimal BUStopLoss { get; set; }

        [Key(86)]
        public int APIGetNrOfCandles { get; set; }

        [Key(87)]
        public decimal BUStopLoss1 { get; set; }
        [Key(88)]
        public decimal BUStopLoss2 { get; set; }

        [Key(89)]
        public bool DeleteNotusefullStatesDBRecords { get; set; }

        [Key(90)]
        public bool DeleteNotusefullActionsDBRecords { get; set; }

        [Key(91)]
        public int TelegramApiId { get; set; }

        [Key(92)]
        public string TelegramApiHash { get; set; }

        [Key(93)]
        public string TelegramUserNumber { get; set; }

        [Key(94)]
        public string TelegramCode { get; set; }

        [Key(95)]
        public string TelegramClientHash { get; set; }

        [Key(96)]
        public int ExchangeId { get; set; }

        [Key(97)]
        public bool UpdateClosePrices { get; set; }

        [Key(98)]
        public int MaxLogLines { get; set; }

        [Key(99)]
        public int MaxRecordLines { get; set; }

        [Key(100)]
        public bool PanicSellActivated { get; set; }

        [Key(101)]
        public decimal PSMC1hr { get; set; }

        [Key(102)]
        public decimal PSMC4hr { get; set; }

        [Key(103)]
        public string LogIndicatorCalculationsForSymbol { get; set; }

        [Key(104)]
        public string WriteLogFileType { get; set; }

        [Key(105)]
        public decimal BUStopLossLast { get; set; }

        [Key(106)]
        public bool ScanAllTimeframesEachMinute {get; set;}

        [Key(107)]
        public decimal MinMC4Hr4Signal { get; set; }
        [Key(108)]

        public decimal MinBNBAmount { get; set; }
        [Key(109)]
        public decimal AddBNBAmount { get; set; }
        [Key(110)]
        public int checkBNBMinutes { get; set; }
        [Key(111)]
        public string AddBNBQuote { get; set; }
        [Key(112)]
        public int MaxRebuysBU { get; set; }

        [Key(113)]
        public string Time2ConvertDust2BNB { get; set; }

        public Settings()
        {
            MinBNBAmount = (Decimal)1.0;
            AddBNBAmount = (Decimal)1.0;
            checkBNBMinutes = 0;
            AddBNBQuote = "";
            MaxRebuysBU = 2;
            Time2ConvertDust2BNB = "";
            ScanAllTimeframesEachMinute = false;
            LogIndicatorCalculationsForSymbol = "";
            PanicSellActivated = false;
            UpdateClosePrices = false;
            Exchange = "Binance";
            DataViaOwnAPI = true;
            TelegramApiId = 0;
            TelegramApiHash = "";
            TelegramClientHash = "";
            TelegramUserNumber = "";
            TelegramCode = "";
            APIGetNrOfCandles = 36;
            BTC = true;
            Min24HrVolume = 200;
            MinBollingerBandWidth = 2;
            MaxBollingerBandWidth = 4;
            AllowShort = true;
            AllowLong = true;
            AutoTrade = false;
            MaxFlatCandles = 1;
            MaxFlatCandleCount = 15;
            MaxPanic = 5;
            //TimeFrame = "3 min";
            TimeFrame3min = true;
            MaxMFI = 20;
            MaxRSI = 20;
            Zignaly = "";
            BlacklistCoinpairs = "";
            WhitelistCoinpairs = "";
            BrowserLocation = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
            DirDataMap = "C:\\";
            WriteLogFile = false;
            WriteLogFileType = "No";
            WriteCsvFile = false;
            LogInvalidSignals = false;
            BmValuesToFile = false;
            BmValuesToFileWhenTfRun = true;
            PlaySounds = false;
            TradeExchangeDirect = false;
            MaxCandles1stBuy = 3;
            AutocloseAfterCandles = 0;
            AutocloseAfterHr = 24;
            MinMC4Signal = (Decimal)0.1;
            MinMC4Hr4Signal = (Decimal)0.1;
            LogUriOwnAPI = false;
            LogUriBinance = false;
            LogUriZignaly = false;
            McCheckActive = true;
            Only1stBuy = false;
            EnableConfirmDialogs = false;
            DeleteNotusefullStatesDBRecords = false;
            DeleteNotusefullActionsDBRecords = false;
            MaxLogLines = 500;
            MaxRecordLines = 200;
        }
    }
}