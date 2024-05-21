namespace igLibrary.DotNet
{
	public class DotNetRuntime : igObject
	{
		//Commented out ones aren't useful for us

		[Obsolete("This exists for the reflection system, do not use.")] public object? _debugger;             //igDotNetDebugger
		public string _prefix;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _allocatedThreads;     //DotNetThreadList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _traceActive;          //bool
		[Obsolete("This exists for the reflection system, do not use.")] public object? _trace;                //igVector<uint>
		[Obsolete("This exists for the reflection system, do not use.")] public object? _traceHead;            //int
		[Obsolete("This exists for the reflection system, do not use.")] public object? _ignoreExceptions;     //igStringIntHashTable
		public igDotNetMetaInterface _methodLookup;
	}
}