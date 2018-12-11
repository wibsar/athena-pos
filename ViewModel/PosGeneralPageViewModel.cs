using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Athena.WpfBindingUtilities;
using Athena;

namespace Athena
{
    /// <summary>
    /// ViewModel class to manipulate manual POS transactions
    /// </summary>
    public class PosGeneralPageViewModel : ObservableObject
    {
        #region Fields

        private static PosGeneralPageViewModel _posGeneralInstance;
        private string _price = "0";
        private int _quantity = 1;
        private string _description;
        private string _category;
        private bool _clear;
        private static ProductBase _manualProduct;
        private static ObservableCollection<string> _categoriesList;
        private bool _usdEnabled = false;
        private bool _transactionZEnabled = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Private constructor; loads the product if there was one as input but did not clear before closing page
        /// </summary>
        private PosGeneralPageViewModel()
        {
            UpdateCategoriesList();
        }

        /// <summary>
        /// Constructor using singleton
        /// </summary>
        /// <returns></returns>
        public static PosGeneralPageViewModel GetInstance()
        {
            if(_posGeneralInstance == null)
                _posGeneralInstance = new PosGeneralPageViewModel();

            return _posGeneralInstance;
        }

        #endregion

        #region Observable Properties

        private string _codeColor = "#2C5066";
        public string CodeColor
        {
            get { return _codeColor; }
            set
            {
                _codeColor = value;
                OnPropertyChanged();
            }
        }

        private string _currencyTypeContent = "Pesos";
        public string CurrencyTypeContent
        {
            get { return _currencyTypeContent; }
            set
            {
                _currencyTypeContent = value;
                OnPropertyChanged();
            }
        }

