using System;
using System.Globalization;
using System.Windows.Data;

namespace CompendiumMapCreator
{
	public class AddConverter : IValueConverter
	{
		public int Value { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int i)
			{
				return i + this.Value;
			}

			throw new ArgumentException("value must be a int");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int i)
			{
				return i - this.Value;
			}

			throw new ArgumentException("value must be a int");
		}
	}
}