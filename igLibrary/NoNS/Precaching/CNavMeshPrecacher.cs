namespace igLibrary
{
	public class CNavMeshPrecacher : CResourcePrecacher
	{
        public override void Precache(string filePath)
        {
			//Unimplemented
            igObjectStreamManager.Singleton.Load(filePath);
        }
    }
}