namespace igLibrary.DotNet
{
	public class DotNetMethodDefinition : DotNetMethodSignature
	{
		public object _method;
		public DotNetTypeList _locals = new DotNetTypeList();
		public DotNetMethodDefinition _next;
		public DotNetType _declaringType;
		public igVector<byte> _IL;
		public int _stackHeight;
		public int _methodIndex;
		public DotNetLibrary _owner;
		public string _name;
	}
	public class DotNetMethodDefinitionList : igTObjectList<DotNetMethodDefinition>{}
}