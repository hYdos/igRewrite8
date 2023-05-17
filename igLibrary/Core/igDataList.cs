using System.Collections;

namespace igLibrary.Core
{
	public class igTDataList<T> : igContainer, IEnumerable<T>, IigDataList
	{
		public int _count;
		public int _capacity;
		public igMemory<T> _data = new igMemory<T>();

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

		public int GetCapacity() => _capacity;

		public int GetCount() => _count;

		public IigMemory GetData()
		{
			return _data;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)_data).GetEnumerator();
		}

		public Type GetMemoryType()
		{
			return _data.GetType().GenericTypeArguments[0];
		}

		public object GetObject(int index)
		{
			return this[index];
		}

		public void SetCapacity(int capacity)
		{
			_data.Realloc(capacity);
			_capacity = capacity;
		}

		public void SetCount(int count)
		{
			SetCapacity(((count + 3) / 4) * 4);
		}

		public void SetObject(int index, object data)
		{
			_data[index] = (T)data;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_data).GetEnumerator();
		}

	}

	public interface IigDataList
	{
		public int GetCount();
		public void SetCount(int count);
		public int GetCapacity();
		public void SetCapacity(int capacity);
		public Type GetMemoryType();
		public IigMemory GetData();
		public object GetObject(int index);
		public void SetObject(int index, object data);
	}

	public class igDataList : igTDataList<byte> {}
}