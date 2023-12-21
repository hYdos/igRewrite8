namespace igLibrary
{
	public class CScriptPrecacher : CResourcePrecacher
	{
        public override void Precache(string filePath)
        {
			if(!CDotNetaManager._Instance.IsLibraryLoaded(filePath))
			{
				CDotNetaManager._Instance.LoadScript(filePath, mDestMemoryPoolId, false);
			}
        }
    }
}