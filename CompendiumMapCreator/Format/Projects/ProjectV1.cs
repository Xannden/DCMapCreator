using System.Collections.Generic;
using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Projects
{
	public class ProjectV1 : Project
	{
		public ProjectV1(Image image)
			: base(image)
		{
		}

		public ProjectV1(string file)
			: base(file)
		{
		}

		internal override bool SupportsOptional => false;

		internal override bool SupportsCopy => false;

		internal override bool SupportsExtraData => false;

		internal override bool SupportsRotation => false;

		protected override Dictionary<int, ElementId> ReadElementTable(BinaryReader reader)
		{
			return new Dictionary<int, ElementId>
			{
				[1] = new ElementId("normalChest"),
				[2] = new ElementId("trappedChest"),
				[3] = new ElementId("lockedChest"),
				[4] = new ElementId("lockedDoor"),
				[5] = new ElementId("activator"),
				[6] = new ElementId("trapBox"),
				[7] = new ElementId("anyCollectible"),
				[8] = new ElementId("loreCollectible"),
				[9] = new ElementId("naturalCollectible"),
				[10] = new ElementId("arcaneCollectible"),
				[11] = new ElementId("questItem"),
				[12] = new ElementId("friendlyNpc"),
				[13] = new ElementId("secretDoor"),
				[14] = new ElementId("questExit"),
				[15] = new ElementId("portal"),
				[16] = new ElementId("label"),
				[17] = new ElementId("trap"),
				[18] = new ElementId("collapsibleFloor"),
				[19] = new ElementId("entrance"),
			};
		}
	}
}