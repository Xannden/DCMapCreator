using System;
using System.Globalization;
using System.Windows.Data;

namespace CompendiumMapCreator.Converter
{
	public class AddConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int i)
			{
				return i + int.Parse((string)parameter);
			}

			throw new ArgumentException("value must be a int");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int i)
			{
				return i - int.Parse((string)parameter);
			}

			throw new ArgumentException("value must be a int");
		}
	}
}