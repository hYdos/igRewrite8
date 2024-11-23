namespace igLibrary.DotNet
{
	public class igDotNetMetaObject : igMetaObject
	{
		public NameDotNetMethodDefinitionHashTable _methods;
		public igDotNetMetaObjectList _interfaces;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _vTable;    //DotNetVTable
		public igMetaObject _boundMeta;
		public DotNetTypeList _templateParameters;
		public igMetaObject _patchMeta;
		public object? _dotNetFieldNames;
		public object? _cppFieldNames;
		public int _exposedFieldCount;
		public object? _cppMethods;
		public object? _cppMethodNames;
		public int _cppMethodCount;
		public DotNetLibrary? _wrappedIn;	//Added to aid with interop types
		public bool _isInterface;
		public bool _baseMethodsInherited;
		public static igMetaObjectBindingTable _bindings;
		public static Dictionary<string, igBaseMeta> _aliases = new Dictionary<string, igBaseMeta>();
		public static DotNetType _thisPointer;

		public override void PostUndump()
		{
			base.PostUndump();

			igObjectRefMetaField? metaField = GetFieldByName("_meta") as igObjectRefMetaField;

			if (metaField != null)
			{
				metaField._default = this;
			}
		}

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