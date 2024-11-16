namespace igLibrary.Core
{
	//I didn't know this class existed when I named this lib
	public class igLibrary : igNamedObject
	{
		public object? _registerFunction;
		public object? _registerEnumsFunction;
		public object? _registerFunctionParametersFunction;
		public object? _libraryFunction;
		public int _version;
		public bool _codeCanBeUnloaded;
	}
}