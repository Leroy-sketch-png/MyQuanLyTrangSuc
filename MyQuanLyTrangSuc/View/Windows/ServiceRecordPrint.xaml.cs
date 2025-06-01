using MyQuanLyTrangSuc.Model;
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
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for ServiceRecordPrint.xaml
    /// </summary>
    public partial class ServiceRecordPrint : Window
    {
        private ServiceRecordDetailLogic _service;

        public ServiceRecordPrint(ServiceRecord record)
        {
            InitializeComponent();
            _service = new ServiceRecordDetailLogic(record);
            DataContext = _service;
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            _service.PrintServiceRecord();
        }
    }
}
