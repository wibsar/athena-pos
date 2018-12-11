using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Athena
{
    public class Expense : DataBase
    {
        #region Fields
        private int _id;
        private string _vendor;
        private string _ticketNumber;
        private CurrencyTypeEnum _currency;
        private decimal _amount;
        private string _description;
        private string _expenseCategory;
        private PaymentTypeEnum _paymentType;
        private string _expensesXFilePath;
        private string _expensesZFilePath;
        private string _expensesHistoryFilePath;
        private string _user;
        private DateTime _date;
        #endregion

        #region Constructors

        public Expense(string expensesXFilePath, string expensesZFilePath, string expensesHistoryFilePath) : base(expensesXFilePath)
        {
            //TODO: Check if path exists
            ExpensesXFilePath = expensesXFilePath;
            ExpensesZFilePath = expensesZFilePath;
            ExpensesHistoryFilePath = expensesHistoryFilePath;
            LoadCsvToDataTable();
        }

        public Expense(string expensesXFilePath, string expensesZFilePath, string expensesHistoryFilePath, int id, string user, string vendor, string description, decimal amount, 
            CurrencyTypeEnum currencyType, PaymentTypeEnum paymentType, string expenseCategory = "General") : base(expensesXFilePath)
        {
            Id = id;
            ExpensesXFilePath = expensesXFilePath;
            ExpensesZFilePath = expensesZFilePath;
            ExpensesHistoryFilePath = expensesHistoryFilePath;
            User = user;
            Vendor = vendor;
            Description = description;
            Amount = amount;
            CurrencyType = currencyType;
            PaymentType = paymentType;
            ExpenseCategory = expenseCategory;
        }

        #endregion

        #region Properties

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public string Vendor
        {
            get
            {
                return _vendor;
            }
            set
            {
                _vendor = value;
            }
        }
        public string TicketNumber
        {
            get
            {
                return _ticketNumber;
            }
            set
            {
                _ticketNumber = value;
            }
        }
        public CurrencyTypeEnum CurrencyType
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
            }
        }
        public string CurrencyTypeString
        {
            get
            {
                return _currency.ToString();
            }
        }
        public decimal Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
        public PaymentTypeEnum PaymentType
        {
            get
            {
                return _paymentType;
            }
            set
            {
                _paymentType = value;
            }
        }
        public string PaymentTypeString
        {
            get { return _paymentType.ToString(); }
        }
        public string ExpensesXFilePath
        {
            get
            {
                return _expensesXFilePath;
            }
            set
            {
                _expensesXFilePath = value;
            }
        }
        public string ExpensesZFilePath
        {
            get
            {
                return _expensesZFilePath;
            }
            set
            {
                _expensesZFilePath = value;
            }
        }
        public string ExpensesHistoryFilePath
        {
            get
            {
                return _expensesHistoryFilePath;
            }
            set
            {
                _expensesHistoryFilePath = value;
            }
        }
        public string User
        {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
            }
        }
        public string ExpenseCategory
        {
            get
            {
                return _expenseCategory;
            }
            set
            {
                _expenseCategory = value;
            }
        }
        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
            }
        }

        #endregion

        #region Instance Methods

        public void Register()
        {
            var data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", Id.ToString(), User, Vendor, TicketNumber, Description, Amount.ToString(), 
                CurrencyType.ToString(), PaymentType.ToString(), ExpenseCategory, DateTime.Now) + Environment.NewLine;
            
            try
            {
                File.AppendAllText(ExpensesXFilePath, data);
                File.AppendAllText(ExpensesZFilePath, data);
                File.AppendAllText(ExpensesHistoryFilePath, data);
            }
            catch (Exception e)
            {
                //Report Error in log file
            }
        }

        public List<Expense> Search(string searchInput)
        {
            var expenses = new List<Expense>();

            //Return empty list if invalid inputs are entered for the search
            if (string.IsNullOrWhiteSpace(searchInput) || searchInput == "x")
                return expenses;

            if (searchInput == "*")
            {
                var allFields = base.DataTable.AsEnumerable();
                foreach (var row in allFields)
                {
                    var expense = new Expense(ExpensesXFilePath, ExpensesZFilePath, ExpensesHistoryFilePath)
                    {
                        Id = Int32.Parse(row["Id"].ToString()),
                        User = row["Usuario"].ToString(),
                        Vendor = row["Proveedor"].ToString(),
                        TicketNumber = row["NumeroTicket"].ToString(),
                        Description = row["Descripcion"].ToString(),
                        Amount = Decimal.Parse(row["Monto"].ToString()),
                        ExpenseCategory = row["CategoriaGasto"].ToString(),
                        Date = Convert.ToDateTime(row["Fecha"].ToString())

                    };
                    if (row["Moneda"].ToString().ToUpper() == "USD")
                        expense.CurrencyType = CurrencyTypeEnum.USD;
                    else
                        expense.CurrencyType = CurrencyTypeEnum.MXN;

                    expense.PaymentType = (PaymentTypeEnum)Enum.Parse(typeof(PaymentTypeEnum),row["MetodoPago"].ToString(), true);

                    expenses.Add(expense);
                }
                return expenses;
            }

            var searchField1 = DataTable.AsEnumerable().Where(r => r.Field<string>("CategoriaGasto").ToLower().Contains(searchInput));
            var searchField2 = DataTable.AsEnumerable().Where(r => r.Field<string>("Proveedor").ToLower().Contains(searchInput));

            foreach (var row in searchField1)
            {
                var expense = new Expense(ExpensesXFilePath, ExpensesZFilePath, ExpensesHistoryFilePath)
                {
                    Id = Int32.Parse(row["Usuario"].ToString()),
                    User = row["Usuario"].ToString(),
                    Vendor = row["Proveedor"].ToString(),
                    TicketNumber = row["NumeroTicket"].ToString(),
                    Description = row["Descripcion"].ToString(),
                    Amount = Decimal.Parse(row["Monto"].ToString()),
                    ExpenseCategory = row["CategoriaGasto"].ToString(),
                    Date = Convert.ToDateTime(row["Fecha"].ToString())

                };
                if (row["Moneda"].ToString().ToUpper() == "USD")
                    expense.CurrencyType = CurrencyTypeEnum.USD;
                else
                    expense.CurrencyType = CurrencyTypeEnum.MXN;

                expense.PaymentType = (PaymentTypeEnum)Enum.Parse(typeof(PaymentTypeEnum), row["MetodoPago"].ToString(), true);

                expenses.Add(expense);
            }

            foreach (var row in searchField2)
            {
                var expense = new Expense(ExpensesXFilePath, ExpensesZFilePath, ExpensesHistoryFilePath)
                {
                    Id = Int32.Parse(row["Usuario"].ToString()),
                    User = row["Usuario"].ToString(),
                    Vendor = row["Proveedor"].ToString(),
                    TicketNumber = row["NumeroTicket"].ToString(),
                    Description = row["Descripcion"].ToString(),
                    Amount = Decimal.Parse(row["Monto"].ToString()),
                    ExpenseCategory = row["CategoriaGasto"].ToString(),
                    Date = Convert.ToDateTime(row["Fecha"].ToString())

                };
                if (row["Moneda"].ToString().ToUpper() == "USD")
                    expense.CurrencyType = CurrencyTypeEnum.USD;
                else
                    expense.CurrencyType = CurrencyTypeEnum.MXN;

                expense.PaymentType = (PaymentTypeEnum)Enum.Parse(typeof(PaymentTypeEnum), row["MetodoPago"].ToString(), true);

                //Add if it does not exist already
                if (!expenses.Exists(x => x.Id == expense.Id))
                    expenses.Add(expense);
            }

            return expenses;
        }

        public bool AddNewItemToTable(object product)
        {
            throw new NotImplementedException();
        }

        public object GetTotal(string searchInput)
        {
            throw new NotImplementedException();
        }

        public bool GetTotal(out decimal expensesMxn, out decimal expensesUsd, out decimal expensesCashMxn, out decimal expensesCashUsd)
        {
            expensesMxn = 0;
            expensesUsd = 0;
            expensesCashMxn = 0;
            expensesCashUsd = 0;

            var allFields = base.DataTable.AsEnumerable();
            foreach (var row in allFields)
            {
                if (row["Moneda"].ToString() == "USD")
                {
                    expensesUsd += Convert.ToDecimal(row["Monto"].ToString());
                    if (row["MetodoPago"].ToString() == "Efectivo")
                    {
                        expensesCashUsd += Convert.ToDecimal(row["Monto"].ToString());
                    }
                }
                else
                {
                    expensesMxn += Convert.ToDecimal(row["Monto"].ToString());
                    if (row["MetodoPago"].ToString() == "Efectivo")
                    {
                        expensesCashMxn += Convert.ToDecimal(row["Monto"].ToString());
                    }
                }
            }
            return true;
        }

        public int GetLastItemNumber()
        {
            if (DataTable.Rows.Count == 0)
                return 0;
            var row = DataTable.Rows[DataTable.Rows.Count - 1];
            return Int32.Parse(row["Id"].ToString());
        }

        /// <summary>
        /// Update expense in the datatable
        /// </summary>
        /// <param name="expense"></param>
        /// <returns></returns>
        public bool UpdateExpenseToTable(Expense expense)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row["Id"].ToString() == expense.Id.ToString())
                {
                    row["Usuario"] = expense.User;
                    row["Proveedor"] = expense.Vendor;
                    row["NumeroTicket"] = expense.TicketNumber;
                    row["Descripcion"] = expense.Description;
                    row["Monto"] = expense.Amount.ToString();
                    row["Moneda"] = expense.CurrencyType.ToString();
                    row["MetodoPago"] = expense.PaymentType.ToString();
                    row["CategoriaGasto"] = expense.ExpenseCategory.ToString();
                    row["Fecha"] = expense.Date.ToString();
                }
            }

            return true;
        }

        /// <summary>
        /// Add new expense to data table
        /// </summary>
        /// <param name="expense"></param>
        /// <returns></returns>
        public bool AddExpenseToTable(Expense expense)
        {
            DataTable.Rows.Add();
            var row = DataTable.Rows[DataTable.Rows.Count - 1];
            row["Id"] = expense.GetLastItemNumber() + 1;
            row["Usuario"] = expense.User;
            row["Proveedor"] = expense.Vendor;
            row["NumeroTicket"] = expense.TicketNumber;
            row["Descripcion"] = expense.Description;
            row["Monto"] = expense.Amount.ToString();
            row["Moneda"] = expense.CurrencyType.ToString();
            row["MetodoPago"] = expense.PaymentType.ToString();
            row["CategoriaGasto"] = expense.ExpenseCategory.ToString();
            row["Fecha"] = expense.Date.ToString();
            return true;
        }

        /// <summary>
        /// Delete expense
        /// </summary>
        public void Delete()
        {
            base.RemoveEntryInDataTable(this.Id.ToString(), "Id");
        }

        /// <summary>
        /// Backup expenses file
        /// </summary>
        /// <param name="expensesFilePath"></param>
        /// <param name="endDayFlag"></param>
        public static void BackUpExpensesFile(string expensesFilePath, bool endDayFlag)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            var currentTime = DateTime.Now;
            var transFileType = endDayFlag ? "Z" : "X";

            string expensesFileBackUpCopyName = Constants.DataFolderPath + Constants.TransactionsBackupFolderPath + "Gastos" + transFileType + "_" + 
                                                   currentTime.Day.ToString("00") + currentTime.Month.ToString("00") + currentTime.Year.ToString("0000") +
                                                   currentTime.Hour.ToString("00") + currentTime.Minute.ToString("00") + currentTime.Second.ToString("00") + ".csv";

            File.Copy(expensesFilePath, expensesFileBackUpCopyName);
        }


        /// <summary>
        /// Clears expenses file
        /// </summary>
        /// <param name="expensesFilePath"></param>
        public static void ClearExpensesFile(string expensesFilePath)
        {
            File.Copy(Constants.DataFolderPath + Constants.TransactionsBackupFolderPath + Constants.ExpenseBlankFileName,
                expensesFilePath, true);
        }
        #endregion
    }
}
