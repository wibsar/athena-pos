using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena
{
    public static class Constants
    {
        //Main data path
        public const string DataFolderPath = @"C:\Projects\seiya-pos\Data\";
        public const string PosDataFileName = "PosData.csv";
        public const string ReceiptBackupFolderPath = @"ReceiptCustomerBackUp\";
        public const string MasterReceiptBackupFolderPath = @"MasterReceiptCustomerBackUp\";

        //Inventory
        public const string InventoryBackupFolderPath = @"InventoryBackUp\";
        public const string InventoryFileName = "Inventario.csv";

        //Transactions Files
        public const string TransactionsBackupFolderPath = @"TransactionBackUp\";
        public const string TransactionsFileName = "Transacciones.csv";
        public const string TransactionsMasterFileName = "TransaccionesMaster.csv";
        public const string TransactionsHistoryFileName = "TransaccionesHistorial.csv";
        public const string TransactionsTypesFileName = "TransactionTypes.txt";
        public const string TransactionBlankFileName = "TransaccionesBlank.csv";
        public const string TransactionMasterBlankFileName = "TransaccionesMasterBlank.csv";
        public const string TransactionsPaymentsFileName = "Pagos.csv";
        public const string TransactionsPaymentsBlankFileName = "PagosBlank.csv";

        //New transaction Files
        public const string TransactionsFullFileName = "TransaccionesHistorial.csv";
        public const string TransactionsXFileName = "TransaccionesX.csv";
        public const string TransactionsZFileName = "TransaccionesZ.csv";
        public const string TransactionsPaymentsXFileName = "PagosX.csv";
        public const string TransactionsPaymentsZFileName = "PagosZ.csv";

        //Expenses Files
        public const string ExpenseFileName = "Gastos.csv";
        public const string ExpenseXFileName = "GastosX.csv";
        public const string ExpenseZFileName = "GastosZ.csv";
        public const string ExpenseHistoryFileName = "GastosHistorial.csv";
        public const string ExpenseBlankFileName = "GastosBlank.csv";


        //End of day reports
        public const string EndOfDaySalesFileName = "CorteZ.csv";
        public const string CurrentDaySalesFileName = "CorteX.csv";
        public const string MasterEndOfDaySalesFileName = "CorteZMaster.csv";
        public const string EndOfDaySalesBackupFolderPath = @"CorteZBackUp\";

        //Product pages by categories
        public const string CategoryListFileName = "CategoryCatalog.txt";
        public const string ProductPageOne = @"ProductPages\ProductsPage1.txt";
        public const string ProductPageTwo = @"ProductPages\ProductsPage2.txt";
        public const string ProductPageThree = @"ProductPages\ProductsPage3.txt";
        public const string ProductPageFour = @"ProductPages\ProductsPage4.txt";
        public const string ProductPageFive = @"ProductPages\ProductsPage5.txt";

        //Orders Files
        public const string OrdersFolderPath = @"Orders\";
        public const string OrdersFileName = "Pedidos.csv";

        //Users and clients
        public const string UsersFileName = "Users.csv";
        public const string CustomersFileName = "Clientes.csv";
        public const string VendorsFileName = "Proveedores.csv";

        //Images
        public const string ImagesFolderPath = @"Images\";

        //Items list
        public const int MaxNumberListItems = 20;

        //Returns
        public const string ReturnsFileName = "Devoluciones.csv";

        //Colors
        public const string ColorCodeError = "Red";
        public const string ColorCodeSave = "#0285BD";
        public const string ColorCodeRegular = "##0285BD0";

        //log
        public const string LogFileName = "Log.txt";

        //Car Reltated files
    }
}
