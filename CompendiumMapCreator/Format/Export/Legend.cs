using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CompendiumMapCreator.Format.Export
{
	public class Legend : IDrawable
	{
		private const int ImageCenterX = ImageWidth / 2;
		private const int ImageCenterY = ImageHeight / 2;
		private const int ImageHeight = 14;
		private const int ImageWidth = 18;
		private const int LineHeight = 20;
		private const int TextX = XOffset + ImageWidth + 2;
		private const int XOffset = 10;
		private readonly bool addLegend;
		private readonly IList<Element> elements;
		private readonly Font font;
		private bool hasPossible;
		private Size size;
		private List<IconType> types;

		public Legend(Font font, IList<Element> elements, bool addLegend)
		{
			this.font = font;
			this.elements = elements;
			this.addLegend = addLegend;
			this.hasPossible = false;
		}

		public void Draw(Graphics g, Point p)
		{
			if (!this.addLegend)
			{
				return;
			}

			int x = p.X + XOffset;
			int y = p.Y;

			for (int i = 0; i < this.types.Count; i++)
			{
				Image image = this.types[i].GetImage();

				g.DrawImage(image.DrawingImage, x + (ImageCenterX - (image.Width / 2)), y + (ImageCenterY - (image.Height / 2)));

				if (this.types[i] == IconType.Entrance)
				{
					g.DrawString("Dungeon Entrance", this.font, Brushes.White, TextX, y);
				}
				else
				{
					g.DrawString(this.types[i].GetName(), this.font, Brushes.White, TextX, y);
				}

				y += LineHeight;
			}

			if (this.hasPossible)
			{
				Image image = Image.GetImageFromResources("Icons/possLoc.png");

				g.DrawImage(image.DrawingImage, x + (ImageCenterX - (image.Width / 2)), y + (ImageCenterY - (image.Height / 2)));
				g.DrawString("Possible Location", this.font, Brushes.White, TextX, y);
			}

			g.DrawVerticalLine(p.X + this.size.Width - 2, p.Y - 1, p.Y + this.size.Height + 1);
		}

		public Size Layout(int width, int height)
		{
			if (!this.addLegend)
			{
				return default;
			}

			this.types = this.elements.Select(e => e.Type).Where(t => t != IconType.Entrance && t != IconType.Label && t != IconType.Shrine).Distinct().ToList();

			this.types.Sort();

			this.types.Insert(0, IconType.Entrance);

			if (this.elements.Any((e) => e.IsOptional))
			{
				this.hasPossible = true;
			}

			this.size = new Size(150, (this.types.Count * LineHeight) + (this.hasPossible ? LineHeight : 0));

			return this.size;
		}
	}
}