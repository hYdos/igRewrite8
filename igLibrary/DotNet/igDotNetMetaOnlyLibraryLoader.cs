namespace igLibrary.DotNet
{
	public class igDotNetMetaOnlyLibraryLoader : igLibraryLoader
	{
		public DotNetRuntime _runtime;
		public static object _loadCallback;
		public override string GetExtension() => "vvl";
		public override string GetType() => "DotNet";
	}
}