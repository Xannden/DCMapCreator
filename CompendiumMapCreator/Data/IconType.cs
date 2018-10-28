using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	public enum IconType
	{
		None,
		NormalChest,
		TrappedChest,
		LockedChest,
		LockedDoor,
		LeverValveRune,
		ControlBox,
		Collectible,
		QuestItem,
		QuestNPC,
		SecretDoor,
		QuestExit,
		Portal1,
		Portal2,
		Portal3,
		Portal4,
	}

	public static class IconTypeExtensions
	{
		public static string GetDescription(this IconType type)
		{
			switch (type)
			{
				case IconType.NormalChest:
					return "Normal Chest";

				case IconType.TrappedChest:
					return "Trapped Chest";

				case IconType.LockedChest:
					return "Locked Chest";

				case IconType.LockedDoor:
					return "Locked Door";

				case IconType.LeverValveRune:
					return "Lever/Valve/Rune";

				case IconType.ControlBox:
					return "Control Box";

				case IconType.Collectible:
					return "Collectible";

				case IconType.QuestItem:
					return "Quest Item";

				case IconType.QuestNPC:
					return "Quest NPC";

				case IconType.SecretDoor:
					return "Secret Door";

				case IconType.QuestExit:
					return "Quest Exit";

				case IconType.Portal1:
					return "Waypoint/Portal 1";

				case IconType.Portal2:
					return "Waypoint/Portal 2";

				case IconType.Portal3:
					return "Waypoint/Portal 3";

				case IconType.Portal4:
					return "Waypoint/Portal 4";

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static string GetFile(this IconType type)
		{
			switch (type)
			{
				case IconType.NormalChest:
					return "Icons/normalChest.png";

				case IconType.TrappedChest:
					return "Icons/trappedChest.png";

				case IconType.LockedChest:
					return "Icons/lockedChest.png";

				case IconType.LockedDoor:
					return "Icons/lockedDoor.png";

				case IconType.LeverValveRune:
					return "Icons/leverValve.png";

				case IconType.ControlBox:
					return "Icons/controlBox.png";

				case IconType.Collectible:
					return "Icons/collectible.png";

				case IconType.QuestItem:
					return "Icons/questItem.png";

				case IconType.QuestNPC:
					return "Icons/questNPC.png";

				case IconType.SecretDoor:
					return "Icons/secretDoor.png";

				case IconType.QuestExit:
					return "Icons/questExit.png";

				case IconType.Portal1:
					return "Icons/portal1.png";

				case IconType.Portal2:
					return "Icons/portal2.png";

				case IconType.Portal3:
					return "Icons/portal3.png";

				case IconType.Portal4:
					return "Icons/portal4.png";

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static BitmapImage GetImage(this IconType type) => new BitmapImage(new Uri("pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + type.GetFile(), UriKind.Absolute));
	}
}