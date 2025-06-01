using System.Configuration;
using System.Data;
using System.Windows;
using OfficeOpenXml; // Make sure this using directive is present

namespace MyQuanLyTrangSuc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ExcelPackage.License.SetNonCommercialPersonal("MyQuanLyTrangSuc");
        }
    }
}