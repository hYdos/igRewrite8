namespace igLibrary.Core
{
	public class igOrderedMapMetaField : igCompoundMetaField
	{
		public igMetaField _t;
		public igMetaField _u;
		public override void SetTemplateParameter(uint index, igMetaField meta)
		{
			if(index > 1) throw new IndexOutOfRangeException("igOrderedMap only has 2 template arguments");

			if(index == 0) _t = meta;
			if(index == 1) _u = meta;
		}
		public override void SetTemplateParameterCount(uint count)
		{
			if(count != 2) throw new IndexOutOfRangeException("igOrderedMap can only has 2 template arguments");
		}
		public override igMetaField GetTemplateParameter(uint index)
		{
			if(index > 1) throw new IndexOutOfRangeException("igOrderedMap only has 2 template arguments");

			if(index == 0) return _t;
			else return _u;
		}
		public override uint GetTemplateParameterCount() => 2;
		public override Type GetOutputType()
		{
			return typeof(igOrderedMap<,>).MakeGenericType(_t.GetOutputType(), _u.GetOutputType());
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_compoundFieldInfo = igArkCore.GetCompoundFieldInfo("igOrderedMapMetaField");
		}
	}
}