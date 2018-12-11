using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena
{
    public struct TransactionDataStruct
    {
        public List<Tuple<string, int, decimal>> SalesInfoPerCategory { get; set; }
        public TransactionType TransactionType { get; set; }
        public PaymentTypeEnum PaymentType { get; set; }
        public int FirstReceiptNumber { get; set; }
        public int LastReceiptNumber { get; set; }
        public int LastTransactionNumber { get; set; }
        public int EndOfSalesNumber { get; set; }
        public int LastInternalTransactionNumber { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal TotalAmountSold { get; set; }
        public decimal CashTotal { get; set; }
        public decimal CardTotal { get; set; }
        public decimal CheckTotal { get; set; }
        public decimal BankTotal { get; set; }
        public decimal OtherTotal { get; set; }
        public double PointsTotal { get; set; }
        public decimal ReturnsCard{ get; set; }
        public decimal ReturnsCash { get; set; }
        public int TotalReturnItems { get; set; }
    }
}