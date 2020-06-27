using CryptoTraderScanner;
using CryptoTraderStandard.TelegramAPI;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace CryptoTrader
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : MetroWindow
    {
        private System.Windows.Controls.ComboBox _dropDownExchange;
        private System.Windows.Controls.ComboBox _dropDownWriteLogFileType;
        private TelegramClientAPI TelegramClientAPI;

        //        private ComboBox _dropDownTimeFrame;
        public ObservableCollection<string> Exchanges { get; set; }
        public ObservableCollection<string> WriteLogFileTypes { get; set; }
        //        public ObservableCollection<string> TimeFrames { get; set; }
        public SettingsDialog()
        {
            DataContext = this;
            Exchanges = new ObservableCollection<string>();
            //	Exchanges.Add("Bitfinex");
            //            Exchanges.Add("Bittrex");
            Exchanges.Add("Binance");
            //            Exchanges.Add("GDax");
            //            Exchanges.Add("HitBTC");
            //            Exchanges.Add("Kraken");

            //            TimeFrames = new ObservableCollection<string>();
            //            TimeFrames.Add("4 hr");
            //            TimeFrames.Add("1 hr");
            //            TimeFrames.Add("30 min");
            //            TimeFrames.Add("15 min");
            //            TimeFrames.Add("5 min");
            //            TimeFrames.Add("3 min");
            //            TimeFrames.Add("1 min");

            WriteLogFileTypes = new ObservableCollection<string>();
            WriteLogFileTypes.Add("No");
            WriteLogFileTypes.Add("Log4Net");
            WriteLogFileTypes.Add("SeriLog");
            WriteLogFileTypes.Add("StreamWriter");

            InitializeComponent();
            Load();
        }

        private void Load()
        {
            // this.AttachDevTools();
            _dropDownExchange = dropExchange;
            _dropDownWriteLogFileType = dropWriteLogFileType;
            // _dropDownTimeFrame = dropTimeFrame;
            btnTelegramCodeRequest.Click += BtnTelegramCodeRequest_Click;
            btnSave.Click += BtnSave_Click;
            btnReset.Click += BtnReset_Click;
            btnCancel.Click += BtnCancel_Click;

            var settings = SettingsStore.Load();
            Init(settings);
        }

        private void Init(Settings settings)
        {
            APIGetNrOfCandles = settings.APIGetNrOfCandles.ToString();
            CurrencyUSDT = settings.USDT;
            CurrencyETH = settings.ETH;
            CurrencyEUR = settings.EUR;
            CurrencyBNB = settings.BNB;
            CurrencyBTC = settings.BTC;
            Zignaly = "" + settings.Zignaly;
            Zignaly1min = "" + settings.Zignaly1min;
            Zignaly3min = "" + settings.Zignaly3min;
            Zignaly5min = "" + settings.Zignaly5min;
            Zignaly15min = "" + settings.Zignaly15min;
            Zignaly30min = "" + settings.Zignaly30min;
            Zignaly1hr = "" + settings.Zignaly1hr;
            Zignaly4hr = "" + settings.Zignaly4hr;
            Zignaly1day = "" + settings.Zignaly1day;
            AllowShort = settings.AllowShort;
            AllowLong = settings.AllowLong;
            AutoTrade = settings.AutoTrade;
            TradeExchangeDirect = settings.TradeExchangeDirect;
            PlaySounds = settings.PlaySounds;
            MaxCandles1stBuy = $"{settings.MaxCandles1stBuy:0}";
            AutocloseAfterCandles = $"{settings.AutocloseAfterCandles:0}";
            AutocloseAfterHr = $"{settings.AutocloseAfterHr:0}";
            MinMC4Signal = $"{settings.MinMC4Signal:0.000}";
            MinMC4Hr4Signal = $"{settings.MinMC4Hr4Signal:0.000}";
            ChBottumUp = settings.ChBottumUp;
            ChMiddleUP = settings.ChMiddleUP;
            ChRSIFamily = settings.ChRSIFamily;
            ChTradingview = settings.ChTradingview;
            ChAltrady = settings.ChAltrady;
            ChHyperTrader = settings.ChHyperTrader;
            ChStoch = settings.ChStoch;
            ChStochRSI = settings.ChStochRSI;
            ChMACDUPMU = settings.ChMACDUPMU;
            MinBollingerBandWidth = $"{settings.MinBollingerBandWidth:0.00}";
            MaxBollingerBandWidth = $"{settings.MaxBollingerBandWidth:0.00}";
            BUBollingerBandWidth = $"{settings.BUMinBollingerBandWidth:0.00}";
            BUMaxBollingerBandWidth = $"{settings.BUMaxBollingerBandWidth:0.00}";
            BUStopLoss = $"{settings.BUStopLoss:0.00}";
            BUStopLoss1 = $"{settings.BUStopLoss1:0.00}";
            BUStopLoss2 = $"{settings.BUStopLoss2:0.00}";
            BUStopLossLast = $"{settings.BUStopLossLast:0.00}";
            MaxPanic = $"{settings.MaxPanic:0.00}";
            MaxFlatCandles = settings.MaxFlatCandles.ToString();
            MaxFlatCandleCount = settings.MaxFlatCandleCount.ToString();
            MaxMFI = $"{settings.MaxMFI:0}";
            MaxRSI = $"{settings.MaxRSI:0}";
            BUMFI = $"{settings.BUMFI:0}";
            BURSI = $"{settings.BURSI:0}";
            StochD = $"{settings.StochD:0}";
            StochK = $"{settings.StochK:0}";
            StochRSID = $"{settings.StochRSID:0}";
            StochRSIK = $"{settings.StochRSIK:0}";
            Volume = settings.Min24HrVolume.ToString();
            LowSatBTC = $"{settings.LowSatBTC}";
            RsiRSI = $"{settings.RsiRSI:0}";
            RsiStochRSI = $"{settings.RsiStochRSI:0}";
            RsiMFI = $"{settings.RsiMFI:0}";
            RsiRSISell = $"{settings.RsiRSISell:0}";
            RsiStochRSISell = $"{settings.RsiStochRSISell:0}";
            RsiMFISell = $"{settings.RsiMFISell:0}";
            _dropDownExchange.SelectedIndex = Exchanges.IndexOf("" + settings.Exchange);
            _dropDownWriteLogFileType.SelectedIndex = WriteLogFileTypes.IndexOf("" + settings.WriteLogFileType);
            DataViaOwnAPI = settings.DataViaOwnAPI;
            BlacklistCoinpairs = "" + settings.BlacklistCoinpairs;
            WhitelistCoinpairs = "" + settings.WhitelistCoinpairs;
            BrowserLocation = "" + settings.BrowserLocation;
            DirDataMap = "" + settings.DirDataMap;
            WriteCsvFile = settings.WriteCsvFile;
            LogInvalidSignals = settings.LogInvalidSignals;
            LogUriOwnAPI = settings.LogUriOwnAPI;
            LogUriBinance = settings.LogUriBinance;
            LogUriZignaly = settings.LogUriZignaly;
            MaxLogLines = $"{settings.MaxLogLines:0}";
            MaxRecordLines = $"{settings.MaxRecordLines:0}";
            BmValuesToFile = settings.BmValuesToFile;
            BmValuesToFileWhenTfRun = settings.BmValuesToFileWhenTfRun;
            ApiKeyBinance = "" + settings.ApikeyBinance;
            ApiSecretBinance = "" + settings.ApiSecretBinance;
            TelegramApiId = $"{settings.TelegramApiId:0}";
            TelegramApiHash = "" + settings.TelegramApiHash;
            TelegramClientHash = "" + settings.TelegramClientHash;
            TelegramUserNumber = "" + settings.TelegramUserNumber;
            TelegramCode = "" + settings.TelegramCode;
            TradingBudget = $"{settings.TradingBudget:0.00000000}";
            ConPositions = $"{settings.ConPositions:0}";
            EnableConfirmDialogs = settings.EnableConfirmDialogs;
            DeleteNotusefullStatesDBRecords = settings.DeleteNotusefullStatesDBRecords;
            DeleteNotusefullActionsDBRecords = settings.DeleteNotusefullActionsDBRecords;
            UpdateClosePrices = settings.UpdateClosePrices;
            ExchangeId = $"{settings.ExchangeId:0}";
            PanicSellActivated = settings.PanicSellActivated;
            PSMC1hr = $"{settings.PSMC1hr:0.00}";
            PSMC4hr = $"{settings.PSMC4hr:0.00}";
            MaxCMDifferencePercentage = $"{settings.MaxCMDifferencePercentage:0.00}";
            LogIndicatorCalculationsForSymbol = settings.LogIndicatorCalculationsForSymbol;
            ScanAllTimeframesEachMinute = settings.ScanAllTimeframesEachMinute;
            MinBNBAmount = $"{settings.MinBNBAmount:0.00}";
            AddBNBAmount = $"{settings.AddBNBAmount:0.00}";
            checkBNBMinutes = $"{settings.checkBNBMinutes:0}";
            AddBNBQuote = "" + settings.AddBNBQuote;
            MaxRebuysBU = $"{settings.MaxRebuysBU:0}";
            Time2ConvertDust2BNB = "" + settings.Time2ConvertDust2BNB;
        }

        // Source: https://stackoverflow.com/questions/1268552/how-do-i-get-a-textbox-to-only-accept-numeric-input-in-wpf
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void IntnumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void BtnTelegramCodeRequest_Click(object sender, RoutedEventArgs e)
        {
            TelegramClientAPI = new TelegramClientAPI();
            TelegramClientHash = await TelegramClientAPI.AuthenticateTelegramUserRequest(TelegramUserNumber);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            var settings = new Settings();
            Init(settings);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var settings = SettingsStore.Load();
            settings.USDT = CurrencyUSDT;
            settings.ETH = CurrencyETH;
            settings.EUR = CurrencyEUR;
            settings.BNB = CurrencyBNB;
            settings.BTC = CurrencyBTC;

            settings.TimeFrame1min = Timeframe1min;
            settings.TimeFrame3min = Timeframe3min;
            settings.TimeFrame5min = Timeframe5min;
            settings.TimeFrame15min = Timeframe15min;
            settings.TimeFrame30min = Timeframe30min;
            settings.TimeFrame1hr = Timeframe1hr;
            settings.TimeFrame4hr = Timeframe4hr;
            settings.TimeFrame1d = Timeframe1day;

            settings.AllowShort = AllowShort;
            settings.AllowLong = AllowLong;
            settings.AutoTrade = AutoTrade;
            settings.TradeExchangeDirect = TradeExchangeDirect;
            settings.PlaySounds = PlaySounds;
            settings.ChBottumUp = ChBottumUp;
            settings.ChMiddleUP = ChMiddleUP;
            settings.ChRSIFamily = ChRSIFamily;
            settings.ChTradingview = ChTradingview;
            settings.ChAltrady = ChAltrady;
            settings.ChHyperTrader = ChHyperTrader;
            settings.ChStoch = ChStoch;
            settings.ChStochRSI = ChStochRSI;
            settings.ChMACDUPMU = ChMACDUPMU;

            long l;
            if (long.TryParse(Volume, out l))
            {
                settings.Min24HrVolume = l;
            }

            int i;
            if (int.TryParse(APIGetNrOfCandles, out i))
            {
                settings.APIGetNrOfCandles = i;
            }
            if (int.TryParse(MaxFlatCandles, out i))
            {
                settings.MaxFlatCandles = i;
            }
            if (int.TryParse(MaxFlatCandleCount, out i))
            {
                settings.MaxFlatCandleCount = i;
            }

            if (int.TryParse(MaxCandles1stBuy, out i))
            {
                settings.MaxCandles1stBuy = i;
            }

            if (int.TryParse(MaxMFI, out i))
            {
                settings.MaxMFI = i;
            }

            if (int.TryParse(MaxRSI, out i))
            {
                settings.MaxRSI = i;
            }

            if (int.TryParse(BUMFI, out i))
            {
                settings.BUMFI = i;
            }

            if (int.TryParse(BURSI, out i))
            {
                settings.BURSI = i;
            }

            if (int.TryParse(StochD, out i))
            {
                settings.StochD = i;
            }

            if (int.TryParse(StochK, out i))
            {
                settings.StochK = i;
            }
            if (int.TryParse(StochRSID, out i))
            {
                settings.StochRSID = i;
            }
            if (int.TryParse(StochRSIK, out i))
            {
                settings.StochRSIK = i;
            }
            if (int.TryParse(RsiRSI, out i))
            {
                settings.RsiRSI = i;
            }

            if (int.TryParse(RsiRSISell, out i))
            {
                settings.RsiRSISell = i;
            }

            if (int.TryParse(RsiMFI, out i))
            {
                settings.RsiMFI = i;
            }

            if (int.TryParse(RsiMFISell, out i))
            {
                settings.RsiMFISell = i;
            }

            if (int.TryParse(RsiStochRSI, out i))
            {
                settings.RsiStochRSI = i;
            }

            if (int.TryParse(RsiStochRSISell, out i))
            {
                settings.RsiStochRSISell = i;
            }

            if (int.TryParse(AutocloseAfterCandles, out i))
            {
                settings.AutocloseAfterCandles = i;
            }
            if (int.TryParse(AutocloseAfterHr, out i))
            {
                settings.AutocloseAfterHr = i;
            }
            if (int.TryParse(TelegramApiId, out i))
            {
                settings.TelegramApiId = i;
            }
            if (int.TryParse(ExchangeId, out i))
            {
                settings.ExchangeId = i;
            }
            if (int.TryParse(MaxLogLines, out i))
            {
                settings.MaxLogLines = i;
            }
            if (int.TryParse(MaxRecordLines, out i))
            {
                settings.MaxRecordLines = i;
            }

            if (int.TryParse(checkBNBMinutes, out i))
            {
                settings.checkBNBMinutes = i;
            }

            if (int.TryParse(MaxRebuysBU, out i))
            {
                settings.MaxRebuysBU = i;
            }

            decimal d;
            if (decimal.TryParse(MaxPanic, out d))
            {
                settings.MaxPanic = d;
            }

            if (decimal.TryParse(BUBollingerBandWidth, out d))
            {
                settings.BUMinBollingerBandWidth = d;

            }
            if (decimal.TryParse(BUMaxBollingerBandWidth, out d))
            {
                settings.BUMaxBollingerBandWidth = d;
            }

            decimal dec;
            if (decimal.TryParse(MinMC4Signal, out dec))
            {
                settings.MinMC4Signal = dec;
            }

            if (decimal.TryParse(MinMC4Hr4Signal, out dec))
            {
                settings.MinMC4Hr4Signal = dec;
            }

            if (decimal.TryParse(ConPositions, out dec))
            {
                settings.ConPositions = dec;
            }

            if (decimal.TryParse(TradingBudget, out dec))
            {
                settings.TradingBudget = dec;
            }

            if (decimal.TryParse(BUStopLoss, out dec))
            {
                settings.BUStopLoss = dec;
            }
            if (decimal.TryParse(BUStopLoss1, out dec))
            {
                settings.BUStopLoss1 = dec;
            }
            if (decimal.TryParse(BUStopLoss2, out dec))
            {
                settings.BUStopLoss2 = dec;
            }
            if (decimal.TryParse(BUStopLossLast, out dec))
            {
                settings.BUStopLossLast = dec;
            }

            if (decimal.TryParse(PSMC1hr, out dec))
            {
                settings.PSMC1hr = dec;
            }
            if (decimal.TryParse(PSMC4hr, out dec))
            {
                settings.PSMC4hr = dec;
            }
            if (decimal.TryParse(MaxCMDifferencePercentage, out dec))
            {
                settings.MaxCMDifferencePercentage = dec;
            }

            if (decimal.TryParse(LowSatBTC, out dec))
            {
                settings.LowSatBTC = dec;
            }

            if (decimal.TryParse(MinBollingerBandWidth, out dec))
            {
                settings.MinBollingerBandWidth = dec;

            }
            if (decimal.TryParse(MaxBollingerBandWidth, out dec))
            {
                settings.MaxBollingerBandWidth = dec;
            }

            if (decimal.TryParse(MinBNBAmount, out dec))
            {
                settings.MinBNBAmount = dec;
            }
            if (decimal.TryParse(AddBNBAmount, out dec))
            {
                settings.AddBNBAmount = dec;
            }

            settings.Exchange = Exchanges[_dropDownExchange.SelectedIndex];
            settings.WriteLogFileType = WriteLogFileTypes[_dropDownWriteLogFileType.SelectedIndex];
            settings.DataViaOwnAPI = DataViaOwnAPI;

            //            settings.TimeFrame = TimeFrames[_dropDownTimeFrame.SelectedIndex];
            settings.Zignaly = Zignaly;
            settings.Zignaly1min = Zignaly1min;
            settings.Zignaly3min = Zignaly3min;
            settings.Zignaly5min = Zignaly5min;
            settings.Zignaly15min = Zignaly15min;
            settings.Zignaly30min = Zignaly30min;
            settings.Zignaly1hr = Zignaly1hr;
            settings.Zignaly4hr = Zignaly4hr;
            settings.Zignaly1day = Zignaly1day;

            // only A-Z and 0-9 and ,
            Regex rgx = new Regex("[^A-Z0-9,]");
            settings.BlacklistCoinpairs = rgx.Replace(("" + BlacklistCoinpairs).ToUpper(), "");
            settings.WhitelistCoinpairs = rgx.Replace(("" + WhitelistCoinpairs).ToUpper(), "");

            settings.BrowserLocation = "" + BrowserLocation;
            settings.DirDataMap = "" + DirDataMap;
            settings.WriteCsvFile = WriteCsvFile;
            settings.LogInvalidSignals = LogInvalidSignals;
            settings.LogUriOwnAPI = LogUriOwnAPI;
            settings.LogUriBinance = LogUriBinance;
            settings.LogUriZignaly = LogUriZignaly;
            settings.BmValuesToFile = BmValuesToFile;
            settings.BmValuesToFileWhenTfRun = BmValuesToFileWhenTfRun;

            settings.ApikeyBinance = ApiKeyBinance;
            settings.ApiSecretBinance = ApiSecretBinance;
            settings.TelegramApiHash = TelegramApiHash;
            settings.TelegramClientHash = TelegramClientHash;
            settings.TelegramUserNumber = TelegramUserNumber;
            settings.TelegramCode = TelegramCode;

            settings.EnableConfirmDialogs = EnableConfirmDialogs;
            settings.DeleteNotusefullStatesDBRecords = DeleteNotusefullStatesDBRecords;
            settings.DeleteNotusefullActionsDBRecords = DeleteNotusefullActionsDBRecords;
            settings.UpdateClosePrices = UpdateClosePrices;
            settings.ScanAllTimeframesEachMinute = ScanAllTimeframesEachMinute;

            settings.PanicSellActivated = PanicSellActivated;

            settings.LogIndicatorCalculationsForSymbol = LogIndicatorCalculationsForSymbol;

            settings.AddBNBQuote = AddBNBQuote;
            settings.Time2ConvertDust2BNB = Time2ConvertDust2BNB;

            SettingsStore.Save(settings);
            this.Close();
        }

        public static readonly DependencyProperty LogIndicatorCalculationsForSymbolProperty = DependencyProperty.Register("LogIndicatorCalculationsForSymbol", typeof(string), typeof(SettingsDialog));
        public string LogIndicatorCalculationsForSymbol
        {
            get { return (string)GetValue(LogIndicatorCalculationsForSymbolProperty); }
            set { this.SetValue(LogIndicatorCalculationsForSymbolProperty, value); }
        }

        public static readonly DependencyProperty APIGetNrOfCandlesProperty = DependencyProperty.Register("APIGetNrOfCandles", typeof(string), typeof(SettingsDialog));
        public string APIGetNrOfCandles
        {
            get { return (string)GetValue(APIGetNrOfCandlesProperty); }
            set { this.SetValue(APIGetNrOfCandlesProperty, value); }
        }

        public static readonly DependencyProperty DataViaOwnAPIProperty = DependencyProperty.Register("DataViaOwnAPI", typeof(bool), typeof(SettingsDialog));
        public bool DataViaOwnAPI
        {
            get { return (bool)GetValue(DataViaOwnAPIProperty); }
            set { this.SetValue(DataViaOwnAPIProperty, value); }
        }

        public static readonly DependencyProperty CurrencyUSDTProperty = DependencyProperty.Register("CurrencyUSDT", typeof(bool), typeof(SettingsDialog));
        public bool CurrencyUSDT
        {
            get { return (bool)GetValue(CurrencyUSDTProperty); }
            set { this.SetValue(CurrencyUSDTProperty, value); }
        }

        public static readonly DependencyProperty CurrencyEURProperty = DependencyProperty.Register("CurrencyEUR", typeof(bool), typeof(SettingsDialog));
        public bool CurrencyEUR
        {
            get { return (bool)GetValue(CurrencyEURProperty); }
            set { this.SetValue(CurrencyEURProperty, value); }
        }

        public static readonly DependencyProperty CurrencyETHProperty = DependencyProperty.Register("CurrencyETH", typeof(bool), typeof(SettingsDialog));
        public bool CurrencyETH
        {
            get { return (bool)GetValue(CurrencyETHProperty); }
            set { this.SetValue(CurrencyETHProperty, value); }
        }

        public static readonly DependencyProperty CurrencyBNBProperty = DependencyProperty.Register("CurrencyBNB", typeof(bool), typeof(SettingsDialog));
        public bool CurrencyBNB
        {
            get { return (bool)GetValue(CurrencyBNBProperty); }
            set { this.SetValue(CurrencyBNBProperty, value); }
        }

        public static readonly DependencyProperty CurrencyBTCProperty = DependencyProperty.Register("CurrencyBTC", typeof(bool), typeof(SettingsDialog));
        public bool CurrencyBTC
        {
            get { return (bool)GetValue(CurrencyBTCProperty); }
            set { this.SetValue(CurrencyBTCProperty, value); }
        }

        public static readonly DependencyProperty Timeframe1minProperty = DependencyProperty.Register("Timeframe1min", typeof(bool), typeof(SettingsDialog));
        public bool Timeframe1min
        {
            get { return (bool)GetValue(Timeframe1minProperty); }
            set { this.SetValue(Timeframe1minProperty, value); }
        }

        public static readonly DependencyProperty Timeframe3minProperty = DependencyProperty.Register("Timeframe3min", typeof(bool), typeof(SettingsDialog));
        public bool Timeframe3min
        {
            get { return (bool)GetValue(Timeframe3minProperty); }
            set { this.SetValue(Timeframe3minProperty, value); }
        }
        public static readonly DependencyProperty Timeframe5minProperty = DependencyProperty.Register("Timeframe5min", typeof(bool), typeof(SettingsDialog));
        public bool Timeframe5min
        {
            get { return (bool)GetValue(Timeframe5minProperty); }
            set { this.SetValue(Timeframe5minProperty, value); }
        }
        public static readonly DependencyProperty Timeframe15minProperty = DependencyProperty.Register("Timeframe15min", typeof(bool), typeof(SettingsDialog));
        public bool Timeframe15min
        {
            get { return (bool)GetValue(Timeframe15minProperty); }
            set { this.SetValue(Timeframe15minProperty, value); }
        }
        public static readonly DependencyProperty Timeframe30minProperty = DependencyProperty.Register("Timeframe30min", typeof(bool), typeof(SettingsDialog));
        public bool Timeframe30min
        {
            get { return (bool)GetValue(Timeframe30minProperty); }
            set { this.SetValue(Timeframe30minProperty, value); }
        }
        public static readonly DependencyProperty Timeframe1hrProperty = DependencyProperty.Register("Timeframe1hr", typeof(bool), typeof(SettingsDialog));
        public bool Timeframe1hr
        {
            get { return (bool)GetValue(Timeframe1hrProperty); }
            set { this.SetValue(Timeframe1hrProperty, value); }
        }
        public static readonly DependencyProperty Timeframe4hrProperty = DependencyProperty.Register("Timeframe4hr", typeof(bool), typeof(SettingsDialog));
        public bool Timeframe4hr
        {
            get { return (bool)GetValue(Timeframe4hrProperty); }
            set { this.SetValue(Timeframe4hrProperty, value); }
        }

        public static readonly DependencyProperty Timeframe1dayProperty = DependencyProperty.Register("Timeframe1day", typeof(bool), typeof(SettingsDialog));
        public bool Timeframe1day
        {
            get { return (bool)GetValue(Timeframe1dayProperty); }
            set { this.SetValue(Timeframe1dayProperty, value); }
        }

        public static readonly DependencyProperty AllowShortProperty = DependencyProperty.Register("AllowShort", typeof(bool), typeof(SettingsDialog));
        public bool AllowShort
        {
            get { return (bool)GetValue(AllowShortProperty); }
            set { this.SetValue(AllowShortProperty, value); }
        }

        public static readonly DependencyProperty AllowLongProperty = DependencyProperty.Register("AllowLong", typeof(bool), typeof(SettingsDialog));
        public bool AllowLong
        {
            get { return (bool)GetValue(AllowLongProperty); }
            set { this.SetValue(AllowLongProperty, value); }
        }

        public static readonly DependencyProperty AutoTradeProperty = DependencyProperty.Register("AutoTrade", typeof(bool), typeof(SettingsDialog));
        public bool AutoTrade
        {
            get { return (bool)GetValue(AutoTradeProperty); }
            set { this.SetValue(AutoTradeProperty, value); }
        }

        public static readonly DependencyProperty TradeExchangeDirectProperty = DependencyProperty.Register("TradeExchangeDirect", typeof(bool), typeof(SettingsDialog));
        public bool TradeExchangeDirect
        {
            get { return (bool)GetValue(TradeExchangeDirectProperty); }
            set { this.SetValue(TradeExchangeDirectProperty, value); }
        }

        public static readonly DependencyProperty PlaySoundsProperty = DependencyProperty.Register("PlaySounds", typeof(bool), typeof(SettingsDialog));
        public bool PlaySounds
        {
            get { return (bool)GetValue(PlaySoundsProperty); }
            set { this.SetValue(PlaySoundsProperty, value); }
        }

        public static readonly DependencyProperty MaxCandles1stBuyProperty = DependencyProperty.Register("MaxCandles1stBuy", typeof(string), typeof(SettingsDialog));
        public string MaxCandles1stBuy
        {
            get { return (string)GetValue(MaxCandles1stBuyProperty); }
            set { this.SetValue(MaxCandles1stBuyProperty, value); }
        }

        public static readonly DependencyProperty AutocloseAfterCandlesProperty = DependencyProperty.Register("AutocloseAfterCandles", typeof(string), typeof(SettingsDialog));
        public string AutocloseAfterCandles
        {
            get { return (string)GetValue(AutocloseAfterCandlesProperty); }
            set { this.SetValue(AutocloseAfterCandlesProperty, value); }
        }

        public static readonly DependencyProperty AutocloseAfterHrProperty = DependencyProperty.Register("AutocloseAfterHr", typeof(string), typeof(SettingsDialog));
        public string AutocloseAfterHr
        {
            get { return (string)GetValue(AutocloseAfterHrProperty); }
            set { this.SetValue(AutocloseAfterHrProperty, value); }
        }

        public static readonly DependencyProperty MinMC4SignalProperty = DependencyProperty.Register("MinMC4Signal", typeof(string), typeof(SettingsDialog));
        public string MinMC4Signal
        {
            get { return (string)GetValue(MinMC4SignalProperty); }
            set { this.SetValue(MinMC4SignalProperty, value); }
        }

        public static readonly DependencyProperty MinMC4Hr4SignalProperty = DependencyProperty.Register("MinMC4Hr4Signal", typeof(string), typeof(SettingsDialog));
        public string MinMC4Hr4Signal
        {
            get { return (string)GetValue(MinMC4Hr4SignalProperty); }
            set { this.SetValue(MinMC4Hr4SignalProperty, value); }
        }

        public static readonly DependencyProperty Only1stBuyProperty = DependencyProperty.Register("Only1stBuy", typeof(bool), typeof(SettingsDialog));
        public bool Only1stBuy
        {
            get { return (bool)GetValue(Only1stBuyProperty); }
            set { this.SetValue(Only1stBuyProperty, value); }
        }

        public static readonly DependencyProperty EnableConfirmDialogsProperty = DependencyProperty.Register("EnableConfirmDialogs", typeof(bool), typeof(SettingsDialog));
        public bool EnableConfirmDialogs
        {
            get { return (bool)GetValue(EnableConfirmDialogsProperty); }
            set { this.SetValue(EnableConfirmDialogsProperty, value); }
        }

        public static readonly DependencyProperty DeleteNotusefullStatesDBRecordsProperty = DependencyProperty.Register("DeleteNotusefullStatesDBRecords", typeof(bool), typeof(SettingsDialog));
        public bool DeleteNotusefullStatesDBRecords
        {
            get { return (bool)GetValue(DeleteNotusefullStatesDBRecordsProperty); }
            set { this.SetValue(DeleteNotusefullStatesDBRecordsProperty, value); }
        }

        public static readonly DependencyProperty DeleteNotusefullActionsDBRecordsProperty = DependencyProperty.Register("DeleteNotusefullActionDBRecords", typeof(bool), typeof(SettingsDialog));
        public bool DeleteNotusefullActionsDBRecords
        {
            get { return (bool)GetValue(DeleteNotusefullActionsDBRecordsProperty); }
            set { this.SetValue(DeleteNotusefullActionsDBRecordsProperty, value); }
        }

        public static readonly DependencyProperty UpdateClosePricesProperty = DependencyProperty.Register("UpdateClosePrices", typeof(bool), typeof(SettingsDialog));
        public bool UpdateClosePrices
        {
            get { return (bool)GetValue(UpdateClosePricesProperty); }
            set { this.SetValue(UpdateClosePricesProperty, value); }
        }

        public static readonly DependencyProperty ScanAllTimeframesEachMinuteProperty = DependencyProperty.Register("ScanAllTimeframesEachMinute", typeof(bool), typeof(SettingsDialog));
        public bool ScanAllTimeframesEachMinute
        {
            get { return (bool)GetValue(ScanAllTimeframesEachMinuteProperty); }
            set { this.SetValue(ScanAllTimeframesEachMinuteProperty, value); }
        }

        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(string), typeof(SettingsDialog));
        public string Volume
        {
            get { return (string)GetValue(VolumeProperty); }
            set { this.SetValue(VolumeProperty, value); }
        }

        public static readonly DependencyProperty LowSatBTCProperty = DependencyProperty.Register("LowSatBTC", typeof(string), typeof(SettingsDialog));
        public string LowSatBTC
        {
            get { return (string)GetValue(LowSatBTCProperty); }
            set { this.SetValue(LowSatBTCProperty, value); }
        }

        public static readonly DependencyProperty MinBollingerBandWidthProperty = DependencyProperty.Register("MinBollingerBandWidth", typeof(string), typeof(SettingsDialog));
        public string MinBollingerBandWidth
        {
            get { return (string)GetValue(MinBollingerBandWidthProperty); }
            set { this.SetValue(MinBollingerBandWidthProperty, value); }
        }

        public static readonly DependencyProperty BUBollingerBandWidthProperty = DependencyProperty.Register("BUBollingerBandWidth", typeof(string), typeof(SettingsDialog));
        public string BUBollingerBandWidth
        {
            get { return (string)GetValue(BUBollingerBandWidthProperty); }
            set { this.SetValue(BUBollingerBandWidthProperty, value); }
        }

        public static readonly DependencyProperty MaxBollingerBandWidthProperty = DependencyProperty.Register("MaxBollingerBandWidth", typeof(string), typeof(SettingsDialog));
        public string MaxBollingerBandWidth
        {
            get { return (string)GetValue(MaxBollingerBandWidthProperty); }
            set { this.SetValue(MaxBollingerBandWidthProperty, value); }
        }

        public static readonly DependencyProperty BUMaxBollingerBandWidthProperty = DependencyProperty.Register("BUMaxBollingerBandWidth", typeof(string), typeof(SettingsDialog));
        public string BUMaxBollingerBandWidth
        {
            get { return (string)GetValue(BUMaxBollingerBandWidthProperty); }
            set { this.SetValue(BUMaxBollingerBandWidthProperty, value); }
        }

        public static readonly DependencyProperty BUStopLossProperty = DependencyProperty.Register("BUStopLoss", typeof(string), typeof(SettingsDialog));
        public string BUStopLoss
        {
            get { return (string)GetValue(BUStopLossProperty); }
            set { this.SetValue(BUStopLossProperty, value); }
        }
        public static readonly DependencyProperty BUStopLoss1Property = DependencyProperty.Register("BUStopLoss1", typeof(string), typeof(SettingsDialog));
        public string BUStopLoss1
        {
            get { return (string)GetValue(BUStopLoss1Property); }
            set { this.SetValue(BUStopLoss1Property, value); }
        }
        public static readonly DependencyProperty BUStopLoss2Property = DependencyProperty.Register("BUStopLoss2", typeof(string), typeof(SettingsDialog));
        public string BUStopLoss2
        {
            get { return (string)GetValue(BUStopLoss2Property); }
            set { this.SetValue(BUStopLoss2Property, value); }
        }
        public static readonly DependencyProperty BUStopLossLastProperty = DependencyProperty.Register("BUStopLossLast", typeof(string), typeof(SettingsDialog));
        public string BUStopLossLast
        {
            get { return (string)GetValue(BUStopLossLastProperty); }
            set { this.SetValue(BUStopLossLastProperty, value); }
        }

        public static readonly DependencyProperty PSMC1hrProperty = DependencyProperty.Register("PSMC1hr", typeof(string), typeof(SettingsDialog));
        public string PSMC1hr
        {
            get { return (string)GetValue(PSMC1hrProperty); }
            set { this.SetValue(PSMC1hrProperty, value); }
        }
        public static readonly DependencyProperty PSMC4hrProperty = DependencyProperty.Register("PSMC4hr", typeof(string), typeof(SettingsDialog));
        public string PSMC4hr
        {
            get { return (string)GetValue(PSMC4hrProperty); }
            set { this.SetValue(PSMC4hrProperty, value); }
        }
        public static readonly DependencyProperty MaxCMDifferencePercentageProperty = DependencyProperty.Register("MaxCMDifferencePercentage", typeof(string), typeof(SettingsDialog));
        public string MaxCMDifferencePercentage
        {
            get { return (string)GetValue(MaxCMDifferencePercentageProperty); }
            set { this.SetValue(MaxCMDifferencePercentageProperty, value); }
        }

        public static readonly DependencyProperty MaxFlatCandlesProperty = DependencyProperty.Register("MaxFlatCandles", typeof(string), typeof(SettingsDialog));
        public string MaxFlatCandles
        {
            get { return (string)GetValue(MaxFlatCandlesProperty); }
            set { this.SetValue(MaxFlatCandlesProperty, value); }
        }

        public static readonly DependencyProperty MaxFlatCandleCountProperty = DependencyProperty.Register("MaxFlatCandleCount", typeof(string), typeof(SettingsDialog));
        public string MaxFlatCandleCount
        {
            get { return (string)GetValue(MaxFlatCandleCountProperty); }
            set { this.SetValue(MaxFlatCandleCountProperty, value); }
        }

        public static readonly DependencyProperty MaxPanicProperty = DependencyProperty.Register("MaxPanic", typeof(string), typeof(SettingsDialog));
        public string MaxPanic
        {
            get { return (string)GetValue(MaxPanicProperty); }
            set { this.SetValue(MaxPanicProperty, value); }
        }

        public static readonly DependencyProperty MaxMFIProperty = DependencyProperty.Register("MaxMFI", typeof(string), typeof(SettingsDialog));
        public string MaxMFI
        {
            get { return (string)GetValue(MaxMFIProperty); }
            set { this.SetValue(MaxMFIProperty, value); }
        }

        public static readonly DependencyProperty MaxRSIProperty = DependencyProperty.Register("MaxRSI", typeof(string), typeof(SettingsDialog));
        public string MaxRSI
        {
            get { return (string)GetValue(MaxRSIProperty); }
            set { this.SetValue(MaxRSIProperty, value); }
        }

        public static readonly DependencyProperty BUMFIProperty = DependencyProperty.Register("BUMFI", typeof(string), typeof(SettingsDialog));
        public string BUMFI
        {
            get { return (string)GetValue(BUMFIProperty); }
            set { this.SetValue(BUMFIProperty, value); }
        }

        public static readonly DependencyProperty BURSIProperty = DependencyProperty.Register("BURSI", typeof(string), typeof(SettingsDialog));
        public string BURSI
        {
            get { return (string)GetValue(BURSIProperty); }
            set { this.SetValue(BURSIProperty, value); }
        }

        public static readonly DependencyProperty StochDProperty = DependencyProperty.Register("StochD", typeof(string), typeof(SettingsDialog));
        public string StochD
        {
            get { return (string)GetValue(StochDProperty); }
            set { this.SetValue(StochDProperty, value); }
        }

        public static readonly DependencyProperty StochKProperty = DependencyProperty.Register("StochK", typeof(string), typeof(SettingsDialog));
        public string StochK
        {
            get { return (string)GetValue(StochKProperty); }
            set { this.SetValue(StochKProperty, value); }
        }

        public static readonly DependencyProperty StochRSIDProperty = DependencyProperty.Register("StochRSID", typeof(string), typeof(SettingsDialog));
        public string StochRSID
        {
            get { return (string)GetValue(StochRSIDProperty); }
            set { this.SetValue(StochRSIDProperty, value); }
        }

        public static readonly DependencyProperty StochRSIKProperty = DependencyProperty.Register("StochRSIK", typeof(string), typeof(SettingsDialog));
        public string StochRSIK
        {
            get { return (string)GetValue(StochRSIKProperty); }
            set { this.SetValue(StochRSIKProperty, value); }
        }

        public static readonly DependencyProperty ZignalyProperty = DependencyProperty.Register("Zignaly", typeof(string), typeof(SettingsDialog));
        public string Zignaly
        {
            get { return (string)GetValue(ZignalyProperty); }
            set { this.SetValue(ZignalyProperty, value); }
        }

        public static readonly DependencyProperty Zignaly1minProperty = DependencyProperty.Register("Zignaly1min", typeof(string), typeof(SettingsDialog));
        public string Zignaly1min
        {
            get { return (string)GetValue(Zignaly1minProperty); }
            set { this.SetValue(Zignaly1minProperty, value); }
        }

        public static readonly DependencyProperty Zignaly3minProperty = DependencyProperty.Register("Zignaly3min", typeof(string), typeof(SettingsDialog));
        public string Zignaly3min
        {
            get { return (string)GetValue(Zignaly3minProperty); }
            set { this.SetValue(Zignaly3minProperty, value); }
        }

        public static readonly DependencyProperty Zignaly5minProperty = DependencyProperty.Register("Zignaly5min", typeof(string), typeof(SettingsDialog));
        public string Zignaly5min
        {
            get { return (string)GetValue(Zignaly5minProperty); }
            set { this.SetValue(Zignaly5minProperty, value); }
        }

        public static readonly DependencyProperty Zignaly15minProperty = DependencyProperty.Register("Zignaly15min", typeof(string), typeof(SettingsDialog));
        public string Zignaly15min
        {
            get { return (string)GetValue(Zignaly15minProperty); }
            set { this.SetValue(Zignaly15minProperty, value); }
        }

        public static readonly DependencyProperty Zignaly30minProperty = DependencyProperty.Register("Zignaly30min", typeof(string), typeof(SettingsDialog));
        public string Zignaly30min
        {
            get { return (string)GetValue(Zignaly30minProperty); }
            set { this.SetValue(Zignaly30minProperty, value); }
        }

        public static readonly DependencyProperty Zignaly1hrProperty = DependencyProperty.Register("Zignaly1hr", typeof(string), typeof(SettingsDialog));
        public string Zignaly1hr
        {
            get { return (string)GetValue(Zignaly1hrProperty); }
            set { this.SetValue(Zignaly1hrProperty, value); }
        }

        public static readonly DependencyProperty Zignaly4hrProperty = DependencyProperty.Register("Zignaly4hr", typeof(string), typeof(SettingsDialog));
        public string Zignaly4hr
        {
            get { return (string)GetValue(Zignaly4hrProperty); }
            set { this.SetValue(Zignaly4hrProperty, value); }
        }

        public static readonly DependencyProperty Zignaly1dayProperty = DependencyProperty.Register("Zignaly1day", typeof(string), typeof(SettingsDialog));
        public string Zignaly1day
        {
            get { return (string)GetValue(Zignaly1dayProperty); }
            set { this.SetValue(Zignaly1dayProperty, value); }
        }

        public static readonly DependencyProperty BlacklistCoinpairsProperty = DependencyProperty.Register("BlacklistCoinpairs", typeof(string), typeof(SettingsDialog));
        public string BlacklistCoinpairs
        {
            get { return (string)GetValue(BlacklistCoinpairsProperty); }
            set { this.SetValue(BlacklistCoinpairsProperty, value); }
        }

        public static readonly DependencyProperty WhitelistCoinpairsProperty = DependencyProperty.Register("WhitelistCoinpairs", typeof(string), typeof(SettingsDialog));
        public string WhitelistCoinpairs
        {
            get { return (string)GetValue(WhitelistCoinpairsProperty); }
            set { this.SetValue(WhitelistCoinpairsProperty, value); }
        }

        public static readonly DependencyProperty BrowserLocationProperty = DependencyProperty.Register("BrowserLocation", typeof(string), typeof(SettingsDialog));
        public string BrowserLocation
        {
            get { return (string)GetValue(BrowserLocationProperty); }
            set { this.SetValue(BrowserLocationProperty, value); }
        }

        public static readonly DependencyProperty DirDataMapProperty = DependencyProperty.Register("DirDataMap", typeof(string), typeof(SettingsDialog));
        public string DirDataMap
        {
            get { return (string)GetValue(DirDataMapProperty); }
            set { this.SetValue(DirDataMapProperty, value); }
        }

        public static readonly DependencyProperty RsiRSIProperty = DependencyProperty.Register("RsiRSI", typeof(string), typeof(SettingsDialog));
        public string RsiRSI
        {
            get { return (string)GetValue(RsiRSIProperty); }
            set { this.SetValue(RsiRSIProperty, value); }
        }

        public static readonly DependencyProperty RsiRSISellProperty = DependencyProperty.Register("RsiRSISell", typeof(string), typeof(SettingsDialog));
        public string RsiRSISell
        {
            get { return (string)GetValue(RsiRSISellProperty); }
            set { this.SetValue(RsiRSISellProperty, value); }
        }

        public static readonly DependencyProperty RsiStochRSIProperty = DependencyProperty.Register("RsiStochRSI", typeof(string), typeof(SettingsDialog));
        public string RsiStochRSI
        {
            get { return (string)GetValue(RsiStochRSIProperty); }
            set { this.SetValue(RsiStochRSIProperty, value); }
        }

        public static readonly DependencyProperty RsiStochRSISellProperty = DependencyProperty.Register("RsiStochRSISell", typeof(string), typeof(SettingsDialog));
        public string RsiStochRSISell
        {
            get { return (string)GetValue(RsiStochRSISellProperty); }
            set { this.SetValue(RsiStochRSISellProperty, value); }
        }

        public static readonly DependencyProperty RsiMFIProperty = DependencyProperty.Register("RsiMFI", typeof(string), typeof(SettingsDialog));
        public string RsiMFI
        {
            get { return (string)GetValue(RsiMFIProperty); }
            set { this.SetValue(RsiMFIProperty, value); }
        }
        public static readonly DependencyProperty RsiMFISellProperty = DependencyProperty.Register("RsiMFISell", typeof(string), typeof(SettingsDialog));
        public string RsiMFISell
        {
            get { return (string)GetValue(RsiMFISellProperty); }
            set { this.SetValue(RsiMFISellProperty, value); }
        }

        private void cbBottumUp_Click(object sender, RoutedEventArgs e)
        {
            if (cbBottumUp.IsChecked == true)
            {
                cbBottumUp.IsChecked = true;
                cbMiddleUp.IsChecked = false;
                cbFamily.IsChecked = false;

            }
        }

        private void cbMiddleUp_Click(object sender, RoutedEventArgs e)
        {
            if (cbMiddleUp.IsChecked == true)
            {
                cbBottumUp.IsChecked = false;
                cbMiddleUp.IsChecked = true;
                cbFamily.IsChecked = false;
            }
        }
        private void cbFamily_Click(object sender, RoutedEventArgs e)
        {
            if (cbFamily.IsChecked == true)
            {
                cbFamily.IsChecked = true;
                cbBottumUp.IsChecked = false;
                cbMiddleUp.IsChecked = false;
            }
        }

        public static readonly DependencyProperty ChBottumUpProperty = DependencyProperty.Register("ChBottumUp", typeof(bool), typeof(SettingsDialog));
        public bool ChBottumUp
        {
            get { return (bool)GetValue(ChBottumUpProperty); }
            set { this.SetValue(ChBottumUpProperty, value); }
        }

        public static readonly DependencyProperty ChMiddleUPProperty = DependencyProperty.Register("ChMiddleUP", typeof(bool), typeof(SettingsDialog));
        public bool ChMiddleUP
        {
            get { return (bool)GetValue(ChMiddleUPProperty); }
            set { this.SetValue(ChMiddleUPProperty, value); }
        }

        public static readonly DependencyProperty ChRSIFamilyProperty = DependencyProperty.Register("ChRSIFamily", typeof(bool), typeof(SettingsDialog));
        public bool ChRSIFamily
        {
            get { return (bool)GetValue(ChRSIFamilyProperty); }
            set { this.SetValue(ChRSIFamilyProperty, value); }
        }

        public static readonly DependencyProperty ChTradingviewProperty = DependencyProperty.Register("ChTradingview", typeof(bool), typeof(SettingsDialog));
        public bool ChTradingview
        {
            get { return (bool)GetValue(ChTradingviewProperty); }
            set { this.SetValue(ChTradingviewProperty, value); }
        }

        public static readonly DependencyProperty ChAltradyProperty = DependencyProperty.Register("ChAltrady", typeof(bool), typeof(SettingsDialog));
        public bool ChAltrady
        {
            get { return (bool)GetValue(ChAltradyProperty); }
            set { this.SetValue(ChAltradyProperty, value); }
        }

        public static readonly DependencyProperty ChHyperTraderProperty = DependencyProperty.Register("ChHyperTrader", typeof(bool), typeof(SettingsDialog));
        public bool ChHyperTrader
        {
            get { return (bool)GetValue(ChHyperTraderProperty); }
            set { this.SetValue(ChHyperTraderProperty, value); }
        }

        public static readonly DependencyProperty ChStochProperty = DependencyProperty.Register("ChStoch", typeof(bool), typeof(SettingsDialog));
        public bool ChStoch
        {
            get { return (bool)GetValue(ChStochProperty); }
            set { this.SetValue(ChStochProperty, value); }
        }

        public static readonly DependencyProperty ChStochRSIProperty = DependencyProperty.Register("ChStochRSI", typeof(bool), typeof(SettingsDialog));
        public bool ChStochRSI
        {
            get { return (bool)GetValue(ChStochRSIProperty); }
            set { this.SetValue(ChStochRSIProperty, value); }
        }

        public static readonly DependencyProperty ChMACDUPMUProperty = DependencyProperty.Register("ChMACDUPMU", typeof(bool), typeof(SettingsDialog));
        public bool ChMACDUPMU
        {
            get { return (bool)GetValue(ChMACDUPMUProperty); }
            set { this.SetValue(ChMACDUPMUProperty, value); }
        }

        private void cbStoch_Click(object sender, RoutedEventArgs e)
        {
            if (cbStoch.IsChecked == true)
            {
                cbStoch.IsChecked = true;
                cbStochRSI.IsChecked = false;
            }
        }

        private void cbStochRSI_Click(object sender, RoutedEventArgs e)
        {
            if (cbStochRSI.IsChecked == true)
            {
                cbStochRSI.IsChecked = true;
                cbStoch.IsChecked = false;

            }
        }

        public static readonly DependencyProperty WriteCsvFileProperty = DependencyProperty.Register("WriteCsvFile", typeof(bool), typeof(SettingsDialog));
        public bool WriteCsvFile
        {
            get { return (bool)GetValue(WriteCsvFileProperty); }
            set { this.SetValue(WriteCsvFileProperty, value); }
        }

        public static readonly DependencyProperty LogInvalidSignalsProperty = DependencyProperty.Register("LogInvalidSignals", typeof(bool), typeof(SettingsDialog));
        public bool LogInvalidSignals
        {
            get { return (bool)GetValue(LogInvalidSignalsProperty); }
            set { this.SetValue(LogInvalidSignalsProperty, value); }
        }

        public static readonly DependencyProperty BmValuesToFileProperty = DependencyProperty.Register("BmValuesToFile", typeof(bool), typeof(SettingsDialog));
        public bool BmValuesToFile
        {
            get { return (bool)GetValue(BmValuesToFileProperty); }
            set { this.SetValue(BmValuesToFileProperty, value); }
        }

        public static readonly DependencyProperty BmValuesToFileWhenTfRunProperty = DependencyProperty.Register("BmValuesToFileWhenTfRun", typeof(bool), typeof(SettingsDialog));
        public bool BmValuesToFileWhenTfRun
        {
            get { return (bool)GetValue(BmValuesToFileWhenTfRunProperty); }
            set { this.SetValue(BmValuesToFileWhenTfRunProperty, value); }
        }

        public static readonly DependencyProperty LogUriOwnAPIProperty = DependencyProperty.Register("LogUriOwnAPI", typeof(bool), typeof(SettingsDialog));
        public bool LogUriOwnAPI
        {
            get { return (bool)GetValue(LogUriOwnAPIProperty); }
            set { this.SetValue(LogUriOwnAPIProperty, value); }
        }

        public static readonly DependencyProperty LogUriBinanceProperty = DependencyProperty.Register("LogUriBinance", typeof(bool), typeof(SettingsDialog));
        public bool LogUriBinance
        {
            get { return (bool)GetValue(LogUriBinanceProperty); }
            set { this.SetValue(LogUriBinanceProperty, value); }
        }

        public static readonly DependencyProperty LogUriZignalyProperty = DependencyProperty.Register("LogUriZignaly", typeof(bool), typeof(SettingsDialog));
        public bool LogUriZignaly
        {
            get { return (bool)GetValue(LogUriZignalyProperty); }
            set { this.SetValue(LogUriZignalyProperty, value); }
        }

        public static readonly DependencyProperty PanicSellActivatedProperty = DependencyProperty.Register("PanicSellActivated", typeof(bool), typeof(SettingsDialog));
        public bool PanicSellActivated
        {
            get { return (bool)GetValue(PanicSellActivatedProperty); }
            set { this.SetValue(PanicSellActivatedProperty, value); }
        }


        //        private void cbTradingview_Click(object sender, RoutedEventArgs e)
        //        {
        //            if (cbTradingview.IsChecked == true)
        //            {
        //                cbTradingview.IsChecked = true;
        //                cbAltrady.IsChecked = false;
        //                cbHyperTrader.IsChecked = false;

        //            }
        //        }

        //        private void cbAltrady_Click(object sender, RoutedEventArgs e)
        //        {
        //            if (cbAltrady.IsChecked == true)
        //            {
        //                cbTradingview.IsChecked = false;
        //                cbAltrady.IsChecked = true;
        //                cbHyperTrader.IsChecked = false;

        //            }
        //        }

        //        private void cbHyperTrader_Click(object sender, RoutedEventArgs e)
        //        {
        //            if (cbHyperTrader.IsChecked == true)
        //            {
        //                cbTradingview.IsChecked = false;
        //                cbAltrady.IsChecked = false;
        //                cbHyperTrader.IsChecked = true;

        //            }
        //        }

        public static readonly DependencyProperty ApiKeyBinanceProperty = DependencyProperty.Register("ApiKeyBinance", typeof(string), typeof(SettingsDialog));
        public string ApiKeyBinance
        {
            get { return (string)GetValue(ApiKeyBinanceProperty); }
            set { this.SetValue(ApiKeyBinanceProperty, value); }
        }
        public static readonly DependencyProperty ApiSecretBinanceProperty = DependencyProperty.Register("ApiSecretBinance", typeof(string), typeof(SettingsDialog));
        public string ApiSecretBinance
        {
            get { return (string)GetValue(ApiSecretBinanceProperty); }
            set { this.SetValue(ApiSecretBinanceProperty, value); }
        }

        public static readonly DependencyProperty TelegramApiIdProperty = DependencyProperty.Register("TelegramApiId", typeof(string), typeof(SettingsDialog));
        public string TelegramApiId
        {
            get { return (string)GetValue(TelegramApiIdProperty); }
            set { this.SetValue(TelegramApiIdProperty, value); }
        }
        public static readonly DependencyProperty TelegramApiHashProperty = DependencyProperty.Register("TelegramApiHash", typeof(string), typeof(SettingsDialog));
        public string TelegramApiHash
        {
            get { return (string)GetValue(TelegramApiHashProperty); }
            set { this.SetValue(TelegramApiHashProperty, value); }
        }
        //public static readonly DependencyProperty TelegramClientHashProperty = DependencyProperty.Register("TelegramClientHash", typeof(string), typeof(SettingsDialog));
        public string TelegramClientHash { get; set; }
        //{
        //    get { return (string)GetValue(TelegramClientHashProperty); }
        //    set { this.SetValue(TelegramClientHashProperty, value); }
        //}
        public static readonly DependencyProperty TelegramUserNumberProperty = DependencyProperty.Register("TelegramUserNumber", typeof(string), typeof(SettingsDialog));
        public string TelegramUserNumber
        {
            get { return (string)GetValue(TelegramUserNumberProperty); }
            set { this.SetValue(TelegramUserNumberProperty, value); }
        }
        public static readonly DependencyProperty TelegramCodeProperty = DependencyProperty.Register("TelegramCode", typeof(string), typeof(SettingsDialog));
        public string TelegramCode
        {
            get { return (string)GetValue(TelegramCodeProperty); }
            set { this.SetValue(TelegramCodeProperty, value); }
        }

        public static readonly DependencyProperty TradingBudgetProperty = DependencyProperty.Register("TradingBudget", typeof(string), typeof(SettingsDialog));
        public string TradingBudget
        {
            get { return (string)GetValue(TradingBudgetProperty); }
            set { this.SetValue(TradingBudgetProperty, value); }
        }

        public static readonly DependencyProperty ConPositionsProperty = DependencyProperty.Register("ConPositions", typeof(string), typeof(SettingsDialog));
        public string ConPositions
        {
            get { return (string)GetValue(ConPositionsProperty); }
            set { this.SetValue(ConPositionsProperty, value); }
        }

        private void DlgDataMap_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog DlgDataMap = new FolderBrowserDialog();
            if (DlgDataMap.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                DlgDataMap.Description = "Select folder to save logging";
            DirDataMap = DlgDataMap.SelectedPath;
        }

        public static readonly DependencyProperty ExchangeIdProperty = DependencyProperty.Register("ExchangeId", typeof(string), typeof(SettingsDialog));
        public string ExchangeId
        {
            get { return (string)GetValue(ExchangeIdProperty); }
            set { this.SetValue(ExchangeIdProperty, value); }
        }

        public static readonly DependencyProperty MaxLogLinesProperty = DependencyProperty.Register("MaxLogLines", typeof(string), typeof(SettingsDialog));
        public string MaxLogLines
        {
            get { return (string)GetValue(MaxLogLinesProperty); }
            set { this.SetValue(MaxLogLinesProperty, value); }
        }
        public static readonly DependencyProperty MaxRecordLinesProperty = DependencyProperty.Register("MaxRecordLines", typeof(string), typeof(SettingsDialog));
        public string MaxRecordLines
        {
            get { return (string)GetValue(MaxRecordLinesProperty); }
            set { this.SetValue(MaxRecordLinesProperty, value); }
        }

        public static readonly DependencyProperty MinBNBAmountProperty = DependencyProperty.Register("MinBNBAmount", typeof(string), typeof(SettingsDialog));
        public string MinBNBAmount
        {
            get { return (string)GetValue(MinBNBAmountProperty); }
            set { this.SetValue(MinBNBAmountProperty, value); }
        }
        public static readonly DependencyProperty AddBNBAmountProperty = DependencyProperty.Register("AddBNBAmount", typeof(string), typeof(SettingsDialog));
        public string AddBNBAmount
        {
            get { return (string)GetValue(AddBNBAmountProperty); }
            set { this.SetValue(AddBNBAmountProperty, value); }
        }
        public static readonly DependencyProperty checkBNBMinutesProperty = DependencyProperty.Register("checkBNBMinutes", typeof(string), typeof(SettingsDialog));
        public string checkBNBMinutes
        {
            get { return (string)GetValue(checkBNBMinutesProperty); }
            set { this.SetValue(checkBNBMinutesProperty, value); }
        }
        public static readonly DependencyProperty AddBNBQuoteProperty = DependencyProperty.Register("AddBNBQuote", typeof(string), typeof(SettingsDialog));
        public string AddBNBQuote
        {
            get { return (string)GetValue(AddBNBQuoteProperty); }
            set { this.SetValue(AddBNBQuoteProperty, value); }
        }

        public static readonly DependencyProperty MaxRebuysBUProperty = DependencyProperty.Register("MaxRebuysBU", typeof(string), typeof(SettingsDialog));
        public string MaxRebuysBU
        {
            get { return (string)GetValue(MaxRebuysBUProperty); }
            set { this.SetValue(MaxRebuysBUProperty, value); }
        }

        public static readonly DependencyProperty Time2ConvertDust2BNBProperty = DependencyProperty.Register("Time2ConvertDust2BNB", typeof(string), typeof(SettingsDialog));
        public string Time2ConvertDust2BNB
        {
            get { return (string)GetValue(Time2ConvertDust2BNBProperty); }
            set { this.SetValue(Time2ConvertDust2BNBProperty, value); }
        }

    }
}
