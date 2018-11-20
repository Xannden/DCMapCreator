using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	public class Image : IDisposable
	{
		private static readonly Dictionary<string, Image> cache = new Dictionary<string, Image>();

		public static Image GetImageFromResources(string fileName)
		{
			if (cache.TryGetValue(fileName, out Image value))
			{
				return value;
			}
			else
			{
				Image image = new Image(new Uri("pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + fileName, UriKind.Absolute));

				cache.Add(fileName, image);

				return image;
			}
		}

		public MemoryStream Data { get; }

		public System.Drawing.Image DrawingImage { get; }

		public BitmapImage BitmapImage { get; }

		public int Width => this.DrawingImage.Width;

		public int Height => this.DrawingImage.Height;

		public Image(string path) : this(File.ReadAllBytes(path))
		{
		}

		public Image(Uri uri)
		{
			Stream stream = Application.GetResourceStream(uri).Stream;

			byte[] data = new BinaryReader(stream).ReadBytes((int)stream.Length);

			this.Data = new MemoryStream(data, 0, data.Length, false, true);

			this.BitmapImage = new BitmapImage();
			this.BitmapImage.BeginInit();
			this.BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			this.BitmapImage.StreamSource = this.Data;
			this.BitmapImage.EndInit();

			this.DrawingImage = System.Drawing.Image.FromStream(this.Data);
		}

		public Image(byte[] data) : this(new MemoryStream(data, 0, data.Length, false, true))
		{
		}

		public Image(MemoryStream stream)
		{
			this.Data = stream;

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