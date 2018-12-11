using Athena.WpfBindingUtilities;
using System;
using System.Globalization;
using System.Windows.Input;
using Athena;

namespace Athena
{
    public class EndSalesPageViewModel : BaseViewModel
    {
        #region Fields

        private int _mxnPeso20;
        private int _mxnPeso50;
        private int _mxnPeso100;
        private int _mxnPeso200;
        private int _mxnPeso500;
        private int _mxnPeso1000;
        private decimal _mxnPesoCoinsTotal;
        private int _usdDollar1;
        private int _usdDollar5;
        private int _usdDollar10;
        private int _usdDollar20;
        private int _usdDollar50;
        private int _usdDollar100;
        private decimal _usdDollarCoinsTotal;
        private decimal _totalSales;
        private decimal _cardTotalSales;
        private decimal _cashTotalSales;
        private decimal _checkTotalSales;
        private decimal _bankTransferTotalSales;
        private decimal _otherTotalSales;
        private double _pointsTotalUsed;
        private decimal _expensesTotal;
        private decimal _expensesCashTotal;
        private decimal _registerPreviousCash;
        private decimal _registerNewCash;
        private decimal _returnsCashTotal;
        private decimal _returnsCardTotal;
        private decimal _mxnCashBalance;
        private int _returnsTotalItems;
        private decimal _delta;
        private string _comments;
        private Pos _pos;
        private bool master = true;
        #endregion

        #region Observable Properties

