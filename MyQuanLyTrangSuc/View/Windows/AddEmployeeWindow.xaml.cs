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
    /// Interaction logic for AddEmployeeWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window
    {
        private AddEmployeeWindowLogic logicService;
        public AddEmployeeWindow()
        {
            InitializeComponent();
            logicService = new AddEmployeeWindowLogic(this);
            DataContext = logicService;
        }

        private void ChooseImageButton_Click(object sender, RoutedEventArgs e) {
            logicService.ChooseImageFileDialog();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            logicService.AddEmployeeToDatabase();
        }
    }
}
