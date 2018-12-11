using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericParsing;
using System.Data;
using System.IO;
using System.Drawing.Printing;

namespace Athena
{
    public class Utilities
    {
        public static IEnumerable<ProductBase> MoveListItemUp(IEnumerable<ProductBase> item, int selectedItemIndex)
        {
            var updatedList = item.ToList();
            var tempItem = updatedList[selectedItemIndex];
            if (selectedItemIndex < updatedList.Count && selectedItemIndex > 0)
            {
                updatedList[selectedItemIndex] = updatedList[selectedItemIndex - 1];
                updatedList[selectedItemIndex - 1] = tempItem;
            }
            return updatedList;
        }

        public static IEnumerable<ProductBase> MoveListItemDown(IEnumerable<ProductBase> item, int selectedItemIndex)
        {
            var updatedList = item.ToList();
            var tempItem = updatedList[selectedItemIndex];
            if (selectedItemIndex < updatedList.Count - 1)
            {
                updatedList[selectedItemIndex] = updatedList[selectedItemIndex + 1];
                updatedList[selectedItemIndex + 1] = tempItem;
            }
            return updatedList;
        }

        public static IEnumerable<ProductBase> DeleteListItem(IEnumerable<ProductBase> item, int selectedItemIndex)
        {
            var updatedList = item.ToList();
            var tempItem = updatedList[selectedItemIndex];
            updatedList.RemoveAt(selectedItemIndex);
            return updatedList;
        }

        public static IEnumerable<ProductBase> AddListItem(IEnumerable<ProductBase> item, ProductBase newItem, int selectedItemIndex)
        {
            var updatedList = item.ToList();
            if(updatedList.Count < 20)
            {
                updatedList.Insert(selectedItemIndex + 1, newItem);
            }
            var tempItem = updatedList[selectedItemIndex];
            updatedList.RemoveAt(selectedItemIndex);
            return updatedList;
        }

        public static IEnumerable<string> MoveListItemUp(IEnumerable<string> item, int selectedItemIndex)
        {
            var updatedList = item.ToList();
            var tempItem = updatedList[selectedItemIndex];
            if (selectedItemIndex < updatedList.Count && selectedItemIndex > 0)
            {
                updatedList[selectedItemIndex] = updatedList[selectedItemIndex - 1];
                updatedList[selectedItemIndex - 1] = tempItem;
            }
            return updatedList;
        }

        public static IEnumerable<string> MoveListItemDown(IEnumerable<string> item, int selectedItemIndex)
        {
            var updatedList = item.ToList();
            var tempItem = updatedList[selectedItemIndex];
            if (selectedItemIndex < updatedList.Count - 1)
            {
                updatedList[selectedItemIndex] = updatedList[selectedItemIndex + 1];
                updatedList[selectedItemIndex + 1] = tempItem;
            }
            return updatedList;
        }

        public static IEnumerable<string> DeleteListItem(IEnumerable<string> item, int selectedItemIndex)
        {
            var updatedList = item.ToList();
            var tempItem = updatedList[selectedItemIndex];
            updatedList.RemoveAt(selectedItemIndex);
            return updatedList;
        }

        public static IEnumerable<string> AddListItem(IEnumerable<string> item, string newItem, int selectedItemIndex)
        {
            var updatedList = item.ToList();
            if (updatedList.Count < 20)
            {
                updatedList.Insert(selectedItemIndex + 1, newItem);
            }
            var tempItem = updatedList[selectedItemIndex];
            updatedList.RemoveAt(selectedItemIndex);
            return updatedList;
        }

        /// <summary>
        /// Load CSV database into a datatable object
        /// </summary>
        public static DataTable LoadCsvToDataTable(string csvFilePath)
        {
            using (var parser = new GenericParserAdapter(csvFilePath))
            {
                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                parser.SkipStartingDataRows = 0;
                parser.SkipEmptyRows = true;
                parser.MaxBufferSize = 4096;
                parser.MaxRows = 8000;

                return parser.GetDataTable();
            }
        }

        /// <summary>
        /// Save datatable object in a CSV file
        /// </summary>
        public static void SaveDataTableToCsv(string csvFilePath, DataTable dataTable)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }
            File.WriteAllText(csvFilePath, sb.ToString());
        }

        #region Printer Utilities

        /// <summary>
        /// Return list of avaialble printers installed in the system
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAvailablePrinters()
        {
            var printers = new List<string>();
            foreach (var printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer.ToString());
            }
            return printers;
        }

        #endregion

        #region File Utilities

        #endregion
    }
}
