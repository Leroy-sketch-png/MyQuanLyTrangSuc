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
            logicReport = new MonthlyStockReportPageLogic(this);
            this.DataContext = logicReport;
        }

        private void OnDoubleClick_InspectReport_MonthlyStockReportPageDataGrid(object sender, MouseButtonEventArgs e)
        {
            if (StockReportDataGrid.SelectedItem is StockReport selectedReport)
            {
                Console.WriteLine($"Đã chọn báo cáo: Tháng {selectedReport.MonthYear.Value.Month}/{selectedReport.MonthYear.Value.Year}");

                // Kiểm tra xem logicReport có tồn tại không
                if (logicReport == null)
                {
                    Console.WriteLine("LỖI: logicReport chưa được khởi tạo!");
                    return;
                }

                logicReport.SelectedStockReport = selectedReport; // Cập nhật SelectedStockReport
                Console.WriteLine($"logicReport.SelectedStockReport có tồn tại không? {logicReport.SelectedStockReport != null}");

                logicReport.LoadReportDetailsWindow();
            }
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