using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Converter
{
	public class ElementToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> (value is Label l && !l.IsCopy) ? Visibility.Visible : (object)Visibility.Collapsed;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}