namespace igLibrary.Core
{
	public class igVector<T> : igVectorCommon
	{
		public long _count;
		public igMemory<T> _data;

		public void SetCount(uint count)
		{
			_count = count;
		}
		public uint GetCount() => (uint)_count;
		public void SetData(IigMemory data)
		{
			_data = (igMemory<T>)data;
		}
		public void SetCapacity(int capacity)
		{
			_data.Realloc(capacity);
			_count = (_count < capacity) ? _count : capacity;
		}
		public void Append(T data)
		{
			if(_count == _data.Length)
			{
				_data.Realloc((int)_count + 1);
			}
			_data[(int)_count] = data;
			_count++;
		}
		public IigMemory GetData() => _data;
	}
	public interface igVectorCommon
	{
		public void SetCount(uint count);
		public void SetData(IigMemory data);
		public uint GetCount();
		public void SetCapacity(int capacity);
		public IigMemory GetData();
	}
}