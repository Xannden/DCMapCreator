using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace CompendiumMapCreator.Data
{
	public static class Tools
	{
		#region Base Tools

		public static Tool Alarm { get; } = new Tool(IconType.Alarm);

		public static Tool AnyCollectible { get; } = new Tool(IconType.AnyCollectible);

		public static Tool Arcane { get; } = new Tool(IconType.Arcane);

		public static Tool BlockedDoor { get; } = new Tool(IconType.BlockedDoor);

		public static Tool CollapsibleFloor { get; } = new Tool(IconType.CollapsibleFloor);

		public static Tool CraftingStation { get; } = new Tool(IconType.CraftingStation);

		public static Tool Cursor { get; } = new Tool(IconType.Cursor);

		public static Tool Disabler { get; } = new Tool(IconType.Disabler);

		public static Tool Drop { get; } = new Tool(IconType.Drop);

		public static Tool Entrance { get; } = new Tool(IconType.Entrance);

		public static Tool Label { get; } = new Tool(IconType.Label);

		public static Tool Lever { get; } = new Tool(IconType.Lever);

		public static Tool LockedChest { get; } = new Tool(IconType.LockedChest);

		public static Tool LockedDoor { get; } = new Tool(IconType.LockedDoor);

		public static Tool Lore { get; } = new Tool(IconType.Lore);

		public static Tool Natural { get; } = new Tool(IconType.Natural);

		public static Tool NormalChest { get; } = new Tool(IconType.NormalChest);

		public static Tool NPC { get; } = new Tool(IconType.NPC);

		public static Tool Plant { get; } = new Tool(IconType.Plant);

		public static Tool Portal { get; } = new Tool(IconType.Portal);

		public static Tool ProgressDoor { get; } = new Tool(IconType.ProgressDoor);

		public static Tool QuestExit { get; } = new Tool(IconType.QuestExit);

		public static Tool QuestItem { get; } = new Tool(IconType.QuestItem);

		public static Tool QuestItemUse { get; } = new Tool(IconType.QuestItemUse);

		public static Tool QuestNPC { get; } = new Tool(IconType.QuestNPC);

		public static Tool RareChest { get; } = new Tool(IconType.RareChest);

		public static Tool Rune { get; } = new Tool(IconType.Rune);

		public static Tool SecretDoor { get; } = new Tool(IconType.SecretDoor);

		public static Tool Shrine { get; } = new Tool(IconType.Shrine);

		public static Tool Trader { get; } = new Tool(IconType.Trader);

		public static Tool Trap { get; } = new Tool(IconType.Trap);

		public static Tool TrapBox { get; } = new Tool(IconType.TrapBox);

		public static Tool TrappedChest { get; } = new Tool(IconType.TrappedChest);

		public static Tool Valve { get; } = new Tool(IconType.Valve);

		public static Tool MapRelocate { get; } = new Tool(IconType.MapRelocate);

		public static Tool LeverLocked { get; } = new Tool(IconType.LeverLocked);

		public static Tool Explorer { get; } = new Tool(IconType.Explorer);

		public static Tool Explorer2 { get; } = new Tool(IconType.Explorer2);

		public static Tool Entrance2 { get; } = new Tool(IconType.Entrance2);

		public static Tool PartyGather { get; } = new Tool(IconType.PartyGather);

		public static Tool Scroll { get; } = new Tool(IconType.Scroll);

		public static Tool RuneLocked { get; } = new Tool(IconType.RuneLocked);

		public static Tool ValveLocked { get; } = new Tool(IconType.ValveLocked);

		#endregion Base Tools

		#region Meta Tools

		public static Tool Activators { get; } = new Tool(IconType.Opener, "Activators")
		{
			Tools = new List<Tool>()
			{
				Lever,
				Valve,
				Rune,
				LeverLocked,
				ValveLocked,
				RuneLocked,
			},
		};

		public static Tool Collectible { get; } = new Tool(IconType.Collectible, "Collectible Devices")
		{
			Tools = new List<Tool>()
			{
				AnyCollectible,
				Lore,
				Natural,
				Arcane,
				Plant,
				Scroll,
				Explorer,
				Explorer2,
			},
		};

		public static Tool Door { get; } = new Tool(IconType.Door, "Door")
		{
			Tools = new List<Tool>()
			{
				LockedDoor,
				BlockedDoor,
				SecretDoor,
				ProgressDoor,
				MapRelocate,
			},
		};

		public static Tool Movement { get; } = new Tool(IconType.Entrance, "Entry/Exit")
		{
			Tools = new List<Tool>()
			{
				Entrance,
				QuestExit,
				Portal,
				Entrance2,
				PartyGather,
			},
			IsSelectable = false,
		};

		public static Tool QuestItems { get; } = new Tool(IconType.Label, "Quest Items")
		{
			Tools = new List<Tool>()
			{
				Label,
				QuestItem,
				QuestItemUse,
				QuestNPC,
				NPC,
				Shrine,
			},
			IsSelectable = false,
		};

		public static Tool Rewards { get; } = new Tool(IconType.NormalChest, "Rewards")
		{
			Tools = new List<Tool>()
			{
				NormalChest,
				LockedChest,
				RareChest,
				TrappedChest,
			},
			IsSelectable = false,
		};

		public static Tool Traps { get; } = new Tool(IconType.Trap, "Traps")
		{
			Tools = new List<Tool>()
			{
				Trap,
				TrapBox,
				CollapsibleFloor,
				Drop,
				Alarm,
				Disabler,
			},
			IsSelectable = false,
		};

		public static Tool Workstation { get; } = new Tool(IconType.CraftingStation, "Workstations")
		{
			Tools = new List<Tool>()
			{
				CraftingStation,
				Trader,
			},
			IsSelectable = false,
		};

		#endregion Meta Tools
	}
}