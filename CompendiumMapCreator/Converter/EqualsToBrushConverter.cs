using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CompendiumMapCreator.Converter
{
	public class EqualsToBrushConverter : IMultiValueConverter
	{
		public Brush IsTrue { get; set; }

		public Brush IsFalse { get; set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => object.ReferenceEquals(values[0], values[1]) ? this.IsTrue : this.IsFalse;

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
	}
}