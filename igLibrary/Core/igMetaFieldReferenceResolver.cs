namespace igLibrary.Core
{
	public class igMetaFieldReferenceResolver : igReferenceResolver
	{
		public override string MakeReference(igObject reference, igReferenceResolverContext ctx)
		{
			igMetaField field = (igMetaField)reference;
			return field._parentMeta._name + "::" + field._fieldName;
		}

		public override igObject? ResolveReference(string reference, igReferenceResolverContext ctx)
		{
			return igArkCore.GetFieldMetaForObject(reference);
		}
	}
}