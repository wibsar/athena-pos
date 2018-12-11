using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena
{
    public struct EndOfSalesDataStruct
    {
        public string User { get; set; }
        public string EndOfSalesReceiptType { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal ExpensesCash { get; set; }
        public decimal ExpensesTotal { get; set; }
        public decimal InitialCash { get; set; }
        public decimal NewInitialCash { get; set; }
        public decimal SalesOffset { get; set; }
        public decimal MxnCoins { get; set; }
        public decimal Mxn20{ get; set; }
        public decimal Mxn50 { get; set; }
        public decimal Mxn100 { get; set; }
        public decimal Mxn200 { get; set; }
        public decimal Mxn500 { get; set; }
        public decimal Mxn1000 { get; set; }
        public decimal UsdCoins { get; set; }
        public decimal Usd1 { get; set; }
        public decimal Usd5 { get; set; }
        public decimal Usd10 { get; set; }
        public decimal Usd20 { get; set; }
        public decimal Usd50 { get; set; }
        public decimal Usd100 { get; set; }
        public decimal Delta { get; set; }
        public decimal MxnTotalCash { get; set; }
        public decimal UsdTotalCash { get; set; }
        public string Comments { get; set; }
    }
}