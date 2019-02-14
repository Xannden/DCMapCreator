using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using CompendiumMapCreator.Controls;
using CompendiumMapCreator.Data;
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
		private bool justClosed;

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

		private void Paste()
		{
			if (this.copied != null)
			{
				ImagePoint p = Mouse.GetPosition(this.Zoom).ToImage(this.Zoom);

				this.ViewModel.Project.Copy(p, this.copied);
			}
		}

		private void Copy()
		{
			if (this.ViewModel.Project == null)
			{
				return;
			}
			this.copied = new List<Element>(this.ViewModel.Project.Selected);
		}

		private void Edit(object sender, RoutedEventArgs e)
		{
			if (this.ViewModel.Project.Selected.Count != 1)
			{
				return;
			}

			Element element = this.ViewModel.Project.Selected[0];

			this.ViewModel.Edit((element.Position() + new ImagePoint(element.Width / 2, element.Height / 2)).ToWindow(this.Zoom) + new WindowPoint(160, 20));

			this.EditWindow.Focus();
		}

		private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (this.ViewModel.Editing.Visibility == Visibility.Visible)
			{
				Point p = e.GetPosition(this.EditWindow);

				if (p.X < 0 || p.Y < 0 || p.X > 200 || p.Y > 50)
				{
					this.ViewModel.Editing.End();
					this.justClosed = true;
				}
			}
		}

		private void Window_MouseUp(object sender, MouseButtonEventArgs e)
		{
			this.justClosed = false;
		}

		private void ShowShortcuts(object sender, RoutedEventArgs e) => new ShortcutsWindow() { Owner = this }.Show();

		private void AboutWindow(object sender, RoutedEventArgs e) => new AboutWindow() { Owner = this }.Show();

		private void Zoom_KeyDown(object sender, KeyEventArgs e)
		{
			if (this.ViewModel.Project == null)
			{
				return;
			}

			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				foreach (Element element in this.ViewModel.Project.Selected)
				{
					element.Opacity = 0.25;
				}
			}

			if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
			{
				if (!this.moving && this.ViewModel.Project.Selected.Count > 0)
				{
					this.moving = true;
					this.origin = new ImagePoint(this.ViewModel.Project.Selected[0].X, this.ViewModel.Project.Selected[0].Y);
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
			if (this.ViewModel.Project == null)
			{
				return;
			}

			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				foreach (Element element in this.ViewModel.Project.Selected)
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

		private void Zoom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.drag.MouseUp(e.GetPosition(this.Zoom).ToImage(this.Zoom));
		}

		private void Zoom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.justClosed)
			{
				return;
			}

			((ZoomControl)sender).Focus();

			this.drag.MouseDown(e.GetPosition(this.Zoom).AsWindow());
		}

		private void Zoom_MouseMove(object sender, MouseEventArgs e) => this.drag.MouseMove(e.GetPosition(this.Zoom).ToImage(this.Zoom), e.GetPosition(this.Zoom).AsWindow(), e.LeftButton);

		private void Zoom_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => this.ViewModel?.Project?.Select(e.GetPosition(this.Zoom).ToImage(this.Zoom));

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
			=> e.Cancel = this.ViewModel.Changing(System.Windows.Window.GetWindow(this.Zoom));

		private void RotateClockwise(object sender, RoutedEventArgs e) => this.ViewModel.RotateClockwise();

		private void RotateCounterClockwise(object sender, RoutedEventArgs e) => this.ViewModel.RotateCounterClockwise();

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (this.ViewModel.Editing.Visibility != Visibility.Collapsed)
			{
				if (e.Key == Key.Escape)
				{
					this.ViewModel.Editing.End();
				}

				return;
			}

			if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
			{
				switch (e.Key)
				{
					//Load Image
					case Key.N:
						this.ViewModel.LoadImage(this.Window);
						break;
					//Export
					case Key.E:
						this.ViewModel.Export();
						break;
					//Undo
					case Key.Z:
						this.ViewModel.Undo();
						break;
					//Redo
					case Key.Y:
						this.ViewModel.Redo();
						break;
					//Save project
					case Key.S:
						this.ViewModel.SaveProject();
						break;
					//Load project
					case Key.L:
						this.ViewModel.LoadProject(this.Window);
						break;
					//Copy
					case Key.C:
						this.Copy();
						break;
					//Paste
					case Key.V:
						this.Paste();
						break;
					//Deselect
					case Key.D:
						this.ViewModel.Deselect();
						break;
				}
			}
			else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0)
			{
				switch (e.Key)
				{
					//QuestNPC
					case Key.D1:
						this.ViewModel.SetType(IconType.QuestNPC);
						break;
					//SecretDoor
					case Key.D2:
						this.ViewModel.SetType(IconType.SecretDoor);
						break;
					//QuestExit
					case Key.D3:
						this.ViewModel.SetType(IconType.QuestExit);
						break;
					//Portal
					case Key.D4:
						this.ViewModel.SetType(IconType.Portal);
						break;
					//Label
					case Key.D5:
						this.ViewModel.SetType(IconType.Label);
						break;
					//Trap
					case Key.D6:
						this.ViewModel.SetType(IconType.Trap);
						break;
					//CollapsibleFloor
					case Key.D7:
						this.ViewModel.SetType(IconType.CollapsibleFloor);
						break;
					//Entrance
					case Key.D8:
						this.ViewModel.SetType(IconType.Entrance);
						break;
				}
			}
			else
			{
				switch (e.Key)
				{
					//Cursor
					case Key.D1:
						this.ViewModel.SetType(IconType.Cursor);
						break;
					//NormalChest
					case Key.D2:
						this.ViewModel.SetType(IconType.NormalChest);
						break;
					//TrappedChest
					case Key.D3:
						this.ViewModel.SetType(IconType.TrappedChest);
						break;
					//LockedChest
					case Key.D4:
						this.ViewModel.SetType(IconType.LockedChest);
						break;
					//LockedDoor
					case Key.D5:
						this.ViewModel.SetType(IconType.LockedDoor);
						break;
					//LeverValveRune
					case Key.D6:
						this.ViewModel.SetType(IconType.LeverValveRune);
						break;
					//ControlBox
					case Key.D7:
						this.ViewModel.SetType(IconType.ControlBox);
						break;
					//Collectible
					case Key.D8:
						this.ViewModel.SetType(IconType.Collectible);
						break;
					//Lore
					case Key.D9:
						this.ViewModel.SetType(IconType.Lore);
						break;
					//Natural
					case Key.D0:
						this.ViewModel.SetType(IconType.Natural);
						break;
					//Arcane
					case Key.OemMinus:
						this.ViewModel.SetType(IconType.Arcane);
						break;
					//QuestItem
					case Key.OemPlus:
						this.ViewModel.SetType(IconType.QuestItem);
						break;
					//Delete
					case Key.Delete:
						this.ViewModel.Delete();
						break;
				}
			}
		}

		private void Paste(object sender, RoutedEventArgs e)
		{
			this.Paste();
		}

		private void Copy(object sender, RoutedEventArgs e)
		{
			this.Copy();
		}

		private void SaveProject_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.SaveProject();
		}

		private void LoadProject_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.LoadProject(this.Window);
		}

		private void Undo_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Undo();
		}

		private void Redo_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Redo();
		}

		private void LoadImage_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.LoadImage(this.Window);
		}

		private void Export_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Export();
		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Delete();
		}

		private void Zoom_ScaleChanged(object sender, EventArgs e)
		{
			this.ViewModel.Editing.End();
		}

		private void Zoom_ViewportChanged(object sender, EventArgs e)
		{
			this.ViewModel.Editing.End();
		}

		private void ChangeMap_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.ChangeImage();
		}

		private void AddTitle_Click(object sender, RoutedEventArgs e)
		{
			if (this.ViewModel.Project == null)
			{
				return;
			}

			TitleWindow window = new TitleWindow(this.ViewModel.Project.Title)
			{
				Owner = this,
			};

			bool? result = window.ShowDialog();

			if (result.GetValueOrDefault())
			{
				this.ViewModel.SetTitle(window.MapTitle);
			}
		}

		private class DragHelper
		{
			private readonly ViewModel.MainWindow viewModel;
			private bool dragging;
			private bool mouseDown;
			private WindowPoint start;

			public DragHelper(ViewModel.MainWindow viewModel)
			{
				this.viewModel = viewModel;
			}

			public void MouseDown(WindowPoint p)
			{
				this.mouseDown = true;
				this.start = p;
			}

			public void MouseMove(ImagePoint p, WindowPoint wp, MouseButtonState state)
			{
				WindowPoint diff = this.start - wp;

				if (this.mouseDown && state == MouseButtonState.Released)
				{
					this.MouseUp(p);
				}
				else if (this.dragging)
				{
					this.viewModel.DragUpdate(p);
				}
				else if (this.mouseDown && (Math.Abs(diff.X) > 3 || Math.Abs(diff.Y) > 3))
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
	}
}