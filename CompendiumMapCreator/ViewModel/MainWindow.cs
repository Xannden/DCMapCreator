using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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

		private Element selected;
		private RelayCommand undoCommand;
		private RelayCommand redoCommand;
		private RelayCommand deleteCommand;
		private Project _project;
		private IconType selectedType;

		public bool AddLegend { get; set; } = true;

		public string Title => $"DDO Compendium Map Creator{(string.IsNullOrEmpty(this.Project?.File) ? "" : $" - {this.Project?.File}")}";

		public event PropertyChangedEventHandler PropertyChanged;

		public Element Selected
		{
			get => this.selected;
			set
			{
				this.selected = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Selected)));
				this.DeleteCommand.RaiseCanExecuteChanged();
			}
		}

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
						if (this.Selected == null)
						{
							return;
						}

						this.AddEdit(new Remove(this.Selected));
						this.Selected = null;
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

		public DelegateCommand<Direction> MoveCommand => new DelegateCommand<Direction>(direction =>
		{
			if (this.Selected == null)
			{
				return;
			}

			switch (direction)
			{
				case Direction.Left:
					this.Selected.X--;
					break;

				case Direction.Right:
					this.Selected.X++;
					break;

				case Direction.Up:
					this.Selected.Y--;
					break;

				case Direction.Down:
					this.Selected.Y++;
					break;
			}
		});

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

		public Element Select(ImagePoint point)
		{
			if (this.Project == null)
			{
				return null;
			}

			for (int i = this.Project.Elements.Count - 1; i >= 0; i--)
			{
				if (this.Project.Elements[i].Contains(point))
				{
					this.Selected = this.Project.Elements[i];

					this.Project.Elements.Move(i, this.Project.Elements.Count - 1);

					return this.Selected;
				}
			}

			this.Selected = null;

			return this.Selected;
		}

		public Element CreateElement(IconType type)
		{
			switch (type)
			{
				case IconType.Label:
					return new Label("", this.Project.Elements.Count((e) => e is Label));

				case IconType.Portal:
					return new Portal(this.Project.Elements.Count((e) => e is Portal));

				default:
					return new Element(type);
			}
		}

		public void Copy(ImagePoint? mousePosition, Element element) => this.AddEdit(new Copy(element, mousePosition));

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
	}
}