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
    /// Interaction logic for EditItemCategoryWindow.xaml
    /// </summary>
    public partial class EditItemCategoryWindow : Window
    {
        private readonly EditItemCategoryWindowLogic logicService;
        public EditItemCategoryWindow(ProductCategory productCategory)
        {
            InitializeComponent();
            logicService = new EditItemCategoryWindowLogic(productCategory);
            DataContext = logicService;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.EditItemCategory();
            if (isSuccess) this.Close();
        }
    }
}
