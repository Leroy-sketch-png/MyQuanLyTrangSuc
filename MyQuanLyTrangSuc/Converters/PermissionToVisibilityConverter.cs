using MyQuanLyTrangSuc.Security;
using System; // Required for Type
using System.Globalization;
using System.Security.Principal; // Required for IPrincipal
using System.Threading;           // Required for Thread.CurrentPrincipal
using System.Windows;
using System.Windows.Data;

namespace MyQuanLyTrangSuc.Converters
{
    [ValueConversion(typeof(IPrincipal), typeof(Visibility))]
    public class PermissionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. Ensure the value passed from the binding is a CustomPrincipal
            if (value is CustomPrincipal currentPrincipal)
            {
                // 2. Ensure the ConverterParameter is a string (your permission name)
                if (parameter is string permissionName)
                {
                    // 3. Check if the principal has the permission and return appropriate Visibility
                    return currentPrincipal.HasPermission(permissionName) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            // Default case: If value is not CustomPrincipal, or parameter is not a string,
            // or if no permissionName is provided, default to Collapsed (hidden).
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}