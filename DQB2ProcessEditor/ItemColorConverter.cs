using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DQB2ProcessEditor
{
	internal class ItemColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var info = value as ItemInfo;
			if (info == null) return "";

			if (info.Name?.IndexOf("(白)") > 0) return "White";
			if (info.Name?.IndexOf("(黒)") > 0) return "Black";
			if (info.Name?.IndexOf("(紫)") > 0) return "Purple";
			if (info.Name?.IndexOf("(ピンク)") > 0) return "Pink";
			if (info.Name?.IndexOf("(赤)") > 0) return "Red";
			if (info.Name?.IndexOf("(緑)") > 0) return "Green";
			if (info.Name?.IndexOf("(黄)") > 0) return "Yellow";
			if (info.Name?.IndexOf("(青)") > 0) return "Blue";
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
