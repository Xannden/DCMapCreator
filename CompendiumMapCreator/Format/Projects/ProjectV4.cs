using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format
{
	public class ProjectV4 : Project
	{
		public ProjectV4(Image image) : base(image)
		{
		}

		public ProjectV4(string file, string title) : base(file)
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
							IsCopy = isCopy
						};
					}
					break;

				case IconType.Portal:
					{
						int number = reader.ReadInt32();

						element = new Portal(number)
						{
							IsCopy = isCopy
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
			switch (value)
			{
				case 0:
					return IconType.Cursor;

				case 1:
					return IconType.NormalChest;

				case 2:
					return IconType.LockedChest;

				case 3:
					return IconType.RareChest;

				case 4:
					return IconType.TrappedChest;

				case 5:
					return IconType.Collectible;

				case 6:
					return IconType.AnyCollectible;

				case 7:
					return IconType.Lore;

				case 8:
					return IconType.Natural;

				case 9:
					return IconType.Arcane;

				case 10:
					return IconType.Plant;

				case 11:
					return IconType.Door;

				case 12:
					return IconType.LockedDoor;

				case 13:
					return IconType.BlockedDoor;

				case 14:
					return IconType.SecretDoor;

				case 15:
					return IconType.ProgressDoor;

				case 16:
					return IconType.Trap;

				case 17:
					return IconType.TrapBox;

				case 18:
					return IconType.CollapsibleFloor;

				case 19:
					return IconType.Drop;

				case 20:
					return IconType.Alarm;

				case 21:
					return IconType.Disabler;

				case 22:
					return IconType.Opener;

				case 23:
					return IconType.Lever;

				case 24:
					return IconType.Valve;

				case 25:
					return IconType.Rune;

				case 26:
					return IconType.Label;

				case 27:
					return IconType.QuestItem;

				case 28:
					return IconType.QuestItemUse;

				case 29:
					return IconType.QuestNPC;

				case 30:
					return IconType.NPC;

				case 31:
					return IconType.Entrance;

				case 32:
					return IconType.QuestExit;

				case 33:
					return IconType.Portal;

				case 34:
					return IconType.Shrine;

				default:
					return null;
			}
		}
	}
}