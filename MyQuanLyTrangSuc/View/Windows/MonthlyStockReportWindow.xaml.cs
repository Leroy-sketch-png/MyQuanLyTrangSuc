using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MyQuanLyTrangSuc.Model;
namespace MyQuanLyTrangSuc.View
{
    public partial class MonthlyStockReportWindow : Window
    {
        public MonthlyStockReportWindow(List<StockReport> details, string title)
        {
            InitializeComponent();
            this.DataContext = new MonthlyStockReportWindowLogic(details, title);
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement print functionality
            MessageBox.Show("Print functionality will be implemented here", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement export to Excel functionality
            MessageBox.Show("Export to Excel functionality will be implemented here", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}