using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for ItemCategoryListPage.xaml
    /// </summary>
    public partial class ItemCategoryListPage : Page
    {
        private readonly ItemCategoryListPageLogic logicService;
        public ItemCategoryListPage()
        {
            InitializeComponent();
            logicService = new ItemCategoryListPageLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddItemCategoryWindow();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadEditItemCategoryWindow((Model.ProductCategory)itemCategoriesDataGrid.SelectedItem);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteItemCategory((Model.ProductCategory)itemCategoriesDataGrid.SelectedItem);
        }

        private void deleteMultipleButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteMultipleItemCategories();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            logicService.CheckBox_Checked(sender, e);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            logicService.CheckBox_Unchecked(sender, e);
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();

                if (selectedValue == "Name")
                {
                    logicService.ItemCategoriesSearchByName(searchTextBox.Text);
                }
                else if (selectedValue == "ID")
                {
                    logicService.ItemCategoriesSearchByID(searchTextBox.Text);
                }
            }
        }
    }
}
