using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MyQuanLyTrangSuc.Model;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyStockReportPageLogic : INotifyPropertyChanged
    {
        private ObservableCollection<MonthlyStockReportViewModel> _monthlyStockReports;
        private MonthlyStockReportViewModel _selectedMonthlyStockReport;
        private string _originalSearchText;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<MonthlyStockReportViewModel> MonthlyStockReports
        {
            get => _monthlyStockReports;
            set
            {
                _monthlyStockReports = value;
                OnPropertyChanged();
            }
        }

        public MonthlyStockReportViewModel SelectedMonthlyStockReport
        {
            get => _selectedMonthlyStockReport;
            set
            {
                _selectedMonthlyStockReport = value;
                OnPropertyChanged();
            }
        }

        public MonthlyStockReportPageLogic()
        {
            MonthlyStockReports = new ObservableCollection<MonthlyStockReportViewModel>();
            LoadReports();
        }

        private void LoadReports()
        {
            // Load reports from database or service
            // This is just sample data
            MonthlyStockReports.Clear();

            for (int i = 1; i <= 12; i++)
            {
                MonthlyStockReports.Add(new MonthlyStockReportViewModel
                {
                    Month = i,
                    Year = DateTime.Now.Year,
                    TotalItems = new Random().Next(50, 200),
                    TotalValue = new Random().Next(5000, 20000)
                });
            }
        }

        public void GenerateMonthlyReport()
        {
            try
            {
                // Implement report generation logic here
                MessageBox.Show("Monthly report generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ImportFromExcel()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
                    Title = "Select an Excel File"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    // Implement import logic here
                    MessageBox.Show($"Successfully imported from {Path.GetFileName(filePath)}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing from Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Save Excel File"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    // Implement export logic here
                    MessageBox.Show($"Successfully exported to {Path.GetFileName(filePath)}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FilterReports(string searchText, int filterType)
        {
            if (_originalSearchText == null)
            {
                _originalSearchText = searchText;
            }

            var filtered = MonthlyStockReports.Where(r =>
                filterType == 0 ? r.Month.ToString().Contains(searchText) :
                r.Year.ToString().Contains(searchText)).ToList();

            MonthlyStockReports = new ObservableCollection<MonthlyStockReportViewModel>(filtered);
        }

        public void OnReportSelected()
        {
            if (SelectedMonthlyStockReport != null)
            {
                // Implement selection logic if needed
            }
        }

        public void ViewReportDetails(MonthlyStockReportViewModel report)
        {
            // Implement view details logic
            MessageBox.Show($"Viewing details for report: {report.Month}/{report.Year}", "Report Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void DeleteReport(MonthlyStockReportViewModel report)
        {
            if (MessageBox.Show("Are you sure you want to delete this report?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MonthlyStockReports.Remove(report);
                MessageBox.Show("Report deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class MonthlyStockReportViewModel : BaseViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalValue { get; set; }
    }
}
