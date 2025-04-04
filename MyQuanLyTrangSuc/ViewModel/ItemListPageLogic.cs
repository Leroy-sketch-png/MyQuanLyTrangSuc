using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ItemListPageLogic
    {
        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        public ObservableCollection<Product> products { get; set; }
        public List<string> Categories { get; set; }

        public ItemListPageLogic()
        {
            products = new ObservableCollection<Product>();
            Categories = new List<string>();
            LoadItemsFromDatabase();
            LoadCategoriesFromDatabase();

            context.OnItemAdded += Context_OnItemAdded;
            context.OnItemRemoved += Context_OnItemRemoved;
            context.OnItemsReset += Context_OnItemsReset;
        }


        private void Context_OnItemAdded(Product product)
        {
            products.Add(product);
        }

        private void Context_OnItemRemoved(Product product)
        {
            products.Remove(product);
        }

        private void Context_OnItemsReset()
        {
            LoadItemsFromDatabase();
        }

        private void LoadItemsFromDatabase()
        {
            products.Clear();
            List<Product> productsFromDb = context.Products.ToList();
            foreach (Product product in productsFromDb)
            {
                if (!product.IsDeleted)
                {
                    products.Add(product);
                }
            }
        }

        private void LoadCategoriesFromDatabase()
        {
            Categories.Clear();
            Categories = context.Products
.GroupBy(product => product.CategoryId)
    .Select(group => group.Key)
    .ToList();
        }

        // AddItemWindow is unfinished
        public void LoadAddItemWindow()
        {
            Window addWindow = new AddItemWindow();
            addWindow.ShowDialog();
        }

        public void ItemsSearchByName(string name)
        {
            List<Product> productsFromDb = context.Products.ToList();
            Application.Current.Dispatcher.Invoke(() => {
                products.Clear();
                foreach (Product product in productsFromDb)
                {
                    if (!product.IsDeleted && product.ProductId.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        products.Add(product);
                    }
                }
            });
        }

        public void FilterItemByCategory(string category)
        {
            List<Product> productsFromDb = context.Products.ToList();
            Application.Current.Dispatcher.Invoke(() => {
                products.Clear();
                foreach (Product product in productsFromDb)
                {
                    if (!product.IsDeleted && product.CategoryId.IndexOf(category, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        products.Add(product);
                    }
                }
            });

        }
    }
}
