using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using MyQuanLyTrangSuc.ViewModel;
using MyQuanLyTrangSuc.Model;

namespace MyQuanLyTrangSuc.View
{
    public partial class ItemPropertiesPage : Page
    {
        private readonly ItemPropertiesPageLogic _logicService;

        public ItemPropertiesPage(Product item)
        {
            InitializeComponent();
            _logicService = new ItemPropertiesPageLogic(this);
            _logicService.LoadProductDetails(item.ProductId);
            DataContext = _logicService;
        }

        private void OnClick_Back_ItemPropertiesPage(object sender, RoutedEventArgs e)
        {
            _logicService.LoadItemListPage();
        }

        private void OnClick_Edit_ItemPropertiesPage(object sender, RoutedEventArgs e)
        {
            _logicService.EditItem();
        }

        private void OnClick_EditImage_ItemPropertiesPage(object sender, RoutedEventArgs e)
        {
            _logicService.EditItemImage();
        }

        /// <summary>
        /// Fired when the user changes the selected category in edit mode.
        /// Forwards the new category name to the ViewModel so it can load and apply the matching Unit.
        /// </summary>
        private void OnCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newCategoryName = e.AddedItems[0]?.ToString();
                _logicService.OnCategoryChanged(newCategoryName);
            }
        }

        // Handle numeric stock input restrictions
        private void OnPreviewTextInput_StockTextBox_ItemPropertiesPageUI(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void OnPasting_StockTextBox_ItemPropertiesPageUI(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!text.All(char.IsDigit)) e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        // Handle numeric price input restrictions
        private void OnPreviewTextInput_PriceTextBox_ItemPropertiesPageUI(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ".") e.Handled = true;

            if (sender is TextBox textBox && e.Text == "." && textBox.Text.Contains("."))
                e.Handled = true;
        }

        private void OnPasting_PriceTextBox_ItemPropertiesPageUI(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!text.All(c => char.IsDigit(c) || c == '.')) e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        // Utility functions for numeric validation
        private bool IsTextNumericInt(string text) => text.All(char.IsDigit);

        private bool IsTextNumericDec(string text) => text.All(c => char.IsDigit(c) || c == '.');
    }
}
