namespace igLibrary.DotNet
{
	public class DotNetLibrary : igObject
	{
		public DotNetRuntime _runtime;
		public StreamHelper _stringTable;
		public DotNetMethodDefinitionList _methodRefs = new DotNetMethodDefinitionList();
		public DotNetMethodDefinitionList _methodDefs = new DotNetMethodDefinitionList();
		public igBaseMetaList _ownedTypes = new igBaseMetaList();
		public igBaseMetaList _wrappedTypes = new igBaseMetaList();
		public DotNetTypeList _referencedTypes = new DotNetTypeList();
		public DotNetFieldDefinitionList _staticFields = new DotNetFieldDefinitionList();
		public igMetaFieldList _fields = new igMetaFieldList();
		public igDotNetMetaObjectList _patchClasses = new igDotNetMetaObjectList();
	}
	public class NonRefCountedDotNetLibraryList : igTObjectList<DotNetLibrary>{}	//Technically inherits fro igTNonRefCountedObjectList but in this library there's no point to have that distinction
}