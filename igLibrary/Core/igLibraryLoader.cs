namespace igLibrary.Core
{
	public class igLibraryLoader : igObjectLoader
	{
		public override string GetLoaderType() => "Library";
		public override string GetLoaderName() => "Dynamically Loaded Library";
	}
}