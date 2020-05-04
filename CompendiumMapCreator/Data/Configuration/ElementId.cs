namespace CompendiumMapCreator.Data
{
	public readonly struct ElementId
	{
		public string Value { get; }

		public ElementId(string id)
		{
			this.Value = id;
		}

		public override bool Equals(object obj)
		{
			if (obj is ElementId id)
			{
				return this.Value == id.Value;
			}

			return false;
		}

		public override int GetHashCode() => this.Value.GetHashCode();

		public override string ToString() => this.Value;

		public static bool operator ==(ElementId lhs, ElementId rhs)
		{
			return lhs.Value == rhs.Value;
		}

		public static bool operator !=(ElementId lhs, ElementId rhs)
		{
			return lhs.Value != rhs.Value;
		}
	}
}