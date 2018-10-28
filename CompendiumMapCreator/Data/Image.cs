using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	public class Image : IDisposable
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

		#region IDisposable Support

		private bool disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposedValue)
			{
				if (disposing)
				{
					this.Data.Dispose();
					this.DrawingImage.Dispose();
				}

				this.disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose() =>
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			this.Dispose(true);

		#endregion IDisposable Support
	}
}