        private string _transactionFileTypeContent = "X";
        public string TransactionFileTypeContent
        {
            get { return _transactionFileTypeContent; }
            set
            {
                _transactionFileTypeContent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Shows price entered
        /// </summary>
        public string Price
        {
            get { return _price; }
            set
            {
                //Check if price has something other than digits or decimal point
                if (value != "")
                {
                    if (value.Substring(value.Length - 1) == "." && !_price.Contains("."))
                    {
                        _price += ".";
                    }
                    else
                    {
                        decimal.TryParse(value, out var number);
                        _price = number > 0 ? number.ToString(CultureInfo.CurrentCulture) : "0";
                    }
                }
                else
                {
                    _price = value;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Shows quantity entered
        /// </summary>
        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; OnPropertyChanged();}
        }

        /// <summary>
        /// Description of item entered
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = string.Empty;
                if(value != null)
                {
                    var desc = value.ToCharArray();
                    for (var index = 0; index < desc.Length; ++index)
                    {
                        if (desc[index] == ',')
                            desc[index] = '.';
                        _description += desc[index];
                    }
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Category of item entered
        /// </summary>
        public string Category
        {
            get { return _category; }
            set { _category = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Shows the status
        /// </summary>
        public bool Clear
        {
            get { return _clear; }
            set { _clear = value; OnPropertyChanged(); OnClearChanged();}
        }

        /// <summary>
        /// List of available categories
        /// </summary>
        public ObservableCollection<string> CategoriesList
        {
            get { return _categoriesList; }
            set { _categoriesList = value; OnPropertyChanged(); OnClearChanged(); }
        }

        #endregion

        #region Properties
        /// <summary>
        /// Property for the manually created product
        /// </summary>
        public ProductBase ManualProduct
        {
            get { return _manualProduct; }
            set
            {
                _manualProduct = value;
                if (_manualProduct == null) return;
                Description = _manualProduct.Description;
                Price = _manualProduct.Price.ToString(CultureInfo.CurrentUICulture);
                Category = _manualProduct.Category;
                Quantity = _manualProduct.LastQuantitySold;
                //TODO: Add MXN or USD HERE once implemented
            }
        }

        public bool UsdEnabled
        {
            get { return _usdEnabled; }
            set
            {
                _usdEnabled = value;
                if (value)
                {
                    CodeColor = Constants.ColorCodeError;
                    CurrencyTypeContent = "Dolar";
                }
                else
                {
                    CodeColor = Constants.ColorCodeSave;
                    CurrencyTypeContent = "Pesos";
                }
            }
        }

        public bool TransactionZEnabled
        {
            get { return _transactionZEnabled; }
            set
            {
                _transactionZEnabled = value;
                if (value)
                {
                    CodeColor = Constants.ColorCodeError;
                    TransactionFileTypeContent = "Z";
                }
                else
                {
                    CodeColor = Constants.ColorCodeSave;
                    TransactionFileTypeContent = "Y";
                }
            }
        }

        #endregion

        #region EnterKeyPadNumberCommand

        public ICommand EnterKeyPadNumberCommand { get { return _enterKeyPadNumberCommand ?? (_enterKeyPadNumberCommand = new DelegateCommand(Execute_EnterKeyPadNumberCommand, CanExecute_EnterKeyPadNumberCommand)); } }
        private ICommand _enterKeyPadNumberCommand;

        internal void Execute_EnterKeyPadNumberCommand(object parameter)
        {
            //Call methods or execute the command
            if ((string) parameter == ".")
            {
                if (!Price.Contains("."))
                {
                    Price += (string)parameter;
                }
            }
            else if ((string)parameter == "C")
            {
                Price = "0";
            }
            else
            {
                Price += (string)parameter;
            }
        }

        internal bool CanExecute_EnterKeyPadNumberCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region ClickEnterCommand

        public ICommand ClickEnterCommand { get { return _clickEnterCommand ?? (_clickEnterCommand = new DelegateCommand(Execute_ClickEnterCommand, CanExecute_ClickEnterCommand)); } }
        private ICommand _clickEnterCommand;

        internal void Execute_ClickEnterCommand(object parameter)
        {
            if (Category != null && Price != null && Price != "0")
            {
                if (ManualProduct == null)
                {
                    if (UsdEnabled == true)
                    {
                        ManualProduct = ProductBase.Add(Description, Category, Math.Round(decimal.Parse(Price)*MainWindowViewModel.GetInstance().ExchangeRate,2), Quantity);
                    }
                    else
                    {
                        ManualProduct = ProductBase.Add(Description, Category, decimal.Parse(Price), Quantity);
                    }
                }
                else
                {
                    //Check price currency and current currency state 
                    if (ManualProduct.PriceCurrency == CurrencyTypeEnum.USD)
                    {
                        UsdEnabled = true;
                        ManualProduct.Price = Math.Round(decimal.Parse(Price) * MainWindowViewModel.GetInstance().ExchangeRate, 2);
                    }
                    else
                    {
                        UsdEnabled = false;
                        ManualProduct.Price = decimal.Parse(Price);
                    }
        
                    ManualProduct.Description = Description;
                    ManualProduct.Category = Category;
                    ManualProduct.LastQuantitySold = Quantity;
                }
            }
            //MainWindowViewModel.AddManualProductToCart(ManualProduct); Changed from static
            var main = MainWindowViewModel.GetInstance();

            main.AddManualProductToCart(ManualProduct);
            Clear = true;
            Price = "0";
            Description = "";
            Quantity = 1;
            ManualProduct = null;

        }

        internal bool CanExecute_ClickEnterCommand(object parameter)
        {
            //Add logic to check if the command can be executed (if any)
            return Price != string.Empty && Price != "0" && Price != null && Category != null;
        }
        #endregion

        #region AddOrSubtractQuantityCommand

        public ICommand AddOrSubtractQuantityCommand { get { return _addOrSubtractQuantityCommand ?? (_addOrSubtractQuantityCommand = new DelegateCommand(Execute_AddOrSubtractQuantityCommand, CanExecute_AddOrSubtractQuantityCommand)); } }
        private ICommand _addOrSubtractQuantityCommand;

        internal void Execute_AddOrSubtractQuantityCommand(object parameter)
        {
            switch ((string) parameter)
            {
                case "Add":
                    Quantity += 1;
                    break;
                case "Subtract" when Quantity >1:
                    Quantity -= 1;
                    break;
            }
        }

        internal bool CanExecute_AddOrSubtractQuantityCommand(object parameter)
        {
            //Add logic to check if the command can be executed (if any)
            return true;
        }
        #endregion

        #region ChangeCurrencyCommand

        public ICommand ChangeCurrencyCommand { get { return _changeCurrencyCommand ?? (_changeCurrencyCommand = new DelegateCommand(Execute_ChangeCurrencyCommand, CanExecute_ChangeCurrencyCommand)); } }
        private ICommand _changeCurrencyCommand;

        internal void Execute_ChangeCurrencyCommand(object parameter)
        {
            if (UsdEnabled)
            {
                UsdEnabled = false;
            }
            else
            {
                UsdEnabled = true;
            }
        }

        internal bool CanExecute_ChangeCurrencyCommand(object parameter)
        {
            //Add logic to check if the command can be executed (if any)
            return true;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Set property values to default settings
        /// </summary>
        public void OnClearChanged()
        {
            if (!Clear) return;

            Price = string.Empty;
            Quantity = 1;
            Description = string.Empty;
            Category = null;
            Clear = false;
            ManualProduct = null;
        }

        /// <summary>
        /// Updates the list of categories from database
        /// </summary>
        public void UpdateCategoriesList()
        {
            CategoriesList = new ObservableCollection<string>(CategoryCatalog.GetList(Constants.DataFolderPath + Constants.CategoryListFileName));
        }
        #endregion
    }
}
