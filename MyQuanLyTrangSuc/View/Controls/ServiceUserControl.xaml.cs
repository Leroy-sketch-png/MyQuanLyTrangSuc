using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.View
{
    public partial class ServiceUserControl : UserControl
    {
        private readonly ServiceUserControlLogic logicService;

        public ServiceUserControl()
        {
            logicService = new ServiceUserControlLogic(this);
            InitializeComponent();
        }

        private void OnClick_Remove_ServiceUserControl(object sender, RoutedEventArgs e)
        {
            logicService.RemoveServiceFromDatabase();
        }
    }
}
