namespace igLibrary.Core
{
	public class igPlaceHolderMetaField : igMetaField
	{
		public igMetaFieldPlatformInfo _platformInfo;
		public short _num;
		public List<igMetaField> _templateArgs = new List<igMetaField>();

		public override int ArrayNum => _num;
		public override bool IsArray => _num > 0;

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

		public override uint GetAlignment(IG_CORE_PLATFORM platform) => _platformInfo._alignments[platform];
		public override uint GetSize(IG_CORE_PLATFORM platform) => _platformInfo._sizes[platform];


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			Logging.Warn("Tried parsing placeholder field {0} string when unimplemented, returning success...", _platformInfo._name);
			return true;
		}
	}
}