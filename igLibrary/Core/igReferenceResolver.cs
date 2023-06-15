namespace igLibrary.Core
{
	public abstract class igReferenceResolver : igNamedObject
	{
		public virtual string MakeReference(igObject reference, igReferenceResolverContext ctx) => null;
		public virtual igObject? ResolveReference(string reference, igReferenceResolverContext ctx) => null;
	}
}