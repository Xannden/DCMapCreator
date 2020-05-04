using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Converter
{
	public class ElementToVisibilityConverter : IValueConverter
	{
		public Type ElementType { get; set; }

		public bool AllowCopies { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ElementVM e && (this.AllowCopies || !e.IsCopy) && value.GetType() == this.ElementType)
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