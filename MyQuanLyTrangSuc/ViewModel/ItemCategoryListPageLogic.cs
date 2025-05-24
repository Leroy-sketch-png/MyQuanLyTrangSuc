using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ItemCategoryListPageLogic
    {
        private readonly ItemCategoryService itemCategoryService;

        private ObservableCollection<ProductCategory> itemCategories;

        public ObservableCollection<ProductCategory> ItemCategories
        {
            get => itemCategories;
            set
            {
                itemCategories = value;
                OnPropertyChanged();
            }
        }

        private readonly HashSet<ProductCategory> _selectedItemCategories = new HashSet<ProductCategory>();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //constructor
        public ItemCategoryListPageLogic()
        {
            itemCategoryService = ItemCategoryService.Instance;
            ItemCategories = new ObservableCollection<ProductCategory>();
            LoadItemCategoriesFromDatabase();
            itemCategoryService.OnItemCategoryAdded += ItemCategoryService_OnItemCategoryAdded;
            itemCategoryService.OnItemCategoryUpdated += ItemCategoryService_OnItemCategoryUpdated;
        }

        private void ItemCategoryService_OnItemCategoryUpdated(ProductCategory updatedCategory)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var index = ItemCategories.IndexOf(ItemCategories.FirstOrDefault(c => c.CategoryId == updatedCategory.CategoryId));
                if (index != -1)
                {
                    ItemCategories[index] = updatedCategory;
                }
                else
                {
                    LoadItemCategoriesFromDatabase();
                }
            });
        }

        //catch event for add new product category
        private void ItemCategoryService_OnItemCategoryAdded(ProductCategory category)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ItemCategories.Add(category);
            });
        }

        //load product categories from database
        private void LoadItemCategoriesFromDatabase()
        {
            var itemCategories = itemCategoryService.GetListOfItemCategories().Where(c => !c.IsNotMarketable).ToList();
            ItemCategories = new ObservableCollection<ProductCategory>(itemCategories);
        }

        //Load AddItemCategoryWindow
        public void LoadAddItemCategoryWindow()
        {
            var temp = new AddItemCategoryWindow();
            temp.ShowDialog();
        }


        //Load EditItemCategoryWindow
        public void LoadEditItemCategoryWindow(ProductCategory selectedItem)
        {
            var temp = new EditItemCategoryWindow(selectedItem);
            temp.ShowDialog();
        }

        //Delete item category
        public void DeleteItemCategory(ProductCategory selectedItem)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this item category?", "Delete Item Category", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                itemCategoryService.DeleteItemCategory(selectedItem);
                ItemCategories.Remove(selectedItem);
            }
        }

        //delete multiple item categories
        public void DeleteMultipleItemCategories()
        {
            if (_selectedItemCategories.Count == 0)
            {
                MessageBox.Show("Please select item categories to delete!", "Delete Item Categories", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete these item categories?", "Delete Item Categories", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var itemCategory in _selectedItemCategories)
                {
                    itemCategoryService.DeleteItemCategory(itemCategory);
                    ItemCategories.Remove(itemCategory);
                }
                _selectedItemCategories.Clear();
            }
        }

        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is ProductCategory itemCategory)
            {
                _selectedItemCategories.Add(itemCategory);
            }

        }

        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is ProductCategory itemCategory)
            {
                _selectedItemCategories.Remove(itemCategory);
            }
        }


        //Search item categories by name or id
        public void ItemCategoriesSearchByName(string name)
        {
            var res = itemCategoryService.ItemCategoriesSearchByName(name);
            UpdateItemCategories(res);
        }

        public void ItemCategoriesSearchByID(string id)
        {
            var res = itemCategoryService.ItemCategoriesSearchByID(id);
            UpdateItemCategories(res);

        }
        private void UpdateItemCategories(List<ProductCategory> res)
        {
            if (!ItemCategories.SequenceEqual(res))
            {
                ItemCategories.Clear();
                foreach (var temp in res)
                {
                    ItemCategories.Add(temp);
                }
            }
        }
    }
}
