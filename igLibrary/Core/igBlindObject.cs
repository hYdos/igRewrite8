namespace igLibrary.Core
{
	public class igBlindObject : igObject
	{
		private igMetaObject _meta;
		private List<object> _variables;

		public void Initialize(igMetaObject meta)
		{
			if(meta == null) throw new ArgumentNullException("MetaObject inputted for igBlindObject is null");
			if(_meta != null) throw new InvalidOperationException("thsi igBlindObject is already initialized!");

			_meta = meta;
			_variables = new List<object>();
		}

		public override igMetaObject GetMeta()
		{
			return _meta;
		}
	}
}