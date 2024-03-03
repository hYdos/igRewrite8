using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System;

namespace igLibrary.Core
{
	public static class igArkCore
	{
		public enum EGame
		{
			EV_ZooCube,
			EV_HootersRoadTrip,
			EV_DogsPlayingPoker,
			EV_EnigmaRisingTide,
			EV_CrashNitroKart,
			EV_SpiderMan2,
			EV_LupinSenseiColumbusNoIsanWaAkeNiSomaru,
			EV_YuGiOhTheDawnOfDestiny,
			EV_GraffitiKingdom,
			EV_XMenLegends,
			EV_GradiusV,
			EV_ShamanKingPowerOfSpirit,
			EV_UltimateSpiderMan,
			EV_XMenLegendsIIRiseOfApocalypse,
			EV_TonyHawksAmericanSk8land,
			EV_DigimonWorld4,
			EV_SpiderManBattleForNewYork,
			EV_MarvelUltimateAlliance,
			EV_TonyHawksDownhillJam,
			EV_TransformersAutobots,
			EV_TransformersDecpticons,
			EV_TonyHawksProvingGround,
			EV_ShrekTheThird,
			EV_BeautfilKatamari,
			EV_LupinSenseiLupinNiWaShiOZenigataNiWaKoiO,
			EV_SpiderMan3_DS,
			EV_WanganMidnightMaximumTune3,
			EV_BackyardBasketball2007,
			EV_SpiderMan3_HC,
			EV_OperationDarkness,
			EV_MadagascarTMEscape2AfricaTMTheGameTM,
			EV_SkylandersSpyrosAdventure,
			EV_SkylandersSpyrosAdventure_3DS,
			EV_HatsuneMikuProjectDiva,
			EV_HatsuneMikuProjectDiva2nd,
			EV_HatsuneMikuProjectDivaExtend,
			EV_SkylandersBattlegrounds,
			EV_SkylandersCloudPatrol,
			EV_SkylandersGiants,
			EV_SkylandersGiants_3DS,
			EV_SkylandersLostIslands,
			EV_SkylandersSwapForce,
			EV_SkylandersSwapForce_3DS,
			EV_SkylandersTrapTeam,
			EV_SkylandersTrapTeam_3DS,
			EV_SkylandersSuperchargers,
			EV_SkylandersImaginators,
			EV_CrashNSaneTrilogy,
			EV_CrashTeamRacingNitroFueled,
			EV_Count
		}

		public const uint _magicCookie = 0x41726B00;
		public const uint _magicVersion = 0x01;
		public const string ArkCoreFolder = "ArkCore";
		public const string dynamicMetaObjectNS = "igLibrary.Gen.MetaObject";
		public const string dynamicMetaEnumNS = "igLibrary.Gen.MetaEnum";
		public const string dynamicCompoundFieldNS = "igLibrary.Gen.CompoundField";

		public static List<igMetaEnum> _metaEnums = new List<igMetaEnum>();
		public static List<igMetaObject> _metaObjects = new List<igMetaObject>();
		public static List<igCompoundMetaFieldInfo> _compoundFieldInfos = new List<igCompoundMetaFieldInfo>();
		public static List<igMetaFieldPlatformInfo> _metaFieldPlatformInfos = new List<igMetaFieldPlatformInfo>();

		private static Dictionary<string, Type>? _vTableCache = null;
		private static Dictionary<string, Type>? _compoundStructCache = null;
		private static Dictionary<string, Type>? _enumCache = null;

		private static AssemblyBuilder? _dynamicTypeAssembly;
		private static ModuleBuilder? _dynamicTypeModule;

		private static Dictionary<string, TypeBuilder>? _dynamicTypes = new Dictionary<string, TypeBuilder>();
		private static Dictionary<string, Type>? _dynamicStructs = new Dictionary<string, Type>();

		public static List<igBaseMeta> _pendingTypes = new List<igBaseMeta>();

