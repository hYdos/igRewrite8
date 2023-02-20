using System.Reflection;

namespace igLibrary.Core
{
	public class igObject : __internalObjectBase
	{
		public virtual void ReadIGZFields(igIGZLoader loader)
		{
			uint objectOffset = loader._stream.Tell();

			List<igMetaField> _metaFields = GetMeta()._metaFields;

			for(int i = 0; i < _metaFields.Count; i++)
			{
				loader._stream.Seek(objectOffset + _metaFields[i]._offsets[loader._platform]);

				object? data = _metaFields[i].ReadIGZField(loader);

				FieldInfo? field = GetType().GetField(_metaFields[i]._name);
				if(field != null)
				{
					field.SetValue(this, data);
				}
			}
		}
	}
}