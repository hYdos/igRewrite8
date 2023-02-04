namespace igLibrary.Core
{
	public class igMetaObject : igBaseMeta
	{
		public igMetaObject? _parent;
		public List<igMetaField> _metaFields; 
		public Type _vTablePointer;

		public igMetaObject()
		{
			_metaFields = new List<igMetaField>();
		}

		public override void PostUndump()
		{
			_vTablePointer = igArkCore.GetObjectDotNetType(_name);
			if(_vTablePointer == null)
			{
				_vTablePointer = typeof(igObject);	//Should be changed to some blind igObject
			}
		}
		public bool CanBeAssignedTo(igMetaObject other)
		{
			igMetaObject? tester = this;
			while(tester != null)
			{
				if(tester == other) return true;
				tester = tester._parent;
			}
			return false;
		}
	}
}