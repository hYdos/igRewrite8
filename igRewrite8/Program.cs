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
			DumpToyDataToRunes(args); return;
			//DumpAllArchives(args); return;
			//ReadFromTextFile(args); return;
			//TestArchives(args); return;

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
		struct ToyData
		{
			public int _toyId;
			public LocalizedString _toyName;
			public int _elementType;
			public List<Variant> _variants;
			public struct Variant
			{
				public int _decoId;
				public int _yearCode;
				public bool _lightcore;
				public bool _fullAltDeco;
				public bool _wowPow;
				public LocalizedString _variantText;
				public LocalizedString _toyName;
			}
			public struct LocalizedString
			{
				public string _da;
				public string _de;
				public string _en;
				public string _es;
				public string _fi;
				public string _fr;
				public string _it;
				public string _mx;
				public string _nl;
				public string _no;
				public string _pt;
				public string _sv;
				public LocalizedString(igObjectDirectory[]? dirs, igObject obj, string fieldName, string defaultValue)
				{
					_da = defaultValue;
					_de = defaultValue;
					_en = defaultValue;
					_es = defaultValue;
					_fi = defaultValue;
					_fr = defaultValue;
					_it = defaultValue;
					_mx = defaultValue;
					_nl = defaultValue;
					_no = defaultValue;
					_pt = defaultValue;
					_sv = defaultValue;
					if(dirs == null) return;
					_en = GetLocalizedString(dirs[0], obj, fieldName) ?? defaultValue;
					_fr = GetLocalizedString(dirs[1], obj, fieldName) ?? defaultValue;
					_it = GetLocalizedString(dirs[2], obj, fieldName) ?? defaultValue;
					_de = GetLocalizedString(dirs[3], obj, fieldName) ?? defaultValue;
					_es = GetLocalizedString(dirs[4], obj, fieldName) ?? defaultValue;
					_mx = GetLocalizedString(dirs[5], obj, fieldName) ?? defaultValue;
					_nl = GetLocalizedString(dirs[6], obj, fieldName) ?? defaultValue;
					_da = GetLocalizedString(dirs[7], obj, fieldName) ?? defaultValue;
					_sv = GetLocalizedString(dirs[8], obj, fieldName) ?? defaultValue;
					_fi = GetLocalizedString(dirs[9], obj, fieldName) ?? defaultValue;
					_no = GetLocalizedString(dirs[10], obj, fieldName) ?? defaultValue;
					_pt = GetLocalizedString(dirs[11], obj, fieldName) ?? defaultValue;
				}
			}
		}
		private static void DumpToyDataToRunes(string[] args)
		{
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			igAlchemyCore.InitializeSystems();
			igFileContext.Singleton.Initialize(args[0]);
			igFileContext.Singleton.InitializeUpdate(args[1]);

			igFileContext.Singleton.LoadArchive("data:/archives/loosefiles.pak");

			CPrecacheManager._Instance.PrecachePackage($"generated/shaders/shaders_ps3", EMemoryPoolID.MP_DEFAULT);

			CPrecacheManager._Instance.PrecachePackage($"generated/packageXmls/permanent_ps3", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/essentialui", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/legal", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/gamestartup", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanentdeveloper", EMemoryPoolID.MP_DEFAULT);

			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/maps/zoneinfos", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent_2015", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_Mobile", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_FrontEnd", EMemoryPoolID.MP_DEFAULT);

			igObjectDirectory dir = igObjectStreamManager.Singleton.Load("packages:/generated/packageXmls/permanent_pkg.igz");

			List<ToyData> toydata = new List<ToyData>();

			igMetaObject toydataType = igArkCore.GetObjectMeta("CToyData")!;
			igMetaObject vidListType = igArkCore.GetObjectMeta("CVariantIdentifierList")!;
			igMetaObject vidType = igArkCore.GetObjectMeta("CVariantIdentifier")!;
			igMetaObject markerType = igArkCore.GetObjectMeta("igLocalizedInfo")!;

			FieldInfo ttField = toydataType._vTablePointer.GetField("_toyId")!;
			FieldInfo tnField = toydataType._vTablePointer.GetField("_toyName")!;
			FieldInfo etField = toydataType._vTablePointer.GetField("_elementType")!;
			FieldInfo varField = toydataType._vTablePointer.GetField("_variants")!;

			FieldInfo decoField = vidType._vTablePointer.GetField("_decoId")!;
			FieldInfo ycField = vidType._vTablePointer.GetField("_yearCode")!;
			FieldInfo lcField = vidType._vTablePointer.GetField("_lightCoreFlag")!;
			FieldInfo fadField = vidType._vTablePointer.GetField("_fullAltDecoFlag")!;
			FieldInfo wowField = vidType._vTablePointer.GetField("_wowPowFlag")!;
			FieldInfo varTxtField = vidType._vTablePointer.GetField("_variantText")!;
			FieldInfo varTnField = vidType._vTablePointer.GetField("_toyName")!;

			igStringRefList list = (igStringRefList)dir._objectList[0];

			StreamWriter f = File.CreateText("runes.yaml");
			f.WriteLine("list:");
			for(int i = 0; i < list._count; i += 2)
			{
				if(list[i] == "igx_file")
				{
					if(list[i+1].Contains("toydata"))
					{
						igObjectDirectory toydataDir = igObjectStreamManager.Singleton.Load(list[i+1]);
						igObjectDirectory[]? langDirs = null;
						if(toydataDir.GetObjectByType(markerType._vTablePointer) != null)
						{
							langDirs = new igObjectDirectory[12];
							string[] langPaths = new string[12];
							string basePath = list[i+1].Remove(list[i+1].Length - 4);
							langPaths[0] = basePath + "_en.lng";
							langPaths[1] = basePath + "_fr.lng";
							langPaths[2] = basePath + "_it.lng";
							langPaths[3] = basePath + "_de.lng";
							langPaths[4] = basePath + "_es.lng";
							langPaths[5] = basePath + "_mx.lng";
							langPaths[6] = basePath + "_nl.lng";
							langPaths[7] = basePath + "_da.lng";
							langPaths[8] = basePath + "_sv.lng";
							langPaths[9] = basePath + "_fi.lng";
							langPaths[10] = basePath + "_no.lng";
							langPaths[11] = basePath + "_pt.lng";
							for(int l = 0; l < langDirs.Length; l++)
							{
								langDirs[l] = igObjectStreamManager.Singleton.Load(langPaths[l])!;
							}
						}

						for(int j = 0; j < toydataDir!._objectList._count; j++)
						{
							if(toydataDir!._objectList[j].GetMeta().CanBeAssignedTo(toydataType))
							{
								ToyData td = new ToyData();
								td._toyName = new ToyData.LocalizedString(langDirs, toydataDir!._objectList[j], "_toyName", (string?)tnField.GetValue(toydataDir!._objectList[j]));
								td._toyId = (int)ttField.GetValue(toydataDir!._objectList[j]);
								td._elementType = (int)etField.GetValue(toydataDir!._objectList[j]);
								td._variants = new List<ToyData.Variant>();
								IigDataList variants = (IigDataList)varField.GetValue(toydataDir!._objectList[j])!;
								for(int v = 0; v < (variants == null ? 0 : variants.GetCount()); v++)
								{
									ToyData.Variant variant = new ToyData.Variant();
									object varObj = variants!.GetObject(v);
									variant._decoId = (int)decoField.GetValue(varObj)!;
									variant._yearCode = (int)ycField.GetValue(varObj)!;
									variant._lightcore = (bool)lcField.GetValue(varObj)!;
									variant._fullAltDeco = (bool)fadField.GetValue(varObj)!;
									variant._wowPow = (bool)wowField.GetValue(varObj)!;
									variant._variantText = new ToyData.LocalizedString(langDirs, (igObject)varObj, "_variantText", (string?)varTxtField.GetValue(varObj));
									variant._toyName = new ToyData.LocalizedString(langDirs, (igObject)varObj, "_toyName", (string?)varTnField.GetValue(varObj));
									td._variants.Add(variant);
								}
								toydata.Add(td);
							}
						}
					}
				}
			}

			toydata = toydata.OrderBy(x => x._toyId).ToList();

			for(int i = 0; i < toydata.Count; i++)
			{
				f.WriteLine($"  - toyId: {toydata[i]._toyId}");
				f.WriteLine($"    toyName:");
				f.WriteLine($"      en: {toydata[i]._toyName._en}");
				f.WriteLine($"      fr: {toydata[i]._toyName._fr}");
				f.WriteLine($"      it: {toydata[i]._toyName._it}");
				f.WriteLine($"      de: {toydata[i]._toyName._de}");
				f.WriteLine($"      es: {toydata[i]._toyName._es}");
				f.WriteLine($"      mx: {toydata[i]._toyName._mx}");
				f.WriteLine($"      nl: {toydata[i]._toyName._nl}");
				f.WriteLine($"      da: {toydata[i]._toyName._da}");
				f.WriteLine($"      sv: {toydata[i]._toyName._sv}");
				f.WriteLine($"      fi: {toydata[i]._toyName._fi}");
				f.WriteLine($"      no: {toydata[i]._toyName._no}");
				f.WriteLine($"      pt: {toydata[i]._toyName._pt}");
				f.WriteLine($"    element: {toydata[i]._elementType}");
				//f.WriteLine($"{(int)ttField.GetValue(toydata[i])},{(variants == null ? 0 : variants.GetCount())},{tnField.GetValue(toydata[i])}");
				f.WriteLine($"    variants:");
				for(int j = 0; j < toydata[i]._variants.Count; j++)
				{
					f.WriteLine($"    - decoId: {toydata[i]._variants[j]._decoId}");
					f.WriteLine($"      yearCode: {toydata[i]._variants[j]._yearCode}");
					f.WriteLine($"      lightcore: {toydata[i]._variants[j]._lightcore}");
					f.WriteLine($"      fullAltDeco: {toydata[i]._variants[j]._fullAltDeco}");
					f.WriteLine($"      wowPow: {toydata[i]._variants[j]._wowPow}");
					f.WriteLine($"      variantText:");
					f.WriteLine($"        en: {toydata[i]._variants[j]._variantText._en}");
					f.WriteLine($"        fr: {toydata[i]._variants[j]._variantText._fr}");
					f.WriteLine($"        it: {toydata[i]._variants[j]._variantText._it}");
					f.WriteLine($"        de: {toydata[i]._variants[j]._variantText._de}");
					f.WriteLine($"        es: {toydata[i]._variants[j]._variantText._es}");
					f.WriteLine($"        mx: {toydata[i]._variants[j]._variantText._mx}");
					f.WriteLine($"        nl: {toydata[i]._variants[j]._variantText._nl}");
					f.WriteLine($"        da: {toydata[i]._variants[j]._variantText._da}");
					f.WriteLine($"        sv: {toydata[i]._variants[j]._variantText._sv}");
					f.WriteLine($"        fi: {toydata[i]._variants[j]._variantText._fi}");
					f.WriteLine($"        no: {toydata[i]._variants[j]._variantText._no}");
					f.WriteLine($"        pt: {toydata[i]._variants[j]._variantText._pt}");
					f.WriteLine($"      toyName:");
					f.WriteLine($"        en: {toydata[i]._variants[j]._toyName._en}");
					f.WriteLine($"        fr: {toydata[i]._variants[j]._toyName._fr}");
					f.WriteLine($"        it: {toydata[i]._variants[j]._toyName._it}");
					f.WriteLine($"        de: {toydata[i]._variants[j]._toyName._de}");
					f.WriteLine($"        es: {toydata[i]._variants[j]._toyName._es}");
					f.WriteLine($"        mx: {toydata[i]._variants[j]._toyName._mx}");
					f.WriteLine($"        nl: {toydata[i]._variants[j]._toyName._nl}");
					f.WriteLine($"        da: {toydata[i]._variants[j]._toyName._da}");
					f.WriteLine($"        sv: {toydata[i]._variants[j]._toyName._sv}");
					f.WriteLine($"        fi: {toydata[i]._variants[j]._toyName._fi}");
					f.WriteLine($"        no: {toydata[i]._variants[j]._toyName._no}");
					f.WriteLine($"        pt: {toydata[i]._variants[j]._toyName._pt}");
				}
			}
			f.Close();
			return;
		}
		private static string? GetLocalizedString(igObjectDirectory dir, igObject obj, string fieldName)
		{
			Type t = obj.GetType();
			FieldInfo fi = t.GetField(fieldName);
			for(int j = 0; j < dir._objectList._count; j++)
			{
				if(dir._objectList[j] is igLocalizedStringDataList localData)
				{
					for(int l = 0; l < localData._count; l++)
					{
						igObject target = localData![l]._object.GetObjectAlias<igObject>()!;
						if(obj != target) continue;
						FieldInfo stringField = t.GetField(localData![l]._field._name!)!;
						if(stringField != fi) continue;
						return localData![l]._string;
					}
				}
			}
			return null;
		}
		private static void DumpAllArchives(string[] args)
		{
			igFileContext.Singleton.Initialize(args[0]);
			DirectoryInfo di = new DirectoryInfo(args[0]);
			FileInfo[] archiveFilePaths = di.GetFiles("*.iga*");
			for(int i = 0; i < archiveFilePaths.Length; i++)
			{
				string fileName = Path.GetFileName(archiveFilePaths[i].FullName);
				igArchive arc = igFileContext.Singleton.LoadArchive(fileName);
				for(int j = 0; j < arc._files.Count; j++)
				{
					string outputPath = Path.Combine(args[1], arc._files[j]._logicalName);
					Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
					FileStream fs = File.Create(outputPath);
					arc.Decompress(arc._files[j], fs);
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
		private static void TestArchives(string[] args)
		{
			igFileContext.Singleton.Initialize(args[0]);
			igArchive archive = new igArchive();
			archive.Open("archives/permanent.backup.pak", igBlockingType.kMayBlock);
			for(int i = 0; i < archive._files.Count; i++)
			{
				MemoryStream uwu = new MemoryStream();
				archive.Decompress(archive._files[0], uwu);
				archive.Compress(archive._files[0], uwu);
				uwu.Close();
			}
			archive.Save("test.pak");
			return;
		}
	}
}