using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyQuanLyTrangSuc.View
{
    public partial class MonthlyRevenueReportWindow : Window
    {
        public MonthlyRevenueReportWindow()
        {
            InitializeComponent();
        }

        public MonthlyRevenueReportWindow(
            List<RevenueReportProductDetail> productDetails,
            List<RevenueReportServiceDetail> serviceDetails,
            string monthYear)
        {
            InitializeComponent();
            DataContext = new MonthlyRevenueReportWindowLogic(productDetails, serviceDetails, monthYear);
        }
    }

    /*
    // 👇 Converter được đặt ở ngoài class Window (vẫn cùng file)
    public class PercentageToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return decimalValue * 100;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return decimalValue / 100;
            return 0;
        }
    }
    */
}
