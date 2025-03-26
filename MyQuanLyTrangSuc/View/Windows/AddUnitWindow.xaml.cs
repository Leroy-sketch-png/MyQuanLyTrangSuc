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
    /// Interaction logic for AddUnitWindow.xaml
    /// </summary>
    public partial class AddUnitWindow : Window
    {
        private AddUnitWindowLogic logicService;    
        public AddUnitWindow()
        {
            InitializeComponent();
            logicService = new AddUnitWindowLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.AddUnit(unitNameTextBox.Text);
            this.Close();
        }
    }
}
