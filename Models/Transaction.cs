using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenericParsing;

namespace Athena
{
    public class Transaction: DataBase
    {
        #region Fields

        private string _transactionFilePath;
        private string _transactionMasterFilePath;
        private string _transactionHistoryFilePath;
        private int _internalNumber;
        private int _transactionNumber;
        private int _receiptNumber;
        private ProductBase _product;
        private DateTime _transactionDate;
        private string _customerName;
        private string _userName;
        private string _fiscalReceiptRequireed;
        private TransactionType _saleType;
        private PaymentTypeEnum _paymentType;
        private int _orderNumber;
        private decimal _totalDue;
        private decimal _amountPaid;
        private decimal _changeDue;

        //product related fields
        private string _productCode;
        private int _productNumber;
        private string _productCategory;
        private string _productDescription;
        private decimal _productPrice;
        private decimal _productTotalSale;
        private int _productQuantitySold;

        #endregion

        #region Properties

        public string TranscationFilePath { get; set; }
        public List<string> TransactionTypes { get; set; }

        public int TransactionNumber { get => _transactionNumber; set => _transactionNumber = value; }
        public int InternalNumber { get => _internalNumber; set => _internalNumber = value; }
        public int ReceiptNumber { get; set; }
        public ProductBase Product { get => _product; set => _product = value; }
        public DateTime TransactionDate { get => _transactionDate; set => _transactionDate = value; }
        public string CustomerName { get => _customerName; set => _customerName = value; }
        public string UserName { get => _userName; set => _userName = value; }
        public string FiscalReceiptRequired { get => _fiscalReceiptRequireed; set => _fiscalReceiptRequireed = value; }
        public TransactionType SaleType { get => _saleType; set => _saleType = value; }
        public PaymentTypeEnum PaymentType { get => _paymentType; set => _paymentType = value; }
        public int OrderNumber { get => _orderNumber; set => _orderNumber = value; }
        public decimal TotalDue
        {
            get
            {
                return _totalDue;
            }
            set
            {
                _totalDue = value;
            }
        }

        public decimal AmountPaid
        {
            get
            {
                return _amountPaid;
            }
            set
            {
                _amountPaid = value;
            }
        }

        public decimal ChangeDue
        {
            get
            {
                return _changeDue;
            }
            set
            {
                _changeDue = value;
            }
        }

        public string ProductCode { get => _productCode; set => _productCode = value; }
        public int ProductNumber { get => _productNumber; set => _productNumber = value; }
        public string ProductCategory { get => _productCategory; set => _productCategory = value; }
        public string ProductDescription { get => _productDescription; set => _productDescription = value; }
        public decimal ProductPrice { get => _productPrice; set => _productPrice = value; }
        public decimal ProductTotalSale { get => _productTotalSale; set => _productTotalSale = value; }
        public int ProductQuantitySold { get => _productQuantitySold; set => _productQuantitySold = value; }

        #endregion

        #region Constructors

        public Transaction(string transactionFilePath) : base (transactionFilePath)
        {
            TranscationFilePath = transactionFilePath;

            try
            {
                var lines = File.ReadAllLines(transactionFilePath);
                TransactionTypes = lines.ToList();
            }
            catch (Exception)
            {
                TransactionTypes.Add("Regular");
            }
        }

        public Transaction(string transactionFilePath, string transactionMasterFilePath, string transactionHistoryFilePath,
            bool datafile) : base(transactionMasterFilePath)
        {
            _transactionFilePath = transactionFilePath;
            _transactionMasterFilePath = transactionMasterFilePath;
            _transactionHistoryFilePath = transactionHistoryFilePath;
        }

        public Transaction(string transactionFilePath, bool datafile) : base (transactionFilePath)
        {
            _transactionFilePath = transactionFilePath;
        }

        public Transaction(string transactionFilePath, bool loadFile, bool analysis) : base(transactionFilePath)
        {
            _transactionFilePath = transactionFilePath;
            LoadCsvToDataTable();
        }

        #endregion  

        #region Methods

