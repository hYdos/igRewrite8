namespace igLibrary.DotNet
{
	public class igDotNetDynamicMetaObject : igDotNetMetaObject
	{
		public DotNetLibrary _owner;
		public void FinalizeAppendToArkCore()
		{
			igDynamicMetaObject.setMetaDataField(this);
		}
		public override igObject ConstructInstance(igMemoryPool memPool, bool setFields = true)
		{
			igObject obj = base.ConstructInstance(memPool, setFields);
			obj.dynamicMeta = true;
			return obj;
		}
	}
}