using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.View;

namespace CompendiumMapCreator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ImagePoint offset;
		private ImagePoint origin;
		private Element element;
		private Element copied;
		private EditWindow editWindow;
		private Data.Label editing;
		private Point contextMenuPosition;
		private bool moving;

		public MainWindow()
		{
			this.InitializeComponent();

			this.ViewModel.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == "Project")
				{
					this.Zoom.Center();
				}
			};
		}

		public ViewModel.MainWindow ViewModel => (ViewModel.MainWindow)this.DataContext;

		private ItemsControl ElementDisplay
		{
			get;
			set;
		}

		private void Paste(object sender, EventArgs e)
		{
			if (this.copied != null)
			{
				ImagePoint p = Mouse.GetPosition(this.Zoom).ToImage(this.Zoom.Scale, this.Zoom.ViewportPositionX, this.Zoom.ViewportPositionY);

				this.ViewModel.Copy(p - new ImagePoint(this.copied.Width / 2, this.copied.Height / 2), this.copied);
			}
		}

		private void Copy(object sender, EventArgs e) => this.copied = this.ViewModel.Selected;

		private void Edit(object sender, EventArgs e)
		{
			this.editWindow?.Close();

			this.editing = this.ViewModel.Selected as Data.Label;

			this.editWindow = new EditWindow
			{
				Text = this.editing.Text,
				ShowInTaskbar = false,
				Owner = this,
			};

			this.editWindow.Closing += this.EditWindow_Closing;

			Point screen = this.PointToScreen(this.contextMenuPosition);

			this.editWindow.Left = screen.X;
			this.editWindow.Top = screen.Y;

			this.editWindow.Show();

			this.editWindow.Focus();
		}

		private void EditWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.editing.Text != this.editWindow.Text)
			{
				this.ViewModel.AddEdit(new ChangeLabel(this.editing, this.editWindow.Text));
			}
		}

		protected override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			this.contextMenuPosition = Mouse.GetPosition(this);

			base.OnContextMenuOpening(e);
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e) => this.editWindow?.Close();

		private void ShowShortcuts(object sender, RoutedEventArgs e) => new ShortcutsWindow() { Owner = this }.Show();

		private void AboutWindow(object sender, RoutedEventArgs e) => new AboutWindow() { Owner = this }.Show();

		private void ItemsControl_Initialized(object sender, EventArgs e)
		{
			this.ElementDisplay = sender as ItemsControl;
		}

		private void Zoom_KeyDown(object sender, KeyEventArgs e)
		{
			if (this.ViewModel.Selected == null)
			{
				return;
			}

			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				this.ViewModel.Selected.Opacity = .45;
			}

			if (this.ElementDisplay.IsMouseCaptured)
			{
				return;
			}

			if (!this.moving && (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right))
			{
				this.moving = true;
				this.origin = new ImagePoint(this.ViewModel.Selected.X, this.ViewModel.Selected.Y);
			}
		}

		private void Zoom_KeyUp(object sender, KeyEventArgs e)
		{
			if (this.ViewModel.Selected == null)
			{
				return;
			}

			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				this.ViewModel.Selected.Opacity = 1;
			}

			if (this.moving && Keyboard.IsKeyUp(Key.Up) && Keyboard.IsKeyUp(Key.Down) && Keyboard.IsKeyUp(Key.Left) && Keyboard.IsKeyUp(Key.Right))
			{
				ImagePoint newPoint = new ImagePoint(this.ViewModel.Selected.X, this.ViewModel.Selected.Y);

				this.ViewModel.Selected.X = this.origin.X;
				this.ViewModel.Selected.Y = this.origin.Y;

				int xChange = newPoint.X - this.origin.X;
				int yChange = newPoint.Y - this.origin.Y;

				this.ViewModel.AddEdit(new Move(xChange, yChange, this.ViewModel.Selected));
				this.moving = false;
			}
		}

		public void Zoom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (this.ElementDisplay.IsMouseCaptured)
			{
				this.ElementDisplay.ReleaseMouseCapture();

				int xChanged = this.element.X - this.origin.X;
				int yChanged = this.element.Y - this.origin.Y;

				if (xChanged != 0 || yChanged != 0)
				{
					this.ViewModel.AddEdit(new Move(xChanged, yChanged, this.element), apply: false);
				}

				this.element.Opacity = 1;
				this.element = null;
			}
			else if (!Keyboard.IsKeyDown(Key.Space) && this.ViewModel.SelectedType != IconType.Cursor)
			{
				Element element = this.ViewModel.CreateElement(this.ViewModel.SelectedType);

				ImagePoint position = e.GetPosition(this.Zoom).ToImage(this.Zoom.Scale, this.Zoom.ViewportPositionX, this.Zoom.ViewportPositionY) - new ImagePoint(element.Width / 2, element.Height / 2);

				element.X = position.X;
				element.Y = position.Y;

				this.ViewModel.AddElement(element);
				this.ViewModel.Selected = null;
			}
		}

		private void Zoom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			((ZoomControl)sender).Focus();

			if (!Keyboard.IsKeyDown(Key.Space) && this.ViewModel.SelectedType == IconType.Cursor && !this.moving)
			{
				Element element = this.ViewModel.Select(e.GetPosition(this.Zoom).ToImage(this.Zoom.Scale, this.Zoom.ViewportPositionX, this.Zoom.ViewportPositionY));

				if (element != null)
				{
					this.element = element;
					this.element.Opacity = .45;

					this.origin = new ImagePoint(element.X, element.Y);

					Point p = e.GetPosition(this.Zoom);

					ImagePoint ip = p.ToImage(this.Zoom.Scale, this.Zoom.ViewportPositionX, this.Zoom.ViewportPositionY);

					this.offset = ip - this.origin;

					this.ElementDisplay.CaptureMouse();
				}
			}
		}

		private void Zoom_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.ElementDisplay.IsMouseCaptured)
			{
				ImagePoint v = e.GetPosition(this.Zoom).ToImage(this.Zoom.Scale, this.Zoom.ViewportPositionX, this.Zoom.ViewportPositionY) - this.offset;

				this.element.X = v.X;
				this.element.Y = v.Y;
			}
		}

		private void Zoom_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => this.ViewModel.Select(e.GetPosition(this.Zoom).ToImage(this.Zoom.Scale, this.Zoom.ViewportPositionX, this.Zoom.ViewportPositionY));
	}
}