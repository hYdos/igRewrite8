namespace igLibrary.Core
{
	public class __internalObjectBase
	{
		public igMemoryPool internalMemoryPool;

		public virtual igMetaObject GetMeta()
		{
			return igArkCore.GetObjectMeta(GetType().Name);
		}
	}
}