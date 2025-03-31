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
    /// Interaction logic for ImportDetailsWindow.xaml
    /// </summary>
    public partial class ImportDetailsWindow : Window
    {
        //MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        public ImportDetailsWindow()
        {
            InitializeComponent();
            //Import selectedImportRecord = context.Imports.FirstOrDefault();
            //ImportDetailsWindowLogic importDetailsWindowLogic = new ImportDetailsWindowLogic(this, selectedImportRecord);
            //this.DataContext = importDetailsWindowLogic;

        }
        public ImportDetailsWindow(Import selectedImportRecord) {
            InitializeComponent();
            ImportDetailsWindowLogic importDetailsWindowLogic = new ImportDetailsWindowLogic(this, selectedImportRecord);
            this.DataContext = importDetailsWindowLogic;
        }
    }
}
