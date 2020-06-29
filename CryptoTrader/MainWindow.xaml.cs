using CryptoTraderScanner;
using CryptoTraderStandard.BinanceNetApi;
using CryptoTraderStandard.coinmarketdataApi;
using CryptoTraderStandard.ScannerSQLiteDB;
using MahApps.Metro.Controls;
using Serilog;
using System;
//using System.ServiceProcess;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace CryptoTrader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private const string AppTitle = "Trading Crypto Scanner v1.9.12.13 ALPHA";

        private static readonly log4net.ILog MainLog4Net = log4net.LogManager.GetLogger
                        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string SOUND_TADA_FILE = "C:\\Windows\\Media\\tada.wav";
        private System.Windows.Controls.ComboBox _ddOrderbookSelectSymbol;
        public ObservableCollection<string> OrderbookSelectSymbols { get; set; }
        private Scanner _scanner;
        private ScannerSQLiteDbStore SQLiteDbStore;
        private BinanceNetApiClient BinanceApi;
        //private TelegramClientAPI ZignalyTelegramApi;
        //private string PreviousGetMarketPairsTime = "";
        private List<MarketPairsCMD> UsedMarketPairs = new List<MarketPairsCMD>();
        private System.Windows.Controls.ListBox _listBoxGrid;
        private int LastExchangeId = 0;
        private bool LastScanAllTimeframesEachMinute = false;

        bool BarometerRefreshed_1Hr = false;
        bool BarometerRefreshed_4Hr = false;
        bool BarometerRefreshed_1D = false;
        const int BAROMETERDEPTH = 3;
        private BarometerCMD[]
            _BarometerData1Hr = new BarometerCMD[BAROMETERDEPTH],
            _BarometerData4Hr = new BarometerCMD[BAROMETERDEPTH],
            _BarometerData1D = new BarometerCMD[BAROMETERDEPTH];
        private int BarometerPoints = -1;
        private string BarometerText = "";
        //private Decimal minBm2Autotrade = (Decimal)0.1;
        private bool UsedMarkedpairsRefreshed = false;
        private bool CanStartATimeFrame1stBuy = false;
        private bool CanStartATimeFrameNextActions = false;
        private bool AlreadyRunNextScanForAllTimeframes = false;

        private bool dgAllCurrentSignalsOnceVisible = false;
        private List<SignalsTBL.SignalStates> VisibleSignalStates = new List<SignalsTBL.SignalStates>();
        private List<SignalsTBL.SignalActions> VisibleSignalActions = new List<SignalsTBL.SignalActions>();

        private StreamWriter swLogFile = null;
        private StreamWriter swCSVFile = null;
        private StreamWriter swBMCSVFile = null;

        //private readonly string[] sliderposNames = {"Off", "On", "Pause"};
        public struct MainThread
        {
            public string name;
            public int minutes;
            public bool running;
            public bool pause;
            public System.Timers.Timer timer1stBuy;
            public System.Timers.Timer timerNextAction;
            public DateTime startdt;
            public DateTime nextDt1stBuy;
            public DateTime nextDtNextAction;
        };
        public MainThread thread1min, thread3min, thread5min, thread15min, thread30min, thread1hr, thread4hr, thread1day;

        //public string time { set; get; }

        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        private static readonly HttpClient httpclient = new HttpClient();

        public MainWindow()
        {
            DataContext = this;
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.File("logs\\ScannerSerilog-.txt", rollingInterval: RollingInterval.Hour)
                            .CreateLogger();

            AddLogMessage($"Start app: {AppTitle}");
            StatusText = "Started";
            ListBoxSignals = new ObservableCollection<SignalsTBL>();
            AllCurrentSignals = new ObservableCollection<SignalsTBL>();
            GridOrderbook = new ObservableCollection<OrderBIN>();
            GridAllLiteDBSignals = new ObservableCollection<SignalsTBL>();
            InitializeComponent();
            Load();
        }

        private async void Load()
        {
            // this.AttachDevTools();
            Title = AppTitle;

            menuItemSettings.Click += MenuItemSettings_Click;
            menuItemQuit.Click += MenuItemQuit_Click;

            //TabControlMain.SelectionChanged += TabControl_SelectionChanged;
            lstbx_Signals.MouseDoubleClick += Lstbx_Signals_MouseDoubleClick;
            lstbx_Signals.SelectionChanged += Lstbx_Signals_SelectionChanged;
            _listBoxGrid = lstbx_Signals;
            _ddOrderbookSelectSymbol = cbOrderbookSelectSymbol;
            OrderbookSelectSymbols = new ObservableCollection<string>();

            btnOrderbookRefresh.Click += btnOrderbookRefresh_Click;
            btnAllCurrentSignalsRefresh.Click += btnAllCurrentSignalsRefresh_Click;

            slTf1min.ValueChanged += SlTf1min_ValueChanged;
            slTf3min.ValueChanged += SlTf3min_ValueChanged1;
            slTf5min.ValueChanged += SlTf5min_ValueChanged;
            slTf15min.ValueChanged += SlTf15min_ValueChanged;
            slTf30min.ValueChanged += SlTf30min_ValueChanged;
            slTf1hr.ValueChanged += SlTf1hr_ValueChanged;
            slTf4hr.ValueChanged += SlTf4hr_ValueChanged;
            slTf1day.ValueChanged += SlTf1day_ValueChanged;

            for (int i = 0; i < BAROMETERDEPTH; i++)
            {
                _BarometerData1Hr[i] = new BarometerCMD();
                _BarometerData4Hr[i] = new BarometerCMD();
                _BarometerData1D[i] = new BarometerCMD();
            }

            System.Timers.Timer clock = new System.Timers.Timer();
            clock.Interval = 1000;
            clock.Elapsed += Clock_Elapsed;
            clock.Start();

            _scanner = new Scanner(httpclient);
            BinanceApi = new BinanceNetApiClient();

            Settings Settings = SettingsStore.Load();
            LastExchangeId = Settings.ExchangeId;
            LastScanAllTimeframesEachMinute = Settings.ScanAllTimeframesEachMinute;

            if (Settings.APIGetNrOfCandles < 10)
            {
                Settings.APIGetNrOfCandles = 36; // Minimal for indicators
                SettingsStore.Save(Settings);
            }
            AddLogMessage($"Load: LastExchangeId={LastExchangeId} APIGetNrOfCandles={Settings.APIGetNrOfCandles} per coinpair");

            McCheckActive = Settings.McCheckActive;
            cbMcCheckActive.Click += cbMcCheckActive_Click;
            AddLogMessage($"Load: McCheckActive is {(McCheckActive ? "Enabled/On" : "Disabled/Off")}");

            AddLogMessage("Load: Start subscriptions");
            string ResultSubscriptions = await _scanner.StartAndCheckSubscriptions();
            AddLogMessage("Load: Subscriptions started. " + ResultSubscriptions);

            SQLiteDbStore = new ScannerSQLiteDbStore(Settings.DirDataMap);
            btnLiteDBRefresh.Click += btnLiteDBRefresh_Click;
            cbLiteDBSignalStates.ItemsSource = Enum.GetValues(typeof(SignalsTBL.SignalStates));
            //VisibleSignalStates = ((SignalsTBL.SignalStates[])Enum.GetValues(typeof(SignalsTBL.SignalStates))).ToList();
            //VisibleSignalStates.Add(SignalsTBL.SignalStates.New);
            VisibleSignalStates.Add(SignalsTBL.SignalStates.SendToExchange);
            VisibleSignalStates.Add(SignalsTBL.SignalStates.InProcessAtExchange);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Buy);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Bought);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Rebuy);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Rebought);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.RebuyStoploss);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.ReboughtStoplossed);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Stoploss);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Sell);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.SellAutoclose);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Panicsell);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Update);
            VisibleSignalActions.Add(SignalsTBL.SignalActions.Updated);
            LiteDBSignalStatesVisibles = string.Join(", ", VisibleSignalStates.Select(s => s.ToString()).ToArray());
            LiteDBSignalActionsVisibles = string.Join(", ", VisibleSignalActions.Select(s => s.ToString()).ToArray());
            btnLiteDBUpdateSignalState.Click += btnLiteDBUpdateSignalState_Click;
            btnLiteDBSSToggleStatesVisibility.Click += btnLiteDBSSToggleStatesVisibility_Click;
            btnLiteDBSSToggleActionsVisibility.Click += btnLiteDBSSToggleActionsVisibility_Click;
            btnSendToTradingView.Click += btnSendToTradingView_Click;
            cbLiteDBSignalActions.ItemsSource = Enum.GetValues(typeof(SignalsTBL.SignalActions));
            btnLiteDBSignalAction.Click += btnLiteDBSignalAction_Click;
            DataGridLiteDBSignals.SelectionChanged += DataGridLiteDBSignals_SelectionChanged;
            btnLiteDBRefresh_Click(null, null); // initial refresh seems to bo necessary

            /*ZignalyTelegramApi = new TelegramClientAPI();
            string Message = await ZignalyTelegramApi.InitZignalyTelegramClient();
            if (!string.IsNullOrEmpty(Message))
            {
                AddLogMessage(Message);
            }*/
            //ConnectZignalyTelegram();
        }

        private async void Clock_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                DateTime Now = DateTime.Now;
                Settings Settings = null;

                if (Now.Second == 0)
                {
                    Settings = SettingsStore.Load();
                    //minBm2Autotrade = Settings.MinMC4Signal;
                    AddLogMessage($"Clock_Elapsed --> 0 sec: start of new cycle --[{AppTitle}]---[Exp:{Settings.OperationEndDate.ToString("ddMMyy")}]------{Settings.MaxRebuysBU}-----------MC1hr={(Settings.McCheckActive ? "Enabled/On (Min={minBm2AuotTrade})" : "Disabled/Off")}---");
                }

                //CanStartATimeFrame1stBuy = (BarometerRefreshed_1Hr && BarometerRefreshed_4Hr && BarometerRefreshed_1D); // valid 1 second after 3th Baromater becomes valid for this minute
                //CanStartATimeFrameNextActions = (BarometerRefreshed_1Hr && BarometerRefreshed_4Hr && BarometerRefreshed_1D); // valid 1 second after 3th Baromater becomes valid for this minute

                if (_scanner != null)
                {
                    if (!BarometerRefreshed_1Hr && _scanner.IsBaroMeterCurrent(60))
                    {
                        if (Settings == null) Settings = SettingsStore.Load();

                        // When in 1st cycles after boot OR only when BmPercentage compared to previous one < Settings max % then add current Barometer to its list
                        BarometerCMD NewBarometer = _scanner.GetBarometerData(60, Settings.DataViaOwnAPI, Settings.Exchange);
                        Decimal PrevDiff = Decimal.Zero;
                        if (_BarometerData1Hr[1].PeriodeMinutes != 0)
                        {
                            PrevDiff = Math.Abs((_BarometerData1Hr[1].BmPercentage != 0) ? (NewBarometer.BmPercentage - _BarometerData1Hr[1].BmPercentage) / _BarometerData1Hr[1].BmPercentage : NewBarometer.BmPercentage);
                            //AddLogMessage($"Clock_Elapsed CM60: prevDiff={PrevDiff}");
                        }
                        if (PrevDiff == Decimal.Zero || (PrevDiff != Decimal.Zero && PrevDiff < Settings.MaxCMDifferencePercentage))
                        {
                            AddLogMessage($"Clock_Elapsed CM60: maxDiff={Settings.MaxCMDifferencePercentage} prev1Hr={_BarometerData1Hr[1].BmPercentage}, new={NewBarometer.BmPercentage} => diff=" + ((PrevDiff != Decimal.Zero) ? PrevDiff.ToString("0.000") : "n.a."));
                            for (int i = (BAROMETERDEPTH - 1); i > 0; i--)
                            {
                                //AddLogMessage($"Clock_Elapsed CM60: Pre shift: i={i} {_BarometerData1Hr[i - 1].BmPercentage} -> {_BarometerData1Hr[i].BmPercentage}");
                                _BarometerData1Hr[i] = _BarometerData1Hr[i - 1];
                                //AddLogMessage($"Clock_Elapsed CM60: Aft shift: i={i} {_BarometerData1Hr[i].BmPercentage} =  {_BarometerData1Hr[i - 1].BmPercentage}");
                            }
                            _BarometerData1Hr[0] = NewBarometer;
                            //AddLogMessage($"Clock_Elapsed CM60: 0 => {_BarometerData1Hr[0].BmPercentage}");
                        }

                        BarometerRefreshed_1Hr = true;
                        AddLogMessage($"Clock_Elapsed: Barometer 1Hr refreshed: Present={_BarometerData1Hr[0].BmPercentage} Prev1={_BarometerData1Hr[1].BmPercentage} prev2={_BarometerData1Hr[2].BmPercentage}, Settings={Settings.MinMC4Signal} => BM {(_BarometerData1Hr[0].BmPercentage < Settings.MinMC4Signal ? "<" : ">=")} Settings; CheckActive={(Settings.McCheckActive ? "Yes" : "No")}");
                        if (BarometerRefreshed_1Hr && BarometerRefreshed_4Hr && BarometerRefreshed_1D) WriteBmToCsvFile(); // only once per minute, but needs in every check-block last one triggers..
                    }
                    if (!BarometerRefreshed_4Hr && _scanner.IsBaroMeterCurrent(240))
                    {
                        if (Settings == null) Settings = SettingsStore.Load();

                        // When in 1st cycles after boot OR only when BmPercentage compared to previous one < Settings max % then add current Barometer to its list
                        BarometerCMD NewBarometer = _scanner.GetBarometerData(240, Settings.DataViaOwnAPI, Settings.Exchange);
                        Decimal PrevDiff = Decimal.Zero;
                        if (_BarometerData4Hr[1].PeriodeMinutes != 0)
                        {
                            PrevDiff = Math.Abs((_BarometerData4Hr[1].BmPercentage != 0) ? (NewBarometer.BmPercentage - _BarometerData4Hr[1].BmPercentage) / _BarometerData4Hr[1].BmPercentage : NewBarometer.BmPercentage);
                            //AddLogMessage($"Clock_Elapsed CM240: prevDiff={PrevDiff}");
                        }
                        if (PrevDiff == Decimal.Zero || (PrevDiff != Decimal.Zero && PrevDiff < Settings.MaxCMDifferencePercentage))
                        {
                            AddLogMessage($"Clock_Elapsed CM4Hr: maxDiff={Settings.MaxCMDifferencePercentage} prev4Hr={_BarometerData4Hr[1].BmPercentage}, new={NewBarometer.BmPercentage} => diff="+ ((PrevDiff != Decimal.Zero) ? PrevDiff.ToString("0.000") : "n.a."));
                            for (int i = (BAROMETERDEPTH - 1); i > 0; i--)
                            {
                                //AddLogMessage($"Clock_Elapsed CM240: Pre shift: i={i} {_BarometerData4Hr[i - 1].BmPercentage} -> {_BarometerData4Hr[i].BmPercentage}");
                                _BarometerData4Hr[i] = _BarometerData4Hr[i - 1];
                                //AddLogMessage($"Clock_Elapsed CM240: Aft shift: i={i} {_BarometerData4Hr[i].BmPercentage} =  {_BarometerData4Hr[i - 1].BmPercentage}");
                            }
                            _BarometerData4Hr[0] = NewBarometer;
                            //AddLogMessage($"Clock_Elapsed CM240: 0 => {_BarometerData4Hr[0].BmPercentage}");
                        }
                        BarometerRefreshed_4Hr = true;
                        AddLogMessage($"Clock_Elapsed: Barometer 4Hr refreshed: Present={_BarometerData4Hr[0].BmPercentage} Prev1={_BarometerData4Hr[1].BmPercentage} Prev2={_BarometerData4Hr[2].BmPercentage}, Settings={Settings.MinMC4Hr4Signal} => BM {(_BarometerData4Hr[0].BmPercentage < Settings.MinMC4Hr4Signal ? "<" : ">=")} Settings");
                        if (BarometerRefreshed_1Hr && BarometerRefreshed_4Hr && BarometerRefreshed_1D) WriteBmToCsvFile();
                    }
                    if (!BarometerRefreshed_1D && _scanner.IsBaroMeterCurrent(1440))
                    {
                        if (Settings == null) Settings = SettingsStore.Load();

                        // When in 1st cycles after boot OR only when BmPercentage compared to previous one < Settings max % then add current Barometer to its list
                        BarometerCMD NewBarometer = _scanner.GetBarometerData(1440, Settings.DataViaOwnAPI, Settings.Exchange);
                        Decimal PrevDiff = Decimal.Zero;
                        if (_BarometerData1D[1].PeriodeMinutes != 0)
                        {
                            PrevDiff = Math.Abs((_BarometerData1D[1].BmPercentage != 0) ? (NewBarometer.BmPercentage - _BarometerData1D[1].BmPercentage) / _BarometerData1D[1].BmPercentage : NewBarometer.BmPercentage);
                            //AddLogMessage($"Clock_Elapsed CM1440: prevDiff={PrevDiff}");
                        }
                        if (PrevDiff == Decimal.Zero || (PrevDiff != Decimal.Zero && PrevDiff < Settings.MaxCMDifferencePercentage))
                        {
                            AddLogMessage($"Clock_Elapsed CM1D: maxDiff={Settings.MaxCMDifferencePercentage} prev1D={_BarometerData1D[1].BmPercentage}, new={NewBarometer.BmPercentage} => diff=" + ((PrevDiff != Decimal.Zero) ? PrevDiff.ToString("0.000") : "n.a."));
                            for (int i = (BAROMETERDEPTH - 1); i > 0; i--)
                            {
                                //AddLogMessage($"Clock_Elapsed CM1440: Pre shift: i={i} {_BarometerData1D[i - 1].BmPercentage} -> {_BarometerData1D[i].BmPercentage}");
                                _BarometerData1D[i] = _BarometerData1D[i - 1];
                                //AddLogMessage($"Clock_Elapsed CM1440: Aft shift: i={i} {_BarometerData1D[i].BmPercentage} =  {_BarometerData1D[i - 1].BmPercentage}");
                            }
                            _BarometerData1D[0] = NewBarometer;
                            //AddLogMessage($"Clock_Elapsed CM1440: 0 => {_BarometerData1D[0].BmPercentage}");
                        }
                        BarometerRefreshed_1D = true;
                        AddLogMessage($"Clock_Elapsed: Barometer 1D refreshed: Present={_BarometerData1D[0].BmPercentage} Prev1={_BarometerData1D[1].BmPercentage} Prev2={_BarometerData1D[2].BmPercentage}");
                        if (BarometerRefreshed_1Hr && BarometerRefreshed_4Hr && BarometerRefreshed_1D) WriteBmToCsvFile();
                    }

                    // After receiving at least 1 BM..
                    int CountBMsRefreshed = 0;
                    if (BarometerRefreshed_1Hr) CountBMsRefreshed++;
                    if (BarometerRefreshed_4Hr) CountBMsRefreshed++;
                    if (BarometerRefreshed_1D) CountBMsRefreshed++;
                    if (CountBMsRefreshed > 0 && !UsedMarkedpairsRefreshed)
                    {
                        UsedMarkedpairsRefreshed = true;
                        string Message = "";
                        UsedMarketPairs = _scanner.FindCoinsWithEnoughVolume(1, out Message);
                        if (String.IsNullOrEmpty(Message))
                        {
                            AddLogMessage($"Clock_Elapsed FindCoinsWithEnoughVolume => {UsedMarketPairs.Count} symbols updated{(UsedMarketPairs.Count > 0 ? ", 1st symbol start24Hr=" + UsedMarketPairs.First().StartDateTime24Hr.ToString("ddMMyy.HHmmss") + " end24Hr=" + UsedMarketPairs.First().EndDateTime24Hr.ToString("ddMMyy.HHmmss") : "")}");
                            /*foreach (MarketPairsCMD ump in UsedMarketPairs)
                            {
                                // For develop only: show all Marketpairs w some fields
                                Console.WriteLine($"UsedMP: {ump.Pairname} now={ump.NowDateTime.ToString("ddMMyy.HHmmss")} start24Hr={ump.StartDateTime24Hr.ToString("ddMMyy.HHmmss")} end24Hr={ump.EndDateTime24Hr.ToString("ddMMyy.HHmmss")} BaseVol24Hr={ump.BaseCurrencyVolume24Hr}");
                            }*/
                        }
                        else
                        {
                            AddLogMessage($"Clock_Elapsed ERROR: {Message}");
                        }
                    }
                    if ((CountBMsRefreshed > 2 && BarometerPoints < 0) || String.IsNullOrEmpty(BarometerText))
                    {
                        // Punten:          0    1   2   3   4   5   6   7
                        // Nieuwe trades:   Dog Dog Dog Dog Rd  Ye  Gn  Gn
                        // Bijkopen?        Rd  Rd  Rd  Rd  Ye  Ye  Gn  Gn
                        // Dog=Wait, walk with dog; Rd=Wait; Ye=Warning: 5..10%; Gn=Buy
                        BarometerPoints = 0;
                        if (!String.IsNullOrEmpty(BarometerText))
                        {
                            if (_BarometerData1D[0].BmPercentage > 0) BarometerPoints += 1;
                            if (_BarometerData4Hr[0].BmPercentage > 0) BarometerPoints += 2;
                            if (_BarometerData1Hr[0].BmPercentage > 0) BarometerPoints += 4;
                            if (_BarometerData1Hr[0].BmPercentage == 0) BarometerPoints += 2;
                        }
                        BarometerText = ((_BarometerData1D[0] != null && _BarometerData4Hr[0] != null && _BarometerData1Hr[0] != null)
                            ? "MC " + BarometerPoints.ToString()  //((_BarometerData1Hr[0].BmPercentage > minBm2Autotrade) ? "**" : "..")
                                + " (1d,4h,1h): " + _BarometerData1D[0].BmPercentage.ToString("f2") + "%"
                                + ", " + _BarometerData4Hr[0].BmPercentage.ToString("f2") + "%"
                                + ", " + _BarometerData1Hr[0].BmPercentage.ToString("f2") + "%"
                            : "");

                        CanStartATimeFrame1stBuy = (BarometerRefreshed_1Hr && BarometerRefreshed_4Hr && BarometerRefreshed_1D); // valid 1 second after 3th Baromater becomes valid for this minute
                        CanStartATimeFrameNextActions = (BarometerRefreshed_1Hr && BarometerRefreshed_4Hr && BarometerRefreshed_1D); // valid 1 second after 3th Baromater becomes valid for this minute
                        
                        AddLogMessage("Clock_Elapsed BM text set, allowed to start 1stbuy / next scans..");
                    }
                }

                switch (Now.Second)
                {
                    /*case 1:
                        if (Settings == null) Settings = SettingsStore.Load();
                        if (Settings.DeleteNotusefullStatesDBRecords || Settings.DeleteNotusefullActionsDBRecords)
                        {
                            CleanSignalTbl(1, Settings.DeleteNotusefullStatesDBRecords, Settings.DeleteNotusefullActionsDBRecords);
                            AddLogMessage($"Clock_Elapsed LiteSignalDB: 1min deleted {StatesCount1} states, {ActionsCount1} actions records");
                        }
                        else
                        {
                            AddLogMessage($"Clock_Elapsed LiteSignalDB: not deleting records");
                        }
                        break;*/
                    case 3:
                        //ConnectZignalyTelegram();
                        break;
                    case 10:
                        string Message10 = "";
                        Message10 = await _scanner.StartAndCheckSubscriptions();
                        if (!String.IsNullOrEmpty(Message10))
                        {
                            AddLogMessage("Clock_Elapsed " + Message10);
                        }
                        break;
                    case 45:
                        if (Settings == null) Settings = SettingsStore.Load();

                        if (!String.IsNullOrEmpty(Settings.ApikeyBinance) && !String.IsNullOrEmpty(Settings.ApiSecretBinance))
                        {
                            if ((Settings.checkBNBMinutes > 0) && !String.IsNullOrEmpty(Settings.AddBNBQuote))
                            {
                                if ((((Now.Hour * 60) + Now.Minute) % Settings.checkBNBMinutes) == 0)
                                {
                                    AddLogMessage("Clock_Elapsed 45s: Check amount BNB to optionally Add funds..");
                                    string Message = "";
                                    Decimal BNBAmount = BinanceApi.GetCoinAmount("BNB", out Message);
                                    if (String.IsNullOrEmpty(Message))
                                    {
                                        AddLogMessage($"Clock_Elapsed 45s: Checked BNB amount={BNBAmount}, minimal={Settings.MinBNBAmount} => " + ((BNBAmount <= Settings.MinBNBAmount) ? "ADD FUNDS" : "STILL ENOUGH"));
                                        if (BNBAmount <= Settings.MinBNBAmount)
                                        {
                                            String OrderResult = BinanceApi.BuyMarketOrder($"BNB{Settings.AddBNBQuote}", Settings.AddBNBAmount, out Message);
                                            AddLogMessage($"Clock_Elapsed 45s: Add BNB funds " + (String.IsNullOrEmpty(Message) ? OrderResult : Message));
                                        }
                                    }
                                    else
                                    {
                                        AddLogMessage($"Clock_Elapsed 45s: Error getting BNB amount: {Message}");
                                    }
                                }
                                else
                                {
                                    AddLogMessage($"Clock_Elapsed 45s: Geen BNB check moment: {(Now.Hour * 60) + Now.Minute} % {Settings.checkBNBMinutes} = {((Now.Hour * 60) + Now.Minute) % Settings.checkBNBMinutes} remainder (must be {Settings.checkBNBMinutes})");
                                }
                            }
                            else
                            {
                                AddLogMessage($"Clock_Elapsed 45s: BNB Check funds disabled or Quote not given");
                            }

                            if (!String.IsNullOrEmpty(Settings.Time2ConvertDust2BNB) && (Now.ToString("HH:mm") == Settings.Time2ConvertDust2BNB))
                            {
                                string Message = "";
                                string Dust2BNBConvertSuccess = BinanceApi.ConvertDust2BNB(out Message);
                                AddLogMessage($"Clock_Elapsed 50s: Convert dust->BNB: " + (String.IsNullOrEmpty(Message) ? Dust2BNBConvertSuccess : Message));
                            }
                            else
                            {
                                AddLogMessage($"Clock_Elapsed 50s: Convert dust->BNB not enabled (no time given or {Settings.Time2ConvertDust2BNB} not reached)");
                            }
                        }
                        break;
                    case 59:
                        AlreadyRunNextScanForAllTimeframes = false;
                        UsedMarkedpairsRefreshed = BarometerRefreshed_1Hr = BarometerRefreshed_4Hr = BarometerRefreshed_1D = false; // reset for next minute
                        CanStartATimeFrame1stBuy = CanStartATimeFrameNextActions = false;
                        BarometerPoints = -1; // invalid
                        AddLogMessage("Clock_Elapsed 59s: reset flags for BM, NextScan and BMPoints");
                        try
                        {
                            Settings SettingsLE = SettingsStore.Load();
                            if (SettingsLE.ExchangeId != LastExchangeId)
                            {
                                SettingsLE.ExchangeId = LastExchangeId;
                                SettingsStore.Save(SettingsLE);
                                AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell Save LastExchangeId: {LastExchangeId}");
                            }
                        }
                        catch (Exception ESS)
                        {
                            AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell Save LastExchangeId WARNING: {ESS.Message}");
                        }

                        break;
                }

                SetClockText(Now.ToString("dd-MMM-yy HH:mm:ss "), BarometerText);
            }
            catch (Exception E)
            {
                AddLogMessage($"Clock_Elapsed ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        /*private async void ConnectZignalyTelegram()
        {
            string Message = "";
            if (!ZignalyTelegramApi.TelegramIsConnected())
            {
                Message = await ZignalyTelegramApi.InitZignalyTelegramClient();
            }
            if (String.IsNullOrEmpty(Message))
            {
                if (!ZignalyTelegramApi.IsUserAuthenticated())
                {
                    Settings Settings5 = SettingsStore.Load();
                    if (!String.IsNullOrEmpty(Settings5.TelegramUserNumber) && !String.IsNullOrEmpty(Settings5.TelegramCode))
                    {
                        Message = await ZignalyTelegramApi.TelegramConnectUser();
                        if (!String.IsNullOrEmpty(Message))
                        {
                            AddLogMessage("TelegramConnectUser: " + Message);
                        }
                    }
                    else
                    {
                        AddLogMessage($"Telegram: Missing Telegram UserNumber and/or Telegram Code in Settings!");
                    }
                }
                else
                {
                    AddLogMessage($"Telegram: user is authenticated.");
                }
            }
            else
            {
                AddLogMessage($"WARNING: Telegram NOT Connected: {Message}");
            }
        }*/

        private void cbMcCheckActive_Click(object sender, RoutedEventArgs e)
        {
            Settings Settings = SettingsStore.Load();
            Settings.McCheckActive = (bool)cbMcCheckActive.IsChecked;
            SettingsStore.Save(Settings);
            AddLogMessage($"cbMcCheckActive_Click: cbMcCheckActive is set to {(Settings.McCheckActive ? "Enabled/ON" : "Disabled/Off")}");
        }

        private void DataGridLiteDBSignals_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridLiteDBSignals.Items.Count > 0)
                {
                    SignalsTBL Signal = (SignalsTBL)DataGridLiteDBSignals.SelectedItem;
                    if (Signal != null)
                    {
                        NumberFormatInfo nfi = new NumberFormatInfo();
                        nfi.NumberDecimalSeparator = ".";
                        LiteDBSignalBuyPrice = $"{Signal.BuyPrice:0.00000000}"; //  Signal.BuyPrice.ToString(nfi);
                    }
                    else
                    {
                        //AddLogMessage("DataGridLiteDBSignals_SelectionChanged: No row selected!");
                    }
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR DataGridLiteDBSignals_SelectionChanged: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private void btnAllCurrentSignalsRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddLogMessage("Refresh All current Signals grid");
                //dgAllCurrentSignals.ItemsSource = null; // forcing update
                //dgAllCurrentSignals.ItemsSource = AllCurrentSignals;
                //if (AllCurrentSignals.Count > 0)
                //{
                //    dgAllCurrentSignals.Columns[2].Visibility = Visibility.Hidden; // TimeframeMinutes
                //}
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR btnAllCurrentSignalsRefresh_Click: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private /*async*/ void btnSendToTradingView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridLiteDBSignals.Items.Count > 0)
                {
                    Settings Settings = SettingsStore.Load();
                    foreach (SignalsTBL Signal in DataGridLiteDBSignals.SelectedItems)
                    {
                        string TVurlQuery = $"symbol={Signal.Exchange}:{Signal.Symbol}";
                        string urlTV = $"https://www.tradingview.com/chart/?{TVurlQuery}";

                        /*if (Settings.AutoTrade)
                        {
                            string responseBody = await httpclient.GetStringAsync(urlTV);
                            AddLogMessage($"btnSendToTradingView_Click {Signal.TimeframeName} {Signal.Symbol} Autotrade=On: send {TVurlQuery} => {responseBody.Length} bytes.");
                        }
                        else
                        {*/
                        Process.Start(Settings.BrowserLocation, urlTV);
                        AddLogMessage($"btnSendToTradingView_Click {Signal.TimeframeName} {Signal.Symbol} Autotrade=Off: open browser with {TVurlQuery}");
                        //}
                    }
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR btnSendToTradingView_Click: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private async void btnLiteDBSignalAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SignalsTBL.SignalActions SignalAction;
                Enum.TryParse<SignalsTBL.SignalActions>(cbLiteDBSignalActions.SelectedValue.ToString(), out SignalAction);
                decimal BuyPrice;
                if ((DataGridLiteDBSignals.Items.Count > 0) && decimal.TryParse(LiteDBSignalBuyPrice, out BuyPrice))
                {
                    SignalsTBL Signal = (SignalsTBL)DataGridLiteDBSignals.SelectedItem;
                    if (Signal != null)
                    {
                        Signal.BuyPrice = BuyPrice;
                        await SendActionToZignaly(Signal, false);
                    }
                    else
                    {
                        AddLogMessage("btnLiteDBSignalAction_Click: No row selected!");
                    }
                }
                else
                {
                    AddLogMessage("btnLiteDBSignalAction_Click: Invalid Price!");
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR btnLiteDBSignalAction_Click: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private async Task<bool> SendActionToZignaly(SignalsTBL Signal, bool AutoAction)
        {
            string ResultMessage = ""; // as alweays: empty is ok
            try
            {
                Settings Settings = SettingsStore.Load();
                string PostData = "";
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                string ZignalyId = $"{Signal.ExchangeId}{(int)Signal.Strategy}A";
                string PreData = $"exchange={Signal.Exchange.ToLower()}&pair={Signal.Symbol}";
                switch (Signal.SignalAction)
                {
                    case SignalsTBL.SignalActions.Buy:
                        PostData = PreData + $"&type=buy&SignalId={ZignalyId}&orderType=limit&limitPrice={Signal.BuyPrice.ToString(nfi)}"
                            + ((Signal.TimeframeMinutes < 6 && Settings.MaxCandles1stBuy > 0) ? $"&buyTTL={(3 * Signal.TimeframeMinutes * 60) + 1}" : "")
                            + $"&takeProfitPercentage1={Signal.TakeProfit.ToString(nfi)}&takeProfitAmountPercentage1=100"
                            + $"&stopLossPercentage={Signal.StopLoss.ToString(nfi)}&positionSize={Signal.PositionSize.ToString(nfi)}"; //BuyTTL removed
                        break;
                    case SignalsTBL.SignalActions.Sell:
                        PostData = PreData + $"&type=sell&orderType=market&SignalId={ZignalyId}";
                        //&limitprice={Signal.BuyPrice.ToString(nfi)}&price={Signal.BuyPrice.ToString(nfi)}";
                        break;
                    case SignalsTBL.SignalActions.SellAutoclose:
                        PostData = PreData + $"&type=sell&orderType=market&SignalId={ZignalyId}";
                        //&limitprice={Signal.BuyPrice.ToString(nfi)}&price={Signal.BuyPrice.ToString(nfi)}";
                        break;
                    case SignalsTBL.SignalActions.Rebuy:
                        PostData = PreData + $"&type=rebuy&SignalId={ZignalyId}";
                        // &limitprice={Signal.BuyPrice.ToString(nfi)}";
                        break;
                    case SignalsTBL.SignalActions.Stoploss:
                        // you need to send it as a negative value (example: -5), if not, it will place a positive stop loss value, meaning that if the current price is below it, the position will be sold. If you want to place positive value, then no symbol is needed.
                        PostData = PreData + $"&type=update&SignalId={ZignalyId}&stopLossPercentage={(-1 * Signal.StopLoss).ToString(nfi)}";
                        // &limitprice={Signal.BuyPrice.ToString(nfi)}";
                        break;
                    case SignalsTBL.SignalActions.Panicsell:
                        PostData = $"exchange={Signal.Exchange}&type=panicSell";
                        break;
                }

                if (!String.IsNullOrEmpty(PostData))
                {
                    string ZignalyKey = "";
                    switch (Signal.TimeframeMinutes)
                    {
                        case 1: ZignalyKey = Settings.Zignaly1min; break;
                        case 3: ZignalyKey = Settings.Zignaly3min; break;
                        case 5: ZignalyKey = Settings.Zignaly5min; break;
                        case 15: ZignalyKey = Settings.Zignaly15min; break;
                        case 30: ZignalyKey = Settings.Zignaly30min; break;
                        case 60: ZignalyKey = Settings.Zignaly1hr; break;
                        case 240: ZignalyKey = Settings.Zignaly4hr; break;
                        case 3660: ZignalyKey = Settings.Zignaly1day; break;
                    }

                    if (!String.IsNullOrEmpty(ZignalyKey))
                    {
                        string Msg = $"SendActionToZignaly Id={Signal.Id} FBId={Signal.FirstBuyId} Zignalyid={ZignalyId} {Signal.TimeframeName} {Signal.Symbol}: " + (AutoAction ? "Auto" : "Manual") + " ";
                        //var (PostMessage, PostStatus, PostResponse) = await _scanner.PostUriData("Zignaly", "signals", ($"key={threadzignaly}&" + PostData).Replace("&", System.Environment.NewLine));
                        var (PostMessage, PostResponse) = await _scanner.GetUriData("Zignaly", "signals", $"key={ZignalyKey}&" + PostData);
                        System.Net.HttpStatusCode PostStatus = (String.IsNullOrEmpty(PostMessage) ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);
                        bool ResultOk = (String.IsNullOrEmpty(PostMessage) && (PostStatus == System.Net.HttpStatusCode.OK));
                        AddLogMessage(Msg + "SEND => " + (ResultOk
                            ? $"key ={ZignalyKey.Substring(0, 5)}.. data={PostData} => Status OK {PostResponse}"
                            : $"Status={PostStatus.ToString()} " + PostMessage));
                        if (!ResultOk)
                        {
                            ResultMessage = $"Zend2Zignaly ERROR: {PostStatus.ToString()} " + PostMessage;
                        }
                    }
                    else
                    {
                        AddLogMessage($"SendActionToZignaly WARNING to send Signal.Id={Signal.Id}, Zignalyid={ZignalyId}: NO ZignalyKey known for {Signal.TimeframeName}, {Signal.Symbol} ! Not send.");
                        ResultMessage = $"Zend2Zignaly ERROR: No Zignaly key known!";
                    }
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"SendActionToZignaly ERROR Id={Signal.Id} FBId={Signal.FirstBuyId}: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
                ResultMessage = $"Zend2Zignaly ERROR: {E.Message}";
            }

            if (!String.IsNullOrEmpty(ResultMessage))
            {
                try
                {
                    Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();
                    Data.Add("Message", $"'{ResultMessage.Replace("'", "\'")}'");

                    AddLogMessage($"SendActionToZignaly To update Id={Signal.Id} FBId={Signal.FirstBuyId} Signal=[{Signal.ToString()}]");
                    AddLogMessage($"SendActionToZignaly Updating: Id={Signal.Id} FBId={Signal.FirstBuyId} data=[" + String.Join("; ", Data) + "]");
                    string Message = "";
                    SQLiteDbStore.UpdateSignal(Signal.Id, false, Data, out Message);
                    if (!String.IsNullOrEmpty(Message))
                    {
                        AddLogMessage($"SendActionToZignaly Id={Signal.Id} FBId={Signal.FirstBuyId}, {Signal.TimeframeName} {Signal.Symbol} ERROR update Signal with Message: {Message}");
                    }
                }
                catch (Exception E)
                {
                    AddLogMessage($"SendActionToZignaly ERROR Updating Signal Id={Signal.Id} FBId={Signal.FirstBuyId}: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
                }
            }

            return String.IsNullOrEmpty(ResultMessage);
        }

        private async void RefreshGridLiteDBSignals()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    string Message = "";
                    Settings Settings = SettingsStore.Load();
                    int MaxRecords = (Settings.MaxRecordLines > 0 ? Settings.MaxRecordLines : 0);
                    List<SignalsTBL> AllSignals = SQLiteDbStore.GetSignals(0, 0, 0, VisibleSignalStates, VisibleSignalActions /*Enumerable.Empty<SignalsTBL.SignalActions>().ToList()*/, out Message);
                    if (String.IsNullOrEmpty(Message))
                    {
                        GridAllLiteDBSignals.Clear();
                        foreach (SignalsTBL Signal in AllSignals)
                        {
                            if (MaxRecords == 0 || ((MaxRecords > 0) && (GridAllLiteDBSignals.Count < MaxRecords)))
                            {
                                GridAllLiteDBSignals.Add(Signal);
                            }
                        }
                        DataGridLiteDBSignals.ItemsSource = null;
                        DataGridLiteDBSignals.ItemsSource = GridAllLiteDBSignals;
                    }
                    else
                    {
                        AddLogMessage($"RefreshGridLiteDBSignals GetAllSignals ERROR: {Message}");
                    }
                });
            }
            catch (Exception E)
            {
                AddLogMessage($"RefreshGridLiteDBSignals ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private void btnLiteDBRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshGridLiteDBSignals();
        }

        private void btnLiteDBUpdateSignalState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Message = "";
                if (DataGridLiteDBSignals.Items.Count > 0)
                {
                    SignalsTBL.SignalStates SignalState;
                    Enum.TryParse<SignalsTBL.SignalStates>(cbLiteDBSignalStates.SelectedValue.ToString(), out SignalState);
                    Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();
                    foreach (SignalsTBL Signal in DataGridLiteDBSignals.SelectedItems)
                    {
                        Message = "";
                        Data.Clear();
                        Data.Add("SignalState", (int)SignalState);
                        AddLogMessage($"btnLiteDBUpdateSignalState_Click To update Id={Signal.Id} Signal=[{Signal.ToString()}]");
                        AddLogMessage($"btnLiteDBUpdateSignalState_Click Updating: Id={Signal.Id} data=[" + String.Join("; ", Data) + "]");
                        SQLiteDbStore.UpdateSignal(Signal.Id, false, Data, out Message);
                        if (!String.IsNullOrEmpty(Message))
                        {
                            AddLogMessage($"btnLiteDBUpdateSignalState ERROR update Signal in DB to set State={SignalState.ToString()}: {Message}");
                        }
                        else
                        {
                            AddLogMessage($"btnLiteDBUpdateSignalState updated Signal id={Signal.Id} in DB to State={SignalState.ToString()}");
                        }
                    }
                    btnLiteDBRefresh_Click(sender, e);
                }
            }
            catch (Exception Ex)
            {
                AddLogMessage($"ERROR btnLiteDBUpdateSignalState_Click: {Ex.Message} at '{Ex.StackTrace.Substring(Math.Max(0, Ex.StackTrace.Length - 50))}'");
            }
        }

        private void btnLiteDBSSToggleStatesVisibility_Click(object sender, RoutedEventArgs e)
        {
            SignalsTBL.SignalStates SignalState;
            Enum.TryParse<SignalsTBL.SignalStates>(cbLiteDBSignalStates.SelectedValue.ToString(), out SignalState);

            if (VisibleSignalStates.Contains(SignalState))
            {
                VisibleSignalStates.Remove(SignalState);
            }
            else
            {
                VisibleSignalStates.Add(SignalState);
            }
            LiteDBSignalStatesVisibles = string.Join(", ", VisibleSignalStates.Select(s => s.ToString()).ToArray());
            btnLiteDBRefresh_Click(sender, e);
        }

        private void btnLiteDBSSToggleActionsVisibility_Click(object sender, RoutedEventArgs e)
        {
            SignalsTBL.SignalActions SignalAction;
            Enum.TryParse<SignalsTBL.SignalActions>(cbLiteDBSignalActions.SelectedValue.ToString(), out SignalAction);

            if (VisibleSignalActions.Contains(SignalAction))
            {
                VisibleSignalActions.Remove(SignalAction);
            }
            else
            {
                VisibleSignalActions.Add(SignalAction);
            }
            LiteDBSignalActionsVisibles = string.Join(", ", VisibleSignalActions.Select(s => s.ToString()).ToArray());
            btnLiteDBRefresh_Click(sender, e);
        }

        private void SlTf1min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (thread1min.minutes != 1)
            {
                //thread1min = new MainThread();
                //Settings settings = SettingsStore.Load();
                thread1min.name = "1 min";
                thread1min.minutes = 1;
                thread1min.running = false;
                thread1min.pause = false;
                thread1min.startdt = DateTime.MinValue;
                thread1min.nextDt1stBuy = DateTime.MinValue;
                thread1min.timer1stBuy = new System.Timers.Timer();
                thread1min.timer1stBuy.AutoReset = false;
                thread1min.timer1stBuy.Elapsed += (sendert, et) => TimerElapsed_FindNewSignalsToBuySell(sendert, /*et,*/ thread1min.minutes);
                thread1min.nextDtNextAction = DateTime.MinValue;
                thread1min.timerNextAction = new System.Timers.Timer();
                thread1min.timerNextAction.AutoReset = false;
                thread1min.timerNextAction.Elapsed += (sendert, et) => TimerElapsed_CheckSignalsNextActions(sendert, /*et,*/ thread1min.minutes);
                //thread1min.scanner = null;
            }
            DoTimeframeSliderChange(slider.Value, ref thread1min);
        }

        private void SlTf3min_ValueChanged1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (thread3min.minutes != 3)
            {
                //thread3min = new MainThread();
                //Settings settings = SettingsStore.Load();
                thread3min.name = "3 min";
                thread3min.minutes = 3;
                thread3min.running = false;
                thread3min.pause = false;
                thread3min.startdt = DateTime.MinValue;
                thread3min.nextDt1stBuy = DateTime.MinValue;
                thread3min.timer1stBuy = new System.Timers.Timer();
                thread3min.timer1stBuy.AutoReset = false;
                thread3min.timer1stBuy.Elapsed += (sendert, et) => TimerElapsed_FindNewSignalsToBuySell(sendert, /*et,*/ thread3min.minutes);
                thread3min.nextDtNextAction = DateTime.MinValue;
                thread3min.timerNextAction = new System.Timers.Timer();
                thread3min.timerNextAction.AutoReset = false;
                thread3min.timerNextAction.Elapsed += (sendert, et) => TimerElapsed_CheckSignalsNextActions(sendert, /*et,*/ thread3min.minutes);
                //thread3min.scanner = null;
            }
            DoTimeframeSliderChange(slider.Value, ref thread3min);
        }

        private void SlTf5min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (thread5min.minutes != 5)
            {
                //thread5min = new MainThread();
                //Settings settings = SettingsStore.Load();
                thread5min.name = "5 min";
                thread5min.minutes = 5;
                thread5min.running = false;
                thread5min.pause = false;
                thread5min.startdt = DateTime.MinValue;
                thread5min.nextDt1stBuy = DateTime.MinValue;
                thread5min.timer1stBuy = new System.Timers.Timer();
                thread5min.timer1stBuy.AutoReset = false;
                thread5min.timer1stBuy.Elapsed += (sendert, et) => TimerElapsed_FindNewSignalsToBuySell(sendert, /*et,*/ thread5min.minutes);
                thread5min.nextDtNextAction = DateTime.MinValue;
                thread5min.timerNextAction = new System.Timers.Timer();
                thread5min.timerNextAction.AutoReset = false;
                thread5min.timerNextAction.Elapsed += (sendert, et) => TimerElapsed_CheckSignalsNextActions(sendert, /*et,*/ thread5min.minutes);
                //thread5min.scanner = null;
            }
            DoTimeframeSliderChange(slider.Value, ref thread5min);
        }

        private void SlTf15min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (thread15min.minutes != 15)
            {
                //thread15min = new MainThread();
                //Settings settings = SettingsStore.Load();
                thread15min.name = "15 min";
                thread15min.minutes = 15;
                thread15min.running = false;
                thread15min.pause = false;
                thread15min.startdt = DateTime.MinValue;
                thread15min.nextDt1stBuy = DateTime.MinValue;
                thread15min.timer1stBuy = new System.Timers.Timer();
                thread15min.timer1stBuy.AutoReset = false;
                thread15min.timer1stBuy.Elapsed += (sendert, et) => TimerElapsed_FindNewSignalsToBuySell(sendert, /*et,*/ thread15min.minutes);
                thread15min.nextDtNextAction = DateTime.MinValue;
                thread15min.timerNextAction = new System.Timers.Timer();
                thread15min.timerNextAction.AutoReset = false;
                thread15min.timerNextAction.Elapsed += (sendert, et) => TimerElapsed_CheckSignalsNextActions(sendert, /*et,*/ thread15min.minutes);
                //thread15min.scanner = null;
            }
            DoTimeframeSliderChange(slider.Value, ref thread15min);
        }

        private void SlTf30min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (thread30min.minutes != 30)
            {
                //thread30min = new MainThread();
                //Settings settings = SettingsStore.Load();
                thread30min.name = "30 min";
                thread30min.minutes = 30;
                thread30min.running = false;
                thread30min.pause = false;
                thread30min.startdt = DateTime.MinValue;
                thread30min.nextDt1stBuy = DateTime.MinValue;
                thread30min.timer1stBuy = new System.Timers.Timer();
                thread30min.timer1stBuy.AutoReset = false;
                thread30min.timer1stBuy.Elapsed += (sendert, et) => TimerElapsed_FindNewSignalsToBuySell(sendert, /*et,*/ thread30min.minutes);
                thread30min.nextDtNextAction = DateTime.MinValue;
                thread30min.timerNextAction = new System.Timers.Timer();
                thread30min.timerNextAction.AutoReset = false;
                thread30min.timerNextAction.Elapsed += (sendert, et) => TimerElapsed_CheckSignalsNextActions(sendert, /*et,*/ thread30min.minutes);
                //thread30min.scanner = null;
            }
            DoTimeframeSliderChange(slider.Value, ref thread30min);
        }

        private void SlTf1hr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (thread1hr.minutes != 60)
            {
                //thread1hr = new MainThread();
                //Settings settings = SettingsStore.Load();
                thread1hr.name = "1 hr";
                thread1hr.minutes = 60;
                thread1hr.running = false;
                thread1hr.pause = false;
                thread1hr.startdt = DateTime.MinValue;
                thread1hr.nextDt1stBuy = DateTime.MinValue;
                thread1hr.timer1stBuy = new System.Timers.Timer();
                thread1hr.timer1stBuy.AutoReset = false;
                thread1hr.timer1stBuy.Elapsed += (sendert, et) => TimerElapsed_FindNewSignalsToBuySell(sendert, /*et,*/ thread1hr.minutes);
                thread1hr.nextDtNextAction = DateTime.MinValue;
                thread1hr.timerNextAction = new System.Timers.Timer();
                thread1hr.timerNextAction.AutoReset = false;
                thread1hr.timerNextAction.Elapsed += (sendert, et) => TimerElapsed_CheckSignalsNextActions(sendert, /*et,*/ thread1hr.minutes);
                //thread1hr.scanner = null;
            }
            DoTimeframeSliderChange(slider.Value, ref thread1hr);
        }

        private void SlTf4hr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (thread4hr.minutes != 240)
            {
                //thread4hr = new MainThread();
                //Settings settings = SettingsStore.Load();
                thread4hr.name = "4 hr";
                thread4hr.minutes = 240;
                thread4hr.running = false;
                thread4hr.pause = false;
                thread4hr.startdt = DateTime.MinValue;
                thread4hr.nextDt1stBuy = DateTime.MinValue;
                thread4hr.timer1stBuy = new System.Timers.Timer();
                thread4hr.timer1stBuy.AutoReset = false;
                thread4hr.timer1stBuy.Elapsed += (sendert, et) => TimerElapsed_FindNewSignalsToBuySell(sendert, /*et,*/ thread4hr.minutes);
                thread4hr.nextDtNextAction = DateTime.MinValue;
                thread4hr.timerNextAction = new System.Timers.Timer();
                thread4hr.timerNextAction.AutoReset = false;
                thread4hr.timerNextAction.Elapsed += (sendert, et) => TimerElapsed_CheckSignalsNextActions(sendert, /*et,*/ thread4hr.minutes);
                //thread4hr.scanner = null;
            }
            DoTimeframeSliderChange(slider.Value, ref thread4hr);
        }

        private void SlTf1day_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (thread1day.minutes != 1440)
            {
                //thread1day = new MainThread();
                //Settings settings = SettingsStore.Load();
                thread1day.name = "1 d";
                thread1day.minutes = 1440;
                thread1day.running = false;
                thread1day.pause = false;
                thread1day.startdt = DateTime.MinValue;
                thread1day.nextDt1stBuy = DateTime.MinValue;
                thread1day.timer1stBuy = new System.Timers.Timer();
                thread1day.timer1stBuy.AutoReset = false;
                thread1day.timer1stBuy.Elapsed += (sendert, et) => TimerElapsed_FindNewSignalsToBuySell(sendert, /*et,*/ thread1day.minutes);
                thread1day.nextDtNextAction = DateTime.MinValue;
                thread1day.timerNextAction = new System.Timers.Timer();
                thread1day.timerNextAction.AutoReset = false;
                thread1day.timerNextAction.Elapsed += (sendert, et) => TimerElapsed_CheckSignalsNextActions(sendert, /*et,*/ thread1day.minutes);
                //thread1day.scanner = null;
            }
            DoTimeframeSliderChange(slider.Value, ref thread1day);
        }

        private void DoTimeframeSliderChange(double sliderPos, ref MainThread thread)
        {
            int spos = (int)sliderPos;
            switch ((int)spos)
            {
                case 1: // 1 = Start Running
                    StartScanning(ref thread);
                    break;
                case 2: // 2 = Pause
                    PauseScanning(ref thread);
                    break;
                default: // 0 = Stop 
                    StopScanning(ref thread);
                    break;
            }
        }
        private void StartScanning(ref MainThread thread)
        {
            try
            {
                Settings Settings = SettingsStore.Load();

                DateTime Now = System.DateTime.Now;
                if (thread.nextDt1stBuy == DateTime.MinValue || thread.nextDt1stBuy <= Now)
                {
                    int threadminutes = thread.minutes;
                    DateTime nextDateTime = (thread.nextDt1stBuy == DateTime.MinValue) ? Now : (DateTime)thread.nextDt1stBuy;
                    nextDateTime = RoundUp(nextDateTime, TimeSpan.FromMinutes(threadminutes));
                    thread.nextDt1stBuy = nextDateTime.AddSeconds(-nextDateTime.Second); // Set Seconds to 0
                    System.TimeSpan ts1stBuy = nextDateTime - Now;
                    thread.timer1stBuy.Interval = (ts1stBuy.TotalSeconds) * 1000;

                    threadminutes = (Settings.ScanAllTimeframesEachMinute ? 1 : threadminutes);
                    nextDateTime = (thread.nextDtNextAction == DateTime.MinValue) ? Now : (DateTime)thread.nextDtNextAction;
                    nextDateTime = RoundUp(nextDateTime, TimeSpan.FromMinutes(threadminutes));
                    thread.nextDtNextAction = nextDateTime.AddSeconds(-nextDateTime.Second); // Set Seconds to 0
                    System.TimeSpan tsNextAction = nextDateTime - Now;
                    thread.timerNextAction.Interval = (tsNextAction.TotalSeconds) * 1000;

                    if (!thread.running)
                    {
                        thread.running = true;
                        thread.startdt = Now;
                    }
                    thread.pause = false;

                    thread.timer1stBuy.Start();
                    thread.timerNextAction.Start();

                    AddLogMessage($"{thread.name} start={thread.startdt.ToString("ddMMyy.HHmmss")} next 1stBuy scan: wait for {thread.nextDt1stBuy.ToString("ddMMyy.HHmmss")} in {Math.Round((ts1stBuy.TotalSeconds),2)} seconds / NextActions: wait for {thread.nextDtNextAction.ToString("ddMMyy.HHmmss")} in {Math.Round((tsNextAction.TotalSeconds),2)} seconds..");
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR StartScanning {thread.name}: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private void PauseScanning(ref MainThread thread)
        {
            if (thread.running && !thread.pause)
            {
                thread.pause = true;
                thread.nextDt1stBuy = DateTime.MinValue;
                thread.timer1stBuy.Stop();
                thread.nextDtNextAction = DateTime.MinValue;
                thread.timerNextAction.Stop();
                //thread.timer.Dispose();
                //thread.timer = null;
            }
            AddLogMessage($"{thread.name} Paused");
        }

        private void StopScanningAll()
        {
            // slider(s) to Off position (visual,UI)
            slTf1min.Value = 0;
            slTf3min.Value = 0;
            slTf5min.Value = 0;
            slTf15min.Value = 0;
            slTf30min.Value = 0;
            slTf1hr.Value = 0;
            slTf4hr.Value = 0;
            slTf1day.Value = 0;

            if (thread1min.running) StopScanning(ref thread1min);
            if (thread3min.running) StopScanning(ref thread3min);
            if (thread5min.running) StopScanning(ref thread5min);
            if (thread15min.running) StopScanning(ref thread15min);
            if (thread30min.running) StopScanning(ref thread30min);
            if (thread1hr.running) StopScanning(ref thread1hr);
            if (thread4hr.running) StopScanning(ref thread4hr);
            if (thread1day.running) StopScanning(ref thread1day);
        }

        private void StopScanning(ref MainThread thread)
        {
            if (thread.running || thread.nextDt1stBuy != DateTime.MinValue)
            {
                AddLogMessage($"{thread.name} Stopped: started {thread.startdt}; skip next 1stBuy={thread.nextDt1stBuy.ToString("ddMMMyy-HHmmss")}, NextAction={thread.nextDtNextAction.ToString("ddMMMyy-HHmmss")}.");

                thread.running = false;
                thread.pause = false;
                thread.nextDt1stBuy = DateTime.MinValue;
                thread.nextDtNextAction = DateTime.MinValue;
                thread.startdt = DateTime.MinValue;
                thread.timer1stBuy.Stop();
                thread.timerNextAction.Stop();
                //thread.timer.Dispose();
                //thread.timer = null;
                //thread.scanner = null;
            }
        }

        private bool CleanSignalTbl(int TimeframeMinutes, bool DeleteStates, bool DeleteActions)
        {
            bool Cleanresult = false;
            try
            {
                string Message = "";
                List<SignalsTBL.SignalStates> States = new List<SignalsTBL.SignalStates>();
                if (DeleteStates)
                {
                    States.Add(SignalsTBL.SignalStates.Canceled);
                    States.Add(SignalsTBL.SignalStates.Completed);
                    States.Add(SignalsTBL.SignalStates.CompletedAutoClose);
                    States.Add(SignalsTBL.SignalStates.CompletedLoss);
                    States.Add(SignalsTBL.SignalStates.CompletedProfit);
                    States.Add(SignalsTBL.SignalStates.Rejected);
                }

                List<SignalsTBL.SignalActions> Actions = new List<SignalsTBL.SignalActions>();
                if (DeleteActions)
                {
                    //Actions.Add(SignalsTBL.SignalActions.Bought);
                    //Actions.Add(SignalsTBL.SignalActions.ReBought);
                    Actions.Add(SignalsTBL.SignalActions.Sold);
                    Actions.Add(SignalsTBL.SignalActions.SoldAutoclose);
                    Actions.Add(SignalsTBL.SignalActions.Panicsold);
                    //Actions.Add(SignalsTBL.SignalActions.Updated);
                }

                if (States.Count > 0 || Actions.Count > 0)
                {
                    int RowsAffected = SQLiteDbStore.DeleteSignal(0, TimeframeMinutes, States, Actions, out Message);
                    if (String.IsNullOrEmpty(Message))
                    {
                        Cleanresult = true;
                        AddLogMessage($"CleanSignalTbl OK: {(DeleteStates ? "States" : "")} {(DeleteActions ? "Actions" : "")} => {RowsAffected} records");
                    }
                    else
                    {
                        AddLogMessage($"ERROR CleanSignalTbl ERROR: {Message}");
                    }
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR CleanSignalTbl: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }

            return Cleanresult;
        }

        private void FindNewSignalsToBuySell(int TimeframeMinutes, string TimeframeName, int APIGetNrOfCandles, bool Autotrade, bool LogInvalidSignals, bool AllowShort, bool AllowLong, bool PlaySounds, bool TradeExchangeDirect, int MaxCandles1stBuy, bool UpdateClosePrices, int MaxRecordLines)
        {
            // Creating the "Mother"-Signals
            try
            {
                if (UsedMarketPairs.Count > 0) //(String.IsNullOrEmpty(ErrorMessage))
                {
                    AddLogMessage($"FindNewSignalsToBuySell START CYCLE {TimeframeName}: scan for firstbuy's with {UsedMarketPairs.Count} coinpairs...");

                    string Message = _scanner.UpdateCandlesData(UsedMarketPairs, TimeframeMinutes, TimeframeName, APIGetNrOfCandles).GetAwaiter().GetResult(); // wait for result before continueing here..

                    if (String.IsNullOrEmpty(Message))
                    {
                        // Update all ClosePrices in existing signals
                        if (UpdateClosePrices)
                        {
                            List<SignalsTBL.SignalStates> States = new List<SignalsTBL.SignalStates>();
                            States.Add(SignalsTBL.SignalStates.InProcessAtExchange);

                            List<SignalsTBL> AllDBSignals = SQLiteDbStore.GetSignals(0, 0, TimeframeMinutes, States, Enumerable.Empty<SignalsTBL.SignalActions>().ToList(), out Message);
                            if (String.IsNullOrEmpty(Message))
                            {
                                Dictionary<string, dynamic> UpdateSignalData = new Dictionary<string, dynamic>();

                                AddLogMessage($"FindNewSignalsToBuySell Update all {TimeframeName}, {AllDBSignals.Count} LiteDB records sendTo/atExchange with their last Closeprices...");

                                foreach (SignalsTBL Signal in AllDBSignals.Where(s => (s.TimeframeMinutes == TimeframeMinutes && (s.SignalState == SignalsTBL.SignalStates.SendToExchange || s.SignalState == SignalsTBL.SignalStates.InProcessAtExchange))))
                                {
                                    UpdateSignalData.Clear();
                                    Decimal LastClosePrice = _scanner.GetLastClosePrice(Signal.Symbol, Signal.TimeframeMinutes, out Message);
                                    if (String.IsNullOrEmpty(Message))
                                    {
                                        NumberFormatInfo nfi = new NumberFormatInfo();
                                        nfi.NumberDecimalSeparator = ".";
                                        UpdateSignalData.Add("ClosePrice", $"{LastClosePrice.ToString(nfi)}");
                                        AddLogMessage($"FindNewSignalsToBuySell-UpdateCP {TimeframeName} To update Id={Signal.Id} Signal=[{Signal.ToString()}]");
                                        AddLogMessage($"FindNewSignalsToBuySell-UpdateCP {TimeframeName} Updating: Id={Signal.Id} data=[" + String.Join("; ", UpdateSignalData) + "]");
                                        List<SignalsTBL> UpdatedSignals = SQLiteDbStore.UpdateSignal(Signal.Id, false, UpdateSignalData, out Message);
                                        if (!String.IsNullOrEmpty(Message))
                                        {
                                            AddLogMessage($"ERROR FindNewSignalsToBuySell updated {UpdatedSignals.Count} signal: id={Signal.Id} {Signal.TimeframeName} {Signal.Symbol}: ERROR Update signal in DB: {Message}");
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (String.IsNullOrEmpty(Message))
                        {
                            AddLogMessage($"FindNewSignalsToBuySell: Find new signals for 1st buy {TimeframeName} with {UsedMarketPairs.Count} symbols, APIGetNrOfCandles={APIGetNrOfCandles}, BM1Hr={Math.Round((_BarometerData1Hr[0].BmPercentage),2)}, BM4Hr={Math.Round((_BarometerData4Hr[0].BmPercentage),2)}, BM1D={Math.Round((_BarometerData1D[0].BmPercentage),2)} ...");

                            // Check for all Marketpairs the buy strategies (retrieve candles)
                            List<SignalsTBL> AllSignals = _scanner.FindNewSignalsToBuySellAsync(UsedMarketPairs, TimeframeMinutes, TimeframeName, APIGetNrOfCandles, _BarometerData1Hr[0].BmPercentage, _BarometerData4Hr[0].BmPercentage, _BarometerData1D[0].BmPercentage, LogInvalidSignals);

                            AddLogMessage($"FindNewSignalsToBuySell: Find new signals for 1st buy {TimeframeName} found {AllSignals.Count} Signals, with {AllSignals.FindAll(s => (s.TimeframeMinutes == TimeframeMinutes)).Count} this timeframe ===> {AllSignals.FindAll(s => (s.TimeframeMinutes == TimeframeMinutes && s.Valid)).Count} Valid ");

                            if (AllSignals.Count > 0)
                            {
                                foreach (SignalsTBL signal in AllSignals.FindAll(s => (s.TimeframeMinutes == TimeframeMinutes)))
                                {
                                    if (signal.Valid || LogInvalidSignals)
                                    {
                                        AddLogMessage($"FindNewSignalsToBuySell: {TimeframeName}: {signal.Symbol}, {signal.CandleCount} candles => " + (signal.Valid
                                            ? $"Valid: {signal.Strategy.ToString()}, {signal.SignalAction.ToString()}, {signal.TradeType.ToString()}, buy={signal.BuyPrice.ToString("0.00000000")}"
                                            : $"Invalid: {signal.Message}"));
                                    }

                                    if (signal.Valid /*&& !(signal.TradeType == SignalsTBL.TradeTypes.Short && !AllowShort) && !(signal.TradeType == SignalsTBL.TradeTypes.Long && !AllowLong)*/)
                                    {
                                        string DbMessage = "";
                                        signal.Id = 0;
                                        signal.ExchangeId = ++LastExchangeId;
                                        signal.Id = SQLiteDbStore.InsertSignal(signal, out DbMessage);
                                        AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} {signal.Symbol}: inserted into LiteDB: id={signal.Id}, {signal.Symbol}, {signal.SignalState.ToString()}, {signal.SignalAction.ToString()}, {signal.StartDateTime.ToString("ddMMyy.HHmm")}, {signal.BuyPrice}; {DbMessage}");

                                        //ListBoxSignals.Insert(0, signal);

                                        // ValidEntry but ignore signals for shorts when not allowed
                                        // got buy/sell signal.. write to console
                                        //Console.Beep(800,200);
                                        if (PlaySounds && File.Exists(SOUND_TADA_FILE))
                                        {
                                            System.Media.SoundPlayer Player = new System.Media.SoundPlayer(@SOUND_TADA_FILE);
                                            Player.Play();
                                        }
                                        // PlaySound(@SOUND_TADA_FILE);

                                        if (Autotrade)
                                        {
                                            if (TradeExchangeDirect)
                                            {
                                                //BinanceApi.PlaceOrder();
                                                AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} {signal.Symbol}: Direct Order To Binance: NOT yet Implemented!!");
                                            }
                                            else
                                            {
                                                // 1st buy to Exchange
                                                if (SendActionToZignaly(signal, true).GetAwaiter().GetResult())
                                                {
                                                    // Update SignalState New -> SendToExchange
                                                    Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();
                                                    Data.Add("SignalState", (int)SignalsTBL.SignalStates.SendToExchange);
                                                    if (signal.SignalAction == SignalsTBL.SignalActions.Buy) Data.Add("BuyCount", (++signal.BuyCount));
                                                    if (signal.SignalAction == SignalsTBL.SignalActions.Sell) Data.Add("BuyCount", (--signal.BuyCount));

                                                    Message = "";
                                                    AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} To update Id={signal.Id} Signal=[{signal.ToString()}]");
                                                    AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} Updating: Id={signal.Id} data=[" + String.Join("; ", Data) + "]");
                                                    SQLiteDbStore.UpdateSignal(signal.Id, false, Data, out Message);
                                                    if (!String.IsNullOrEmpty(Message))
                                                    {
                                                        AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} {signal.Symbol} ERROR update Signal id={signal.Id} in DB to set Action=SendToExchange, BuyCount={signal.BuyCount}: {Message}");
                                                    }
                                                    else
                                                    {
                                                        AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} {signal.Symbol} OK update Signal id={signal.Id} in DB to Action=SendToExchange, BuyCount={signal.BuyCount}");
                                                        btnLiteDBRefresh_Click(null, null);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} {signal.Symbol} autotrade=Off: buyprice={signal.BuyPrice} takeprofit={signal.TakeProfit} SL={signal.StopLoss} Msg={signal.Message}");
                                        }

                                        WriteToCsvFile(TimeframeName, signal.GetCsvDataRow());
                                    }
                                    else
                                    {
                                        if (false && LogInvalidSignals)
                                        {
                                            AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} invalid signal for {signal.Symbol}: " + signal.TradeType.ToString() + " " + signal.Message
                                                + ((signal.TradeType == SignalsTBL.TradeTypes.Short && !AllowShort) ? "Short niet toegestaan" : "")
                                                + ((signal.TradeType == SignalsTBL.TradeTypes.Long && !AllowLong) ? "Long niet toegestaan" : ""));
                                        }
                                    }
                                }
                                AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} {UsedMarketPairs.Count} symbols: processed all {AllSignals.Count} signals");

                                _ = Dispatcher.InvokeAsync(() =>
                                {
                                    foreach (SignalsTBL signal in AllSignals.FindAll(s => (s.TimeframeMinutes == TimeframeMinutes)))
                                    {
                                        if (signal.Valid && !(signal.TradeType == SignalsTBL.TradeTypes.Short && !AllowShort) && !(signal.TradeType == SignalsTBL.TradeTypes.Long && !AllowLong))
                                        {
                                            if (MaxRecordLines > 0 && ListBoxSignals.Count > MaxRecordLines)
                                            {
                                                ListBoxSignals.RemoveAt(MaxRecordLines - 1);
                                            }
                                            ListBoxSignals.Insert(0, signal);
                                        }

                                        int IndexOfExistingSignal = -1;
                                        for (int i = 0; i < AllCurrentSignals.Count; i++)
                                        {
                                            if (AllCurrentSignals[i].TimeframeMinutes == TimeframeMinutes && AllCurrentSignals[i].Symbol == signal.Symbol)
                                            {
                                                IndexOfExistingSignal = i;
                                                break;
                                            }
                                        }
                                        if (IndexOfExistingSignal != -1)
                                        {
                                            AllCurrentSignals[IndexOfExistingSignal] = signal;
                                        }
                                        else
                                        {
                                            AllCurrentSignals.Add(signal);
                                        }

                                        if (dgAllCurrentSignalsOnceVisible) //|| dgAllCurrentSignals.IsVisible)
                                        {
                                            dgAllCurrentSignalsOnceVisible = true;

                                            // niet meer automatisch: btnAllCurrentSignalsRefresh_Click(null, null);
                                            //dgAllCurrentSignals.ItemsSource = null; // forcing update
                                            //dgAllCurrentSignals.ItemsSource = AllCurrentSignals;
                                            //dgAllCurrentSignals.Columns[2].Visibility = Visibility.Hidden; // TimeframeMinutes
                                        }
                                    }
                                });
                            }
                            else
                            {
                                AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} {UsedMarketPairs.Count} symbols: No signals received");
                            }
                        }
                        else
                        {
                            AddLogMessage($"FindNewSignalsToBuySell {TimeframeName} {UsedMarketPairs.Count} symbols: {Message}");
                        }
                    }
                }
                else
                {
                    AddLogMessage($"FindNewSignalsToBuySell WARNING: NO UsedMarketPairs received from API => NO scan to find new signals!");
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR FindNewSignalsToBuySell: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private async void CheckSignalsNextActions(int threadminutes)
        {
            try
            {
                AddLogMessage($"CheckSignalsNextActions START CYCLE for {threadminutes} minutes: actions after 1st buy...");

                Settings Settings = SettingsStore.Load();

                System.Windows.Forms.DialogResult DialogResult; // http://www.dotnetperls.com/messagebox-show

                Dictionary<string, dynamic> UpdateSignalData = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> UpdateMotherSignalData = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> UpdateChildSignalData = new Dictionary<string, dynamic>();

                bool UpdateSignal = false;
                bool AnyUpdated = false;

                // Scan these states..
                List<SignalsTBL.SignalStates> States = new List<SignalsTBL.SignalStates>();
                States.Add(SignalsTBL.SignalStates.SendToExchange);
                States.Add(SignalsTBL.SignalStates.InProcessAtExchange);
                // Scan these Actions to change
                List<SignalsTBL.SignalActions> Actions = new List<SignalsTBL.SignalActions>();
                Actions.Add(SignalsTBL.SignalActions.Buy);
                Actions.Add(SignalsTBL.SignalActions.Bought);
                Actions.Add(SignalsTBL.SignalActions.Rebuy);
                Actions.Add(SignalsTBL.SignalActions.RebuyStoploss);
                Actions.Add(SignalsTBL.SignalActions.Sell);
                Actions.Add(SignalsTBL.SignalActions.SellAutoclose);
                Actions.Add(SignalsTBL.SignalActions.Panicsell);
                //Actions.Add(SignalsTBL.SignalActions.Update);

                string Message = "";
                int getminutes = (Settings.ScanAllTimeframesEachMinute ? 0 : threadminutes);
                List<SignalsTBL> AllSignals = SQLiteDbStore.GetSignals(0, 0, getminutes, States, Actions /*Enumerable.Empty<SignalsTBL.SignalActions>().ToList()*/, out Message);
                if (String.IsNullOrEmpty(Message))
                {
                    AddLogMessage($"CheckSignalsNextActions {AllSignals.Count} AllSignals got from DB for {(Settings.ScanAllTimeframesEachMinute ? "All available timeframes" : threadminutes + "min timeframe")}...");
                    // Update Orderbook with Symbols in Signals
                    List<string> AllSymbols = AllSignals.Select(s => s.Symbol).Distinct().OrderBy(id => id).ToList();
                    List<OrderBIN> Orders = RefreshOrderBook(AllSymbols);
                    AddLogMessage($"CheckSignalsNextActions {AllSymbols.Count} symbols from AllSignals selected and {Orders.Count} Orders from RefreshOrderBook() for the symbols; START Signal loop for {threadminutes} and SendToExchange or InProcessAtExchange...");

                    // All Signals send to / at exchange for current timeframe(!)
                    foreach (SignalsTBL Signal in AllSignals.Where(s => (s.Valid == true)))
                    {
                        AddLogMessage($"CheckSignalsNextActions {Signal.Symbol} in {Signal.TimeframeName} {(Signal.Valid ? "" : "In")}Valid State={Signal.SignalState.ToString()} Action={Signal.SignalAction} checking...");

                        UpdateSignalData.Clear();
                        UpdateMotherSignalData.Clear();
                        UpdateChildSignalData.Clear();
                        UpdateSignal = false;

                        if (Signal.SignalState == SignalsTBL.SignalStates.SendToExchange)
                        {
                            // Check within 1st x candles period
                            int AcceptMinutes = (Settings.MaxCandles1stBuy * Signal.TimeframeMinutes) + 1; // + 1 since candle datetime is 1 minute earlier
                            if (AcceptMinutes > 0)
                            {
                                bool InAcceptPeriod = (DateTime.Compare(Signal.StartDateTime.AddMinutes(AcceptMinutes).AddSeconds(-Signal.StartDateTime.Second), DateTime.Now.AddSeconds(-DateTime.Now.Second)) == 1);

                                AddLogMessage($"CheckSignalsNextActions SendToExchange id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.SignalAction.ToString()} {Signal.TimeframeName} {Signal.Symbol}: StartTime={Signal.StartDateTime.ToString()} is " + (InAcceptPeriod ? "shorter then" : "after")
                                    + $" {AcceptMinutes} minutes ago" + (InAcceptPeriod ? "" : " => SET MANUALY SignalState with Dialog " + (Settings.EnableConfirmDialogs ? "" : "In") + "visible"));

                                if (!InAcceptPeriod)
                                {
                                    if (Settings.EnableConfirmDialogs)
                                    {
                                        DialogResult = System.Windows.Forms.MessageBox.Show(
                                            $"{Signal.TimeframeName}: Is {Signal.Symbol} id={Signal.FirstBuyId}, ExchangeId={Signal.ExchangeId} NOT (partially)filled, so Cancel signal?",
                                            "Manual set State to Cancel", MessageBoxButtons.YesNoCancel);

                                        if (DialogResult == DialogResult.Yes)
                                        {
                                            UpdateSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.Canceled);
                                            UpdateSignal = true;
                                        }
                                        else if (DialogResult == DialogResult.No)
                                        {
                                            UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.Bought);
                                            UpdateSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.InProcessAtExchange);
                                            UpdateSignal = true;
                                        }
                                    }
                                    else
                                    {
                                        // Auto update to InProcessAtExchange
                                        UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.Bought);
                                        UpdateSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.InProcessAtExchange);
                                        UpdateSignal = true;
                                    }
                                }
                            }
                            AddLogMessage($"CheckSignalsNextActions SendToExchange id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.SignalAction.ToString()} {Signal.TimeframeName} {Signal.Symbol}: update to: {String.Join("; ", UpdateSignalData.ToArray())}");
                        }
                        else if (Signal.SignalState == SignalsTBL.SignalStates.InProcessAtExchange) // At Exchange and (partially)filled...
                        {
                            AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.SignalAction.ToString()} {Signal.TimeframeName} {Signal.Symbol}: NOT autoclose. ");
                            switch (Signal.SignalAction)
                            {
                                // 1st 2: As long as no reply from Exchange (Zignaly), asumed at next candle: set to action done so and wait next candle for scanning..
                                case SignalsTBL.SignalActions.Buy:
                                    UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.Bought);
                                    UpdateSignal = true;
                                    AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalAction.ToString()}=>Bought");
                                    break;
                                case SignalsTBL.SignalActions.Rebuy:
                                    //UpdateSignalData.Add("BuyCount", ++Signal.BuyCount);
                                    UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.Rebought);
                                    UpdateSignal = true;
                                    AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalAction.ToString()}=>ReBought");
                                    break;
                                case SignalsTBL.SignalActions.RebuyStoploss:
                                    //UpdateSignalData.Add("BuyCount", ++Signal.BuyCount);
                                    UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.ReboughtStoplossed);
                                    UpdateSignal = true;
                                    AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalAction.ToString()}=>ReBoughtStoplossed");
                                    break;
                                case SignalsTBL.SignalActions.Sell:
                                    //UpdateSignalData.Add("BuyCount", --Signal.BuyCount);
                                    UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.Sold);
                                    UpdateSignalData.Add("SignalState", (Signal.Strategy == SignalsTBL.Strategies.BottomUp ? (int)SignalsTBL.SignalStates.CompletedLoss : (int)SignalsTBL.SignalStates.CompletedProfit));
                                    UpdateMotherSignalData.Add("SignalState", (Signal.Strategy == SignalsTBL.Strategies.BottomUp ? (int)SignalsTBL.SignalStates.CompletedLoss : (int)SignalsTBL.SignalStates.CompletedProfit));
                                    UpdateChildSignalData.Add("SignalState", (Signal.Strategy == SignalsTBL.Strategies.BottomUp ? (int)SignalsTBL.SignalStates.CompletedLoss : (int)SignalsTBL.SignalStates.CompletedProfit));
                                    UpdateSignal = true;
                                    AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalState.ToString()}=>Completed*, {Signal.SignalAction.ToString()}=>Sold");
                                    break;
                                case SignalsTBL.SignalActions.SellAutoclose:
                                    UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.SoldAutoclose);
                                    UpdateSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.CompletedLoss);
                                    UpdateMotherSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.CompletedLoss);
                                    UpdateChildSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.CompletedLoss);
                                    UpdateSignal = true;
                                    AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalState.ToString()}=>CompletedLoss, {Signal.SignalAction.ToString()}=>SoldAutoClose");
                                    break;
                                case SignalsTBL.SignalActions.Panicsell:
                                    // ToDo all signals to sold..
                                    UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.Panicsold);
                                    UpdateSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.CompletedLoss);
                                    UpdateMotherSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.CompletedLoss);
                                    UpdateChildSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.CompletedLoss);
                                    UpdateSignal = true;
                                    AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalState.ToString()}=>CompletedLoss {Signal.SignalAction.ToString()}=>PanicSold");
                                    break;
                                case SignalsTBL.SignalActions.Update: // Not as Signal inserted, so not applicable, but to be complete..
                                    UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.Updated);
                                    UpdateSignal = true;
                                    AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalAction.ToString()}=>Updated");
                                    break;
                                case SignalsTBL.SignalActions.Stoploss: // Not as Signal inserted, so not applicable, but to be complete..
                                    UpdateSignalData.Add("SignalAction", (int)SignalsTBL.SignalActions.Stoplossed);
                                    UpdateSignal = true;
                                    AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalAction.ToString()}=>Stoplossed");
                                    break;
                                default: // Bought
                                    // Only "Mother"-Signals, excluding the Rebuy/Sell ones created in the "CheckSignalsNextActions" stage
                                    if (Signal.Id == Signal.FirstBuyId)
                                    {
                                        AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalAction.ToString()} go scanning...");
                                        if (!UpdateSignal)
                                        {
                                            SignalsTBL NewSignal = _scanner.ProcessSignalAfterFilled(Signal, Settings.AutocloseAfterCandles, Settings.AutocloseAfterHr, _BarometerData1Hr[0].BmPercentage, _BarometerData4Hr[0].BmPercentage, _BarometerData1D[0].BmPercentage);
                                            if (NewSignal.Valid)
                                            {
                                                AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={NewSignal.Id} {NewSignal.SignalState.ToString()} {NewSignal.TimeframeName} {NewSignal.Symbol}: {NewSignal.SignalAction.ToString()} Message={NewSignal.Message}");
                                                Message = "";
                                                if (NewSignal.SignalAction != SignalsTBL.SignalActions.Update || NewSignal.SignalAction != SignalsTBL.SignalActions.Stoploss)
                                                {
                                                    AddLogMessage($"CheckSignalsNextActions Inserting NewSignal=[" + String.Join("; ", NewSignal) + "]");
                                                    int NewId = SQLiteDbStore.InsertSignal(NewSignal, out Message);
                                                    if (String.IsNullOrEmpty(Message))
                                                    {
                                                        // Update "Mother" signal causing this Rebuy or Sell
                                                        switch (NewSignal.SignalAction)
                                                        {
                                                            case SignalsTBL.SignalActions.Rebuy:
                                                            case SignalsTBL.SignalActions.RebuyStoploss:
                                                                UpdateMotherSignalData.Add("BuyCount", (++Signal.BuyCount));
                                                                //case SignalsTBL.SignalActions.Sell: UpdateMotherSignalData.Add("BuyCount", (--Signal.BuyCount)); break; -- Family gives - value...
                                                                break;
                                                            case SignalsTBL.SignalActions.Sold:
                                                                if (NewSignal.BuyCount == 1)
                                                                {
                                                                    UpdateMotherSignalData.Add("SignalAction", (int)NewSignal.SignalAction);
                                                                    UpdateMotherSignalData.Add("SignalState", (int)NewSignal.SignalState);
                                                                }
                                                                break;
                                                        }

                                                        AddLogMessage($"CheckSignalsNextActions Inserted NewSignal OK, Id={NewId} Send to Exchange/Zignaly..");
                                                    }
                                                    else
                                                    {
                                                        AddLogMessage($"CheckSignalsNextActions ERROR inserting new signal: {Message}");
                                                    }
                                                }
                                                else
                                                {
                                                    // Update values in orig signal where NewSignal is a copy of -- ToDo
                                                    //UpdateMotherSignalData.
                                                }

                                                if (String.IsNullOrEmpty(Message) && (NewSignal.SignalState == SignalsTBL.SignalStates.SendToExchange || NewSignal.SignalState == SignalsTBL.SignalStates.InProcessAtExchange))
                                                {
                                                    AnyUpdated = true;
                                                    if (NewSignal.SignalAction == SignalsTBL.SignalActions.RebuyStoploss)
                                                    {
                                                        NewSignal.SignalAction = SignalsTBL.SignalActions.Stoploss;
                                                        await SendActionToZignaly(NewSignal, true);
                                                        NewSignal.SignalAction = SignalsTBL.SignalActions.Rebuy;
                                                    }
                                                    await SendActionToZignaly(NewSignal, true);
                                                }
                                            }
                                            else
                                            {
                                                AddLogMessage($"CheckSignalsNextActions No New insert Id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.Symbol} {Signal.TimeframeName} {Signal.Strategy.ToString()} {Signal.SignalAction.ToString()}");
                                            }
                                        }
                                        else
                                        {
                                            AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalAction.ToString()} No scan due Update=True");
                                        }
                                    }
                                    else
                                    {
                                        AddLogMessage($"CheckSignalsNextActions InProcessAtExch id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: {Signal.SignalAction.ToString()} No scan due id != FirstBuyId ({Signal.Id}!={Signal.FirstBuyId})");
                                    }
                                    break;
                            }

                            if (UpdateMotherSignalData.Count > 0)
                            {
                                AddLogMessage($"CheckSignalsNextActions To update Id={Signal.FirstBuyId} Signal=[{Signal.ToString()}]");
                                AddLogMessage($"CheckSignalsNextActions Updating: Id={Signal.FirstBuyId} UpdateMotherSignalData=[" + String.Join("; ", UpdateMotherSignalData) + "]");
                                string MotherMessage = "";
                                List<SignalsTBL> UpdatedMotherSignals = SQLiteDbStore.UpdateSignal(Signal.FirstBuyId, false, UpdateMotherSignalData, out MotherMessage);
                                if (String.IsNullOrEmpty(MotherMessage))
                                {
                                    AnyUpdated = true;
                                    SignalsTBL UpdatedMotherSignal = UpdatedMotherSignals.First();
                                    AddLogMessage($"CheckSignalsNextActions updated Mother id={UpdatedMotherSignal.Id} FBId={UpdatedMotherSignal.FirstBuyId} ExchangeId={UpdatedMotherSignal.ExchangeId} {UpdatedMotherSignal.SignalState.ToString()}=>{UpdatedMotherSignal.SignalState.ToString()} {UpdatedMotherSignal.SignalAction.ToString()} {UpdatedMotherSignal.TimeframeName} {UpdatedMotherSignal.Symbol}: OK, "
                                        + $"BuyPrice={UpdatedMotherSignal.BuyPrice} ");
                                }
                                else
                                {
                                    AddLogMessage($"ERROR CheckSignalsNextActions updated Mother id={Signal.FirstBuyId} ERROR: {MotherMessage}");
                                }
                            }

                            if (UpdateChildSignalData.Count > 0) // all records with FirstBuyId = Signal.FirstBuyId and Id!=FirstBuyId
                            {
                                AddLogMessage($"CheckSignalsNextActions To update Id={Signal.FirstBuyId} Signal=[{Signal.ToString()}]");
                                AddLogMessage($"CheckSignalsNextActions Updating: Id={Signal.FirstBuyId} UpdateChildSignalData=[" + String.Join("; ", UpdateMotherSignalData) + "]");
                                string ChildMessage = "";
                                List<SignalsTBL> UpdatedChildSignals = SQLiteDbStore.UpdateSignal(Signal.FirstBuyId, true, UpdateChildSignalData, out ChildMessage);
                                if (String.IsNullOrEmpty(ChildMessage))
                                {
                                    AnyUpdated = true;
                                    AddLogMessage($"CheckSignalsNextActions updated {UpdatedChildSignals.Count} Child(s) FBid={Signal.FirstBuyId}: OK");
                                }
                                else
                                {
                                    AddLogMessage($"ERROR CheckSignalsNextActions updated Child(s) FBId={Signal.FirstBuyId} ERROR: {ChildMessage}");
                                }
                            }
                        }

                        // Update Signal
                        if (UpdateSignal)
                        {
                            AddLogMessage($"CheckSignalsNextActions To update Id={Signal.Id} Signal=[{Signal.ToString()}]");
                            AddLogMessage($"CheckSignalsNextActions Updating: Id={Signal.Id} UpdateSignalData=[" + String.Join("; ", UpdateSignalData) + "]");
                            Message = "";
                            List<SignalsTBL> UpdatedSignals = SQLiteDbStore.UpdateSignal(Signal.Id, false, UpdateSignalData, out Message);
                            if (String.IsNullOrEmpty(Message))
                            {
                                AnyUpdated = true;
                                SignalsTBL UpdatedSignal = UpdatedSignals.First();
                                AddLogMessage($"CheckSignalsNextActions updated id={Signal.Id}=>{UpdatedSignal.Id} FBId={Signal.FirstBuyId}=>{UpdatedSignal.FirstBuyId}  ExchangeId={Signal.ExchangeId}=>{UpdatedSignal.ExchangeId} {Signal.SignalState.ToString()}=>{UpdatedSignal.SignalState.ToString()} {Signal.SignalAction.ToString()}=>{UpdatedSignal.SignalAction.ToString()} {UpdatedSignal.TimeframeName} {UpdatedSignal.Symbol}: OK, "
                                    + $"BuyPrice={UpdatedSignal.BuyPrice} ");

                                /* Via ChilUpdate solved..
                                if (Signal.SignalAction == SignalsTBL.SignalActions.Sold)
                                {
                                    // Set others(!) Signals with same FirstBuyId to Completed..
                                    foreach (SignalsTBL Signal2 in AllSignals.Where(s => s.FirstBuyId == Signal.FirstBuyId && s.Id != Signal.Id))
                                    {
                                        UpdateSignalData.Clear();
                                        UpdateSignalData.Add("SignalState", (int)SignalsTBL.SignalStates.Completed);
                                        AddLogMessage($"CheckSignalsNextActions To update Id={Signal2.Id} Signal=[{Signal.ToString()}]");
                                        AddLogMessage($"CheckSignalsNextActions Updating: Id={Signal2.Id} set to Sold => also set Id={Signal2.Id} with FirstBuyId={Signal.FirstBuyId}, ExchangeId={Signal.ExchangeId} set to state is Completed");
                                        Message = "";
                                        List<SignalsTBL> UpdatedSignal2s = SQLiteDbStore.UpdateSignal(Signal2.Id, false, UpdateSignalData, out Message);
                                        if (!String.IsNullOrEmpty(Message))
                                        {
                                            AddLogMessage($"CheckSignalsNextActions Updating: set to Sold => also set Id={Signal2.Id} with FirstBuyId={Signal.FirstBuyId}, ExchangeId={Signal.ExchangeId} set to state is Completed; ERROR: {Message}");
                                        }
                                    }
                                }*/
                            }
                            else
                            {
                                AddLogMessage($"ERROR CheckSignalsNextActions updated id={Signal.Id} FBId={Signal.FirstBuyId} ExchangeId={Signal.ExchangeId} {Signal.SignalState.ToString()} {Signal.TimeframeName} {Signal.Symbol}: ERROR Update signal in DB: {Message}");
                            }
                        }
                    }

                    if (AnyUpdated)
                    {
                        AddLogMessage($"CheckSignalsNextActions: go RefreshGridLiteDBSignals..");
                        RefreshGridLiteDBSignals();
                    }
                    else
                    {
                        AddLogMessage($"CheckSignalsNextActions: none marked for update ({Message})");
                    }
                }
                else
                {
                    AddLogMessage($"CheckSignalsNextActions: No Signals from DB. {Message}");
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR CheckSignalsNextActions: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private void TimerElapsed_FindNewSignalsToBuySell(object sender, /*ElapsedEventArgs e,*/ int threadminutes)
        {
            try
            {
                bool validthread = true;
                MainThread thread;
                switch (threadminutes)
                {
                    default: validthread = false; thread = thread1min; /* dummy assign.. */ break;
                    case 1: thread = thread1min; break;
                    case 3: thread = thread3min; break;
                    case 5: thread = thread5min; break;
                    case 15: thread = thread15min; break;
                    case 30: thread = thread30min; break;
                    case 60: thread = thread1hr; break;
                    case (4 * 60): thread = thread4hr; break;
                    case (24 * 60): thread = thread1day; break;
                }

                if (validthread)
                {
                    AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell Timeframe {threadminutes}={thread.name} Next={thread.nextDt1stBuy.ToString("ddMMyy.HHmm")} CanStartATimeFrame1stBuy={(CanStartATimeFrame1stBuy ? "T" : "F")} => Entered, but wait for BM's received..!");
                    bool StillCurrentMinute = true;
                    string NowStr = "";
                    do
                    {
                        NowStr = System.DateTime.Now.ToString("ddMMyyHHmm");
                        StillCurrentMinute = (thread.nextDt1stBuy.ToString("ddMMyyHHmm") == NowStr);
                        Thread.Sleep(1000);
                        // Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {thread.name}: Sleep 1sec Now={NowStr} startdt={thread.nextdt.ToString("ddMMyyHHmm")} StillCurrentMinute={(StillCurrentMinute ? "T" : "F")} CanStartATimeFrame1stBuy={(CanStartATimeFrame1stBuy ? "T" : "F")}");
                    } while (!CanStartATimeFrame1stBuy && StillCurrentMinute);

                    Settings settings = SettingsStore.Load();

                    if (thread.running && !thread.pause)
                    {
                        if (!StillCurrentMinute)
                        {
                            AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell Timeframe {threadminutes}={thread.name} => TIMEOUT to start this cycle (BM's not received in time)!");
                        }
                        else if (DateTime.Now > settings.OperationEndDate)
                        {
                            AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell Oops, DOA: is it realy after {settings.OperationEndDate.ToString()}, opertaion period ??! Then ask developers friendly what to do next... ;)");
                        }
                        else
                        {
                            AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell timeframe {thread.name} is {(thread.running ? "" : "not ")}Running, is {(thread.pause ? "" : "not ")}Paused; Let's START 1stBuy CYCLE !");

                            SetStatusText($"Initializing {settings.Exchange} with 24hr volume of {settings.Min24HrVolume} for {thread.name} timeframe...");

                            CleanSignalTbl(thread.minutes, settings.DeleteNotusefullStatesDBRecords, settings.DeleteNotusefullActionsDBRecords);

                            // Find new Signals to buy
                            FindNewSignalsToBuySell(thread.minutes, thread.name, settings.APIGetNrOfCandles, settings.AutoTrade, settings.LogInvalidSignals, settings.AllowShort, settings.AllowLong, settings.PlaySounds, settings.TradeExchangeDirect, settings.MaxCandles1stBuy, settings.UpdateClosePrices, settings.MaxRecordLines);

                            SetStatusText($"{thread.name} {settings.Exchange} scanning {UsedMarketPairs.Count} symbols ready");

                            AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell {thread.name} end scan: {UsedMarketPairs.Count} symbols..");
                        }

                        // repeat this action next candle period
                        DateTime Now = System.DateTime.Now;
                        DateTime NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(threadminutes));
                        System.TimeSpan ts = NextDatetime - Now;

                        switch (threadminutes)
                        {
                            case 1:
                                thread1min.nextDt1stBuy = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread1min.timer1stBuy.Interval = (ts.TotalSeconds) * 1000;
                                thread1min.timer1stBuy.AutoReset = false;
                                thread1min.timer1stBuy.Start();
                                break;
                            case 3:
                                thread3min.nextDt1stBuy = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread3min.timer1stBuy.Interval = (ts.TotalSeconds) * 1000;
                                thread3min.timer1stBuy.AutoReset = false;
                                thread3min.timer1stBuy.Start();
                                break;
                            case 5:
                                thread5min.nextDt1stBuy = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread5min.timer1stBuy.Interval = (ts.TotalSeconds) * 1000;
                                thread5min.timer1stBuy.AutoReset = false;
                                thread5min.timer1stBuy.Start();
                                break;
                            case 15:
                                thread15min.nextDt1stBuy = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread15min.timer1stBuy.Interval = (ts.TotalSeconds) * 1000;
                                thread15min.timer1stBuy.AutoReset = false;
                                thread15min.timer1stBuy.Start();
                                break;
                            case 30:
                                thread30min.nextDt1stBuy = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread30min.timer1stBuy.Interval = (ts.TotalSeconds) * 1000;
                                thread30min.timer1stBuy.AutoReset = false;
                                thread30min.timer1stBuy.Start();
                                break;
                            case 60:
                                thread1hr.nextDt1stBuy = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread1hr.timer1stBuy.Interval = (ts.TotalSeconds) * 1000;
                                thread1hr.timer1stBuy.AutoReset = false;
                                thread1hr.timer1stBuy.Start();
                                break;
                            case (4 * 60):
                                thread4hr.nextDt1stBuy = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread4hr.timer1stBuy.Interval = (ts.TotalSeconds) * 1000;
                                thread4hr.timer1stBuy.AutoReset = false;
                                thread4hr.timer1stBuy.Start();
                                break;
                            case (24 * 60):
                                thread1day.nextDt1stBuy = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread1day.timer1stBuy.Interval = (ts.TotalSeconds) * 1000;
                                thread1day.timer1stBuy.AutoReset = false;
                                thread1day.timer1stBuy.Start();
                                break;
                        }
                        AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell {thread.name} start={thread.startdt.ToString("ddMMyy.HHmmss")} next scan: now={Now.ToString("ddMMyy.HHmmss")} wait for Next={NextDatetime.AddSeconds(-NextDatetime.Second).ToString("ddMMyy.HHmmss")} in {Math.Round((ts.TotalSeconds),2)} seconds..");
                    }
                    else
                    {
                        AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell {thread.name}: not running!");
                    }

                }
                else
                {
                    AddLogMessage($"TimerElapsed_FindNewSignalsToBuySell ERROR: for {threadminutes}minutes NO Thread!");
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR TimerElapsed_FindNewSignalsToBuySell: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private void TimerElapsed_CheckSignalsNextActions(object sender, /*ElapsedEventArgs e,*/ int threadminutes)
        {
            try
            {
                bool validthread = true;
                MainThread thread;
                switch (threadminutes)
                {
                    default: validthread = false; thread = thread1min; /* dummy assign.. */ break;
                    case 1: thread = thread1min; break;
                    case 3: thread = thread3min; break;
                    case 5: thread = thread5min; break;
                    case 15: thread = thread15min; break;
                    case 30: thread = thread30min; break;
                    case 60: thread = thread1hr; break;
                    case (4 * 60): thread = thread4hr; break;
                    case (24 * 60): thread = thread1day; break;
                }

                if (validthread)
                {
                    AddLogMessage($"TimerElapsed_CheckSignalsNextActions Timeframe {threadminutes}={thread.name} Next={thread.nextDtNextAction.ToString("ddMMyy.HHmm")} => Entered, but wait for BM's received..!");
                    bool StillCurrentMinute = true;
                    string NowStr = "";
                    do
                    {
                        NowStr = System.DateTime.Now.ToString("ddMMyyHHmm");
                        StillCurrentMinute = (thread.nextDtNextAction.ToString("ddMMyyHHmm") == NowStr);
                        Thread.Sleep(1000);
                        // Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} {thread.name}: Sleep 1sec Now={NowStr} startdt={thread.nextdt.ToString("ddMMyyHHmm")} StillCurrentMinute={(StillCurrentMinute ? "T" : "F")} CanStartATimeFrameNextActions={(CanStartATimeFrameNextActions ? "T" : "F")}");
                    } while (!CanStartATimeFrameNextActions && StillCurrentMinute);

                    Settings Settings = SettingsStore.Load();

                    if (thread.running && !thread.pause)
                    {
                        if (!StillCurrentMinute)
                        {
                            AddLogMessage($"TimerElapsed_CheckSignalsNextActions Timeframe {threadminutes}={thread.name} => TIMEOUT to start this cycle (BM's not received in time)!");
                        }
                        else if (DateTime.Now > Settings.OperationEndDate)
                        {
                            AddLogMessage($"TimerElapsed_CheckSignalsNextActions Oops, DOA: is it realy after {Settings.OperationEndDate.ToString()}, opertaion period ??! Then ask developers friendly what to do next... ;)");
                        }
                        else
                        {
                            if (!Settings.Only1stBuy)
                            {
                                if (!Settings.ScanAllTimeframesEachMinute || (Settings.ScanAllTimeframesEachMinute && !AlreadyRunNextScanForAllTimeframes))
                                {
                                    AlreadyRunNextScanForAllTimeframes = true;
                                    // Check buy period limit else cancel buyorder
                                    AddLogMessage($"TimerElapsed_CheckSignalsNextActions timeframe {thread.name} is {(thread.running ? "" : "not ")}Running, is {(thread.pause ? "" : "not ")}Paused: Let's START NextActions CYCLE..!");
                                    CheckSignalsNextActions(threadminutes);
                                }
                                else
                                {
                                    AddLogMessage($"TimerElapsed_CheckSignalsNextActions: ==WARNING== {thread.name} NextActions already started for All frames!");
                                }
                            }
                            else
                            {
                                AddLogMessage($"TimerElapsed_CheckSignalsNextActions: ==WARNING== {thread.name} NextActions is disabled by setting 'Only 1st buy' !");
                            }
                        }

                        // repeat this action next candle period
                        DateTime Now = System.DateTime.Now;
                        DateTime NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(Settings.ScanAllTimeframesEachMinute ? 1 : threadminutes));
                        System.TimeSpan ts = NextDatetime - Now;

                        switch (threadminutes)
                        {
                            case 1:
                                thread1min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread1min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                                thread1min.timerNextAction.AutoReset = false;
                                thread1min.timerNextAction.Start();
                                break;
                            case 3:
                                thread3min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread3min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                                thread3min.timerNextAction.AutoReset = false;
                                thread3min.timerNextAction.Start();
                                break;
                            case 5:
                                thread5min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread5min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                                thread5min.timerNextAction.AutoReset = false;
                                thread5min.timerNextAction.Start();
                                break;
                            case 15:
                                thread15min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread15min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                                thread15min.timerNextAction.AutoReset = false;
                                thread15min.timerNextAction.Start();
                                break;
                            case 30:
                                thread30min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread30min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                                thread30min.timerNextAction.AutoReset = false;
                                thread30min.timerNextAction.Start();
                                break;
                            case 60:
                                thread1hr.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread1hr.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                                thread1hr.timerNextAction.AutoReset = false;
                                thread1hr.timerNextAction.Start();
                                break;
                            case (4 * 60):
                                thread4hr.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread4hr.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                                thread4hr.timerNextAction.AutoReset = false;
                                thread4hr.timerNextAction.Start();
                                break;
                            case (24 * 60):
                                thread1day.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                                thread1day.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                                thread1day.timerNextAction.AutoReset = false;
                                thread1day.timerNextAction.Start();
                                break;
                        }
                        AddLogMessage($"TimerElapsed_CheckSignalsNextActions {threadminutes}={thread.name} {(Settings.ScanAllTimeframesEachMinute ? "Each Minute" : "Per Timeframe")} start={thread.startdt.ToString("ddMMyy.HHmmss")} next scan: now={Now.ToString("ddMMyy.HHmmss")} wait for Next={NextDatetime.AddSeconds(-NextDatetime.Second).ToString("ddMMyy.HHmmss")} in {Math.Round((ts.TotalSeconds),2)} seconds..");
                    }
                    else
                    {
                        AddLogMessage($"TimerElapsed_CheckSignalsNextActions {thread.name}: not running!");
                    }
                }
                else
                {
                    AddLogMessage($"TimerElapsed_CheckSignalsNextActions ERROR: for {threadminutes}minutes NO Thread!");
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR TimerElapsed_CheckSignalsNextActions {threadminutes}minutes: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private void MenuItemQuit_Click(object sender, RoutedEventArgs e)
        {
            AddLogMessage($"Quit: stop all scanning..");
            StopScanningAll();
            this.Close();
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            //AddLogMessage($"Settings: stop all scannings.."); // keep running
            //StopScanningAll();
            SettingsDialog dlg = new SettingsDialog();
            dlg.Closing += MenuItemSettingsDlg_Closing;
            dlg.Show();
        }

        private void MenuItemSettingsDlg_Closing(object sender, CancelEventArgs e)
        {
            Settings Settings = SettingsStore.Load();
            if (Settings.ExchangeId != LastExchangeId)
            {
                LastExchangeId = Settings.ExchangeId;
                AddLogMessage($"MenuItemSettingsDlg_Closing LastExchangeId reset to {LastExchangeId}");
            }

            if (Settings.ScanAllTimeframesEachMinute != LastScanAllTimeframesEachMinute)
            {
                LastScanAllTimeframesEachMinute = Settings.ScanAllTimeframesEachMinute;
                DateTime Now = System.DateTime.Now;
                DateTime NextDatetime;
                int IntervalMinutes;
                System.TimeSpan ts;
                if (thread1min.nextDtNextAction != DateTime.MinValue)
                {
                    IntervalMinutes = (Settings.ScanAllTimeframesEachMinute ? 1 : thread1min.minutes);
                    NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(IntervalMinutes));
                    thread1min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                    ts = NextDatetime - Now;
                    thread1min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                    AddLogMessage($"MenuItemSettingsDlg_Closing Setting ScanAllTimeframesEachMinute changed => {thread1min.name} NextScan start={thread1min.nextDtNextAction.ToString("ddMMMyy-HHmmss")} in {ts.TotalSeconds} seconds...");
                }
                if (thread3min.nextDtNextAction != DateTime.MinValue)
                {
                    IntervalMinutes = (Settings.ScanAllTimeframesEachMinute ? 1 : thread3min.minutes);
                    NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(IntervalMinutes));
                    thread3min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                    ts = NextDatetime - Now;
                    thread3min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                    AddLogMessage($"MenuItemSettingsDlg_Closing Setting ScanAllTimeframesEachMinute changed => {thread3min.name} NextScan start={thread3min.nextDtNextAction.ToString("ddMMMyy-HHmmss")} in {ts.TotalSeconds} seconds...");
                }
                if (thread5min.nextDtNextAction != DateTime.MinValue)
                {
                    IntervalMinutes = (Settings.ScanAllTimeframesEachMinute ? 1 : thread5min.minutes);
                    NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(IntervalMinutes));
                    thread5min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                    ts = NextDatetime - Now;
                    thread5min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                    AddLogMessage($"MenuItemSettingsDlg_Closing Setting ScanAllTimeframesEachMinute changed => {thread5min.name} NextScan start={thread5min.nextDtNextAction.ToString("ddMMMyy-HHmmss")} in {ts.TotalSeconds} seconds...");
                }
                if (thread15min.nextDtNextAction != DateTime.MinValue)
                {
                    IntervalMinutes = (Settings.ScanAllTimeframesEachMinute ? 1 : thread15min.minutes);
                    NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(IntervalMinutes));
                    thread15min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                    ts = NextDatetime - Now;
                    thread15min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                    AddLogMessage($"MenuItemSettingsDlg_Closing Setting ScanAllTimeframesEachMinute changed => {thread15min.name} NextScan start={thread15min.nextDtNextAction.ToString("ddMMMyy-HHmmss")} in {ts.TotalSeconds} seconds...");
                }
                if (thread30min.nextDtNextAction != DateTime.MinValue)
                {
                    IntervalMinutes = (Settings.ScanAllTimeframesEachMinute ? 1 : thread30min.minutes);
                    NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(IntervalMinutes));
                    thread30min.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                    ts = NextDatetime - Now;
                    thread30min.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                    AddLogMessage($"MenuItemSettingsDlg_Closing Setting ScanAllTimeframesEachMinute changed => {thread30min.name} NextScan start={thread30min.nextDtNextAction.ToString("ddMMMyy-HHmmss")} in {ts.TotalSeconds} seconds...");
                }
                if (thread1hr.nextDtNextAction != DateTime.MinValue)
                {
                    IntervalMinutes = (Settings.ScanAllTimeframesEachMinute ? 1 : thread1hr.minutes);
                    NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(IntervalMinutes));
                    thread1hr.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                    ts = NextDatetime - Now;
                    thread1hr.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                    AddLogMessage($"MenuItemSettingsDlg_Closing Setting ScanAllTimeframesEachMinute changed => {thread1hr.name} NextScan start={thread1hr.nextDtNextAction.ToString("ddMMMyy-HHmmss")} in {ts.TotalSeconds} seconds...");
                }
                if (thread4hr.nextDtNextAction != DateTime.MinValue)
                {
                    IntervalMinutes = (Settings.ScanAllTimeframesEachMinute ? 1 : thread4hr.minutes);
                    NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(IntervalMinutes));
                    thread4hr.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                    ts = NextDatetime - Now;
                    thread4hr.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                    AddLogMessage($"MenuItemSettingsDlg_Closing Setting ScanAllTimeframesEachMinute changed => {thread4hr.name} NextScan start={thread4hr.nextDtNextAction.ToString("ddMMMyy-HHmmss")} in {ts.TotalSeconds} seconds...");
                }
                if (thread1day.nextDtNextAction != DateTime.MinValue)
                {
                    IntervalMinutes = (Settings.ScanAllTimeframesEachMinute ? 1 : thread1day.minutes);
                    NextDatetime = RoundUp(Now, TimeSpan.FromMinutes(IntervalMinutes));
                    thread1day.nextDtNextAction = NextDatetime.AddSeconds(-NextDatetime.Second);
                    ts = NextDatetime - Now;
                    thread1day.timerNextAction.Interval = (ts.TotalSeconds) * 1000;
                    AddLogMessage($"MenuItemSettingsDlg_Closing Setting ScanAllTimeframesEachMinute changed => {thread1day.name} NextScan start={thread1day.nextDtNextAction.ToString("ddMMMyy-HHmmss")} in {ts.TotalSeconds} seconds...");
                }
            }
        }

        private async void Lstbx_Signals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SignalsTBL listboxSignalRow = _listBoxGrid.SelectedItem as SignalsTBL;
                Settings settings = SettingsStore.Load();
                if (listboxSignalRow != null && !String.IsNullOrEmpty(listboxSignalRow.Symbol))
                {
                    _ddOrderbookSelectSymbol.SelectedIndex = OrderbookSelectSymbols.IndexOf(listboxSignalRow.Symbol);

                    string TVurlQuery = $"symbol={listboxSignalRow.Exchange}:{listboxSignalRow.Symbol}";
                    string urlTV = $"https://www.tradingview.com/chart/?{TVurlQuery}";

                    if (settings.AutoTrade)
                    {
                        string responseBody = await httpclient.GetStringAsync(urlTV);
                        AddLogMessage($"{listboxSignalRow.TimeframeName} send to chart: {TVurlQuery} => {responseBody.Length} bytes.");
                    }
                    else
                    {
                        Process.Start(settings.BrowserLocation, urlTV);
                        AddLogMessage($"{listboxSignalRow.TimeframeName} open browser for chart: {TVurlQuery}");
                    }
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR selection: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private async void Lstbx_Signals_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                SignalsTBL listboxSignalRow = _listBoxGrid.SelectedItem as SignalsTBL;
                Settings settings = SettingsStore.Load();
                string urlQuery = $"key={settings.Zignaly}&exchange={listboxSignalRow.Exchange}&type=buy&market={listboxSignalRow.Symbol}";
                string url = $"https://zignaly.com/api/signals.php?{urlQuery}";
                if (settings.AutoTrade)
                {
                    string responseBody = await httpclient.GetStringAsync(url);
                    AddLogMessage($"{listboxSignalRow.TimeframeName} send to zignaly: {urlQuery} => {responseBody.Length} bytes.");
                }
                else
                {
                    Process.Start(settings.BrowserLocation, url);
                    AddLogMessage($"{listboxSignalRow.TimeframeName} open browser for zignaly: {urlQuery}");
                }
            }
            catch (Exception E)
            {
                AddLogMessage($"ERROR doubleclick: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*if (TabAllSignals.IsSelected)
            {
                dgAllCurrentSignalsOnceVisible = true;

                if (AllCurrentSignals != null)
                {
                    btnAllCurrentSignalsRefresh_Click(sender, e);
                    //DataGridScannerSignals.ItemsSource = null; // forcing update
                    //DataGridScannerSignals.ItemsSource = AllCurrentSignals;
                    ////DataGridScannerSignals.Columns[2].Visibility = Visibility.Hidden; // TimeframeMinutes
                }
            }*/
        }

        private void btnOrderbookRefresh_Click(object sender, RoutedEventArgs e)
        {
            /* ToDo: store in own orderbook var to not influence Signal processing
            if (OrderbookSelectSymbols.Count >= _ddOrderbookSelectSymbol.SelectedIndex)
            {
                string Symbol = OrderbookSelectSymbols[_ddOrderbookSelectSymbol.SelectedIndex];

                if (!String.IsNullOrEmpty(Symbol))
                {
                    List<string> Symbols = new List<string>();
                    Symbols.Add(Symbol);
                    RefreshOrderBook(Symbols);
                }
                else
                {
                    AddLogMessage($"ERROR RefreshOrderBookClick: No symbol from Scanner list row!");
                }
            }
            else
            {
                AddLogMessage($"WARNING Orderbook: Symbollist is empty.. (will be auto updated)");
            }*/
        }

        private List<OrderBIN> RefreshOrderBook(List<string> Symbols)
        {
            List<OrderBIN> Orders = new List<OrderBIN>();

            try
            {
                Settings Settings = SettingsStore.Load();
                int MaxRecords = (Settings.MaxRecordLines > 0 ? Settings.MaxRecordLines : 0);
                Dispatcher.InvokeAsync(() =>
                {
                    GridOrderbook.Clear();
                    foreach (string Symbol in Symbols)
                    {
                        string Message = "";
                        Orders = BinanceApi.GetOrders(Symbol, out Message);
                        if (String.IsNullOrEmpty(Message))
                        {
                            foreach (OrderBIN Order in Orders)
                            {
                                if ((MaxRecords == 0) || ((MaxRecords > 0) && (GridOrderbook.Count < MaxRecords)))
                                {
                                    GridOrderbook.Add(Order);
                                }
                            }
                            AddLogMessage($"RefreshOrderBook: Got Orderbook for {Symbol}: {Orders.Count} orders");
                        }
                        else
                        {
                            AddLogMessage($"RefreshOrderBook: {Symbol} {Message}");
                            break;
                        }
                    }
                    //DataGridOrderbook.ItemsSource = null; -- seems not necessary!
                    //DataGridOrderbook.ItemsSource = GridOrderbook;
                });
            }
            catch (Exception E)
            {
                AddLogMessage($"RefreshOrderBook ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }

            return Orders;
        }

        private static DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            var modTicks = dt.Ticks % d.Ticks;
            var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        /*private static DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }*/

        /*private static DateTime RoundToNearest(DateTime dt, TimeSpan d)
        {

            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;

            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
        }*/

        private void AddLogMessage(string Msg)
        {
            string NowStr = DateTime.Now.ToString("ddMMMyy.HHmmss");
            Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    Settings Settings = SettingsStore.Load();
                    int MaxLines = (Settings.MaxLogLines > 0 ? Settings.MaxLogLines : 0);
                    int LineCount = Regex.Matches("" + LogMessages, Environment.NewLine).Count - 1;
                    while (MaxLines > 0 && LineCount > MaxLines)
                    {
                        LogMessages = LogMessages.Remove(LogMessages.TrimEnd().LastIndexOf(Environment.NewLine));
                        LineCount = Regex.Matches("" + LogMessages, Environment.NewLine).Count - 1;
                    }
                    if (MaxLines > 0)
                        LogMessages = $"{NowStr}: {Msg}" + Environment.NewLine + LogMessages; // insert on top
                }
                catch (Exception E)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} AddLogMessage ERROR for={Msg}: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
                }
            });
            WriteToLogFile($"Log: {Msg}");

            Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} L: {Msg}");
        }

        private void SetStatusText(string statusTxt)
        {
            Dispatcher.InvokeAsync(() =>
            {
                StatusText = statusTxt;
            });
            WriteToLogFile($"Stat: {statusTxt}");
        }

        private void WriteToLogFile(string Msg)
        {

            Settings Settings = SettingsStore.Load();
            try
            {
                switch (Settings.WriteLogFileType)
                {
                    case "Log4Net":
                        if (Msg.Contains("ERROR"))
                            MainLog4Net.Error(Msg);
                        else
                            MainLog4Net.Info(Msg);
                        break;
                    case "SeriLog":
                        if (Msg.Contains("ERROR"))
                            Log.Error(Msg);
                        else
                            Log.Information(Msg);
                        break;
                    case "StreamWriter":
                        string filename = Settings.DirDataMap + "\\"
                            + System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                            + $"-{DateTime.Now.ToString("yyMMMddHH")}.log";
                        if (swLogFile == null || filename != ((FileStream)(swLogFile.BaseStream)).Name)
                        {
                            //if (swLogFile != null) swLogFile.Dispose();
                            swLogFile = new StreamWriter(filename);
                        }
                        swLogFile.WriteLine(Msg);
                        break;
                }
                //niet meer: File.AppendAllLines(filename, new string[] { Msg });
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} WriteToLogFile ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        private void WriteToCsvFile(string TimeFrameName, string Text)
        {
            Settings Settings = SettingsStore.Load();
            if (Settings.WriteCsvFile)
            {
                try
                {
                    string filename = Settings.DirDataMap + "\\" + "Signals" + $"-{TimeFrameName}-{DateTime.Now.ToString("yyMMMdd")}.csv";
                    if (swCSVFile == null || filename != ((FileStream)(swCSVFile.BaseStream)).Name)
                    {
                        if (swCSVFile != null) swCSVFile.Dispose();
                        swCSVFile = new StreamWriter(filename);
                    }
                    swCSVFile.WriteLine(Text);
                    //niet meer: File.AppendAllLines(filename, new string[] { Text });
                }
                catch (Exception E)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} WriteToCsvFile ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
                }
            }
        }

        private void WriteBmToCsvFile()
        {
            try
            {
                Settings Settings = SettingsStore.Load();
                if (Settings.BmValuesToFile && (_BarometerData1D[0].PeriodeMinutes != 0 || _BarometerData4Hr[0].PeriodeMinutes != 0 || _BarometerData1Hr[0].PeriodeMinutes != 0)
                    && (!Settings.BmValuesToFileWhenTfRun
                      || Settings.BmValuesToFileWhenTfRun && (thread1min.running || thread3min.running || thread5min.running || thread15min.running || thread30min.running || thread1hr.running || thread4hr.running || thread1day.running)))
                {
                    string Filename = Settings.DirDataMap + $"\\BmValues-{DateTime.Now.ToString("yyMMMdd")}.csv";
                    string Headerline = "";
                    if (!File.Exists(Filename))
                    {
                        Headerline = _BarometerData1D[0].ToCsvHeader("1D") + ";" + _BarometerData4Hr[0].ToCsvHeader("4H") + ";" + _BarometerData1Hr[0].ToCsvHeader("1H") + ";\"Timeframes Run\"\r\n";
                    }

                    if (swBMCSVFile == null || Filename != ((FileStream)(swBMCSVFile.BaseStream)).Name)
                    {
                        if (swBMCSVFile != null) swBMCSVFile.Dispose();
                        swBMCSVFile = new StreamWriter(Filename);
                    }

                    //niet meer: File.AppendAllLines(Filename, new string[] { 
                    swBMCSVFile.WriteLine(Headerline + _BarometerData1D[0].ToCsvData() + ";" + _BarometerData4Hr[0].ToCsvData() + ";" + _BarometerData1Hr[0].ToCsvData() + ";\""
                        + (thread1min.running ? "1m " : "")
                        + (thread3min.running ? "3m " : "")
                        + (thread5min.running ? "5m " : "")
                        + (thread15min.running ? "15m " : "")
                        + (thread30min.running ? "30m " : "")
                        + (thread1hr.running ? "1h " : "")
                        + (thread4hr.running ? "4h " : "")
                        + (thread1day.running ? "1d" : "") + "\"" // }
                    );
                }
            }
            catch (Exception E)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} WriteToCsvFile ERROR: {E.Message} at '{E.StackTrace.Substring(Math.Max(0, E.StackTrace.Length - 50))}'");
            }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("StatusText", typeof(string), typeof(MainWindow));
        public string StatusText
        {
            get { return (string)GetValue(StatusProperty); }
            set { this.SetValue(StatusProperty, value); }
        }

        private void SetClockText(string ClockTxt, string BmText)
        {
            Dispatcher.InvokeAsync(() =>
            {
                ClockText = ClockTxt;
                BMPercentage = BmText;
            });
        }

        public static readonly DependencyProperty ClockProperty = DependencyProperty.Register("ClockText", typeof(string), typeof(MainWindow));
        public string ClockText
        {
            get { return (string)GetValue(ClockProperty); }
            set { this.SetValue(ClockProperty, value); }
        }

        public static readonly DependencyProperty BMPercentageProperty = DependencyProperty.Register("BMPercentage", typeof(string), typeof(MainWindow));
        public string BMPercentage
        {
            get { return (string)GetValue(BMPercentageProperty); }
            set { this.SetValue(BMPercentageProperty, value); }
        }

        public static readonly DependencyProperty LogMessagesProperty = DependencyProperty.Register("LogMessages", typeof(string), typeof(MainWindow));
        public string LogMessages
        {
            get { return (string)GetValue(LogMessagesProperty); }
            set { this.SetValue(LogMessagesProperty, value); }
        }

        public static readonly DependencyProperty LiteDBSignalStatesVisiblesProperty = DependencyProperty.Register("LiteDBSignalStatesVisibles", typeof(string), typeof(MainWindow));
        public string LiteDBSignalStatesVisibles
        {
            get { return (string)GetValue(LiteDBSignalStatesVisiblesProperty); }
            set { this.SetValue(LiteDBSignalStatesVisiblesProperty, value); }
        }

        public static readonly DependencyProperty LiteDBSignalActionsVisiblesProperty = DependencyProperty.Register("LiteDBSignalActionsVisibles", typeof(string), typeof(MainWindow));
        public string LiteDBSignalActionsVisibles
        {
            get { return (string)GetValue(LiteDBSignalActionsVisiblesProperty); }
            set { this.SetValue(LiteDBSignalActionsVisiblesProperty, value); }
        }

        public static readonly DependencyProperty McCheckActiveProperty = DependencyProperty.Register("McCheckActive", typeof(bool), typeof(MainWindow));
        public bool McCheckActive
        {
            get { return (bool)GetValue(McCheckActiveProperty); }
            set { this.SetValue(McCheckActiveProperty, value); }
        }

        public static readonly DependencyProperty LiteDBSignalBuyPriceProperty = DependencyProperty.Register("LiteDBSignalBuyPrice", typeof(string), typeof(MainWindow));
        public string LiteDBSignalBuyPrice
        {
            get { return (string)GetValue(LiteDBSignalBuyPriceProperty); }
            set { this.SetValue(LiteDBSignalBuyPriceProperty, value); }
        }

        public ObservableCollection<SignalsTBL> ListBoxSignals { get; set; }
        public ObservableCollection<SignalsTBL> AllCurrentSignals { get; set; }
        public ObservableCollection<OrderBIN> GridOrderbook { get; set; }
        public ObservableCollection<SignalsTBL> GridAllLiteDBSignals { get; set; }
    }
}
