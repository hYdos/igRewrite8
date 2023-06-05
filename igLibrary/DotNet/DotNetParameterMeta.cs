namespace igLibrary.DotNet
{
	public class DotNetParameterMeta : igObject
	{
		public string _name;
		//public DotNetAttributeList _attributes;
	}
	public class DotNetParameterMetaList : igTObjectList<DotNetParameterMeta>{}
}