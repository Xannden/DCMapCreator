using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public abstract class Project
	{
		public static Project Load()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				DefaultExt = ".dmc",
				Filter = "Project (*.dmc)|*.dmc",
			};

			bool? result = dialog.ShowDialog();

			if (result.HasValue && result == true)
			{
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

		public static Project FromImage(Image image)
		{
			return new ProjectV2(image);
		}

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
			get; protected set;
		}

		public ObservableCollection<Element> Elements
		{
			get; protected set;
		}

		public UndoRedoStack<Edit> Edits { get; } = new UndoRedoStack<Edit>();

		public void Save()
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

				result = dialog.ShowDialog() ?? false;

				if (result)
				{
					this.File = dialog.FileName;
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
						writer.Write(2);
						writer.Write(this.Image.Data.Length);
						writer.Write(this.Image.Data.GetBuffer());
						writer.Write(this.Elements.Count);

						foreach (Element item in this.Elements)
						{
							writer.Write((int)item.Type);
							this.WriteElement(writer, item);
						}
					}
				}
				catch (Exception)
				{
					MessageBox.Show("Unable to save file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		public void Export(bool addLegend)
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

			bool? result = dialog.ShowDialog();

			if (result.HasValue && result == true)
			{
				using (Font font = new Font(new FontFamily(GenericFontFamilies.SansSerif), 8))
				{
					DImage legend = null;

					if (addLegend)
					{
						legend = this.CreateLegend(font);
					}

					int width = this.Image.Width + (legend != null ? legend.Width + 3 : 0);

					DImage info;
					int height;

					if (this.Image.Height > (legend?.Height ?? 0))
					{
						info = this.CreateInfoList(width, font);
						height = this.Image.Height + (info == null ? 0 : info.Height + 3);
					}
					else
					{
						info = this.CreateInfoList(this.Image.Width, font);
						height = Math.Max(legend.Height, this.Image.Height + (info == null ? 0 : info.Height + 3));
					}

					using (DImage image = new Bitmap(width, height, PixelFormat.Format32bppArgb))
					{
						using (Graphics g = Graphics.FromImage(image))
						{
							g.FillRectangle(Brushes.Black, 0, 0, image.Width, image.Height);

							if (this.Image.Height > (legend?.Height ?? 0))
							{
								if (info != null)
								{
									g.DrawHorizontalLine(0, this.Image.Height + 1, image.Width, this.Image.Height + 1);
								}

								g.DrawVerticalLine(149, 0, 149, this.Image.Height);
							}
							else
							{
								g.DrawVerticalLine(149, 0, 149, image.Height);

								if (info != null)
								{
									g.DrawHorizontalLine(150, this.Image.Height + 1, image.Width, this.Image.Height + 1);
								}
							}

							int offset = legend != null ? 151 : 0;

							g.DrawImage(this.Image.DrawingImage, offset, 0);

							for (int i = 0; i < this.Elements.Count; i++)
							{
								g.DrawImage(this.Elements[i].Image.DrawingImage, this.Elements[i].X + offset, (float)this.Elements[i].Y);
							}

							if (info != null)
							{
								if (this.Image.Height > (legend?.Height ?? 0))
								{
									g.DrawImage(info, 0, this.Image.Height + 3);
								}
								else
								{
									g.DrawImage(info, offset, this.Image.Height + 3);
								}
							}

							if (legend != null)
							{
								g.DrawImage(legend, 0, 0);
							}
						}

						image.Save(dialog.FileName, ImageFormat.Png);
					}

					legend?.Dispose();
					info?.Dispose();
				}
			}
		}

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
			}
		}

		private DImage CreateLegend(Font font)
		{
			List<IconType> icons = new List<IconType>();

			foreach (Element item in this.Elements)
			{
				if (item.Type != IconType.Label && !icons.Contains(item.Type))
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

		private DImage CreateInfoList(int width, Font font)
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
					float columnOffset = (i % columns) * columnWidth;

					Image icon = labels[i].Image;

					g.DrawImage(icon.DrawingImage, (int)columnOffset + (9 - (icon.Width / 2)), rowOffset + (7 - (icon.Height / 2)));

					g.DrawString(labels[i].Text, font, Brushes.White, new RectangleF(columnOffset + 16, rowOffset, columnWidth - 16, rowHeights[i / columns]), format);

					if ((i + 1) % columns == 0)
					{
						rowOffset += rowHeights[i / columns];
					}
				}
			}

			return info;
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
	}
}