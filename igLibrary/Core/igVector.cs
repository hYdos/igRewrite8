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
		public void SetData(IigMemory data)
		{
			_data = (igMemory<T>)data;
		}
	}
	public interface igVectorCommon
	{
		public void SetCount(uint count);
		public void SetData(IigMemory data);
	}
}