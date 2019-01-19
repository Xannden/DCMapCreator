using System;
using System.Collections.Generic;
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
		private ImagePoint origin;
		private ImagePoint change = new ImagePoint(0, 0);
		private IList<Element> copied;

		private bool moving;

		private readonly DragHelper drag;
		private readonly EditHelper edit = new EditHelper();

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

			this.drag = new DragHelper(this.ViewModel);
		}

		public ViewModel.MainWindow ViewModel => (ViewModel.MainWindow)this.DataContext;

		private void Paste(object sender, EventArgs e)
		{
			if (this.copied != null)
			{
				ImagePoint p = Mouse.GetPosition(this.Zoom).ToImage(this.Zoom);

				this.ViewModel.Copy(p, this.copied);
			}
		}

		private void Copy(object sender, EventArgs e)
		{
			this.copied = new List<Element>(this.ViewModel.Selected);
		}

		private void Edit(object sender, EventArgs e)
		{
			this.edit.editWindow?.Close();

			if (this.ViewModel.Selected.Count != 1)
			{
				return;
			}

			this.edit.editing = this.ViewModel.Selected[0] as Data.Label;

			this.edit.editWindow = new EditWindow
			{
				Text = this.edit.editing.Text,
				ShowInTaskbar = false,
				Owner = this,
			};

			this.edit.editWindow.Closing += this.EditWindow_Closing;

			Point screen = this.PointToScreen(this.edit.contextMenuPosition);

			this.edit.editWindow.Left = screen.X;
			this.edit.editWindow.Top = screen.Y;

			this.edit.editWindow.Show();

			this.edit.editWindow.Focus();
		}

		private void EditWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.edit.editing.Text != this.edit.editWindow.Text)
			{
				this.ViewModel.AddEdit(new ChangeLabel(this.edit.editing, this.edit.editWindow.Text));
			}
		}

		protected override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			this.edit.contextMenuPosition = Mouse.GetPosition(this);

			base.OnContextMenuOpening(e);
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e) => this.edit.editWindow?.Close();

		private void ShowShortcuts(object sender, RoutedEventArgs e) => new ShortcutsWindow() { Owner = this }.Show();

		private void AboutWindow(object sender, RoutedEventArgs e) => new AboutWindow() { Owner = this }.Show();

		private void Zoom_KeyDown(object sender, KeyEventArgs e)
		{
			if (this.ViewModel.Selected == null)
			{
				return;
			}

			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				foreach (Element element in this.ViewModel.Selected)
				{
					element.Opacity = 0.25;
				}
			}

			if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
			{
				if (!this.moving && this.ViewModel.Selected.Count > 0)
				{
					this.moving = true;
					this.origin = new ImagePoint(this.ViewModel.Selected[0].X, this.ViewModel.Selected[0].Y);
					this.ViewModel.DragStart(this.origin);

					if (Keyboard.IsKeyDown(Key.Up))
					{
						this.change.Y--;
					}

					if (Keyboard.IsKeyDown(Key.Down))
					{
						this.change.Y++;
					}

					if (Keyboard.IsKeyDown(Key.Left))
					{
						this.change.X--;
					}

					if (Keyboard.IsKeyDown(Key.Right))
					{
						this.change.X++;
					}
					this.ViewModel.DragUpdate(this.origin + this.change);
				}
				else if (this.moving)
				{
					if (Keyboard.IsKeyDown(Key.Up))
					{
						this.change.Y--;
					}

					if (Keyboard.IsKeyDown(Key.Down))
					{
						this.change.Y++;
					}

					if (Keyboard.IsKeyDown(Key.Left))
					{
						this.change.X--;
					}

					if (Keyboard.IsKeyDown(Key.Right))
					{
						this.change.X++;
					}

					this.ViewModel.DragUpdate(this.origin + this.change);
				}
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
				foreach (Element element in this.ViewModel.Selected)
				{
					element.Opacity = 1;
				}
			}

			if (this.moving && Keyboard.IsKeyUp(Key.Up) && Keyboard.IsKeyUp(Key.Down) && Keyboard.IsKeyUp(Key.Left) && Keyboard.IsKeyUp(Key.Right))
			{
				this.ViewModel.DragEnd();
				this.change = new ImagePoint(0, 0);

				this.moving = false;
			}
		}

		public void Zoom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => this.drag.MouseUp(e.GetPosition(this.Zoom).ToImage(this.Zoom));

		private void Zoom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			((ZoomControl)sender).Focus();

			this.drag.MouseDown();
		}

		private void Zoom_MouseMove(object sender, MouseEventArgs e) => this.drag.MouseMove(e.GetPosition(this.Zoom).ToImage(this.Zoom), e.LeftButton);

		private void Zoom_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => this.ViewModel.Select(e.GetPosition(this.Zoom).ToImage(this.Zoom));

		private class DragHelper
		{
			private readonly ViewModel.MainWindow viewModel;
			private bool dragging;
			private bool mouseDown;

			public DragHelper(ViewModel.MainWindow viewModel)
			{
				this.viewModel = viewModel;
			}

			public void MouseDown()
			{
				this.mouseDown = true;
			}

			public void MouseMove(ImagePoint p, MouseButtonState state)
			{
				if (this.mouseDown && state == MouseButtonState.Released)
				{
					this.MouseUp(p);
				}
				else if (this.dragging)
				{
					this.viewModel.DragUpdate(p);
				}
				else if (this.mouseDown)
				{
					this.dragging = true;
					this.viewModel.DragStart(p);
				}
			}

			public void MouseUp(ImagePoint mousePosition)
			{
				if (this.dragging)
				{
					this.viewModel.DragEnd();
				}
				else if (this.mouseDown)
				{
					this.viewModel.Click(mousePosition);
				}

				this.dragging = false;
				this.mouseDown = false;
			}
		}

		private class EditHelper
		{
			public EditWindow editWindow;
			public Data.Label editing;
			public Point contextMenuPosition;
		}
	}
}