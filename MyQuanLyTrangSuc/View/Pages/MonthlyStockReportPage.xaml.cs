using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.View
{
    public partial class MonthlyStockReportPage : Page
    {
        private MonthlyStockReportPageLogic logicService;

        public MonthlyStockReportPage()
        {
            InitializeComponent();
            logicService = new MonthlyStockReportPageLogic(this);
            this.DataContext = logicService;
        }

        private void OnDoubleClick_InspectReport_MonthlyStockReportPageDataGrid(object sender, MouseButtonEventArgs e)
        {
            logicService.LoadReportDetailsWindow();
        }

        private void viewButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadReportDetailsWindow();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.CreateOrUpdateCurrentMonthReport();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchTextBlock.Text = "";
            if (searchComboBox.SelectionBoxItem.ToString() == "Month")
                logicService.ReportsSearchByMonth(searchTextBox.Text);
            else if (searchComboBox.SelectionBoxItem.ToString() == "Year")
                logicService.ReportsSearchByYear(searchTextBox.Text);
            if (searchTextBox.Text == "")
                searchTextBlock.Text = "Search by month/year";
        }
    }
}