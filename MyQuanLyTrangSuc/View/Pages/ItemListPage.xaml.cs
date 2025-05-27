using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;

namespace MyQuanLyTrangSuc.View
{
    public partial class ItemListPage : Page
    {
        private readonly ItemListPageLogic _logicService;

        public ItemListPage()
        {
            InitializeComponent();
            _logicService = new ItemListPageLogic();
            DataContext = _logicService;
        }

        private void TextChanged_Search(object sender, RoutedEventArgs e)
        {
            _logicService.SearchItemsByName(SearchTextBox.Text);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _logicService.LoadProducts();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                _logicService.SelectedCategory = comboBox.SelectedItem as ProductCategory;
            }
        }

        private void OnClick_Add_ItemList(object sender, RoutedEventArgs e)
        {
            _logicService.LoadAddItemWindow();
        }

        private void ComboBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.SelectedIndex = -1;
                _logicService.SelectedCategory = null;
                _logicService.LoadProducts();
            }
        }
    }
}