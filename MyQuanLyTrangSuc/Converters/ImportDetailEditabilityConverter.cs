using System;
using System.Globalization;
using System.Windows.Data;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel; 

namespace MyQuanLyTrangSuc.Converters 
{
    public class ImportDetailEditabilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (values.Length < 2 || !(values[0] is ImportDetail importDetail) || !(values[1] is EditImportWindowLogic viewModel))
            {
                return true;
            }

            bool canEdit = viewModel.CanEditImportDetail(importDetail);

            return !canEdit;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Converting back is not supported.");
        }
    }
}
