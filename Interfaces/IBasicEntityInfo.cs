using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena
{
    public interface IBasicEntityInfo
    {
        #region Properties
        string Name { get; set; }
        int Id { get; set; }
        string Email { get; set; }
        string Phone { get; set; }
        DateTime RegistrationDate { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Register new entity and save it to database
        /// </summary>
        void Register();
        /// <summary>
        /// Search for item in database, update, and save the item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parameter"></param>
        /// <param name="newData"></param>
        void Update(string item, string parameter, string newData);
        /// <summary>
        /// Remove item from database
        /// </summary>
        void Delete();
        /// <summary>
        /// Get item frmo database
        /// </summary>
        void Get(string item);
        #endregion
    }
}
