using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CompendiumMapCreator.Properties;
using CompendiumMapCreator.ViewModel;

namespace CompendiumMapCreator.Data
{
	public class Config
	{
		private readonly Dictionary<ElementId, ElementData> elements = new Dictionary<ElementId, ElementData>();
		private readonly List<ToolListItem> toollist = new List<ToolListItem>();

		public void Init()
		{
			this.LoadElements();
			this.LoadTools();
		}

		public ElementData GetElement(ElementId? id)
		{
			if (!id.HasValue)
			{
				return null;
			}

			return this.elements[id.Value];
		}

		public List<ToolVM> GetTools()
		{
			List<ToolVM> tools = new List<ToolVM>();
			tools.Add(ToolVM.Cursor);

			foreach (ToolListItem tool in this.toollist)
			{
				List<ToolVM> elements = new List<ToolVM>();

				foreach (ElementId element in tool.Elements)
				{
					elements.Add(new ToolVM(element));
				}

				tools.Add(new ToolVM(tool, elements));
			}

			return tools;
		}

		public IEnumerable<ElementData> GetElements()
		{
			foreach (KeyValuePair<ElementId, ElementData> item in this.elements)
			{
				yield return item.Value;
			}
		}

		internal Dictionary<ElementId, int> WriteElementTable(BinaryWriter writer)
		{
			Dictionary<ElementId, int> dictionary = new Dictionary<ElementId, int>();

			writer.Write(this.elements.Count);

			int i = 0;

			foreach (ElementId id in this.elements.Keys)
			{
				writer.Write(id.Value);
				writer.Write(i);

				dictionary.Add(id, i);

				i++;
			}

			return dictionary;
		}

		internal Dictionary<int, ElementId> ReadElementTable(BinaryReader reader)
		{
			Dictionary<int, ElementId> table = new Dictionary<int, ElementId>();

			int count = reader.ReadInt32();

			for (int i = 0; i < count; i++)
			{
				string id = reader.ReadString();
				int num = reader.ReadInt32();

				table.Add(num, new ElementId(id));
			}

			return table;
		}

		private void LoadElements()
		{
			Utf8JsonReader reader = new Utf8JsonReader(Resources.elements);

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.StartObject:
						ElementData data = ElementData.ReadElementObject(reader);

						this.elements.Add(data.Id, data);
						break;
				}
			}
		}

		private void LoadTools()
		{
			Utf8JsonReader reader = new Utf8JsonReader(Resources.tools);

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.StartObject:
						ToolListItem item = ToolListItem.ReadToolListObject(reader, this.elements);

						this.toollist.Add(item);
						break;
				}
			}
		}
	}
}