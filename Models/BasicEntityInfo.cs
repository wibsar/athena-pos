using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena
{
    public class BasicEntityInfo
    {
        #region Fields

        private string _name;
        private int _id;
        private string _address;
        private string _email;
        private string _phone;
        private DateTime _registrationDate;

        #endregion

        #region Properties

        public string Name  
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }
        public int Id   
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }
        public DateTime RegistrationDate
        {
            get { return _registrationDate; }
            set { _registrationDate = value; }
        }

        #endregion

        #region Constructors

        public BasicEntityInfo()
        {

        }

        #endregion

    }
}
