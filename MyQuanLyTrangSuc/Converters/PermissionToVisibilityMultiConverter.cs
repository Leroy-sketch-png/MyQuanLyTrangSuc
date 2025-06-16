using MyQuanLyTrangSuc.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MyQuanLyTrangSuc.Converters
{
    public class PermissionToVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var principal = values[0] as CustomPrincipal;
            var permission = parameter as string;

            if (principal != null && !string.IsNullOrEmpty(permission) && principal.HasPermission(permission))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
