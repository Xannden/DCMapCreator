using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CompendiumMapCreator
{
	public class Image : IDisposable
	{
		private static readonly Dictionary<string, Image> Cache = new Dictionary<string, Image>();

		public static Image GetImageFromResources(string fileName)
		{
			if (Cache.TryGetValue(fileName, out Image value))
			{
				return value;
			}
			else
			{
				Image image = new Image(GetImageUri(fileName), Rotation.Rotate0);

				Cache.Add(fileName, image);

				return image;
			}
		}

		public MemoryStream Data { get; private set; }

		public Bitmap DrawingImage { get; private set; }

		public BitmapImage BitmapImage { get; private set; }

		public int Width => this.DrawingImage.Width;

		public int Height => this.DrawingImage.Height;

		public Image(string uri, Rotation rotation)
		{
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(uri);

			byte[] data = new BinaryReader(stream).ReadBytes((int)stream.Length);

			this.Load(new MemoryStream(data, 0, data.Length, false, true), rotation);
		}

		public Image(byte[] data, Rotation rotation = Rotation.Rotate0)
		{
			this.Load(new MemoryStream(data, 0, data.Length, false, true), rotation);
		}

		public Image(MemoryStream stream, Rotation rotation = Rotation.Rotate0)
		{
			this.Load(stream, rotation);
		}

		public static string GetImageUri(string fileName)
			=> "CompendiumMapCreator." + fileName.Replace('\\', '.').Replace('/', '.');

		private void Load(MemoryStream stream, Rotation rotation)
		{
			this.Data = stream;

			this.BitmapImage = new BitmapImage();
			this.BitmapImage.BeginInit();
			this.BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			this.BitmapImage.StreamSource = this.Data;
			this.BitmapImage.Rotation = rotation;
			this.BitmapImage.EndInit();

			this.DrawingImage = (Bitmap)Bitmap.FromStream(this.Data);

			this.DrawingImage.SetResolution(96, 96);

			switch (rotation)
			{
				case Rotation.Rotate90:
					this.DrawingImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
					break;

				case Rotation.Rotate180:
					this.DrawingImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
					break;

				case Rotation.Rotate270:
					this.DrawingImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
					break;
			}
		}

		#region IDisposable Support

		private bool disposedValue; // To detect redundant calls

		public void Dispose() => this.Dispose(true);

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

		#endregion IDisposable Support
	}
}