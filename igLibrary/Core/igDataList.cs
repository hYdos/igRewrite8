namespace igLibrary.Core
{
	public class igTDataList<T> : igContainer
	{
		public int _count;
		public int _capacity;
		public igMemory<T> _data;
	}

	public class igDataList : igTDataList<byte> {}
}