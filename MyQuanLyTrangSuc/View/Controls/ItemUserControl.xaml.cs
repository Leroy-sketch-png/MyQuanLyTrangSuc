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
    /// Interaction logic for ItemUserControlUI.xaml
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
            logicService.RemoveItemFromDatabase();
        }

        private void OnClick_ExportExcelFile_ItemUserControl(object sender, RoutedEventArgs e)
        {
            logicService.ExportExcelFile("Ngày Nhập");
        }

        private void OnClick_LoadProperties_ItemUserControl(object sender, RoutedEventArgs e)
        {
            logicService.LoadItemPropertiesPage();

        }
    }
}
