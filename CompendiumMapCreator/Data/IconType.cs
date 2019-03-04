using System;
using System.Reflection;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator
{
	public enum IconType
	{
		[Name("Cursor")]
		[IconFile("cursor.png")]
		Cursor = 0,

		[Name("Normal Chest")]
		[IconFile("normalChest.png")]
		NormalChest = 1,

		[Name("Locked Chest")]
		[IconFile("lockedChest.png")]
		LockedChest = 2,

		[Name("Rare Chest")]
		[IconFile("rareChest.png")]
		RareChest = 3,

		[Name("Trapped Chest")]
		[IconFile("trappedChest.png")]
		TrappedChest = 4,

		[Name("Collectible")]
		[IconFile("collectible.png")]
		Collectible = 5,

		[Name("Any Collectible")]
		[IconFile("anyCollectible.png")]
		[ToolTip("Adventurer's Pack, Rubble")]
		AnyCollectible = 6,

		[Name("Lore Collectible")]
		[IconFile("lore.png")]
		[ToolTip("Some Bookshelf, Some Cabinet")]
		Lore = 7,

		[Name("Natural Collectible")]
		[IconFile("natural.png")]
		[ToolTip("Bones, Fungus, Moss, Mushroom, Some Crude Altar")]
		Natural = 8,

		[Name("Arcane Collectible")]
		[IconFile("arcane.png")]
		[ToolTip("Alchemy Table, Scroll Rack, Some Bookshelf, Some Cabinet, Some Crude Altar")]
		Arcane = 9,

		[Name("Plant Collectible")]
		[IconFile("plant.png")]
		[ToolTip("Flowering Plant")]
		Plant = 10,

		[Name("Door")]
		[IconFile("door.png")]
		Door = 11,

		[Name("Locked Door")]
		[IconFile("lockedDoor.png")]
		LockedDoor = 12,

		[Name("Blocked Door")]
		[IconFile("blockedDoor.png")]
		BlockedDoor = 13,

		[Name("Secret Door")]
		[IconFile("secretDoor.png")]
		SecretDoor = 14,

		[Name("Progress Door")]
		[IconFile("progressDoor.png")]
		ProgressDoor = 15,

		[Name("Trap")]
		[IconFile("trap.png")]
		Trap = 16,

		[Name("Trap Box")]
		[IconFile("controlBox.png")]
		TrapBox = 17,

		[Name("Collapsible Floor")]
		[IconFile("collapsibleFloor.png")]
		CollapsibleFloor = 18,

		[Name("Drop")]
		[IconFile("drop.png")]
		Drop = 19,

		[Name("Alarm")]
		[IconFile("alarm.png")]
		Alarm = 20,

		[Name("Disabler")]
		[IconFile("disabler.png")]
		Disabler = 21,

		[Name("Opener")]
		[IconFile("leverValve.png")]
		Opener = 22,

		[Name("Lever")]
		[IconFile("lever.png")]
		Lever = 23,

		[Name("Valve")]
		[IconFile("valve.png")]
		Valve = 24,

		[Name("Rune")]
		[IconFile("rune.png")]
		Rune = 25,

		[Name("Label")]
		[IconFile("label.png")]
		Label = 26,

		[Name("Quest Item")]
		[IconFile("questItem.png")]
		QuestItem = 27,

		[Name("Quest Item Used Here")]
		[IconFile("questItemUse.png")]
		QuestItemUse = 28,

		[Name("Quest NPC")]
		[IconFile("questNPC.png")]
		QuestNPC = 29,

		[Name("NPC")]
		[IconFile("questNPCNeutral.png")]
		NPC = 30,

		[Name("Entrance")]
		[IconFile("entrance.png")]
		Entrance = 31,

		[Name("Quest Exit")]
		[IconFile("questExit.png")]
		QuestExit = 32,

		[Name("Portal")]
		[IconFile("portal.png")]
		Portal = 33,

		[Name("Shrine")]
		[IconFile("shrine.png")]
		Shrine = 34,
	}

	public static class AttributeExtensions
	{
		public static bool TryGetCustomAttribute<T>(this TypeInfo info, out T attribute)
			where T : Attribute
		{
			T[] attributes = (T[])info.GetCustomAttributes<T>();

			if (attributes.Length > 0)
			{
				attribute = attributes[0];
				return true;
			}

			attribute = default(T);
			return false;
		}

		public static bool TryGetCustomAttribute<T>(this MemberInfo info, out T attribute)
			where T : Attribute
		{
			T[] attributes = (T[])info?.GetCustomAttributes<T>();

			if (attributes?.Length > 0)
			{
				attribute = attributes[0];
				return true;
			}

			attribute = default(T);
			return false;
		}
	}

	public static class IconTypeExtensions
	{
		public static IconType FromDescription(this string s)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo[] fields = typeInfo.GetFields();

			for (int i = 0; i < fields.Length; i++)
			{
				NameAttribute attribute = null;
				if ((fields[i]?.TryGetCustomAttribute(out attribute) ?? false) && attribute.Name == s)
				{
					return (IconType)fields[i].GetValue(null);
				}
			}

			throw new ArgumentOutOfRangeException(nameof(s));
		}

		public static string GetDescription(this IconType item)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo field = typeInfo.GetField(item.ToString());

			NameAttribute attribute = null;
			if (field?.TryGetCustomAttribute(out attribute) ?? false)
			{
				return attribute.Name;
			}

			throw new ArgumentOutOfRangeException(nameof(item));
		}

		public static Image GetImage(this IconType item)
		{
			return Image.GetImageFromResources(item.GetImageFile());
		}

		public static string GetImageFile(this IconType item)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo field = typeInfo.GetField(item.ToString());

			IconFileAttribute attribute = null;
			if (field?.TryGetCustomAttribute(out attribute) ?? false)
			{
				return "Icons/" + attribute.FileName;
			}

			throw new ArgumentOutOfRangeException(nameof(item));
		}

		public static string GetToolTip(this IconType item)
		{
			TypeInfo typeInfo = typeof(IconType).GetTypeInfo();

			FieldInfo field = typeInfo.GetField(item.ToString());

			ToolTipAttribute attribute = null;
			if (field?.TryGetCustomAttribute(out attribute) ?? false)
			{
				return attribute.ToolTip;
			}

			return null;
		}
	}
}