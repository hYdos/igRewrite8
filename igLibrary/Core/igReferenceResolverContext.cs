namespace igLibrary.Core
{
	public class igReferenceResolverContext : igObject
	{
		public igObjectList _rootObjects;
		public string _basePath;					//Technically igMemory<byte>
		public Dictionary<string, igObject> _data;	//Technically igStringObjectHashTable
	}
}