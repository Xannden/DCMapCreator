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

namespace CompendiumMapCreator.ViewModel
{
	public class MainWindow : INotifyPropertyChanged
	{
		private IconType iconType;
		private Image background;
		private string projectFile = string.Empty;
		private readonly UndoRedoStack<Edit> edits = new UndoRedoStack<Edit>();
		private Element selected;
		private RelayCommand undoCommand;
		private RelayCommand redoCommand;
		private RelayCommand deleteCommand;

		public IconType IconType
		{
			get => this.iconType;
			set
			{
				this.iconType = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IconType)));
			}
		}

		public Image BackgroundImage
		{
			get => this.background;
			set
			{
				this.background?.Dispose();
				this.background = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.BackgroundImage)));
			}
		}

		public bool AddLegend { get; set; } = true;

		public string Title => $"DDO Compendium Map Creator{(string.IsNullOrEmpty(this.projectFile) ? "" : $" - {this.projectFile}")}";

		public string ProjectFile
		{
			get => this.projectFile;
			set
			{
				this.projectFile = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ProjectFile)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ObservableCollection<Element> Elements { get; set; } = new ObservableCollection<Element>();

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

		public DelegateCommand<IconType> SelectType => new DelegateCommand<IconType>(type => this.IconType = type);

#pragma warning disable RCS1171 // Simplify lazy initialization.
		public RelayCommand UndoCommand
		{
			get
			{
				if (this.undoCommand == null)
				{
					this.undoCommand = new RelayCommand(_ => this.Undo(), _ => this.edits.Count > 0);
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
					this.redoCommand = new RelayCommand(_ => this.Redo(), _ => this.edits.Count < this.edits.Total);
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

		public RelayCommand SaveProjectCommand => new RelayCommand(this.SaveProject);

		public RelayCommand LoadProjectCommand => new RelayCommand(this.LoadProject);

		public RelayCommand ExportCommand => new RelayCommand(this.SaveImage);

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
				Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg",
			};

			bool? result = dialog.ShowDialog();

			if (result.HasValue && result == true)
			{
				this.BackgroundImage = new Image(dialog.FileName);
				this.IconType = IconType.Cursor;
				this.ProjectFile = string.Empty;
				this.Elements.Clear();
			}
		}

		public void AddElement(Element element)
		{
			if (this.BackgroundImage == null)
			{
				return;
			}

			this.AddEdit(new Add(element));
		}

		public void SaveImage(object parameter)
		{
			if (this.BackgroundImage == null)
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

					if (this.AddLegend)
					{
						legend = this.CreateLegend(font);
					}

					int width = this.BackgroundImage.Width + (legend != null ? legend.Width + 3 : 0);

					DImage info;
					int height;

					if (this.BackgroundImage.Height > (legend?.Height ?? 0))
					{
						info = this.CreateInfoList(width, font);
						height = this.BackgroundImage.Height + (info == null ? 0 : info.Height + 3);
					}
					else
					{
						info = this.CreateInfoList(this.BackgroundImage.Width, font);
						height = Math.Max(legend.Height, this.BackgroundImage.Height + (info == null ? 0 : info.Height + 3));
					}

					using (DImage image = new Bitmap(width, height, PixelFormat.Format32bppArgb))
					{
						using (Graphics g = Graphics.FromImage(image))
						{
							g.FillRectangle(Brushes.Black, 0, 0, image.Width, image.Height);

							if (this.BackgroundImage.Height > (legend?.Height ?? 0))
							{
								if (info != null)
								{
									this.DrawHorizontalLine(g, 0, this.BackgroundImage.Height + 1, image.Width, this.BackgroundImage.Height + 1);
								}

								this.DrawVerticalLine(g, 149, 0, 149, this.BackgroundImage.Height);
							}
							else
							{
								this.DrawVerticalLine(g, 149, 0, 149, image.Height);

								if (info != null)
								{
									this.DrawHorizontalLine(g, 150, this.BackgroundImage.Height + 1, image.Width, this.BackgroundImage.Height + 1);
								}
							}

							int offset = legend != null ? 151 : 0;

							g.DrawImage(this.BackgroundImage.DrawingImage, offset, 0);

							for (int i = 0; i < this.Elements.Count; i++)
							{
								g.DrawImage(this.Elements[i].Image.DrawingImage, this.Elements[i].X + offset, (float)this.Elements[i].Y);
							}

							if (info != null)
							{
								if (this.BackgroundImage.Height > (legend?.Height ?? 0))
								{
									g.DrawImage(info, 0, this.BackgroundImage.Height + 3);
								}
								else
								{
									g.DrawImage(info, offset, this.BackgroundImage.Height + 3);
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

		public void SaveProject(object parameter)
		{
			if (this.BackgroundImage == null)
			{
				return;
			}

			bool result = this.ProjectFile != null;

			if (this.ProjectFile?.Length == 0)
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
					this.ProjectFile = dialog.FileName;
				}
			}

			if (result)
			{
				try
				{
					using (BinaryWriter writer = new BinaryWriter(File.Create(this.ProjectFile)))
					{
						writer.Write(407893541);
						writer.Write("DMC".ToCharArray());
						writer.Write(1);
						writer.Write(this.BackgroundImage.Data.Length);
						writer.Write(this.background.Data.GetBuffer());
						writer.Write(this.Elements.Count);

						foreach (Element item in this.Elements)
						{
							writer.Write((int)item.Type);
							writer.Write(item.X);
							writer.Write(item.Y);

							if (item.Type == IconType.Label)
							{
								Label l = (Label)item;

								writer.Write(l.Number);
								writer.Write(l.Text);
							}
							else if (item.Type == IconType.Portal)
							{
								Portal p = (Portal)item;

								writer.Write(p.Number);
							}
						}
					}
				}
				catch (Exception)
				{
					MessageBox.Show("Unable to save file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		public void LoadProject(object parameter)
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
					using (BinaryReader reader = new BinaryReader(File.OpenRead(dialog.FileName)))
					{
						int magic = reader.ReadInt32();

						if (magic != 407893541)
						{
							throw new InvalidOperationException();
						}

						char[] check = reader.ReadChars(3);

						if (check[0] != 'D' || check[1] != 'M' || check[2] != 'C')
						{
							throw new InvalidOperationException();
						}

						int version = reader.ReadInt32();

						if (version != 1)
						{
							throw new InvalidOperationException();
						}

						long length = reader.ReadInt64();
						byte[] data = reader.ReadBytes((int)length);

						this.BackgroundImage = new Image(data);

						int count = reader.ReadInt32();
						this.Elements.Clear();

						for (int i = 0; i < count; i++)
						{
							this.ReadElement(reader);
						}

						this.edits.Clear();
					}

					this.ProjectFile = dialog.FileName;
					this.IconType = IconType.Cursor;
				}
				catch (Exception)
				{
					MessageBox.Show("Unable to load file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		public Element Select(ImagePoint point)
		{
			for (int i = this.Elements.Count - 1; i >= 0; i--)
			{
				if (this.Elements[i].Contains(point))
				{
					this.Selected = this.Elements[i];

					this.Elements.Move(i, this.Elements.Count - 1);

					return this.Selected;
				}
			}

			this.Selected = null;

			return this.Selected;
		}

		public Element CreateElement()
		{
			if (this.IconType == IconType.Label)
			{
				return new Label("", this.Elements.Count((e) => e is Label));
			}
			else if (this.IconType == IconType.Portal)
			{
				return new Portal(this.Elements.Count((e) => e is Portal));
			}
			else
			{
				return new Element(this.IconType);
			}
		}

		public void Copy(ImagePoint? mousePosition, Element element) => this.AddEdit(new Copy(element, mousePosition));

		public void AddEdit(Edit edit, bool apply = true)
		{
			if (apply)
			{
				this.edits.Add(edit, this.Elements);
			}
			else
			{
				this.edits.Add(edit);
			}

			this.undoCommand?.RaiseCanExecuteChanged();
			this.redoCommand?.RaiseCanExecuteChanged();
		}

		public void Undo()
		{
			this.edits.Undo(this.Elements);

			this.undoCommand?.RaiseCanExecuteChanged();
			this.redoCommand?.RaiseCanExecuteChanged();
		}

		public void Redo()
		{
			this.edits.Redo(this.Elements);

			this.undoCommand?.RaiseCanExecuteChanged();
			this.redoCommand?.RaiseCanExecuteChanged();
		}

		private void ReadElement(BinaryReader reader)
		{
			int value = reader.ReadInt32();

			if (value > (int)IconType.Label)
			{
				throw null;
			}

			IconType type = (IconType)value;
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();

			Element element;

			switch (type)
			{
				case IconType.Label:
					{
						int number = reader.ReadInt32();
						string text = reader.ReadString();

						element = new Label(text, number);
					}
					break;

				case IconType.Portal:
					{
						int number = reader.ReadInt32();

						element = new Portal(number);
					}
					break;

				default:
					element = new Element(type);
					break;
			}

			element.X = x;
			element.Y = y;

			this.Elements.Add(element);
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
					this.DrawLegendLine(g, font, item, ref position);
				}
			}

			return image;
		}

		private void DrawLegendLine(Graphics g, Font font, IconType type, ref int position)
		{
			Image image = Image.GetImageFromResources(type.GetImageFile());

			g.DrawImage(image.DrawingImage, 10 + (9 - (image.Width / 2)), position + (7 - (image.Height / 2)));
			g.DrawString(type.GetDescription(), font, new SolidBrush(Color.White), 30, position);

			position += 20;
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

		private void DrawVerticalLine(Graphics g, int x0, int y0, int x1, int y1)
		{
			g.DrawLine(new Pen(Color.Gray, 1f), x0 - 1, y0, x1 - 1, y1);
			g.DrawLine(new Pen(Color.White, 1f), x0, y0, x1, y1);
			g.DrawLine(new Pen(Color.Gray, 1f), x0 + 1, y0, x1 + 1, y1);
		}

		private void DrawHorizontalLine(Graphics g, int x0, int y0, int x1, int y1)
		{
			g.DrawLine(new Pen(Color.Gray, 1f), x0, y0 - 1, x1, y1 - 1);
			g.DrawLine(new Pen(Color.White, 1f), x0, y0, x1, y1);
			g.DrawLine(new Pen(Color.Gray, 1f), x0, y0 + 1, x1, y1 + 1);
		}
	}
}