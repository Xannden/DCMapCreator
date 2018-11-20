using System;
using System.Globalization;
using System.Windows.Data;

namespace CompendiumMapCreator.Converter
{
	public class IconTypeToFileConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((IconType)value).GetImageFile();

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}