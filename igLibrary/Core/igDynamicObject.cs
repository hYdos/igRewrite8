namespace igLibrary.Core
{
	public class igDynamicObject : igObject
	{
		public igMetaObject _meta;
		public override igMetaObject GetMeta()
		{
			return _meta;
		}
	}
}