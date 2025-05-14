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
using System.Windows.Shapes;
using MyQuanLyTrangSuc.ViewModel;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System.Windows.Navigation;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for MonthlyStockReportWindow.xaml
    /// </summary>
    public partial class MonthlyStockReportWindow : Window
    {
        private MonthlyStockReportWindowLogic _logic;

        public MonthlyStockReportWindow()
        {
            InitializeComponent();
            _logic = new MonthlyStockReportWindowLogic(this);
            this.DataContext = _logic;
        }

        // Constructor với tham số để truyền tháng/năm cụ thể
        public MonthlyStockReportWindow(DateTime selectedMonthYear) : this()
        {
            _logic.SelectedMonthYear = selectedMonthYear;
        }
    }
}
