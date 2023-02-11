using System.Collections;

namespace igLibrary.Core
{
	public struct igMemory<T> : IEnumerable<T>, IigMemory
	{
		public igMemoryPool _memoryPool;
		public T[] _data;

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)this._data).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this._data).GetEnumerator();
		}
		igMemoryPool IigMemory.GetMemoryPool() => _memoryPool;
		void IigMemory.SetMemoryPool(igMemoryPool pool) => _memoryPool = pool;
		Array IigMemory.GetData() => _data;
		void IigMemory.SetData(Array data)
		{
			if(data.GetType().GetElementType().IsAssignableTo(typeof(T)))
			{
				_data = data.Cast<T>().ToArray();
			}
		}
	}
	public interface IigMemory
	{
		igMemoryPool GetMemoryPool();
		void SetMemoryPool(igMemoryPool pool);
		Array GetData();
		void SetData(Array data);
	}
}