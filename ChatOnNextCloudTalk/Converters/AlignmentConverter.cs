using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatOnNextCloudTalk.Converters
{
    public class AlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HorizontalAlignment alignment = new HorizontalAlignment();
            string val = value.ToString();
            if (val == "left")
                alignment = HorizontalAlignment.Left;
            else if (val == "right")
                alignment = HorizontalAlignment.Right;

            return alignment;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
