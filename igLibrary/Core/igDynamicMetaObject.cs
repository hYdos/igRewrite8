namespace igLibrary.Core
{
	public class igDynamicMetaObject : igMetaObject
	{
		public static void setMetaDataField(igMetaObject meta)
		{
			int metaMetaFieldIndex = meta.GetFieldIndexByName("_meta");

			if(metaMetaFieldIndex < 0) return;

			igMetaField field = meta._metaFields[metaMetaFieldIndex].CreateFieldCopy();

			field._default = meta;

			meta.ValidateAndSetField(metaMetaFieldIndex, field);
		}
	}
}