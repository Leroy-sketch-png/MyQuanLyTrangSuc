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
    /// Interaction logic for ItemListPage.xaml
    /// </summary>
    public partial class ItemListPage : Page
    {
        private readonly ItemListPageLogic logicService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public ItemListPage()
        {
            InitializeComponent();
            logicService = new ItemListPageLogic();
            notificationWindowLogic = new NotificationWindowLogic();
            this.DataContext = logicService;
        }

        private void OnClick_Add_ItemList(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddItemWindow();
        }

        private void TextChanged_Search(object sender, RoutedEventArgs e)
        {
            logicService.ItemsSearchByName(SearchTextBox.Text);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            notificationWindowLogic.LoadNotificationForItem();
        }



        
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem == null) return;
            string category = comboBox.SelectedItem.ToString();
            logicService.FilterItemByCategory(category);
        }

        private void ComboBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox.SelectedIndex = -1;
            logicService.FilterItemByCategory("");
        }
    }
}