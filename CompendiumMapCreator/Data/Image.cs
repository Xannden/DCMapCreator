using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using CompendiumMapCreator.Data;

namespace CompendiumMapCreator
{
	public class Image : IDisposable
	{
		private static readonly Dictionary<string, Image> Cache = new Dictionary<string, Image>();

		public static Image GetImageFromElementId(ElementId id)
		{
			string fileName = id.Value + ".png";

			if (Cache.TryGetValue(fileName, out Image value))
			{
				return value;
			}
			else
			{
				Image image = new Image(GetImageUri(fileName));

				Cache.Add(fileName, image);

				return image;
			}
		}

		public static Image GetImageFromFileName(string name)
		{
			string fileName = name + ".png";

			if (Cache.TryGetValue(fileName, out Image value))
			{
				return value;
			}
			else
			{
				Image image = new Image(GetImageUri(fileName));

				Cache.Add(fileName, image);

				return image;
			}
		}

		public static Image GetImageFromTool(ToolListItem item)
		{
			if (Cache.TryGetValue(item.Icon, out Image value))
			{
				return value;
			}
			else
			{
				Image image = new Image(GetImageUri(item.Icon));

				Cache.Add(item.Icon, image);

				return image;
			}
		}

		public MemoryStream Data { get; private set; }

		public Bitmap DrawingImage { get; private set; }

		public BitmapImage BitmapImage { get; private set; }

		public int Width => this.DrawingImage.Width;

		public int Height => this.DrawingImage.Height;

		public Image(byte[] data)
		{
			this.Load(new MemoryStream(data, 0, data.Length, false, true));
		}

		public Image(MemoryStream stream)
		{
			this.Load(stream);
		}

		internal Image(string uri)
		{
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(uri);

			byte[] data = new BinaryReader(stream).ReadBytes((int)stream.Length);

			this.Load(new MemoryStream(data, 0, data.Length, false, true));
		}

		internal static string GetImageUri(string fileName)
			=> "CompendiumMapCreator.Icons." + fileName.Replace('\\', '.').Replace('/', '.');

		private void Load(MemoryStream stream)
		{
			this.Data = stream;

			this.BitmapImage = new BitmapImage();
			this.BitmapImage.BeginInit();
			this.BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			this.BitmapImage.StreamSource = this.Data;
			this.BitmapImage.EndInit();

			this.DrawingImage = (Bitmap)Bitmap.FromStream(this.Data);

			this.DrawingImage.SetResolution(96, 96);
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