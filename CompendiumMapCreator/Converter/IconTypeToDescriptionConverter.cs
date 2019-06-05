using System;
using System.Globalization;
using System.Windows.Data;

namespace CompendiumMapCreator.Converter
{
	public class IconTypeToDescriptionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((IconType)value).GetName();

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}