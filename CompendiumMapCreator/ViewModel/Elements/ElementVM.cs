using System.ComponentModel;
using System.IO;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.Format;

namespace CompendiumMapCreator.ViewModel
{
	public class ElementVM : INotifyPropertyChanged
	{
		private Image image;
		private byte[] unkownData;
		private int x;
		private int y;
		private double opacity = 1;
		private int rotation = 0;

		public event PropertyChangedEventHandler PropertyChanged;

		public ElementId Id { get; }

		public bool IsCopy { get; set; }

		public bool Optional { get; set; }

		public int X
		{
			get => this.x;

			set
			{
				this.x = value;

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.X)));
			}
		}

		public int Y
		{
			get => this.y;

			set
			{
				this.y = value;

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Y)));
			}
		}

		public int Rotation
		{
			get => this.rotation;

			set
			{
				this.rotation = value;

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Rotation)));
			}
		}

		public double Opacity
		{
			get => this.opacity;

			set
			{
				this.opacity = value;

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Opacity)));
			}
		}

		public virtual int Width => this.Image.Width;

		public virtual int Height => this.Image.Height;

		public bool IsBackground => this.Element.Background;

		public bool CanRotate => this.Element.Rotate;

		public bool Hidden => this.Element.Hidden;

		public int CenterX => this.Width / 2;

		public int CenterY => this.Height / 2;

		public virtual Image Image => this.image ?? (this.image = Image.GetImageFromElementId(this.Id));

		public virtual string ToolTip { get; }

		protected ElementData Element { get; }

		protected ElementVM(ElementId id)
		{
			this.Id = id;
			this.Element = App.Config.GetElement(this.Id);
		}

		public static ElementVM CreateAreaElement(ElementId id, int width, int height)
		{
			return new AreaElementVM(id, width, height);
		}

		public static ElementVM CreateElement(ElementId id)
		{
			ElementData data = App.Config.GetElement(id);

			if (data.Type == "numbered")
			{
				return new NumberedElementVM(id, 0);
			}
			else if (data.Type == "text")
			{
				return new LabelElementVM(id, 0);
			}
			else if (data.Type == "area")
			{
				return new AreaElementVM(id, 8, 8);
			}
			else
			{
				return new ElementVM(id);
			}
		}

		public static ElementVM ReadElement(BinaryReader reader, ElementId id, Project project)
		{
			ElementVM element = CreateElement(id);

			element.x = reader.ReadInt32();
			element.y = reader.ReadInt32();
			if (project.SupportsCopy)
			{
				element.IsCopy = reader.ReadBoolean();
			}

			if (project.SupportsOptional)
			{
				element.Optional = reader.ReadBoolean();
			}

			int dataLength = 0;

			if (project.SupportsExtraData)
			{
				dataLength = reader.ReadInt32();
			}

			if (project.SupportsRotation && element.CanRotate)
			{
				element.Rotation = reader.ReadInt32();
				dataLength -= sizeof(int);
			}

			element.ReadData(reader, dataLength);

			return element;
		}

		public void RotateCC()
		{
			this.Rotation -= 90;
			this.Rotation %= 360;
		}

		public void RotateCW()
		{
			this.Rotation += 90;
			this.Rotation %= 360;
		}

		public bool Contains(ImagePoint point)
		{
			return this.X <= point.X && this.Y <= point.Y && (this.X + this.Width) >= point.X && (this.Y + this.Height) >= point.Y;
		}

		public virtual ElementVM Clone()
		{
			return new ElementVM(this.Id);
		}

		internal void WriteElement(BinaryWriter writer)
		{
			writer.Write(this.X);
			writer.Write(this.Y);
			writer.Write(this.IsCopy);
			writer.Write(this.Optional);

			// Handle extra data
			long start = writer.BaseStream.Position;
			writer.Write(0); // write temp data to move the stream foreword

			if (this.CanRotate)
			{
				writer.Write(this.Rotation);
			}

			this.WriteData(writer);

			// Calculate the length of the extra data and write that before the extra data
			long end = writer.BaseStream.Position;
			writer.Seek((int)start, SeekOrigin.Begin);

			writer.Write((int)(end - start - 4));

			writer.Seek((int)end, SeekOrigin.Begin);
		}

		protected void SendPropertyChanged(object sender, string propertyName)
		{
			this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void WriteData(BinaryWriter writer)
		{
			if (this.unkownData == null || this.unkownData.Length == 0)
			{
				return;
			}

			writer.Write(this.unkownData);
		}

		protected virtual void ReadData(BinaryReader reader, int dataLength)
		{
			if (dataLength == 0)
			{
				return;
			}

			this.unkownData = reader.ReadBytes(dataLength);
		}
	}
}