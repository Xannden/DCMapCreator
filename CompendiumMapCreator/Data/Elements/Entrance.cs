namespace CompendiumMapCreator.Data
{
	public class Entrance : Element
	{
		public Rotation Rotation { get; private set; }

		public Entrance(Rotation rotation) : base(CreateImage(rotation), IconType.Entrance)
		{
			this.Rotation = rotation;
		}

		public void Rotate_Clockwise()
		{
			switch ((int)this.Rotation + 1)
			{
				case 1:
					this.Rotation = Rotation._90;
					break;

				case 2:
					this.Rotation = Rotation._180;
					break;

				case 3:
					this.Rotation = Rotation._270;
					break;

				case 4:
					this.Rotation = Rotation._0;
					break;
			}

			this.Image = CreateImage(this.Rotation);
		}

		public void Rotate_CounterClockwise()
		{
			switch ((int)this.Rotation - 1)
			{
				case -1:
					this.Rotation = Rotation._270;
					break;

				case 0:
					this.Rotation = Rotation._0;
					break;

				case 1:
					this.Rotation = Rotation._90;
					break;

				case 2:
					this.Rotation = Rotation._180;
					break;
			}

			this.Image = CreateImage(this.Rotation);
		}

		private static Image CreateImage(Rotation rotation)
		{
			switch (rotation)
			{
				case Rotation._0:
					return new Image(Image.GetImageUri(IconType.Entrance.GetImageFile()), System.Windows.Media.Imaging.Rotation.Rotate0);

				case Rotation._90:
					return new Image(Image.GetImageUri(IconType.Entrance.GetImageFile()), System.Windows.Media.Imaging.Rotation.Rotate90);

				case Rotation._180:
					return new Image(Image.GetImageUri(IconType.Entrance.GetImageFile()), System.Windows.Media.Imaging.Rotation.Rotate180);

				case Rotation._270:
					return new Image(Image.GetImageUri(IconType.Entrance.GetImageFile()), System.Windows.Media.Imaging.Rotation.Rotate270);

				default:
					return null;
			}
		}
	}

	public enum Rotation
	{
		_0 = 0,
		_90 = 1,
		_180 = 2,
		_270 = 3,
	}
}