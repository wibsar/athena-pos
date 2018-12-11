using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Media.Imaging;

namespace Athena
{
    public class Order : DataBase
    {
        #region Fields
        private string _dbPath;

        private int _id;
        private string _customer;
        private string _title;
        private DateTime _registrationDate;
        private DateTime _dueDate;
        private string _category;
        private string _description;
        private decimal _totalAmount;
        private decimal _totalPrePaid;
        private decimal _totalDue;
        private int _prePaidTicketNumber;
        private int _orderTicketNumber;
        private BitmapImage _image;
        private CurrencyTypeEnum _currency;

        #endregion

        #region Properties

        private string DbPath
        {
            get
            {
                return _dbPath;
            }
            set
            {
                _dbPath = value;
            }
        }

        public string Customer
        {
            get { return _customer; }
            set { _customer = value; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public DateTime RegistrationDate
        {
            get { return _registrationDate; }
            set { _registrationDate = value; }
        }

        public DateTime DueDate
        {
            get { return _dueDate; }
            set { _dueDate = value; }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set { _totalAmount = value; }
        }

        public decimal TotalPrePaid
        {
            get { return _totalPrePaid; }
            set { _totalPrePaid = value; }
        }

        public decimal TotalDue
        {
            get { return _totalDue; }
            set { _totalDue = value; }
        }

        public int PrePaidTicketNumber
        {
            get { return _prePaidTicketNumber; }
            set { _prePaidTicketNumber = value; }
        }

        public int OrderTicketNumber
        {
            get { return _orderTicketNumber; }
            set { _orderTicketNumber = value; }
        }

        public string ImageName { get; set; }

        public BitmapImage Image
        {
            get
            {
                BitmapImage bitmap = new BitmapImage();
                if (ImageName != null)
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Constants.DataFolderPath + Constants.ImagesFolderPath + ImageName);
                    bitmap.EndInit();
                    _image = bitmap;
                }
                return bitmap;
            }
            set
            {
                _image = value;
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

        #endregion

        #region Constructors

        public Order(string dbPath) : base(dbPath)
        {
            //TODO: Check if path exists
            DbPath = dbPath;
            FilePath = dbPath;
            LoadCsvToDataTable();
        }

        public Order(string dbPath, int id, int orderTicketNumber, string customer, string title, DateTime dueDate, string category,
            decimal totalAmount, decimal totalPrePaid, int prePaidTicketNumber, decimal totalDue, CurrencyTypeEnum currencyType, string description, 
            string imageName) : base(dbPath)
        {
            //TODO: Check if path exists
            DbPath = dbPath;
            Id = id;
            OrderTicketNumber = orderTicketNumber;
            Customer = customer;
            Title = title;
            DueDate = dueDate;
            Category = category;
            TotalAmount = totalAmount;
            CurrencyType = currencyType;
            TotalPrePaid = totalPrePaid;
            PrePaidTicketNumber = prePaidTicketNumber;
            TotalDue = totalDue;
            Description = description;
            ImageName = imageName;
        }

        #endregion

        #region Methods

        public static void RegisterOrder(string filePath, int id, int orderTickerNumber, string customer, string title,
            string description, string category, decimal totalAmount, decimal totalPrePaid, int prePaidTicketNumber,
            decimal totalDue, CurrencyTypeEnum currencyType, string imageName, DateTime dueDate)
        {
            //TODO: Check if username already exists
            //TODO: Implement feature
            string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", id, orderTickerNumber.ToString(),
                     customer, title, description, category, totalAmount.ToString(), totalPrePaid.ToString(),
                     prePaidTicketNumber.ToString(), DateTime.Now.ToString(), totalDue.ToString(), currencyType.ToString(), imageName, dueDate.ToString())
                     + Environment.NewLine;

            File.AppendAllText(filePath, data);
        }

        public void Register()
        { 
            string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", Id, OrderTicketNumber,
                     Customer, Title, Description, Category, TotalAmount.ToString(), TotalPrePaid.ToString(),
                     PrePaidTicketNumber.ToString(), DateTime.Now.ToString(), TotalDue.ToString(), CurrencyType.ToString(), ImageName, DueDate.ToString())
                     + Environment.NewLine;

            File.AppendAllText(DbPath, data);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        public void Delete()
        {
            base.RemoveEntryInDataTable(this.Id.ToString(), "Id");
        }

        public void Update(string item, string parameter, string newData)
        {
            base.UpdateDataFieldInDataTable(item, parameter, newData);
        }

        public void Get(string item)
        {

        }

        public int GetLastItemNumber()
        {
            //TODO: Check method for all other classes
            if (DataTable.Rows.Count == 0)
                return 0;
            var row = DataTable.Rows[DataTable.Rows.Count - 1];
            return Int32.Parse(row["Id"].ToString());
        }

        public List<Order> Search(string searchInput)
        {
            var orders = new List<Order>();

            //Return empty list if invalid inputs are entered for the search
            if (string.IsNullOrWhiteSpace(searchInput) || searchInput == "x")
                return orders;

            if (searchInput == "*")
            {
                var allFields = base.DataTable.AsEnumerable();
                foreach (var row in allFields)
                {
                    var order = new Order(base.FilePath)
                    {
                        Id = Int32.Parse(row["Id"].ToString()),
                        OrderTicketNumber = Int32.Parse(row["NumeroFolio"].ToString()),
                        Customer = row["Cliente"].ToString(),
                        Title = row["Titulo"].ToString(),
                        Description = row["Descripcion"].ToString(),
                        Category = row["Categoria"].ToString(),
                        TotalAmount = Decimal.Parse(row["MontoTotal"].ToString()),
                        TotalPrePaid = Decimal.Parse(row["Anticipo"].ToString()),
                        PrePaidTicketNumber = Int32.Parse(row["TicketAnticipo"].ToString()),
                        RegistrationDate = Convert.ToDateTime(row["FechaOrden"].ToString()),
                        TotalDue = Decimal.Parse(row["Saldo"].ToString()),
                        ImageName = row["Imagen"].ToString(),
                        DueDate = Convert.ToDateTime(row["FechaEntrega"].ToString())
                    };
                    if (row["Moneda"].ToString().ToUpper() == "USD")
                        order.CurrencyType = CurrencyTypeEnum.USD;
                    else
                        order.CurrencyType = CurrencyTypeEnum.MXN;

                    orders.Add(order);
                }
                return orders;
            }

            var numberFilter = base.DataTable.AsEnumerable().Where(r => r.Field<string>("NumeroFolio").ToLower().Contains(searchInput));
            var clientFilter = base.DataTable.AsEnumerable().Where(r => r.Field<string>("Cliente").ToLower().Contains(searchInput));

            foreach (var row in numberFilter)
            {
                var order = new Order(base.FilePath)
                {
                    Id = Int32.Parse(row["Id"].ToString()),
                    OrderTicketNumber = Int32.Parse(row["NumeroFolio"].ToString()),
                    Customer = row["Cliente"].ToString(),
                    Title = row["Titulo"].ToString(),
                    Description = row["Descripcion"].ToString(),
                    Category = row["Categoria"].ToString(),
                    TotalAmount = Decimal.Parse(row["MontoTotal"].ToString()),
                    TotalPrePaid = Decimal.Parse(row["Anticipo"].ToString()),
                    PrePaidTicketNumber = Int32.Parse(row["TicketAnticipo"].ToString()),
                    RegistrationDate = Convert.ToDateTime(row["FechaOrden"].ToString()),
                    TotalDue = Decimal.Parse(row["Saldo"].ToString()),
                    ImageName = row["Imagen"].ToString(),
                    DueDate = Convert.ToDateTime(row["FechaEntrega"].ToString())
                };
                if (row["Moneda"].ToString().ToUpper() == "USD")
                    order.CurrencyType = CurrencyTypeEnum.USD;
                else
                    order.CurrencyType = CurrencyTypeEnum.MXN;

                orders.Add(order);
            }

            foreach (var row in clientFilter)
            {
                var order = new Order(base.FilePath)
                {
                    Id = Int32.Parse(row["Id"].ToString()),
                    OrderTicketNumber = Int32.Parse(row["NumeroFolio"].ToString()),
                    Customer = row["Cliente"].ToString(),
                    Title = row["Titulo"].ToString(),
                    Description = row["Descripcion"].ToString(),
                    Category = row["Categoria"].ToString(),
                    TotalAmount = Decimal.Parse(row["MontoTotal"].ToString()),
                    TotalPrePaid = Decimal.Parse(row["Anticipo"].ToString()),
                    PrePaidTicketNumber = Int32.Parse(row["TicketAnticipo"].ToString()),
                    RegistrationDate = Convert.ToDateTime(row["FechaOrden"].ToString()),
                    TotalDue = Decimal.Parse(row["Saldo"].ToString()),
                    ImageName = row["Imagen"].ToString(),
                    DueDate = Convert.ToDateTime(row["FechaEntrega"].ToString())
                };

                if (row["Moneda"].ToString().ToUpper() == "USD")
                    order.CurrencyType = CurrencyTypeEnum.USD;
                else
                    order.CurrencyType = CurrencyTypeEnum.MXN;

                //Add if it does not exist already
                if (!orders.Exists(x => x.OrderTicketNumber == order.OrderTicketNumber))
                    orders.Add(order);
            }

            return orders;
        }

        /// <summary>
        /// Update customer in the datatable
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool UpdateOrderToTable(Order order)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row["Id"].ToString() == order.Id.ToString())
                {

                    row["Id"] = order.Id.ToString();
                    row["NumeroFolio"] = order.OrderTicketNumber.ToString();
                    row["Cliente"] = order.Customer;
                    row["Titulo"] = order.Title;
                    row["Descripcion"] = order.Description;
                    row["Categoria"] = order.Category;
                    row["MontoTotal"] = order.TotalAmount.ToString();
                    row["Moneda"] = order.CurrencyType.ToString();
                    row["Anticipo"] = order.TotalPrePaid.ToString();
                    row["TicketAnticipo"] = order.PrePaidTicketNumber.ToString();
                    row["FechaOrden"] = order.RegistrationDate.ToString();
                    row["Saldo"] = order.TotalDue.ToString();
                    row["Imagen"] = order.ImageName;
                    row["FechaEntrega"] = order.DueDate.ToString();
                }
            }

            return true;
        }

        /// <summary>
        /// Add new product to data table
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool AddOrderToTable(Order order)
        {
            DataTable.Rows.Add();
            var row = DataTable.Rows[DataTable.Rows.Count - 1];
            row["Id"] = order.GetLastItemNumber() + 1;
            row["NumeroFolio"] = order.OrderTicketNumber.ToString();
            row["Cliente"] = order.Customer;
            row["Titulo"] = order.Title;
            row["Descripcion"] = order.Description;
            row["Categoria"] = order.Category;
            row["MontoTotal"] = order.TotalAmount.ToString();
            row["Moneda"] = order.CurrencyType.ToString();
            row["Anticipo"] = order.TotalPrePaid.ToString();
            row["TicketAnticipo"] = order.PrePaidTicketNumber.ToString();
            row["FechaOrden"] = order.RegistrationDate.ToString();
            row["Saldo"] = order.TotalDue.ToString();
            row["Imagen"] = order.ImageName;
            row["FechaEntrega"] = order.DueDate.ToString();
            return true;
        }
        #endregion
    }
}
