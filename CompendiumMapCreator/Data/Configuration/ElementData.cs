using System;
using System.Text.Json;

namespace CompendiumMapCreator.Data
{
	public sealed class ElementData
	{
		public ElementId Id { get; private set; }

		public string Name { get; private set; }

		public string ToolTip { get; private set; }

		public bool Background { get; private set; }

		public bool Important { get; private set; }

		public bool Hidden { get; private set; }

		public string Type { get; private set; }

		public bool Rotate { get; private set; }

		public byte[] Color { get; private set; }

		public override string ToString() => $"{this.Id.ToString()}, {this.Type}";

		private ElementData()
		{
		}

		internal static ElementData ReadElementObject(Utf8JsonReader reader)
		{
			ElementData element = new ElementData();

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.EndObject:
						return element;

					case JsonTokenType.PropertyName:
						string name = reader.GetString();

						reader.Read();

						switch (name)
						{
							case "id":
								element.Id = new ElementId(reader.GetString());
								break;

							case "name":
								element.Name = reader.GetString();
								break;

							case "type":
								element.Type = reader.GetString();
								break;

							case "toolTip":
								element.ToolTip = reader.GetString();
								break;

							case "background":
								element.Background = reader.GetBoolean();
								break;

							case "important":
								element.Important = reader.GetBoolean();
								break;

							case "hidden":
								element.Hidden = reader.GetBoolean();
								break;

							case "rotate":
								element.Rotate = reader.GetBoolean();
								break;

							case "color":
								string[] colors = reader.GetString().Split(',');

								element.Color = new byte[] { byte.Parse(colors[0]), byte.Parse(colors[1]), byte.Parse(colors[2]) };
								break;
						}

						break;
				}
			}

			throw new InvalidOperationException();
		}
	}
}