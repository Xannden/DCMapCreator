using System.Collections.Generic;
using System.IO;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator.Format.Projects
{
	public sealed class ProjectV6 : Project
	{
		public ProjectV6(Image image)
			: base(image)
		{
		}

		public ProjectV6(string file, string title)
			: base(file)
		{
			this.Title = title;
		}

		internal override bool SupportsOptional => true;

		protected override Dictionary<int, ElementId> ReadElementTable(BinaryReader reader) => App.Config.ReadElementTable(reader);
	}
}