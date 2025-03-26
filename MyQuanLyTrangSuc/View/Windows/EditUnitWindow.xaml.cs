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

namespace MyQuanLyTrangSuc.View.Windows
{
    /// <summary>
    /// Interaction logic for EditUnitWindow.xaml
    /// </summary>
    public partial class EditUnitWindow : Window
    {
        private readonly EditUnitWindowLogic logicService;
        public EditUnitWindow(Unit unit)
        {
            InitializeComponent();
            logicService = new EditUnitWindowLogic(unit);
            DataContext = logicService;  
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.EditUnit();
            if (isSuccess) this.Close();
        }
    }
}
