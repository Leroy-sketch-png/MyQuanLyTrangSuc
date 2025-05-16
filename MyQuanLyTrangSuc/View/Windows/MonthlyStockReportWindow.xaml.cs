using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MyQuanLyTrangSuc.Model;
namespace MyQuanLyTrangSuc.View
{
    public partial class MonthlyStockReportWindow : Window
    {
        public MonthlyStockReportWindow(List<StockReport> details, string title)
        {
            InitializeComponent();
            this.DataContext = new MonthlyStockReportWindowLogic(details, title);
        }
    }
}