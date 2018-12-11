using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericParsing;
using System.Drawing.Printing;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Documents;


namespace Athena
{

    public class Receipt
    {
        #region Fields
        PrintDocument _printDocument;
        PrintDialog _printDialog;
        private Transaction _transaction;

        private readonly ReceiptType _receiptType;
        private int _receiptNumber;
        private DateTime _currentDate;
        private string _customerName;
        private Pos _posData;
        private DataTable _data;
        private int _firstReceiptNumber;
        private int _lastReceiptNumber;
        private Pos _pos;
        private ObservableCollection<ProductBase> _products;

        private int maxCharPerLine = 35;
        #endregion

        #region Properties
        public ObservableCollection<ProductBase> Products { get; set; }
        public Pos Pos
        {
            get
            {
                return _pos;
            }
            set
            {
                _pos = value;
            }
        }
        public ReceiptType Type { get; set; }    

        //old properties
        public int TotalUnitsSold { get; set; }
        public decimal TotalAmountSold { get; set; }

        public Transaction Transaction
        {
            get
            {
                return _transaction;
            }
            set
            {
                _transaction = value;
            }
        }

        public int FirstReceiptNumber
        {
            get
            {
                return _firstReceiptNumber;
            }
        }

        public int LastReceiptNumber
        {
            get
            {
                return _lastReceiptNumber;
            }
        }

        public TransactionDataStruct EndOfDayReceiptData { get; set; }
        public EndOfSalesDataStruct EndOfDayAmountData { get; set; }
        public SalesDataStruct SalesData { get; set; }

        #endregion

        #region Constructors

        public Receipt(Pos posData, Transaction transaction, SalesDataStruct salesData)
        {
            _receiptType = salesData.ReceiptType;
            Pos = posData;
            Transaction = transaction;
            _products = salesData.Products;
            SalesData = salesData;
        }

