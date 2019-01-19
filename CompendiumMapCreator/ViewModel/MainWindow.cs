using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.Format;
using Microsoft.Win32;

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

		private RelayCommand undoCommand;
		private RelayCommand redoCommand;
		private RelayCommand deleteCommand;
		private Project _project;
		private IconType selectedType;
		private DragHelper dragging;

		public bool AddLegend { get; set; } = true;

		public string Title => $"DDO Compendium Map Creator{(string.IsNullOrEmpty(this.Project?.File) ? "" : $" - {this.Project?.File}")}";

		public event PropertyChangedEventHandler PropertyChanged;

		public ObservableCollection<Element> Selected { get; } = new ObservableCollection<Element>();

		public IconType[] Types
		{
			get
			{
				return Enum.GetValues(typeof(IconType)) as IconType[];
			}
		}

		public IconType SelectedType
		{
			get => this.selectedType;
			set
			{
				this.selectedType = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectedType)));
			}
		}

		public DelegateCommand<IconType> SetType => new DelegateCommand<IconType>((value) => this.SelectedType = value);

#pragma warning disable RCS1171 // Simplify lazy initialization.
		public RelayCommand UndoCommand
		{
			get
			{
				if (this.undoCommand == null)
				{
					this.undoCommand = new RelayCommand(_ => this.Undo(), _ => (this.Project?.Edits.Count ?? 0) > 0);
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
					this.redoCommand = new RelayCommand(_ => this.Redo(), _ => (this.Project?.Edits.Count ?? 0) < (this.Project?.Edits.Total ?? 0));
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
						if (this.Selected.Count == 0)
						{
							return;
						}

						this.AddEdit(new Remove(new List<Element>(this.Selected)));
						this.Selected.Clear();
					}, _ => this.Selected != null);
				}

				return this.deleteCommand;
			}
		}
#pragma warning restore RCS1171 // Simplify lazy initialization.

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
			}
		});

		public RelayCommand ExportCommand => new RelayCommand((_) => this.Project.Export(this.AddLegend));

		public MainWindow()
		{
			this.Selected.CollectionChanged += (s, e) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Selected)));
		}

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

			this.AddEdit(new Add(element));
		}

		public void Select(ImagePoint point, bool clear = true)
		{
			if (this.Project == null)
			{
				return;
			}

			for (int i = this.Project.Elements.Count - 1; i >= 0; i--)
			{
				if (this.Project.Elements[i].Contains(point))
				{
					if (clear)
					{
						this.Selected.Clear();
					}

					if (this.Selected.Contains(this.Project.Elements[i]))
					{
						this.Selected.Remove(this.Project.Elements[i]);
					}
					else
					{
						this.Selected.Add(this.Project.Elements[i]);
					}

					this.Project.Elements.Move(i, this.Project.Elements.Count - 1);

					return;
				}
			}

			this.Selected.Clear();
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

		public void Copy(ImagePoint mousePosition, IList<Element> elements) => this.AddEdit(new Copy(elements, mousePosition));

		public void AddEdit(Edit edit, bool apply = true)
		{
			if (apply)
			{
				this.Project.Edits.Add(edit, this.Project.Elements);
			}
			else
			{
				this.Project.Edits.Add(edit);
			}

			this.undoCommand?.RaiseCanExecuteChanged();
			this.redoCommand?.RaiseCanExecuteChanged();
		}

		public void Undo()
		{
			this.Project.Edits.Undo(this.Project.Elements);

			this.undoCommand?.RaiseCanExecuteChanged();
			this.redoCommand?.RaiseCanExecuteChanged();
		}

		public void Redo()
		{
			this.Project.Edits.Redo(this.Project.Elements);

			this.undoCommand?.RaiseCanExecuteChanged();
			this.redoCommand?.RaiseCanExecuteChanged();
		}

		public void DragStart(ImagePoint p)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				return;
			}

			if (!Keyboard.IsKeyDown(Key.Space) && this.SelectedType == IconType.Cursor)
			{
				if (!this.Selected.Any(e => e.Contains(p)))
				{
					this.Select(p);
				}

				if (this.Selected.Count != 0)
				{
					this.dragging = new DragHelper(new List<Element>(this.Selected), p);
				}
			}
		}

		public void DragUpdate(ImagePoint p)
		{
			this.dragging?.Update(p.X, p.Y);
		}

		public void DragEnd()
		{
			Edit result = this.dragging?.End();

			if (result != null)
			{
				this.AddEdit(result, apply: false);
			}

			this.dragging = null;
		}

		public void Click(ImagePoint p)
		{
			if (this.SelectedType != IconType.Cursor)
			{
				Element element = this.CreateElement(this.SelectedType);

				ImagePoint position = p - new ImagePoint(element.Width / 2, element.Height / 2);

				element.X = position.X;
				element.Y = position.Y;

				this.AddElement(element);
				this.Selected.Clear();
			}
			else
			{
				this.Select(p, !Keyboard.IsKeyDown(Key.LeftCtrl));
			}
		}

		private class DragHelper
		{
			private (int X, int Y) Change { get; set; }

			private IList<Element> Elements { get; }

			private ImagePoint Start { get; }

			private ImagePoint[] Offsets { get; }

			public DragHelper(IList<Element> elements, ImagePoint start)
			{
				this.Elements = elements;
				this.Start = start;
				this.Offsets = this.Elements.Select(e => start - new ImagePoint(e.X, e.Y)).ToArray();

				for (int i = 0; i < this.Elements.Count; i++)
				{
					this.Elements[i].Opacity = 0.25;
				}
			}

			public void Update(int x, int y)
			{
				for (int i = 0; i < this.Elements.Count; i++)
				{
					this.Elements[i].X = x - this.Offsets[i].X;
					this.Elements[i].Y = y - this.Offsets[i].Y;
				}

				this.Change = (x - this.Start.X, y - this.Start.Y);
			}

			public Edit End()
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

				return result;
			}
		}
	}
}