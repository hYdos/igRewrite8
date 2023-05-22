namespace igLibrary.Core
{
	public class igReferenceResolver : igNamedObject
	{
		public virtual void MakeReference(string reference, igReferenceResolverContext ctx){}
		public virtual void ResolveReference(string reference, igReferenceResolverContext ctx){}
	}
}