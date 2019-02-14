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
								string title = reader.ReadString();

								project = new ProjectV2(dialog.FileName)
								{
									Title = title
								};
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

		public static Project FromImage(Image image) => new ProjectV2(image);

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

		public string Title { get; set; }

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

		public void Save(ref string initialDirectory)
		{
			if (this.Image == null)
			{
				return;
			}

			bool result = this.File != null;

			if (string.IsNullOrEmpty(this.File))
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

				IconType[] values = Enum.GetValues(typeof(IconType)) as IconType[];

				if (value > (int)values[values.Length - 1])
				{
					throw new InvalidDataException();
				}

				IconType type = (IconType)value;

				this.Elements.Add(this.ReadElement(reader, type));
			}
		}

		protected abstract Element ReadElement(BinaryReader reader, IconType type);

		private void WriteElement(BinaryWriter writer, Element element)
		{
			writer.Write(element.X);
			writer.Write(element.Y);
			writer.Write(element.IsCopy);

			switch (element)
			{
				case Label l:
					writer.Write(l.Number);
					writer.Write(l.Text);
					break;

				case Portal p:
					writer.Write(p.Number);
					break;

				case AreaElement a:
					writer.Write(a.AreaWidth);
					writer.Write(a.AreaHeight);
					break;

				case Entrance e:
					writer.Write((int)e.Rotation);
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
					Layout layout = this.GetLayout(map, legend);

					using (DImage info = this.CreateInfoList(font, layout.Info))
					using (DImage image = new Bitmap(layout.Width(), layout.Height(), PixelFormat.Format32bppArgb))
					using (Graphics g = Graphics.FromImage(image))
					{
						g.FillRectangle(Brushes.Black, 0, 0, layout.Width(), layout.Height());

						g.DrawBoarders(layout);

						if (!string.IsNullOrEmpty(this.Title))
						{
							using (Font titleFont = new Font(new FontFamily(GenericFontFamilies.SansSerif), 15))
							{
								int width = (int)g.MeasureString(this.Title, titleFont).Width;

								int x = (layout.Title.Width / 2) - (width / 2);

								g.DrawString(this.Title, titleFont, Brushes.White, x, 0);
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

		private Layout GetLayout(DImage map, DImage legend)
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

			layout.Finish();

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
				g.FillRectangle(Brushes.Black, 0, 0, image.Width, image.Height);
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

		private DImage CreateInfoList(Font font, Position p)
		{
			List<Label> labels = new List<Label>();

			for (int i = 0; i < this.Elements.Count; i++)
			{
				if (this.Elements[i] is Label l && !string.IsNullOrEmpty(l.Text))
				{
					labels.Add(l);
				}
			}

			if (labels.Count == 0)
			{
				return null;
			}

			labels.Sort((lhs, rhs) => lhs.Number.CompareTo(rhs.Number));

			int columns = p.Width / 200;

			if (columns > labels.Count)
			{
				columns = labels.Count;
			}

			columns = Math.Max(columns, 1);

			int rows = (int)Math.Ceiling(labels.Count / (float)columns);

			float columnWidth = p.Width / (float)columns;

			int[] rowHeights = new int[rows];

			StringFormat format = new StringFormat()
			{
				FormatFlags = StringFormatFlags.LineLimit,
				Trimming = StringTrimming.None,
			};

			using (Graphics g = Graphics.FromImage(new Bitmap(10, 10)))
			{
				for (int i = 0; i < labels.Count; i++)
				{
					SizeF size = g.MeasureString(labels[i].Text, font, new SizeF(columnWidth - 16, 1000), format);

					rowHeights[i / columns] = Math.Max(Math.Max((int)Math.Ceiling(size.Height), rowHeights[i / columns]), 20);
				}
			}

			DImage info = new Bitmap(p.Width, rowHeights.Sum());

			using (Graphics g = Graphics.FromImage(info))
			{
				g.FillRectangle(Brushes.Black, 0, 0, info.Width, info.Height);

				float rowOffset = 0;

				for (int i = 0; i < labels.Count; i++)
				{
					float columnOffset = i % columns * columnWidth;

					Image icon = labels[i].Image;

					g.DrawImage(icon.DrawingImage, (int)columnOffset + (9 - (icon.Width / 2)), rowOffset + (7 - (icon.Height / 2)));

					g.DrawString(labels[i].Text, font, Brushes.White, new RectangleF(columnOffset + 16, rowOffset, columnWidth - 16, rowHeights[i / columns]), format);

					if ((i + 1) % columns == 0)
					{
						rowOffset += rowHeights[i / columns];
					}
				}
			}

			p.Height = info.Height;

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

	public class Layout
	{
		public Position Title { get; private set; }

		public Position Legend { get; private set; }

		public Position Map { get; }

		public Position Info { get; private set; }

		public Layout(DImage map)
		{
			this.Map = new Position()
			{
				X = 0,
				Y = 0,
				Width = map.Width,
				Height = map.Height,
			};
		}

		public void AddTitle()
		{
			this.Title = new Position()
			{
				X = 0,
				Y = 0,
				Height = 28,
				Width = this.Map.Width,
			};
		}

		public void AddLegend(int legendHeight)
		{
			this.Legend = new Position()
			{
				X = 0,
				Y = 0,
				Width = 150,
				Height = legendHeight,
			};
		}

		public void AddInfo()
		{
			this.Info = new Position();
		}

		public void Finish()
		{
			if (this.Title != null)
			{
				if (this.Legend != null)
				{
					this.Legend.Y = this.Title.Height;
					this.Title.Width += this.Legend.Width;
				}

				this.Map.Y = this.Title.Height;
			}

			if (this.Legend != null)
			{
				this.Map.X = this.Legend.Width;
			}

			if (this.Info != null)
			{
				if ((this.Legend?.Height ?? 0) > this.Map.Height)
				{
					this.Info.X = this.Legend.Width;
					this.Info.Width = this.Map.Width;
				}
				else
				{
					this.Info.X = 0;
					this.Info.Width = this.Map.Width + (this.Legend?.Width ?? 0);
				}

				this.Info.Y = this.Map.Y + this.Map.Height + 3;
			}
		}

		public int Width()
		{
			return (this.Legend?.Width ?? 0) + this.Map.Width;
		}

		public int Height()
		{
			int height = this.Title?.Height ?? 0;

			if (this.Legend?.Height > this.Map.Height + (this.Info?.Height ?? 0))
			{
				height += this.Legend.Height;
			}
			else
			{
				height += this.Map.Height + (this.Info?.Height ?? 0);
			}

			return height;
		}
	}

	public class Position
	{
		public int X { get; set; }

		public int Y { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }
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
	}
}