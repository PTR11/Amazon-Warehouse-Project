using Brute_Force.Model;
using Brute_Force.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Brute_Force.View
{

    public partial class ShelfProduct : Window
    {
        #region Fields
        public ViewModelClass viewModel;
        private List<Shelf> shelves = new List<Shelf>();
        public ObservableCollection<Item> items = new ObservableCollection<Item>();
        public ObservableCollection<Product> prItems = new ObservableCollection<Product>();
        private bool added;
        #endregion

        #region Constructor
        /// <summary>
        /// Construct this window
        /// </summary>
        /// <param name="vm">ViewModel is given to set up datas</param>
        public ShelfProduct(ViewModelClass vm)
        {
            InitializeComponent();
            int i = 1;
            this.viewModel = vm;
            added = false;
            shelves = viewModel.designatedShelfList;
            shelves.ForEach(item =>
            {
                List<String> pc = new List<String>();
                if (item.Products.Count == 0)
                {
                    pc.Add("Nincs termék");
                }
                else
                {
                    item.Products.ForEach(p => { pc.Add(p + ""); });
                }
                items.Add(
                    new Item
                    {
                        Coord = item.Position.ToString(),
                        Name = "Polc" + i.ToString(),
                        ProductsCount = item.NumOfProducts,
                        Products = pc
                    });
                i++;
            });

            listView.ItemsSource = items;
        }
        #endregion

        #region Method
        /// <summary>
        /// Add product to shelves
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void AddProduct(object sender, RoutedEventArgs e)
        {
            foreach (Item i in items)
            {

                if (product.Text != "")
                {
                    try
                    {
                        int converted = Int32.Parse(product.Text.Trim());
                        if (converted > viewModel.Model.StationsListProp.Count)
                        {
                            MessageBox.Show("A megadott érték nem lehet nagyobb mint az állomások száma!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                            product.Text = "";
                            return;
                        }
                        if (i.Products.Contains(product.Text.Trim()))
                        {
                            MessageBox.Show("A megadott termék már szerepel a kijelölt polcokon, vagy a kijelölt polcok közül valamelyiken. \n" +
                                "Ha új polchoz szeretné hozzáadni, módosítsa a kijelölést", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                            product.Text = "";
                            return;
                        }
                        else
                        {
                            if (i.Products.Count != 0 && i.Products[0] == "Nincs termék")
                            {
                                i.Products.Clear();
                            }
                            i.Products.Add(product.Text.Trim());
                            added = true;
                            i.ProductsCount++;
                        }
                    }
                    catch (System.FormatException)
                    {
                        MessageBox.Show("A termékek azonosítása számmal történik, számot adjon meg!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }

                if (productsList.Visibility == Visibility.Visible && (Item)listView.SelectedItem == i)
                {
                    RefreshList(i);
                }
            }
            product.Text = "";
            listView.Items.Refresh();


        }
        /// <summary>
        /// Refresh products list on the right side by the clicked item.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Item asd = (Item)listView.SelectedItem;
            productsList.Visibility = Visibility.Visible;
            RefreshList(asd);
        }
        /// <summary>
        /// Refresh shelves list
        /// </summary>
        /// <param name="item">The item what we wants to add</param>
        public void RefreshList(Item item)
        {
            prItems.Clear();
            if (item.ProductsCount != 0)
            {
                foreach (String s in item.Products)
                {
                    prItems.Add(new Product { Parent = item, Products = s });
                }
            }
            else
            {
                prItems.Add(new Product { Parent = item, Products = "Nincs termék" });
            }
            productsList.ItemsSource = prItems;
            productsList.Items.Refresh();
        }
        /// <summary>
        /// Open context menu to delete the chosen product
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void OnListViewItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Product deleteProduct = (Product)productsList.SelectedItem;
            if (deleteProduct != null)
            {
                ContextMenu cm = this.FindResource("DeleteMenu") as ContextMenu;
                cm.IsOpen = true;
            }
        }
        /// <summary>
        /// Delete the item
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Item deleteItem = (Item)listView.SelectedItem;
            Product deleteProduct = (Product)productsList.SelectedItem;
            foreach (Item item in items)
            {
                if (item == deleteItem)
                {
                    item.Products.Remove(deleteProduct.Products);
                    item.ProductsCount = item.Products.Count;
                    break;
                }
            }
            listView.Items.Refresh();
            prItems.Remove(deleteProduct);
            productsList.Items.Refresh();
        }
        /// <summary>
        /// Close the window.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        public void Window_Closing(object sender, CancelEventArgs e)
        {
            if (added)
            {
                viewModel.AddProductToShelf(items);
            }
        }
        #endregion
    }
}