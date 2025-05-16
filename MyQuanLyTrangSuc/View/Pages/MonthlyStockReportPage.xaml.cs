using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.View
{
    public partial class MonthlyStockReportPage : Page
    {
        private MonthlyStockReportPageLogic logicReport;

        public MonthlyStockReportPage()
        {
            InitializeComponent();
            logicReport = new MonthlyStockReportPageLogic();
            this.DataContext = logicReport;
        }

        private void OnDoubleClick_InspectReport_MonthlyStockReportPageDataGrid(object sender, MouseButtonEventArgs e)
        {
            logicReport.LoadReportDetailsWindow();
        }

        //private void ViewButton_Click(object sender, RoutedEventArgs e)
        //{
        //    logicReport.LoadReportDetailsWindow();
        //}

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            logicReport.CreateOrUpdateCurrentMonthReport();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicReport.DeleteStockReport();
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            logicReport.CreateOrUpdateCurrentMonthReport();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchTextBlock.Text = "";
            if (searchComboBox.SelectionBoxItem.ToString() == "Month")
                logicReport.ReportsSearchByMonth(searchTextBox.Text);
            else if (searchComboBox.SelectionBoxItem.ToString() == "Year")
                logicReport.ReportsSearchByYear(searchTextBox.Text);
            if (searchTextBox.Text == "")
                searchTextBlock.Text = "Search by month/year";
        }
    }
}