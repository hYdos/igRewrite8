using igLibrary.Gfx;

namespace igLibrary.Core
{
	public class igRegistry : igObject
	{
		public IG_CORE_PLATFORM _platform;
		public IG_GFX_PLATFORM _gfxPlatform;
		private static igRegistry _instance;
		public static igRegistry GetRegistry()
		{
			if(_instance == null) _instance = new igRegistry();
			return _instance;
		}
	}
}