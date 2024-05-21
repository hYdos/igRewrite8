namespace igLibrary
{
	public class CEntityPrecacher : CResourcePrecacher
	{
		[Obsolete("This exists for the reflection system, do not use.")] public static igVector<igObjectDirectory> _directories = new igVector<igObjectDirectory>();
		[Obsolete("This exists for the reflection system, do not use.")] public static object? _currentlyLoadingZone;    //CZone
		public override void Precache(string filePath)
		{
			//Unimplemented
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}