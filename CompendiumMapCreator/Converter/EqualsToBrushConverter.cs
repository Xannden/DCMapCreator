using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Converter
{
	public class EqualsToBrushConverter : IMultiValueConverter
	{
		public Brush IsTrue { get; set; }

		public Brush IsFalse { get; set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => ((values[0] as IList<ElementVM>)?.Contains(values[1] as ElementVM) ?? false) ? this.IsTrue : this.IsFalse;

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
	}
}