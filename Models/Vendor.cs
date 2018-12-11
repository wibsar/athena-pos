using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Athena
{
    public class Vendor : DataBase , IBasicEntityInfo
    {
        #region Fields
        private string _dbPath;

        private string _name;
        private int _id;
        private string _email;
        private string _phone;
        private DateTime _registrationDate;

        private string _rfc;
        private string _businessName;
        private string _bank;
        private string _bankAccount;
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
        public string BusinessName { get => _businessName; set => _businessName = value; }
        public string Bank { get => _bank; set => _bank = value; }
        public string BankAccount { get => _bankAccount; set => _bankAccount = value; }

        #endregion

        #region Constructors

        public Vendor(string dbPath) : base(dbPath)
        {
            //TODO: Check if path exists
            DbPath = dbPath;
            FilePath = dbPath;
            LoadCsvToDataTable();
        }

        public Vendor(string dbPath, string name, string email, string phone, int id, string rfc, string businessName,
            string bank, string bankAccount) : base(dbPath)
        {
            //TODO: Check if path exists
            Name = name;
            Email = email;
            Phone = phone;
            Id = id;
            DbPath = dbPath;
            Rfc = rfc;
            BusinessName = businessName;
            Bank = bank;
            BankAccount = bankAccount;
        }

        #endregion

        #region Methods

        public static void RegisterVendor(string filePath, string name, string email, string phone, string id, string rfc,
            string businessName, string bank, string bankAccount)
        {
            //TODO: Check if username already exists
            //TODO: Implement feature
            string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", id, name,
                email, phone, DateTime.Now.ToString(), rfc, businessName, bank, bankAccount)
                + Environment.NewLine;

            File.AppendAllText(filePath, data);
        }

        public void Register()
        {
            string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", Id, Name,
                Email, Phone, DateTime.Now.ToString(), Rfc, BusinessName, Bank, BankAccount)
                + Environment.NewLine;

            File.AppendAllText(DbPath, data);
        }

        /// <summary>
        /// Delete vendor
        /// </summary>
        public void Delete()
        {
            base.RemoveEntryInDataTable(this.Id.ToString(), "Id");
        }

        public void FullUpdate(string name, string email, string phone, string id, string rfc,
            string businessName, string bank, string bankAccount)
        {
            //TODO: Check if exists, and update if valid
        }

        public Vendor GetByUserName()
        {
            //Look for user by username
            return new Vendor("a");
        }

        public Vendor GetByID()
        {
            //Look for user by username
            return new Vendor("a");
        }

        public Vendor GetByName()
        {
            //Look for user by username
            return new Vendor("a");
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

        public List<Vendor> Search(string searchInput)
        {
            var vendors = new List<Vendor>();

            //Return empty list if invalid inputs are entered for the search
            if (string.IsNullOrWhiteSpace(searchInput) || searchInput == "x")
                return vendors;

            if (searchInput == "*")
            {
                var allFields = base.DataTable.AsEnumerable();
                foreach (var row in allFields)
                {
                    var vendor = new Vendor(base.FilePath)
                    {
                        Id = Int32.Parse(row["Id"].ToString()),
                        Name = row["Nombre"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Telefono"].ToString(),
                        RegistrationDate = Convert.ToDateTime(row["FechaRegistro"].ToString()),
                        Rfc = row["RFC"].ToString(),
                        BusinessName = row["NombreProveedor"].ToString(),
                        Bank = row["Banco"].ToString(),
                        BankAccount = row["CuentaBanco"].ToString(),
                    };
                    vendors.Add(vendor);
                }
                return vendors;
            }

            return vendors;
        }


        /// <summary>
        /// Update vendor in the datatable
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public bool UpdateUserToTable(Vendor vendor)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row["Id"].ToString() == vendor.Id.ToString())
                {
                    row["Nombre"] = vendor.Name;
                    row["Email"] = vendor.Email;
                    row["Telefono"] = vendor.Phone;
                    row["FechaRegistro"] = vendor.RegistrationDate;
                    row["RFC"] = vendor.Rfc;
                    row["NombreProveedor"] = vendor.BusinessName;
                    row["Banco"] = vendor.Bank;
                    row["CuentaBanco"] = vendor.BankAccount;
                }
            }
            return true;
        }

        /// <summary>
        /// Add new vendor to data table
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public bool AddUserToTable(Vendor vendor)
        {
            DataTable.Rows.Add();
            var row = DataTable.Rows[DataTable.Rows.Count - 1];
            row["Id"] = vendor.GetLastItemNumber() + 1;
            row["Nombre"] = vendor.Name;
            row["Email"] = vendor.Email;
            row["Telefono"] = vendor.Phone;
            row["FechaRegistro"] = vendor.RegistrationDate;
            row["RFC"] = vendor.Rfc;
            row["NombreProveedor"] = vendor.BusinessName;
            row["Banco"] = vendor.Bank;
            row["CuentaBanco"] = vendor.BankAccount;
            return true;
        }
        #endregion
    }
}
