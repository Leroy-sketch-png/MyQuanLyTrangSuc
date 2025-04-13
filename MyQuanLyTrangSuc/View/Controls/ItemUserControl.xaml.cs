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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for ItemUserControl.xaml
    /// </summary>
    public partial class ItemUserControl : UserControl
    {
        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        private readonly ItemUserControlLogic logicService;

        public ItemUserControl()
        {
            logicService = new ItemUserControlLogic(this);
            InitializeComponent();
        }

        private void OnClick_Remove_ItemUserControl(object sender, RoutedEventArgs e)
        {
            logicService.RemoveProductFromDatabase();
        }
        /*
        private void OnMouseDoubleClick_UserControl(object sender, MouseButtonEventArgs e)
        {
            logicService.LoadItemPropertiesPage();
        }
        */
        private void OnClick_ExportExcelFile_ItemUserControl(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to export by Import Date or Invoice Date?", "Chose Import Date", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                logicService.ExportExcelFile("Ngày Nhập");
            else if (result == MessageBoxResult.No)
                logicService.ExportExcelFile("Ngày Xuất Hóa Đơn");
        }
    }
}
