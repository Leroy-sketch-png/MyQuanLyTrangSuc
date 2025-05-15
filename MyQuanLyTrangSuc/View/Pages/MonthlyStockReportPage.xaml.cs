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
using MyQuanLyTrangSuc.ViewModel;

namespace MyQuanLyTrangSuc.View
{
    //public partial class MonthlyStockReportPage : Page
    //{
    //    private MonthlyStockReportPageLogic _logic;

    //    public MonthlyStockReportPage()
    //    {
    //        InitializeComponent();
    //        _logic = new MonthlyStockReportPageLogic(this);
    //        this.DataContext = _logic;
    //    }

    //    private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
    //    {
    //        _logic.GenerateMonthlyReport();
    //    }

    //    private void ImportExcelFileButton_Click(object sender, RoutedEventArgs e)
    //    {
    //        _logic.ImportFromExcel();
    //    }

    //    private void ExportExcelFileButton_Click(object sender, RoutedEventArgs e)
    //    {
    //        _logic.ExportToExcel();
    //    }

    //    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        _logic.FilterReports(searchTextBox.Text, searchComboBox.SelectedIndex);
    //    }

    //    private void SearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //    {
    //        _logic.FilterReports(searchTextBox.Text, searchComboBox.SelectedIndex);
    //    }

    //    //private void StockReportDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //    //{
    //    //    _logic.OnMonthReportSelected();
    //    //}

    //    private void ViewButton_Click(object sender, RoutedEventArgs e)
    //    {
    //        if (sender is Button button && button.DataContext is MonthlyStockReportViewModel report)
    //        {
    //            _logic.ViewReportDetails(report);
    //        }
    //    }

    //    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    //    {
    //        if (sender is Button button && button.DataContext is MonthlyStockReportViewModel report)
    //        {
    //            _logic.DeleteReport(report);
    //        }
    //    }
    //}
}
