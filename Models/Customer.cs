using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Athena
{
    public class Customer : DataBase, IBasicEntityInfo
    {
        #region Fields
        private string _dbPath;

        private string _name;
        private int _id;
        private string _email;
        private string _phone;
        private DateTime _registrationDate;

        private string _rfc;
        private double _pointsAvailable;
        private double _pointsUsed;
        private int _totalVisits;
        private decimal _totalSpent;
        private DateTime _lastVisitDate;

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

        public string Name { get => _name; set => _name = value; }
        public int Id { get => _id; set => _id = value; }
        public string Email { get => _email; set => _email = value; }
        public string Phone { get => _phone; set => _phone = value; }
        public DateTime RegistrationDate { get => _registrationDate; set => _registrationDate = value; }

        public string Rfc { get => _rfc; set => _rfc = value; }
        public double PointsAvailable { get => _pointsAvailable; set => _pointsAvailable = value; }
        public double PointsUsed { get => _pointsUsed; set => _pointsUsed = value; }
        public int TotalVisits { get => _totalVisits; set => _totalVisits = value; }
        public decimal TotalSpent { get => _totalSpent; set => _totalSpent = value; }
        public DateTime LastVisitDate { get => _lastVisitDate; set => _lastVisitDate = value; }

        #endregion

        #region Constructors

        public Customer(string dbPath) : base(dbPath)
        {
            //TODO: Check if path exists
            DbPath = dbPath;
            FilePath = dbPath;
            LoadCsvToDataTable();
        }

        public Customer(string dbPath, string name, string email, string phone, int id, string rfc, double pointsAvailable,
            double pointsUsed, int totalVisits, decimal totalSpent) : base(dbPath)
        {
            //TODO: Check if path exists
            DbPath = dbPath;
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            Rfc = rfc;
            PointsAvailable = pointsAvailable;
            PointsUsed = pointsUsed;
            TotalVisits = totalVisits;
            TotalSpent = totalSpent;
        }

        #endregion

        #region Methods

        public static void RegisterUser(string filePath, string name, string email, string phone, string id,
            string rfc, double pointsAvailable, double pointsUsed, int totalVisits, decimal totalSpent)
        {
            //TODO: Check if username already exists
            //TODO: Implement feature
            string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", id, name,
                email, phone, DateTime.Now.ToString(), rfc, pointsAvailable.ToString(), pointsUsed.ToString(),
                totalVisits.ToString(), totalSpent.ToString(), DateTime.Now.ToString())
                + Environment.NewLine;

            File.AppendAllText(filePath, data);
        }

        public void Register()
        {
            string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", Id, Name,
                 Email, Phone, DateTime.Now.ToString(), Rfc, PointsAvailable.ToString(), PointsUsed.ToString(),
                 TotalVisits.ToString(), TotalSpent.ToString(), DateTime.Now.ToString())
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

        public void FullUpdate(string filePath, string name, string email, string phone, string id,
            string rfc, double pointsAvailable, double pointsUsed, int totalVisits, decimal totalSpent)
        {
            //TODO: Check if exists, and update if valid
        }

        public Customer GetByPhoneNumber()
        {
            //Look for user by username
            return new Customer("a");
        }

        public Customer GetByID()
        {
            //Look for user by username
            return new Customer("a");
        }

        public Customer GetByName()
        {
            //Look for user by username
            return new Customer("a");
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
            if (DataTable.Rows.Count == 0)
                return 0;
            var row = DataTable.Rows[DataTable.Rows.Count - 1];
            return Int32.Parse(row["Id"].ToString());
        }

        public List<Customer> Search(string searchInput)
        {
            var customers = new List<Customer>();

            //Return empty list if invalid inputs are entered for the search
            if (string.IsNullOrWhiteSpace(searchInput) || searchInput == "x")
                return customers;

            if (searchInput == "*")
            {
                var allFields = base.DataTable.AsEnumerable();
                foreach (var row in allFields)
                {
                    var customer = new Customer(base.FilePath)
                    {
                        Id = Int32.Parse(row["Id"].ToString()),
                        Name = row["Nombre"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Telefono"].ToString(),
                        RegistrationDate = Convert.ToDateTime(row["FechaRegistro"].ToString()),
                        Rfc = row["RFC"].ToString(),
                        PointsAvailable = double.Parse(row["PuntosDisponibles"].ToString()),
                        PointsUsed = double.Parse(row["PuntosUsados"].ToString()),
                        TotalVisits = Int32.Parse(row["TotalVisitas"].ToString()),
                        TotalSpent = Decimal.Parse(row["TotalVendido"].ToString()),
                        LastVisitDate = Convert.ToDateTime(row["UltimaVisitaFecha"].ToString())
                    };
                    customers.Add(customer);
                }
                return customers;
            }

            var phoneFilter = base.DataTable.AsEnumerable().Where(r => r.Field<string>("Telefono").ToLower().Contains(searchInput));
            var nameFilter = base.DataTable.AsEnumerable().Where(r => r.Field<string>("Nombre").ToLower().Contains(searchInput));

            foreach (var row in phoneFilter)
            {
                var customer = new Customer(base.FilePath)
                {
                    Id = Int32.Parse(row["Id"].ToString()),
                    Name = row["Nombre"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Telefono"].ToString(),
                    RegistrationDate = Convert.ToDateTime(row["FechaRegistro"].ToString()),
                    Rfc = row["RFC"].ToString(),
                    PointsAvailable = double.Parse(row["PuntosDisponibles"].ToString()),
                    PointsUsed = double.Parse(row["PuntosUsados"].ToString()),
                    TotalVisits = Int32.Parse(row["TotalVisitas"].ToString()),
                    TotalSpent = Decimal.Parse(row["TotalVendido"].ToString()),
                    LastVisitDate = Convert.ToDateTime(row["UltimaVisitaFecha"].ToString())
                };

                customers.Add(customer);
            }

            foreach (var row in nameFilter)
            {
                var customer = new Customer(base.FilePath)
                {
                    Id = Int32.Parse(row["Id"].ToString()),
                    Name = row["Nombre"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Telefono"].ToString(),
                    RegistrationDate = Convert.ToDateTime(row["FechaRegistro"].ToString()),
                    Rfc = row["RFC"].ToString(),
                    PointsAvailable = double.Parse(row["PuntosDisponibles"].ToString()),
                    PointsUsed = double.Parse(row["PuntosUsados"].ToString()),
                    TotalVisits = Int32.Parse(row["TotalVisitas"].ToString()),
                    TotalSpent = Decimal.Parse(row["TotalVendido"].ToString()),
                    LastVisitDate = Convert.ToDateTime(row["UltimaVisitaFecha"].ToString())
                };

                //Add if it does not exist already
                if (!customers.Exists(x => x.Phone == customer.Phone))
                    customers.Add(customer);
            }

            return customers;
        }

        /// <summary>
        /// Update customer in the datatable
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public bool UpdateUserToTable(Customer customer)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row["Id"].ToString() == customer.Id.ToString())
                {
                    row["Nombre"] = customer.Name;
                    row["Email"] = customer.Email;
                    row["Telefono"] = customer.Phone;
                    row["FechaRegistro"] = customer.RegistrationDate;
                    row["RFC"] = customer.Rfc;
                    row["PuntosDisponibles"] = customer.PointsAvailable.ToString();
                    row["PuntosUsados"] = customer.PointsUsed.ToString();
                    row["TotalVisitas"] = customer.TotalVisits.ToString();
                    row["TotalVendido"] = customer.TotalSpent.ToString();
                    row["UltimaVisitaFecha"] = customer.LastVisitDate.ToString();
                }
            }

            return true;
        }

        /// <summary>
        /// Update customer in the datatable
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public bool UpdateUserToTable()
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row["Id"].ToString() == this.Id.ToString())
                {
                    row["Nombre"] = this.Name;
                    row["Email"] = this.Email;
                    row["Telefono"] = this.Phone;
                    row["FechaRegistro"] = this.RegistrationDate;
                    row["RFC"] = this.Rfc;
                    row["PuntosDisponibles"] = this.PointsAvailable.ToString();
                    row["PuntosUsados"] = this.PointsUsed.ToString();
                    row["TotalVisitas"] = this.TotalVisits.ToString();
                    row["TotalVendido"] = this.TotalSpent.ToString();
                    row["UltimaVisitaFecha"] = this.LastVisitDate.ToString();
                }
            }
            return true;
        }


        /// <summary>
        /// Add new product to data table
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public bool AddUserToTable(Customer customer)
        {
            DataTable.Rows.Add();
            var row = DataTable.Rows[DataTable.Rows.Count - 1];
            row["Id"] = customer.GetLastItemNumber() + 1;
            row["Nombre"] = customer.Name;
            row["Email"] = customer.Email;
            row["Telefono"] = customer.Phone;
            row["RFC"] = customer.Rfc;
            row["PuntosDisponibles"] = customer.PointsAvailable.ToString();
            row["PuntosUsados"] = customer.PointsUsed.ToString();
            row["TotalVisitas"] = customer.TotalVisits.ToString();
            row["TotalVendido"] = customer.TotalSpent.ToString();
            row["UltimaVisitaFecha"] = customer.LastVisitDate.ToString();
            return true;
        }
        #endregion
    }
}
