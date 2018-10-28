using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CompendiumMapCreator
{
	public static class Extensions
	{
		public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(ImageSource), typeof(Extensions));

		public static void SetSource(this RadioButton element, ImageSource source) => element.SetValue(SourceProperty, source);

		public static ImageSource GetSource(this RadioButton element) => (ImageSource)element.GetValue(SourceProperty);
	}
}