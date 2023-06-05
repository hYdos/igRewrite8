namespace igLibrary.Core
{
	public class igLibraryLoader : igObjectLoader
	{
		public virtual string GetType() => "Library";
		public virtual string GetName() => "Dynamically Loaded Library";
	}
}