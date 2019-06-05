using System.Collections.Generic;
using System.Windows.Media;

namespace CompendiumMapCreator.Controls
{
	public static class Tools
	{
		#region Basic Tools

		public static ITool Alarm = new Tool(IconType.Alarm);
		public static ITool AnyCollectible = new Tool(IconType.AnyCollectible);
		public static ITool Arcane = new Tool(IconType.Arcane);
		public static ITool BlockedDoor = new Tool(IconType.BlockedDoor);
		public static ITool CollapsibleFloor = new Tool(IconType.CollapsibleFloor);
		public static ITool Cursor = new Tool(IconType.Cursor);
		public static ITool Disabler = new Tool(IconType.Disabler);
		public static ITool Drop = new Tool(IconType.Drop);
		public static ITool Entrance = new Tool(IconType.Entrance);
		public static ITool Label = new Tool(IconType.Label);
		public static ITool Lever = new Tool(IconType.Lever);
		public static ITool LockedChest = new Tool(IconType.LockedChest);
		public static ITool LockedDoor = new Tool(IconType.LockedDoor);
		public static ITool Lore = new Tool(IconType.Lore);
		public static ITool Natural = new Tool(IconType.Natural);
		public static ITool NormalChest = new Tool(IconType.NormalChest);
		public static ITool NPC = new Tool(IconType.NPC);
		public static ITool Plant = new Tool(IconType.Plant);
		public static ITool Portal = new Tool(IconType.Portal);
		public static ITool ProgressDoor = new Tool(IconType.ProgressDoor);
		public static ITool QuestExit = new Tool(IconType.QuestExit);
		public static ITool QuestItem = new Tool(IconType.QuestItem);
		public static ITool QuestItemUse = new Tool(IconType.QuestItemUse);
		public static ITool QuestNPC = new Tool(IconType.QuestNPC);
		public static ITool RareChest = new Tool(IconType.RareChest);
		public static ITool Rune = new Tool(IconType.Rune);
		public static ITool SecretDoor = new Tool(IconType.SecretDoor);
		public static ITool Trap = new Tool(IconType.Trap);
		public static ITool TrapBox = new Tool(IconType.TrapBox);
		public static ITool TrappedChest = new Tool(IconType.TrappedChest);
		public static ITool Valve = new Tool(IconType.Valve);

		#endregion Basic Tools

		#region Meta Tools

		public static MultiTool Collectible = new MultiTool(IconType.Collectible, "Collectible Devices")
		{
			Tools = new List<ITool>()
			{
				AnyCollectible,
				Lore,
				Natural,
				Arcane,
				Plant,
			},
		};

		public static MultiTool Door = new MultiTool(IconType.Door, "Door")
		{
			Tools = new List<ITool>()
			{
				LockedDoor ,
				BlockedDoor,
				SecretDoor ,
				ProgressDoor,
			},
		};

		public static MultiTool Movement = new MultiTool(IconType.Entrance, "Movement", false)
		{
			Tools = new List<ITool>()
			{
				Entrance,
				QuestExit,
				Portal,
			},
		};

		public static MultiTool Opener = new MultiTool(IconType.Opener, "Opener")
		{
			Tools = new List<ITool>()
			{
				Lever,
				Valve,
				Rune,
			},
		};

		public static MultiTool QuestItems = new MultiTool(IconType.Label, "Quest Items", false)
		{
			Tools = new List<ITool>()
			{
				Label ,
				QuestItem ,
				QuestItemUse,
				QuestNPC ,
				NPC ,
			},
		};

		public static MultiTool Rewards = new MultiTool(IconType.NormalChest, "Rewards", false)
		{
			Tools = new List<ITool>()
			{
				NormalChest,
				LockedChest,
				RareChest,
				TrappedChest,
			},
		};

		public static MultiTool Traps = new MultiTool(IconType.Trap, "Traps", false)
		{
			Tools = new List<ITool>()
			{
				Trap ,
				TrapBox,
				CollapsibleFloor,
				Drop ,
				Alarm ,
				Disabler ,
			},
		};

		#endregion Meta Tools
	}

	public class MultiTool : ITool
	{
		public MultiTool(IconType type, string name = null, bool isSelectable = true)
		{
			this.Type = type;
			this.Name = string.IsNullOrEmpty(name) ? type.GetDescription() : name;
			this.Icon = type.GetImage().BitmapImage;
			this.ToolTip = type.GetToolTip();
			this.IsSelectable = isSelectable;
		}

		public ImageSource Icon { get; }

		public bool IsSelectable { get; }

		public string Name { get; }

		public List<ITool> Tools { get; set; }

		public string ToolTip { get; }

		public IconType Type { get; }

		public ITool Next(ITool curr)
		{
			ITool result = this;
			if (curr == null || this.Tools == null || this.Tools.Count == 0)
			{
				return result;
			}

			if (this.Name == curr.Name)
			{
				return this.Tools[0];
			}

			for (int i = 0; i < this.Tools.Count; i++)
			{
				if (this.Tools[i]?.Name == curr.Name)
				{
					return i == (this.Tools.Count - 1) ? result : this.Tools[i + 1];
				}
			}

			return result;
		}
	}

	public class Tool : ITool
	{
		public Tool(IconType type)
		{
			this.Type = type;
			this.Name = this.Type.GetDescription();
			this.Icon = type.GetImage().BitmapImage;
			this.ToolTip = this.Type.GetToolTip();
		}

		public ImageSource Icon { get; }

		public bool IsSelectable => true;

		public string Name { get; }

		public string ToolTip { get; }

		public IconType Type { get; set; }
	}
}