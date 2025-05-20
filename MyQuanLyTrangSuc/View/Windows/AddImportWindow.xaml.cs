using MyQuanLyTrangSuc.Model;
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
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for AddImportRecordWindow.xaml
    /// </summary>
    public partial class AddImportWindow : Window
    {
        private readonly AddImportRecordWindowLogic logicService;
        public AddImportWindow()
        {
            InitializeComponent();
            logicService = new AddImportRecordWindowLogic();
            DataContext = logicService;
        }

        private void ImportItemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //logicService.ItemSelectionChanged();
        }

        private void addImportDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            logicService.AddImportDetail();
        }

        private void importDetailsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is ImportDetail selectedDetail)
            {
                if (MessageBox.Show($"Do you want to remove this import detail",
                                    "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    logicService.RemoveImportDetail(selectedDetail);
                }
            }
        }

        private void addNewItemBtn_Click(object sender, RoutedEventArgs e)
        {
            AddItemWindow addItemWindow = new AddItemWindow();
            addItemWindow.ShowDialog();
        }

        private void addNewSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            AddSupplierWindow addSupplierWindow = new AddSupplierWindow();
            addSupplierWindow.ShowDialog();
        }

        private void applyImportBtn_Click(object sender, RoutedEventArgs e)
        {
            logicService.AddImport();
        }
    }
}
