namespace igLibrary.DotNet
{
	public class igDotNetDynamicMetaObject : igDotNetMetaObject
	{
		public DotNetLibrary _owner;
		public override void AppendToArkCore()
		{
			igDynamicMetaObject.setMetaDataField(this);
			base.AppendToArkCore();
		}
		public override igObject ConstructInstance(igMemoryPool memPool, bool setFields = true)
		{
			igObject obj = base.ConstructInstance(memPool, setFields);
			obj.dynamicMeta = true;
			return obj;
		}
	}
}