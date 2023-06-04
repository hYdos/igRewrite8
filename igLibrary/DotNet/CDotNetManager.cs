using igLibrary.DotNet;

namespace igLibrary
{
	public class CDotNetaManager //: CManager
	{
		//public igObjectDirectoryLoadCallback _objectDirectoryUnloadedCallback;
		//public igUnsignedIntStringHashTable _functionHashes;
		//public CDotNetLibraryHashTable _libraries;
		public DotNetRuntime _runtime = new DotNetRuntime();
		//public igObject _dotNetCommunicator;
		//public bool _debuggerEnabled;
		private static CDotNetaManager fakeInstance = new CDotNetaManager();
		public static CDotNetaManager _Instance => fakeInstance;

	}
}