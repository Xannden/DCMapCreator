using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CompendiumMapCreator.Data
{
	public class UndoRedoStack<T> : INotifyCollectionChanged, INotifyPropertyChanged, IList<T>
	{
		private readonly List<T> data;

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public event PropertyChangedEventHandler PropertyChanged;

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

		public void Add(T item)
		{
			if (this.Count < this.data.Count)
			{
				this.RemoveExtra();
			}

			this.data.Add(item);
			this.Count++;

			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, this.Count - 1));
			this.OnPropertyChanged(nameof(this.Count));
		}

		public void Clear()
		{
			this.data.Clear();
			this.Count = 0;

			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			this.OnPropertyChanged(nameof(this.Count));
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

				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element, index));
				this.OnPropertyChanged(nameof(this.Count));

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

			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			this.OnPropertyChanged(nameof(this.Count));
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

			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element, index));
			this.OnPropertyChanged(nameof(this.Count));
		}

		public void Undo()
		{
			if (this.Count > 0)
			{
				this.Count--;

				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.data[this.Count], this.Count));
				this.OnPropertyChanged(nameof(this.Count));
			}
		}

		public void Redo()
		{
			if (this.Count < this.data.Count)
			{
				this.Count++;

				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.data[this.Count - 1], this.Count - 1));
				this.OnPropertyChanged(nameof(this.Count));
			}
		}

		public void SetCount(int count)
		{
			if (this.Count < count)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			int diff = this.Count - count;

			this.Count = count;
			this.OnPropertyChanged(nameof(this.Count));

			if (diff != 0)
			{
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.data.GetRange(count, diff), count));
			}
		}

		private void RemoveExtra()
		{
			int diff = this.data.Count - this.Count;

			for (int i = 0; i < diff; i++)
			{
				this.data.RemoveAt(this.data.Count - 1);
			}
		}

		private void OnPropertyChanged(string property) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

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

			public object Current
			{
				get
				{
					if (this.index < 0)
					{
						return null;
					}

					return this.data[this.index];
				}
			}

			T IEnumerator<T>.Current
			{
				get
				{
					if (this.index < 0)
					{
						return default(T);
					}

					return this.data[this.index];
				}
			}

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