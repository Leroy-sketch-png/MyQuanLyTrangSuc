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

        private void InspectButton_Click(object sender, RoutedEventArgs e)
        {
            if (StockReportDataGrid.SelectedItem is StockReport selectedReport)
            {
                logicReport.CreateOrUpdateCurrentMonthReport(true);
                logicReport.LoadReportDetailsWindow(selectedReport);
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicReport.DeleteStockReport();
        }
        private bool isSelectingMonthYear = false;

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSelectingMonthYear)
            {
                // First click: reveal controls, change button text
                monthComboBox.Visibility = Visibility.Visible;
                yearTextBox.Visibility = Visibility.Visible;
                addButton.Content = "Confirm Add (Selected Month)";
                isSelectingMonthYear = true;
            }
            else
            {
                // Second click: validate and create report
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;

                if (!string.IsNullOrWhiteSpace(yearTextBox.Text) && int.TryParse(yearTextBox.Text, out int inputYear))
                {
                    year = inputYear;
                }

                if (monthComboBox.SelectedItem is ComboBoxItem selectedMonthItem)
                {
                    month = int.Parse(selectedMonthItem.Content.ToString());
                }

                var now = DateTime.Now;

                if (year > now.Year)
                {
                    MessageBox.Show("Year cannot be in the future.", "Invalid Year", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (year == now.Year && month > now.Month)
                {
                    MessageBox.Show("Month cannot be in the future.", "Invalid Month", MessageBoxButton.OK, MessageBoxImage.Warning);
                    month = now.Month;
                }

                var reportDate = new DateTime(year, month, 1);
                logicReport.CreateOrUpdateCurrentMonthReport(reportDate);

                ResetAddButtonUI();
            }
        }
        private void ResetAddButtonUI()
        {
            monthComboBox.Visibility = Visibility.Collapsed;
            yearTextBox.Visibility = Visibility.Collapsed;
            addButton.Content = "Add Stock Report";
            isSelectingMonthYear = false;
        }
        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isSelectingMonthYear)
            {
                ResetAddButtonUI();
            }
        }
        private void YearTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchComboBox.SelectionBoxItem.ToString() == "Month")
                logicReport.ReportsSearchByMonth(searchTextBox.Text);
            else if (searchComboBox.SelectionBoxItem.ToString() == "Year")
                logicReport.ReportsSearchByYear(searchTextBox.Text);
        }
    }
}