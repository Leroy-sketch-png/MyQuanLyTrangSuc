using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MyQuanLyTrangSuc.Converters
{
    public class BorderClipConverter : IMultiValueConverter
    {
        public static readonly BorderClipConverter Instance = new BorderClipConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = ActualWidth of the border
            // values[1] = ActualHeight of the border
            // values[2] = CornerRadius of the border
            // values[3] = BorderThickness of the border (optional, but good to include)

            if (values.Length < 3 || !(values[0] is double width) || !(values[1] is double height) || !(values[2] is CornerRadius cornerRadius))
            {
                return Geometry.Empty; // Return empty geometry if inputs are invalid
            }

            // Get border thickness if provided, otherwise default to 0
            Thickness borderThickness = new Thickness(0);
            if (values.Length > 3 && values[3] is Thickness thickness)
            {
                borderThickness = thickness;
            }

            // Calculate the inner rectangle for clipping, accounting for border thickness
            Rect rect = new Rect(
                borderThickness.Left,
                borderThickness.Top,
                Math.Max(0, width - borderThickness.Left - borderThickness.Right),
                Math.Max(0, height - borderThickness.Top - borderThickness.Bottom)
            );

            if (rect.IsEmpty)
            {
                return Geometry.Empty;
            }

            // Create a rounded rectangle geometry
            return new RectangleGeometry(rect, cornerRadius.TopLeft, cornerRadius.TopLeft);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}