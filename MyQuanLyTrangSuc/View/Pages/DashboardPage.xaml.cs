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
using static MyQuanLyTrangSuc.ViewModel.DashboardPageLogic;
using System.Windows.Threading;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page {
        private MostRecentCustomers mostRecentCustomers;
        private MostConsumedProducts mostConsumedProducts;
        private DispatcherTimer timer;

        private readonly DashboardPageLogic dashboardPageLogic;

        public DashboardPage() {
            InitializeComponent();
            dashboardPageLogic = new DashboardPageLogic(this);
            InitializeDateTimeTextBlock();
            UpdateDailyRevenueTextBox(DateTime.Today);
            UpdateDailyCountTextBlock(DateTime.Today);
            mostConsumedProducts = new MostConsumedProducts(); mostConsumedProducts.UpdateTopProductsTextBlocks(firstTextBlock, secondTextBlock, thirdTextBlock, fourthTextBlock, firstItem, secondItem, thirdItem, fourthItem);
            mostRecentCustomers = new MostRecentCustomers(); mostRecentCustomers.UpdateRecentCustomerTextBlocks(fifthTextBlock, sixthTextBlock, seventhTextBlock, eighthTextBlock);
        }

        private void InitializeDateTimeTextBlock() {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            DateTimeTextBlock.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy HH:mm:ss");
        }

        private void UpdateDailyRevenueTextBox(DateTime date) {
            dashboardPageLogic.UpdateDailyRevenueTextBox(date, DailyRevenueTextblock);
        }

        private void UpdateDailyCountTextBlock(DateTime date) {
            dashboardPageLogic.UpdateDailyCountTextBox(date, DailyCountTextblock);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            // Update the revenue when the page loads, if necessary
            UpdateDailyRevenueTextBox(DateTime.Today);
        }
    }
}
