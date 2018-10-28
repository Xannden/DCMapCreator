using System.IO;
using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	public class Image
	{
		public MemoryStream Data { get; }

		public System.Drawing.Image DrawingImage { get; }

		public BitmapImage BitmapImage { get; }

		public int Width => this.DrawingImage.Width;

		public int Height => this.DrawingImage.Height;

		public Image(string path) : this(File.ReadAllBytes(path))
		{
		}

		public Image(byte[] data)
		{
			this.Data = new MemoryStream(data, 0, data.Length, false, true);

			this.BitmapImage = new BitmapImage();
			this.BitmapImage.BeginInit();
			this.BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			this.BitmapImage.StreamSource = this.Data;
			this.BitmapImage.EndInit();

			this.DrawingImage = System.Drawing.Image.FromStream(this.Data);
		}
	}
}