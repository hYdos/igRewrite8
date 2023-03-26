using igLibrary.Core;
using igLibrary;
using igRewrite8.Devel;

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace igRewrite8
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			//ReadFromTextFile(args);

			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			List<igMetaObject> pendingTypes = igArkCore._pendingTypes;

			igObjectDirectory directory = new igObjectDirectory();
			FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read);
			igIGZLoader igzLoader = new igIGZLoader(directory, fs, false);

			igzLoader.Read(directory, false);
			igzLoader.ReadObjects();

			return;
		}
		private static void ReadFromTextFile(string[] args)
		{
			TextParser parser = new TextParser();
			parser.ReadMetaEnumFile(args[0]);
			parser.ReadMetaFieldFile(args[1]);
			parser.ReadMetaObjectFile(args[2]);
			
			List<igMetaEnum> enums = igArkCore._metaEnums;
			List<igMetaFieldPlatformInfo> platformInfos = igArkCore._metaFieldPlatformInfos;
			List<igMetaObject> objects = igArkCore._metaObjects;

			igArkCore.WriteToFile(igArkCore.EGame.EV_SkylandersSuperchargers);
			
			igArkCore.Reset();
		}
	}
}