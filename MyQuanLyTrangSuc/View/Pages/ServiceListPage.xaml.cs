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
    public partial class ServiceListPage : Page
    {
        private readonly ServiceListPageLogic _logicService;
        public ServiceListPage()
        {
            InitializeComponent();
            _logicService = new ServiceListPageLogic();
            DataContext = _logicService;
        }

        private void TextChanged_Search(object sender, RoutedEventArgs e)
        {
            _logicService.SearchServicesByName(SearchTextBox.Text);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _logicService.LoadServices();
        }

    }
}
