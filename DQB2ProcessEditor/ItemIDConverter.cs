using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace DQB2ProcessEditor
{
	internal class ItemIDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var info = value as ItemInfo;
            if (info == null) return "";

            var sb = new StringBuilder();
            sb.Append($"{info.Name} {{{info.ID}}}");
            if (info.Rare > 0) sb.Append(" ");
            for (int i = 0; i < info.Rare; i++)
            {
                sb.Append("☆");
            }
            if (info.Link)
            {
                sb.Append(" 🔗");
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