        public bool UpdateToTable(Transaction transaction)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row["NumeroTransaccion"].ToString() == transaction.TransactionNumber.ToString() && row["NumeroTicket"].ToString() == transaction.ReceiptNumber.ToString() &&
                    row["FechaVenta"].ToString() == transaction.TransactionDate.ToString() && row["TipoVenta"].ToString() == transaction.SaleType.ToString())
                {
                    row["PrecioVendido"] = transaction.ProductPrice;
                    row["UnidadesVendidas"] = transaction.ProductQuantitySold;
                    row["TotalVendido"] = Math.Round(transaction.ProductPrice * transaction.ProductQuantitySold, 2);
                    row["FechaVenta"] = transaction.TransactionDate;
                    break;
                }
            }
            return true;
        }

        public bool UpdateToTable()
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row["NumeroTransaccion"].ToString() == this.TransactionNumber.ToString() && row["NumeroTicket"].ToString() == this.ReceiptNumber.ToString() &&
                    row["FechaVenta"].ToString() == this.TransactionDate.ToString() && row["TipoVenta"].ToString() == this.SaleType.ToString())
                {
                    row["PrecioVendido"] = this.ProductPrice;
                    row["UnidadesVendidas"] = this.ProductQuantitySold;
                    row["TotalVendido"] = Math.Round(this.ProductPrice * this.ProductQuantitySold, 2);
                    row["FechaVenta"] = this.TransactionDate;
                    break;
                }
            }
            return true;
        }

        public void Delete()
        {
            for (int index = 0; index < base.DataTable.Rows.Count; index++)
            {
                var row = base.DataTable.Rows[index];
                if (row["NumeroTransaccion"].ToString() == this.TransactionNumber.ToString() && row["NumeroTicket"].ToString() == this.ReceiptNumber.ToString() &&
                    row["FechaVenta"].ToString() == this.TransactionDate.ToString() && row["TipoVenta"].ToString() == this.SaleType.ToString())
                {
                    DataTable.Rows[index].Delete();
                    return;
                }
            }
        }

        public List<Transaction> Search(string searchInput)
        {
            var transactions = new List<Transaction>();

            //Return empty list if invalid inputs are entered for the search
            if (string.IsNullOrWhiteSpace(searchInput) || searchInput == "x")
                return transactions;

            if (searchInput == "*")
            {
                var allFields = base.DataTable.AsEnumerable();
                foreach (var row in allFields)
                {
                    var transaction = new Transaction(base.FilePath, true, true)
                    {
                        TransactionNumber = Int32.Parse(row["NumeroTransaccion"].ToString()),
                        ReceiptNumber = Int32.Parse(row["NumeroTicket"].ToString()),
                        ProductCode = row["Codigo"].ToString(),
                        ProductCategory = row["CategoriaProducto"].ToString(),
                        ProductDescription = row["Descripcion"].ToString(),
                        ProductPrice = Decimal.Parse(row["PrecioVendido"].ToString()),
                        ProductQuantitySold = Int32.Parse(row["UnidadesVendidas"].ToString()),
                        ProductTotalSale = Decimal.Parse(row["TotalVendido"].ToString()),
                        TransactionDate = Convert.ToDateTime(row["FechaVenta"].ToString()),
                        CustomerName = row["Cliente"].ToString(),
                        UserName = row["Usuario"].ToString()                  
                    };

                    transaction.SaleType = (TransactionType)Enum.Parse(typeof(TransactionType), row["TipoVenta"].ToString(), true);
                    transaction.PaymentType = (PaymentTypeEnum)Enum.Parse(typeof(PaymentTypeEnum), row["MetodoPago"].ToString(), true);

                    transactions.Add(transaction);
                }
                return transactions;
            }

            var ticketFilter = base.DataTable.AsEnumerable().Where(r => r.Field<string>("NumeroTicket").ToLower().Contains(searchInput));
            var clientFilter = base.DataTable.AsEnumerable().Where(r => r.Field<string>("Cliente").ToLower().Contains(searchInput));

            foreach (var row in ticketFilter)
            {
                var transaction = new Transaction(base.FilePath, true, true)
                {
                    TransactionNumber = Int32.Parse(row["NumeroTransaccion"].ToString()),
                    ReceiptNumber = Int32.Parse(row["NumeroTicket"].ToString()),
                    ProductCode = row["Codigo"].ToString(),
                    ProductCategory = row["CategoriaProducto"].ToString(),
                    ProductDescription = row["Descripcion"].ToString(),
                    ProductPrice = Decimal.Parse(row["PrecioVendido"].ToString()),
                    ProductQuantitySold = Int32.Parse(row["UnidadesVendidas"].ToString()),
                    ProductTotalSale = Decimal.Parse(row["TotalVendido"].ToString()),
                    TransactionDate = Convert.ToDateTime(row["FechaVenta"].ToString()),
                    CustomerName = row["Cliente"].ToString(),
                    UserName = row["Usuario"].ToString()
                };

                transaction.SaleType = (TransactionType)Enum.Parse(typeof(TransactionType), row["TipoVenta"].ToString(), true);
                transaction.PaymentType = (PaymentTypeEnum)Enum.Parse(typeof(PaymentTypeEnum), row["MetodoPago"].ToString(), true);

                transactions.Add(transaction);
            }

            foreach (var row in clientFilter)
            {
                var transaction = new Transaction(base.FilePath, true, true)
                {
                    TransactionNumber = Int32.Parse(row["NumeroTransaccion"].ToString()),
                    ReceiptNumber = Int32.Parse(row["NumeroTicket"].ToString()),
                    ProductCode = row["Codigo"].ToString(),
                    ProductCategory = row["CategoriaProducto"].ToString(),
                    ProductDescription = row["Descripcion"].ToString(),
                    ProductPrice = Decimal.Parse(row["PrecioVendido"].ToString()),
                    ProductQuantitySold = Int32.Parse(row["UnidadesVendidas"].ToString()),
                    ProductTotalSale = Decimal.Parse(row["TotalVendido"].ToString()),
                    TransactionDate = Convert.ToDateTime(row["FechaVenta"].ToString()),
                    CustomerName = row["Cliente"].ToString(),
                    UserName = row["Usuario"].ToString()
                };

                transaction.SaleType = (TransactionType)Enum.Parse(typeof(TransactionType), row["TipoVenta"].ToString(), true);
                transaction.PaymentType = (PaymentTypeEnum)Enum.Parse(typeof(PaymentTypeEnum), row["MetodoPago"].ToString(), true);

                //Add if it does not exist already
                if (!transactions.Exists(x => x.ReceiptNumber == transaction.ReceiptNumber))
                    transactions.Add(transaction);
            }

            return transactions;
        }

        /// <summary>
        /// Record transaction data into databases
        /// </summary>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        public bool Record(TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Regular:
                case TransactionType.Interno:

                    var data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                         TransactionNumber, ReceiptNumber, Product.Code, Product.Id, Product.Category,
                         Product.Description, Product.Price, Product.LastQuantitySold, Product.LastQuantitySold * Product.Price,
                         TransactionDate, CustomerName, UserName, FiscalReceiptRequired, SaleType, PaymentType.ToString(), OrderNumber) + Environment.NewLine;
                     
                    return SaveTransaction(_transactionFilePath, data) && SaveTransaction(_transactionMasterFilePath, data) &&
                        SaveTransaction(_transactionHistoryFilePath, data);
                   
                case TransactionType.Remover:
                    var removalData = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                          TransactionNumber, ReceiptNumber, Product.Code, Product.Id, Product.Category,
                          Product.Description, Product.Price * 0, Product.LastQuantitySold, Product.LastQuantitySold * Product.Price * 0,
                          TransactionDate, CustomerName, UserName, FiscalReceiptRequired, SaleType, PaymentType.ToString(), OrderNumber) + Environment.NewLine;

                    return SaveTransaction(_transactionFilePath, removalData) && SaveTransaction(_transactionMasterFilePath, removalData)
                           && SaveTransaction(_transactionHistoryFilePath, removalData);

                case TransactionType.DevolucionEfectivo:
                case TransactionType.DevolucionTarjeta:
                    var returnData = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                        TransactionNumber, ReceiptNumber, Product.Code, Product.Id, Product.Category,
                        Product.Description, -1 * Product.Price, -1 * Product.LastQuantitySold, -1 * Product.LastQuantitySold * Product.Price,
                        TransactionDate, CustomerName, UserName, FiscalReceiptRequired, SaleType, PaymentType.ToString(), OrderNumber) + Environment.NewLine;

                    return SaveTransaction(_transactionFilePath, returnData) && SaveTransaction(_transactionMasterFilePath, returnData) &&
                        SaveTransaction(_transactionHistoryFilePath, returnData);

                default:
                    return false;
            }
        }

        private bool SaveTransaction(string transactionDataFilePath, string data)
        {
            try
            {
                File.AppendAllText(transactionDataFilePath, data);
            }
            catch (Exception e)
            {
                //Report Error in log file
            }
            return true;
        }

        //Method to create a backup file
        public static void BackUpTransactionFile(string transactionFilePath, bool endDayFlag)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            var currentTime = DateTime.Now;

            var transFileType = endDayFlag ? "Z" : "X";

            string TransactionFileBackUpCopyName = Constants.DataFolderPath + Constants.TransactionsBackupFolderPath +
                "Transacciones" + transFileType + "_" + currentTime.Day.ToString("00") + currentTime.Month.ToString("00") +
                currentTime.Year.ToString("0000") + currentTime.Hour.ToString("00") + currentTime.Minute.ToString("00") + 
                currentTime.Second.ToString("00") + ".csv";

            File.Copy(transactionFilePath, TransactionFileBackUpCopyName);
        }
        ////Method to create a backup file
        //public static void BackUpTransactionMasterFile(string transactionFilePath)
        //{
        //    Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
        //    var currentTime = DateTime.Now;
        //    //Load inventory csv file and create a backup copy
        //    string TransactionFileBackUpCopyName = Constants.DataFolderPath + Constants.TransactionsBackupFolderPath +
        //        "TransaccionesMaster" + currentTime.Day.ToString("00") + currentTime.Month.ToString("00") +
        //        currentTime.Year.ToString("0000") + currentTime.Hour.ToString("00") + currentTime.Minute.ToString("00") +
        //        currentTime.Second.ToString("00") + ".csv";

        //    File.Copy(transactionFilePath, TransactionFileBackUpCopyName);
        //}

        /// <summary>
        /// Clears the transaction file
        /// </summary>
        /// <param name="transactionFilePath"></param>
        public static void ClearTransactionFile(string transactionFilePath)
        {
            File.Copy(Constants.DataFolderPath + Constants.TransactionsBackupFolderPath + Constants.TransactionBlankFileName,
                transactionFilePath, true);
        }

        ////Method to clear file
        //public static void ClearTransactionMasterFile(string transactionFilePath)
        //{
        //    File.Copy(Constants.DataFolderPath + Constants.TransactionsBackupFolderPath + Constants.TransactionMasterBlankFileName,
        //        transactionFilePath, true);
        //}

        /// <summary>
        /// Cleans payment file
        /// </summary>
        /// <param name="paymentsFilePath"></param>
        /// <param name="endDayFlag"></param>
        public static void BackUpPaymentsFile(string paymentsFilePath, bool endDayFlag)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            var currentTime = DateTime.Now;
            var transFileType = endDayFlag ? "Z" : "X";
            string PaymentsFileBackUpCopyName = Constants.DataFolderPath + Constants.TransactionsBackupFolderPath + "Pagos" + transFileType + "_" +
                currentTime.Day.ToString("00") + currentTime.Month.ToString("00") + currentTime.Year.ToString("0000") +
                currentTime.Hour.ToString("00") + currentTime.Minute.ToString("00") + currentTime.Second.ToString("00") + ".csv";

            File.Copy(paymentsFilePath, PaymentsFileBackUpCopyName);
        }

        //Method to clear file
        public static void ClearPaymentsFile(string paymentsFilePath)
        {
            File.Copy(Constants.DataFolderPath + Constants.TransactionsBackupFolderPath + Constants.TransactionsPaymentsBlankFileName,
                paymentsFilePath, true);
        }

        ///// <summary>
        ///// Get transaction sales data for end of day sales report
        ///// </summary>
        ///// <param name="transactionType"></param>
        ///// <param name="posData"></param>
        ///// <param name="transactionData"></param>
        ///// <returns></returns>
        //public static List<Tuple<string, int, decimal>> GetTransactionsData(TransactionType transactionType,
        //    Pos posData, out TransactionDataStruct transactionData)
        //{
        //    System.Data.DataTable data;
        //    decimal cashMxnOffset = 0;
        //    decimal cashUsdOffset = 0;
        //    decimal cardUsdOffset = 0;
        //    decimal transferMxnOffset = 0;
        //    decimal checkMxnOffset = 0;
        //    decimal otherMxnOffset = 0;
        //    decimal tempPartialTotalMxn = 0;
        //    var ticketsList = new List<int>();

        //    transactionData = new TransactionDataStruct
        //    {
        //        TotalAmountSold = 0,
        //        TotalItemsSold = 0,
        //        CashTotal = 0,
        //        CardTotal = 0,
        //        CheckTotal = 0,
        //        BankTotal = 0,
        //        OtherTotal = 0,
        //        PointsTotal = 0,
        //        ReturnsCash = 0,
        //        ReturnsCard = 0,
        //        TotalReturnItems = 0,
        //        LastTransactionNumber = 0,
        //        LastInternalTransactionNumber = 0,
        //        EndOfSalesNumber = 0,
        //        LastReceiptNumber = 0
        //    };

        //    string selectedFilePath = String.Empty;

        //    var categoryData = new List<Tuple<string, int, decimal>>();

        //    //Open current transaction file and get data
        //    if (transactionType == TransactionType.Interno)
        //    {
        //        selectedFilePath = posData.TransactionMasterDataFilePath;
        //    }
        //    else if (transactionType == TransactionType.Regular)
        //    {
        //        selectedFilePath = posData.TransactionsDataFilePath;
        //    }

        //    using (var parser = new GenericParserAdapter(selectedFilePath))
        //    {
        //        parser.ColumnDelimiter = ',';
        //        parser.FirstRowHasHeader = true;
        //        parser.SkipStartingDataRows = 0;
        //        parser.SkipEmptyRows = true;
        //        parser.MaxBufferSize = 4096;
        //        parser.MaxRows = 8000;

        //        data = parser.GetDataTable();
        //    }

        //    var categories = CategoryCatalog.GetList(posData.Catalog);

        //    //Add points category
        //    categories.Add("Puntos");

        //    //Get each category
        //    foreach (var category in categories)
        //    {
        //        var amount = 0M;
        //        var itemsNumber = 0;

        //        for (var index = 0; index < data.Rows.Count; index++)
        //        {
        //            var row = data.Rows[index];
        //            //Separate categories
        //            if (row["CategoriaProducto"].ToString() == category)
        //            {
        //                amount += decimal.Parse(row["TotalVendido"].ToString());

        //                if (category != "Puntos")
        //                    itemsNumber += int.Parse(row["UnidadesVendidas"].ToString());

        //                //Get payment method
        //                if (row["TipoVenta"].ToString() != "DevolucionEfectivo" && row["TipoVenta"].ToString() != "DevolucionTarjeta")
        //                {
        //                    switch (row["MetodoPago"].ToString())
        //                    {
        //                        case "Cash":
        //                        case "Efectivo":
        //                            transactionData.CashTotal += decimal.Parse(row["TotalVendido"].ToString());
        //                            break;
        //                        case "Card":
        //                        case "Tarjeta":
        //                            transactionData.CardTotal += decimal.Parse(row["TotalVendido"].ToString());
        //                            break;
        //                        case "Check":
        //                        case "Cheque":
        //                            transactionData.CheckTotal += decimal.Parse(row["TotalVendido"].ToString());
        //                            break;
        //                        case "BankTransfer":
        //                        case "Transferencia":
        //                            transactionData.BankTotal += decimal.Parse(row["TotalVendido"].ToString());
        //                            break;
        //                        case "Other":
        //                        case "Otro":
        //                            transactionData.OtherTotal += decimal.Parse(row["TotalVendido"].ToString());
        //                            break;
        //                        case "Partial":
        //                        case "Parcial":
        //                            //add ticket number to list so it can be searched later
        //                            if (!ticketsList.Contains(Int32.Parse(row["NumeroTicket"].ToString())))
        //                            {
        //                                ticketsList.Add(Int32.Parse(row["NumeroTicket"].ToString()));
        //                            }
        //                            tempPartialTotalMxn += decimal.Parse(row["TotalVendido"].ToString());
        //                            break;
        //                        default:
        //                            transactionData.OtherTotal += decimal.Parse(row["TotalVendido"].ToString());
        //                            break;
        //                    }
        //                }

        //                if (row["Descripcion"].ToString() == "Puntos Descuento")
        //                {
        //                    transactionData.PointsTotal += double.Parse(row["TotalVendido"].ToString()) * -1;
        //                }

        //                if (row["TipoVenta"].ToString() == "DevolucionEfectivo")
        //                {
        //                    transactionData.ReturnsCash += decimal.Parse(row["TotalVendido"].ToString()) * -1;
        //                    transactionData.TotalReturnItems -= Int32.Parse(row["UnidadesVendidas"].ToString());
        //                }

        //                if (row["TipoVenta"].ToString() == "DevolucionTarjeta")
        //                {
        //                    transactionData.ReturnsCard += decimal.Parse(row["TotalVendido"].ToString()) * -1;
        //                    transactionData.TotalReturnItems -= Int32.Parse(row["UnidadesVendidas"].ToString());
        //                }
        //            }
        //        }

        //        categoryData.Add(new Tuple<string, int, decimal>(category.ToString(), itemsNumber, amount));
        //        transactionData.TotalAmountSold += amount;
        //        transactionData.TotalItemsSold += itemsNumber;
        //    }

        //    //Subtract returns
        //    transactionData.TotalAmountSold = transactionData.TotalAmountSold + transactionData.ReturnsCash + transactionData.ReturnsCard;

        //    //Get first and last receipt number
        //    if (data.Rows.Count > 1)
        //    {
        //        var firstRow = data.Rows[0];
        //        var lastRow = data.Rows[data.Rows.Count - 1];
        //        transactionData.FirstReceiptNumber = Int32.Parse(firstRow["NumeroTicket"].ToString());
        //        transactionData.LastReceiptNumber = Int32.Parse(lastRow["NumeroTicket"].ToString());
        //        if (transactionType == TransactionType.Interno)
        //        {
        //            transactionData.LastTransactionNumber = Int32.Parse(lastRow["NumeroTransaccion"].ToString());
        //        }
        //    }
        //    else if (data.Rows.Count == 1)
        //    {
        //        var firstRow = data.Rows[0];
        //        var lastRow = data.Rows[0];
        //        transactionData.FirstReceiptNumber = Int32.Parse(firstRow["NumeroTicket"].ToString());
        //        transactionData.LastReceiptNumber = Int32.Parse(lastRow["NumeroTicket"].ToString());
        //        if (transactionType == TransactionType.Interno)
        //        {
        //            transactionData.LastTransactionNumber = Int32.Parse(lastRow["NumeroTransaccion"].ToString());
        //        }
        //    }
        //    transactionData.SalesInfoPerCategory = categoryData;

        //    //Search for partial payment tickets in payments db
        //    decimal cashMxn = 0, cashUsd = 0, cardMxn = 0, transferMxn = 0, checkMxn = 0, otherMxn = 0, totalChangeMxn = 0, totalSoldMxn = 0;
        //    if (ticketsList.Count > 0)
        //    {
        //        GetSalePaymentTotalPerType(transactionType, ticketsList, out cashMxn, out cashUsd, out cardMxn, out transferMxn, out checkMxn,
        //            out otherMxn, out totalChangeMxn, out totalSoldMxn);
        //    }

        //    transactionData.CashTotal += (cashMxn + Math.Round(cashUsd * MainWindowViewModel.GetInstance().ExchangeRate, 2) - totalChangeMxn);
        //    transactionData.CardTotal += cardMxn;
        //    transactionData.CheckTotal += checkMxn;
        //    transactionData.BankTotal += transferMxn;
        //    transactionData.OtherTotal += otherMxn;

        //    return categoryData;
        //}

        /// <summary>
        /// Get transaction sales data for sales report with flag
        /// </summary>
        /// <param name="posData"></param>
        /// <param name="endDayFlag"></param>
        /// <param name="transactionData"></param>
        /// <param name="intFlag"></param>
        /// <returns></returns>
        public static List<Tuple<string, int, decimal>> GetTransactionsData(Pos posData, bool endDayFlag,
            out TransactionDataStruct transactionData, bool intFlag = false)
        {
            System.Data.DataTable data;
            decimal cashMxnOffset = 0;
            decimal cashUsdOffset = 0;
            decimal cardUsdOffset = 0;
            decimal transferMxnOffset = 0;
            decimal checkMxnOffset = 0;
            decimal otherMxnOffset = 0;
            decimal tempPartialTotalMxn = 0;
            var ticketsList = new List<int>();

            transactionData = new TransactionDataStruct
            {
                TotalAmountSold = 0,
                TotalItemsSold = 0,
                CashTotal = 0,
                CardTotal = 0,
                CheckTotal = 0,
                BankTotal = 0,
                OtherTotal = 0,
                PointsTotal = 0,
                ReturnsCash = 0,
                ReturnsCard = 0,
                TotalReturnItems = 0,
                LastTransactionNumber = 0,
                LastInternalTransactionNumber = 0,
                EndOfSalesNumber = 0,
                LastReceiptNumber = 0
            };

            string selectedFilePath = String.Empty;

            var categoryData = new List<Tuple<string, int, decimal>>();

            //Select data based on current or end of day flag
            selectedFilePath = endDayFlag ? posData.TransactionMasterDataFilePath : posData.TransactionsDataFilePath;

            using (var parser = new GenericParserAdapter(selectedFilePath))
            {
                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                parser.SkipStartingDataRows = 0;
                parser.SkipEmptyRows = true;
                parser.MaxBufferSize = 4096;
                parser.MaxRows = 8000;

                data = parser.GetDataTable();
            }

            var categories = CategoryCatalog.GetList(posData.Catalog);

            //Add points category
            categories.Add("Puntos");

            //Get each category
            foreach (var category in categories)
            {
                var amount = 0M;
                var itemsNumber = 0;

                for (var index = 0; index < data.Rows.Count; index++)
                {
                    var row = data.Rows[index];
                    //Separate categories
                    if (row["CategoriaProducto"].ToString() == category)
                    {
                        if ((row["TipoVenta"].ToString() == "Interno" && intFlag))
                        {
                            amount += decimal.Parse(row["TotalVendido"].ToString());
                        }
                        else if (row["TipoVenta"].ToString() != "Interno")
                        {
                            amount += decimal.Parse(row["TotalVendido"].ToString());
                        }

                        if (category != "Puntos")
                            itemsNumber += int.Parse(row["UnidadesVendidas"].ToString());

                        //Get payment method
                        if (row["TipoVenta"].ToString() != "DevolucionEfectivo" && row["TipoVenta"].ToString() != "DevolucionTarjeta")
                        {
                            switch (row["MetodoPago"].ToString())
                            {
                                case "Cash":
                                case "Efectivo":
                                    if (row["TipoVenta"].ToString() == "Regular")
                                    {
                                        transactionData.CashTotal += decimal.Parse(row["TotalVendido"].ToString());
                                    }
                                    else if (row["TipoVenta"].ToString() == "Interno" && intFlag)
                                    {
                                        transactionData.CashTotal += decimal.Parse(row["TotalVendido"].ToString());
                                    }
                                    break;
                                case "Card":
                                case "Tarjeta":
                                    transactionData.CardTotal += decimal.Parse(row["TotalVendido"].ToString());
                                    break;
                                case "Check":
                                case "Cheque":
                                    transactionData.CheckTotal += decimal.Parse(row["TotalVendido"].ToString());
                                    break;
                                case "BankTransfer":
                                case "Transferencia":
                                    transactionData.BankTotal += decimal.Parse(row["TotalVendido"].ToString());
                                    break;
                                case "Other":
                                case "Otro":
                                    transactionData.OtherTotal += decimal.Parse(row["TotalVendido"].ToString());
                                    break;
                                case "Partial":
                                case "Parcial":
                                    //add ticket number to list so it can be searched later
                                    if (!ticketsList.Contains(Int32.Parse(row["NumeroTicket"].ToString())))
                                    {
                                        ticketsList.Add(Int32.Parse(row["NumeroTicket"].ToString()));
                                    }
                                    tempPartialTotalMxn += decimal.Parse(row["TotalVendido"].ToString());
                                    break;
                                default:
                                    transactionData.OtherTotal += decimal.Parse(row["TotalVendido"].ToString());
                                    break;
                            }
                        }

                        if (row["Descripcion"].ToString() == "Puntos Descuento")
                        {
                            transactionData.PointsTotal += double.Parse(row["TotalVendido"].ToString()) * -1;
                        }

                        if (row["TipoVenta"].ToString() == "DevolucionEfectivo")
                        {
                            transactionData.ReturnsCash += decimal.Parse(row["TotalVendido"].ToString()) * -1;
                            transactionData.TotalReturnItems -= Int32.Parse(row["UnidadesVendidas"].ToString());
                        }

                        if (row["TipoVenta"].ToString() == "DevolucionTarjeta")
                        {
                            transactionData.ReturnsCard += decimal.Parse(row["TotalVendido"].ToString()) * -1;
                            transactionData.TotalReturnItems -= Int32.Parse(row["UnidadesVendidas"].ToString());
                        }
                    }
                }

                categoryData.Add(new Tuple<string, int, decimal>(category.ToString(), itemsNumber, amount));
                transactionData.TotalAmountSold += amount;
                transactionData.TotalItemsSold += itemsNumber;
            }

            //Subtract returns
            transactionData.TotalAmountSold = transactionData.TotalAmountSold + transactionData.ReturnsCash + transactionData.ReturnsCard;

            //Get first and last receipt number
            if (data.Rows.Count > 1)
            {
                var firstRow = data.Rows[0];
                var lastRow = data.Rows[data.Rows.Count - 1];
                transactionData.FirstReceiptNumber = Int32.Parse(firstRow["NumeroTicket"].ToString());
                transactionData.LastReceiptNumber = Int32.Parse(lastRow["NumeroTicket"].ToString());
                transactionData.LastTransactionNumber = Int32.Parse(lastRow["NumeroTransaccion"].ToString());
            }
            else if (data.Rows.Count == 1)
            {
                var firstRow = data.Rows[0];
                var lastRow = data.Rows[0];
                transactionData.FirstReceiptNumber = Int32.Parse(firstRow["NumeroTicket"].ToString());
                transactionData.LastReceiptNumber = Int32.Parse(lastRow["NumeroTicket"].ToString());
                transactionData.LastTransactionNumber = Int32.Parse(lastRow["NumeroTransaccion"].ToString());
            }
            transactionData.SalesInfoPerCategory = categoryData;

            //Search for partial payment tickets in payments db
            decimal cashMxn = 0, cashUsd = 0, cardMxn = 0, transferMxn = 0, checkMxn = 0, otherMxn = 0, totalChangeMxn = 0, totalSoldMxn = 0;
            if (ticketsList.Count > 0)
            {
                GetSalePaymentTotalPerType(endDayFlag, ticketsList, out cashMxn, out cashUsd, out cardMxn, out transferMxn, out checkMxn,
                    out otherMxn, out totalChangeMxn, out totalSoldMxn);
            }

            transactionData.CashTotal += (cashMxn + Math.Round(cashUsd * posData.ExchangeRate, 2) - totalChangeMxn);
            transactionData.CardTotal += cardMxn;
            transactionData.CheckTotal += checkMxn;
            transactionData.BankTotal += transferMxn;
            transactionData.OtherTotal += otherMxn;

            return categoryData;
        }

        /// <summary>
        /// Gets the total payment method per type for partial payments
        /// </summary>
        /// <param name="ticketNumbers"></param>
        /// <param name="cashMxn"></param>
        /// <param name="cashUsd"></param>
        /// <param name="cardMxn"></param>
        /// <param name="transferMxn"></param>
        /// <param name="checkMxn"></param>
        /// <param name="otherMxn"></param>
        /// <param name="totalChangeMxn"></param>
        /// <param name="totalSoldMxn"></param>
        private static void GetSalePaymentTotalPerType(TransactionType transactionType, List<int> ticketNumbers, out decimal cashMxn, out decimal cashUsd, out decimal cardMxn,
            out decimal transferMxn, out decimal checkMxn, out decimal otherMxn, out decimal totalChangeMxn, out decimal totalSoldMxn)
        {
            cashMxn = 0;
            cashUsd = 0;
            cardMxn = 0;
            transferMxn = 0;
            checkMxn = 0;
            otherMxn = 0;
            totalChangeMxn = 0;
            totalSoldMxn = 0;

            System.Data.DataTable data;
            data = transactionType == TransactionType.Regular ? Utilities.LoadCsvToDataTable(Constants.DataFolderPath + Constants.TransactionsPaymentsXFileName) : 
                Utilities.LoadCsvToDataTable(Constants.DataFolderPath + Constants.TransactionsPaymentsZFileName);

            foreach (var ticketNumber in ticketNumbers)
            {
                for (var index = 0; index < data.Rows.Count; index++)
                {
                    var row = data.Rows[index];
                    if (Int32.Parse(row["NumeroTicket"].ToString()) != ticketNumber) continue;

                    if (row["TipoVenta"].ToString() == "DevolucionEfectivo" &&
                        row["TipoVenta"].ToString() == "DevolucionTarjeta" &&
                        row["TipoVenta"].ToString() == "Remover") continue;

                    cashMxn += decimal.Parse(row["EfectivoMXN"].ToString());
                    cashUsd += decimal.Parse(row["EfectivoUSD"].ToString());
                    cardMxn += decimal.Parse(row["TarjetaMXN"].ToString());
                    checkMxn += decimal.Parse(row["ChequeMXN"].ToString());
                    transferMxn += decimal.Parse(row["TransferenciaMXN"].ToString());
                    otherMxn += decimal.Parse(row["OtroMXN"].ToString());
                    totalChangeMxn += decimal.Parse(row["CambioMXN"].ToString());
                    totalSoldMxn += decimal.Parse(row["TotalVendidoMXN"].ToString());
                    break;
                }
            }
        }

        private static void GetSalePaymentTotalPerType(bool endDayFlag, List<int> ticketNumbers, out decimal cashMxn, out decimal cashUsd, out decimal cardMxn,
            out decimal transferMxn, out decimal checkMxn, out decimal otherMxn, out decimal totalChangeMxn, out decimal totalSoldMxn)
        {
            cashMxn = 0;
            cashUsd = 0;
            cardMxn = 0;
            transferMxn = 0;
            checkMxn = 0;
            otherMxn = 0;
            totalChangeMxn = 0;
            totalSoldMxn = 0;

            System.Data.DataTable data;
            data = endDayFlag ? Utilities.LoadCsvToDataTable(Constants.DataFolderPath + Constants.TransactionsPaymentsZFileName) :
                Utilities.LoadCsvToDataTable(Constants.DataFolderPath + Constants.TransactionsPaymentsXFileName);

            foreach (var ticketNumber in ticketNumbers)
            {
                for (var index = 0; index < data.Rows.Count; index++)
                {
                    var row = data.Rows[index];
                    if (Int32.Parse(row["NumeroTicket"].ToString()) != ticketNumber) continue;

                    if (row["TipoVenta"].ToString() == "DevolucionEfectivo" &&
                        row["TipoVenta"].ToString() == "DevolucionTarjeta" &&
                        row["TipoVenta"].ToString() == "Remover") continue;

                    cashMxn += decimal.Parse(row["EfectivoMXN"].ToString());
                    cashUsd += decimal.Parse(row["EfectivoUSD"].ToString());
                    cardMxn += decimal.Parse(row["TarjetaMXN"].ToString());
                    checkMxn += decimal.Parse(row["ChequeMXN"].ToString());
                    transferMxn += decimal.Parse(row["TransferenciaMXN"].ToString());
                    otherMxn += decimal.Parse(row["OtroMXN"].ToString());
                    totalChangeMxn += decimal.Parse(row["CambioMXN"].ToString());
                    totalSoldMxn += decimal.Parse(row["TotalVendidoMXN"].ToString());
                    break;
                }
            }
        }

        public static void RecordEndOfDaySalesTransaction(string filePath, int endOfDayReceiptNumber, int firstReceipt, int lastReceipt,
            int totalUnitsSold, double totalPointsUsed, decimal totalCashSold, decimal totalCardSold, decimal totalCheckSold,
            decimal totalBankSold, decimal totalOthersSold, decimal totalAmountSold, decimal totalReturnCash, decimal totalReturnCard,
            decimal exchangeRate, string date)
        {
            var data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", endOfDayReceiptNumber, firstReceipt,
                           lastReceipt, totalUnitsSold, totalPointsUsed, totalCashSold, totalCardSold, totalCheckSold, totalBankSold,
                           totalOthersSold, totalAmountSold, totalReturnCash, totalReturnCard, exchangeRate, date) + Environment.NewLine;
            //Append to daily receipt
            File.AppendAllText(filePath, data);
        }

        public static void RecordPaymentTransaction(string filePath, int receiptNumber, string user, string customer, string date, decimal exchangeRate, 
            string fiscalReceipt, decimal totalSold, CurrencyTypeEnum currencySold, TransactionType transactionType, decimal cashMxn, decimal cashUsd,
            decimal cardMxn, decimal checkMxn, decimal transferMxn, decimal otherMxn, decimal changeDueMxn, decimal totalSoldMxn)
        {
            var data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}", receiptNumber, user, customer, date, exchangeRate, 
                           fiscalReceipt, totalSold.ToString(), currencySold.ToString(), transactionType.ToString(), cashMxn.ToString(), cashUsd.ToString(),
                           cardMxn, checkMxn, transferMxn, otherMxn, changeDueMxn, totalSoldMxn) + Environment.NewLine;
            //Append to payments file
            File.AppendAllText(filePath, data);
        }

        #endregion
    }
}
