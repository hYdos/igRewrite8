namespace igLibrary.Core
{
	public class __internalObjectBase
	{
		public uint refCount;
		public igMemoryPool internalMemoryPool;

		public virtual igMetaObject GetMeta()
		{
			return igArkCore.GetObjectMeta(GetType().Name);
		}
	}
}