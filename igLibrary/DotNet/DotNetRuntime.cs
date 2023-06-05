namespace igLibrary.DotNet
{
	public class DotNetRuntime : igObject
	{
		//Commented out ones aren't useful for us

		//public igDotNetDebugger _debugger;
		public string _prefix;
		//public DotNetThreadList _allocatedThreads;
		//bool _traceActive;
		//igVector<uint> _trace;
		//int _traceHead;
		//igStringIntHashTable _ignoreExceptions;
		public igDotNetMetaInterface _methodLookup;
	}
}