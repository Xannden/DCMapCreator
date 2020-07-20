using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CompendiumMapCreator.Converter
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public Visibility IfTrue { get; set; }

		public Visibility IfFalse { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b && b)
			{
				return this.IfTrue;
			}
			else
			{
				return this.IfFalse;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
	}
}