using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.Format;
using Microsoft.Win32;
using MBrush = System.Windows.Media.Brush;
using MColor = System.Windows.Media.Color;

namespace CompendiumMapCreator.ViewModel
{
	public class MainWindow : INotifyPropertyChanged
	{
		public Project Project
		{
			get => this._project;
			set
			{
				this._project = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Project)));
			}
		}

		private Project _project;
		private IconType selectedType;
		private IDrag dragging;
		private RelayCommand undoCommand;
		private RelayCommand redoCommand;
		private RelayCommand deleteCommand;

		public Rectangle Selection => this.dragging?.Selection ?? new Rectangle(0, 0, 0, 0);

		public MBrush SelectionFill
		{
			get
			{
				MColor color = this.dragging?.Color ?? Colors.Transparent;

				color.A = 60;

				return new SolidColorBrush(color);
			}
		}

		public MBrush SelectionStroke
		{
			get
			{
				MColor color = this.dragging?.Color ?? Colors.Transparent;

				color.A = 255;

				return new SolidColorBrush(color);
			}
		}

		public bool AddLegend { get; set; } = true;

		public string Title => $"DDO Compendium Map Creator{(string.IsNullOrEmpty(this.Project?.File) ? "" : $" - {this.Project?.File}")}";

		public event PropertyChangedEventHandler PropertyChanged;

		public IconType[] Types => Enum.GetValues(typeof(IconType)) as IconType[];

		public IconType SelectedType
		{
			get => this.selectedType;
			set
			{
				this.selectedType = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectedType)));
			}
		}

		public DelegateCommand<IconType> SetType => new DelegateCommand<IconType>((value) =>
		{
			this.SelectedType = value;
			this.Project.Selected.Clear();
		});

		public RelayCommand LoadImageCommand => new RelayCommand(this.LoadImage);

		public RelayCommand SaveProjectCommand => new RelayCommand((_) => this.Project.Save());

		public RelayCommand LoadProjectCommand => new RelayCommand((_) =>
		{
			Project result = Project.Load();
			if (result != null)
			{
				this.Project = result;
				this.Project.Edits.Clear();
				this.SelectedType = IconType.Cursor;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
			}
		});

		public RelayCommand ExportCommand => new RelayCommand((_) => this.Project.Export(this.AddLegend));

#pragma warning disable RCS1171 // Simplify lazy initialization.
		public RelayCommand UndoCommand
		{
			get
			{
				if (this.undoCommand == null)
				{
					this.undoCommand = new RelayCommand(_ =>
					{
						this.Project.Undo();
						this.undoCommand?.RaiseCanExecuteChanged();
						this.redoCommand?.RaiseCanExecuteChanged();
					}
					, _ => this.Project?.Edits.Count > 0);
				}

				return this.undoCommand;
			}
		}

		public RelayCommand RedoCommand
		{
			get
			{
				if (this.redoCommand == null)
				{
					this.redoCommand = new RelayCommand(_ =>
					{
						this.Project.Redo();
						this.undoCommand?.RaiseCanExecuteChanged();
						this.redoCommand?.RaiseCanExecuteChanged();
					}, _ => this.Project?.Edits.Count < this.Project?.Edits.Total);
				}

				return this.redoCommand;
			}
		}

		public RelayCommand DeleteCommand
		{
			get
			{
				if (this.deleteCommand == null)
				{
					this.deleteCommand = new RelayCommand(_ =>
					{
						if (this.Project.Selected.Count == 0)
						{
							return;
						}

						this.Project.AddEdit(new Remove(new List<Element>(this.Project.Selected)));
						this.undoCommand?.RaiseCanExecuteChanged();
						this.redoCommand?.RaiseCanExecuteChanged();
						this.Project.Selected.Clear();
					}, _ => this.Project?.Selected != null);
				}

				return this.deleteCommand;
			}
		}

		public RelayCommand DeselectCommand => new RelayCommand((_) => this.Project?.Selected.Clear());
