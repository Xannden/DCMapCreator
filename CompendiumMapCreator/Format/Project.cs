using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Edits;
using Microsoft.Win32;
using DImage = System.Drawing.Image;

namespace CompendiumMapCreator.Format
{
	public abstract class Project : INotifyPropertyChanged
	{
		private const int Version = 5;

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
					using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(dialog.FileName)))
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
								project = new ProjectV1(dialog.FileName);
								break;

							case 2:
							case 3:
							case 4:
								project = new ProjectV2(dialog.FileName);
								break;

							case 5:
								project = new ProjectV3(dialog.FileName, reader.ReadString());
								break;

							default:
								throw new InvalidDataException();
						}

						project.Load(reader);

						return project;
					}
				}
				catch (Exception)
				{
					MessageBox.Show("Unable to load file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			return null;
		}

		public static Project FromImage(Image image) => new ProjectV3(image);

		protected Project(Image image)
		{
			this.Image = image;
			this.Elements = new ObservableCollection<Element>();
			this.Selected.CollectionChanged += (s, e) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Selected)));
		}

		protected Project(string file)
		{
			this.File = file;
			this.Elements = new ObservableCollection<Element>();
			this.Selected.CollectionChanged += (s, e) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Selected)));
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
					}

					if (this.Selected.Contains(this.Elements[i]))
					{
						this.Selected.Remove(this.Elements[i]);
					}
					else
					{
						this.Selected.Add(this.Elements[i]);
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
				int value = reader.ReadInt32();

				Element e = this.ReadElement(reader, this.ReadType(value));

				if (e != null)
				{
					this.Elements.Add(e);
				}
			}
		}

		protected abstract Element ReadElement(BinaryReader reader, IconType? type);

		protected abstract IconType? ReadType(int value);

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
					writer.Write(l.Text);

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

				using (Font font = new Font(new FontFamily(GenericFontFamilies.SansSerif), 8))
				using (DImage map = this.CreateMap())
				using (DImage legend = addLegend ? this.CreateLegend(font) : null)
				{
					Layout layout = this.GetLayout(font, map, legend);

					using (DImage info = this.CreateInfoList(font, layout))
					using (DImage image = new Bitmap(layout.Width(), layout.Height(), PixelFormat.Format32bppArgb))
					using (Graphics g = Graphics.FromImage(image))
					{
						g.FillRectangle(Brushes.Black, 0, 0, layout.Width(), layout.Height());

						g.DrawBoarders(layout);

						if (!string.IsNullOrEmpty(this.Title))
						{
							using (Font titleFont = new Font(new FontFamily(GenericFontFamilies.SansSerif), 15))
							{
								StringFormat format = new StringFormat()
								{
									Alignment = StringAlignment.Center,
								};

								g.DrawString(this.Title, titleFont, Brushes.White, layout.Title.ToRectF(), format);
							}
						}

						g.DrawImage(map, layout.Map.X, layout.Map.Y);

						if (info != null)
						{
							g.DrawImage(info, layout.Info.X, layout.Info.Y);
						}

						if (legend != null)
						{
							g.DrawImage(legend, layout.Legend.X, layout.Legend.Y);
						}

						image.Save(dialog.FileName, ImageFormat.Png);
					}
				}
			}
		}

		private Layout GetLayout(Font font, DImage map, DImage legend)
		{
			Layout layout = new Layout(map);

			if (!string.IsNullOrEmpty(this.Title))
			{
				layout.AddTitle();
			}

			if (legend != null)
			{
				layout.AddLegend(legend.Height);
			}

			if (this.Elements.Any(e => e is Label l && !string.IsNullOrEmpty(l.Text)))
			{
				layout.AddInfo();
			}

			layout.Finish(this.Elements, font);

			return layout;
		}

		private DImage CreateLegend(Font font)
		{
			List<IconType> icons = new List<IconType>();

			foreach (Element item in this.Elements)
			{
				if (item.Type != IconType.Label && item.Type != IconType.Entrance && !icons.Contains(item.Type))
				{
					icons.Add(item.Type);
				}
			}

			icons.Sort();

			DImage image = new Bitmap(147, 20 + (icons.Count * 20), PixelFormat.Format32bppArgb);

			using (Graphics g = Graphics.FromImage(image))
			{
				g.DrawImage(Image.GetImageFromResources("Icons/entrance.png").DrawingImage, 10, 0);
				g.DrawString("Dungeon Entrance", font, new SolidBrush(Color.White), 30, 0);

				int position = 20;

				foreach (IconType item in icons)
				{
					g.DrawLegendLine(font, item, ref position);
				}
			}

			return image;
		}

		private DImage CreateInfoList(Font font, Layout layout)
		{
			List<Label> labels = this.Elements.GetLabels();

			DImage info = new Bitmap(layout.Info.Width, layout.Info.Height);

			using (Graphics g = Graphics.FromImage(info))
			{
				StringFormat format = new StringFormat()
				{
					FormatFlags = StringFormatFlags.LineLimit,
					Trimming = StringTrimming.None,
				};

				for (int i = 0; i < labels.Count; i++)
				{
					Image icon = labels[i].Image;

					g.DrawImage(icon.DrawingImage, layout.Labels[i].X + (9 - (icon.Width / 2)), layout.Labels[i].Y + (7 - (icon.Height / 2)));

					g.DrawString(labels[i].Text, font, Brushes.White, layout.Labels[i].ToRectF(xOffset: 16, widthOffset: -16), format);
				}
			}

			return info;
		}

		private DImage CreateMap()
		{
			(int x, int y) min = (0, 0);
			(int x, int y) max = (this.Image.Width, this.Image.Height);

			for (int i = 0; i < this.Elements.Count; i++)
			{
				min.x = Math.Min(min.x, this.Elements[i].X);
				min.y = Math.Min(min.y, this.Elements[i].Y);
				max.x = Math.Max(max.x, this.Elements[i].X + this.Elements[i].Width);
				max.y = Math.Max(max.y, this.Elements[i].Y + this.Elements[i].Height);
			}

			Rectangle bounding = Rectangle.FromLTRB(min.x, min.y, max.x, max.y);

			Bitmap image = new Bitmap(bounding.Width + 10, bounding.Height + 10);

			using (Graphics g = Graphics.FromImage(image))
			{
				int xOffset = Math.Abs(min.x) + 5;
				int yOffset = Math.Abs(min.y) + 5;

				g.DrawImage(this.Image.DrawingImage, xOffset, yOffset);

				for (int i = 0; i < this.Elements.Count; i++)
				{
					g.DrawImage(this.Elements[i].Image.DrawingImage, this.Elements[i].X + xOffset, this.Elements[i].Y + yOffset);
				}
			}

			return image;
		}
	}

	public static class Extensions
	{
		public static void DrawVerticalLine(this Graphics g, int x, int y0, int y1)
		{
			g.DrawLine(new Pen(Color.Gray, 1f), x - 1, y0, x - 1, y1);
			g.DrawLine(new Pen(Color.White, 1f), x, y0, x, y1);
			g.DrawLine(new Pen(Color.Gray, 1f), x + 1, y0, x + 1, y1);
		}

		public static void DrawHorizontalLine(this Graphics g, int y, int x0, int x1)
		{
			g.DrawLine(new Pen(Color.Gray, 1f), x0, y - 1, x1, y - 1);
			g.DrawLine(new Pen(Color.White, 1f), x0, y, x1, y);
			g.DrawLine(new Pen(Color.Gray, 1f), x0, y + 1, x1, y + 1);
		}

		public static void DrawLegendLine(this Graphics g, Font font, IconType type, ref int position)
		{
			Image image = Image.GetImageFromResources(type.GetImageFile());

			g.DrawImage(image.DrawingImage, 10 + (9 - (image.Width / 2)), position + (7 - (image.Height / 2)));
			g.DrawString(type.GetDescription(), font, new SolidBrush(Color.White), 30, position);

			position += 20;
		}

		public static void DrawBoarders(this Graphics g, Layout layout)
		{
			if (layout.Title != null)
			{
				g.DrawHorizontalLine(layout.Title.Height - 2, layout.Title.X, layout.Title.Width);
			}

			if (layout.Legend.Height > layout.Map.Height)
			{
				g.DrawVerticalLine(layout.Legend.Width - 2, layout.Legend.Y - 1, layout.Height());

				if (layout.Info != null)
				{
					g.DrawHorizontalLine(layout.Info.Y - 2, layout.Info.X - 1, layout.Width());
				}
			}
			else
			{
				if (layout.Info != null)
				{
					g.DrawHorizontalLine(layout.Info.Y - 2, layout.Info.X, layout.Width());
				}

				g.DrawVerticalLine(layout.Legend.Width - 2, layout.Legend.Y - 1, layout.Map.Height);
			}
		}

		public static List<Label> GetLabels(this IList<Element> elements)
		{
			List<Label> labels = new List<Label>();

			for (int i = 0; i < elements.Count; i++)
			{
				if (elements[i] is Label l && !string.IsNullOrEmpty(l.Text))
				{
					labels.Add(l);
				}
			}

			if (labels.Count == 0)
			{
				return null;
			}

			labels.Sort((lhs, rhs) => lhs.Number.CompareTo(rhs.Number));

			return labels;
		}
	}
}