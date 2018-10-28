using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CompendiumMapCreator
{
	public class BoolToBrushConverter : IValueConverter
	{
		public Brush IsTrue { get; set; }

		public Brush IsFalse { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((bool)value) ? this.IsTrue : this.IsFalse;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}