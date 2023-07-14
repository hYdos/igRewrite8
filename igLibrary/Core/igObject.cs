using System.Reflection;

namespace igLibrary.Core
{
	public class igObject : __internalObjectBase
	{
		public virtual void ReadIGZFields(igIGZLoader loader)
		{
			uint objectOffset = loader._stream.Tell();

			igMetaObject meta = GetMeta();
			List<igMetaField> metaFields = meta._metaFields;

			if(GetMeta()._name == "CBehaviorPhysicsPencilComponentData")
			;

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;

				//if(!metaFields[i]._properties._persistent) continue;

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
			igMetaObject meta = GetMeta();
			List<igMetaField> metaFields = GetMeta()._metaFields;
			WriteIGZFieldsInternal(saver, section, metaFields);
			igDependenciesAttribute? depAttr = meta.GetAttribute<igDependenciesAttribute>();
			if(depAttr == null) return;
			igDependencyProvider depProvider = (igDependencyProvider)depAttr._value.ConstructInstance(section._pool);
			depProvider.GetBuildDependencies(this, out igStringRefList? list);
			if(list != null)
			{
				for(int i = 0; i < list._count; i++)
				{
					saver.AddBuildDependency(list[i]);
				}
			}
			depProvider.GetFileDependencies(this, out list);
			if(list != null)
			{
				for(int i = 0; i < list._count; i++)
				{
					saver.AddFileDependency(list[i]);
				}
			}
		}
		public virtual void WriteIGZFieldsInternal(igIGZSaver saver, igIGZSaver.SaverSection section, List<igMetaField> metaFields)
		{
			uint objectOffset = section._sh.Tell();

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;

				section.PushAlignment(metaFields[i].GetAlignment(saver._platform));

				object? data = null;

				if(metaFields[i]._properties._persistent)
				{
					FieldInfo? field = GetType().GetField(metaFields[i]._name);

					if(field == null) continue;

					data = field.GetValue(this);
				}
				else
				{
					data = metaFields[i].GetDefault(this);
					if((metaFields[i].GetOutputType().IsValueType || metaFields[i].GetOutputType() == typeof(string)) && data == null) continue;
				}
				
				section._sh.Seek(objectOffset + metaFields[i]._offsets[saver._platform]);

				metaFields[i].WriteIGZField(saver, section, data);
			}
		}
		public virtual void ResetFields() => ResetFields(GetMeta());
		public virtual void ResetFields(igMetaObject meta)
		{
			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				if(meta._metaFields[i] is igStaticMetaField || meta._metaFields[i] is igPropertyFieldMetaField || meta._metaFields[i] is igBitFieldMetaField) continue;

				FieldInfo? field = meta._vTablePointer.GetField(meta._metaFields[i]._name);

				object? data = meta._metaFields[i].GetDefault(this);

				if(meta._metaFields[i].GetOutputType().IsValueType && data == null) continue;

				field.SetValue(this, data);
			}
		}
	}
}