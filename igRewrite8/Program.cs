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

			igMetaObject hashTable = igArkCore.GetObjectMeta("igHashTable");
			return;
		}
	}
}