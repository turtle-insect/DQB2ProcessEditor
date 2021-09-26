using System;
using System.Globalization;
using System.Windows.Data;

namespace DQB2ProcessEditor
{
    class CarryTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int)(ProcessMemory.CarryType)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (ProcessMemory.CarryType)(int)value;
		}
	}
}
