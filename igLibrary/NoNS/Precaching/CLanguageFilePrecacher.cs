namespace igLibrary
{
	public class CLanguageFilePrecacher : CResourcePrecacher
	{
		[Obsolete("This exists for the reflection system, do not use.")] public static object? _loadedCallback;         //igObjectDirectoryLoadCallback
		[Obsolete("This exists for the reflection system, do not use.")] public static object? _localizedNamespaceData; //igStringObjectDirectoryHashTable
		public override void Precache(string filePath)
		{
			//Unimplemented
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}