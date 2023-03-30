using System.Collections;

namespace igLibrary.Core
{
	public class igTDataList<T> : igContainer, IEnumerable<T>
	{
		public int _count;
		public int _capacity;
		public igMemory<T> _data;

		public T this[int index]
		{
			get => _data[index];
			set => _data[index] = value;
		}

		public T Append(T item)
		{
			if(_count >= _capacity)
			{
				SetCapacity(_capacity + 4);
			}

			_data[_count] = item;
			_count++;
			return item;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)_data).GetEnumerator();
		}

		public void SetCapacity(int newCapacity)
		{
			_data.Realloc(newCapacity);
			_capacity += newCapacity;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_data).GetEnumerator();
		}
	}

	public class igDataList : igTDataList<byte> {}
}