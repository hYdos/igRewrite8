namespace igLibrary
{
	public class CAudioArchive : igObject
	{
		public string _name = "NOT_SET";
		public igObject _soundList;
		public igObject _settingsOverrideList;
		public LoadContext _protectionCategory = LoadContext.kContextNone;
		public string _protectionName;
		public bool _isStreamed;
		public bool _loadInVram;
		public bool _isGenerated;
		public bool _isLocalized;
		public bool _default;
		public bool _isCollisionBank;
		public bool _isLowMemoryBank = true;
		public bool _loaded;
		public bool _sorted;
		public igArchive _archive;
		public igFileDescriptor _fileDescriptor;
		public ulong _sizeLoaded;
		public ulong _sizeCouldntLoad;
		public bool _mute;
		public bool _solo;
	}
}