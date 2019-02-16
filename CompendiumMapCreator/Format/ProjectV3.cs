using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format
{
	public class ProjectV3 : Project
	{
		public ProjectV3(Image image) : base(image)
		{
		}

		public ProjectV3(string file, string title) : base(file)
		{
			this.Title = title;
		}

		protected override Element ReadElement(BinaryReader reader, IconType? type)
		{
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
					reader.BaseStream.Seek(length, SeekOrigin.Current);

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

				case 10:
					return IconType.NormalChest;

				case 20:
					return IconType.TrappedChest;

				case 30:
					return IconType.LockedChest;

				case 40:
					return IconType.LockedDoor;

				case 41:
					return IconType.BlockedDoor;

				case 50:
					return IconType.LeverValveRune;

				case 60:
					return IconType.ControlBox;

				case 70:
					return IconType.Collectible;

				case 80:
					return IconType.Lore;

				case 90:
					return IconType.Natural;

				case 100:
					return IconType.Arcane;

				case 110:
					return IconType.QuestItem;

				case 111:
					return IconType.UseQuestItem;

				case 120:
					return IconType.QuestNPC;

				case 130:
					return IconType.SecretDoor;

				case 140:
					return IconType.QuestExit;

				case 150:
					return IconType.Portal;

				case 160:
					return IconType.Label;

				case 170:
					return IconType.Trap;

				case 180:
					return IconType.CollapsibleFloor;

				case 181:
					return IconType.Drop;

				case 190:
					return IconType.Entrance;

				default:
					return null;
			}
		}
	}
}