using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Athena.WpfBindingUtilities;

namespace Athena
{
    public class ReturnsViewModel : ObservableObject
    {
        #region Fields

        private string _returnId;
        private int _ticketNumber;
        private decimal _returnAmount;
        private string _returnReason;
        private DateTime _purchaseDate;
        private string _customerName;
        private string _customerNumber;

        #endregion

        #region Constructors
        public ReturnsViewModel()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            ReturnID = CreateReturnID();
            PurchaseDate = DateTime.Today;
        }
        #endregion

        #region Observable Properties

        public string ReturnID
        {
            get { return _returnId; }
            set
            {
                _returnId = value;
                OnPropertyChanged();
            }
        }

        public int TicketNumber
        {
            get { return _ticketNumber; }
            set
            {
                _ticketNumber = value; 
                OnPropertyChanged();
            }
        }

        public string ReturnReason
        {
            get { return _returnReason; }
            set
            {
                _returnReason = Formatter.SanitizeInput(value);
                OnPropertyChanged();
            }
        }

        public string CustomerName
        {
            get { return _customerName; }
            set
            {
                _customerName = Formatter.SanitizeInput(value);
                OnPropertyChanged();
            }
        }

        public string CustomerNumber
        {
            get { return _customerNumber; }
            set
            {
                _customerNumber = Formatter.SanitizeInput(value);
                OnPropertyChanged();
            }
        }

        public DateTime PurchaseDate
        {
            get { return _purchaseDate; }
            set { _purchaseDate = value; }
        }

        #endregion

        #region Methods
        private string CreateReturnID()
        {
            Random generator = new Random();
            var num = generator.Next(0, 9999).ToString("D4");
            var timeVar = DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            return timeVar + num;
        }

        private void RecordReturn()
        {
            var data = string.Format("{0},{1},{2:g},{3},{4:s},{5},{6},{7}", ReturnID, MainWindowViewModel.GetInstance().CurrentUser.Name, DateTime.Now, TicketNumber, PurchaseDate, CustomerName, CustomerNumber,
                           ReturnReason) + Environment.NewLine; 
            //Append to daily receipt
            File.AppendAllText(Constants.DataFolderPath + Constants.ReturnsFileName, data);
        }
        #endregion

        #region Commands

        #region ReturnSaveChangesCommand
        public ICommand ReturnSaveChangesCommand { get { return _returnSaveChangesCommand ?? (_returnSaveChangesCommand = new DelegateCommand(Execute_ReturnSaveChangesCommand, CanExecute_ReturnSaveChangesCommand)); } }
        private ICommand _returnSaveChangesCommand;

        internal void Execute_ReturnSaveChangesCommand(object parameter)
        {
            if (MainWindowViewModel.GetInstance().CurrentCartProducts.Count > 0)
            {
                //Record Transaction
                RecordReturn();
                //Message
                MainWindowViewModel.GetInstance().Code = "¡Devolución Registrada!";
                MainWindowViewModel.GetInstance().CodeColor = Constants.ColorCodeSave;
                MainWindowViewModel.GetInstance().ReturnID = Int32.Parse(ReturnID);
                MainWindowViewModel.GetInstance().ReturnTransaction = true;
                MainWindowViewModel.GetInstance().PaymentReceivedMXN = MainWindowViewModel.GetInstance().CalculateCurrentCartTotal();
                //Log
                MainWindowViewModel.GetInstance().Log.Write(MainWindowViewModel.GetInstance().CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Devolucion Registrada Folio: " + ReturnID.ToString() );
                MainWindowViewModel.GetInstance().CurrentPage = "\\View\\PaymentPage.xaml";
            }
            else
            {
                MainWindowViewModel.GetInstance().Code = "¡Agregar Artículos al Carrito!";
                MainWindowViewModel.GetInstance().CodeColor = Constants.ColorCodeError;
            }
        }

        internal bool CanExecute_ReturnSaveChangesCommand(object parameter)
        {
            return TicketNumber != 0 && ReturnReason != null & CustomerName != null;
        }
        #endregion

        #endregion
    }
}
