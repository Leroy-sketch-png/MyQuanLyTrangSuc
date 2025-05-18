using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyQuanLyTrangSuc.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for MonthlyRevenueReportPage.xaml
    /// </summary>
    public partial class MonthlyRevenueReportPage : Page
    {
        private MonthlyRevenueReportPageLogic logicReport;

        public MonthlyRevenueReportPage()
        {
            InitializeComponent();
            logicReport = new MonthlyRevenueReportPageLogic(this);
            this.DataContext = logicReport;
        }

        private void OnDoubleClick_InspectReport_MonthlyRevenueReportPageDataGrid(object sender, MouseButtonEventArgs e)
        {
            if (RevenueReportDataGrid.SelectedItem is RevenueReport selectedReport)
            {
                Console.WriteLine($"Đã chọn báo cáo: Tháng {selectedReport.MonthYear.Month}/{selectedReport.MonthYear.Year}");

                // Kiểm tra xem logicReport có tồn tại không
                if (logicReport == null)
                {
                    Console.WriteLine("LỖI: logicReport chưa được khởi tạo!");
                    return;
                }

                logicReport.SelectedRevenueReport = selectedReport; // Cập nhật SelectedRevenueReport
                Console.WriteLine($"logicReport.SelectedRevenueReport có tồn tại không? {logicReport.SelectedRevenueReport != null}");

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
            logicReport.DeleteRevenueReport();
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
