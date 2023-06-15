namespace igLibrary.Core
{
	public class igExternalReferenceSystem : igSingleton<igExternalReferenceSystem>
	{
		public igReferenceResolverSet _globalSet = new igReferenceResolverSet();
	}
}