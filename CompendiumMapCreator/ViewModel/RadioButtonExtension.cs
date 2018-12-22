using System.Windows;
using System.Windows.Controls;

namespace CompendiumMapCreator
{
	public static class ControlType
	{
		public static readonly DependencyProperty TypeProperty = DependencyProperty.RegisterAttached("Type", typeof(IconType), typeof(ControlType));

		public static void SetType(this RadioButton element, IconType source) => element.SetValue(TypeProperty, source);

		public static IconType GetType(this RadioButton element) => (IconType)element.GetValue(TypeProperty);
	}
}