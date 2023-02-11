using igLibrary.Core;

namespace igRewrite8
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			List<igMetaObject> metaObjects = igArkCore._metaObjects;
			List<igMetaEnum> metaEnums = igArkCore._metaEnums;

			igObjectDirectory directory = new igObjectDirectory();
			FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read);
			igIGZLoader igzLoader = new igIGZLoader(directory, fs, false);

			igzLoader.Read(directory, false);	
			igzLoader.ReadObjects();		
			return;
		}
	}
}