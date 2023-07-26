namespace igLibrary.DotNet
{
	public class igDotNetMetaOnlyLibraryLoader : igLibraryLoader
	{
		public DotNetRuntime _runtime;
		public static object _loadCallback;
		public override string GetLoaderExtension() => "vvl";
		public override string GetLoaderType() => "DotNet";
		public override void ReadFile(string filePath, igBlockingType blockingType)
		{
			_runtime = CDotNetaManager._Instance._runtime;
			DotNetLibrary lib = VvlLoader.Load(filePath, _runtime, out bool success);
		}
	}
}