namespace igLibrary.DotNet
{
	public class DotNetParameterMeta : igObject
	{
		public string _name;
		public igObjectList _attributes;    //DotNetAttributeList
	}
	public class DotNetParameterMetaList : igTObjectList<DotNetParameterMeta>{}
}