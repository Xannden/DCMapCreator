using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
		private Project project;
		private IconType selectedType;
		private IDrag dragging;
		private string projectDir;
		private string imageDir;

		public Project Project
		{
			get => this.project;
			set
			{
				this.project = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Project)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));

				this.project.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == "File" || e.PropertyName == "Title")
					{
						this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
					}
				};
			}
		}

		public Rectangle Selection => this.dragging?.Selection ?? new Rectangle(0, 0, 0, 0);

		public Editing Editing { get; } = new Editing();

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

		public string Title
		{
			get
			{
				StringBuilder builder = new StringBuilder("DDO Compendium Map Creator");

				if (!string.IsNullOrEmpty(this.Project?.Title))
				{
					builder.Append(" - ");
					builder.Append(this.Project.Title);
				}

				if (!string.IsNullOrEmpty(this.Project?.File))
				{
					builder.Append(" - ");
					builder.Append(this.Project.File);

					if (this.Project?.HasUnsaved() ?? false)
					{
						builder.Append("*");
					}
				}

				return builder.ToString();
			}
		}

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

		public MainWindow()
		{
			this.Editing.Closing += (text, label) =>
			{
				this.Project?.AddEdit(new ChangeLabel(label, text));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
			};
		}

		public void SaveProject(bool forcePrompt = false)
		{
			this.Project?.Save(ref this.projectDir, forcePrompt);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
		}

		public void LoadProject(Window window)
		{
			if (this.Changing(window))
			{
				return;
			}

			Project result = Project.Load(ref this.projectDir);
			if (result != null)
			{
				this.Project = result;
				this.Project.Edits.Clear();
				this.SelectedType = IconType.Cursor;
			}
		}

		public void Export()
		{
			this.Project?.Export(this.AddLegend, ref this.imageDir);
		}

		public void Undo()
		{
			this.Project?.Undo();
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
		}

		public void Redo()
		{
			this.Project?.Redo();
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
		}

		public void Delete()
		{
			if (this.Project?.Selected.Count == 0)
			{
				return;
			}

			this.Project?.AddEdit(new Remove(new List<Element>(this.Project.Selected)));
			this.Project?.Selected.Clear();
		}

		public void Deselect()
		{
			this.Project?.Selected.Clear();
		}

		public void SetType(IconType type)
		{
			this.SelectedType = type;
			this.Project?.Selected.Clear();
		}

		public void LoadImage(Window window)
		{
			if (this.Changing(window))
			{
				return;
			}

			OpenFileDialog dialog = new OpenFileDialog
			{
				DefaultExt = ".png",
				Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
				InitialDirectory = this.imageDir,
			};

			bool? result = dialog.ShowDialog();

			if (result.GetValueOrDefault())
			{
				this.imageDir = Path.GetDirectoryName(dialog.FileName);

				try
				{
					this.Project = Project.FromImage(new Image(dialog.FileName));

					this.SelectedType = IconType.Cursor;
				}
				catch (Exception)
				{
					MessageBox.Show("Unable to load image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		public void ChangeImage()
		{
			if (this.Project == null)
			{
				return;
			}

			OpenFileDialog dialog = new OpenFileDialog
			{
				DefaultExt = ".png",
				Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
				InitialDirectory = this.imageDir,
			};

			bool? result = dialog.ShowDialog();

			if (result.GetValueOrDefault())
			{
				this.imageDir = Path.GetDirectoryName(dialog.FileName);

				try
				{
					this.Project.AddEdit(new ChangeMap(this.Project, new Image(dialog.FileName)));

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
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
		}

		public Element CreateElement(IconType type)
		{
			switch (type)
			{
				case IconType.Label:
					return new Label("", this.Project.Elements.Count((e) => e is Label l && !l.IsCopy));

				case IconType.Portal:
					return new Portal(this.Project.Elements.Count((e) => e is Portal p && !p.IsCopy));

				case IconType.Entrance:
					return new Entrance(Rotation._0);

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
				if (this.SelectedType == IconType.Trap)
				{
					this.dragging = new DragTrap(p);
				}
				else if (this.SelectedType == IconType.CollapsibleFloor)
				{
					this.dragging = new DragCollapsibleFloor(p);
				}
				else
				{
					if (!this.Project.Selected.Any(e => e.Contains(p)))
					{
						this.Project.Select(p);
					}

					if (this.Project.Selected.Count != 0)
					{
						this.dragging = new DragMove(new List<Element>(this.Project.Selected), p);
					}
					else if (this.SelectedType == IconType.Cursor)
					{
						this.dragging = new DragSelect(p);
					}
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
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
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

		public bool Changing(Window owner)
		{
			if (this.Project?.HasUnsaved() ?? false)
			{
				MessageBoxResult result = MessageBox.Show(owner, "Do you want to save changes?", "DDO Compendium Map Creator", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);

				if (result == MessageBoxResult.Yes)
				{
					this.SaveProject();
				}
				else if (result == MessageBoxResult.Cancel)
				{
					return true;
				}
			}

			return false;
		}

		public void RotateClockwise()
		{
			if (this.Project.Selected.Count != 1 || this.Project.Selected[0].Type != IconType.Entrance)
			{
				return;
			}

			this.Project.AddEdit(new Rotate((Entrance)this.Project.Selected[0], clockwise: true));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
		}

		public void RotateCounterClockwise()
		{
			if (this.Project.Selected.Count != 1 || this.Project.Selected[0].Type != IconType.Entrance)
			{
				return;
			}

			this.Project.AddEdit(new Rotate((Entrance)this.Project.Selected[0], clockwise: false));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
		}

		public void Edit(WindowPoint p)
		{
			if (this.Project?.Selected.Count != 1 || !(this.Project.Selected[0] is Label))
			{
				return;
			}

			this.Editing.Start(p, (Label)this.Project.Selected[0]);
		}

		public void SetTitle(string title)
		{
			this.Project.Title = title;
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

	public class Editing : INotifyPropertyChanged
	{
		private Label label;
		private bool started;

		public Visibility Visibility { get; private set; } = Visibility.Collapsed;

		public int X { get; private set; }

		public int Y { get; private set; }

		public string Text { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public event Action<string, Label> Closing;

		public void Start(WindowPoint p, Label label)
		{
			if (this.started)
			{
				throw new InvalidOperationException();
			}

			this.Visibility = Visibility.Visible;
			this.label = label;
			this.Text = label.Text;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Visibility)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Text)));

			this.SetPosition(p.X, p.Y);
			this.started = true;
		}

		private void SetPosition(int x, int y)
		{
			this.X = x;
			this.Y = y;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.X)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Y)));
		}

		public void End()
		{
			if (!this.started)
			{
				return;
			}

			this.Visibility = Visibility.Collapsed;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Visibility)));

			if (!string.Equals(this.Text, this.label.Text, StringComparison.Ordinal))
			{
				this.Closing?.Invoke(this.Text, this.label);
			}

			this.started = false;
		}
	}
}