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
    /// Interaction logic for ImportDetailsWindowUI.xaml
    /// </summary>
    public partial class ImportDetailsWindow : Window
    {
        private readonly ImportDetailsWindowLogic logicService;
        public ImportDetailsWindow()
        {
            InitializeComponent();
        }
        public ImportDetailsWindow(Import selectedImportRecord)
        {
            InitializeComponent();
            logicService = new ImportDetailsWindowLogic(this, selectedImportRecord);
            DataContext = logicService;
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.Print(this);
            this.Activate();
        }
    }
}
