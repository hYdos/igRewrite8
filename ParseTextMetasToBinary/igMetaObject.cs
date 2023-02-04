namespace PTMTB
{
	public class igMetaObject : igBaseMeta
	{
		public string _typeName;
		public string? _parentName;
		public List<igMetaField> _fields; 

		private long _fieldOffset;

		public igMetaObject()
		{
			_fields = new List<igMetaField>();
		}
	}
}