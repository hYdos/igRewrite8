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
		public virtual void WriteIGZFields(igIGZSaver saver, igIGZSaver.SaverSection section)
		{
			uint objectOffset = section._sh.Tell();

			List<igMetaField> metaFields = GetMeta()._metaFields;

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;

				FieldInfo? field = GetType().GetField(metaFields[i]._name);

				if(field == null) continue;

				object? data = field.GetValue(this);
				
				section._sh.Seek(objectOffset + metaFields[i]._offsets[saver._platform]);

				metaFields[i].WriteIGZField(saver, section, data);
			}
		}
	}
}