using System.Collections.Generic;
using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Projects
{
	public class ProjectV3 : Project
	{
		public ProjectV3(Image image)
			: base(image)
		{
		}

		public ProjectV3(string file, string title)
			: base(file)
		{
			this.Title = title;
		}

		internal override bool SupportsOptional => false;

		protected override Dictionary<int, ElementId> ReadElementTable(BinaryReader reader)
		{
			return new Dictionary<int, ElementId>
			{
				[10] = new ElementId("normalChest"),
				[20] = new ElementId("trappedChest"),
				[30] = new ElementId("lockedChest"),
				[35] = new ElementId("rareChest"),
				[40] = new ElementId("lockedDoor"),
				[41] = new ElementId("blockedDoor"),
				[50] = new ElementId("activator"),
				[60] = new ElementId("trapBox"),
				[70] = new ElementId("anyCollectible"),
				[80] = new ElementId("loreCollectible"),
				[90] = new ElementId("naturalCollectible"),
				[100] = new ElementId("arcaneCollectible"),
				[110] = new ElementId("questItem"),
				[111] = new ElementId("useQuestItem"),
				[120] = new ElementId("friendlyNpc"),
				[130] = new ElementId("secretDoor"),
				[140] = new ElementId("questExit"),
				[150] = new ElementId("portal"),
				[160] = new ElementId("label"),
				[170] = new ElementId("trap"),
				[180] = new ElementId("collapsibleFloor"),
				[181] = new ElementId("drop"),
				[190] = new ElementId("entrance"),
			};
		}
	}
}