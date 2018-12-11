using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Athena;
using MySql.Data.MySqlClient;
using Athena.WpfBindingUtilities;

namespace Athena
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Fields
        //Main instances
        private static InventoryBase _inventoryInstance = null;
        private static MainWindowViewModel _appInstance = null;
        private static PosGeneralPageViewModel _posGeneralInstance = null;
        private static Pos _posInstance = null;
        private static User _userInstance = null;
        private static Expense _expenseInstance = null;
        private static Logger _logInstance = null;
        private static UserAccessLevelEnum _accessLevelGranted;
        private static bool _systemUnlock = false;
        private static User _currentUser;
        private static Customer _currentCustomer;

        //Products page list related fields
        private static ObservableCollection<string> _products;
        private static string _pageOneTitle;
        private static string _pageTwoTitle;
        private static string _pageThreeTitle;
        private static string _pageFourTitle;
        private static string _pageFiveTitle;
        private static string _currentPageListTitle;
        private static ObservableCollection<ProductBase> _currentPageListProducts = new ObservableCollection<ProductBase>();
        private static string _currentSelectedItemsListPageFileName;
        private static ProductBase _selectedPageListProduct;
        private static int _lastSelectedProductsPage = 1;
        public ObservableCollection<ProductBase> _productObjects;
        private int _groupSquaresAvailable;

        //Navegation related fields
        private string _currentPage;

        private string _code;
        private string _checkoutTotal;// = "2";

        //Carts related fields
        private ObservableCollection<ProductBase> _cartProducts;
        private static ProductBase _selectedCartProduct;
        private int _currentCartNumber;
        private static ObservableCollection<ProductBase> _cartOneProducts = new ObservableCollection<ProductBase>();
        private static ObservableCollection<ProductBase> _cartTwoProducts = new ObservableCollection<ProductBase>();
        private static ObservableCollection<ProductBase> _cartThreeProducts = new ObservableCollection<ProductBase>();

        //Payment related fields
        private decimal _paymentTotalUSD;
        private decimal _paymentTotalMXN;
        private decimal _paymentReceivedUSD;
        private decimal _paymentReceivedMXN;
        private int _paymentCustomerNumber;
        private decimal _paymentChangeMXN;
        private decimal _paymentChangeUSD;
        private decimal _paymentRemainingMXN;
        private decimal _paymentRemainingUSD;
        private decimal _exchangeRate;
        private string _exchangeRateString;
        private decimal _pointsConvertionRatio = 100M; //TODO: add this to the UI after it is defined
        private double _paymentPointsInUse;
        private bool _partialPaymentEnabled = false;

        //Inventory Related Fields
        private ProductBase _inventoryTemporalItem;
        private static ObservableCollection<string> _categoriesList;
        private static ObservableCollection<string> _currentCategoryList;

        //Category Related Fields
        private string _selectedCategoryItem;
        private string _newCategoryItem;

        //Holds active cart product list
        private static ObservableCollection<ProductBase> _currentCartProducts = new ObservableCollection<ProductBase>()
        {

        };

        private bool _transactionZEnabled = false;

        #endregion

        #region Constructors

        private MainWindowViewModel()
        {
            //Testing
            //try
            //{
            //    var conn = new MySqlConnection(@"Server=wibsarlicencias.csqn2onotlww.us-east-1.rds.amazonaws.com;Database=Licenses;Uid=armoag;Pwd=Yadira00;");
            //    conn.Open();

            //    string sql = @"SELECT LicenseKey, CurrentUser FROM Licenses WHERE idLicenses=2";

            //    var cmd = new MySqlCommand(sql, conn);

            //    MySqlDataReader reader = cmd.ExecuteReader();
            //    if (reader.Read())
            //    {
            //        var license = reader["LicenseKey"].ToString();
            //        var currentUser = reader["CurrentUser"].ToString();
            //    }

            //    sql = @"SELECT LicenseKey, CurrentUser FROM Licenses WHERE idLicenses=1";

            //    var cmd2 = new MySqlCommand(sql, conn);
            //    //         MySqlDataReader reader = cmd.ExecuteReader();
            //    if (reader.Read())
            //    {
            //        var license = reader["LicenseKey"].ToString();
            //        var currentUser = reader["CurrentUser"].ToString();
            //    }

            //    conn.Close();
            //}
            //catch (Exception e)
            //{
            //    var x = 1;
            //}


            //Log
            Log = Logger.GetInstance(Constants.DataFolderPath + Constants.LogFileName);
            Log.Write("Desconocido", this.ToString() + MethodBase.GetCurrentMethod().Name, "<<<Iniciando programa>>>");
            //Initialize current cart and list number
            CurrentCartNumber = 1;
            CurrentCartProducts = _cartOneProducts;
            //Initialize inventory and Pos data files
            _inventoryInstance = InventoryBase.GetInstance(Constants.DataFolderPath + Constants.InventoryFileName);
            _posInstance = Pos.GetInstance(Constants.DataFolderPath + Constants.PosDataFileName);
            ExchangeRate = _posInstance.ExchangeRate;
            //Page Titles
            GetInitialPagesTitles();
            //Initial Categories
            if (CategoriesList == null)
                CategoriesList = new ObservableCollection<string>(CategoryCatalog.GetList(Constants.DataFolderPath + Constants.CategoryListFileName));
            //Log
            Log.Write("Desconocido", this.ToString() + " " + MethodBase.GetCurrentMethod().Name, "Inicio de programa completado");
            //Set default page
            CurrentPage = "\\View\\LoginPage.xaml";
            LoginMessage = "¡Bienvenido!";
        }

        public static MainWindowViewModel GetInstance()
        {
            if (_appInstance == null)
                _appInstance = new MainWindowViewModel();
            return _appInstance;
        }

        #endregion

        #region Properties

        public Logger Log
        {
            get { return _logInstance; }
            private set { _logInstance = value; }
        }
        public BitmapImage Image
        {
            get
            {
                BitmapImage bitmap = new BitmapImage();
                if (LogoImage != null)
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(@"C:\Projects\seiya-pos\Resources\Images\" + _posInstance.LogoName);
                    bitmap.EndInit();
                    _logoImage = bitmap;
                }
                return bitmap;
            }
            set
            {
                _logoImage = value;
            }
        }

        public ProductBase InventoryTemporalItem
        {
            get
            {
                return _inventoryTemporalItem;
            }
            set
            {
                _inventoryTemporalItem = value;
                OnPropertyChanged();
            }
        }

        public static InventoryBase InventoryInstance
        {
            get { return _inventoryInstance; }
        }

        //Holds last selected products page
        public static int LastSelectedProductsPage
        {
            get
            {
                return _lastSelectedProductsPage;
            }
            set
            {
                _lastSelectedProductsPage = value;
            }
        }

        public UserAccessLevelEnum AccessLevelGranted
        {
            get { return _accessLevelGranted; }
            set { _accessLevelGranted = value; }
        }

        private bool SystemUnlock
        {
            get { return _systemUnlock; }
            set { _systemUnlock = value; }
        }

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public double PaymentPointsInUse
        {
            get { return _paymentPointsInUse; }
            set { _paymentPointsInUse = value; }
        }

        private int _returnID = 0;
        public int ReturnID
        {
            get
            {
                return _returnID;
            }
            set
            {
                _returnID = value;
            }
        }

        public decimal PointsConvertionRatio
        {
            get { return _posInstance.PointsPercent / 100; }
        }

        public decimal DiscountPercent
        {
            get { return _posInstance.DiscountPercent / 100; }
        }

        #region Orders Properties

        #endregion

        #endregion

        #region Observable Properties

        private BitmapImage _logoImage;
        public BitmapImage LogoImage
        {
            get
            {
                //return SelectedInventoryProduct == null ? null : SelectedInventoryProduct.Image;
                return Image == null ? null : _logoImage;
            }
            set
            {
                _logoImage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// List of available categories
        /// </summary>
        public ObservableCollection<string> CategoriesList
        {
            get { return _categoriesList; }
            set { _categoriesList = value; OnPropertyChanged();}
        }

        /// <summary>
        /// Holds currently selected category items
        /// </summary>
        public string SelectedCategoryItem
        {
            get { return _selectedCategoryItem; }
            set
            {
                _selectedCategoryItem = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hold active cart items list
        /// </summary>
        public ObservableCollection<string> CurrentCategoryList
        {
            get { return _currentCategoryList; }
            set
            {
                _currentCategoryList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Holds currently selected category items
        /// </summary>
        public string NewCategoryItem
        {
            get { return _newCategoryItem; }
            set
            {
                _newCategoryItem = Formatter.SanitizeInput(value);
                OnPropertyChanged();                          
            }
        }

        /// <summary>
        /// Hold active cart items list
        /// </summary>
        public ObservableCollection<ProductBase> CurrentCartProducts
        {
            get { return _currentCartProducts; }
            set
            {
                _currentCartProducts = value;
                PaymentTotalMXN = CalculateCurrentCartTotal();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Holds currently selected item
        /// </summary>
        public ProductBase SelectedCartProduct
        {
            get { return _selectedCartProduct; }
            set
            {
                _selectedCartProduct = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Holds currently selected item for the product page list
        /// </summary>
        public ProductBase SelectedPageListProduct
        {
            get { return _selectedPageListProduct; }
            set
            {
                _selectedPageListProduct = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hold active cart items list
        /// </summary>
        public ObservableCollection<ProductBase> CurrentPageListProducts
        {
            get { return _currentPageListProducts; }
            set
            {
                _currentPageListProducts = value;
                OnPropertyChanged();
                OnPropertyChanged("GroupSquaresAvailable");
            }
        }
        
        /// <summary>
        /// Holds active products page title
        /// </summary>
        public string CurrentPageListTitle
        {
            get { return _currentPageListTitle; }
            set
            {
                _currentPageListTitle = Formatter.SanitizeInput(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Holds active products page title
        /// </summary>
        public string CurrentSelectedItemsListPageFileName
        {
            get { return _currentSelectedItemsListPageFileName; }
            set
            {
                _currentSelectedItemsListPageFileName = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Holds the active cart number
        /// </summary>
        public int CurrentCartNumber
        {
            get { return _currentCartNumber; }
            set
            {
                _currentCartNumber = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Holds current page
        /// </summary>
        public string CurrentPage
        {
            get { return _currentPage; }
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public string Code
        {
            get { return _code; }
            set
            {
                //var product = _inventoryInstance.GetProduct(value + "x");
                //if (product.Code != null)
                //{
                //    product.LastQuantitySold = 1;
                //    AddProductToCart(product);
                //    _code = string.Empty;
                //}
                //else
                //    _code = value;

                _code = value;
                OnPropertyChanged();
            }
        }

        public string CheckoutTotal
        {
            get { return _checkoutTotal; }
            set
            {
                _checkoutTotal = value;
                OnPropertyChanged();
            }
        }

        public string CheckoutTotalUSD
        {
            //TODO: Add Exchange rate from POS info
            get { return _checkoutTotal; }
            set
            {
                _checkoutTotal = value;
                OnPropertyChanged("CheckoutTotal");
            }
        }

        public ObservableCollection<string> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProductBase> ProductObjects
        {
            get { return _productObjects; }
            set
            {
                _productObjects = value;
                OnPropertyChanged();
            }
        }

        public decimal PaymentTotalUSD
        {
            get { return _paymentTotalUSD; }
            set
            {
                _paymentTotalUSD = value;
                OnPropertyChanged();
            }
        }

        public string PaymentTotalUSDString
        {
            get { return "+$" + _paymentTotalUSD.ToString(); }
            set
            {
                OnPropertyChanged();
            }
        }

        public decimal PaymentTotalMXN
        {
            get
            {
                return _paymentTotalMXN;
            }
            set
            {
                _paymentTotalMXN = Math.Round(value, 2);
                if(_exchangeRate != 0)
                    PaymentTotalUSD = Math.Round(_paymentTotalMXN /_exchangeRate, 2);
                OnPropertyChanged();
            }
        }
        
        public string PaymentTotalMXNString
        {
            get
            {
                return "+$" + _paymentTotalMXN.ToString();
            }
            set
            {                
                OnPropertyChanged();
            }
        }

        public decimal PaymentReceivedUSD
        {
            get { return _paymentReceivedUSD; }
            set
            {
                _paymentReceivedUSD = value;
                PaymentUpdateRemaining();
                OnPropertyChanged();
            }
        }

        public decimal PaymentReceivedMXN
        {
            get { return _paymentReceivedMXN; }
            set
            {
                _paymentReceivedMXN = value;
                PaymentUpdateRemaining();
                OnPropertyChanged();
            }
        }

        private decimal _paymentPartialCashUSD;
        public decimal PaymentPartialCashUSD
        {
            get { return _paymentPartialCashUSD; }
            set
            {
                _paymentPartialCashUSD = value;
                PaymentPartialUpdateRemaining();
                OnPropertyChanged();
            }
        }

        private decimal _paymentPartialCashMXN;
        public decimal PaymentPartialCashMXN
        {
            get { return _paymentPartialCashMXN; }
            set
            {
                _paymentPartialCashMXN = value;
                PaymentPartialUpdateRemaining();
                OnPropertyChanged();
            }
        }

        private decimal _paymentPartialCardMXN;
        public decimal PaymentPartialCardMXN
        {
            get { return _paymentPartialCardMXN; }
            set
            {
                _paymentPartialCardMXN = value;
                PaymentPartialUpdateRemaining();
                OnPropertyChanged();
            }
        }

        private decimal _paymentPartialCheckMXN;
        public decimal PaymentPartialCheckMXN
        {
            get { return _paymentPartialCheckMXN; }
            set
            {
                _paymentPartialCheckMXN = value;
                PaymentPartialUpdateRemaining();
                OnPropertyChanged();
            }
        }

        private decimal _paymentPartialTransferMXN;
        public decimal PaymentPartialTransferMXN
        {
            get { return _paymentPartialTransferMXN; }
            set
            {
                _paymentPartialTransferMXN = value;
                PaymentPartialUpdateRemaining();
                OnPropertyChanged();
            }
        }

        private decimal _paymentPartialOtherMXN;
        public decimal PaymentPartialOtherMXN
        {
            get { return _paymentPartialOtherMXN; }
            set
            {
                _paymentPartialOtherMXN = value;
                PaymentPartialUpdateRemaining();
                OnPropertyChanged();
            }
        }

        public decimal PaymentChangeMXN
        {
            get { return _paymentChangeMXN; }
            set
            {
                _paymentChangeMXN = value;
                OnPropertyChanged();
            }
        }

        public decimal PaymentChangeUSD
        {
            get { return _paymentChangeUSD; }
            set
            {
                _paymentChangeUSD = value;
                OnPropertyChanged();
            }
        }

        public decimal PaymentRemainingMXN
        {
            get
            {
                return _paymentRemainingMXN; 
            }
            set
            {
                _paymentRemainingMXN = value;
                OnPropertyChanged();
            }
        }

        public decimal PaymentRemainingUSD
        {
            get { return _paymentRemainingUSD; }
            set
            {
                _paymentRemainingUSD = value;
                OnPropertyChanged();
            }
        }

        public int PaymentCustomerNumber
        {
            get { return _paymentCustomerNumber; }
            set
            {
                _paymentCustomerNumber = value;
                OnPropertyChanged();
            }
        }

        private string _paymentCustomerSearchInput;
        public string PaymentCustomerSearchInput
        {
            get { return _paymentCustomerSearchInput; }
            set
            {
                _paymentCustomerSearchInput = value.ToLower();
                OnPropertyChanged();
            }
        }

        private double _paymentPointsReceived;
        public double PaymentPointsReceived
        {
            get { return _paymentPointsReceived; }
            set
            {
                _paymentPointsReceived = value;
                OnPropertyChanged();
            }
        }

        public bool PartialPaymentEnabled
        {
            get { return _partialPaymentEnabled; }
            set
            {
                _partialPaymentEnabled = value;
                if (value)
                {
                    CodeColor = Constants.ColorCodeError;
                }
                else
                {
                    CodeColor = Constants.ColorCodeSave;
                }
            }
        }

        private bool _returnTransaction = false;
        public bool ReturnTransaction
        {
            get { return _returnTransaction; }
            set
            {
                _returnTransaction = value;
                OnPropertyChanged("SaleOrReturn");
            }
        }

        private string _saleOrReturn;
        public string SaleOrReturn
        {
            get
            {
                return _returnTransaction == true ? "DEVOLUCIÓN EN EFECTIVO" : "COBRAR EN EFECTIVO";
            } 
            set
            {
                _saleOrReturn = value;
                OnPropertyChanged();
            }
        }

        #region Enum Related Properties

        private IList<PaymentTypeEnum> _paymentTypes;
        public IList<PaymentTypeEnum> PaymentTypes
        {
            get { return Enum.GetValues(typeof(PaymentTypeEnum)).Cast<PaymentTypeEnum>().ToList(); }
            set
            {
                _paymentTypes = value;
                OnPropertyChanged();
            }
        }

        private IList<CurrencyTypeEnum> _currencyTypes;
        public IList<CurrencyTypeEnum> CurrencyTypes
        {
            get { return Enum.GetValues(typeof(CurrencyTypeEnum)).Cast<CurrencyTypeEnum>().ToList(); }
            set
            {
                _currencyTypes = value;
                OnPropertyChanged();
            }
        }

        private IList<CurrencyTypeEnum> _inventoryPriceCurrencyTypes;
        public IList<CurrencyTypeEnum> InventoryPriceCurrencyTypes
        {
            get { return Enum.GetValues(typeof(CurrencyTypeEnum)).Cast<CurrencyTypeEnum>().ToList(); }
            set
            {
                _inventoryPriceCurrencyTypes = value;
                OnPropertyChanged();
            }
        }

        private IList<CurrencyTypeEnum> _inventoryCostCurrencyTypes;
        public IList<CurrencyTypeEnum> InventoryCostCurrencyTypes
        {
            get { return Enum.GetValues(typeof(CurrencyTypeEnum)).Cast<CurrencyTypeEnum>().ToList(); }
            set
            {
                _inventoryCostCurrencyTypes = value;
                OnPropertyChanged();
            }
        }

        private IList<UserAccessLevelEnum> _userAccessLevelTypes;
        public IList<UserAccessLevelEnum> UserAccessLevelTypes
        {
            get { return Enum.GetValues(typeof(UserAccessLevelEnum)).Cast<UserAccessLevelEnum>().ToList(); }
            set
            {
                _userAccessLevelTypes = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Products Page Titles

        public string PageOneTitle
        {
            get
            {
                return _pageOneTitle;
            }
            set
            {
                _pageOneTitle = value;
                OnPropertyChanged();
            }
        }

        public string PageTwoTitle
        {
            get
            {
                return _pageTwoTitle;
            }
            set
            {
                _pageTwoTitle = value;
                OnPropertyChanged();
            }
        }

        public string PageThreeTitle
        {
            get
            {
                return _pageThreeTitle;
            }
            set
            {
                _pageThreeTitle = value;
                OnPropertyChanged();
            }
        }

        public string PageFourTitle
        {
            get
            {
                return _pageFourTitle;
            }
            set
            {
                _pageFourTitle = value;
                OnPropertyChanged();
            }
        }

        public string PageFiveTitle
        {
            get
            {
                return _pageFiveTitle;
            }
            set
            {
                _pageFiveTitle = value;
                OnPropertyChanged();
            }
        }

        public int GroupSquaresAvailable
        {
            get { return Constants.MaxNumberListItems - CurrentPageListProducts.Count;}
            set
            {
                _groupSquaresAvailable = value; 
                OnPropertyChanged();
            }
        }
        #endregion

        #region Inventory Related Properties

        private BitmapImage _productImage;
        public BitmapImage ProductImage
        {
            get
            {
                //return SelectedInventoryProduct == null ? null : SelectedInventoryProduct.Image;
                return InventoryTemporalItem == null ? null : InventoryTemporalItem.Image;
            }
            set
            {
                _productImage = value;
                OnPropertyChanged();
            }
        }

        private string _inventorySearchText;
        public string InventorySearchText
        {
            get
            {
                return _inventorySearchText;
            }
            set
            {
                _inventorySearchText = value.ToLower();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hold active cart items list
        /// </summary>
        /// 
        private ObservableCollection<ProductBase> _inventorySearchedProducts;

        public ObservableCollection<ProductBase> InventorySearchedProducts
        {
            get { return _inventorySearchedProducts; }
            set
            {
                _inventorySearchedProducts = value;
                OnPropertyChanged();
            }
        }

        private ProductBase _selectedInventoryProduct;
        public ProductBase SelectedInventoryProduct
        {
            get { return _selectedInventoryProduct; }
            set
            {
                _selectedInventoryProduct = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Vendor Related Properties
        private Vendor _vendorTemporalItem;
        public Vendor VendorTemporalItem
        {
            get
            {
                return _vendorTemporalItem;
            }
            set
            {
                _vendorTemporalItem = value;
                OnPropertyChanged();
            }
        }

        private string _vendorsSearchText;
        public string VendorsSearchText
        {
            get
            {
                return _vendorsSearchText;
            }
            set
            {
                _vendorsSearchText = value.ToLower();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hold active cart items list
        /// </summary>
        /// 
        private ObservableCollection<Vendor> _vendorsSearchedEntries;

        public ObservableCollection<Vendor> VendorsSearchedEntries
        {
            get { return _vendorsSearchedEntries; }
            set
            {
                _vendorsSearchedEntries = value;
                OnPropertyChanged();
            }
        }

        private Vendor _selectedVendor;
        public Vendor SelectedVendor
        {
            get { return _selectedVendor; }
            set
            {
                _selectedVendor = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Users Related Properties
        private User _userTemporalItem;
        public User UserTemporalItem
        {
            get
            {
                return _userTemporalItem;
            }
            set
            {
                _userTemporalItem = value;
                OnPropertyChanged();
            }
        }

        private string _usersSearchText;
        public string UsersSearchText
        {
            get
            {
                return _usersSearchText;
            }
            set
            {
                _usersSearchText = Formatter.SanitizeInput(value.ToLower());
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hold active cart items list
        /// </summary>
        /// 
        private ObservableCollection<User> _usersSearchedEntries;

        public ObservableCollection<User> UsersSearchedEntries
        {
            get { return _usersSearchedEntries; }
            set
            {
                _usersSearchedEntries = value;
                OnPropertyChanged();
            }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Customers Related Properties
        private Customer _customerTemporalItem;
        public Customer CustomerTemporalItem
        {
            get
            {
                return _customerTemporalItem;
            }
            set
            {
                _customerTemporalItem = value;
                OnPropertyChanged();
            }
        }

        private string _customersSearchText;
        public string CustomersSearchText
        {
            get
            {
                return _customersSearchText;
            }
            set
            {
                _customersSearchText = value.ToLower();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hold active cart items list
        /// </summary>
        /// 
        private ObservableCollection<Customer> _customersSearchedEntries;

        public ObservableCollection<Customer> CustomersSearchedEntries
        {
            get { return _customersSearchedEntries; }
            set
            {
                _customersSearchedEntries = value;
                OnPropertyChanged();
            }
        }

        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
            }
        }

        public Customer CurrentCustomer
        {
            get { return _currentCustomer; }
            set
            {
                _currentCustomer = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Expenses Related Properties

        private Expense _expenseTemporalItem;
        public Expense ExpenseTemporalItem
        {
            get
            {
                return _expenseTemporalItem;
            }
            set
            {
                _expenseTemporalItem = value;
                OnPropertyChanged();
            }
        }

        private string _expensesSearchText;
        public string ExpensesSearchText
        {
            get
            {
                return _expensesSearchText;
            }
            set
            {
                _expensesSearchText = value.ToLower();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hold active cart items list
        /// </summary>
        /// 
        private ObservableCollection<Expense> _expensesSearchedEntries;

        public ObservableCollection<Expense> ExpensesSearchedEntries
        {
            get { return _expensesSearchedEntries; }
            set
            {
                _expensesSearchedEntries = value;
                OnPropertyChanged();
            }
        }

        private Expense _selectedExpense;
        public Expense SelectedExpense
        {
            get { return _selectedExpense; }
            set
            {
                _selectedExpense = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Orders Related Properties

        private Order _orderTemporalItem;

        public Order OrderTemporalItem
        {
            get { return _orderTemporalItem; }
            set
            {
                _orderTemporalItem = value;
                OnPropertyChanged();
            }
        }
        
        private string _ordersSearchText;
        public string OrdersSearchText
        {
            get
            {
                return _ordersSearchText;
            }
            set
            {
                _ordersSearchText = value.ToLower();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Hold search results
        /// </summary>
        /// 
        private ObservableCollection<Order> _ordersSearchedEntries;

        public ObservableCollection<Order> OrdersSearchedEntries
        {
            get { return _ordersSearchedEntries; }
            set
            {
                _ordersSearchedEntries = value;
                OnPropertyChanged();
            }
        }

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get { return _selectedOrder; }
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage _orderImage;
        public BitmapImage OrderImage
        {
            get
            {
                //return SelectedInventoryProduct == null ? null : SelectedInventoryProduct.Image;
                return OrderTemporalItem == null ? null : OrderTemporalItem.Image;
            }
            set
            {
                _orderImage = value;
                OnPropertyChanged();
            }
        }

        private int _progressBarValue = 0;

        public int ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                OnPropertyChanged();
            }
        }

        public string _progressMessage;

        public string ProgressMessage
        {
            get { return _progressMessage; }
            set
            {
                _progressMessage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Transaction Related Properties

        private Transaction _transactionTemporalItem;

        public Transaction TransactionTemporalItem
        {
            get { return _transactionTemporalItem; }
            set
            {
                _transactionTemporalItem = value;
                OnPropertyChanged();
            }
        }

        private string _transactionsSearchText;
        public string TransactionsSearchText
        {
            get
            {
                return _transactionsSearchText;
            }
            set
            {
                _transactionsSearchText = value.ToLower();
                OnPropertyChanged();
            }            
        }


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
                    TransactionFileTypeContent = "X";
                }
            }
        }
        /// <summary>
        /// Hold search results
        /// </summary>
        /// 
        private ObservableCollection<Transaction> _transactionsSearchedEntries;

        public ObservableCollection<Transaction> TransactionsSearchedEntries
        {
            get { return _transactionsSearchedEntries; }
            set
            {
                _transactionsSearchedEntries = value;
                OnPropertyChanged();
            }
        }

        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get { return _selectedTransaction; }
            set
            {
                _selectedTransaction = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Exchange Rate Related Properties

        public decimal ExchangeRate
        {
            get
            {
                return _exchangeRate;
            }
            set
            {
                _exchangeRate = value;
                OnPropertyChanged("ExchangeRate");
            }
        }

        public string ExchangeRateString
        {
            get
            {
                return _posInstance.ExchangeRate.ToString(CultureInfo.CurrentCulture);
            }
            set
            {
                _exchangeRateString = value;
                OnPropertyChanged("ExchangeRateString");
            }
        }
        #endregion

        #region Login Related Properties

        private string _loginUserNameText;
        public string LoginUserNameText
        {
            get
            {
                return _loginUserNameText;
            }
            set
            {
                _loginUserNameText = value.ToLower();
                OnPropertyChanged("LoginUserNameText");
            }
        }

        private string _loginPasswordText;
        public string LoginPasswordText
        {
            get
            {
                return _loginPasswordText;
            }
            set
            {
                _loginPasswordText = value.ToLower();
                OnPropertyChanged("UserLoginText");
            }
        }

        private string _loginMessage;
        public string LoginMessage
        {
            get
            {
                return _loginMessage;
            }
            set
            {
                _loginMessage = value.ToLower();
                OnPropertyChanged("LoginMessage");
            }
        }

        #endregion

        #endregion

        #region Commands

        #region ChangePageCommand

        public ICommand ChangePageCommand { get { return _changePageCommand ?? (_changePageCommand = new DelegateCommand(Execute_ChangePageCommand, CanExecute_ChangePageCommand)); } }
        private ICommand _changePageCommand;

        internal void Execute_ChangePageCommand(object parameter)
        {
            string pageTitleHolder;
            TransactionType transactionType;
            ClearSearchLists();
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "general":
                    CurrentPage = "\\View\\PosGeneralPage.xaml";
                    break;
                case "payment":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Pago Iniciado Cantidad: " + PaymentTotalMXN);
                    PaymentCustomerSearchInput = "";
                    ClearPaymentInput();
                    CurrentCustomer = null;
                    CurrentPage = "\\View\\PaymentPage.xaml";
                    break;
                case "remove_inventory":
                    CurrentPage = "\\View\\RemoveInventoryPage.xaml";
                    break;
                case "end_remove_inventory":
                    transactionType = TransactionType.Remover;
                    ProcessInventoryRemoval(transactionType);
                    CurrentPage = "\\View\\PosGeneralPage.xaml";
                    break;
                case "product_list_1":
                    LastSelectedProductsPage = 1;
                    ProductObjects = GetPageProductsList(LastSelectedProductsPage, out pageTitleHolder);
                    PageOneTitle = pageTitleHolder;
                    CurrentPage = "\\View\\ProductsPage.xaml";
                    break;
                case "product_list_2":
                    LastSelectedProductsPage = 2;
                    ProductObjects = GetPageProductsList(LastSelectedProductsPage, out pageTitleHolder);
                    PageTwoTitle = pageTitleHolder;
                    CurrentPage = "\\View\\ProductsPage.xaml";
                    break;
                case "product_list_3":
                    LastSelectedProductsPage = 3;
                    ProductObjects = GetPageProductsList(LastSelectedProductsPage, out pageTitleHolder);
                    PageThreeTitle = pageTitleHolder;
                    CurrentPage = "\\View\\ProductsPage.xaml";
                    break;
                case "product_list_4":
                    LastSelectedProductsPage = 4;
                    ProductObjects = GetPageProductsList(LastSelectedProductsPage, out pageTitleHolder);
                    PageFourTitle = pageTitleHolder;
                    CurrentPage = "\\View\\ProductsPage.xaml";
                    break;
                case "product_list_5":
                    LastSelectedProductsPage = 5;
                    ProductObjects = GetPageProductsList(LastSelectedProductsPage, out pageTitleHolder);
                    PageFiveTitle = pageTitleHolder;
                    CurrentPage = "\\View\\ProductsPage.xaml";
                    break;
                case "menu":
                    CurrentPage = "\\View\\PosMenuPage.xaml";
                    break;
                case "inventory":
                case "inventory_cancel":
                    CurrentPage = "\\View\\InventoryMainPage.xaml";
                    SelectedInventoryProduct = null;
                    break;
                case "sales_report":
                    CurrentPage = "\\View\\EndSalesPage.xaml";
                    break;
                case "product_list":
                    GetPageProductsList(LastSelectedProductsPage, out var currentProductListTitle);
                    CurrentPage = "\\View\\ProductsListEditPage.xaml";
                    break;
                case "inventory_add":
                    CurrentPage = "\\View\\InventoryItemPage.xaml";
                    SelectedInventoryProduct = null;
                    break;
                case "inventory_details":
                    CurrentPage = "\\View\\InventoryItemPage.xaml";
                    break;
                case "system":
                    CurrentPage = "\\View\\SystemPage.xaml";
                    break;
                case "customers":
                    CurrentPage = "\\View\\CustomerMainPage.xaml";
                    break;
                case "vendors":
                    CurrentPage = "\\View\\VendorMainPage.xaml";
                    break;
                case "users":
                    CurrentPage = "\\View\\UserMainPage.xaml";
                    break;
                case "orders":
                    CurrentPage = "\\View\\OrderMainPage.xaml";
                    break;
                case "exchange_rate":
                    CurrentPage = "\\View\\ExchangeRatePage.xaml";
                    break;
                case "calculator":
                    CurrentPage = "\\View\\PosGeneralPage.xaml";
                    System.Diagnostics.Process.Start("Calc.exe");
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Inicio de Calculadora");
                    break;
                case "users_guide":
                    CurrentPage = "\\View\\PosGeneralPage.xaml";
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Guia de Usuario Inicio");
                    System.Diagnostics.Process.Start(@"C:\Projects\seiya-pos\Resources\UsersGuide\GuiaUsuario.pdf");
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Guia de Usuario Completado");
                    break;
                case "categories_edit":
                    CurrentCategoryList = new ObservableCollection<string>(CategoryCatalog.GetList(Constants.DataFolderPath + Constants.CategoryListFileName));
                    NewCategoryItem = "";
                    CurrentPage = "\\View\\CategoryListPage.xaml";
                    break;
                case "support":
                    CurrentPage = "\\View\\TechSupportPage.xaml";
                    break;
                case "more_options":
                    CurrentPage = "";
                    break;
                case "expenses":
                    CurrentPage = "\\View\\ExpenseMainPage.xaml";
                    break;
                case "returns":
                    CurrentPage = "\\View\\ReturnsPage.xaml";
                    break;
                case "analysis":
                    CurrentPage = "\\View\\AnalysisMainPage.xaml";
                    break;
                case "transactions":
                    CurrentPage = "\\View\\TransactionMainPage.xaml";
                    break;
                case "login":
                    CurrentUser = null;
                    SystemUnlock = false;
                    CurrentPage = "\\View\\LoginPage.xaml";
                    break;
                case "Efectivo":
                    if (PaymentTotalMXN != 0M && (PaymentTotalMXN <= (PaymentReceivedMXN + PaymentReceivedUSD * _exchangeRate)))
                    {
                        //TODO: Add internal capability for personal use
                        transactionType = PaymentTotalMXN <= 150 ? TransactionType.Regular : TransactionType.Interno;
                        PaymentProcessStart(parameter.ToString(), transactionType);
                        SystemUnlock = false;
                        CurrentPage = "\\View\\PaymentEndPage.xaml";
                        //Log
                        Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Pago Terminado en Efectivo");
                    }
                    else
                    {
                        Code = "Efectivo Inválido";
                        CodeColor = Constants.ColorCodeError;
                    }
                    break;
                case "Tarjeta":
                    transactionType = TransactionType.Regular;
                    PaymentProcessStart(parameter.ToString(), transactionType);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Pago Terminado en Tarjeta");
                    SystemUnlock = false;
                    CurrentPage = "\\View\\PaymentEndPage.xaml";
                    break;
                case "partial_start":
                    PaymentCustomerSearchInput = "";
                    ClearPaymentInput();
                    CurrentPage = "\\View\\PaymentPartialPage.xaml";
                    break;

                case "Parcial":
                    if (PaymentTotalMXN != 0M && (PaymentTotalMXN <= (PaymentReceivedMXN + PaymentReceivedUSD * _exchangeRate)))
                    {
                        transactionType = TransactionType.Regular;
                        PaymentProcessStart(parameter.ToString(), transactionType);
                        //Log
                        Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Pago Terminado en Parcial");
                        SystemUnlock = false;
                        CurrentPage = "\\View\\PaymentEndPage.xaml";
                    }
                    else
                    {
                        Code = "Pago Inválido";
                        CodeColor = Constants.ColorCodeError;
                    }
                    break;
                case "Transferencia":
                case "Cheque":
                    if (!ReturnTransaction)
                    {
                        transactionType = TransactionType.Regular;
                        PaymentProcessStart(parameter.ToString(), transactionType);
                        //Log
                        Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Pago Terminado en Cheque");
                        SystemUnlock = false;
                        CurrentPage = "\\View\\PaymentEndPage.xaml";
                    }
                    else
                    {
                        Code = "No Permitido";
                        CodeColor = Constants.ColorCodeError;
                    }
                    break;
                case "end_transaction":
                    PaymentEndProcess(parameter.ToString());
                    Code = "Transaccion Exitosa";
                    CodeColor = Constants.ColorCodeSave;
                    SystemUnlock = true;
                    CurrentPage = "\\View\\PosGeneralPage.xaml";
                    break;
                case "save_transactions":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Transacciones Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.TransactionsMasterFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Transacciones Exportacion Completada");
                    break;
                case "others":
                    CurrentPage = "\\View\\CarRegistrationMainPage.xaml";
                    break;
               
            }
        }

        internal bool CanExecute_ChangePageCommand(object parameter)
        {
            return SystemUnlock || parameter.ToString() == "end_transaction";
        }
        #endregion

        #region LoginPageCommand

        public ICommand LoginPageCommand { get { return _loginPageCommand ?? (_loginPageCommand = new DelegateCommand(Execute_LoginPageCommand, CanExecute_LoginPageCommand)); } }
        private ICommand _loginPageCommand;

        internal void Execute_LoginPageCommand(object parameter)
        {
            ClearSearchLists();
            if ((string) parameter != "login") return;
            CurrentUser = null;
            SystemUnlock = false;
            CurrentPage = "\\View\\LoginPage.xaml";
        }

        internal bool CanExecute_LoginPageCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region CartNumberClickCommand
        private ICommand _cartNumberClickCommand;
        public ICommand CartNumberClickCommand { get { return _cartNumberClickCommand ?? (_cartNumberClickCommand = new DelegateCommand(Execute_CartNumberClickCommand, CanExecute_CartNumberClickCommand)); } }
        
        internal void Execute_CartNumberClickCommand(object parameter)
        {
            switch ((string)parameter)
            {
                case "1":
                    CurrentCartNumber = 1;
                    PaymentReceivedMXN = 0;
                    PaymentReceivedUSD = 0;
                    //Update button colors  
                    CurrentCartProducts = _cartOneProducts;
                    PaymentUpdateRemaining();
                    break;
                case "2":
                    CurrentCartNumber = 2;
                    PaymentReceivedMXN = 0;
                    PaymentReceivedUSD = 0;
                    CurrentCartProducts = _cartTwoProducts;
                    PaymentUpdateRemaining();
                    break;
                case "3":
                    CurrentCartNumber = 3;
                    PaymentReceivedMXN = 0;
                    PaymentReceivedUSD = 0;
                    CurrentCartProducts = _cartThreeProducts;
                    PaymentUpdateRemaining();
                    break;
            }
        }

        internal bool CanExecute_CartNumberClickCommand(object parameter)
        {
            var cartNumber = 0;
            Int32.TryParse((string)parameter, out cartNumber);
            if (cartNumber == CurrentCartNumber)
            {
                return false;
            }
            return true;
        }
       

        #endregion

        #region SampleClickCommand
        public ICommand ClickCommand { get { return _clickCommand ?? (_clickCommand = new DelegateCommand(Execute_ClickCommand, CanExecute_ClickCommand));} }
        private ICommand _clickCommand;

        internal void Execute_ClickCommand(object parameter)
        {
            CurrentPage = "\\View\\PosGeneralPage.xaml";
        }
        internal bool CanExecute_ClickCommand(object parameter)

        {
            return true;
        }
        #endregion

        #region Search Code Related Commands

        #region SearchCodeCommand
        public ICommand SearchCodeCommand { get { return _searchCodeCommand ?? (_searchCodeCommand = new DelegateCommand(Execute_SearchCodeCommand, CanExecute_SearchCodeCommand)); } }
        private ICommand _searchCodeCommand;

        internal void Execute_SearchCodeCommand(object parameter)
        {
            var product = _inventoryInstance.GetProduct((string)parameter + "x");
            if (product.Code != null)
            {
                product.LastQuantitySold = 1;
                AddProductToCart(product);
                Code = "";
            }
            else
            {
                Code = "Código Inválido";
                CodeColor = Constants.ColorCodeError;
            }

            //TODO: Turn red if it is not found

        }
        internal bool CanExecute_SearchCodeCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region CodeLeftClickClearCommand
        public ICommand CodeLeftClickClearCommand { get { return _codeLeftClickClearCommand ?? (_codeLeftClickClearCommand = new DelegateCommand(Execute_CodeLeftClickClearCommand, CanExecute_CodeLeftClickClearCommand)); } }
        private ICommand _codeLeftClickClearCommand;

        internal void Execute_CodeLeftClickClearCommand(object parameter)
        {
            Code = "";           
        }
        internal bool CanExecute_CodeLeftClickClearCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        #region SubtractFromProductListCommand
        public ICommand SubtractFromProductListCommand { get { return _subtractFromProductListCommand ?? (_subtractFromProductListCommand = new DelegateCommand(Execute_SubtractFromProductListCommand, CanExecute_SubtractFromProductListCommand)); } }
        private ICommand _subtractFromProductListCommand;

        internal void Execute_SubtractFromProductListCommand(object parameter)
        {
            var index = CurrentCartProducts.IndexOf((ProductBase)parameter);
            var activeProduct = (ProductBase)parameter;
            CurrentCartProducts.RemoveAt(index);
            activeProduct.LastQuantitySold -= 1;
            if (activeProduct.LastQuantitySold > 0)
            {
                CurrentCartProducts.Insert(index, activeProduct);
                SelectedCartProduct = activeProduct;
            }
            PaymentTotalMXN = CalculateCurrentCartTotal();
        }
        internal bool CanExecute_SubtractFromProductListCommand(object parameter)
        {
            return parameter != null;
        }
        #endregion

        #region AddToProductListCommand
        public ICommand AddToProductListCommand { get { return _addToProductListCommand ?? (_addToProductListCommand = new DelegateCommand(Execute_AddToProductListCommand, CanExecute_AddToProductListCommand)); } }
        private ICommand _addToProductListCommand;

        internal void Execute_AddToProductListCommand(object parameter)
        {
            var index = CurrentCartProducts.IndexOf((ProductBase)parameter);
            var activeProduct = (ProductBase)parameter;

            CurrentCartProducts.RemoveAt(index);
            activeProduct.LastQuantitySold += 1;
            CurrentCartProducts.Insert(index, activeProduct);
            SelectedCartProduct = activeProduct;
            PaymentTotalMXN = CalculateCurrentCartTotal();
        }
        internal bool CanExecute_AddToProductListCommand(object parameter)
        {
             return parameter != null;
        }

        #endregion

        #region ApplyDiscountToProductListCommand

        public ICommand ApplyDiscountToProductListCommand { get { return _applyDiscountToProductListCommand ?? (_applyDiscountToProductListCommand = new DelegateCommand(Execute_ApplyDiscountToProductListCommand, CanExecute_ApplyDiscountToProductListCommand)); } }
        private ICommand _applyDiscountToProductListCommand;

        internal void Execute_ApplyDiscountToProductListCommand(object parameter)
        {
            var index = CurrentCartProducts.IndexOf((ProductBase)parameter);
            var activeProduct = (ProductBase)parameter;
            CurrentCartProducts.RemoveAt(index);
            activeProduct.Price = Math.Round(activeProduct.Price*(1 - DiscountPercent), 2);
            CurrentCartProducts.Insert(index, activeProduct);
            SelectedCartProduct = activeProduct;
            PaymentTotalMXN = CalculateCurrentCartTotal();
        }
        internal bool CanExecute_ApplyDiscountToProductListCommand(object parameter)
        {
            return parameter != null;
        }

        #endregion

        #region ClearCurrentProductListCommand

        public ICommand ClearCurrentProductListCommand { get { return _clearCurrentProductListCommand ?? (_clearCurrentProductListCommand = new DelegateCommand(Execute_ClearCurrentProductListCommand, CanExecute_ClearCurrentProductListCommand)); } }
        private ICommand _clearCurrentProductListCommand;

        internal void Execute_ClearCurrentProductListCommand(object parameter)
        {
            CurrentCartProducts.Clear();
            PaymentTotalMXN = 0M;
        }
        internal bool CanExecute_ClearCurrentProductListCommand(object parameter)
        {
            return CurrentCartProducts.Count > 0;
        }

        #endregion

        #region SelectProductsListCommand
        private ICommand _selectProductsListCommand;
        public ICommand SelectProductsListCommand { get { return _selectProductsListCommand ?? (_selectProductsListCommand = new DelegateCommand(Execute_SelectProductsListCommand, CanExecute_SelectProductsListCommand)); } }

        internal void Execute_SelectProductsListCommand(object parameter)
        {
            var currentProductListTitle = string.Empty;
            List<ProductBase> items;

            switch ((string)parameter)
            {
                case "1":
                    items = GetPageProductsList(1, out currentProductListTitle).ToList();
                    LastSelectedProductsPage = 1;
                    break;
                case "2":
                    items = GetPageProductsList(2, out currentProductListTitle).ToList();
                    LastSelectedProductsPage = 2;
                    break;
                case "3":
                    items = GetPageProductsList(3, out currentProductListTitle).ToList();
                    LastSelectedProductsPage = 3;
                    break;
                case "4":
                    items = GetPageProductsList(4, out currentProductListTitle).ToList();
                    LastSelectedProductsPage = 4;
                    break;
                case "5":
                    items = GetPageProductsList(5, out currentProductListTitle).ToList();
                    LastSelectedProductsPage = 5;
                    break;
            }
        }

        internal bool CanExecute_SelectProductsListCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region SelectItemCommand

        public ICommand SelectItemCommand { get { return _selectItemCommand ?? (_selectItemCommand = new DelegateCommand(Execute_SelectItemCommand, CanExecute_SelectItemCommand)); } }
        private ICommand _selectItemCommand;

        internal void Execute_SelectItemCommand(object parameter)
        {
            //TODO: Check to make sure the item is found, otherwise show error message
            //Create a new object for every product 
            var product = new ProductBase((ProductBase) parameter) {LastQuantitySold = 1};
            AddProductToCart(product);
        }

        internal bool CanExecute_SelectItemCommand(object parameter)
        {
            return parameter != null;
        }

        #endregion

        #region AddItemToGeneralCommand

        public ICommand AddItemToGeneralCommand { get { return _addItemToGeneralCommand ?? (_addItemToGeneralCommand = new DelegateCommand(Execute_AddItemToGeneralCommand, CanExecute_Execute_AddItemToGeneralCommand)); } }
        private ICommand _addItemToGeneralCommand;

        internal void Execute_AddItemToGeneralCommand(object parameter)
        {
            //TODO: Check to make sure the item is found, otherwise show error message
            //Create a new object for every product 
            var product = new ProductBase((ProductBase)parameter) { LastQuantitySold = 1 };
            AddManualProductToCart(product);
        }

        internal bool CanExecute_Execute_AddItemToGeneralCommand(object parameter)
        {
            return parameter != null;
        }

        #endregion

        #region PaymentBillClickCommand
        public ICommand PaymentBillClickCommand { get { return _paymentBillClickCommand ?? (_paymentBillClickCommand = new DelegateCommand(Execute_PaymentBillClickCommand, CanExecute_PaymentBillClickCommand)); } }
        private ICommand _paymentBillClickCommand;

        internal void Execute_PaymentBillClickCommand(object parameter)
        {
            switch ((string)parameter)
            {
                case "exact_mxn":
                    PaymentReceivedMXN = PaymentTotalMXN;
                    break;
                case "20_mxn":
                    PaymentReceivedMXN += 20M;
                    break;
                case "50_mxn":
                    PaymentReceivedMXN += 50M;
                    break;
                case "100_mxn":
                    PaymentReceivedMXN += 100M;
                    break;
                case "200_mxn":
                    PaymentReceivedMXN += 200M;
                    break;
                case "500_mxn":
                    PaymentReceivedMXN += 500M;
                    break;
                case "exact_usd":
                    PaymentReceivedUSD = PaymentTotalUSD;
                    break;
                case "1_usd":
                    PaymentReceivedUSD += 1M;
                    break;
                case "5_usd":
                    PaymentReceivedUSD += 5M;
                    break;
                case "10_usd":
                    PaymentReceivedUSD += 10M;
                    break;
                case "20_usd":
                    PaymentReceivedUSD += 20M;
                    break;
                case "50_usd":
                    PaymentReceivedUSD += 50M;
                    break;
                case "100_usd":
                    PaymentReceivedUSD += 100M;
                    break;
            }
        }
        internal bool CanExecute_PaymentBillClickCommand(object parameter)
        {
            return ReturnTransaction == false;
        }
        #endregion

        #region Payment Processing Commands

        #region PaymentUsePointsCommand

        public ICommand PaymentUsePointsCommand { get { return _paymentUsePointsCommand ?? (_paymentUsePointsCommand = new DelegateCommand(Execute_PaymentUsePointsCommand, CanExecute_PaymentUsePointsCommand)); } }
        private ICommand _paymentUsePointsCommand;

        internal void Execute_PaymentUsePointsCommand(object parameter)
        {
            if (CurrentCustomer.PointsAvailable >= 1)
            {
                ProductBase productMimic;
                var tempTotal = CalculateCurrentCartTotal();
                if (CurrentCustomer.PointsAvailable > Convert.ToDouble(tempTotal))
                {
                    productMimic = ProductBase.Add("Puntos Descuento", "Puntos", Convert.ToDecimal(tempTotal - 1) * -1, 1);
                    PaymentPointsInUse = Convert.ToDouble(tempTotal - 1);
                }
                else
                {
                    productMimic = ProductBase.Add("Puntos Descuento", "Puntos", Convert.ToDecimal(CurrentCustomer.PointsAvailable) * -1, 1);
                    PaymentPointsInUse = CurrentCustomer.PointsAvailable;
                }
                AddManualProductToCart(productMimic);
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Puntos Utilizados: " + PaymentPointsInUse);
        }

        internal bool CanExecute_PaymentUsePointsCommand(object parameter)
        {
            return CurrentCustomer != null && ReturnTransaction == false;
        }
        #endregion
        
        #region PaymentCashProcessCommand

        public ICommand PaymentCashProcessCommand { get { return _paymentCashProcessCommand ?? (_paymentCashProcessCommand = new DelegateCommand(Execute_PaymentCashProcessCommand, CanExecute_PaymentCashProcessCommand)); } }
        private ICommand _paymentCashProcessCommand;

        internal void Execute_PaymentCashProcessCommand(object parameter)
        {
            //Get payment type from string
            var status = PaymentTypeEnum.TryParse(parameter.ToString(), out PaymentTypeEnum paymentType);
            if (status == false)
                paymentType = PaymentTypeEnum.Efectivo;

            //TODO: Review if needed
            ProcessPayment(paymentType, TransactionType.Regular);

            if (paymentType == PaymentTypeEnum.Efectivo)
            {
                PaymentChangeMXN = Math.Round((PaymentReceivedMXN + PaymentReceivedUSD * ExchangeRate) - PaymentTotalMXN, 2);
                PaymentChangeUSD = Math.Round(PaymentChangeMXN / ExchangeRate, 2);
            }
            else
            {
                PaymentChangeMXN = 0;
                PaymentChangeUSD = 0;
            }

            PaymentPointsReceived = Convert.ToDouble(PaymentTotalMXN * PointsConvertionRatio);

            CurrentCartProducts.Clear();
            CurrentPage = "\\View\\PaymentEndPage.xaml";
        }
        internal bool CanExecute_PaymentCashProcessCommand(object parameter)
        {
            return PaymentTotalMXN != 0M && PaymentTotalMXN <= PaymentReceivedMXN + PaymentReceivedUSD * _exchangeRate;
        }
        #endregion

        #region PaymentProcessCommand

        public ICommand PaymentProcessCommand { get { return _paymentProcessCommand ?? (_paymentProcessCommand = new DelegateCommand(Execute_PaymentProcessCommand, CanExecute_PaymentProcessCommand)); } }
        private ICommand _paymentProcessCommand;

        internal void Execute_PaymentProcessCommand(object parameter)
        {
            //Get payment type from string
            var status = PaymentTypeEnum.TryParse(parameter.ToString(), out PaymentTypeEnum paymentType);
            if (status == false)
                paymentType = PaymentTypeEnum.Efectivo;

            //TODO: Review if needed
            ProcessPayment(paymentType, TransactionType.Regular);

            if (paymentType == PaymentTypeEnum.Efectivo)
            {
                PaymentChangeMXN = Math.Round((PaymentReceivedMXN + PaymentReceivedUSD * ExchangeRate) - PaymentTotalMXN, 2);
                PaymentChangeUSD = Math.Round(PaymentChangeMXN / ExchangeRate, 2);
            }
            else
            {
                PaymentChangeMXN = 0;
                PaymentChangeUSD = 0;
            }

            PaymentPointsReceived = Convert.ToInt32(PaymentTotalMXN * PointsConvertionRatio);

            CurrentCartProducts.Clear();
            PaymentTotalMXN = 0;
            CurrentPage = "\\View\\PaymentEndPage.xaml";
            
        }
        internal bool CanExecute_PaymentProcessCommand(object parameter)
        {
            return PaymentTotalMXN > 0;
        }
        #endregion

        #region PaymentEndCommand

        public ICommand PaymentEndCommand { get { return _paymentEndCommand ?? (_paymentEndCommand = new DelegateCommand(Execute_PaymentEndCommand, CanExecute_PaymentEndCommand)); } }
        private ICommand _paymentEndCommand;

        internal void Execute_PaymentEndCommand(object parameter)
        {
            //Clear all properties and return to general page
            PaymentTotalMXN = 0;
            PaymentTotalUSD = 0;
            PaymentReceivedMXN = 0;
            PaymentReceivedUSD = 0;
            PaymentChangeMXN = 0;
            PaymentChangeUSD = 0;
            PaymentPointsReceived = 0;
            CurrentCustomer = null;
            CurrentPage = "\\View\\PosGeneralPage.xaml";
        }
        internal bool CanExecute_PaymentEndCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region PaymentCustomerSearchCommand

        public ICommand PaymentCustomerSearchCommand { get { return _paymentCustomerSearchCommand ?? (_paymentCustomerSearchCommand = new DelegateCommand(Execute_PaymentCustomerSearchCommand, CanExecute_PaymentCustomerSearchCommand)); } }
        private ICommand _paymentCustomerSearchCommand;

        internal void Execute_PaymentCustomerSearchCommand(object parameter)
        {
            //Inventory search method that returns a list of products for the datagrid
            CustomersSearchedEntries = new ObservableCollection<Customer>(new Customer(Constants.DataFolderPath + Constants.CustomersFileName).Search(PaymentCustomerSearchInput.ToString()));
            if(CustomersSearchedEntries.Count > 0)
                CurrentCustomer = CustomersSearchedEntries.First();
            PaymentCustomerSearchInput = "";
        }
        internal bool CanExecute_PaymentCustomerSearchCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ChangeParcialPaymentCommand

        public ICommand ChangeParcialPaymentCommand { get { return _changeParcialPaymentCommand ?? (_changeParcialPaymentCommand = new DelegateCommand(Execute_ChangeParcialPaymentCommand, CanExecute_ChangeParcialPaymentCommand)); } }
        private ICommand _changeParcialPaymentCommand;

        internal void Execute_ChangeParcialPaymentCommand(object parameter)
        {
            if (PartialPaymentEnabled)
            {
                PartialPaymentEnabled = false;
            }
            else
            {
                PartialPaymentEnabled = true;
            }
        }

        internal bool CanExecute_ChangeParcialPaymentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #endregion

        #region Products List Edit Page Commands

        #region MoveUpListItemCommand
        public ICommand MoveUpListItemCommand { get { return _moveUpListItemCommand ?? (_moveUpListItemCommand = new DelegateCommand(Execute_MoveUpListItemCommand, CanExecute_MoveUpListItemCommand)); } }
        private ICommand _moveUpListItemCommand;

        internal void Execute_MoveUpListItemCommand(object parameter)
        {
            var updatedProductList = Utilities.MoveListItemUp(CurrentPageListProducts, (int)parameter);
            CurrentPageListProducts = new ObservableCollection<ProductBase>(updatedProductList);
        }
        internal bool CanExecute_MoveUpListItemCommand(object parameter)
        {
            return (int?) parameter >= 0;
        }
        #endregion

        #region MoveDownListItemCommand
        public ICommand MoveDownListItemCommand { get { return _moveDownListItemCommand ?? (_moveDownListItemCommand = new DelegateCommand(Execute_MoveDownListItemCommand, CanExecute_MoveDownListItemCommand)); } }
        private ICommand _moveDownListItemCommand;

        internal void Execute_MoveDownListItemCommand(object parameter)
        {
            var updatedProductList = Utilities.MoveListItemDown(CurrentPageListProducts, (int)parameter);
            CurrentPageListProducts = new ObservableCollection<ProductBase>(updatedProductList);
        }
        internal bool CanExecute_MoveDownListItemCommand(object parameter)
        {
            return (int?) parameter >= 0;
        }
        #endregion

        #region DeleteListItemCommand
        public ICommand DeleteListItemCommand { get { return _deleteListItemCommand ?? (_deleteListItemCommand = new DelegateCommand(Execute_DeleteListItemCommand, CanExecute_DeleteListItemCommand)); } }
        private ICommand _deleteListItemCommand;

        internal void Execute_DeleteListItemCommand(object parameter)
        {
            CurrentPageListProducts.RemoveAt((int)parameter);
            GroupSquaresAvailable = Constants.MaxNumberListItems - CurrentPageListProducts.Count;
        }
        internal bool CanExecute_DeleteListItemCommand(object parameter)
        {
            return (int?) parameter >= 0;
        }

        #region AddListItemCommand

        public ICommand AddListItemCommand { get { return _addListItemCommand ?? (_addListItemCommand = new DelegateCommand(Execute_AddListItemCommand, CanExecute_AddListItemCommand)); } }
        private ICommand _addListItemCommand;

        internal void Execute_AddListItemCommand(object parameter)
        {
            var productFound = _inventoryInstance.GetProduct(((TextBox) parameter).Text);
            if (productFound.Code != null && (CurrentPageListProducts.Count < Constants.MaxNumberListItems))
            {
                CurrentPageListProducts.Add(productFound);
                //Log
                Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Codigo:" + productFound.Code + " " + "Agregado a Grupo:" + CurrentPageListTitle);
                GroupSquaresAvailable = Constants.MaxNumberListItems - CurrentPageListProducts.Count;
            }
            else
            {
                Code = "Código Incorrecto";
                CodeColor = Constants.ColorCodeError;
            }

            ((TextBox) parameter).Text = "";
        }

        internal bool CanExecute_AddListItemCommand(object parameter)
        {
            return parameter != null;
        }

        #endregion

        #region SaveChangesProductListCommand
        public ICommand SaveChangesProductListCommand { get { return _saveChangesProductListCommand ?? (_saveChangesProductListCommand = new DelegateCommand(Execute_SaveChangesProductListCommand, CanExecute_SaveChangesProductListCommand)); } }

        private ICommand _saveChangesProductListCommand;

        internal void Execute_SaveChangesProductListCommand(object parameter)
        {
            ProductBase.UpdateProductListFile(parameter.ToString(), CurrentPageListProducts.ToList(), CurrentPageListTitle);
            if (LastSelectedProductsPage == 1)
                PageOneTitle = CurrentPageListTitle;
            else if (LastSelectedProductsPage == 2)
                PageTwoTitle = CurrentPageListTitle;
            else if (LastSelectedProductsPage == 3)
                PageThreeTitle = CurrentPageListTitle;
            else if (LastSelectedProductsPage == 4)
                PageFourTitle = CurrentPageListTitle;
            else if (LastSelectedProductsPage == 5)
                PageFiveTitle = CurrentPageListTitle;

            Code = "Grupo Actualizado";
            CodeColor = Constants.ColorCodeSave;
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Grupo Modificado:" + " " + CurrentPageListTitle);
        }
        internal bool CanExecute_SaveChangesProductListCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        #endregion

        #region Category Edit Page Commands

        #region MoveUpCategoryItemCommand
        public ICommand MoveUpCategoryItemCommand { get { return _moveUpCategoryItemCommand ?? (_moveUpCategoryItemCommand = new DelegateCommand(Execute_MoveUpCategoryItemCommand, CanExecute_MoveUpCategoryItemCommand)); } }
        private ICommand _moveUpCategoryItemCommand;

        internal void Execute_MoveUpCategoryItemCommand(object parameter)
        {
            var updatedCategoryList = Utilities.MoveListItemUp(CurrentCategoryList, (int)parameter);
            CurrentCategoryList = new ObservableCollection<string>(updatedCategoryList);
        }
        internal bool CanExecute_MoveUpCategoryItemCommand(object parameter)
        {
            return (int)parameter >= 0;
        }
        #endregion

        #region MoveDownCategoryItemCommand
        public ICommand MoveDownCategoryItemCommand { get { return _moveDownCategoryItemCommand ?? (_moveDownCategoryItemCommand = new DelegateCommand(Execute_MoveDownCategoryItemCommand, CanExecute_MoveDownCategoryItemCommand)); } }
        private ICommand _moveDownCategoryItemCommand;

        internal void Execute_MoveDownCategoryItemCommand(object parameter)
        {
            var updatedCategoryList = Utilities.MoveListItemDown(CurrentCategoryList, (int)parameter);
            CurrentCategoryList = new ObservableCollection<string>(updatedCategoryList);
        }
        internal bool CanExecute_MoveDownCategoryItemCommand(object parameter)
        {
            return (int)parameter >= 0;
        }
        #endregion

        #region DeleteCategoryItemCommand
        public ICommand DeleteCategoryItemCommand { get { return _deleteCategoryItemCommand ?? (_deleteCategoryItemCommand = new DelegateCommand(Execute_DeleteCategoryItemCommand, CanExecute_DeleteCategoryItemCommand)); } }
        private ICommand _deleteCategoryItemCommand;

        internal void Execute_DeleteCategoryItemCommand(object parameter)
        {
            if (CurrentCategoryList.Count > 1)
            {
                if (CurrentCategoryList[(int)parameter] != "general" && CurrentCategoryList[(int)parameter] != "General")
                {                   
                    CurrentCategoryList.RemoveAt((int)parameter);                    
                }
                else
                {
                    Code = "No es Posible Eliminar";
                    CodeColor = Constants.ColorCodeError;
                }
            }
        }
        internal bool CanExecute_DeleteCategoryItemCommand(object parameter)
        {
            return (int)parameter >= 0;
        }
        #endregion

        #region SaveChangesCategoryListCommand
        public ICommand SaveChangesCategoryListCommand { get { return _saveChangesCategoryListCommand ?? (_saveChangesCategoryListCommand = new DelegateCommand(Execute_SaveChangesCategoryListCommand, CanExecute_SaveChangesCategoryListCommand)); } }

        private ICommand _saveChangesCategoryListCommand;

        internal void Execute_SaveChangesCategoryListCommand(object parameter)
        {
            CategoryCatalog.UpdateCategoryListFile(Constants.DataFolderPath + Constants.CategoryListFileName, CurrentCategoryList.ToList());
            //Reload categories
            CategoriesList = new ObservableCollection<string>(CategoryCatalog.GetList(Constants.DataFolderPath + Constants.CategoryListFileName));
            PosGeneralPageViewModel.GetInstance().CategoriesList = CategoriesList;
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Categorias Actualizadas");
            Code = "Categorías Actualizadas";
            CodeColor = Constants.ColorCodeSave;
        }
        internal bool CanExecute_SaveChangesCategoryListCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region AddCategoryListCommand
        public ICommand AddCategoryListCommand { get { return _addCategoryListCommand ?? (_addCategoryListCommand = new DelegateCommand(Execute_AddCategoryListCommand, CanExecute_AddCategoryListCommand)); } }

        private ICommand _addCategoryListCommand;

        internal void Execute_AddCategoryListCommand(object parameter)
        {
            //Format the string before adding it to the list
            if (!CurrentCategoryList.Contains(Formatter.FirstLetterUpperConverter(NewCategoryItem)))
                CurrentCategoryList.Add(Formatter.FirstLetterUpperConverter(NewCategoryItem));
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Agregar Nueva Categoria:" + " " + NewCategoryItem);
            NewCategoryItem = "";
        }
        internal bool CanExecute_AddCategoryListCommand(object parameter)
        {
            return parameter != null && parameter.ToString() != "";
        }
        #endregion

        #endregion

        #region Inventory Commands

        #region InventorModifyItemCommand

        public ICommand InventorModifyItemCommand { get { return _inventorModifyItemCommand ?? (_inventorModifyItemCommand = new DelegateCommand(Execute_InventorModifyItemCommand, CanExecute_InventorModifyItemCommand)); } }
        private ICommand _inventorModifyItemCommand;

        internal void Execute_InventorModifyItemCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "inventory_details":
                    //Create new productt
                    var temporalProduct = new ProductBase()
                    {
                        Id = SelectedInventoryProduct.Id,
                        Code = SelectedInventoryProduct.Code,
                        AlternativeCode = SelectedInventoryProduct.AlternativeCode,
                        AmountSold = SelectedInventoryProduct.AmountSold,
                        Category = SelectedInventoryProduct.Category,
                        Cost = SelectedInventoryProduct.Cost,
                        CostCurrency = SelectedInventoryProduct.CostCurrency,
                        Description = SelectedInventoryProduct.Description,
                        Image = SelectedInventoryProduct.Image,
                        ImageName = SelectedInventoryProduct.ImageName,
                        InternalQuantity = SelectedInventoryProduct.InternalQuantity,
                        LastPurchaseDate = SelectedInventoryProduct.LastPurchaseDate,
                        LastQuantitySold = SelectedInventoryProduct.LastQuantitySold,
                        LastSaleDate = SelectedInventoryProduct.LastSaleDate,
                        LocalQuantityAvailable = SelectedInventoryProduct.LocalQuantityAvailable,
                        MinimumStockQuantity = SelectedInventoryProduct.MinimumStockQuantity,
                        Name = SelectedInventoryProduct.Name,
                        Price = SelectedInventoryProduct.Price,
                        PriceCurrency = SelectedInventoryProduct.PriceCurrency,
                        Provider = SelectedInventoryProduct.Provider,
                        ProviderProductId = SelectedInventoryProduct.ProviderProductId,
                        QuantitySold = SelectedInventoryProduct.QuantitySold,
                        TotalQuantityAvailable = SelectedInventoryProduct.TotalQuantityAvailable,
                        Brand = SelectedInventoryProduct.Brand
                    };

                    CurrentPage = "\\View\\InventoryItemPage.xaml";
                    InventoryTemporalItem = temporalProduct;
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Detalle de Producto:" + " " + SelectedInventoryProduct.Code);
                    break;

                case "inventory_add_clone":
                    //Create new productt
                    var temporalClonedProduct = new ProductBase()
                    {
                        Id = _inventoryInstance.GetLastItemNumber()+1,
                        Code = "",
                        AlternativeCode = "NA",
                        AmountSold = 0,
                        Category = SelectedInventoryProduct.Category,
                        Cost = SelectedInventoryProduct.Cost,
                        CostCurrency = SelectedInventoryProduct.CostCurrency,
                        Description = SelectedInventoryProduct.Description,
                        Image = SelectedInventoryProduct.Image,
                        ImageName = SelectedInventoryProduct.ImageName,
                        InternalQuantity = 0,
                        LastPurchaseDate = DateTime.Today,
                        LastQuantitySold = 0,
                        LastSaleDate = DateTime.Today,
                        LocalQuantityAvailable = 0,
                        MinimumStockQuantity = SelectedInventoryProduct.MinimumStockQuantity,
                        Name = SelectedInventoryProduct.Name,
                        Price = SelectedInventoryProduct.Price,
                        PriceCurrency = SelectedInventoryProduct.PriceCurrency,
                        Provider = SelectedInventoryProduct.Provider,
                        ProviderProductId = SelectedInventoryProduct.ProviderProductId,
                        QuantitySold = 0,
                        TotalQuantityAvailable = 0,
                        Brand = SelectedInventoryProduct.Brand
                    };

                    var clonedCode = SelectedInventoryProduct.Code;
                    SelectedInventoryProduct = null;
                    CurrentPage = "\\View\\InventoryItemPage.xaml";
                    InventoryTemporalItem = temporalClonedProduct;

                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Clon de Producto:" + " " + clonedCode);
                    break;
            }
            //Log
        }

        internal bool CanExecute_InventorModifyItemCommand(object parameter)
        {
            return SelectedInventoryProduct != null;
        }

        #endregion

        #region InventoryAddNewItemCommand

        public ICommand InventoryAddNewItemCommand { get { return _inventoryAddNewItemCommand ?? (_inventoryAddNewItemCommand = new DelegateCommand(Execute_InventoryAddNewItemCommand, CanExecute_InventoryAddNewItemCommand)); } }
        private ICommand _inventoryAddNewItemCommand;

        internal void Execute_InventoryAddNewItemCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "inventory_add":
                    SelectedInventoryProduct = null;    //Clear selected item in the inventory
                    //Create new productt
                    var temporalProduct = new ProductBase()
                    {
                        Id = _inventoryInstance.GetLastItemNumber() + 1,
                        Code = "",
                        AlternativeCode = "",
                        AmountSold = 0M,
                        Category = "",
                        Cost = 0M,
                        CostCurrency = CurrencyTypeEnum.MXN,
                        Description = "",
                        ImageName = "NA.jpg",
                        InternalQuantity = 0,
                        LastPurchaseDate = DateTime.Now,
                        LastQuantitySold = 0,
                        LastSaleDate = DateTime.Now,
                        LocalQuantityAvailable = 0,
                        MinimumStockQuantity = 1,
                        Name = "",
                        Price = 0M,
                        PriceCurrency = CurrencyTypeEnum.MXN,
                        Provider = "",
                        ProviderProductId = "",
                        QuantitySold = 0,
                        TotalQuantityAvailable = 0,
                        Brand = ""
                    };

                    CurrentPage = "\\View\\InventoryItemPage.xaml";
                    InventoryTemporalItem = temporalProduct;
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Nuevo Producto Iniciado");
                    break;
            }

        }

        internal bool CanExecute_InventoryAddNewItemCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region InventoryAddItemToCartCommand

        public ICommand InventoryAddItemToCartCommand { get { return _inventoryAddItemToCartCommand ?? (_inventoryAddItemToCartCommand = new DelegateCommand(Execute_InventoryAddItemToCartCommand, CanExecute_InventoryAddItemToCartCommand)); } }
        private ICommand _inventoryAddItemToCartCommand;

        internal void Execute_InventoryAddItemToCartCommand(object parameter)
        {
            //TODO: Check to make sure the item is found, otherwise show error message
            //Create a new object for every product 
            var product = new ProductBase((ProductBase)parameter) { LastQuantitySold = 1 };
            AddProductToCart(product);
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Busqueda Agregada al Carrito:" + " " + product.Code);
        }

        internal bool CanExecute_InventoryAddItemToCartCommand(object parameter)
        {
            return parameter != null;
        }

        #endregion

        #region InventoryStartSearchCommand

        public ICommand InventoryStartSearchCommand { get { return _inventoryStartSearchCommand ?? (_inventoryStartSearchCommand = new DelegateCommand(Execute_InventoryStartSearchCommand, CanExecute_InventoryStartSearchCommand)); } }
        private ICommand _inventoryStartSearchCommand;

        internal void Execute_InventoryStartSearchCommand(object parameter)
        {
            //Inventory search method that returns a list of products for the datagrid
            InventorySearchedProducts = new ObservableCollection<ProductBase>(_inventoryInstance.Search(InventorySearchText));
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Busqueda en Inventario:" + " " + InventorySearchText);
            InventorySearchText = "";
                       
        }
        internal bool CanExecute_InventoryStartSearchCommand(object parameter)
        {
            return true;//InventorySearchText != "" && InventorySearchText != null;
        }
       

        #endregion

        #region InventoryItemImageCommand

        private ICommand _inventoryItemImageCommand;
        public ICommand InventoryItemImageCommand { get { return _inventoryItemImageCommand ?? (_inventoryItemImageCommand = new DelegateCommand(Execute_InventoryItemCommand, CanExecute_InventoryItemCommand)); } }

        internal void Execute_InventoryItemCommand(object parameter)
        {
            switch (parameter)
            {
                case "add":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Agregar Imagen Iniciado Producto:" + " " + InventoryTemporalItem.Code);
                    var imageSelected = SelectImage();
                    if (imageSelected != null)
                    {
                        try
                        {
                            InventoryTemporalItem.ImageName = imageSelected;
                            ProductImage = InventoryTemporalItem.Image;
                        }
                        catch (Exception e)
                        {
                            //Log
                            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Error al cargar imagen: " + InventoryTemporalItem.ImageName);
                            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Exception: " + e.Message);
                            InventoryTemporalItem.ImageName = "NA.jpg";
                            ProductImage = InventoryTemporalItem.Image;
                        }
                    }
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Agregar Imagen Completado Producto:" + " " + InventoryTemporalItem.Code);
                    break;

                case "remove":                    
                    InventoryTemporalItem.ImageName = "NA.jpg";
                    ProductImage = InventoryTemporalItem.Image;
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Imagen Eliminada Producto:" + " " + InventoryTemporalItem.Code);
                    break;
            }
           
        }
        internal bool CanExecute_InventoryItemCommand(object parameter)
        {
            return true; // SelectedInventoryProduct.Image != null;
        }
        #endregion

        #region InventorySaveChangesCommand
        public ICommand InventorySaveChangesCommand { get { return _inventorySaveChangesCommand ?? (_inventorySaveChangesCommand = new DelegateCommand(Execute_InventorySaveChangesCommand, CanExecute_InventorySaveChangesCommand)); } }
        private ICommand _inventorySaveChangesCommand;

        internal void Execute_InventorySaveChangesCommand(object parameter)
        {
            //Save temporal product changes to actual product by looking for Code

            //Check if code was updated
            if(SelectedInventoryProduct != null)
            {
                _inventoryInstance.UpdateProductToTable(InventoryTemporalItem);
                _inventoryInstance.SaveDataTableToCsv();
                CurrentPage = "\\View\\InventoryMainPage.xaml";
            }
            else
            {
                _inventoryInstance.AddNewProductToTable(InventoryTemporalItem);
                _inventoryInstance.SaveDataTableToCsv();
                CurrentPage = "\\View\\InventoryMainPage.xaml";
            }

            Code = "Producto Guardado";
            CodeColor = Constants.ColorCodeSave;
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Producto Guardado:" + " " + InventoryTemporalItem.Code);
            //Reset list
            InventorySearchedProducts = null;
            SelectedInventoryProduct = null;
        }
        internal bool CanExecute_InventorySaveChangesCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region InventoryDeleteCommand

        public ICommand InventoryDeleteCommand { get { return _inventoryDeleteCommand ?? (_inventoryDeleteCommand = new DelegateCommand(Execute_InventoryDeleteCommand, CanExecute_InventoryDeleteCommand)); } }
        private ICommand _inventoryDeleteCommand;

        internal void Execute_InventoryDeleteCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedInventoryProduct != null)
            {
                _inventoryInstance.DeleteItemInDataTable(SelectedInventoryProduct.Code, "Codigo");
                _inventoryInstance.SaveDataTableToCsv();
                CurrentPage = "\\View\\InventoryMainPage.xaml";

                Code = "Producto Eliminado";
                CodeColor = Constants.ColorCodeError;
                //Log
                Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Producto Eliminado:" + " " + SelectedInventoryProduct.Code);
                //Reset list
                InventorySearchedProducts = null;
                SelectedInventoryProduct = null;
            }           
        }

        internal bool CanExecute_InventoryDeleteCommand(object parameter)
        {
            return SelectedInventoryProduct != null;
        }
        #endregion

        #endregion Inventory Commands

        #region Users Commands

        #region UserUpdateCommand

        public ICommand UserUpdateCommand { get { return _userUpdateCommand ?? (_userUpdateCommand = new DelegateCommand(Execute_UserUpdateCommand, CanExecute_UserUpdateCommand)); } }
        private ICommand _userUpdateCommand;

        internal void Execute_UserUpdateCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "user_details":
                    var temporalProduct = new User(Constants.DataFolderPath + Constants.UsersFileName)
                    {
                        Id = SelectedUser.Id,
                        Name = SelectedUser.Name,
                        Email = SelectedUser.Email,
                        Phone = SelectedUser.Phone,
                        RegistrationDate = SelectedUser.RegistrationDate,
                        Rights = SelectedUser.Rights,
                        UserName = SelectedUser.UserName,
                        Password = SelectedUser.Password,
                        LastLogin = SelectedUser.LastLogin
                    };

                    CurrentPage = "\\View\\UserDetailPage.xaml";
                    UserTemporalItem = temporalProduct;
                    break;
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Detalle de Usuario:" + " " + SelectedUser.UserName);
        }
         
        internal bool CanExecute_UserUpdateCommand(object parameter)
        {
            return SelectedUser != null;
        }

        #endregion

        #region UserAddNewItemCommand

        public ICommand UserAddNewItemCommand { get { return _userAddNewItemCommand ?? (_userAddNewItemCommand = new DelegateCommand(Execute_UserAddNewItemCommand, CanExecute_UserAddNewItemCommand)); } }
        private ICommand _userAddNewItemCommand;

        internal void Execute_UserAddNewItemCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "user_add":
                    SelectedUser = null;    //Clear selected item in the user list
                    //Create new productt
                    var temporalProduct = new User(Constants.DataFolderPath + Constants.UsersFileName)
                    {
                        Name = "",
                        Email = "",
                        Phone = "",
                        RegistrationDate = DateTime.Now,
                        Rights = UserAccessLevelEnum.Basico,
                        UserName = "",
                        Password = "",
                        LastLogin = DateTime.Now
                    };
                    temporalProduct.Id = temporalProduct.GetLastItemNumber() + 1;

                    CurrentPage = "\\View\\UserDetailPage.xaml";
                    UserTemporalItem = temporalProduct;
                    break;
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Nuevo Usuario Iniciado");
        }

        internal bool CanExecute_UserAddNewItemCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region UserStartSearchCommand

        public ICommand UserStartSearchCommand { get { return _userStartSearchCommand ?? (_userStartSearchCommand = new DelegateCommand(Execute_UserStartSearchCommand, CanExecute_UserStartSearchCommand)); } }
        private ICommand _userStartSearchCommand;

        internal void Execute_UserStartSearchCommand(object parameter)
        {
            //Inventory search method that returns a list of products for the datagrid
            UsersSearchedEntries = new ObservableCollection<User>(new User(Constants.DataFolderPath + Constants.UsersFileName).Search(UsersSearchText));
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Busqueda de Usuario:" + " " + UsersSearchText );
            UsersSearchText = "";
        }
        internal bool CanExecute_UserStartSearchCommand(object parameter)
        {
            return true;//InventorySearchText != "" && InventorySearchText != null;
        }

        #endregion

        #region  UserSaveChangesCommand
        public ICommand UserSaveChangesCommand { get { return _userSaveChangesCommand ?? (_userSaveChangesCommand = new DelegateCommand(Execute_UserSaveChangesCommand, CanExecute_UserSaveChangesCommand)); } }
        private ICommand _userSaveChangesCommand;

        internal void Execute_UserSaveChangesCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedUser != null)
            {
                SelectedUser.UpdateUserToTable(UserTemporalItem);
                SelectedUser.SaveDataTableToCsv();
            }
            else
            {
                UserTemporalItem.Register();
            }

            Code = "Usuario Guardado";
            CodeColor = Constants.ColorCodeSave;
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Usuario Guardado:" + " " + UserTemporalItem.UserName);
            UsersSearchedEntries = null;
            CurrentPage = "\\View\\UserMainPage.xaml";
        }

        internal bool CanExecute_UserSaveChangesCommand(object parameter)
        {
            return UserTemporalItem.Password == UserTemporalItem.PasswordVerification &&
                   UserTemporalItem.Password != "" && UserTemporalItem.Name != "" && UserTemporalItem.UserName != "";
        }
        #endregion

        #region  UserDeleteCommand

        public ICommand UserDeleteCommand { get { return _userDeleteCommand ?? (_userDeleteCommand = new DelegateCommand(Execute_UserDeleteCommand, CanExecute_UserDeleteCommand)); } }
        private ICommand _userDeleteCommand;

        internal void Execute_UserDeleteCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedUser != null)
            {
                SelectedUser.Delete();
                SelectedUser.SaveDataTableToCsv();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Usuario Eliminado:" + " " + SelectedUser.UserName);
            UsersSearchedEntries = null;
            CurrentPage = "\\View\\UserMainPage.xaml";
            Code = "Usuario Eliminado";
            CodeColor = Constants.ColorCodeError;
        }

        internal bool CanExecute_UserDeleteCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion User Commands

        #region Customer Commands

        #region CustomerUpdateCommand

        public ICommand CustomerUpdateCommand { get { return _customerUpdateCommand ?? (_customerUpdateCommand = new DelegateCommand(Execute_CustomerUpdateCommand, CanExecute_CustomerUpdateCommand)); } }
        private ICommand _customerUpdateCommand;

        internal void Execute_CustomerUpdateCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "customer_details":
                    var temporalCustomer = new Customer(Constants.DataFolderPath + Constants.CustomersFileName)
                    {
                        Id = SelectedCustomer.Id,
                        Name = SelectedCustomer.Name,
                        Email = SelectedCustomer.Email,
                        Phone = SelectedCustomer.Phone,
                        Rfc = SelectedCustomer.Rfc,
                        PointsAvailable = SelectedCustomer.PointsAvailable,
                        PointsUsed = SelectedCustomer.PointsUsed,
                        TotalSpent = SelectedCustomer.TotalSpent,
                        TotalVisits = SelectedCustomer.TotalVisits,
                    };
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Detalle de Cliente:" + " " + SelectedCustomer.Name);
                    CurrentPage = "\\View\\CustomerDetailPage.xaml";
                    CustomerTemporalItem = temporalCustomer;
                    break;
            }
        }

        internal bool CanExecute_CustomerUpdateCommand(object parameter)
        {
            return SelectedCustomer != null;
        }

        #endregion

        #region CustomerAddNewItemCommand

        public ICommand CustomerAddNewItemCommand { get { return _customerAddNewItemCommand ?? (_customerAddNewItemCommand = new DelegateCommand(Execute_CustomerAddNewItemCommand, CanExecute_CustomerAddNewItemCommand)); } }
        private ICommand _customerAddNewItemCommand;

        internal void Execute_CustomerAddNewItemCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "customer_add":
                    SelectedCustomer = null;    //Clear selected item in the user list
                    //Create new productt
                    var temporalCustomer = new Customer(Constants.DataFolderPath + Constants.CustomersFileName)
                    {
                        Name = "",
                        Email = "",
                        Phone = "",
                        RegistrationDate = DateTime.Now,
                        Rfc = "",
                        PointsAvailable = 0,
                        PointsUsed = 0,
                        TotalSpent = 0,
                        TotalVisits = 0,
                        LastVisitDate = DateTime.Now
                    };
                    temporalCustomer.Id = temporalCustomer.GetLastItemNumber() + 1;
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Nuevo Cliente Iniciado:");
                    CurrentPage = "\\View\\CustomerDetailPage.xaml";
                    CustomerTemporalItem = temporalCustomer;
                    break;
            }
        }

        internal bool CanExecute_CustomerAddNewItemCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region CustomerStartSearchCommand

        public ICommand CustomerStartSearchCommand { get { return _customerStartSearchCommand ?? (_customerStartSearchCommand = new DelegateCommand(Execute_CustomerStartSearchCommand, CanExecute_CustomerStartSearchCommand)); } }
        private ICommand _customerStartSearchCommand;

        internal void Execute_CustomerStartSearchCommand(object parameter)
        {
            //Inventory search method that returns a list of products for the datagrid
            CustomersSearchedEntries = new ObservableCollection<Customer>(new Customer(Constants.DataFolderPath + Constants.CustomersFileName).Search(CustomersSearchText));
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Busqueda de Cliente:" + " " + CustomersSearchText);
            CustomersSearchText = "";
        }
        internal bool CanExecute_CustomerStartSearchCommand(object parameter)
        {
            return true;//InventorySearchText != "" && InventorySearchText != null;
        }

        #endregion

        #region  CustomerSaveChangesCommand
        public ICommand CustomerSaveChangesCommand { get { return _customerSaveChangesCommand ?? (_customerSaveChangesCommand = new DelegateCommand(Execute_CustomerSaveChangesCommand, CanExecute_CustomerSaveChangesCommand)); } }
        private ICommand _customerSaveChangesCommand;

        internal void Execute_CustomerSaveChangesCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedCustomer != null)
            {
                SelectedCustomer.UpdateUserToTable(CustomerTemporalItem);
                SelectedCustomer.SaveDataTableToCsv();
            }
            else
            {
                CustomerTemporalItem.Register();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Usuario Guardado:" + " " + CustomerTemporalItem.Name);
            CustomersSearchedEntries = null;
            CurrentPage = "\\View\\CustomerMainPage.xaml";
            Code = "Cliente Guardado";
            CodeColor = Constants.ColorCodeSave;
        }

        internal bool CanExecute_CustomerSaveChangesCommand(object parameter)
        {
            return true;// UserTemporalItem.Password == UserTemporalItem.PasswordVerification && UserTemporalItem.Password != "";
        }
        #endregion

        #region  CustomerDeleteCommand

        public ICommand CustomerDeleteCommand { get { return _customerDeleteCommand ?? (_customerDeleteCommand = new DelegateCommand(Execute_CustomerDeleteCommand, CanExecute_CustomerDeleteCommand)); } }
        private ICommand _customerDeleteCommand;

        internal void Execute_CustomerDeleteCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedCustomer != null)
            {
                SelectedCustomer.Delete();
                SelectedCustomer.SaveDataTableToCsv();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Cliente Eliminado:" + " " + SelectedCustomer.Name);
            CustomersSearchedEntries = null;
            CurrentPage = "\\View\\CustomerMainPage.xaml";
            Code = "Cliente Eliminado";
            CodeColor = Constants.ColorCodeError;
        }

        internal bool CanExecute_CustomerDeleteCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion Customer Commands

        #region Expenses Commands

        #region ExpenseUpdateCommand

        public ICommand ExpenseUpdateCommand { get { return _expenseUpdateCommand ?? (_expenseUpdateCommand = new DelegateCommand(Execute_ExpenseUpdateCommand, CanExecute_ExpenseUpdateCommand)); } }
        private ICommand _expenseUpdateCommand;

        internal void Execute_ExpenseUpdateCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "expense_details":
                    var temporalExpense = new Expense(Constants.DataFolderPath + Constants.ExpenseXFileName, Constants.DataFolderPath + Constants.ExpenseZFileName, 
                        Constants.DataFolderPath + Constants.ExpenseHistoryFileName)
                    {
                        Id = SelectedExpense.Id,
                        User = SelectedExpense.User,
                        Vendor = SelectedExpense.Vendor,
                        TicketNumber = SelectedExpense.TicketNumber,
                        Description = SelectedExpense.Description,
                        Amount = SelectedExpense.Amount,
                        ExpenseCategory = SelectedExpense.ExpenseCategory,
                        Date = SelectedExpense.Date,
                        PaymentType = SelectedExpense.PaymentType,
                        CurrencyType = SelectedExpense.CurrencyType
                    };
                    CurrentPage = "\\View\\ExpenseDetailPage.xaml";
                    ExpenseTemporalItem = temporalExpense;
                    break;
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Detalle de Gasto Ticket: " + SelectedExpense.TicketNumber);
        }

        internal bool CanExecute_ExpenseUpdateCommand(object parameter)
        {
            return SelectedExpense != null;
        }

        #endregion

        #region ExpenseAddNewItemCommand

        public ICommand ExpenseAddNewItemCommand { get { return _expenseAddNewItemCommand ?? (_expenseAddNewItemCommand = new DelegateCommand(Execute_ExpenseAddNewItemCommand, CanExecute_ExpenseAddNewItemCommand)); } }
        private ICommand _expenseAddNewItemCommand;

        internal void Execute_ExpenseAddNewItemCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "expense_add":
                    SelectedExpense = null;    //Clear selected item in the user list
                    //Create new productt
                    var temporalExpense = new Expense(Constants.DataFolderPath + Constants.ExpenseXFileName, Constants.DataFolderPath + Constants.ExpenseZFileName, 
                        Constants.DataFolderPath + Constants.ExpenseHistoryFileName)
                    {
                        Vendor = "",
                        TicketNumber = "",
                        Description = "",
                        Amount = 0,
                        ExpenseCategory = "",
                        Date = DateTime.Now,
                        PaymentType = PaymentTypeEnum.Efectivo,
                        CurrencyType = CurrencyTypeEnum.MXN
                    };
                    temporalExpense.Id = temporalExpense.GetLastItemNumber() + 1;

                    CurrentPage = "\\View\\ExpenseDetailPage.xaml";
                    ExpenseTemporalItem = temporalExpense;
                    break;
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Inicio Gasto Nuevo");
        }

        internal bool CanExecute_ExpenseAddNewItemCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ExpenseStartSearchCommand

        public ICommand ExpenseStartSearchCommand { get { return _expenseStartSearchCommand ?? (_expenseStartSearchCommand = new DelegateCommand(Execute_ExpenseStartSearchCommand, CanExecute_ExpenseStartSearchCommand)); } }
        private ICommand _expenseStartSearchCommand;

        internal void Execute_ExpenseStartSearchCommand(object parameter)
        {
            //Inventory search method that returns a list of products for the datagrid
            ExpensesSearchedEntries = new ObservableCollection<Expense>(new Expense(Constants.DataFolderPath + Constants.ExpenseXFileName, 
                Constants.DataFolderPath + Constants.ExpenseZFileName, Constants.DataFolderPath + Constants.ExpenseHistoryFileName).Search(ExpensesSearchText));
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Busqueda en Gastos: " + ExpensesSearchText);
            ExpensesSearchText = "";
        }
        internal bool CanExecute_ExpenseStartSearchCommand(object parameter)
        {
            return true;//InventorySearchText != "" && InventorySearchText != null;
        }

        #endregion

        #region  ExpenseSaveChangesCommand
        public ICommand ExpenseSaveChangesCommand { get { return _expenseSaveChangesCommand ?? (_expenseSaveChangesCommand = new DelegateCommand(Execute_ExpenseSaveChangesCommand, CanExecute_ExpenseSaveChangesCommand)); } }
        private ICommand _expenseSaveChangesCommand;

        internal void Execute_ExpenseSaveChangesCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedExpense != null)
            {
                SelectedExpense.UpdateExpenseToTable(ExpenseTemporalItem);
                SelectedExpense.SaveDataTableToCsv();
            }
            else
            {
                ExpenseTemporalItem.Register();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Gasto Guardado Ticket: " + ExpenseTemporalItem.TicketNumber);
            Code = "Gasto Guardado";
            ExpensesSearchedEntries = null;
            CurrentPage = "\\View\\ExpenseMainPage.xaml";
            CodeColor = Constants.ColorCodeSave;
        }

        internal bool CanExecute_ExpenseSaveChangesCommand(object parameter)
        {
            return true; // UserTemporalItem.Password == UserTemporalItem.PasswordVerification && UserTemporalItem.Password != "";
        }
        #endregion

        #region  ExpenseDeleteCommand

        public ICommand ExpenseDeleteCommand { get { return _expenseDeleteCommand ?? (_expenseDeleteCommand = new DelegateCommand(Execute_ExpenseDeleteCommand, CanExecute_ExpenseDeleteCommand)); } }
        private ICommand _expenseDeleteCommand;

        internal void Execute_ExpenseDeleteCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedExpense != null)
            {
                SelectedExpense.Delete();
                SelectedExpense.SaveDataTableToCsv();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Gasto Eliminado Ticket: " + SelectedExpense.TicketNumber);
            ExpensesSearchedEntries = null;          
            CurrentPage = "\\View\\ExpenseMainPage.xaml";
            Code = "Gasto Eliminado";
            CodeColor = Constants.ColorCodeError;
        }

        internal bool CanExecute_ExpenseDeleteCommand(object parameter)
        {
            return true;
        }

        #endregion

        #endregion Expenses Commands

        #region Vendor Commands

        #region VendorUpdateCommand

        public ICommand VendorUpdateCommand { get { return _vendorUpdateCommand ?? (_vendorUpdateCommand = new DelegateCommand(Execute_VendorUpdateCommand, CanExecute_VendorUpdateCommand)); } }
        private ICommand _vendorUpdateCommand;

        internal void Execute_VendorUpdateCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "vendor_details":
                    var temporalVendor = new Vendor(Constants.DataFolderPath + Constants.VendorsFileName)
                    {
                        Id = SelectedVendor.Id,
                        Name = SelectedVendor.Name,
                        Email = SelectedVendor.Email,
                        Phone = SelectedVendor.Phone,
                        RegistrationDate = SelectedVendor.RegistrationDate,
                        Rfc = SelectedVendor.Rfc,
                        BusinessName = SelectedVendor.BusinessName,
                        Bank = SelectedVendor.Bank,
                        BankAccount = SelectedVendor.BankAccount
                    };

                    CurrentPage = "\\View\\VendorDetailPage.xaml";
                    VendorTemporalItem = temporalVendor;
                    break;
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Detalle de Vendedor:" + " " + SelectedVendor.Name);
        }

        internal bool CanExecute_VendorUpdateCommand(object parameter)
        {
            return SelectedVendor != null;
        }

        #endregion

        #region VendorAddNewItemCommand

        public ICommand VendorAddNewItemCommand { get { return _vendorAddNewItemCommand ?? (_vendorAddNewItemCommand = new DelegateCommand(Execute_VendorAddNewItemCommand, CanExecute_VendorAddNewItemCommand)); } }
        private ICommand _vendorAddNewItemCommand;

        internal void Execute_VendorAddNewItemCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "vendor_add":
                    SelectedVendor = null;    //Clear selected item in the user list
                    //Create new productt
                    var temporalVendor = new Vendor(Constants.DataFolderPath + Constants.VendorsFileName)
                    {
                        Name = "",
                        Email = "",
                        Phone = "",
                        RegistrationDate = DateTime.Now,
                        Rfc = "",
                        BusinessName = "",
                        Bank = "",
                        BankAccount = ""
                    };
                    temporalVendor.Id = temporalVendor.GetLastItemNumber() + 1;

                    CurrentPage = "\\View\\VendorDetailPage.xaml";
                    VendorTemporalItem = temporalVendor;
                    break;
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Nuevo Proveedor Iniciado:");
        }

        internal bool CanExecute_VendorAddNewItemCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region VendorStartSearchCommand

        public ICommand VendorStartSearchCommand { get { return _vendorStartSearchCommand ?? (_vendorStartSearchCommand = new DelegateCommand(Execute_VendorStartSearchCommand, CanExecute_VendorStartSearchCommand)); } }
        private ICommand _vendorStartSearchCommand;

        internal void Execute_VendorStartSearchCommand(object parameter)
        {
            //Inventory search method that returns a list of products for the datagrid
            VendorsSearchedEntries = new ObservableCollection<Vendor>(new Vendor(Constants.DataFolderPath + Constants.VendorsFileName).Search(VendorsSearchText));
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Busqueda de Proveedor:" + " " + VendorsSearchText);
            VendorsSearchText = "";
        }
        internal bool CanExecute_VendorStartSearchCommand(object parameter)
        {
            return true;//InventorySearchText != "" && InventorySearchText != null;
        }

        #endregion

        #region  VendorDeleteCommand

        public ICommand VendorDeleteCommand { get { return _vendorDeleteCommand ?? (_vendorDeleteCommand = new DelegateCommand(Execute_VendorDeleteCommand, CanExecute_VendorDeleteCommand)); } }
        private ICommand _vendorDeleteCommand;

        internal void Execute_VendorDeleteCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedVendor != null)
            {
                SelectedVendor.Delete();
                SelectedVendor.SaveDataTableToCsv();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Proveedor Eliminado:" + " " + SelectedVendor.Name);
            VendorsSearchedEntries = null;
            CurrentPage = "\\View\\VendorMainPage.xaml";
            Code = "Proveedor Eliminado";
            CodeColor = Constants.ColorCodeError;
        }

        internal bool CanExecute_VendorDeleteCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region VendorSaveChangesCommand

        public ICommand VendorSaveChangesCommand { get { return _vendorSaveChangesCommand ?? (_vendorSaveChangesCommand = new DelegateCommand(Execute_VendorSaveChangesCommand, CanExecute_VendorSaveChangesCommand)); } }
        private ICommand _vendorSaveChangesCommand;

        internal void Execute_VendorSaveChangesCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedVendor != null)
            {
                SelectedVendor.UpdateUserToTable(VendorTemporalItem);
                SelectedVendor.SaveDataTableToCsv();
            }
            else
            {
                VendorTemporalItem.Register();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Proveedor Guardado:" + " " + VendorTemporalItem.Name);
            VendorsSearchedEntries = null;
            Code = "Proveedor Guardado";
            CodeColor = Constants.ColorCodeSave;

            CurrentPage = "\\View\\VendorMainPage.xaml";
        }

        internal bool CanExecute_VendorSaveChangesCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion Vendor Commands

        #region Orders Commands

        #region OrderUpdateCommand

        public ICommand OrderUpdateCommand { get { return _orderUpdateCommand ?? (_orderUpdateCommand = new DelegateCommand(Execute_OrderUpdateCommand, CanExecute_OrderUpdateCommand)); } }
        private ICommand _orderUpdateCommand;

        internal void Execute_OrderUpdateCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "order_details":
                    var temporalOrder = new Order(Constants.DataFolderPath + Constants.OrdersFileName)
                    {
                        Id = SelectedOrder.Id,
                        OrderTicketNumber = SelectedOrder.OrderTicketNumber,
                        Customer = SelectedOrder.Customer,
                        Title = SelectedOrder.Title,
                        DueDate = SelectedOrder.DueDate,
                        Category = SelectedOrder.Category,
                        TotalDue = SelectedOrder.TotalDue,
                        TotalPrePaid = SelectedOrder.TotalPrePaid,
                        PrePaidTicketNumber = SelectedOrder.PrePaidTicketNumber,
                        TotalAmount = SelectedOrder.TotalAmount,
                        Description = SelectedOrder.Description,
                        ImageName = SelectedOrder.ImageName,
                        RegistrationDate = SelectedOrder.RegistrationDate
                    };

                    CurrentPage = "\\View\\OrderPage.xaml";
                    OrderTemporalItem = temporalOrder;
                    break;
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Detalle de Pedido:" + " " + SelectedOrder.OrderTicketNumber);
        }

        internal bool CanExecute_OrderUpdateCommand(object parameter)
        {
            return SelectedOrder != null;
        }

        #endregion

        #region OrderAddNewItemCommand

        public ICommand OrderAddNewItemCommand { get { return _orderAddNewItemCommand ?? (_orderAddNewItemCommand = new DelegateCommand(Execute_OrderAddNewItemCommand, CanExecute_OrderAddNewItemCommand)); } }
        private ICommand _orderAddNewItemCommand;

        internal void Execute_OrderAddNewItemCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "order_add":
                    SelectedOrder= null;    //Clear selected item in the user list
                    //Create new productt
                    var temporalOrder = new Order(Constants.DataFolderPath + Constants.OrdersFileName)
                    {
                        OrderTicketNumber = 0,
                        Customer = "",
                        Title = "",
                        DueDate = DateTime.Now,
                        Category = "",
                        TotalDue = 0,
                        TotalPrePaid = 0,
                        PrePaidTicketNumber = 0,
                        TotalAmount = 0,
                        Description = "",
                        ImageName = "NA.jpg",
                        RegistrationDate = DateTime.Now
                    };
                    temporalOrder.Id = temporalOrder.GetLastItemNumber() + 1;
                    CurrentPage = "\\View\\OrderPage.xaml";
                    OrderTemporalItem = temporalOrder;
                    break;
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Nuevo Pedido Iniciado:");
        }

        internal bool CanExecute_OrderAddNewItemCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region OrderStartSearchCommand

        public ICommand OrderStartSearchCommand { get { return _orderStartSearchCommand ?? (_orderStartSearchCommand = new DelegateCommand(Execute_OrderStartSearchCommand, CanExecute_OrderStartSearchCommand)); } }
        private ICommand _orderStartSearchCommand;

        internal void Execute_OrderStartSearchCommand(object parameter)
        {
            //Inventory search method that returns a list of products for the datagrid
            OrdersSearchedEntries = new ObservableCollection<Order>(new Order(Constants.DataFolderPath + Constants.OrdersFileName).Search(OrdersSearchText));
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Busqueda de Pedido:" + " " + OrdersSearchText);
            OrdersSearchText = "";
        }
        internal bool CanExecute_OrderStartSearchCommand(object parameter)
        {
            return true;//InventorySearchText != "" && InventorySearchText != null;
        }

        #endregion

        #region  OrderSaveChangesCommand
        public ICommand OrderSaveChangesCommand { get { return _orderSaveChangesCommand ?? (_orderSaveChangesCommand = new DelegateCommand(Execute_OrderSaveChangesCommand, CanExecute_OrderSaveChangesCommand)); } }
        private ICommand _orderSaveChangesCommand;

        internal void Execute_OrderSaveChangesCommand(object parameter)
        {
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Inicia el Envio del Pedido:" + OrderTemporalItem.OrderTicketNumber);
            ProgressMessage = "Enviando...";
            ProgressBarValue = 20;
            //Check if code was updated
            if (SelectedOrder != null)
            {
                SelectedOrder.UpdateOrderToTable(OrderTemporalItem);
                SelectedOrder.SaveDataTableToCsv();
            }
            else
            {
                OrderTemporalItem.Register();
            }

            ProgressBarValue = 65;
            SendEmailNotification();
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Envio Terminado Pedido Guardado:" + OrderTemporalItem.OrderTicketNumber);
        }

        private void SendEmailNotification()
        {
            //TODO: Make it generic based on POS data later
            var toName = "Estrella de Regalos";
            var toEmailAddress = _posInstance.EmailOrders; //"armoag+movvfdhrzrdgpqmw5qcg@boards.trello.com";
            var subject = OrderTemporalItem.Customer + " " + OrderTemporalItem.OrderTicketNumber;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var body = OrderTemporalItem.Description + Environment.NewLine +
                       OrderTemporalItem.DueDate.ToString("G", CultureInfo.CreateSpecificCulture("mx"));
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            var attachmentFilePath = Constants.DataFolderPath + Constants.ImagesFolderPath + OrderTemporalItem.ImageName;
            var fromEmailAddress = _posInstance.EmailSender; //"lluviasantafe@gmail.com";
            var fromPassword = _posInstance.EmailSenderPassword; //"Yadira00";
            Notification.SendNotification(toName, toEmailAddress, subject, body, attachmentFilePath, fromEmailAddress, fromPassword);
            ProgressBarValue = 90;
            OrdersSearchedEntries = null;
            CurrentPage = "\\View\\OrderMainPage.xaml";
            Code = "¡Pedido Enviado!";
            CodeColor = Constants.ColorCodeSave;
            ProgressMessage = "";
            ProgressBarValue = 0;
        }

        internal bool CanExecute_OrderSaveChangesCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  OrderDeleteCommand

        public ICommand OrderDeleteCommand { get { return _orderDeleteCommand ?? (_orderDeleteCommand = new DelegateCommand(Execute_OrderDeleteCommand, CanExecute_OrderDeleteCommand)); } }
        private ICommand _orderDeleteCommand;

        internal void Execute_OrderDeleteCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedOrder != null)
            {
                SelectedOrder.Delete();
                SelectedOrder.SaveDataTableToCsv();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Pedido Eliminado:" + " " + SelectedOrder.OrderTicketNumber);
            OrdersSearchedEntries = null;
            CurrentPage = "\\View\\OrderMainPage.xaml";
            Code = "Pedido Eliminado";
            CodeColor = Constants.ColorCodeError;
        }

        internal bool CanExecute_OrderDeleteCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region OrderItemImageCommand

        private ICommand _orderItemImageCommand;
        public ICommand OrderItemImageCommand { get { return _orderItemImageCommand ?? (_orderItemImageCommand = new DelegateCommand(Execute_OrderItemImageCommand, CanExecute_OrderItemImageCommand)); } }

        internal void Execute_OrderItemImageCommand(object parameter)
        {
            switch (parameter)
            {
                case "add":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Inicio Carga de Imagen Pedido: " + OrderTemporalItem.OrderTicketNumber);
                    OrderTemporalItem.ImageName = SelectImage();
                    OrderImage = OrderTemporalItem.Image;
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Carga de Imagen Completa Pedido: " + OrderTemporalItem.OrderTicketNumber);
                    break;
                case "remove":
                    OrderTemporalItem.ImageName = "NA.jpg";
                    OrderImage = OrderTemporalItem.Image;
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Imagen eliminada Pedido: " + OrderTemporalItem.OrderTicketNumber);
                    break;
            }
        }
        internal bool CanExecute_OrderItemImageCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion Orders Commands

        #region Return Commands

        #region ReturnUpdateCommand

        public ICommand ReturnUpdateCommand { get { return _returnUpdateCommand ?? (_returnUpdateCommand = new DelegateCommand(Execute_ReturnUpdateCommand, CanExecute_ReturnUpdateCommand)); } }
        private ICommand _returnUpdateCommand;

        internal void Execute_ReturnUpdateCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                //case "customer_details":
                //    var temporalProduct = new User(Constants.DataFolderPath + Constants.UsersFileName)
                //    {
                //        Id = SelectedUser.Id,
                //        Name = SelectedUser.Name,
                //        Email = SelectedUser.Email,
                //        Phone = SelectedUser.Phone,
                //        RegistrationDate = SelectedUser.RegistrationDate,
                //        Rights = SelectedUser.Rights,
                //        UserName = SelectedUser.UserName,
                //        Password = SelectedUser.Password,
                //        LastLogin = SelectedUser.LastLogin
                //    };

                //    CurrentPage = "\\View\\UserDetailPage.xaml";
                //    UserTemporalItem = temporalProduct;
                //    break;
            }
        }

        internal bool CanExecute_ReturnUpdateCommand(object parameter)
        {
            return SelectedUser != null;
        }

        #endregion

        #region ReturnAddNewItemCommand

        public ICommand ReturnAddNewItemCommand { get { return _returnAddNewItemCommand ?? (_returnAddNewItemCommand = new DelegateCommand(Execute_ReturnAddNewItemCommand, CanExecute_ReturnAddNewItemCommand)); } }
        private ICommand _returnAddNewItemCommand;

        internal void Execute_ReturnAddNewItemCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                //case "user_add":
                //    SelectedUser = null;    //Clear selected item in the user list
                //    //Create new productt
                //    var temporalProduct = new User(Constants.DataFolderPath + Constants.UsersFileName)
                //    {
                //        Name = "",
                //        Email = "",
                //        Phone = "",
                //        RegistrationDate = DateTime.Now,
                //        Rights = UserAccessLevelEnum.Basic,
                //        UserName = "",
                //        Password = "",
                //        LastLogin = DateTime.Now
                //    };
                //    temporalProduct.Id = temporalProduct.GetLastItemNumber() + 1;

                //    CurrentPage = "\\View\\UserDetailPage.xaml";
                //    UserTemporalItem = temporalProduct;
                //    break;
            }
        }

        internal bool CanExecute_ReturnAddNewItemCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ReturnStartSearchCommand

        public ICommand ReturnStartSearchCommand { get { return _returnStartSearchCommand ?? (_returnStartSearchCommand = new DelegateCommand(Execute_ReturnStartSearchCommand, CanExecute_ReturnStartSearchCommand)); } }
        private ICommand _returnStartSearchCommand;

        internal void Execute_ReturnStartSearchCommand(object parameter)
        {
            ////Inventory search method that returns a list of products for the datagrid
            //UsersSearchedEntries = new ObservableCollection<User>(new User(Constants.DataFolderPath + Constants.UsersFileName).Search(UsersSearchText));
            //UsersSearchText = "";
        }
        internal bool CanExecute_ReturnStartSearchCommand(object parameter)
        {
            return true;//InventorySearchText != "" && InventorySearchText != null;
        }

        #endregion

        #region  ReturnSaveChangesCommand
        public ICommand ReturnSaveChangesCommand { get { return _returnSaveChangesCommand ?? (_returnSaveChangesCommand = new DelegateCommand(Execute_ReturnSaveChangesCommand, CanExecute_ReturnSaveChangesCommand)); } }
        private ICommand _returnSaveChangesCommand;

        internal void Execute_ReturnSaveChangesCommand(object parameter)
        {
            ////Check if code was updated
            //if (SelectedUser != null)
            //{
            //    SelectedUser.UpdateUserToTable(UserTemporalItem);
            //    SelectedUser.SaveDataTableToCsv();
            //}
            //else
            //{
            //    UserTemporalItem.Register();
            //}
            //UsersSearchedEntries = null;
            //CurrentPage = "\\View\\UserMainPage.xaml";
        }

        internal bool CanExecute_ReturnSaveChangesCommand(object parameter)
        {
            return true;//UserTemporalItem.Password == UserTemporalItem.PasswordVerification && UserTemporalItem.Password != "";
        }
        #endregion

        #endregion Returns Commands

        #region Transaction Analysis Commands

        #region TransactionUpdateCommand

        public ICommand TransactionUpdateCommand { get { return _transactionUpdateCommand ?? (_transactionUpdateCommand = new DelegateCommand(Execute_TransactionUpdateCommand, CanExecute_TransactionUpdateCommand)); } }
        private ICommand _transactionUpdateCommand;

        internal void Execute_TransactionUpdateCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "transaction_details":
                    var temporalTransaction = new Transaction(Constants.DataFolderPath + Constants.TransactionsMasterFileName, true, true)
                    {
                        TransactionNumber = SelectedTransaction.TransactionNumber,
                        ReceiptNumber = SelectedTransaction.ReceiptNumber,
                        ProductCode = SelectedTransaction.ProductCode,
                        ProductCategory = SelectedTransaction.ProductCategory,
                        ProductDescription = SelectedTransaction.ProductDescription,
                        ProductPrice = SelectedTransaction.ProductPrice,
                        ProductQuantitySold = SelectedTransaction.ProductQuantitySold,
                        ProductTotalSale = SelectedTransaction.ProductTotalSale,
                        TransactionDate = SelectedTransaction.TransactionDate,
                        CustomerName = SelectedTransaction.CustomerName,
                        UserName = SelectedTransaction.UserName,
                        SaleType = SelectedTransaction.SaleType,
                        PaymentType = SelectedTransaction.PaymentType
                    };
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Detalle de Transaccion: " + SelectedTransaction.TransactionNumber);
                    CurrentPage = "\\View\\TransactionDetailPage.xaml";
                    TransactionTemporalItem = temporalTransaction;
                    break;
            }
        }

        internal bool CanExecute_TransactionUpdateCommand(object parameter)
        {
            return SelectedTransaction != null;
        }

        #endregion

        #region TransactionStartSearchCommand

        public ICommand TransactionStartSearchCommand { get { return _transactionStartSearchCommand ?? (_transactionStartSearchCommand = new DelegateCommand(Execute_TransactionStartSearchCommand, CanExecute_TransactionStartSearchCommand)); } }
        private ICommand _transactionStartSearchCommand;

        internal void Execute_TransactionStartSearchCommand(object parameter)
        {
            //Inventory search method that returns a list of products for the datagrid
            TransactionsSearchedEntries = TransactionZEnabled ? new ObservableCollection<Transaction>(new Transaction(Constants.DataFolderPath + Constants.TransactionsZFileName, true, true).Search(TransactionsSearchText)) :
                new ObservableCollection<Transaction>(new Transaction(Constants.DataFolderPath + Constants.TransactionsXFileName, true, true).Search(TransactionsSearchText));
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Busqueda en Transacciones: " + TransactionsSearchText);
            TransactionsSearchText = "";
        }
        internal bool CanExecute_TransactionStartSearchCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region  TransactionSaveChangesCommand
        public ICommand TransactionSaveChangesCommand { get { return _transactionSaveChangesCommand ?? (_transactionSaveChangesCommand = new DelegateCommand(Execute_TransactionSaveChangesCommand, CanExecute_TransactionSaveChangesCommand)); } }
        private ICommand _transactionSaveChangesCommand;

        internal void Execute_TransactionSaveChangesCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedTransaction != null)
            {
                SelectedTransaction.UpdateToTable(TransactionTemporalItem);
                SelectedTransaction.SaveDataTableToCsv();
                Code = "Cambio Guardado";
                CodeColor = Constants.ColorCodeSave;
            }
            else
            {
                //There is no way to register a transaction through this option
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Transaccion Guardada: " + SelectedTransaction.TransactionNumber);
            TransactionsSearchedEntries = null;
            CurrentPage = "\\View\\TransactionMainPage.xaml";
        }

        internal bool CanExecute_TransactionSaveChangesCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region TransactionDeleteCommand

        public ICommand TransactionDeleteCommand { get { return _transactionDeleteCommand ?? (_transactionDeleteCommand = new DelegateCommand(Execute_TransactionDeleteCommand, CanExecute_TransactionDeleteCommand)); } }
        private ICommand _transactionDeleteCommand;

        internal void Execute_TransactionDeleteCommand(object parameter)
        {
            //Check if code was updated
            if (SelectedTransaction != null)
            {
                SelectedTransaction.Delete();
                SelectedTransaction.SaveDataTableToCsv();
            }
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Transaccion Eliminada: " + SelectedTransaction.TransactionNumber);
            TransactionsSearchedEntries = null;
            CurrentPage = "\\View\\TransactionMainPage.xaml";
            Code = "Transaccion Eliminada";
            CodeColor = Constants.ColorCodeError;
        }

        internal bool CanExecute_TransactionDeleteCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region ChangeTransactionFileTypeCommand

        public ICommand ChangeTransactionFileTypeCommand { get { return _changeTransactionFileTypeCommand ?? (_changeTransactionFileTypeCommand = new DelegateCommand(Execute_ChangeTransactionFileTypeCommand, CanExecute_ChangeTransactionFileTypeCommand)); } }
        private ICommand _changeTransactionFileTypeCommand;

        internal void Execute_ChangeTransactionFileTypeCommand(object parameter)
        {
            if (TransactionZEnabled)
            {
                TransactionZEnabled = false;
            }
            else
            {
                TransactionZEnabled = true;
            }
        }

        internal bool CanExecute_ChangeTransactionFileTypeCommand(object parameter)
        {
            //Add logic to check if the command can be executed (if any)
            return true;
        }
        #endregion

        #endregion Returns Commands

        #region Exchange Rate Commands

        #region ExchangeRateSaveCommand
        public ICommand ExchangeRateSaveCommand { get { return _exchangeRateSaveCommand ?? (_exchangeRateSaveCommand = new DelegateCommand(Execute_ExchangeRateSaveCommand, CanExecute_ExchangeRateSaveCommand)); } }
        private ICommand _exchangeRateSaveCommand;

        internal void Execute_ExchangeRateSaveCommand(object parameter)
        {
            _posInstance.UpdateExchangeRate(ExchangeRate);
            _posInstance.UpdateAllData();
            _posInstance.SaveDataTableToCsv();
            ExchangeRateString = _posInstance.ExchangeRate.ToString();
            Code = "Tipo de Cambio Actualizado";
            CodeColor = Constants.ColorCodeSave;
            //Log
            Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Tipo de Cambio Actualizado:" + " " + ExchangeRateString);
            CurrentPage = "\\View\\PosGeneralPage.xaml";
            
        }
        internal bool CanExecute_ExchangeRateSaveCommand(object parameter)
        {
            return ExchangeRate.ToString() != "" || ExchangeRate > 0.0M;
        }
        #endregion


        #endregion

        #region Login Commands

        #region LoginCheckCommand
        public ICommand LoginCheckCommand { get { return _loginCheckCommand ?? (_loginCheckCommand = new DelegateCommand(Execute_LoginCheckCommand, CanExecute_LoginCheckCommand)); } }
        private ICommand _loginCheckCommand;

        internal void Execute_LoginCheckCommand(object parameter)
        {
            
            var userFound = new User(Constants.DataFolderPath + Constants.UsersFileName).GetByUserName(LoginUserNameText);
            if (userFound.Password == LoginPasswordText)
            {
                AccessLevelGranted = userFound.Rights;
                LoginMessage = String.Format("Welcome {0}", userFound.Name);
                SystemUnlock = true;
                CurrentUser = userFound;
                //Log
                Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Sesion Iniciada Usuario: " + CurrentUser.UserName);
                CurrentPage = "\\View\\PosGeneralPage.xaml";
                LoginMessage = "¡Bienvenido!";
                LoginUserNameText = "";
                LoginPasswordText = "";
                
            }
            else
            {
                SystemUnlock = false;
                LoginMessage = "¡Usuario o Contraseña Incorrecto! Intente de Nuevo...";
                //Log
                Log.Write("Desconocido", this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Inicio de Sesion Fallida: " + LoginUserNameText);
            }
        }
        internal bool CanExecute_LoginCheckCommand(object parameter)
        {
            return
                LoginUserNameText !=
                null; //&& LoginPasswordText != null && LoginPasswordText != "" && LoginPasswordText != "";
        }
        #endregion

        #endregion

        #region ExportDataBaseCommand

        public ICommand ExportDataBaseCommand { get { return _exportDataBaseCommand ?? (_exportDataBaseCommand = new DelegateCommand(Execute_ExportDataBaseCommand, CanExecute_ExportDataBaseCommand)); } }
        private ICommand _exportDataBaseCommand;

        internal void Execute_ExportDataBaseCommand(object parameter)
        {
            //Change main frame page based on the parameter
            switch ((string)parameter)
            {
                case "save_transactions":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Transacciones Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.TransactionsMasterFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Transacciones Exportacion Completada");
                    break;
                case "save_customers":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Clientes Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.CustomersFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Clientes Exportacion Completada");
                    break;
                case "save_vendors":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Proveedores Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.VendorsFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Proveedores Exportacion Completada");
                    break;
                case "save_inventory":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Inventario Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.InventoryFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Inventario Exportacion Completada");
                    break;
                case "save_expenses":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Gastos Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.ExpenseFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Gastos Exportacion Completada");
                    break;
                case "save_orders":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Pedidos Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.OrdersFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Pedidos Exportacion Completada");
                    break;
                case "save_returns":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Devoluciones Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.ReturnsFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Devoluciones Exportacion Completada");
                    break;
                case "save_users":
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Usuarios Exportacion Iniciada");
                    FileIO.MoveFileToUserDefinedFolder(Constants.DataFolderPath + Constants.UsersFileName);
                    //Log
                    Log.Write(CurrentUser.Name, this.ToString() + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, "Usuarios Exportacion Completada");
                    break;
            }
        }

        internal bool CanExecute_ExportDataBaseCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        #region Methods

        #region CartMethods

        public void AddProductToCart(ProductBase product)
        {
            //Check if product already exists in the file
            if (product.Price != 0M)
            {
                if(!CategoriesList.Contains(product.Category)) product.Category = "General";
                for (var index = 0; index < CurrentCartProducts.Count; index++)
                {
                    if (product.Code == null || (product.Code != _currentCartProducts[index].Code) || product.Price == 0M) continue;
                    AddOneAdditinoalQuantityToProductInCart(CurrentCartProducts[index], index);
                    PaymentTotalMXN = CalculateCurrentCartTotal();
                    return;
                }
                //end test
                CurrentCartProducts.Insert(0, product);
                PaymentTotalMXN = CalculateCurrentCartTotal();
            }
            else
            {
                PosGeneralPageViewModel.GetInstance().ManualProduct = product;
                //Set price currency based on product
                if (product.PriceCurrency == CurrencyTypeEnum.USD)
                {
                    PosGeneralPageViewModel.GetInstance().UsdEnabled = true;
                }
                else
                {
                    PosGeneralPageViewModel.GetInstance().UsdEnabled = false;
                }
                CurrentPage = "\\View\\PosGeneralPage.xaml";
            }
        }

        /// <summary>
        /// Method to add a product to a cart
        /// </summary>
        /// <param name="product"></param>
        public void AddManualProductToCart(ProductBase product)
        {
            if (!CategoriesList.Contains(product.Category)) product.Category = "General";
            //Check if product already exists in the file
            if (product.Price != 0M)
            {
                CurrentCartProducts.Insert(0, product);
                PaymentTotalMXN = CalculateCurrentCartTotal();
            }
            else
            {
                PosGeneralPageViewModel.GetInstance().ManualProduct = product;
                CurrentPage = "\\View\\PosGeneralPage.xaml";
            }
        }

        public void RemoveProductFromCart(ProductBase product)
        {
            if (CurrentCartProducts.Contains(product))
            {
                CurrentCartProducts.Remove(product);
            }
        }

        public decimal CalculateCurrentCartTotal()
        {
            var total = 0M;
            foreach(var item in _currentCartProducts)
            {
                total += item.Price * item.LastQuantitySold;
            }
            return total;
        }

        public void AddOneAdditinoalQuantityToProductInCart(ProductBase product, int cartIndex)
        {
            var productIndex = CurrentCartProducts.IndexOf(product);
            product.LastQuantitySold = product.LastQuantitySold + 1;
            CurrentCartProducts.RemoveAt(productIndex);
            CurrentCartProducts.Insert(productIndex,product);
        }
        #endregion

        #region CheckOutProcessMethods
        /// <summary>
        /// Method to initialize payment page and make payment
        /// </summary>
        /// <returns></returns>
        private bool ProcessInventoryRemoval(TransactionType transactionType)
        {
            RecordInventoryRemovalTransaction(PaymentTypeEnum.Desconocido, transactionType, out var currentTransaction);
            CurrentCartProducts.Clear();
            return true;
        }

        /// <summary>
        /// Method to initialize payment process based on user input
        /// </summary>
        /// <param name="paymentType"></param>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        private bool ProcessPayment(PaymentTypeEnum paymentType, TransactionType transactionType)
        {
            //Get all transaction info and push it to the db
            RecordTransaction(paymentType, transactionType, out var currentTransaction);

            //Check if it is a partial payment based on the properties
            var total = PaymentPartialOtherMXN + PaymentPartialCardMXN + PaymentPartialCashMXN + Math.Round(PaymentPartialCashUSD * ExchangeRate, 2) +
                        PaymentPartialCheckMXN + PaymentPartialTransferMXN;

            //If it is partial payment
            if (total > 0)
            {
                PaymentChangeMXN = Math.Round(total - currentTransaction.TotalDue, 2);
                PaymentChangeUSD = Math.Round(PaymentChangeMXN / ExchangeRate, 2);
                PaymentReceivedMXN = PaymentPartialOtherMXN + PaymentPartialCardMXN + PaymentPartialCashMXN +
                                     PaymentPartialCheckMXN + PaymentPartialTransferMXN;
                PaymentReceivedUSD = PaymentPartialCashUSD;
            }
            else
            {
                switch (paymentType)
                {
                    case PaymentTypeEnum.Efectivo:
                        PaymentPartialCashMXN = PaymentReceivedMXN;
                        PaymentPartialCashUSD = PaymentReceivedUSD;
                        PaymentChangeMXN = Math.Round((PaymentReceivedMXN + PaymentReceivedUSD * ExchangeRate) - PaymentTotalMXN, 2);
                        PaymentChangeUSD = Math.Round(PaymentChangeMXN / ExchangeRate, 2);
                        break;
                    case PaymentTypeEnum.Tarjeta:
                        PaymentPartialCardMXN = PaymentTotalMXN;
                        PaymentChangeMXN = 0;
                        PaymentChangeUSD = 0;
                        break;
                    case PaymentTypeEnum.Cheque:
                        PaymentPartialCheckMXN = PaymentTotalMXN;
                        PaymentChangeMXN = 0;
                        PaymentChangeUSD = 0;
                        break;
                    case PaymentTypeEnum.Transferencia:
                        PaymentPartialTransferMXN = PaymentTotalMXN;
                        PaymentChangeMXN = 0;
                        PaymentChangeUSD = 0;
                        break;
                }
            }

            //Record transaction in both payments tables db
            Transaction.RecordPaymentTransaction(Constants.DataFolderPath + Constants.TransactionsPaymentsXFileName, currentTransaction.ReceiptNumber, CurrentUser.Name,
                CurrentCustomer != null ? CurrentUser.Name : "General", currentTransaction.TransactionDate.ToString("G"), ExchangeRate, currentTransaction.FiscalReceiptRequired,
                currentTransaction.TotalDue, CurrencyTypeEnum.MXN, transactionType, PaymentPartialCashMXN, PaymentPartialCashUSD, PaymentPartialCardMXN,
                PaymentPartialCheckMXN, PaymentPartialTransferMXN, PaymentPartialOtherMXN, PaymentChangeMXN, currentTransaction.TotalDue);

            Transaction.RecordPaymentTransaction(Constants.DataFolderPath + Constants.TransactionsPaymentsZFileName, currentTransaction.ReceiptNumber, CurrentUser.Name,
                CurrentCustomer != null ? CurrentUser.Name : "General", currentTransaction.TransactionDate.ToString("G"), ExchangeRate, currentTransaction.FiscalReceiptRequired,
                currentTransaction.TotalDue, CurrencyTypeEnum.MXN, transactionType, PaymentPartialCashMXN, PaymentPartialCashUSD, PaymentPartialCardMXN,
                PaymentPartialCheckMXN, PaymentPartialTransferMXN, PaymentPartialOtherMXN, PaymentChangeMXN, currentTransaction.TotalDue);

            //Record transaction in history
            Transaction.RecordPaymentTransaction(Constants.DataFolderPath + Constants.TransactionsPaymentsFileName, currentTransaction.ReceiptNumber, CurrentUser.Name,
                CurrentCustomer != null ? CurrentUser.Name : "General", currentTransaction.TransactionDate.ToString("G"), ExchangeRate, currentTransaction.FiscalReceiptRequired,
                currentTransaction.TotalDue, CurrencyTypeEnum.MXN, transactionType, PaymentPartialCashMXN, PaymentPartialCashUSD, PaymentPartialCardMXN,
                PaymentPartialCheckMXN, PaymentPartialTransferMXN, PaymentPartialOtherMXN, PaymentChangeMXN, currentTransaction.TotalDue);

            PrintReceipt(currentTransaction, true);
            
            return true;
        }

        /// <summary>
        /// Method to record transaction
        /// </summary>
        /// <returns></returns>
        private bool RecordInventoryRemovalTransaction(PaymentTypeEnum paymentMethod, TransactionType transactionType, out Transaction transaction)
        {
            //Create new instance
            transaction = new Transaction(Constants.DataFolderPath + Constants.TransactionsFileName,
                Constants.DataFolderPath + Constants.TransactionsMasterFileName, Constants.DataFolderPath +
                Constants.TransactionsHistoryFileName, true);

            //General transaction information
            var transactionNumber = _posInstance.GetNextTransactionNumber();
            var internalNumber = _posInstance.GetNextInternalNumber();
            var user = CurrentUser.Name;
            var fiscalReceipt = "No";
            var saleType = transactionType;
            var paymentType = paymentMethod;
            //TODO: if order is created, add the number of the order here
            int orderNumber = 0;
            var transactionDate = DateTime.Now;
            decimal totalDue = 0M;
            string customer = "General";

            //Set receipt number to 0 for internal item inventory removal
            var receiptNumber = 0;

            //Record each item in the transactions db
            foreach (var product in CurrentCartProducts)
            {
                transaction.TransactionNumber = transactionNumber;
                transaction.InternalNumber = internalNumber;
                transaction.ReceiptNumber = receiptNumber;
                transaction.Product = product;
                transaction.TransactionDate = transactionDate;
                transaction.CustomerName = customer;
                transaction.UserName = user;
                transaction.FiscalReceiptRequired = fiscalReceipt;
                transaction.SaleType = saleType;
                transaction.PaymentType = paymentType;
                transaction.OrderNumber = orderNumber;

                //Record Transaction
                transaction.Record(saleType);

                //update inventory for each product, if applicable
                UpdateInventory(product, transactionDate, saleType);
            }

            //Save inventory
            _inventoryInstance.SaveDataTableToCsv();

            return true;
        }

        /// <summary>
        /// Analyze and record transactions and return the transaction information
        /// </summary>
        /// <param name="paymentMethod">Method of payment</param>
        /// <param name="transactionType">Transaction type</param>
        /// <param name="transaction">Output transaction information</param>
        /// <returns></returns>
        private bool RecordTransaction(PaymentTypeEnum paymentMethod, TransactionType transactionType, out Transaction transaction)
        {
            //Create new instance
            //transaction = new Transaction(Constants.DataFolderPath + Constants.TransactionsFileName,
            //    Constants.DataFolderPath + Constants.TransactionsMasterFileName, Constants.DataFolderPath + 
            //    Constants.TransactionsHistoryFileName, true);

            transaction = new Transaction(Constants.DataFolderPath + Constants.TransactionsXFileName,
                Constants.DataFolderPath + Constants.TransactionsZFileName, Constants.DataFolderPath +
                Constants.TransactionsHistoryFileName, true);

            //General transaction information
            var transactionNumber = _posInstance.GetNextTransactionNumber();
 //           var internalNumber = _posInstance.GetNextInternalNumber();
            var user = CurrentUser.Name;
            var fiscalReceipt = "No";
            var saleType = transactionType;
            var paymentType = paymentMethod;
            int orderNumber = 0;
            var transactionDate = DateTime.Now;
            decimal totalDue = 0M;
            string customer = "General";
            
            //Get customer, if registered
            if(CurrentCustomer != null)
                customer = CurrentCustomer.Name;

            //Check if it is a return
            if (transactionType == TransactionType.DevolucionEfectivo || transactionType == TransactionType.DevolucionTarjeta)
            {
                orderNumber = ReturnID;
            }

            //Get next receipt number, if applicable
            var receiptNumber = saleType == TransactionType.Interno ?_posInstance.LastReceiptNumber : _posInstance.GetNextReceiptNumber();

            //Record each item in the transactions db
            foreach (var product in CurrentCartProducts)
            {
                transaction.TransactionNumber = transactionNumber;
                //transaction.InternalNumber = internalNumber;
                transaction.InternalNumber = 0;
                transaction.ReceiptNumber = receiptNumber;
                transaction.Product = product;
                transaction.TransactionDate = transactionDate;
                transaction.CustomerName = customer;
                transaction.UserName = user;
                transaction.FiscalReceiptRequired = fiscalReceipt;
                transaction.SaleType = saleType;
                transaction.PaymentType = paymentType;
                transaction.OrderNumber = orderNumber;

                //Record Transaction in all the transaction files
                transaction.Record(saleType);

                //update inventory for each product, if applicable
                UpdateInventory(product, transactionDate, saleType);
                totalDue += product.Price * product.LastQuantitySold;
            }

            //Set total due amount and total paid and calculate the change due
            transaction.TotalDue = totalDue;
            //Calculate amount paid if it is in cash
            if (PaymentReceivedMXN == 0 && paymentType == PaymentTypeEnum.Efectivo)
            {
                transaction.AmountPaid = paymentType == PaymentTypeEnum.Efectivo ? Math.Round(PaymentReceivedUSD * ExchangeRate, 2) : PaymentTotalMXN;
            }
            else
            {
                transaction.AmountPaid = paymentType == PaymentTypeEnum.Efectivo ? PaymentReceivedMXN : PaymentTotalMXN;
            }
            transaction.ChangeDue = transaction.AmountPaid - transaction.TotalDue;

            //Save inventory
            _inventoryInstance.SaveDataTableToCsv();

            //Save pos data
            _posInstance.SaveDataTableToCsv();

            if (CurrentCustomer != null)
            {
                PaymentPointsReceived = Math.Round(Convert.ToDouble(transaction.TotalDue * PointsConvertionRatio), 2);

                if (transactionType != TransactionType.DevolucionEfectivo && transactionType != TransactionType.DevolucionTarjeta && transactionType != TransactionType.Remover)
                {
                    CurrentCustomer.PointsAvailable += PaymentPointsReceived;
                    CurrentCustomer.TotalSpent += transaction.TotalDue;
                }
                else
                {
                    CurrentCustomer.PointsAvailable -= PaymentPointsReceived;
                    CurrentCustomer.TotalSpent -= transaction.TotalDue;
                }
                CurrentCustomer.TotalVisits += 1;
                CurrentCustomer.PointsAvailable -= PaymentPointsInUse;
                CurrentCustomer.PointsUsed += PaymentPointsInUse;
                CurrentCustomer.LastVisitDate = DateTime.Now;
                CurrentCustomer.UpdateUserToTable();
                CurrentCustomer.SaveDataTableToCsv();
                _inventoryInstance.LoadCsvToDataTable();
            }

            return true;
        }

        /// <summary>
        /// Method to update inventory after a transaction is sucessful
        /// </summary>
        /// <param name="product"></param>
        /// <param name="transactionDate"></param>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        private bool UpdateInventory(ProductBase product, DateTime transactionDate, TransactionType transactionType)
        {
            if (product.Id == 0) return false;

            var invProduct = _inventoryInstance.GetProduct(product.Code);

            if (transactionType == TransactionType.Regular || transactionType == TransactionType.Interno || 
                transactionType == TransactionType.Remover)
            {
                if (invProduct.LocalQuantityAvailable > 0)
                {
                    invProduct.LocalQuantityAvailable = (invProduct.LocalQuantityAvailable - product.LastQuantitySold) > 0 ?
                        (invProduct.LocalQuantityAvailable - product.LastQuantitySold) : 0;
                }

                if (invProduct.TotalQuantityAvailable > 0)
                {
                    invProduct.TotalQuantityAvailable = (invProduct.TotalQuantityAvailable - product.LastQuantitySold) > 0 ?
                        (invProduct.TotalQuantityAvailable - product.LastQuantitySold) : 0;
                }

                invProduct.LastSaleDate = transactionDate;

                if (transactionType == TransactionType.Remover)
                {
                    invProduct.InternalQuantity = invProduct.InternalQuantity + product.LastQuantitySold;
                }

                if (transactionType == TransactionType.Regular || transactionType == TransactionType.Interno)
                {
                    invProduct.QuantitySold = invProduct.QuantitySold + product.LastQuantitySold;
                }

                if (transactionType != TransactionType.Remover)
                {
                    invProduct.AmountSold = invProduct.AmountSold + product.LastAmountSold;
                }

                _inventoryInstance.UpdateProductToTable(invProduct);
            }
            else
            {
                //if it is a return
                if (invProduct.LocalQuantityAvailable > 0)
                {
                    invProduct.LocalQuantityAvailable = invProduct.LocalQuantityAvailable + product.LastQuantitySold;
                }

                if (invProduct.TotalQuantityAvailable > 0)
                {
                    invProduct.TotalQuantityAvailable = invProduct.TotalQuantityAvailable + product.LastQuantitySold;
                }

                invProduct.LastSaleDate = transactionDate;
                invProduct.QuantitySold = invProduct.QuantitySold + product.LastQuantitySold;
                invProduct.AmountSold = invProduct.AmountSold + product.LastAmountSold;

                _inventoryInstance.UpdateProductToTable(invProduct);
            }

            return true;
        }

        /// <summary>
        /// Method to print receipt of the transaction
        /// </summary>
        /// <returns></returns>
        private bool PrintReceipt(Transaction transaction, bool printToFileOnly = false)
        {
            //Receipt for individual sale
            var salesData = new SalesDataStruct()
            {
                User = CurrentUser,
                Customer = CurrentCustomer,
                PointsObtained = PaymentPointsReceived,
                ReceiptType = ReceiptType.RegularSale,
                Products = CurrentCartProducts      
            };

            var receipt = new Receipt(_posInstance, transaction, salesData);
            receipt.PrintSalesReceipt();

            return true;
        }

        #endregion
     
        #region ProductsPageMethods
 
        /// <summary>
        /// Get items list and title for the pages
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public ObservableCollection<string> GetPageItemsList(int pageNumber, out string pageTitle)
        {
            List<string> items;

            switch (pageNumber)
            {
                case 1:
                    {
                        var tempList = new ObservableCollection<ProductBase>();
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageOne);
                        pageTitle = items.First();
                        items.RemoveAt(0);
                        CurrentPageListProducts = tempList;
                        break;
                    }

                case 2:
                    {
                        var tempList = new ObservableCollection<ProductBase>();
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageTwo);
                        pageTitle = items.First();
                        items.RemoveAt(0);
                        CurrentPageListProducts = tempList;
                        break;
                    }

                case 3:
                    {
                        var tempList = new ObservableCollection<ProductBase>();
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageThree);
                        pageTitle = items.First();
                        items.RemoveAt(0);
                        CurrentPageListProducts = tempList;
                        break;
                    }
                case 4:
                    {
                        var tempList = new ObservableCollection<ProductBase>();
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageFour);
                        pageTitle = items.First();
                        items.RemoveAt(0);
                        CurrentPageListProducts = tempList;
                        break;
                    }
                case 5:
                    {
                        var tempList = new ObservableCollection<ProductBase>();
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageFive);
                        pageTitle = items.First();
                        items.RemoveAt(0);
                        CurrentPageListProducts = tempList;
                        break;
                    }
                default:
                    {
                        items = new List<string>() { "Varios" };
                        pageTitle = "Pagina de Productos";
                        break;
                    }
            }
            return new ObservableCollection<string>(items);
        }

       
        /// <summary>
        /// Get products list and title for the pages
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageTitle"></param>
        /// <returns></returns>
        public ObservableCollection<ProductBase> GetPageProductsList(int pageNumber, out string pageTitle)
        {
            List<ProductBase> items;
            ObservableCollection<ProductBase> products;
            switch (pageNumber)
            {
                case 1:
                    {
                        items = InventoryBase.GetProductList(Constants.DataFolderPath + Constants.ProductPageOne, out pageTitle);
                        products = new ObservableCollection<ProductBase>(items);
                        CurrentPageListProducts = products;
                        PageOneTitle = pageTitle;
                        //TODO: need to change to only change page titles when the changes are saved
                        CurrentPageListTitle = pageTitle;
                        CurrentSelectedItemsListPageFileName = Constants.DataFolderPath + Constants.ProductPageOne;
                        break;
                    }

                case 2:
                    {
                        items = InventoryBase.GetProductList(Constants.DataFolderPath + Constants.ProductPageTwo, out pageTitle);
                        products = new ObservableCollection<ProductBase>(items);
                        CurrentPageListProducts = products;
                        PageTwoTitle = pageTitle;
                        CurrentPageListTitle = pageTitle;
                        CurrentSelectedItemsListPageFileName = Constants.DataFolderPath + Constants.ProductPageTwo;
                        break;
                    }

                case 3:
                    {
                        items = InventoryBase.GetProductList(Constants.DataFolderPath + Constants.ProductPageThree, out pageTitle);
                        products = new ObservableCollection<ProductBase>(items);
                        CurrentPageListProducts = products;
                        PageThreeTitle = pageTitle;
                        CurrentPageListTitle = pageTitle;
                        CurrentSelectedItemsListPageFileName = Constants.DataFolderPath + Constants.ProductPageThree;
                        break;
                    }
                case 4:
                    {
                        items = InventoryBase.GetProductList(Constants.DataFolderPath + Constants.ProductPageFour, out pageTitle);
                        products = new ObservableCollection<ProductBase>(items);
                        CurrentPageListProducts = products;
                        PageFourTitle = pageTitle;
                        CurrentPageListTitle = pageTitle;
                        CurrentSelectedItemsListPageFileName = Constants.DataFolderPath + Constants.ProductPageFour;
                        break;
                    }
                case 5:
                    {
                        items = InventoryBase.GetProductList(Constants.DataFolderPath + Constants.ProductPageFive, out pageTitle);
                        products = new ObservableCollection<ProductBase>(items);
                        CurrentPageListProducts = products;
                        PageFiveTitle = pageTitle;
                        CurrentPageListTitle = pageTitle;
                        CurrentSelectedItemsListPageFileName = Constants.DataFolderPath + Constants.ProductPageFive;
                        break;
                    }
                default:
                    {
                        items = new List<ProductBase>() { ProductBase.Add("Varios", "Varios", 1, 1)};
                        products = new ObservableCollection<ProductBase>(items);
                        pageTitle = "Pagina de Productos";
                        break;
                    }
            }
            return products;
        }

        /// <summary>
        /// Set page titles based on the initial values
        /// </summary>
        private void GetInitialPagesTitles()
        {
            PageOneTitle = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageOne).First();
            PageTwoTitle = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageTwo).First();
            PageThreeTitle = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageThree).First();
            PageFourTitle = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageFour).First();
            PageFiveTitle = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageFive).First();
        }

        #endregion

        #region Inventory Methods

        public bool RemoveImage()
        {
            //Remove image from product
            return true;
        }

        public string SelectImage()
        {
            //Open dialog and select jpg image
            var dialog = new Microsoft.Win32.OpenFileDialog {DefaultExt = ".jpg"};
            //Display dialog
            bool? result = dialog.ShowDialog();

            if(result == true)
            {
                var fileName = Path.GetFileName(dialog.FileName);

                //Move the file to the images file and append the time at the beginning of the name
                fileName = DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() +
                            DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + "_" + fileName;
                
                File.Copy(dialog.FileName, Constants.DataFolderPath + Constants.ImagesFolderPath + fileName);
                return fileName;
            }
            else
            {
                return null;
            }
        }

        public void SaveProductIntoTemporalProduct()
        {

        }

        #endregion

        #region Payment Methods

        public void PaymentUpdateRemaining()
        {
            PaymentRemainingMXN = PaymentTotalMXN - PaymentReceivedMXN - Math.Round(PaymentReceivedUSD*ExchangeRate, 2);
            PaymentRemainingUSD = Math.Round(PaymentRemainingMXN / ExchangeRate, 2);
        }

        public void PaymentPartialUpdateRemaining()
        {
            PaymentRemainingMXN = PaymentTotalMXN - PaymentPartialCashMXN - PaymentPartialCardMXN - PaymentPartialCheckMXN - PaymentPartialTransferMXN - PaymentPartialOtherMXN - Math.Round(PaymentPartialCashUSD * ExchangeRate, 2);
            PaymentRemainingUSD = Math.Round(PaymentRemainingMXN / ExchangeRate, 2);
        }

        /// <summary>
        /// Initializes main payment procedure
        /// </summary>
        /// <param name="parameter">Method of payment</param>
        /// <param name="transactionType">Payment transaction type</param>
        public void PaymentProcessStart(string parameter, TransactionType transactionType)
        {
            //Get payment type from string
            var status = PaymentTypeEnum.TryParse(parameter, out PaymentTypeEnum paymentType);
            if (status == false)
            {
                paymentType = PaymentTypeEnum.Efectivo;
            }

            //Check if it is a return
            if (ReturnTransaction)
            {
                if (paymentType == PaymentTypeEnum.Efectivo)
                {
                    transactionType = TransactionType.DevolucionEfectivo;
                }
                else if (paymentType == PaymentTypeEnum.Tarjeta)
                {
                    transactionType = TransactionType.DevolucionTarjeta;
                }
                else
                {
                    Code = "Transaccion Invalida";
                    return;
                }
            }

            ProcessPayment(paymentType, transactionType);

            CurrentCartProducts.Clear();
            if(paymentType != PaymentTypeEnum.Efectivo)
                PaymentTotalMXN = 0;
        }

        public void PaymentEndProcess(string parameter)
        {
            //Clear all properties and return to general page
            PaymentTotalMXN = 0;
            PaymentTotalUSD = 0;
            PaymentReceivedMXN = 0;
            PaymentReceivedUSD = 0;
            PaymentChangeMXN = 0;
            PaymentChangeUSD = 0;
            PaymentPointsReceived = 0;
            PaymentPointsInUse = 0;
            Code = "";
            ReturnID = 0;
            PaymentPartialOtherMXN = 0;
            PaymentPartialCardMXN = 0;
            PaymentPartialCashMXN = 0;
            PaymentPartialCashUSD = 0;
            PaymentPartialCheckMXN = 0;
            PaymentPartialTransferMXN = 0;

            ReturnTransaction = false;
            CurrentCustomer = null;
        }

        public void InventoryRemovalProcessStart(TransactionType transactionType)
        {
            ProcessInventoryRemoval(transactionType);
            CurrentCartProducts.Clear();
        }

        #endregion

        #region Main Page Methods
        /// <summary>
        /// Clears all the searchable lists 
        /// </summary>
        private void ClearSearchLists()
        {
            InventorySearchedProducts = null;
            SelectedInventoryProduct = null;
            UsersSearchedEntries = null;
            SelectedUser = null;
            CustomersSearchedEntries = null;
            SelectedCustomer = null;
            VendorsSearchedEntries = null;
            SelectedVendor = null;
            OrdersSearchedEntries = null;
            SelectedOrder = null;
            ExpensesSearchedEntries = null;
            SelectedExpense = null;
            TransactionsSearchedEntries = null;
            SelectedTransaction = null;
        }

        private void ClearPaymentInput()
        {
            PaymentReceivedMXN = 0;
            PaymentReceivedUSD = 0;
            PaymentPartialCashMXN = 0;
            PaymentPartialCashUSD = 0;
            PaymentPartialCheckMXN = 0;
            PaymentPartialTransferMXN = 0;
            PaymentPartialCardMXN = 0;
            PaymentPartialOtherMXN = 0;
        }

        #endregion

        #endregion

    }
}