namespace CompendiumMapCreator.Data
{
	public class Entrance : Element
	{
		public Rotation Rotation { get; private set; }

		public Entrance(Rotation rotation)
			: base(CreateImage(rotation), IconType.Entrance)
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
			return rotation switch
			{
				Rotation._0 => new Image(Image.GetImageUri(IconType.Entrance.GetImageFile()), System.Windows.Media.Imaging.Rotation.Rotate0),
				Rotation._90 => new Image(Image.GetImageUri(IconType.Entrance.GetImageFile()), System.Windows.Media.Imaging.Rotation.Rotate90),
				Rotation._180 => new Image(Image.GetImageUri(IconType.Entrance.GetImageFile()), System.Windows.Media.Imaging.Rotation.Rotate180),
				Rotation._270 => new Image(Image.GetImageUri(IconType.Entrance.GetImageFile()), System.Windows.Media.Imaging.Rotation.Rotate270),
				_ => null,
			};
		}
	}
}