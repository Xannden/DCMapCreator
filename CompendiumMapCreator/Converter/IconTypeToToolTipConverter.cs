using System;
using System.Globalization;
using System.Windows.Data;

namespace CompendiumMapCreator.Converter
{
	public class IconTypeToToolTipConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((IconType)value).GetToolTip();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}