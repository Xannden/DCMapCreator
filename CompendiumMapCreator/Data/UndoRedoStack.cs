using System;
using System.Collections;
using System.Collections.Generic;
using CompendiumMapCreator.Edits;

namespace CompendiumMapCreator.Data
{
	public class UndoRedoStack<T> : IList<T>
		where T : Edit
	{
		private readonly List<T> data;

		public IEnumerable<T> Data
		{
			get
			{
				foreach (T item in this.data)
				{
					yield return item;
				}
			}
		}

		public int Total => this.data.Count;

		public int Generation { get; private set; }

		public int Count { get; private set; }

		public bool IsReadOnly => false;

		public T this[int index]
		{
			get
			{
				if (index < this.Count)
				{
					return this.data[index];
				}

				throw new ArgumentOutOfRangeException(nameof(index));
			}

			set
			{
				if (index < this.Count)
				{
					this.data[index] = value;
				}

				throw new ArgumentOutOfRangeException(nameof(index));
			}
		}

		public UndoRedoStack() : this(0)
		{
		}

		public UndoRedoStack(int size)
		{
			this.data = new List<T>(size);
		}

		public void Add(T item, IList<Element> list)
		{
			item.Apply(list);
			this.Add(item);
		}

		public void Add(T item)
		{
			if (this.Count < this.data.Count)
			{
				this.RemoveExtra();
			}

			this.data.Add(item);
			this.Count++;
		}

		public void Clear()
		{
			this.data.Clear();
			this.Count = 0;
		}

		public bool Contains(T item)
		{
			int index = this.data.IndexOf(item);

			return index == -1 ? false : index < this.Count;
		}

		public void CopyTo(T[] array, int arrayIndex) => this.data.CopyTo(0, array, arrayIndex, this.Count);

		public bool Remove(T item)
		{
			int index = this.data.IndexOf(item);

			if (index < this.Count && index != -1)
			{
				T element = this.data[index];
				this.data.RemoveAt(index);
				this.Count--;

				return true;
			}

			return false;
		}

		public IEnumerator<T> GetEnumerator() => new Enumerator(this.data, this.Count);

		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this.data, this.Count);

		public int IndexOf(T item)
		{
			int index = this.data.IndexOf(item);

			return index < this.Count ? index : -1;
		}

		public void Insert(int index, T item)
		{
			if (index < 0 || index > this.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (this.Count < this.data.Count)
			{
				this.RemoveExtra();
			}

			this.data.Insert(index, item);
			this.Count++;
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= this.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			T element = this.data[index];

			this.data.RemoveAt(index);
			this.Count--;
		}

		public void Undo(IList<Element> list)
		{
			if (this.Count > 0)
			{
				this.Count--;

				this.data[this.Count].Undo(list);
			}
		}

		public void Redo(IList<Element> list)
		{
			if (this.Count < this.data.Count)
			{
				this.data[this.Count].Apply(list);

				this.Count++;
			}
		}

		private void RemoveExtra()
		{
			int diff = this.data.Count - this.Count;

			for (int i = 0; i < diff; i++)
			{
				this.data.RemoveAt(this.data.Count - 1);
			}

			if (this.Count > 0)
			{
				this.Generation++;
			}
		}

		private struct Enumerator : IEnumerator, IEnumerator<T>
		{
			private readonly List<T> data;
			private readonly int position;
			private int index;

			public Enumerator(List<T> data, int position)
			{
				this.data = data;
				this.position = position;
				this.index = -1;
			}

			public object Current => this.index < 0 ? null : this.data[this.index];

			T IEnumerator<T>.Current => this.index < 0 ? default(T) : this.data[this.index];

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (this.position == 0)
				{
					return false;
				}

				if (this.index < this.position)
				{
					this.index++;
				}

				return this.index < this.position;
			}

			public void Reset() => this.index = -1;
		}
	}
}