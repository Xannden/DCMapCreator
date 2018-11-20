namespace CompendiumMapCreator.Data
{
	public class ElementCopy : Element
	{
		public ElementCopy(Element source) : base(source.Type)
		{
			this.X = source.X;
			this.Y = source.Y;
		}
	}

	public class NumberedElementCopy : NumberedElement
	{
		public NumberedElementCopy(NumberedElement source) : base(source.Number, source.Background, source.Type)
		{
			this.X = source.X;
			this.Y = source.Y;
		}
	}
}