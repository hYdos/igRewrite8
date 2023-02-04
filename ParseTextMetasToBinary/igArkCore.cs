namespace PTMTB
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
		public static string ArkCoreFolder = "ArkCore";

		public static List<igMetaEnum> _metaEnums = new List<igMetaEnum>();
		public static List<igMetaObject> _metaObjects = new List<igMetaObject>();

		//This isn't real and ideally shouldn't exist but it's probably the best way to handle compound fields
		public static List<igCompoundMetaFieldInfo> _compoundFieldInfos = new List<igCompoundMetaFieldInfo>();

		/*public static void WriteToFile2(EGame game)
		{
			igArkCoreSaver saver = new igArkCoreSaver($"{ArkCoreFolder}/{game.ToString()}.ark", game);
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
				saver.SaveCompoundMetaFieldFieldList(_compoundFieldInfos[i]);
			}
			saver.FinishSave();
		}*/
		public static void WriteToFile2(EGame game)
		{
			igArkCoreSaver2 saver = new igArkCoreSaver2();
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
			saver.FinishSave();
		}
		public static void Reset()
		{
			_metaEnums.Clear();
		}
		public static void ReadFromFile(EGame game)
		{
			/*igArkCoreLoader loader = new igArkCoreLoader($"{ArkCoreFolder}/{game.ToString()}.ark", game);
			for(uint i = 0; i < loader._metaEnumCount; i++)
			{
				igMetaEnum? metaEnum = loader.LoadMetaEnum();
				_metaEnums.Add(metaEnum);
			}
			loader.LoadMetaObjects(_metaObjects);*/
		}
		public static void ReadFromFile2(EGame game)
		{
			igArkCoreSaver2 loader = new igArkCoreSaver2();
			loader.ReadFile($"{ArkCoreFolder}/{game.ToString()}.ark");
			_metaObjects.AddRange(loader._metaObjectsInFile);
			_metaEnums.AddRange(loader._metaEnumsInFile);
			_compoundFieldInfos.AddRange(loader._compoundsInFile);
		}
		public static igMetaObject? GetObjectMeta(string name)
		{
			if(name == null) return null;
			
			int index = _metaObjects.FindIndex(x => x._name == name);
			if(index < 0) return null;
			else return _metaObjects[index];
		}
		public static igMetaEnum? GetMetaEnum(string name)
		{
			if(name == null) return null;
			
			int index = _metaEnums.FindIndex(x => x._name == name);
			if(index < 0) return null;
			else return _metaEnums[index];
		}
	}
}