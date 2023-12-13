namespace igLibrary.DotNet
{
	public class igDotNetMetaObject : igMetaObject
	{
		public NameDotNetMethodDefinitionHashTable _methods;
		public igDotNetMetaObjectList _interfaces;
		//public DotNetVTable _vTable;
		public igMetaObject _boundMeta;
		public DotNetTypeList _templateParameters;
		public igMetaObject _patchMeta;
		public DotNetLibrary? _wrappedIn;	//Added to aid with interop types
		public bool _isInterface;
		public bool _baseMethodsInherited;
		public static igMetaObjectBindingTable _bindings;
		public static Dictionary<string, igBaseMeta> _aliases = new Dictionary<string, igBaseMeta>();
		public static DotNetType _thisPointer;

		public static igMetaObject? FindType(string name, DotNetRuntime runtime)
		{
			if(string.IsNullOrEmpty(name)) return null;
			string typeName = runtime._prefix + name;
			if(!_aliases.TryGetValue(typeName, out igBaseMeta? meta))
			{
				meta = igArkCore.GetObjectMeta(name);
			}
			return (igMetaObject?)meta;
		}
		public static igMetaEnum? FindEnum(string name, DotNetRuntime runtime)
		{
			if(string.IsNullOrEmpty(name)) return null;
			string typeName = runtime._prefix + name;
			if(!_aliases.TryGetValue(typeName, out igBaseMeta? meta))
			{
				meta = igArkCore.GetMetaEnum(name);
			}
			return (igMetaEnum?)meta;
		}
	}
	public class igDotNetMetaObjectList : igTObjectList<igDotNetMetaObject>{}
}