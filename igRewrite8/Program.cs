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

			igFileContext.Singleton.Initialize(args[0]);

			List<igMetaObject> pendingTypes = igArkCore._pendingTypes;

			//igObjectDirectory directory = igObjectStreamManager.Singleton.Load(args[1], false);

			igArchive arc = new igArchive(File.Open("F:/GAMES/BLES02172-[Skylanders SuperChargers]/PS3_GAME/USRDIR/archives/permanent_ps3.pak", FileMode.Open));

			igFileContext.Singleton.Open(args[1], 0, out igFileDescriptor fd, igBlockingType.kBlocking, igFileWorkItem.Priority.kPriorityNormal);

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