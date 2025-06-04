using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;

namespace MyQuanLyTrangSuc.View
{
    public partial class MonthlyRevenueReportPage : Page
    {
        private MonthlyRevenueReportPageLogic logic;

        public MonthlyRevenueReportPage()
        {
            InitializeComponent();
            logic = new MonthlyRevenueReportPageLogic(this);
            this.DataContext = logic;
        }

        private void CreateOrUpdateReportButton_Click(object sender, RoutedEventArgs e)
        {
            logic.CreateOrUpdateCurrentMonthReport();
        }

        // Search by month/year when selecting in ComboBox
        private void SearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            logic.SearchComboBox_SelectionChanged();
        }

        // Quick search when typing Enter in TextBox
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                logic.SearchComboBox_SelectionChanged();
            }
        }

        // Open the detail window when double clicking the line or pressing the “View” button
        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            logic.LoadReportDetailsWindow();
        }

        // (Optional) Delete the report
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var report = RevenueReportDataGrid.SelectedItem as Model.RevenueReport;
            if (report != null)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xoá báo cáo {report.RevenueReportId}?", "Xác nhận xoá", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    report.IsDeleted = true;
                    MyQuanLyTrangSucContext.Instance.SaveChanges();
                    logic.LoadReportsFromDatabase();
                    MessageBox.Show("Đã xoá báo cáo.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
