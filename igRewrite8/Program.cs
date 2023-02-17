using igLibrary.Core;
using igLibrary;

namespace igRewrite8
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			List<igMetaObject> metaObjects = igArkCore._metaObjects;
			List<igMetaEnum> metaEnums = igArkCore._metaEnums;

			ReadMetaFieldList();

			List<igMetaFieldPlatformInfo> platformInfos = igArkCore._metaFieldPlatformInfos;

			igArkCore.WriteToFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			/*igObjectDirectory directory = new igObjectDirectory();
			FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read);
			igIGZLoader igzLoader = new igIGZLoader(directory, fs, false);

			igzLoader.Read(directory, false);	
			igzLoader.ReadObjects();*/
			return;
		}
		public static void ReadMetaFieldList()
		{
			StreamHelper fieldSh = new StreamHelper(new FileStream("metafields.txt", FileMode.Open, FileAccess.Read));

			List<IG_CORE_PLATFORM> platformLookup = new List<IG_CORE_PLATFORM>();
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX);

			while(true)
			{
				string metaFieldLine = fieldSh.ReadLine();
				if(metaFieldLine.Length == 0) break;

				string[] members = metaFieldLine.Split(' ');


				int index = 0;
				igMetaFieldPlatformInfo platformInfo = new igMetaFieldPlatformInfo();
				platformInfo._name = members[index++];
				while(true)
				{
					index++;	//Skip "psa"
					if(index > members.Length) break;
					IG_CORE_PLATFORM platform = platformLookup[int.Parse(members[index++], System.Globalization.NumberStyles.HexNumber)];
					ushort size = ushort.Parse(members[index++], System.Globalization.NumberStyles.HexNumber);
					ushort alignment = ushort.Parse(members[index++], System.Globalization.NumberStyles.HexNumber);

					platformInfo._sizes.Add(platform, size);
					platformInfo._alignments.Add(platform, alignment);
				}

				igArkCore._metaFieldPlatformInfos.Add(platformInfo);
			}
		}
	}
}