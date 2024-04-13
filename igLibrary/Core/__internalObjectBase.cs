using System.Reflection;

namespace igLibrary.Core
{
	public class __internalObjectBase
	{
		public uint refCount;
		public igMemoryPool internalMemoryPool;
		internal bool dynamicMeta;
		internal igMetaObject internalMeta;

		public virtual igMetaObject GetMeta()
		{
			return internalMeta;
		}
	}
}