        public Receipt(Pos posData, ReceiptType receiptType, TransactionDataStruct endOfDayReceiptData, EndOfSalesDataStruct endOfDayAmountData)
        {
            _receiptType = receiptType;
            Pos = posData;
            EndOfDayReceiptData = endOfDayReceiptData;
            EndOfDayAmountData = endOfDayAmountData;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prints regular sales receipt
        /// </summary>
        public void PrintSalesReceipt()
        {
            bool printToFileOnly = false;

            PrintDocument printDocument = new PrintDocument();

            printDocument.PrintPage += new PrintPageEventHandler(printReceipt);

            var fullReceiptName = Constants.DataFolderPath + Constants.ReceiptBackupFolderPath + "Recibo_" +
                                  Pos.LastReceiptNumber.ToString() + ".xps";

            if (!printToFileOnly)
            {
                printDocument.PrinterSettings.PrinterName = Pos.PrinterName;

                printDocument.Print();
            }

            try
            {
                printDocument.PrinterSettings.PrinterName = "Microsoft XPS Document Writer";
                printDocument.PrinterSettings.PrintToFile = true;
                printDocument.PrinterSettings.PrintFileName = fullReceiptName;
                printDocument.Print();
            }
            catch (Exception)
            {

            }

            _products.Clear();
            printDocument.PrintPage -= printReceipt;
        }

        /// <summary>
        /// Prints regular sale receipt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void printReceipt(object sender, PrintPageEventArgs e)
        {
            Graphics graphic = e.Graphics;

            Font font = new Font("Courier New", 9, System.Drawing.FontStyle.Bold);
            Font storeNameFont = new Font("Courier New", 14, System.Drawing.FontStyle.Bold);
            Font storeInfoFont = new Font("Courier New", 8, System.Drawing.FontStyle.Bold);

            ///TODO: might be good to add returns to ticket to be noticable
            string paymentMethod = Transaction.PaymentType.ToString();

            var buf = string.Empty;
            float fontHeight = font.GetHeight();
            float storeNameFontHeight = storeNameFont.GetHeight();
            float storeInfoFontHeight = storeInfoFont.GetHeight();

            int startX = 5;
            int startY = 5;
            int offset = 15;

            int itemsNumber = 0;

            //Ticket header with business information
            buf = "  " + Pos.BusinessName;
            graphic.DrawString(buf, storeNameFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeNameFontHeight;
            buf = "     " + Pos.FiscalName;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;
            buf = "          " + Pos.FiscalType + "  " + Pos.FiscalNumber;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;            
            buf = "  " + Pos.FiscalStreetAddress;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black),
                startX, startY + offset);
            offset  = offset + (int)storeInfoFontHeight;
            buf = "   " + Pos.FiscalCityAndZipCode + "  " +Pos.FiscalPhoneNumber;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;         
            buf = "        " + Pos.FiscalEmail;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 10;

            //Username
            graphic.DrawString("Usuario: " + SalesData.User.UserName, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 2;

            //Date and ticket number
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            var date = DateTime.Now.ToString("g");
            var ticketNumber = "No. " + Pos.LastReceiptNumber.ToString();
            graphic.DrawString(ticketNumber.PadRight(18) + date, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 10;

            //Product loop
            foreach (var product in _products)
            {
                //TODO: poner descripcion en vez de solo la categoria como default o opcion a elegir
                string productDescription = product.Category.PadRight(15);
                string productTotal = string.Format("{0:c}", product.Price);
                string productLine = productDescription + productTotal;

                //product line
                graphic.DrawString(product.ToString(), font, new SolidBrush(Color.Black), startX, startY + offset);

                offset = offset + (int)fontHeight;

                if (product.Category != "Puntos")
                {
                    itemsNumber = itemsNumber + product.LastQuantitySold;
                }
            }

            offset = offset + 20;
            graphic.DrawString("Total: ".PadLeft(20) + string.Format("{0:c}", Transaction.TotalDue), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight;

            graphic.DrawString(string.Format("{0}: ", Transaction.PaymentType.ToString()).PadLeft(20) + string.Format("{0:c}", Transaction.AmountPaid), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight;

            graphic.DrawString("Cambio: ".PadLeft(20) + string.Format("{0:c}", Transaction.ChangeDue), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight;

            graphic.DrawString(("Articulos: " + itemsNumber.ToString()).PadLeft(21), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            //Customer info, if available
            if (SalesData.Customer != null)
            {
                offset = offset + (int)fontHeight;

                graphic.DrawString(("Cliente: " + SalesData.Customer.Name).PadLeft(21), font,
                    new SolidBrush(Color.Black), startX, startY + offset);

                offset = offset + (int)fontHeight;

                graphic.DrawString(("Puntos Obtenidos: " + SalesData.PointsObtained.ToString()).PadLeft(21), font,
                    new SolidBrush(Color.Black), startX, startY + offset);

                offset = offset + (int)fontHeight;

                graphic.DrawString(("Puntos Disponibles: " + SalesData.Customer.PointsAvailable.ToString()).PadLeft(21), font,
                    new SolidBrush(Color.Black), startX, startY + offset);
            }

            offset = offset + (int)fontHeight + 5;

            //Footer message
            graphic.DrawString("       " + Pos.FooterMessage, font, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;
        }

        /// <summary>
        /// Prints end of day Z receipt
        /// </summary>
        public void PrintEndOfDaySalesReceipt()
        {
            bool printToFileOnly = false;

            PrintDocument printDocument = new PrintDocument();

            printDocument.PrintPage += new PrintPageEventHandler(PrintEndOfDaySalesReceipt);

            var currentTime = DateTime.Now;
            var fullReceiptName = Constants.DataFolderPath + Constants.EndOfDaySalesBackupFolderPath + "CorteZ_No_" +
                                  Pos.LastCorteZNumber + "_" + currentTime.Day.ToString("00") + currentTime.Month.ToString("00") + currentTime.Year.ToString("0000") +
                                  currentTime.Hour.ToString("00") + currentTime.Minute.ToString("00") + currentTime.Second.ToString("00") + ".xps";

            if (!printToFileOnly)
            {
                printDocument.PrinterSettings.PrinterName = Pos.PrinterName;

                printDocument.Print();
            }

            try
            {
                printDocument.PrinterSettings.PrinterName = "Microsoft XPS Document Writer";
                printDocument.PrinterSettings.PrintToFile = true;
                printDocument.PrinterSettings.PrintFileName = fullReceiptName;
                printDocument.Print();
            }
            catch (Exception)
            {

            }

            printDocument.PrintPage -= PrintEndOfDaySalesReceipt;
        }

        /// <summary>
        /// Prints end of day X receipt
        /// </summary>
        public void PrintEndOfDaySalesFullReceipt()
        {
            bool printToFileOnly = false;

            PrintDocument printDocument = new PrintDocument();

            printDocument.PrintPage += new PrintPageEventHandler(PrintEndOfDaySalesFullReceipt);

            var currentTime = DateTime.Now;
            var fullReceiptName = Constants.DataFolderPath + Constants.EndOfDaySalesBackupFolderPath + "CorteX_No_" +
                                  Pos.LastCorteZNumber + "_" + currentTime.Day.ToString("00") + currentTime.Month.ToString("00") + currentTime.Year.ToString("0000") +
                                  currentTime.Hour.ToString("00") + currentTime.Minute.ToString("00") + currentTime.Second.ToString("00") + ".xps";

            if (!printToFileOnly)
            {
                printDocument.PrinterSettings.PrinterName = Pos.PrinterName;

                printDocument.Print();
            }

            try
            {
                printDocument.PrinterSettings.PrinterName = "Microsoft XPS Document Writer";
                printDocument.PrinterSettings.PrintToFile = true;
                printDocument.PrinterSettings.PrintFileName = fullReceiptName;
                printDocument.PrinterSettings.PrintRange = PrintRange.AllPages;
                printDocument.Print();
            }
            catch (Exception)
            {

            }

            printDocument.PrintPage -= PrintEndOfDaySalesFullReceipt;
        }

        /// <summary>
        /// Prints end of day Z receipt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PrintEndOfDaySalesReceipt(object sender, PrintPageEventArgs e)
        {
            Graphics graphic = e.Graphics;

            Font font = new Font("Courier New", 9, System.Drawing.FontStyle.Bold);
            Font storeNameFont = new Font("Courier New", 14, System.Drawing.FontStyle.Bold);
            Font storeInfoFont = new Font("Courier New", 8, System.Drawing.FontStyle.Bold);

            var buf = string.Empty;
            float fontHeight = font.GetHeight();
            float storeNameFontHeight = storeNameFont.GetHeight();
            float storeInfoFontHeight = storeInfoFont.GetHeight();

            int startX = 5;
            int startY = 2;
            int offset = 10;

            int itemsNumber = 0;

            //Header business information
            buf = "  " + Pos.BusinessName;
            graphic.DrawString(buf, storeNameFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeNameFontHeight;
            buf = "     " + Pos.FiscalName;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;
            buf = "          " + Pos.FiscalType + "  " + Pos.FiscalNumber;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;
            buf = "  " + Pos.FiscalStreetAddress;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black),
                startX, startY + offset);
            offset = offset + (int)storeInfoFontHeight;
            buf = "   " + Pos.FiscalCityAndZipCode + "  " + Pos.FiscalPhoneNumber;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;

            buf = "        " + Pos.FiscalEmail;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 10;

            //Receipt number and tickets number range
            graphic.DrawString("Corte Z    " + "Folio " + EndOfDayReceiptData.FirstReceiptNumber.ToString() + " al " +
                EndOfDayReceiptData.LastReceiptNumber, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 10;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            var date = DateTime.Now.ToString("g");

            var ticketNumber = "No. " + Pos.LastCorteZNumber.ToString();
            graphic.DrawString(ticketNumber.PadRight(18) + date, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 10;

            //Gets each category information
            foreach (var catInfo in EndOfDayReceiptData.SalesInfoPerCategory)
            {
                //mimic a product just to use the ToString overwrite
                var cat = new ProductBase()
                {
                    Category = catInfo.Item1,
                    LastQuantitySold = catInfo.Item2,
                    Price = catInfo.Item3
                };
                //product line
                if (cat.LastQuantitySold != 0 || cat.Price != 0)
                {
                    graphic.DrawString(cat.ToString(ReceiptType.DailyRegular), font, new SolidBrush(Color.Black), startX, startY + offset);
                    offset = offset + (int)fontHeight;
                }
            }

            offset = offset + 10;

            graphic.DrawString("Articulos: ".PadLeft(20) + string.Format("{0}", EndOfDayReceiptData.TotalItemsSold), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 5;

            graphic.DrawString("Efectivo: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.CashTotal), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 5;

            graphic.DrawString("Tarjeta: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.CardTotal), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 5;

            graphic.DrawString("Otro Metodo: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.OtherTotal + 
                EndOfDayReceiptData.BankTotal + EndOfDayReceiptData.BankTotal), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 5;

            graphic.DrawString("Total: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.TotalAmountSold), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 5;

            graphic.DrawString(string.Format("  Final Corte {0} {1}", EndOfDayAmountData.EndOfSalesReceiptType, date), storeInfoFont, new SolidBrush(Color.Black),
                startX, startY + offset);
            offset = offset + (int)storeInfoFontHeight;

        }

        /// <summary>
        /// Prints end of sales X receipt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PrintEndOfDaySalesFullReceipt(object sender, PrintPageEventArgs e)
        {
            Graphics graphic = e.Graphics;

            Font font = new Font("Courier New", 9, System.Drawing.FontStyle.Bold);
            Font storeNameFont = new Font("Courier New", 14, System.Drawing.FontStyle.Bold);
            Font storeInfoFont = new Font("Courier New", 8, System.Drawing.FontStyle.Bold);

            var buf = string.Empty;
            float fontHeight = 9.0f + 4.5f;
            float storeNameFontHeight = storeNameFont.GetHeight();
            float storeInfoFontHeight = 8 + 6f;

            //Parse Description
            var commentsLines = new List<string>();
            if (EndOfDayAmountData.Comments == null)
                commentsLines.Add(" ");
            else
                commentsLines = Formatter.BreakDownString(EndOfDayAmountData.Comments, 35);

            int startX = 5;
            int startY = 1;
            int offset = 10;

            int itemsNumber = 0;

            //Header business info
            buf = "  " + Pos.BusinessName;
            graphic.DrawString(buf, storeNameFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeNameFontHeight;
            buf = "     " + Pos.FiscalName;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;
            buf = "          " + Pos.FiscalType + "  " + Pos.FiscalNumber;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;
            buf = "  " + Pos.FiscalStreetAddress;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black),
                startX, startY + offset);
            offset = offset + (int)storeInfoFontHeight;
            buf = "   " + Pos.FiscalCityAndZipCode + "  " + Pos.FiscalPhoneNumber;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight;

            buf = "        " + Pos.FiscalEmail;
            graphic.DrawString(buf, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 10;

            //Ticket numbers and range
            graphic.DrawString("Corte X    " + "Folio " + EndOfDayReceiptData.FirstReceiptNumber.ToString() + " al " +
                EndOfDayReceiptData.LastReceiptNumber, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 10;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            var date = DateTime.Now.ToString("g");

            var ticketNumber = "No. " + Pos.LastCorteZNumber.ToString();
            graphic.DrawString(ticketNumber.PadRight(18) + date, storeInfoFont, new SolidBrush(Color.Black), startX,
                startY + offset);
            offset = offset + (int)storeInfoFontHeight + 10;

            //Sales per category loop
            foreach (var catInfo in EndOfDayReceiptData.SalesInfoPerCategory)
            {
                //mimic a product just to use the ToString overwrite
                var cat = new ProductBase()
                {
                    Category = catInfo.Item1,
                    LastQuantitySold = catInfo.Item2,
                    Price = catInfo.Item3
                };
                if (cat.LastAmountSold != 0 || cat.Price != 0)
                {
                    graphic.DrawString(cat.ToString(ReceiptType.DailyRegular), font, new SolidBrush(Color.Black), startX, startY + offset);
                    offset = offset + (int)fontHeight;
                }
            }

            offset = offset + 10;

            //TOTAL LE CABEN 35 letras
            //graphic.DrawString("***********************************", font,
            //    new SolidBrush(Color.Black), startX, startY + offset);

            //offset = offset + (int)fontHeight + 1;

            graphic.DrawString("********** Total Ventas ***********", font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Puntos Usados: ".PadLeft(20) + string.Format("{0}", EndOfDayReceiptData.PointsTotal), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Total Articulos: ".PadLeft(20) + string.Format("{0}", EndOfDayReceiptData.TotalItemsSold), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Efectivo MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.CashTotal), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Tarjeta MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.CardTotal), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Otro Metodo MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.OtherTotal +
                EndOfDayReceiptData.BankTotal + EndOfDayReceiptData.CheckTotal), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Total MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.TotalAmountSold), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("********** Devoluciones ***********", font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Total Articulos: ".PadLeft(20) + string.Format("{0}", EndOfDayReceiptData.TotalReturnItems), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Efectivo MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.ReturnsCash), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Tarjeta MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.ReturnsCard), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Total MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayReceiptData.ReturnsCard + EndOfDayReceiptData.ReturnsCash),
                font, new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("*********** Total Caja ************", font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Monedas Pesos: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.MxnCoins), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Pesos 20: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Mxn20), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Pesos 50: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Mxn50), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Pesos 100: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Mxn100), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Pesos 200: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Mxn200), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Pesos 500: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Mxn500), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Pesos 1000: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Mxn1000), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Total Efectivo MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.MxnTotalCash), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Monedas Dolar: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.UsdCoins), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Dolar 1: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Usd1), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Dolar 5: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Usd5), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Dolar 10: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Usd10), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Dolar 20: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Usd20), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Dolar 50: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Usd50), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Dolar 100: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.Usd100), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Total Efectivo USD: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.UsdTotalCash), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("********** Total Gastos ***********", font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Efectivo MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.ExpensesCash), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Totales MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.ExpensesTotal), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("************* Resumen *************", font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Usuario: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.User), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Tipo de Corte: ".PadLeft(20) + string.Format("{0}", EndOfDayAmountData.EndOfSalesReceiptType), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Tipo de Cambio: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.ExchangeRate), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Caja Inicio MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.InitialCash), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Caja Nueva MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.NewInitialCash), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;
            
            graphic.DrawString("Diferencia MXN: ".PadLeft(20) + string.Format("{0:c}", EndOfDayAmountData.Delta), font,
                new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;
            offset = offset + (int)fontHeight + 1;

            graphic.DrawString("Comentarios: ", font, new SolidBrush(Color.Black), startX, startY + offset);

            offset = offset + (int)fontHeight + 1;

            foreach(var commentLine in commentsLines)
            {
                graphic.DrawString(string.Format("{0}", commentLine), font, new SolidBrush(Color.Black), startX, startY + offset);

                offset = offset + (int)fontHeight + 1;
            }

            graphic.DrawString(string.Format("  Final Corte {0} {1}", EndOfDayAmountData.EndOfSalesReceiptType, date), storeInfoFont, new SolidBrush(Color.Black),
                startX, startY + offset);
            offset = offset + (int)storeInfoFontHeight;

        }

        public List<Tuple<string, int, decimal>> GetDataPerCategory(out decimal cashAmount, out decimal cardAmount)
        {
            TotalAmountSold = 0;
            TotalUnitsSold = 0;
            _firstReceiptNumber = 0;
            _lastReceiptNumber = 0;
            cashAmount = 0;
            cardAmount = 0;

            string selectedFilePath = String.Empty;

            var categoryData = new List<Tuple<string, int, decimal>>();

            //Open current transaction file and get data
            if (_receiptType == ReceiptType.DailyInternal)
            {
                selectedFilePath = _posData.TransactionMasterDataFilePath;
            }
            else if (_receiptType == ReceiptType.DailyRegular)
            {
                selectedFilePath = _posData.TransactionsDataFilePath;
            }

            using (var parser = new GenericParserAdapter(selectedFilePath))
            {
                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                parser.SkipStartingDataRows = 0;
                parser.SkipEmptyRows = true;
                parser.MaxBufferSize = 4096;
                parser.MaxRows = 8000;

                _data = parser.GetDataTable();
            }

            var categories = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.CategoryListFileName); //new CategoryCatalog(_posData.Catalog).categories;

            //Get each category
            foreach (var category in categories)
            {
                decimal amount = 0M;
                int itemsNumber = 0;

                for (var index = 0; index < _data.Rows.Count; index++)
                {
                    var row = _data.Rows[index];

                    if (row["CategoriaProducto"].ToString() == category)
                    {
                        var tempAmount = decimal.Parse(row["TotalVendido"].ToString());
                        amount += tempAmount;
                        itemsNumber += Int32.Parse(row["UnidadesVendidas"].ToString());
                        if (row["MetodoPago"].ToString() == "Cash")
                        {
                            cashAmount += tempAmount;
                        }
                        else
                        {
                            cardAmount += tempAmount;
                        }
                    }
                }

                categoryData.Add(new Tuple<string, int, decimal>(category.ToString(), itemsNumber, amount));
                TotalAmountSold += amount;
                TotalUnitsSold += itemsNumber;
            }

            //Get first and last receipt number
            var firstRow = _data.Rows[0];
            var lastRow = _data.Rows[_data.Rows.Count - 1];
            _firstReceiptNumber = Int32.Parse(firstRow["NumeroTicket"].ToString());
            _lastReceiptNumber = Int32.Parse(lastRow["NumeroTicket"].ToString());

            return categoryData;
        }

        //Get customer information, if available
        public void GetCustomerName(Customer customer)
        {
            _customerName = customer.Name;
        }

        public static void AddReceiptTransaction(string filePath, int receiptNumber, int totalUnitsSold, decimal totalCashSold, decimal totalCardSold, decimal totalAmountSold, string date)
        {
            string data = string.Format("{0},{1},{2},{3},{4},{5}", receiptNumber, totalUnitsSold, totalCashSold, totalCardSold, totalAmountSold, date) + Environment.NewLine;

            //Append to daily receipt
            File.AppendAllText(filePath, data);
        }

        #endregion

    }

    public enum ReceiptType
    {
        Single,
        RegularSale,
        DailyRegular,
        DailyInternal,
        DailyInformative,
        Weekly,
        Monthly,
        Unknown
    }

}