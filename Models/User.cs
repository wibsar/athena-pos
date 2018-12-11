using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Athena
{
    public class User : DataBase , IBasicEntityInfo
    {
        #region Fields
        private UserAccessLevelEnum _rights;
        private string _userName;
        private string _password;
        private DateTime _lastLogin;
        private string _dbPath;

        private string _name;
        private int _id;
        private string _address;
        private string _email;
        private string _phone;
        private DateTime _registrationDate;
        #endregion

        #region Properties

        public UserAccessLevelEnum Rights
        {
            get { return _rights; }
            set
            {
                _rights = value;
            }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public DateTime LastLogin
        {
            get { return _lastLogin; }
            set { _lastLogin = value; }
        }

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

        public string PasswordVerification { get; set; }
        public string AccessLevel
        {
            get
            {
                return _rights.ToString();
            }
            //set
            //{
            //    if (value == "Administrador")
            //    {
            //        Rights = UserAccessLevelEnum.Admin;
            //    }
            //    else if (value == "Basico")
            //    {
            //        Rights = UserAccessLevelEnum.Basic;
            //    }
            //    else if (value == "Avanzado")
            //    {
            //        Rights = UserAccessLevelEnum.Advanced;
            //    }
            //    else
            //    {
            //        Rights = UserAccessLevelEnum.Unknown;
            //    }
            //}
        }

        public string Name { get => _name; set => _name = value; }
        public int Id { get => _id; set => _id = value; }
        public string Address { get => _address; set => _address = value; }
        public string Email { get => _email; set => _email = value; }
        public string Phone { get => _phone; set => _phone = value; }
        public DateTime RegistrationDate { get => _registrationDate; set => _registrationDate = value; }

        #endregion

        #region Constructors

        public User(string dbPath) : base(dbPath)
        {
            //TODO: Check if path exists
            DbPath = dbPath;
            FilePath = dbPath;
            LoadCsvToDataTable();
        }

        public User(string dbPath, string name, string email, string phone, int id, UserAccessLevelEnum rights,
            string username, string password, string address = "") : base(dbPath)
        {
            //TODO: Check if path exists
            Name = name;
            Email = email;
            Phone = phone;
            Id = id;
            Rights = rights;
            UserName = username;
            Password = password;
            Address = address;
            DbPath = dbPath;
        }

        #endregion

        #region Methods

        public static void RegisterUser(string filePath, string name, string email, string phone, string id, 
            UserAccessLevelEnum rights, string userName, string password)
        {
            //TODO: Check if username already exists
            //TODO: Implement feature
            string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", id, name,
                email, phone, DateTime.Now.ToString(), rights.ToString(), userName, password, DateTime.Now.ToString())
                + Environment.NewLine;

            File.AppendAllText(filePath, data);
        }

        public void Register()
        {
            string data = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", Id, Name,
                Email, Phone, DateTime.Now.ToString(), Rights, UserName, Password, DateTime.Now.ToString())
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

        public void FullUpdate(string name, string email, string phone, string address, string id,
            UserAccessLevelEnum rights, string userName, string password)
        {
            //TODO: Check if exists, and update if valid
        }

        public User GetByUserName(string userName)
        {
            var allFields = base.DataTable.AsEnumerable();
            foreach (var row in allFields)
            {
                var user = new User(base.FilePath)
                {
                    Id = Int32.Parse(row["Id"].ToString()),
                    Name = row["Nombre"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Telefono"].ToString(),
                    RegistrationDate = Convert.ToDateTime(row["FechaRegistro"].ToString()),
                    UserName = row["Usuario"].ToString(),
                    Password = row["Password"].ToString(),
                    LastLogin = Convert.ToDateTime(row["UltimaSession"].ToString())
                };

                if (row["Usuario"].ToString() == userName)
                return user;
            }
            return new User(base.FilePath);
        }

        public User GetByID()
        {
            //Look for user by username
            return new User("a");
        }

        public User GetByName()
        {
            //Look for user by username
            return new User("a");
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

        public List<User> Search(string searchInput)
        {
            var users = new List<User>();

            //Return empty list if invalid inputs are entered for the search
            if (string.IsNullOrWhiteSpace(searchInput) || searchInput == "x")
                return users;

            if (searchInput == "*")
            {
                var allFields = base.DataTable.AsEnumerable();
                foreach (var row in allFields)
                {
                    var user = new User(base.FilePath)
                    {
                        Id = Int32.Parse(row["Id"].ToString()),
                        Name = row["Nombre"].ToString(),
                        Email = row["Email"].ToString(),
                        Phone = row["Telefono"].ToString(),
                        RegistrationDate = Convert.ToDateTime(row["FechaRegistro"].ToString()),
                        UserName = row["Usuario"].ToString(),
                        Password = row["Password"].ToString(),
                        LastLogin = Convert.ToDateTime(row["UltimaSession"].ToString())
                    };

                    user.Rights = (UserAccessLevelEnum)Enum.Parse(typeof(UserAccessLevelEnum), row["NivelAcceso"].ToString(), true);

                    users.Add(user);
                }
                return users;
            }

            return users;
        }


        /// <summary>
        /// Update product in the datatable
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UpdateUserToTable(User user)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row["Id"].ToString() == user.Id.ToString())
                {
                    row["Nombre"] = user.Name;
                    row["Email"] = user.Email;
                    row["Telefono"] = user.Phone;
                    row["FechaRegistro"] = user.RegistrationDate.ToString("d");
                    row["Usuario"] = user.UserName;
                    row["Password"] = user.Password;
                    row["UltimaSession"] = user.LastLogin.ToString("d");
                    row["NivelAcceso"] = user.Rights;
                }
            }

            return true;
        }

        /// <summary>
        /// Add new product to data table
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public bool AddUserToTable(User user)
        {
            DataTable.Rows.Add();
            var row = DataTable.Rows[DataTable.Rows.Count - 1];
            row["Id"] = user.GetLastItemNumber() + 1;
            row["Nombre"] = user.Name;
            row["Email"] = user.Email;
            row["Telefono"] = user.Phone;
            row["FechaRegistro"] = user.RegistrationDate;
            row["Usuario"] = user.UserName;
            row["Password"] = user.Password;
            row["UltimaSession"] = user.LastLogin;
            row["NivelAcceso"] = user.Rights;

            return true;
        }

        public static implicit operator User(string v)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
