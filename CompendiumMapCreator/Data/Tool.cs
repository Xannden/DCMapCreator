using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace CompendiumMapCreator.Data
{
	public static class Tools
	{
		#region Base Tools

		public static Tool Alarm = new Tool(IconType.Alarm);
		public static Tool AnyCollectible = new Tool(IconType.AnyCollectible);
		public static Tool Arcane = new Tool(IconType.Arcane);
		public static Tool BlockedDoor = new Tool(IconType.BlockedDoor);
		public static Tool CollapsibleFloor = new Tool(IconType.CollapsibleFloor);
		public static Tool CraftingStation = new Tool(IconType.CraftingStation);
		public static Tool Cursor = new Tool(IconType.Cursor);
		public static Tool Disabler = new Tool(IconType.Disabler);
		public static Tool Drop = new Tool(IconType.Drop);
		public static Tool Entrance = new Tool(IconType.Entrance);
		public static Tool Label = new Tool(IconType.Label);
		public static Tool Lever = new Tool(IconType.Lever);
		public static Tool LockedChest = new Tool(IconType.LockedChest);
		public static Tool LockedDoor = new Tool(IconType.LockedDoor);
		public static Tool Lore = new Tool(IconType.Lore);
		public static Tool Natural = new Tool(IconType.Natural);
		public static Tool NormalChest = new Tool(IconType.NormalChest);
		public static Tool NPC = new Tool(IconType.NPC);
		public static Tool Plant = new Tool(IconType.Plant);
		public static Tool Portal = new Tool(IconType.Portal);
		public static Tool ProgressDoor = new Tool(IconType.ProgressDoor);
		public static Tool QuestExit = new Tool(IconType.QuestExit);
		public static Tool QuestItem = new Tool(IconType.QuestItem);
		public static Tool QuestItemUse = new Tool(IconType.QuestItemUse);
		public static Tool QuestNPC = new Tool(IconType.QuestNPC);
		public static Tool RareChest = new Tool(IconType.RareChest);
		public static Tool Rune = new Tool(IconType.Rune);
		public static Tool SecretDoor = new Tool(IconType.SecretDoor);
		public static Tool Shrine = new Tool(IconType.Shrine);
		public static Tool Trader = new Tool(IconType.Trader);
		public static Tool Trap = new Tool(IconType.Trap);
		public static Tool TrapBox = new Tool(IconType.TrapBox);
		public static Tool TrappedChest = new Tool(IconType.TrappedChest);
		public static Tool Valve = new Tool(IconType.Valve);
		public static Tool MapRelocate = new Tool(IconType.MapRelocate);
		public static Tool LeverLocked = new Tool(IconType.LeverLocked);
		public static Tool Explorer = new Tool(IconType.Explorer);
		public static Tool Explorer2 = new Tool(IconType.Explorer2);
		public static Tool Entrance2 = new Tool(IconType.Entrance2);
		public static Tool PartyGather = new Tool(IconType.PartyGather);
		public static Tool Scroll = new Tool(IconType.Scroll);
		public static Tool RuneLocked = new Tool(IconType.RuneLocked);
		public static Tool ValveLocked = new Tool(IconType.ValveLocked);

		#endregion Base Tools

		#region Meta Tools

		public static Tool Activators = new Tool(IconType.Opener, "Activators")
		{
			Tools = new List<Tool>()
			{
				Lever,
				Valve,
				Rune,
				LeverLocked,
				ValveLocked,
				RuneLocked
			},
		};
		public static Tool Collectible = new Tool(IconType.Collectible, "Collectible Devices")
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
				Explorer2
			},
		};

		public static Tool Door = new Tool(IconType.Door, "Door")
		{
			Tools = new List<Tool>()
			{
				LockedDoor ,
				BlockedDoor,
				SecretDoor ,
				ProgressDoor,
				MapRelocate,
			},
		};

		public static Tool Movement = new Tool(IconType.Entrance, "Entry/Exit")
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
		public static Tool QuestItems = new Tool(IconType.Label, "Quest Items")
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
		public static Tool Rewards = new Tool(IconType.NormalChest, "Rewards")
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

		public static Tool Traps = new Tool(IconType.Trap, "Traps")
		{
			Tools = new List<Tool>()
			{
				Trap ,
				TrapBox,
				CollapsibleFloor,
				Drop ,
				Alarm ,
				Disabler ,
			},
			IsSelectable = false,
		};

		public static Tool Workstation = new Tool(IconType.CraftingStation, "Workstations")
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

	public class Tool : INotifyPropertyChanged
	{
		private bool isExpanded;

		public Tool(IconType type)
		{
			this.Type = type;
			this.Description = this.Type.GetName();
		}

		public Tool(IconType type, string name)
		{
			this.Type = type;
			this.Description = name;
		}

		public string Description { get; }

		public bool IsExpanded
		{
			get => this.isExpanded;

			set
			{
				this.isExpanded = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
			}
		}

		public ImageSource Image => this.Type.GetImage().BitmapImage;

		public bool IsSelectable { get; set; } = true;

		public List<Tool> Tools { get; set; }

		public string ToolTip => this.Type.GetToolTip();

		public IconType Type { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public Tool Next(Tool curr)
		{
			Tool result = this;
			if (curr == null || this.Tools == null || this.Tools.Count == 0)
			{
				return result;
			}

			if (this.Description == curr.Description)
			{
				return this.Tools[0];
			}

			for (int i = 0; i < this.Tools.Count; i++)
			{
				if (this.Tools[i]?.Description == curr.Description)
				{
					return i == (this.Tools.Count - 1) ? result : this.Tools[i + 1];
				}
			}

			return result;
		}
	}
}