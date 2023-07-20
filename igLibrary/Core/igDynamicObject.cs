namespace igLibrary.Core
{
	public class igDynamicObject : igObject
	{
		public igMetaObject _meta;
		public override void ReadIGZFields(igIGZLoader loader)
		{
			igMetaObject meta = GetMeta();
			base.ReadIGZFields(loader);
			_meta = meta;
		}
	}
}