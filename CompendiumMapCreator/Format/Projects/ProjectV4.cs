using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Projects
{
	public class ProjectV4 : Project
	{
		public ProjectV4(Image image)
			: base(image)
		{
		}

		public ProjectV4(string file, string title)
			: base(file)
		{
			this.Title = title;
		}

		protected override Element ReadElement(BinaryReader reader)
		{
			IconType? type = this.ReadType(reader.ReadInt32());

			if (type == null)
			{
				return null;
			}

			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			bool isCopy = reader.ReadBoolean();

			int length = reader.ReadInt32();

			Element element;

			switch (type)
			{
				case IconType.Label:
					{
						int number = reader.ReadInt32();
						string text = reader.ReadString();

						element = new Label(text?.Length == 0 ? null : text, number)
						{
							IsCopy = isCopy,
						};
					}

					break;

				case IconType.Portal:
					{
						int number = reader.ReadInt32();

						element = new Portal(number)
						{
							IsCopy = isCopy,
						};
					}

					break;

				case IconType.Trap:
					{
						int width = reader.ReadInt32();
						int height = reader.ReadInt32();

						element = new Trap(width, height);
					}

					break;

				case IconType.CollapsibleFloor:
					{
						int width = reader.ReadInt32();
						int height = reader.ReadInt32();

						element = new CollapsibleFloor(width, height);
					}

					break;

				case IconType.Entrance:
					{
						Rotation rotation = (Rotation)reader.ReadInt32();

						element = new Entrance(rotation);
					}

					break;

				default:
					reader.BaseStream.Seek(length, SeekOrigin.Current);

					element = new Element(type.Value);
					break;
			}

			element.X = x;
			element.Y = y;

			return element;
		}

		private IconType? ReadType(int value)
		{
			if (value < (int)IconType.Max && value >= 0)
			{
				return (IconType)value;
			}
			else
			{
				return null;
			}
		}
	}
}