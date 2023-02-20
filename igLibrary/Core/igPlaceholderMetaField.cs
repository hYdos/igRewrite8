namespace igLibrary.Core
{
	public class igPlaceHolderMetaField : igMetaField
	{
		public string _typeName;
		public short _num;
		public List<igMetaField> _templateArgs = new List<igMetaField>();

		public override void SetTemplateParameterCount(uint count)
		{
			if(_templateArgs.Count < count)
			{
				while(_templateArgs.Count < count)
				{
					_templateArgs.Add(null);
				}
			}
			else if(_templateArgs.Count > count)
			{
				while(_templateArgs.Count > count)
				{
					_templateArgs.RemoveAt(_templateArgs.Count - 1);
				}
			}
		}
		public override void SetTemplateParameter(uint index, igMetaField meta)
		{
			_templateArgs[(int)index] = meta;
		}
		public override uint GetTemplateParameterCount()
		{
			return (uint)_templateArgs.Count;
		}
		public override igMetaField GetTemplateParameter(uint index)
		{
			return _templateArgs[(int)index];
		}
	}
}