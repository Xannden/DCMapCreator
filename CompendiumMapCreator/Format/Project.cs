using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using CompendiumMapCreator.Format.Export;
using Microsoft.Win32;

namespace CompendiumMapCreator.Format
{
	public abstract class Project : INotifyPropertyChanged
	{
		private const int Version = 6;

		private (int gen, int count) saved = (0, 0);
		private Image _image;
		private string title;

		public event PropertyChangedEventHandler PropertyChanged;

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
				catch (Exception)
				{
					MessageBox.Show("Unable to load file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return null;
		}

		public static Project LoadFile(string file)
		{
			using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(file)))
			{
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

					default:
						throw new InvalidDataException();
				}

				project.Load(reader);

				return project;
			}
		}

		public static Project FromImage(Image image) => new ProjectV3(image);

		protected Project(Image image)
		{
			this.Image = image;
			this.Elements = new ObservableCollection<Element>();
		}

		protected Project(string file)
		{
			this.File = file;
			this.Elements = new ObservableCollection<Element>();
		}

		public string File
		{
			get; private set;
		}

		public Image Image
		{
			get => this._image;
			set
			{
				this._image = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Image)));
			}
		}

		public ObservableCollection<Element> Elements
		{
			get; protected set;
		}

		public UndoRedoStack<Edit> Edits { get; } = new UndoRedoStack<Edit>();

		public ObservableCollection<Element> Selected { get; } = new ObservableCollection<Element>();

		public string Title
		{
			get => this.title;
			set
			{
				this.title = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
			}
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

		public void Undo() => this.Edits.Undo(this.Elements);

		public void Redo() => this.Edits.Redo(this.Elements);

		public void Copy(ImagePoint mousePosition, IList<Element> elements) => this.AddEdit(new Copy(elements, mousePosition));

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

					if (this.Elements[i] is AreaElement)
					{
						int index = 0;

						for (int j = this.Elements.Count - 1; j >= 0; j--)
						{
							if (this.Elements[j] is AreaElement)
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
				try
				{
					using (MemoryStream stream = new MemoryStream())
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						writer.Write(407893541);
						writer.Write("DMC".ToCharArray());
						writer.Write(Version);
						writer.Write(this.Title ?? string.Empty);
						writer.Write(this.Image.Data.Length);
						writer.Write(this.Image.Data.GetBuffer());
						writer.Write(this.Elements.Count);

						foreach (Element item in this.Elements)
						{
							writer.Write((int)item.Type);
							this.WriteElement(writer, item);
						}

						System.IO.File.WriteAllBytes(this.File, stream.ToArray());
					}

					this.saved = (this.Edits.Generation, this.Edits.Count);
				}
				catch (Exception)
				{
					MessageBox.Show("Unable to save file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		public bool HasUnsaved() => this.saved.gen != this.Edits.Generation || this.saved.count != this.Edits.Count;

		private void Load(BinaryReader reader)
		{
			long length = reader.ReadInt64();
			byte[] data = reader.ReadBytes((int)length);

			this.Image = new Image(data);

			int count = reader.ReadInt32();
			this.Elements.Clear();

			for (int i = 0; i < count; i++)
			{
				Element e = this.ReadElement(reader);

				if (e != null)
				{
					this.Elements.Add(e);
				}
			}

			int index = -1;

			for (int i = this.Elements.Count - 1; i >= 0; i--)
			{
				if (this.Elements[i] is AreaElement)
				{
					if (index == -1)
					{
						index = i;
					}
				}
				else if (index != -1)
				{
					this.Elements.Move(i, index);
					index--;
				}
			}
		}

		protected abstract Element ReadElement(BinaryReader reader);

		private void WriteElement(BinaryWriter writer, Element element)
		{
			writer.Write(element.X);
			writer.Write(element.Y);
			writer.Write(element.IsCopy);

			switch (element)
			{
				case Label l:
					long start = writer.BaseStream.Position;
					writer.Write(0);
					writer.Write(l.Number);
					writer.Write(l.Text ?? string.Empty);

					long end = writer.BaseStream.Position;
					writer.Seek(-(int)(end - start), SeekOrigin.Current);

					writer.Write((int)(end - start));

					writer.Seek((int)(end - start - 4), SeekOrigin.Current);
					break;

				case Portal p:
					writer.Write(sizeof(int));
					writer.Write(p.Number);
					break;

				case AreaElement a:
					writer.Write(sizeof(int) + sizeof(int));
					writer.Write(a.AreaWidth);
					writer.Write(a.AreaHeight);
					break;

				case Entrance e:
					writer.Write(sizeof(int));
					writer.Write((int)e.Rotation);
					break;

				default:
					writer.Write(0);
					break;
			}
		}

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

		internal void OnPropertyChanged(string name)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}