using System.Windows;
using System.Windows.Input;

namespace CompendiumMapCreator.View
{
	/// <summary>
	/// Interaction logic for EditWindow.xaml
	/// </summary>
	public partial class EditWindow : Window
	{
		public EditWindow()
		{
			this.InitializeComponent();
			this.DataContext = this;

			this.Loaded += this.EditWindow_Loaded;
		}

		public string Text
		{
			get => (string)this.GetValue(TextProperty);
			set => this.SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditWindow));

		private void EditWindow_Loaded(object sender, RoutedEventArgs e) => this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				this.Close();
			}
		}
	}
}