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

namespace MyQuanLyTrangSuc.View.Windows
{
    /// <summary>
    /// Interaction logic for EditImportWindow.xaml
    /// </summary>
    public partial class EditImportWindow : Window
    {
        private readonly EditImportWindowLogic logicService;
        public EditImportWindow(Import import)
        {
            InitializeComponent();
            logicService = new EditImportWindowLogic(import);
            DataContext = logicService;
        }

        private void addNewItemBtn_Click(object sender, RoutedEventArgs e)
        {
            AddItemWindow addItemWindow = new AddItemWindow();
            bool? result = addItemWindow.ShowDialog();

            if (result == true)
            {
                logicService.LoadInitialData();
            }
        }

        private void addNewSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            AddSupplierWindow addSupplierWindow = new AddSupplierWindow();
            bool? result = addSupplierWindow.ShowDialog();

            if (result == true)
            {
                logicService.LoadInitialData();
            }
        }

        private void addImportDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            logicService.AddImportDetail();
        }

        private void applyImportBtn_Click(object sender, RoutedEventArgs e)
        {
            logicService.SaveImport();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is ImportDetail selectedDetail)
                {
                    if (MessageBox.Show($"Do you want to remove this import detail?",
                                        "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        logicService.RemoveImportDetail(selectedDetail);
                    }
                }
            }
        }
    }
}
