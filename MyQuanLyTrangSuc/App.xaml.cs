using OfficeOpenXml;

using System.Windows;


namespace MyQuanLyTrangSuc {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App()
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyQuanLyTrangSuc");
        }
    }

}