        public decimal TotalSales
        {
            get
            {
                return _totalSales;
            }
            set
            {
                _totalSales = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal CardTotalSales
        {
            get
            {
                return _cardTotalSales;
            }
            set
            {
                _cardTotalSales = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal CashTotalSales
        {
            get
            {
                return _cashTotalSales;
            }
            set
            {
                _cashTotalSales = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal CheckTotalSales
        {
            get { return _checkTotalSales; }
            set
            {
                _checkTotalSales = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal BankTransferTotalSales
        {
            get { return _bankTransferTotalSales; }
            set
            {
                _bankTransferTotalSales = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal OtherTotalSales
        {
            get { return _otherTotalSales; }
            set
            {
                _otherTotalSales = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal ExpensesTotal
        {
            get { return _expensesTotal; }
            set
            {
                _expensesTotal = Math.Round(value, 2);
                OnPropertyChanged(); 
            }
        }

        public decimal ExpensesCashTotal
        {
            get { return _expensesCashTotal; }
            set
            {
                _expensesCashTotal = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public double PointsTotalUsed
        {
            get
            {
                return _pointsTotalUsed;
            }
            set
            {
                _pointsTotalUsed = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal RegisterPreviousCash
        {
            get { return _registerPreviousCash; }
            set
            {
                _registerPreviousCash = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal RegisterNewCash
        {
            get { return _registerNewCash; }
            set
            {
                _registerNewCash = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal ReturnsCashTotal
        {
            get { return _returnsCashTotal; }
            set
            {
                _returnsCashTotal = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public decimal ReturnsCardTotal
        {
            get { return _returnsCardTotal; }
            set
            {
                _returnsCardTotal = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        public int ReturnsTotalItems
        {
            get { return _returnsTotalItems; }
            set
            {
                _returnsTotalItems = value;
                OnPropertyChanged();
            }
        }

        public decimal Delta
        {
            get { return _delta; }
            set
            {
                _delta = value;
                OnPropertyChanged();
            }
        }
        
        public decimal MxnCashBalance
        {
            get { return _mxnCashBalance; }
            set
            {
                _mxnCashBalance = value;
                OnPropertyChanged();
            }
        }

        public string Comments
        {
            get { return _comments; }
            set
            {
                _comments = Formatter.SanitizeInput(value); 
                OnPropertyChanged();
            }
        }

        #region Bills and Coins Properties

        public int MxnPeso20
        {
            get
            {
                return _mxnPeso20;
            }
            set
            {
                _mxnPeso20 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int MxnPeso50
        {
            get
            {
                return _mxnPeso50;
            }
            set
            {
                _mxnPeso50 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int MxnPeso100
        {
            get
            {
                return _mxnPeso100;
            }
            set
            {
                _mxnPeso100 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int MxnPeso200
        {
            get
            {
                return _mxnPeso200;
            }
            set
            {
                _mxnPeso200 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int MxnPeso500
        {
            get
            {
                return _mxnPeso500;
            }
            set
            {
                _mxnPeso500 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int MxnPeso1000
        {
            get
            {
                return _mxnPeso1000;
            }
            set
            {
                _mxnPeso1000 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public decimal MxnPesoCoinsTotal
        {
            get
            {
                return _mxnPesoCoinsTotal;
            }
            set
            {
                _mxnPesoCoinsTotal = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int UsdDollar1
        {
            get
            {
                return _usdDollar1;
            }
            set
            {
                _usdDollar1 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int UsdDollar5
        {
            get
            {
                return _usdDollar5;
            }
            set
            {
                _usdDollar5 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int UsdDollar10
        {
            get
            {
                return _usdDollar10;
            }
            set
            {
                _usdDollar10 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int UsdDollar20
        {
            get
            {
                return _usdDollar20;
            }
            set
            {
                _usdDollar20 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int UsdDollar50
        {
            get
            {
                return _usdDollar50;
            }
            set
            {
                _usdDollar50 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public int UsdDollar100
        {
            get
            {
                return _usdDollar100;
            }
            set
            {
                _usdDollar100 = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        public decimal UsdDollarCoinsTotal
        {
            get
            {
                return _usdDollarCoinsTotal;
            }
            set
            {
                _usdDollarCoinsTotal = value;
                CalculateDelta();
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region Properties

        public int FirstReceiptNumber { get; set; }
        public int LastReceiptNumber { get; set; }
        public int TotalItemsSold { get; set; }
        public string EndOfSalesType { get; set; }
        public TransactionDataStruct TransactionData { get; set; }
        public TransactionDataStruct TransactionDataReg { get; set; }
        public EndOfSalesDataStruct EndOfSalesData { get; set; }
        #endregion

        #region Constructors

        public EndSalesPageViewModel()
        {
            _pos = Pos.GetInstance(Constants.DataFolderPath + Constants.PosDataFileName);
            //Calculate sales from transactions
            CalculateInitialCash();
            CalculateExpenses();
            CalculateSales(false, true);
            CalculateDelta();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to calculate the sales, record transaction, print receipt, and backup files
        /// </summary>
        void GenerateEndOfDaySalesReport()
        {
            EndOfSalesType = "Z";
            _pos.GetNextCorteZNumber();
            //Calculate sales and print receipts
            if (master)
            {
                //Regular
                CalculateSales(true);
                //Record End Of Sales Transaction in db
                Transaction.RecordEndOfDaySalesTransaction(Constants.DataFolderPath + Constants.EndOfDaySalesFileName,
                    _pos.LastCorteZNumber, TransactionData.FirstReceiptNumber, TransactionData.LastReceiptNumber, TransactionData.TotalItemsSold,
                    TransactionData.PointsTotal, TransactionData.CashTotal, TransactionData.CardTotal, TransactionData.CheckTotal,
                    TransactionData.BankTotal, TransactionData.OtherTotal, TransactionData.TotalAmountSold, TransactionData.ReturnsCash,
                    TransactionData.ReturnsCard, _pos.ExchangeRate, DateTime.Now.ToString(CultureInfo.CurrentCulture));
                //Print Receipt
                PrintReceipt(ReceiptType.DailyRegular, false);

                CalculateSales(true, true);
                //Record End Of Sales Transaction in db
                Transaction.RecordEndOfDaySalesTransaction(Constants.DataFolderPath + Constants.MasterEndOfDaySalesFileName,
                    _pos.LastCorteZNumber, TransactionData.FirstReceiptNumber, TransactionData.LastReceiptNumber, TransactionData.TotalItemsSold,
                    TransactionData.PointsTotal, TransactionData.CashTotal, TransactionData.CardTotal, TransactionData.CheckTotal,
                    TransactionData.BankTotal, TransactionData.OtherTotal, TransactionData.TotalAmountSold, TransactionData.ReturnsCash,
                    TransactionData.ReturnsCard, _pos.ExchangeRate, DateTime.Now.ToString(CultureInfo.CurrentCulture));
                //Print Receipt
                PrintReceipt(ReceiptType.DailyInternal, false);
            }
            else
            {
                CalculateSales(true);
                CalculateDelta();
                CollectEndOfSalesReceiptInformation();
                //Record End Of Sales Transaction in db
                Transaction.RecordEndOfDaySalesTransaction(Constants.DataFolderPath + Constants.EndOfDaySalesFileName,
                    _pos.LastCorteZNumber, TransactionData.FirstReceiptNumber, TransactionData.LastReceiptNumber, TransactionData.TotalItemsSold,
                    TransactionData.PointsTotal, TransactionData.CashTotal, TransactionData.CardTotal, TransactionData.CheckTotal,
                    TransactionData.BankTotal, TransactionData.OtherTotal, TransactionData.TotalAmountSold, TransactionData.ReturnsCash,
                    TransactionData.ReturnsCard, _pos.ExchangeRate, DateTime.Now.ToString(CultureInfo.CurrentCulture));
                //Print Full Detailed Receipt
                PrintReceipt(ReceiptType.DailyRegular, false);
            }

            //BackUp Z Files and Clear
            Transaction.BackUpTransactionFile(Constants.DataFolderPath + Constants.TransactionsZFileName, true);
           //Transaction.BackUpTransactionMasterFile(Constants.DataFolderPath + Constants.TransactionsMasterFileName);
            Transaction.ClearTransactionFile(Constants.DataFolderPath + Constants.TransactionsZFileName);
            //Transaction.ClearTransactionMasterFile(Constants.DataFolderPath + Constants.TransactionsMasterFileName);
            FileIO.FileBackUp(Constants.DataFolderPath + Constants.InventoryFileName, Constants.DataFolderPath + Constants.InventoryBackupFolderPath);
            //Inventory.InventoryBackUp(Constants.DataFolderPath + Constants.InventoryFileName);
            //BackUp Z Expenses files
            Expense.BackUpExpensesFile(Constants.DataFolderPath + Constants.ExpenseZFileName, true);
            Expense.ClearExpensesFile(Constants.DataFolderPath + Constants.ExpenseZFileName);
            //Backup Z Paymenets Files
            Transaction.BackUpPaymentsFile(Constants.DataFolderPath + Constants.TransactionsPaymentsZFileName, true);
            Transaction.ClearPaymentsFile(Constants.DataFolderPath + Constants.TransactionsPaymentsZFileName);

            //Update POS Data
 //           _pos.LastReceiptNumber = TransactionData.LastReceiptNumber;
 //           _pos.LastTransactionNumber = TransactionData.LastTransactionNumber;
  //          _pos.LastCashierAmountMxn = RegisterNewCash;
    //        _pos.UpdateAllData();
      //      _pos.SaveDataTableToCsv();
        }

        /// <summary>
        /// Method to generate current sales report and print receipt X
        /// </summary>
        public void GenerateCurrentSalesReport()
        {
            EndOfSalesType = "X";
            SaveRegisterCashAmount();

            if (master)
            {
                CalculateSales(false, true);
                CalculateDelta();
                CollectEndOfSalesReceiptInformation();
                //Record End Of Sales Transaction in db
                Transaction.RecordEndOfDaySalesTransaction(Constants.DataFolderPath + Constants.CurrentDaySalesFileName,
                    _pos.LastCorteZNumber + 1, TransactionData.FirstReceiptNumber, TransactionData.LastReceiptNumber, TransactionData.TotalItemsSold,
                    TransactionData.PointsTotal, TransactionData.CashTotal, TransactionData.CardTotal, TransactionData.CheckTotal,
                    TransactionData.BankTotal, TransactionData.OtherTotal, TransactionData.TotalAmountSold, TransactionData.ReturnsCash,
                    TransactionData.ReturnsCard, _pos.ExchangeRate, DateTime.Now.ToString(CultureInfo.CurrentCulture));
                //Print Receipt
                PrintReceipt(ReceiptType.DailyInternal, true);
            }
            else
            {
                CalculateSales(false);
                CalculateDelta();
                CollectEndOfSalesReceiptInformation();
                //Record End Of Sales Transaction in db
                Transaction.RecordEndOfDaySalesTransaction(Constants.DataFolderPath + Constants.CurrentDaySalesFileName,
                    _pos.LastCorteZNumber + 1, TransactionData.FirstReceiptNumber, TransactionData.LastReceiptNumber, TransactionData.TotalItemsSold,
                    TransactionData.PointsTotal, TransactionData.CashTotal, TransactionData.CardTotal, TransactionData.CheckTotal,
                    TransactionData.BankTotal, TransactionData.OtherTotal, TransactionData.TotalAmountSold, TransactionData.ReturnsCash,
                    TransactionData.ReturnsCard, _pos.ExchangeRate, DateTime.Now.ToString(CultureInfo.CurrentCulture));
                //Print Receipt
                PrintReceipt(ReceiptType.DailyRegular, true);
            }

            //BackUp X Files
            Transaction.BackUpTransactionFile(Constants.DataFolderPath + Constants.TransactionsXFileName, false);
            //Transaction.BackUpTransactionMasterFile(Constants.DataFolderPath + Constants.TransactionsMasterFileName);
            Transaction.ClearTransactionFile(Constants.DataFolderPath + Constants.TransactionsXFileName);
            //Transaction.ClearTransactionMasterFile(Constants.DataFolderPath + Constants.TransactionsMasterFileName);
           // Inventory.InventoryBackUp(Constants.DataFolderPath + Constants.InventoryFileName);
            FileIO.FileBackUp(Constants.DataFolderPath + Constants.InventoryFileName, Constants.DataFolderPath + Constants.InventoryBackupFolderPath);
            //BackUp X Expenses files
            Expense.BackUpExpensesFile(Constants.DataFolderPath + Constants.ExpenseXFileName, false);
            Expense.ClearExpensesFile(Constants.DataFolderPath + Constants.ExpenseXFileName);
            //Backup X Payments Files
            Transaction.BackUpPaymentsFile(Constants.DataFolderPath + Constants.TransactionsPaymentsXFileName, false);
            Transaction.ClearPaymentsFile(Constants.DataFolderPath + Constants.TransactionsPaymentsXFileName);

            //Update POS Data
            _pos.LastReceiptNumber = TransactionData.LastReceiptNumber;
            _pos.LastTransactionNumber = TransactionData.LastTransactionNumber;
            _pos.LastCashierAmountMxn = RegisterNewCash;
            _pos.UpdateAllData();
            _pos.SaveDataTableToCsv();
        }

        /// <summary>
        /// Calculate sales and return transactions summary
        /// </summary>
        /// <param name="endDayFlag"></param>
        /// <param name="intFlag"></param>
        /// <returns></returns>
        private TransactionDataStruct CalculateSales(bool endDayFlag, bool intFlag = false)
        {
            Transaction.GetTransactionsData(_pos, endDayFlag, out var transactionData, intFlag);
            TransactionData = transactionData;
            TotalItemsSold = transactionData.TotalItemsSold;
            FirstReceiptNumber = transactionData.FirstReceiptNumber;
            LastReceiptNumber = transactionData.LastReceiptNumber;
            CashTotalSales = transactionData.CashTotal;
            CardTotalSales = transactionData.CardTotal;
            BankTransferTotalSales = transactionData.BankTotal;
            CheckTotalSales = transactionData.CheckTotal;
            PointsTotalUsed = transactionData.PointsTotal;
            ReturnsCardTotal = transactionData.ReturnsCard;
            ReturnsCashTotal = transactionData.ReturnsCash;
            ReturnsTotalItems = transactionData.TotalReturnItems;
            OtherTotalSales = transactionData.OtherTotal;
            TotalSales = transactionData.TotalAmountSold;

            MxnCashBalance = CashTotalSales - ReturnsCashTotal - ExpensesCashTotal;

            return TransactionData;
        }

        /// <summary>
        /// Collect information required for the end of sales receipt based on user inputs during procedure
        /// </summary>
        private void CollectEndOfSalesReceiptInformation()
        {
            EndOfSalesData = new EndOfSalesDataStruct()
            {
                User = MainWindowViewModel.GetInstance().CurrentUser.Name,
                Comments = Comments,
                EndOfSalesReceiptType = EndOfSalesType,
                ExpensesCash = ExpensesCashTotal,
                ExpensesTotal = ExpensesTotal,
                ExchangeRate = MainWindowViewModel.GetInstance().ExchangeRate,
                InitialCash = RegisterPreviousCash,
                NewInitialCash = RegisterNewCash,
                SalesOffset = Delta,
                MxnCoins = MxnPesoCoinsTotal,
                Mxn20 = MxnPeso20,
                Mxn50 = MxnPeso50,
                Mxn100 = MxnPeso100,
                Mxn200 = MxnPeso200,
                Mxn500 = MxnPeso500,
                Mxn1000 = MxnPeso1000,
                UsdCoins = UsdDollarCoinsTotal,
                Usd1 = UsdDollar1,
                Usd5 = UsdDollar5,
                Usd10 = UsdDollar10,
                Usd20 = UsdDollar20,
                Usd50 = UsdDollar50,
                Usd100 = UsdDollar100,
                Delta = Delta,
                UsdTotalCash = UsdDollar1 + UsdDollar5 * 5 + UsdDollar10 * 10 + UsdDollar20 * 20 + 
                               UsdDollar50 * 50 + UsdDollar100 * 100 + UsdDollarCoinsTotal,
                MxnTotalCash = MxnPeso20 * 20 + MxnPeso50 * 50 + MxnPeso100 * 100 + MxnPeso200 * 200 + 
                               MxnPeso500 * 500 + MxnPeso1000 * 1000 + MxnPesoCoinsTotal
            };
        }

        /// <summary>
        /// Calculate the expenses for the current period
        /// </summary>
        private void CalculateExpenses()
        {
            var expenses = new Expense(Constants.DataFolderPath + Constants.ExpenseXFileName, Constants.DataFolderPath + Constants.ExpenseZFileName, 
                Constants.DataFolderPath + Constants.ExpenseXFileName);
            expenses.GetTotal(out var expensesMxn, out var expensesUsd, out var expensesCashMxn, out var expensesCashUsd);
            ExpensesTotal = expensesMxn + expensesUsd * _pos.ExchangeRate;
            ExpensesCashTotal = expensesCashMxn + expensesCashUsd * _pos.ExchangeRate;
        }

        /// <summary>
        /// Calculate the delta between the sales, expenses, and available cash
        /// </summary>
        private void CalculateDelta()
        {
            //Total cash available in register
            var cashMxn = MxnPeso20*20 + MxnPeso50*50 + MxnPeso100*100 + MxnPeso200*200 + MxnPeso500*500 + MxnPeso1000*1000 + MxnPesoCoinsTotal;
            var cashUsd = UsdDollar1 + UsdDollar5*5 + UsdDollar10*10 + UsdDollar20*20 + UsdDollar50*50 + UsdDollar100*100 + UsdDollarCoinsTotal;
            var totalCash = cashMxn + cashUsd * _pos.ExchangeRate;
            //Calculate delta
            Delta = totalCash + ExpensesCashTotal + ReturnsCashTotal - RegisterPreviousCash - CashTotalSales;     
        }

        /// <summary>
        /// Get initial register cash
        /// </summary>
        private void CalculateInitialCash()
        {
            RegisterPreviousCash = _pos.GetRegisterCashAmount();
        }

        /// <summary>
        /// Save current register cash amount
        /// </summary>
        private void SaveRegisterCashAmount()
        {
            _pos.UpdateRegisterCashAmount(RegisterNewCash);
        }

        /// <summary>
        /// Print the sales summary receipt either with or without details
        /// </summary>
        /// <param name="receiptType"></param>
        /// <param name="fullReceipt"></param>
        private void PrintReceipt(ReceiptType receiptType, bool fullReceipt)
        {
            var receipt = new Receipt(_pos, receiptType, TransactionData, EndOfSalesData);
            if (fullReceipt)
            {
                receipt.PrintEndOfDaySalesFullReceipt();

            }
            else
            {
                receipt.PrintEndOfDaySalesReceipt();
            }
        }
        #endregion

        #region Commands

        #region GenerateEndOfDaySalesReportCommand

        public ICommand GenerateEndOfDaySalesReportCommand { get { return _generateEndOfDaySalesReportCommand ?? (_generateEndOfDaySalesReportCommand = new DelegateCommand(Execute_GenerateEndOfDaySalesReportCommand, CanExecute_GenerateEndOfDaySalesReportCommand)); } }

        private ICommand _generateEndOfDaySalesReportCommand;

        internal void Execute_GenerateEndOfDaySalesReportCommand(object parameter)
        {
            switch ((string)parameter)
            {
                case "x":
                    GenerateCurrentSalesReport();
                    //Log
                    MainWindowViewModel.GetInstance().Log.Write(MainWindowViewModel.GetInstance().CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Corte X realizado");
                    MainWindowViewModel.GetInstance().Code = "Corte X realizado!";                    
                    break;
                  
                case "z":
                    GenerateEndOfDaySalesReport();
                    //Log
                    MainWindowViewModel.GetInstance().Log.Write(MainWindowViewModel.GetInstance().CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Corte Z realizado");
                    MainWindowViewModel.GetInstance().Code = "Corte Z realizado!";                                     
                    break;
                    
            }
            MainWindowViewModel.GetInstance().CurrentPage = "\\View\\PosGeneralPage.xaml";

        }

        internal bool CanExecute_GenerateEndOfDaySalesReportCommand(object parameter)
        {
            switch ((string)parameter)
            {
                case "x":
                    return RegisterNewCash != 0;
                case "z":
                    return CashTotalSales + CardTotalSales + CheckTotalSales + BankTransferTotalSales +
                           ExpensesCashTotal + ExpensesTotal + ReturnsCashTotal + ReturnsCardTotal == 0;
                default:
                    return false;
            }
        }
        #endregion

        #endregion
    }

    public enum EndOfSaleReportTypeEnum
    {
        IntraDayReport,
        EndOfDayReport
    }
}
