namespace igLibrary.Core
{
	public class igIGZLoader
	{
		public List<string> _stringList = new List<string>();
		public List<Type> _vtableList = new List<Type>();
		public Dictionary<ulong, igObject> _offsetObjectList = new Dictionary<ulong, igObject>();
		public List<ushort> _metaSizes = new List<ushort>();
		public List<string> _vtableNameList = new List<string>();
		//public List<igHandle> _externalList = new List<igHandle>();
		//public List<igHandle> _namedHandleList = new List<igHandle>();
		//public List<igHandle> _unresolvedNames = new List<igHandle>();
		//public igObjectList _namedExternalList = new igObjectList();
		//public List<igMemory> _thumbnails = new List<igMemory>();
		//public igRuntimeFields _runtimeFields = new igRuntimeFields();
		public uint _version;
		public uint _metaObjectVersion;
		public IG_CORE_PLATFORM _platform;
		public uint _numFixups;
		//public igFileDescriptor _file;
		public StreamHelper _stream;
		//public igObjectDirectory _dir;
		public uint _fixups;
		public uint[] _loadedPointers = new uint[0x1F];
		public ulong nameListOffset;
		private bool _readDependancies;

		public igIGZLoader(igObjectDirectory dir, Stream stream, bool readDependancies)
		{
			
		}
	}
}