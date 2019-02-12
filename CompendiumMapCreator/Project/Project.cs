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
		private (int gen, int count) saved = (0, 0);

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
			get; protected set;
		}

		public ObservableCollection<Element> Elements
		{
			get; protected set;
		}

		public UndoRedoStack<Edit> Edits { get; } = new UndoRedoStack<Edit>();

		public ObservableCollection<Element> Selected { get; } = new ObservableCollection<Element>();

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
					using (BinaryWriter writer = new BinaryWriter(System.IO.File.Create(this.File)))
					{
						writer.Write(407893541);
						writer.Write("DMC".ToCharArray());
						writer.Write(4);
						writer.Write(this.Image.Data.Length);
						writer.Write(this.Image.Data.GetBuffer());
						writer.Write(this.Elements.Count);

						foreach (Element item in this.Elements)
						{
							writer.Write((int)item.Type);
							this.WriteElement(writer, item);
						}
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
				using (DImage legend = addLegend ? this.CreateLegend(font) : null)
				{
					((int x, int y) offset, Area imageArea) = this.GetImageAreaAndOffsets((legend?.Width + 3) ?? 0);

					Area fullArea = new Area(imageArea.Width + (legend != null ? legend.Width + 3 : 0), 0);

					using (DImage info = this.CreateInfoList(font, fullArea, imageArea, legend?.Height ?? 0))
					{
						using (DImage image = new Bitmap(fullArea.Width, fullArea.Height, PixelFormat.Format32bppArgb))
						{
							using (Graphics g = Graphics.FromImage(image))
							{
								g.FillRectangle(Brushes.Black, 0, 0, fullArea.Width, fullArea.Height);

								g.DrawBoarders(fullArea, imageArea.Height, legend?.Height ?? 0, info != null);

								g.DrawImage(this.Image.DrawingImage, offset.x, offset.y);

								for (int i = 0; i < this.Elements.Count; i++)
								{
									g.DrawImage(this.Elements[i].Image.DrawingImage, this.Elements[i].X + offset.x, (float)this.Elements[i].Y + offset.y);
								}

								if (info != null)
								{
									if (imageArea.Height > (legend?.Height ?? 0))
									{
										g.DrawImage(info, 0, imageArea.Height + 3);
									}
									else
									{
										g.DrawImage(info, (legend?.Width + 3) ?? 0, imageArea.Height + 3);
									}
								}

								if (legend != null)
								{
									g.DrawImage(legend, 0, 0);
								}
							}

							image.Save(dialog.FileName, ImageFormat.Png);
						}
					}
				}
			}
		}

		private ((int x, int y), Area) GetImageAreaAndOffsets(int legendWidth)
		{
#pragma warning disable IDE0042 // Deconstruct variable declaration
			(int x, int y) min = (int.MaxValue, int.MaxValue);
			(int x, int y) max = (int.MinValue, int.MinValue);
#pragma warning restore IDE0042 // Deconstruct variable declaration

			for (int i = 0; i < this.Elements.Count; i++)
			{
				min.x = Math.Min(min.x, this.Elements[i].X);
				min.y = Math.Min(min.y, this.Elements[i].Y);
				max.x = Math.Max(max.x, this.Elements[i].X + this.Elements[i].Width);
				max.y = Math.Max(max.y, this.Elements[i].Y + this.Elements[i].Height);
			}

			Rectangle bounding = Rectangle.FromLTRB(min.x, min.y, max.x, max.y);
			int width = this.Image.Width;
			int height = this.Image.Height;
			int xOffset = 0;
			int yOffset = 0;

			if (bounding.Left < 0)
			{
				int diff = -bounding.Left;

				xOffset = diff;
				width += diff;
			}

			if (bounding.Right > this.Image.Width)
			{
				width += bounding.Right - this.Image.Width;
			}

			if (bounding.Top < 0)
			{
				int diff = -bounding.Top;

				yOffset = diff;
				height += diff;
			}

			if (bounding.Bottom > this.Image.Height)
			{
				height += bounding.Bottom - this.Image.Height;
			}

			return ((legendWidth + xOffset + 5, yOffset + 5), new Area(width + 10, height + 10));
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

		private DImage CreateInfoList(Font font, Area fullArea, Area imageArea, int legendHeight)
		{
			int width = imageArea.Height > legendHeight ? fullArea.Width : imageArea.Width;

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
				if (imageArea.Height > legendHeight)
				{
					fullArea.Height = imageArea.Height;
				}
				else
				{
					fullArea.Height = Math.Max(legendHeight, imageArea.Height);
				}

				return null;
			}

			labels.Sort((lhs, rhs) => lhs.Number.CompareTo(rhs.Number));

			int columns = width / 200;

			if (columns > labels.Count)
			{
				columns = labels.Count;
			}

			columns = Math.Max(columns, 1);

			int rows = (int)Math.Ceiling(labels.Count / (float)columns);

			float columnWidth = width / (float)columns;

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

			DImage info = new Bitmap(width, rowHeights.Sum());

			using (Graphics g = Graphics.FromImage(info))
			{
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

			if (imageArea.Height > legendHeight)
			{
				fullArea.Height = imageArea.Height + (info == null ? 0 : info.Height + 3);
			}
			else
			{
				fullArea.Height = Math.Max(legendHeight, imageArea.Height + (info == null ? 0 : info.Height + 3));
			}

			return info;
		}
	}

	public class Area
	{
		public int Width { get; set; }

		public int Height { get; set; }

		public Area(int width, int height)
		{
			this.Width = width;
			this.Height = height;
		}
	}

	public static class Extensions
	{
		public static void DrawVerticalLine(this Graphics g, int x0, int y0, int x1, int y1)
		{
			g.DrawLine(new Pen(Color.Gray, 1f), x0 - 1, y0, x1 - 1, y1);
			g.DrawLine(new Pen(Color.White, 1f), x0, y0, x1, y1);
			g.DrawLine(new Pen(Color.Gray, 1f), x0 + 1, y0, x1 + 1, y1);
		}

		public static void DrawHorizontalLine(this Graphics g, int x0, int y0, int x1, int y1)
		{
			g.DrawLine(new Pen(Color.Gray, 1f), x0, y0 - 1, x1, y1 - 1);
			g.DrawLine(new Pen(Color.White, 1f), x0, y0, x1, y1);
			g.DrawLine(new Pen(Color.Gray, 1f), x0, y0 + 1, x1, y1 + 1);
		}

		public static void DrawLegendLine(this Graphics g, Font font, IconType type, ref int position)
		{
			Image image = Image.GetImageFromResources(type.GetImageFile());

			g.DrawImage(image.DrawingImage, 10 + (9 - (image.Width / 2)), position + (7 - (image.Height / 2)));
			g.DrawString(type.GetDescription(), font, new SolidBrush(Color.White), 30, position);

			position += 20;
		}

		public static void DrawBoarders(this Graphics g, Area fullArea, int imageHeight, int legendHeight, bool hasInfo)
		{
			if (imageHeight > legendHeight)
			{
				if (hasInfo)
				{
					g.DrawHorizontalLine(0, imageHeight + 1, fullArea.Width, imageHeight + 1);
				}

				g.DrawVerticalLine(148, 0, 148, imageHeight);
			}
			else
			{
				g.DrawVerticalLine(148, 0, 148, fullArea.Height);

				if (hasInfo)
				{
					g.DrawHorizontalLine(149, imageHeight + 1, fullArea.Width, imageHeight + 1);
				}
			}
		}
	}
}