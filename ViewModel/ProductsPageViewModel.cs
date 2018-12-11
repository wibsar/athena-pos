using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Athena.WpfBindingUtilities;

namespace Athena
{
    /// <summary>
    /// Class for the product list page where the items are selected
    /// </summary>
    public class ProductsPageViewModel : ObservableObject
    {
        #region Fields
        private static ObservableCollection<string> _products;
        private static string _pageOneButtonTitle;
        private static string _pageTwoButtonTitle;
        private static string _pageThreeButtonTitle;
        private static string _pageFourButtonTitle;
        private static string _pageFiveButtonTitle;
        #endregion

        #region Constructors

        public ProductsPageViewModel()
        {
            Products = this.GetPageItemsList(MainWindowViewModel.LastSelectedProductsPage);
        }  

        #endregion

        #region Observable Properties

        public ObservableCollection<string> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public string PageOneButtonTitle
        {
            get
            {
                return _pageOneButtonTitle;
            }
            set
            {
                _pageOneButtonTitle = value;
                OnPropertyChanged();
            }
        }

        public string PageTwoButtonTitle
        {
            get
            {
                return _pageTwoButtonTitle;
            }
            set
            {
                _pageTwoButtonTitle = value;
                OnPropertyChanged();
            }
        }

        public string PageThreeButtonTitle
        {
            get
            {
                return _pageThreeButtonTitle;
            }
            set
            {
                _pageThreeButtonTitle = value;
                OnPropertyChanged();
            }
        }

        public string PageFourButtonTitle
        {
            get
            {
                return _pageFourButtonTitle;
            }
            set
            {
                _pageFourButtonTitle = value;
                OnPropertyChanged();
            }
        }

        public string PageFiveButtonTitle
        {
            get
            {
                return _pageFiveButtonTitle;
            }
            set
            {
                _pageFiveButtonTitle = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region SelectItemCommand

        public ICommand SelectItemCommand { get { return _selectItemCommand ?? (_selectItemCommand = new DelegateCommand(Execute_SelectItemCommand, CanExecute_SelectItemCommand)); } }
        private ICommand _selectItemCommand;

        internal void Execute_SelectItemCommand(object parameter)
        {
            //TODO: Check to make sure the item is found, otherwise show error message
            var product = MainWindowViewModel.InventoryInstance.GetProduct(parameter.ToString());
            product.LastQuantitySold += 1;
            //MainWindowViewModel.AddManualProductToCart(product); changed static
            var main = MainWindowViewModel.GetInstance();
            main.AddManualProductToCart(product);
        }

        internal bool CanExecute_SelectItemCommand(object parameter)
        {
            return parameter != null;
        }

        #endregion

        #region LoadProductsListCommand

        public ICommand LoadProductsListCommand { get { return _loadProductsListCommand ?? (_loadProductsListCommand = new DelegateCommand(Execute_LoadProductsListCommand, CanExecute_LoadProductsListCommand)); } }
        private ICommand _loadProductsListCommand;

        internal void Execute_LoadProductsListCommand(object parameter)
        {
            //Re-loads the list of products to be shown in the page
            var temp = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageOne);
            _products = new ObservableCollection<string>(temp);
        }

        internal bool CanExecute_LoadProductsListCommand(object parameter)
        {
            //Add logic to check if the command can be executed (if any)
            return true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get items list and title for the pages
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public ObservableCollection<string> GetPageItemsList(int pageNumber)
        {
            List<string> items;

            switch (pageNumber)
            {
                case 1:
                    {
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageOne);
                        _pageOneButtonTitle = items.First();
                        items.RemoveAt(0);
                        break;
                    }

                case 2:
                    {
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageTwo);
                        _pageTwoButtonTitle = items.First();
                        items.RemoveAt(0);
                        break;
                    }

                case 3:
                    {
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageThree);
                        _pageThreeButtonTitle = items.First();
                        items.RemoveAt(0);
                        break;
                    }
                case 4:
                    {
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageFour);
                        _pageFourButtonTitle = items.First();
                        items.RemoveAt(0);
                        break;
                    }
                case 5:
                    {
                        items = CategoryCatalog.GetList(Constants.DataFolderPath + Constants.ProductPageFive);
                        _pageFiveButtonTitle = items.First();
                        items.RemoveAt(0);
                        break;
                    }
                default:
                    {
                        items = null;
                        break;
                    }
            }
            return new ObservableCollection<string>(items);
        }

        #endregion
    }
}
