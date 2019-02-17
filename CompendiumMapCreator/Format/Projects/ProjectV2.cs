using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format
{
	public class ProjectV2 : Project
	{
		public ProjectV2(Image image) : base(image)
		{
		}

		public ProjectV2(string file) : base(file)
		{
		}

		protected override Element ReadElement(BinaryReader reader, IconType? type)
		{
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			bool isCopy = reader.ReadBoolean();

			Element element;

			switch (type.Value)
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
					element = new Element(type.Value);
					break;
			}

			element.X = x;
			element.Y = y;
			element.IsCopy = isCopy;

			return element;
		}

		protected override IconType? ReadType(int value)
		{
			switch (value)
			{
				case 0:
					return IconType.Cursor;

				case 1:
					return IconType.NormalChest;

				case 2:
					return IconType.TrappedChest;

				case 3:
					return IconType.LockedChest;

				case 4:
					return IconType.LockedDoor;

				case 5:
					return IconType.LeverValveRune;

				case 6:
					return IconType.ControlBox;

				case 7:
					return IconType.Collectible;

				case 8:
					return IconType.Lore;

				case 9:
					return IconType.Natural;

				case 10:
					return IconType.Arcane;

				case 11:
					return IconType.QuestItem;

				case 12:
					return IconType.QuestNPC;

				case 13:
					return IconType.SecretDoor;

				case 14:
					return IconType.QuestExit;

				case 15:
					return IconType.Portal;

				case 16:
					return IconType.Label;

				case 17:
					return IconType.Trap;

				case 18:
					return IconType.CollapsibleFloor;

				case 19:
					return IconType.Entrance;

				default:
					throw new InvalidDataException();
			}
		}
	}
}