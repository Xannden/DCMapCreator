using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.Format.Export;
using CompendiumMapCreator.Format.Projects;

//using CompendiumMapCreator.Format.Projects;
using CompendiumMapCreator.ViewModel;
using Microsoft.Win32;

namespace CompendiumMapCreator.Format
{
	public abstract class Project : INotifyPropertyChanged
	{
		private const int Version = 9;

		private Image image;
		private (int gen, int count) saved = (0, 0);
		private string title;

		protected Project(Image image)
		{
			this.Image = image;
			this.Elements = new ObservableCollection<ElementVM>();
		}

		protected Project(string file)
		{
			this.File = file;
			this.Elements = new ObservableCollection<ElementVM>();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public UndoRedoStack<Edit> Edits { get; } = new UndoRedoStack<Edit>();

		public ObservableCollection<ElementVM> Elements
		{
			get; protected set;
		}

		public string File
		{
			get; private set;
		}

		public Image Image
		{
			get => this.image;

			set
			{
				this.image = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Image)));
			}
		}

		public ObservableCollection<ElementVM> Selected { get; } = new ObservableCollection<ElementVM>();

		public string Title
		{
			get => this.title;

			set
			{
				this.title = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
			}
		}

		internal virtual bool SupportsOptional => true;

		internal virtual bool SupportsCopy => true;

		internal virtual bool SupportsExtraData => true;

		internal virtual bool SupportsRotation => true;
		public static Project FromImage(Image image) => new ProjectV6(image);

		public static Project Load(ref string initialDirectory)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				DefaultExt = ".dmc",
				Filter = "Project (*.dmc)|*.dmc",
			};

			if (!string.IsNullOrEmpty(initialDirectory))
			{
				dialog.InitialDirectory = initialDirectory;
			}

			bool? result = dialog.ShowDialog();

			if (result.GetValueOrDefault())
			{
				initialDirectory = Path.GetDirectoryName(dialog.FileName);
				try
				{
					return Project.LoadFile(dialog.FileName);
				}
				catch (Exception e)
				{
					System.IO.File.AppendAllText("errorLog.txt", e.ToString());
					MessageBox.Show("Unable to load file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return null;
		}

		public static Project LoadFile(string file)
		{
			using BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(file));

			int magic = reader.ReadInt32();

			if (magic != 407893541)
			{
				throw new InvalidDataException();
			}

			char[] check = reader.ReadChars(3);

			if (check[0] != 'D' || check[1] != 'M' || check[2] != 'C')
			{
				throw new InvalidDataException();
			}

			int version = reader.ReadInt32();

			Project project;

			switch (version)
			{
				case 1:
					project = new ProjectV1(file);
					break;

				case 2:
				case 3:
				case 4:
					project = new ProjectV2(file);
					break;

				case 5:
					project = new ProjectV3(file, reader.ReadString());
					break;

				case 6:
					project = new ProjectV4(file, reader.ReadString());
					break;

				case 7:
				case 8:
					reader.ReadInt32();
					project = new ProjectV5(file, reader.ReadString());
					break;

				case 9:
					reader.ReadInt32();
					project = new ProjectV6(file, reader.ReadString());
					break;

				default:
					throw new InvalidDataException();
			}

			project.Load(reader);

			return project;
		}

		public void AddEdit(Edit edit, bool apply = true)
		{
			if (apply)
			{
				this.Edits.Add(edit, this.Elements);
			}
			else
			{
				this.Edits.Add(edit);
			}
		}

		public void Copy(ImagePoint mousePosition, IList<ElementVM> elements) => this.AddEdit(new Copy(elements, mousePosition));

		public void Export(bool addLegend, ref string initialDirectory)
		{
			if (this.Image == null)
			{
				return;
			}

			SaveFileDialog dialog = new SaveFileDialog
			{
				AddExtension = true,
				DefaultExt = ".png",
				Filter = "PNG (*.png)|*.png",
				OverwritePrompt = true,
				ValidateNames = true,
			};

			if (!string.IsNullOrEmpty(initialDirectory))
			{
				dialog.InitialDirectory = initialDirectory;
			}

			bool? result = dialog.ShowDialog();

			if (result.HasValue && result == true)
			{
				initialDirectory = Path.GetDirectoryName(dialog.FileName);

				this.Export(addLegend, dialog.FileName);
			}
		}

		public void Export(bool addLegend, string file)
		{
			Exporter.Run(file, this.Image, this.Elements, addLegend, this.Title);
		}

		public bool HasUnsaved() => this.saved.gen != this.Edits.Generation || this.saved.count != this.Edits.Count;

		public void Redo() => this.Edits.Redo(this.Elements);

		public void Save(ref string initialDirectory, bool forcePrompt)
		{
			if (this.Image == null)
			{
				return;
			}

			bool result = this.File != null;

			if (forcePrompt || string.IsNullOrEmpty(this.File))
			{
				SaveFileDialog dialog = new SaveFileDialog
				{
					AddExtension = true,
					DefaultExt = ".dmc",
					Filter = "Project (*.dmc)|*.dmc",
					OverwritePrompt = true,
					ValidateNames = true,
				};

				if (!string.IsNullOrEmpty(initialDirectory))
				{
					dialog.InitialDirectory = initialDirectory;
				}

				result = dialog.ShowDialog() ?? false;

				if (result)
				{
					this.File = dialog.FileName;
					initialDirectory = Path.GetDirectoryName(dialog.FileName);
				}
			}

			if (result)
			{
				this.WriteData();
			}
		}

		public void Select(ImagePoint point, bool clear = true)
		{
			for (int i = this.Elements.Count - 1; i >= 0; i--)
			{
				if (this.Elements[i].Contains(point))
				{
					if (clear)
					{
						this.Selected.Clear();
						this.OnPropertyChanged(nameof(this.Selected));
					}

					if (this.Selected.Contains(this.Elements[i]))
					{
						this.Selected.Remove(this.Elements[i]);
						this.OnPropertyChanged(nameof(this.Selected));
					}
					else
					{
						this.Selected.Add(this.Elements[i]);
						this.OnPropertyChanged(nameof(this.Selected));
					}

					if (this.Elements[i] is AreaElementVM)
					{
						int index = 0;

						for (int j = this.Elements.Count - 1; j >= 0; j--)
						{
							if (this.Elements[j] is AreaElementVM)
							{
								index = j;
								break;
							}
						}

						this.Elements.Move(i, index);
					}
					else
					{
						this.Elements.Move(i, this.Elements.Count - 1);
					}

					return;
				}
			}

			for (int i = 0; i < this.Selected.Count; i++)
			{
				this.Selected[i].Opacity = 1;
			}

			this.Selected.Clear();
			this.OnPropertyChanged(nameof(this.Selected));
		}

		public void Undo() => this.Edits.Undo(this.Elements);

		internal void OnPropertyChanged(string name)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		protected abstract Dictionary<int, ElementId> ReadElementTable(BinaryReader reader);

		private void Load(BinaryReader reader)
		{
			this.Image = this.ReadImage(reader);

			Dictionary<int, ElementId> table = this.ReadElementTable(reader);

			int count = reader.ReadInt32();
			this.Elements.Clear();

			for (int i = 0; i < count; i++)
			{
				int id = reader.ReadInt32();

				if (!table.TryGetValue(id, out ElementId eId))
				{
					continue;
				}

				ElementVM e = ElementVM.ReadElement(reader, eId, this);

				if (e != null)
				{
					this.Elements.Add(e);
				}
			}
		}

		private Image ReadImage(BinaryReader reader)
		{
			long length = reader.ReadInt64();
			byte[] data = reader.ReadBytes((int)length);

			return new Image(data);
		}

		private void WriteData()
		{
			try
			{
				using (MemoryStream stream = new MemoryStream())
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					this.WriteHeader(writer);

					writer.Write(this.Image.Data.Length);
					writer.Write(this.Image.Data.GetBuffer());

					Dictionary<ElementId, int> table = App.Config.WriteElementTable(writer); // Element type table

					this.WriteElements(writer, table);

					System.IO.File.WriteAllBytes(this.File, stream.ToArray());
				}

				this.saved = (this.Edits.Generation, this.Edits.Count);
			}
			catch (Exception e)
			{
				System.IO.File.AppendAllText("errorLog.txt", e.ToString());
				MessageBox.Show("Unable to save file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void WriteHeader(BinaryWriter writer)
		{
			writer.Write(407893541); // Magic number
			writer.Write("DMC".ToCharArray()); // File name check
			writer.Write(Version); // Project Version
			writer.Write(0); // TODO: Dungeon or wilderness
			writer.Write(this.Title ?? string.Empty); // project title
		}

		private void WriteElements(BinaryWriter writer, Dictionary<ElementId, int> table)
		{
			writer.Write(this.Elements.Count);

			foreach (ElementVM element in this.Elements)
			{
				writer.Write(table[element.Id]);
				element.WriteElement(writer);
			}
		}
	}
}