		private static void FixupClasses(EGame game)
		{
			string funcName = game.ToString();
			MethodInfo? func = typeof(igArkCoreFixups).GetMethod(funcName.ReplaceBeginning("EV_", ""));
			func?.Invoke(null, null);
		}
		public static void WriteToFile(EGame game)
		{
			FixupClasses(game);
			igArkCoreFile saver = new igArkCoreFile();
			saver.BeginSave($"{ArkCoreFolder}/{game.ToString()}.ark");
			for(int i = 0; i < _metaEnums.Count; i++)
			{
				saver.SaveMetaEnum(_metaEnums[i]);
			}
			for(int i = 0; i < _metaObjects.Count; i++)
			{
				saver.SaveMetaObject(_metaObjects[i]);
			}
			for(int i = 0; i < _compoundFieldInfos.Count; i++)
			{
				saver.SaveCompoundInfo(_compoundFieldInfos[i]);
			}
			for(int i = 0; i < _metaFieldPlatformInfos.Count; i++)
			{
				saver.SaveMetaFieldPlatformInfo(_metaFieldPlatformInfos[i]);
			}
			saver.FinishSave();
			saver.Dispose();
		}
		public static void DebugDumpDynamicTypes()
		{
			if(_dynamicTypeAssembly != null)
			{
				throw new NotImplementedException("Building assemblies isn't yet a thing.");
			}
		}
		public static void Reset()
		{
			_metaEnums.Clear();
		}
		public static void ReadFromFile(EGame game)
		{
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

			stopwatch.Start();

			igArkCoreFile loader = new igArkCoreFile();
			loader.ReadFile($"{ArkCoreFolder}/{game.ToString()}.ark");
			loader.Dispose();

			stopwatch.Stop();

			Console.WriteLine($"Loading and generating all types took {stopwatch.Elapsed.TotalSeconds} seconds");
		}
		public static igMetaObject? GetObjectMeta(string name)
		{
			if(name == null) return null;
			//TODO: Optimise this
			int index = _metaObjects.FindIndex(x => x._name == name);
			if(index < 0) return null;
			else return _metaObjects[index];
		}
		public static igMetaFieldPlatformInfo? GetMetaFieldPlatformInfo(string name)
		{
			if(name == null) return null;
			
			int index = _metaFieldPlatformInfos.FindIndex(x => x._name == name);
			if(index < 0) return null;
			else return _metaFieldPlatformInfos[index];
		}
		public static Type? GetStructDotNetType(string name)
		{
			CheckAndInitializeCaches();

			if(_compoundStructCache.ContainsKey(name))
			{
				return _compoundStructCache[name];
			}
			else if(_dynamicStructs.ContainsKey(name))
			{
				return _compoundStructCache[name];
			}
			else return null;
		}
		public static Type? GetObjectDotNetType(string name)
		{
			CheckAndInitializeCaches();

			if(_vTableCache.ContainsKey(name))
			{
				return _vTableCache[name];
			}
			else if(_dynamicTypes.ContainsKey(name))
			{
				return _dynamicTypes[name];
			}
			else return null;
		}
		private static void CheckAndInitializeCaches()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			if(_vTableCache == null)
			{
				Type[] types = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsAssignableTo(typeof(__internalObjectBase))).ToArray();
				_vTableCache = new Dictionary<string, Type>();
				for(uint i = 0; i < types.Length; i++)
				{
					_vTableCache.Add(types[i].Name, types[i]);
				}
			}

