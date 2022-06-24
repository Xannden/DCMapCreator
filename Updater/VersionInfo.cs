using System;
using System.IO;
using System.Text.Json;

namespace Xannden.Update
{
	internal class VersionInfo : IComparable<VersionInfo>
	{
		public Version Version { get; private set; }

		public string Link { get; private set; }

		public static VersionInfo Parse(Utf8JsonReader reader)
		{
			VersionInfo info = new VersionInfo();

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.EndObject:
						return info;

					case JsonTokenType.PropertyName:
						string propName = reader.GetString();

						reader.Read();

						if (propName == "version")
						{
							info.Version = Version.Parse(reader.GetString() + ".0");
						}
						else if (propName == "link")
						{
							info.Link = reader.GetString();
						}
						break;
				}
			}

			throw new InvalidDataException();
		}

		public int CompareTo(VersionInfo other) => this.Version.CompareTo(other.Version);
	}
}