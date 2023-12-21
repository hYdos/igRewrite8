namespace igLibrary
{
	public class CBehaviorAssetPrecacher : CResourcePrecacher
	{
        public override void Precache(string filePath)
        {
			//Unimplemented
            igObjectStreamManager.Singleton.Load(filePath);
        }
    }
}