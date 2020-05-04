using System.Collections.Generic;
using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Projects
{
	public class ProjectV5 : Project
	{
		public ProjectV5(Image image)
			: base(image)
		{
		}

		public ProjectV5(string file, string title)
			: base(file)
		{
			this.Title = title;
		}

		internal override bool SupportsOptional => true;

		protected override Dictionary<int, ElementId> ReadElementTable(BinaryReader reader)
		{
			return new Dictionary<int, ElementId>
			{
				[1] = new ElementId("normalChest"),
				[2] = new ElementId("lockedChest"),
				[3] = new ElementId("rareChest"),
				[4] = new ElementId("trappedChest"),
				[5] = new ElementId("collectible"),
				[6] = new ElementId("anyCollectible"),
				[7] = new ElementId("loreCollectible"),
				[8] = new ElementId("naturalCollectible"),
				[9] = new ElementId("arcaneCollectible"),
				[10] = new ElementId("plantCollectible"),
				[11] = new ElementId("door"),
				[12] = new ElementId("lockedDoor"),
				[13] = new ElementId("blockedDoor"),
				[14] = new ElementId("secretDoor"),
				[15] = new ElementId("progressDoor"),
				[16] = new ElementId("trap"),
				[17] = new ElementId("trapBox"),
				[18] = new ElementId("collapsibleFloor"),
				[19] = new ElementId("drop"),
				[20] = new ElementId("alarm"),
				[21] = new ElementId("disabler"),
				[22] = new ElementId("activator"),
				[23] = new ElementId("lever"),
				[24] = new ElementId("valve"),
				[25] = new ElementId("Rune"),
				[26] = new ElementId("label"),
				[27] = new ElementId("questItem"),
				[28] = new ElementId("useQuestItem"),
				[29] = new ElementId("friendlyNpc"),
				[30] = new ElementId("shiftyNpc"),
				[31] = new ElementId("entrance"),
				[32] = new ElementId("questExit"),
				[33] = new ElementId("portal"),
				[34] = new ElementId("shrine"),
				[35] = new ElementId("craftingStation"),
				[36] = new ElementId("trader"),
				[37] = new ElementId("mapRelocate"),
				[38] = new ElementId("leverLocked"),
				[39] = new ElementId("explorer"),
				[40] = new ElementId("explorer2"),
				[41] = new ElementId("entrance2"),
				[42] = new ElementId("partyGather"),
				[43] = new ElementId("scroll"),
				[44] = new ElementId("runeLocked"),
				[45] = new ElementId("valveLocked"),
			};
		}
	}
}