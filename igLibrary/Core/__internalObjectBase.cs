namespace igLibrary.Core
{
	public class __internalObjectBase
	{
		public virtual igMetaObject GetMeta()
		{
			return igArkCore.GetObjectMeta(GetType().Name);
		}
	}
}