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
			//DumpToyDataToRunes(args); return;
			//DumpAllArchives(args); return;
			//ReadFromTextFile(args); return;

			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			igFileContext.Singleton.Initialize(args[0]);

			TypeBuilder item = igArkCore.GetNewTypeBuilder("notAComponent");
			TypeBuilder list = igArkCore.GetNewTypeBuilder("notAComponentList");
			TypeBuilder reference = igArkCore.GetNewTypeBuilder("notAnEntity");

			item.SetParent(typeof(igObject));
			list.SetParent(typeof(igTObjectList<>).MakeGenericType(item));

			item.DefineField("_refToEntity", reference, FieldAttributes.Public);
			reference.DefineField("_refToItem", list, FieldAttributes.Public);

			//item has to come before list

			reference.CreateType();
			item.CreateType();
			list.CreateType();

			return;
		}
		private static void DumpToyDataToRunes(string[] args)
		{
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			igFileContext.Singleton.Initialize(args[0]);

			igArchive arc = new igArchive("archives/permanent.pak");

			//igFileContext.Singleton.Open("packages:/generated/packageXmls/permanent_pkg.igz", igFileContext.GetOpenFlags(FileAccess.Read, FileMode.Open), out igFileDescriptor fd, igBlockingType.kBlocking, igFileWorkItem.Priority.kPriorityNormal);

			igObjectDirectory dir = igObjectStreamManager.Singleton.Load("packages:/generated/packageXmls/permanent_pkg.igz");

			List<igObject> toydata = new List<igObject>();

			igMetaObject toydataType = igArkCore.GetObjectMeta("CToyData");

			igStringRefList list = (igStringRefList)dir._objectList[0];

			StreamWriter f = File.CreateText("runes.csv");
			for(int i = 0; i < list._count; i += 2)
			{
				if(list[i] == "igx_file")
				{
					if(list[i+1].Contains("toydata"))
					{
						igObjectDirectory toydataDir = igObjectStreamManager.Singleton.Load(list[i+1]);
						for(int j = 0; j < toydataDir._objectList._count; j++)
						{
							if(toydataDir._objectList[j].GetMeta().CanBeAssignedTo(toydataType))
							{
								toydata.Add(toydataDir._objectList[j]);
							}
						}
					}
				}
			}

			FieldInfo? ttField = toydataType._vTablePointer.GetField("_toyId");
			FieldInfo? tnField = toydataType._vTablePointer.GetField("_toyName");
			for(int i = 0; i < toydata.Count; i++)
			{
				f.WriteLine($"{tnField.GetValue(toydata[i])},{(int)ttField.GetValue(toydata[i])}");
			}
			f.Close();
			return;
			//igFileContext.Singleton.Open()
		}
		private static void DumpAllArchives(string[] args)
		{
			igFileContext.Singleton.Initialize(args[0]);
			DirectoryInfo di = new DirectoryInfo(args[0]);
			FileInfo[] archiveFilePaths = di.GetFiles("*.iga*");
			for(int i = 0; i < archiveFilePaths.Length; i++)
			{
				string fileName = Path.GetFileName(archiveFilePaths[i].FullName);
				igArchive arc = new igArchive(fileName);
				for(int j = 0; j < arc.fileHeaders.Length; j++)
				{
					string outputPath = Path.Combine(args[1], arc.fileHeaders[j].fullName);
					Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
					FileStream fs = File.Create(outputPath);
					arc.ExtractFile(j, fs);
					fs.Flush();
					fs.Close();
				}
			}
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