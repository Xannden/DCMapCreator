using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CompendiumMapCreator.Data
{
	public struct ToolListItem
	{
		public string Name { get; private set; }

		public string Icon { get; private set; }

		public string Shortcut { get; private set; }

		public List<ElementId> Elements { get; private set; }

		public ElementId? DefaultElement { get; private set; }

		public override string ToString() => this.Name;

		internal static ToolListItem ReadToolListObject(Utf8JsonReader reader, IReadOnlyDictionary<ElementId, ElementData> elements)
		{
			ToolListItem item = default;

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.EndObject:
						return item;

					case JsonTokenType.PropertyName:
						string name = reader.GetString();

						reader.Read();

						switch (name)
						{
							case "name":
								item.Name = reader.GetString();
								break;

							case "icon":
								item.Icon = reader.GetString();
								break;

							case "shortcut":
								item.Shortcut = reader.GetString();
								break;

							case "elements":
								item.Elements = ReadElements(reader, elements);
								break;

							case "defaultElement":
								{
									ElementId id = new ElementId(reader.GetString());

									if (!elements.ContainsKey(id))
									{
										throw new System.Exception($"element id {id} does not exist");
									}

									item.DefaultElement = id;
								}

								break;
						}

						break;
				}
			}

			throw new InvalidOperationException();
		}

		private static List<ElementId> ReadElements(Utf8JsonReader reader, IReadOnlyDictionary<ElementId, ElementData> elements)
		{
			List<ElementId> elementList = new List<ElementId>();
			while (reader.TokenType != JsonTokenType.EndArray)
			{
				reader.Read();

				if (reader.TokenType == JsonTokenType.String)
				{
					ElementId id = new ElementId(reader.GetString());

					if (!elements.ContainsKey(id))
					{
						throw new System.Exception($"element id {id} does not exist");
					}

					elementList.Add(id);
				}
			}

			return elementList;
		}
	}
}