namespace igLibrary.DotNet
{
	public class DotNetMethodMeta : igObject
	{
		public string _importTag;
		public string _methodName;
		public string _entryPoint;
		public DotNetParameterMeta _return;
		public DotNetParameterMetaList _parameters = new DotNetParameterMetaList();
	}
	public class DotNetMethodMetaList : igTObjectList<DotNetMethodMeta>{}
}