namespace igLibrary.Core
{
	public class igRegistry : igObject
	{
		public IG_CORE_PLATFORM _platform;
		private static igRegistry _instance;
		public static igRegistry GetRegistry()
		{
			if(_instance == null) _instance = new igRegistry();
			return _instance;
		}
	}
}