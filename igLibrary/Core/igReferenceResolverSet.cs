namespace igLibrary.Core
{
	public class igReferenceResolverSet : Dictionary<string, igReferenceResolver>		//INHERITS FROM igStringObjectHashTable
	{
		public igObject ResolveReference(igHandleName handleName, igReferenceResolverContext ctx)
		{
			return null;
		}
	}
}