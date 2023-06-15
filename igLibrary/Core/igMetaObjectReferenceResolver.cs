namespace igLibrary.Core
{
	public class igMetaObjectReferenceResolver : igReferenceResolver
	{
		public override string MakeReference(igObject reference, igReferenceResolverContext ctx)
		{
			return ((igMetaObject)reference)._name;
		}

		public override igObject? ResolveReference(string reference, igReferenceResolverContext ctx)
		{
			return igArkCore.GetObjectMeta(reference);
		}
	}
}