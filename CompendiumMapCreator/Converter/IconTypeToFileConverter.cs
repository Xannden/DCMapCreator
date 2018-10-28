using System;
using System.Globalization;
using System.Windows.Data;

namespace CompendiumMapCreator
{
	public class IconTypeToFileConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((IconType)value).GetFile();

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}