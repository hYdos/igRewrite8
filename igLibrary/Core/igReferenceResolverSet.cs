namespace igLibrary.Core
{
	public class igReferenceResolverSet : Dictionary<string, igReferenceResolver>		//INHERITS FROM igStringObjectHashTable
	{
		public void MakeReference(igObject reference, igReferenceResolverContext ctx, out igHandleName handleName)
		{
			string key = string.Empty;

			     if(reference is igMetaObject) key = "metaobject";
			else if(reference is igMetaField) key = "metafield";
			else
			{
				handleName = default;
				return;
			}

			handleName = new igHandleName();
			handleName._ns.SetString(key);
			handleName._name.SetString(this[key].MakeReference(reference, ctx));
		}
		public igObject? ResolveReference(igHandleName handleName, igReferenceResolverContext ctx)
		{
			igReferenceResolver resolver;
			if(this.TryGetValue(handleName._ns._string, out resolver))
			{
				return resolver.ResolveReference(handleName._name._string, ctx);
			}
			else return null;
		}
	}
}