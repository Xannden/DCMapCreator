using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CompendiumMapCreator.Data;
using Microsoft.Win32;
using DImage = System.Drawing.Image;

namespace CompendiumMapCreator.ViewModel
{
	public class MainWindow : INotifyPropertyChanged
	{
		private IconType iconType;
		private Image background;
		private string projectFile = string.Empty;

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
				this.background = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.BackgroundImage)));
			}
		}

		public bool AddLegend { get; set; } = true;

		public string ProjectFile
		{
			get => this.projectFile;
			set
			{
				this.projectFile = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ProjectFile)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public UndoRedoStack<Element> Elements { get; set; } = new UndoRedoStack<Element>();

		public bool LoadImage()
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
				this.IconType = IconType.None;
				this.ProjectFile = string.Empty;
				this.Elements.Clear();
				return true;
			}

			return false;
		}

		public void AddElement(Element element)
		{
			if (this.BackgroundImage == null)
			{
				return;
			}

			this.Elements.Add(element);
		}

		public void Window_KeyDown(KeyEventArgs e)
		{
			if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
			{
				if (e.Key == Key.Z && this.Elements.Count > 0)
				{
					this.Elements.Undo();
				}
				else if (e.Key == Key.Y)
				{
					this.Elements.Redo();
				}
				else if (e.Key == Key.S)
				{
					this.SaveProject();
				}
			}
			else if (Keyboard.IsKeyDown(Key.LeftShift))
			{
				if (e.Key == Key.D1)
				{
					this.IconType = IconType.Portal1;
				}
				else if (e.Key == Key.D2)
				{
					this.IconType = IconType.Portal2;
				}
				else if (e.Key == Key.D3)
				{
					this.IconType = IconType.Portal3;
				}
				else if (e.Key == Key.D4)
				{
					this.IconType = IconType.Portal4;
				}
			}
			else if (e.Key == Key.D1)
			{
				this.IconType = IconType.NormalChest;
			}
			else if (e.Key == Key.D2)
			{
				this.IconType = IconType.TrappedChest;
			}
			else if (e.Key == Key.D3)
			{
				this.IconType = IconType.LockedChest;
			}
			else if (e.Key == Key.D4)
			{
				this.IconType = IconType.LockedDoor;
			}
			else if (e.Key == Key.D5)
			{
				this.IconType = IconType.LeverValveRune;
			}
			else if (e.Key == Key.D6)
			{
				this.IconType = IconType.ControlBox;
			}
			else if (e.Key == Key.D7)
			{
				this.IconType = IconType.Collectible;
			}
			else if (e.Key == Key.D8)
			{
				this.IconType = IconType.QuestItem;
			}
			else if (e.Key == Key.D9)
			{
				this.IconType = IconType.QuestNPC;
			}
			else if (e.Key == Key.D0)
			{
				this.IconType = IconType.SecretDoor;
			}
			else if (e.Key == Key.OemMinus)
			{
				this.IconType = IconType.QuestExit;
			}
		}

		public void SaveImage()
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
				int position = this.AddLegend ? 150 : 0;
				using (DImage image = new Bitmap(this.BackgroundImage.Width + position, this.BackgroundImage.Height, PixelFormat.Format32bppArgb))
				{
					using (Graphics g = Graphics.FromImage(image))
					{
						if (this.AddLegend)
						{
							this.DrawLegend(g, image);
						}

						g.DrawImage(this.BackgroundImage.DrawingImage, position, 0);

						for (int i = 0; i < this.Elements.Count; i++)
						{
							using (DImage element = DImage.FromStream(Application.GetResourceStream(this.Elements[i].Image.UriSource).Stream))
							{
								g.DrawImage(element, this.Elements[i].X + position, (float)this.Elements[i].Y);
							}
						}
					}

					image.Save(dialog.FileName, ImageFormat.Png);
				}
			}
		}

		public void SaveProject()
		{
			if (this.BackgroundImage == null)
			{
				return;
			}

			bool result = this.ProjectFile != null;

			if (this.ProjectFile == string.Empty)
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
				using (BinaryWriter writer = new BinaryWriter(File.Create(this.ProjectFile)))
				{
					writer.Write("DMC".ToCharArray());
					writer.Write(this.BackgroundImage.Data.Length);
					writer.Write(this.background.Data.GetBuffer());
					writer.Write(this.Elements.Total);
					writer.Write(this.Elements.Count);

					foreach (Element item in this.Elements.Data)
					{
						writer.Write(item.X);
						writer.Write(item.Y);
						writer.Write((int)item.Type);
					}
				}
			}
		}

		public void LoadProject()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				DefaultExt = ".dmc",
				Filter = "Project (*.dmc)|*.dmc",
			};

			bool? result = dialog.ShowDialog();

			if (result.HasValue && result == true)
			{
				this.ProjectFile = dialog.FileName;
				this.IconType = IconType.None;

				using (BinaryReader reader = new BinaryReader(File.OpenRead(dialog.FileName)))
				{
					char[] check = reader.ReadChars(3);

					if (check[0] != 'D' || check[1] != 'M' || check[2] != 'C')
					{
						return;
					}

					long length = reader.ReadInt64();
					byte[] data = reader.ReadBytes((int)length);

					this.BackgroundImage = new Image(data);

					int total = reader.ReadInt32();
					int count = reader.ReadInt32();
					this.Elements.Clear();

					for (int i = 0; i < total; i++)
					{
						this.Elements.Add(new Element(reader.ReadInt32(), reader.ReadInt32(), (IconType)reader.ReadInt32()));
					}

					this.Elements.SetCount(count);
				}
			}
		}

		private void DrawLegend(Graphics g, DImage image)
		{
			g.FillRectangle(Brushes.Black, 0, 0, 150, image.Height);
			g.DrawLine(new Pen(Color.Gray, 2f), 148, 0, 148, image.Height);
			g.DrawLine(new Pen(Color.White, 2f), 149, 0, 149, image.Height);
			g.DrawLine(new Pen(Color.Gray, 2f), 150, 0, 150, image.Height);

			using (Font font = new Font(new FontFamily(System.Drawing.Text.GenericFontFamilies.SansSerif), 8))
			{
				g.DrawImage(DImage.FromStream(this.GetImageStream("Icons/entrance.png")), 10, 0);
				g.DrawString("Dungeon Entrance", font, new SolidBrush(Color.White), 30, 0);

				int position = 20;

				this.DrawLegendLine(g, font, IconType.NormalChest, ref position);
				this.DrawLegendLine(g, font, IconType.TrappedChest, ref position);
				this.DrawLegendLine(g, font, IconType.LockedChest, ref position);
				this.DrawLegendLine(g, font, IconType.LockedDoor, ref position);
				this.DrawLegendLine(g, font, IconType.LeverValveRune, ref position);
				this.DrawLegendLine(g, font, IconType.ControlBox, ref position);
				this.DrawLegendLine(g, font, IconType.Collectible, ref position);
				this.DrawLegendLine(g, font, IconType.QuestItem, ref position);
				this.DrawLegendLine(g, font, IconType.QuestNPC, ref position);
				this.DrawLegendLine(g, font, IconType.SecretDoor, ref position);
				this.DrawLegendLine(g, font, IconType.QuestExit, ref position);
				this.DrawLegendLine(g, font, IconType.Portal1, ref position);
				this.DrawLegendLine(g, font, IconType.Portal2, ref position);
				this.DrawLegendLine(g, font, IconType.Portal3, ref position);
				this.DrawLegendLine(g, font, IconType.Portal4, ref position);
			}
		}

		private void DrawLegendLine(Graphics g, Font font, IconType type, ref int position)
		{
			if (this.Elements.Any(e => e.Type == type))
			{
				using (DImage image = DImage.FromStream(this.GetImageStream(type.GetFile())))
				{
					g.DrawImage(image, 10 + (9 - (image.Width / 2)), position + (7 - (image.Height / 2)));
					g.DrawString(type.GetDescription(), font, new SolidBrush(Color.White), 30, position);
				}

				position += 20;
			}
		}

		private Stream GetImageStream(string fileName) => Application.GetResourceStream(new Uri("pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + fileName, UriKind.Absolute)).Stream;
	}
}