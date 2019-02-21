using System;
using System.Reflection;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator
{
	public enum IconType
	{
		[Name("Cursor")]
		[IconFile("cursor.png")]
		Cursor,

		[Name("Normal Chest")]
		[IconFile("normalChest.png")]
		NormalChest,

		[Name("Locked Chest")]
		[IconFile("lockedChest.png")]
		LockedChest,

		[Name("Rare Chest")]
		[IconFile("rareChest.png")]
		RareChest,

		[Name("Trapped Chest")]
		[IconFile("trappedChest.png")]
		TrappedChest,

		[Name("Collectible")]
		[IconFile("collectible.png")]
		Collectible,

		[Name("Any Collectible")]
		[IconFile("anyCollectible.png")]
		[ToolTip("Adventurer's Pack, Rubble")]
		AnyCollectible,

		[Name("Lore Collectible")]
		[IconFile("lore.png")]
		[ToolTip("Some Bookshelf, Some Cabinet")]
		Lore,

		[Name("Natural Collectible")]
		[IconFile("natural.png")]
		[ToolTip("Bones, Fungus, Moss, Mushroom, Some Crude Altar")]
		Natural,

		[Name("Arcane Collectible")]
		[IconFile("arcane.png")]
		[ToolTip("Alchemy Table, Scroll Rack, Some Bookshelf, Some Cabinet, Some Crude Altar")]
		Arcane,

		[Name("Plant Collectible")]
		[IconFile("plant.png")]
		[ToolTip("Flowering Plant")]
		Plant,

		[Name("Door")]
		[IconFile("door.png")]
		Door,

		[Name("Locked Door")]
		[IconFile("lockedDoor.png")]
		LockedDoor,

		[Name("Blocked Door")]
		[IconFile("blockedDoor.png")]
		BlockedDoor,

		[Name("Secret Door")]
		[IconFile("secretDoor.png")]
		SecretDoor,

		[Name("Progress Door")]
		[IconFile("progressDoor.png")]
		ProgressDoor,

		[Name("Trap")]
		[IconFile("trap.png")]
		Trap,

		[Name("Trap Box")]
		[IconFile("controlBox.png")]
		TrapBox,

		[Name("Collapsible Floor")]
		[IconFile("collapsibleFloor.png")]
		CollapsibleFloor,

		[Name("Drop")]
		[IconFile("drop.png")]
		Drop,

		[Name("Alarm")]
		[IconFile("alarm.png")]
		Alarm,

		[Name("Disabler")]
		[IconFile("disabler.png")]
		Disabler,

		[Name("Opener")]
		[IconFile("leverValve.png")]
		Opener,

		[Name("Lever")]
		[IconFile("lever.png")]
		Lever,

		[Name("Valve")]
		[IconFile("valve.png")]
		Valve,

		[Name("Rune")]
		[IconFile("rune.png")]
		Rune,

		[Name("Label")]
		[IconFile("label.png")]
		Label,

		[Name("Quest Item")]
		[IconFile("questItem.png")]
		QuestItem,

		[Name("Quest Item Used Here")]
		[IconFile("questItemUse.png")]
		QuestItemUse,

		[Name("Quest NPC")]
		[IconFile("questNPC.png")]
		QuestNPC,

		[Name("NPC")]
		[IconFile("questNPCNeutral.png")]
		NPC,

		[Name("Entrance")]
		[IconFile("entrance.png")]
		Entrance,

		[Name("Quest Exit")]
		[IconFile("questExit.png")]
		QuestExit,

		[Name("Portal")]
		[IconFile("portal.png")]
		Portal,
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

		public static Image GetImage(this IconType item)
		{
			return Image.GetImageFromResources(item.GetImageFile());
		}
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
}