#pragma warning restore RCS1171 // Simplify lazy initialization.

		public void LoadImage(object parameter)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				DefaultExt = ".png",
				Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
			};

			bool? result = dialog.ShowDialog();

			if (result.HasValue && result == true)
			{
				try
				{
					this.Project = Project.FromImage(new Image(dialog.FileName));
					this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));

					this.SelectedType = IconType.Cursor;
				}
				catch (Exception)
				{
					MessageBox.Show("Unable to load image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		public void AddElement(Element element)
		{
			if (this.Project?.Image == null)
			{
				return;
			}

			this.Project.AddEdit(new Add(element));
			this.undoCommand?.RaiseCanExecuteChanged();
			this.redoCommand?.RaiseCanExecuteChanged();
		}

		public Element CreateElement(IconType type)
		{
			switch (type)
			{
				case IconType.Label:
					return new Label("", this.Project.Elements.Count((e) => e is Label l && !l.IsCopy));

				case IconType.Portal:
					return new Portal(this.Project.Elements.Count((e) => e is Portal p && !p.IsCopy));

				default:
					return new Element(type);
			}
		}

		public void DragStart(ImagePoint p)
		{
			if (this.Project == null)
			{
				return;
			}

			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				return;
			}

			if (!Keyboard.IsKeyDown(Key.Space))
			{
				if (this.SelectedType == IconType.Cursor)
				{
					if (!this.Project.Selected.Any(e => e.Contains(p)))
					{
						this.Project.Select(p);
					}

#pragma warning disable IDE0045 // Convert to conditional expression
					if (this.Project.Selected.Count != 0)
					{
						this.dragging = new DragMove(new List<Element>(this.Project.Selected), p);
					}
					else
					{
						this.dragging = new DragSelect(p);
					}

#pragma warning restore IDE0045 // Convert to conditional expression
				}
				else if (this.SelectedType == IconType.Trap)
				{
					this.dragging = new DragTrap(p);
				}
				else if (this.SelectedType == IconType.CollapsibleFloor)
				{
					this.dragging = new DragCollapsibleFloor(p);
				}

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectionStroke)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectionFill)));
			}
		}

		public void DragUpdate(ImagePoint p)
		{
			this.dragging?.Update(p.X, p.Y, this.Project);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Selection)));
		}

		public void DragEnd()
		{
			(bool apply, Edit element) = this.dragging?.End() ?? (false, null);

			if (element != null)
			{
				this.Project.AddEdit(element, apply);
			}

			this.dragging = null;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Selection)));
		}

		public void Click(ImagePoint p)
		{
			if (this.Project == null || this.SelectedType == IconType.Trap || this.SelectedType == IconType.CollapsibleFloor)
			{
				return;
			}

			if (this.SelectedType != IconType.Cursor)
			{
				Element element = this.CreateElement(this.SelectedType);

				ImagePoint position = p - new ImagePoint(element.Width / 2, element.Height / 2);

				element.X = position.X;
				element.Y = position.Y;

				this.AddElement(element);
				this.Project.Selected.Clear();
			}
			else
			{
				this.Project.Select(p, !Keyboard.IsKeyDown(Key.LeftCtrl));
			}
		}

		private interface IDrag
		{
			void Update(int x, int y, Project project);

			(bool apply, Edit) End();

			MColor Color { get; }

			Rectangle Selection { get; }
		}

		private class DragMove : IDrag
		{
			private (int X, int Y) Change { get; set; }

			private IList<Element> Elements { get; }

			private ImagePoint Start { get; }

			private ImagePoint[] Offsets { get; }

			public DragMove(IList<Element> elements, ImagePoint start)
			{
				this.Elements = elements;
				this.Start = start;
				this.Offsets = this.Elements.Select(e => start - new ImagePoint(e.X, e.Y)).ToArray();

				for (int i = 0; i < this.Elements.Count; i++)
				{
					this.Elements[i].Opacity = 0.25;
				}
			}

			public void Update(int x, int y, Project project)
			{
				for (int i = 0; i < this.Elements.Count; i++)
				{
					this.Elements[i].X = x - this.Offsets[i].X;
					this.Elements[i].Y = y - this.Offsets[i].Y;
				}

				this.Change = (x - this.Start.X, y - this.Start.Y);
			}

			public (bool apply, Edit) End()
			{
				Edit result = null;

				(int xChanged, int yChanged) = this.Change;

				if (xChanged != 0 || yChanged != 0)
				{
					result = new Move(xChanged, yChanged, this.Elements);
				}

				for (int i = 0; i < this.Elements.Count; i++)
				{
					this.Elements[i].Opacity = 1;
				}

				return (false, result);
			}

			public MColor Color => MColor.FromRgb(0, 0, 0);

			public Rectangle Selection => Rectangle.Empty;
		}

		private class DragSelect : IDrag
		{
			private ImagePoint start;

			public Rectangle Selection { get; private set; }

			public MColor Color => MColor.FromRgb(0, 120, 215);

			public DragSelect(ImagePoint start)
			{
				this.start = start;
			}

			public void Update(int x, int y, Project project)
			{
				this.Selection = Rectangle.FromLTRB(Math.Min(this.start.X, x), Math.Min(this.start.Y, y), Math.Max(this.start.X, x) + 1, Math.Max(this.start.Y, y) + 1);

				for (int i = 0; i < project.Selected.Count; i++)
				{
					if (!this.Selection.IntersectsWith(new Rectangle(project.Selected[i].X, project.Selected[i].Y, project.Selected[i].Width, project.Selected[i].Height)))
					{
						project.Selected.Remove(project.Selected[i]);
						i--;
					}
				}

				for (int i = 0; i < project.Elements.Count; i++)
				{
					if (project.Selected.Contains(project.Elements[i]))
					{
						continue;
					}

					if (this.Selection.IntersectsWith(new Rectangle(project.Elements[i].X, project.Elements[i].Y, project.Elements[i].Width, project.Elements[i].Height)))
					{
						project.Selected.Add(project.Elements[i]);
					}
				}
			}

			public (bool apply, Edit) End() => (false, null);
		}

		private class DragTrap : IDrag
		{
			private ImagePoint start;

			public Rectangle Selection { get; private set; }

			public MColor Color => Trap.DrawingColor.ToMediaColor();

			public DragTrap(ImagePoint start)
			{
				this.start = start;
			}

			public void Update(int x, int y, Project project) => this.Selection = Rectangle.FromLTRB(Math.Min(this.start.X, x), Math.Min(this.start.Y, y), Math.Max(this.start.X, x) + 1, Math.Max(this.start.Y, y) + 1);

			public (bool apply, Edit) End() => (true, new Add(new Trap(this.Selection.Width, this.Selection.Height) { X = this.Selection.Left, Y = this.Selection.Top }));
		}

		private class DragCollapsibleFloor : IDrag
		{
			private ImagePoint start;

			public Rectangle Selection { get; private set; }

			public MColor Color => CollapsibleFloor.DrawingColor.ToMediaColor();

			public DragCollapsibleFloor(ImagePoint start)
			{
				this.start = start;
			}

			public void Update(int x, int y, Project project) => this.Selection = Rectangle.FromLTRB(Math.Min(this.start.X, x), Math.Min(this.start.Y, y), Math.Max(this.start.X, x) + 1, Math.Max(this.start.Y, y) + 1);

			public (bool apply, Edit) End() => (true, new Add(new CollapsibleFloor(this.Selection.Width, this.Selection.Height) { X = this.Selection.Left, Y = this.Selection.Top }));
		}
	}

	public static class Extensions
	{
		public static MColor ToMediaColor(this System.Drawing.Color c) => MColor.FromArgb(c.A, c.R, c.G, c.B);
	}
}