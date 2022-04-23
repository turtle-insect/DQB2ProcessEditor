using System;
using System.Globalization;
using System.Windows.Data;

namespace DQB2ProcessEditor
{
	internal class ItemImageConverter : IValueConverter
	{
		public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			UInt16 index = (UInt16)value;
			bool exist = Info.GetInstance().AllImage.ContainsKey(index);
			return exist ? Info.GetInstance().AllImage[index] : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
