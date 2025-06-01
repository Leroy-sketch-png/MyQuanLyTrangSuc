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
    /// Interaction logic for SupplierListPage.xaml
    /// </summary>
    public partial class SupplierListPage : Page
    {
        private readonly SupplierListPageLogic logicService;
        public SupplierListPage()
        {
            InitializeComponent();
            logicService = new SupplierListPageLogic();
            DataContext = logicService;
        }


        private void supplierDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            logicService.SearchSupplierOnGoogle((Model.Supplier)supplierDataGrid.SelectedItem);
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();

                if (selectedValue == "Name")
                {
                    logicService.SuppliersSearchByName(searchTextBox.Text);
                }
                else if (selectedValue == "ID")
                {
                    logicService.SuppliersSearchByID(searchTextBox.Text);
                }
            }

        }
    }
}
