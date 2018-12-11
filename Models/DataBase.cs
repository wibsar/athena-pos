using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericParsing;
using System.Data;
using System.IO;

namespace Athena
{
    public class DataBase
    {
        #region Fields
        private string _filePath;
        private DataTable _dataTable;
        #endregion

        #region Properties
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }
        public DataTable DataTable
        {
            get
            {
                return _dataTable;
            }
            set
            {
                _dataTable = value;
            }
        }
        #endregion

        public DataBase(string filePath)
        {
            FilePath = filePath;
        }

        #region Instance Methods
        /// <summary>
        /// Load CSV database into a datatable object
        /// </summary>
        public void LoadCsvToDataTable()
        {
            using (var parser = new GenericParserAdapter(FilePath))
            {
                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                parser.SkipStartingDataRows = 0;
                parser.SkipEmptyRows = true;
                parser.MaxBufferSize = 4096;
                parser.MaxRows = 8000;

                DataTable = parser.GetDataTable();
            }
        }

        /// <summary>
        /// Save datatable object in a CSV file
        /// </summary>
        public void SaveDataTableToCsv()
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = DataTable.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in DataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }
            File.WriteAllText(FilePath, sb.ToString());
        }

        /// <summary>
        /// Search for a specific item in a specific column and return data found
        /// </summary>
        /// <param name="inputSearch"></param>
        /// <param name="searchableColumn"></param>
        /// <returns></returns>
        public string QueryDataTable(string inputSearch, string searchableColumn)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row[searchableColumn].ToString() == inputSearch)
                {
                    return row[searchableColumn].ToString();
                }
            }
            return string.Format("No se encontro {0} en {1}", inputSearch, searchableColumn);
        }

        /// <summary>
        /// Update an item based on the input search in a specific column
        /// </summary>
        /// <param name="inputSearch"></param>
        /// <param name="columnName"></param>
        /// <param name="newData"></param>
        public void UpdateDataFieldInDataTable(string inputSearch, string columnName, string newData)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row[columnName].ToString() == inputSearch)
                {
                    row[columnName] = newData;
                    return;
                }
            }
        }

        /// <summary>
        /// Removes a full entry in the datatable
        /// </summary>
        /// <param name="inputSearch"></param>
        /// <param name="columnName"></param>
        public void RemoveEntryInDataTable(string inputSearch, string columnName)
        {
            for (int index = 0; index < DataTable.Rows.Count; index++)
            {
                var row = DataTable.Rows[index];
                if (row[columnName].ToString() == inputSearch)
                {
                    DataTable.Rows[index].Delete();
                    return;
                }
            }
        }

        #endregion

    }
}
