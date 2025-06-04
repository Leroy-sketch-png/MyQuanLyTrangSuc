using MyQuanLyTrangSuc.Security;
using System;
using System.Globalization;
using System.Security.Principal; // Required for IPrincipal
using System.Threading;           // Required for Thread.CurrentPrincipal
using System.Windows.Data;

namespace MyQuanLyTrangSuc.Converters
{
    [ValueConversion(typeof(IPrincipal), typeof(bool))] // The input 'value' will be an IPrincipal
    public class PermissionToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The 'value' coming into the converter *should* be the IPrincipal
            // from the binding: {Binding Path=CurrentUserPrincipal} or {x:Static System.Threading.Thread.CurrentPrincipal}
            if (value is CustomPrincipal currentPrincipal)
            {
                if (parameter is string permissionName)
                {
                    return currentPrincipal.HasPermission(permissionName);
                }
            }
            // If the value is null, not a CustomPrincipal, or parameter is not a string,
            // or if no permissionName is provided, return false (or default enabled/disabled state)
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}