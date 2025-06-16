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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for ServicesListPage.xaml
    /// </summary>
    public partial class ServiceRecordListPage : Page
    {
        private readonly ServiceRecordListPageLogic logicService;
        public ServiceRecordListPage()
        {
            InitializeComponent();
            logicService = new ServiceRecordListPageLogic(this);
            DataContext = logicService;
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchComboBox.SelectedItem != null)
            {
                string selectedCriteria = (searchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                switch (selectedCriteria)
                {
                    case "Customer":
                        logicService.SearchServiceRecordsByNameOfCustomer(searchTextBox.Text);
                        break;
                    case "ID":
                        logicService.SearchServiceRecordsByID(searchTextBox.Text);
                        break;
                    case "Date":
                        logicService.SearchServiceRecordsByDate(searchTextBox.Text);
                        break;
                }
            }
            if (string.IsNullOrEmpty(searchTextBox.Text))
            {
                searchTextBlock.Text = "Search";
            }
        }



        // Show the filter popup when filter button is clicked
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterOverlay.Visibility = Visibility.Visible;
        }

        // Hide the filter popup when clicking outside the popup area
        private void FilterOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only close if the click is on the overlay itself, not on the popup content
            if (e.Source == FilterOverlay)
            {
                FilterOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // Hide the filter popup when close button is clicked
        private void CloseFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterOverlay.Visibility = Visibility.Collapsed;
        }


        private void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse and validate input values
                var filterParams = ExtractFilterParameters();

                // Apply the filters using the existing logicService instance
                logicService.ApplyAdvancedFilters(
                    filterParams.EnableIdFilter, filterParams.IdFrom, filterParams.IdTo,
                    filterParams.EnableDateFilter, filterParams.DateFrom, filterParams.DateTo,
                    filterParams.EnableCustomerFilter, filterParams.CustomerText,
                    filterParams.EnableTotalFilter, filterParams.TotalFrom, filterParams.TotalTo,
                    filterParams.EnablePaidFilter, filterParams.PaidFrom, filterParams.PaidTo,
                    filterParams.EnableRemainingFilter, filterParams.RemainingFrom, filterParams.RemainingTo,
                    filterParams.EnableStatusFilter, filterParams.SelectedStatus);

                // Close the filter popup
                FilterOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Filter Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear all filters in ViewModel using existing logicService instance
                logicService.ClearAllFilters();

                // Reset all filter UI controls
                ResetFilterControls();

                // Close the filter popup
                FilterOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing filters: {ex.Message}", "Filter Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this helper class inside the ServiceRecordListPage class
        private class FilterParameters
        {
            public bool EnableIdFilter { get; set; }
            public string IdFrom { get; set; }
            public string IdTo { get; set; }

            public bool EnableDateFilter { get; set; }
            public DateTime? DateFrom { get; set; }
            public DateTime? DateTo { get; set; }

            public bool EnableCustomerFilter { get; set; }
            public string CustomerText { get; set; }

            public bool EnableTotalFilter { get; set; }
            public decimal? TotalFrom { get; set; }
            public decimal? TotalTo { get; set; }

            public bool EnablePaidFilter { get; set; }
            public decimal? PaidFrom { get; set; }
            public decimal? PaidTo { get; set; }

            public bool EnableRemainingFilter { get; set; }
            public decimal? RemainingFrom { get; set; }
            public decimal? RemainingTo { get; set; }

            public bool EnableStatusFilter { get; set; }
            public string SelectedStatus { get; set; }
        }

        // Helper method to extract and validate filter parameters from UI controls
        private FilterParameters ExtractFilterParameters()
        {
            var parameters = new FilterParameters();

            // ID Filter
            parameters.EnableIdFilter = EnableIdFilter.IsChecked == true;
            parameters.IdFrom = IdFromTextBox.Text?.Trim();
            parameters.IdTo = IdToTextBox.Text?.Trim();

            // Date Filter
            parameters.EnableDateFilter = EnableDateFilter.IsChecked == true;
            parameters.DateFrom = DateFromPicker.SelectedDate;
            parameters.DateTo = DateToPicker.SelectedDate;

            // Customer Filter
            parameters.EnableCustomerFilter = EnableCustomerFilter.IsChecked == true;
            parameters.CustomerText = CustomerFilterTextBox.Text?.Trim();

            // Total Amount Filter
            parameters.EnableTotalFilter = EnableTotalFilter.IsChecked == true;
            if (decimal.TryParse(TotalFromTextBox.Text?.Trim(), out decimal totalFrom))
                parameters.TotalFrom = totalFrom;
            if (decimal.TryParse(TotalToTextBox.Text?.Trim(), out decimal totalTo))
                parameters.TotalTo = totalTo;

            // Paid Amount Filter
            parameters.EnablePaidFilter = EnablePaidFilter.IsChecked == true;
            if (decimal.TryParse(PaidFromTextBox.Text?.Trim(), out decimal paidFrom))
                parameters.PaidFrom = paidFrom;
            if (decimal.TryParse(PaidToTextBox.Text?.Trim(), out decimal paidTo))
                parameters.PaidTo = paidTo;

            // Remaining Amount Filter
            parameters.EnableRemainingFilter = EnableRemainingFilter.IsChecked == true;
            if (decimal.TryParse(RemainingFromTextBox.Text?.Trim(), out decimal remainingFrom))
                parameters.RemainingFrom = remainingFrom;
            if (decimal.TryParse(RemainingToTextBox.Text?.Trim(), out decimal remainingTo))
                parameters.RemainingTo = remainingTo;

            // Status Filter
            parameters.EnableStatusFilter = EnableStatusFilter.IsChecked == true;
            parameters.SelectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            return parameters;
        }

        // Helper method to reset all filter controls to their default state
        private void ResetFilterControls()
        {
            // Reset checkboxes
            EnableIdFilter.IsChecked = false;
            EnableDateFilter.IsChecked = false;
            EnableCustomerFilter.IsChecked = false;
            EnableTotalFilter.IsChecked = false;
            EnablePaidFilter.IsChecked = false;
            EnableRemainingFilter.IsChecked = false;
            EnableStatusFilter.IsChecked = false;

            // Clear text boxes
            IdFromTextBox.Text = string.Empty;
            IdToTextBox.Text = string.Empty;
            CustomerFilterTextBox.Text = string.Empty;
            TotalFromTextBox.Text = string.Empty;
            TotalToTextBox.Text = string.Empty;
            PaidFromTextBox.Text = string.Empty;
            PaidToTextBox.Text = string.Empty;
            RemainingFromTextBox.Text = string.Empty;
            RemainingToTextBox.Text = string.Empty;

            // Reset date pickers
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;

            // Reset combo box
            StatusFilterComboBox.SelectedIndex = -1;
        }
    }
}
