using System.Reflection;

namespace igLibrary.Core
{
	public class igObject : __internalObjectBase
	{
		public virtual void ReadIGZFields(igIGZLoader loader)
		{
			uint objectOffset = loader._stream.Tell();

			List<igMetaField> metaFields = GetMeta()._metaFields;

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;

				loader._stream.Seek(objectOffset + metaFields[i]._offsets[loader._platform]);

				object? data = metaFields[i].ReadIGZField(loader);

				FieldInfo? field = GetType().GetField(metaFields[i]._name);
				if(field != null)
				{
					field.SetValue(this, data);
				}
			}
		}
	}
}