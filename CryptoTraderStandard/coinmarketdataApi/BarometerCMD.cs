using System;

namespace CryptoTraderStandard.coinmarketdataApi
{
    public class BarometerCMD
    {
        public BarometerCMD()
        {
            // Initial / Default values
            Quotename = "";
            PeriodeMinutes = PairCount = 0;
            BmPercentage = PreviousSum = NowSum = Decimal.Zero;
            PreviousDatetime = NowDatetime = DateTime.MinValue;
        }

        public string Quotename { get; set; }
        public int PeriodeMinutes { get; set; }
        public Decimal BmPercentage { get; set; }
        public Decimal PreviousSum { get; set; }
        public Decimal NowSum { get; set; }
        public DateTime PreviousDatetime { get; set; }
        public DateTime NowDatetime { get; set; }
        public int PairCount { get; set; }
        public string ToLogMsg()
        {
            return "Quote=" + Quotename + " pairs=" + PairCount.ToString()
                + " periode=" + PeriodeMinutes.ToString()
                + ": " + PreviousDatetime.ToString() + ".." + NowDatetime.ToString()
                + " " + PreviousSum.ToString("f5") + ".." + NowSum.ToString("f5")
                + " => " + BmPercentage.ToString("f4");
        }
        public string ToCsvHeader(string PeriodCode)
        {
            return $"=\"Quote{PeriodCode}\";\"Period\";\"Bm{PeriodCode} %\";\"Sum{PeriodCode} start\";\"Sum{PeriodCode} end\";\"Datetime{PeriodCode} start\";\"Datetime{PeriodCode} end\";\"Pairs{PeriodCode}\"";
        }
        public string ToCsvData()
        {
            return "\"" + Quotename + "\";" + PeriodeMinutes + ";" + BmPercentage + ";" + PreviousSum + ";" + NowSum + ";"
                + PreviousDatetime.ToString("dd-MM-yyyy HH:mm:00") + ";" + NowDatetime.ToString("dd-MM-yyyy HH:mm:00") + ";" + PairCount;
        }
    }
}
