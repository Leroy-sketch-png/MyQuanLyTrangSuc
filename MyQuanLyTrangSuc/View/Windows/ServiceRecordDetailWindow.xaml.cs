using System.Windows;
using System.Windows.Controls;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;

namespace MyQuanLyTrangSuc.View
{
    public partial class ServiceRecordDetailWindow : Window
    {
        private ServiceRecordDetailLogic viewModel;

        public ServiceRecordDetailWindow(ServiceRecord record)
        {
            InitializeComponent();
            viewModel = new ServiceRecordDetailLogic(record);
            DataContext = viewModel;
        }

        private void ViewService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string serviceId)
            {
                MessageBox.Show($"View details for service ID: {serviceId}");
            }
        }

        private void payButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ServiceDetail serviceDetail)
            {
                viewModel.UpdateServiceDetailStatusAsync(serviceDetail);
                btn.Visibility = Visibility.Hidden;
            }
        }
    }
}