			if(_compoundStructCache == null)
			{
				Type[] types = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsValueType && !x.IsEnum && x.GetCustomAttribute<igStruct>() != null).ToArray();
				_compoundStructCache = new Dictionary<string, Type>();
				for(uint i = 0; i < types.Length; i++)
				{
					string typeName = types[i].Name;
					int backtickIndex = typeName.IndexOf('`');
					if(backtickIndex >= 0) typeName = typeName.Substring(0, backtickIndex);
					_compoundStructCache.Add(typeName, types[i]);
				}
			}

			if(_enumCache == null)
			{
				Type[] types = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsEnum && x.Namespace != null && x.Namespace.StartsWith("ig")).ToArray();
				_enumCache = new Dictionary<string, Type>();
				for(uint i = 0; i < types.Length; i++)
				{
					_enumCache.Add(types[i].Name, types[i]);
				}
			}
		}
		public static Type? GetEnumDotNetType(string name)
		{
			CheckAndInitializeCaches();

			if(_enumCache.ContainsKey(name))
			{
				return _enumCache[name];
			}
			else
			{
				return null;
			}
		}
		public static igMetaEnum? GetMetaEnum(string name)
		{
			if(name == null) return null;
			
			int index = _metaEnums.FindIndex(x => x._name == name);
			if(index < 0) return null;
			else return _metaEnums[index];
		}
		public static igCompoundMetaFieldInfo? GetCompoundFieldInfo(string name)
		{
			if(_compoundFieldInfos == null) return null;
			
			int index = _compoundFieldInfos.FindIndex(x => x._name == name);
			if(index < 0) return null;
			else return _compoundFieldInfos[index];			
		}
		public static igMetaField? GetFieldMetaForObject(string handle)
		{
			string[] args = handle.Split("::");
			igMetaObject? targetMeta = GetObjectMeta(args[0]);
			if(targetMeta == null) return null;
			return targetMeta.GetFieldByName(args[1]);
		}
		private static void CreateDynamicModule()
		{
			AssemblyName dynamicAssemblyName = new AssemblyName("ArkGeneratedTypes");

			_dynamicTypeAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.RunAndCollect);
			_dynamicTypeModule = _dynamicTypeAssembly.DefineDynamicModule(dynamicAssemblyName.Name);
		}
		public static EnumBuilder GetNewEnumBuilder(string name)
		{
			if(_dynamicTypeModule == null)
			{
				CreateDynamicModule();
			}

			return _dynamicTypeModule.DefineEnum($"{dynamicMetaEnumNS}.{name}", TypeAttributes.Public, typeof(int));
		}
		public static TypeBuilder GetNewStructTypeBuilder(string name)
		{
			if(_dynamicTypeModule == null)
			{
				CreateDynamicModule();
			}

			TypeBuilder tb = _dynamicTypeModule.DefineType($"{dynamicCompoundFieldNS}.{name}", TypeAttributes.Public, typeof(ValueType));

			_dynamicStructs.Add(name, tb);

			return tb;
		}
		public static TypeBuilder GetNewTypeBuilder(string name)
		{
			if(_dynamicTypeModule == null)
			{
				CreateDynamicModule();
			}

			TypeBuilder tb = _dynamicTypeModule.DefineType($"{dynamicMetaObjectNS}.{name}", TypeAttributes.AutoClass | TypeAttributes.Public);

			_dynamicTypes.Add(name, tb);

			return tb;
		}
		public static void AddDynamicTypeToCache(Type type)
		{
			CheckAndInitializeCaches();


			if(_dynamicTypes.ContainsKey(type.Name))
			{
				_dynamicTypes.Remove(type.Name);
			}

			if(!_vTableCache.ContainsKey(type.Name))
			{
				_vTableCache.Add(type.Name, type);
			}
		}
		public static void GeneratePendingTypes()
		{
			for(int i = 0; i < _pendingTypes.Count; i++)
			{
				_pendingTypes[i].DefineType();
			}
			_pendingTypes = _pendingTypes.OrderByDescending(x => (int)x._priority).ToList();
			for(int i = 0; i < _pendingTypes.Count; i++)
			{
				_pendingTypes[i].FinalizeType();
			}
			_pendingTypes.Clear();
		}
		public static void FlushPendingTypes()
		{
			for(int i = 0; i < _pendingTypes.Count; i++)
			{
				_pendingTypes[i].DefineType2();
			}
			for(int i = 0; i < _pendingTypes.Count; i++)
			{
				_pendingTypes[i].CreateType2();
			}
			_pendingTypes.Clear();
		}
	}
}