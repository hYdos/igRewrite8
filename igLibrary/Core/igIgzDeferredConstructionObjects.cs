namespace igLibrary.Core
{
	public class igIgzDeferredConstructionObjects : igObject
	{
		public igObjectDirectoryList _dependsOn;
		public igObjectList _objects;
	}
	public class igIgzDeferredConstructionObjectsList : igTObjectList<igIgzDeferredConstructionObjects> {}
}