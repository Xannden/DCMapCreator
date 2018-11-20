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
		{
			if (value is Label)
			{
				return Visibility.Visible;
			}
			else
			{
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}