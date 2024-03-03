using System.Diagnostics;

namespace igLibrary.Core
{
	internal static class igArkCoreFixups
	{
		private static igMetaObject GetMetaSafe(string name, int expectedFieldNum)
		{
			igMetaObject? meta = igArkCore.GetObjectMeta(name);
			if(meta == null) throw new TypeLoadException($"Failed to load {name}");
			ErrorIfFieldCountMismatch(meta, expectedFieldNum);
			return meta;
		}
		private static void ErrorIfFieldCountMismatch(igMetaObject meta, int expectedFieldNum)
		{
			Debug.Assert(meta._metaFields.Count == expectedFieldNum, $"Expected {meta._name}._metaFields count to be {expectedFieldNum} before adding fields, instead got {meta._metaFields.Count}");
		}
		private static void ErrorIfFieldExists(igMetaObject meta, string fieldName)
		{
			if(meta.GetFieldByName(fieldName) != null) throw new InvalidOperationException($"{fieldName} was already added to the type!");
		}
		private static igMetaField GetFieldSafe(igMetaObject meta, string fieldName)
		{
			igMetaField? field = meta.GetFieldByName(fieldName);
			if(field == null) throw new InvalidOperationException($"{fieldName} was not added to the type!");
			return field;
		}
		public static void SkylandersSuperchargers()
		{
			igMetaObject playerSystemDataMeta = GetMetaSafe("CPlayerSystemData", 22);
			ErrorIfFieldExists(playerSystemDataMeta, "_virtualDeadZoneDeflection");
			igFloatMetaField _virtualDeadZoneDeflection = new igFloatMetaField();
			_virtualDeadZoneDeflection._attributes.SetCapacity(2);
			_virtualDeadZoneDeflection._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_virtualDeadZoneDeflection._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			_virtualDeadZoneDeflection._default = 0.05f;
			_virtualDeadZoneDeflection._name = "_virtualDeadZoneDeflection";
			_virtualDeadZoneDeflection._offset = 0x1D;	//in between the two surrounding fields
			_virtualDeadZoneDeflection._parentMeta = playerSystemDataMeta;
			playerSystemDataMeta._metaFields.Insert(6, _virtualDeadZoneDeflection);
			ErrorIfFieldExists(playerSystemDataMeta, "_virtualWalkAndTurnStickThreshold");
			igFloatMetaField _virtualWalkAndTurnStickThreshold = new igFloatMetaField();
			_virtualWalkAndTurnStickThreshold._attributes.SetCapacity(2);
			_virtualWalkAndTurnStickThreshold._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_virtualWalkAndTurnStickThreshold._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			_virtualWalkAndTurnStickThreshold._default = 0.05f;
			_virtualWalkAndTurnStickThreshold._name = "_virtualWalkAndTurnStickThreshold";
			_virtualWalkAndTurnStickThreshold._offset = 0x21;	//in between the two surrounding fields
			_virtualWalkAndTurnStickThreshold._parentMeta = playerSystemDataMeta;
			playerSystemDataMeta._metaFields.Insert(8, _virtualWalkAndTurnStickThreshold);
			ErrorIfFieldExists(playerSystemDataMeta, "_virtualRunStickThreshold");
			igFloatMetaField _virtualRunStickThreshold = new igFloatMetaField();
			_virtualRunStickThreshold._attributes.SetCapacity(2);
			_virtualRunStickThreshold._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_virtualRunStickThreshold._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			_virtualRunStickThreshold._default = 0.3f;
			_virtualRunStickThreshold._name = "_virtualRunStickThreshold";
			_virtualRunStickThreshold._offset = 0x25;	//in between the two surrounding fields
			_virtualRunStickThreshold._parentMeta = playerSystemDataMeta;
			playerSystemDataMeta._metaFields.Insert(10, _virtualRunStickThreshold);
			ErrorIfFieldCountMismatch(playerSystemDataMeta, 25);

			igMetaObject vehicleSystemDataMeta = GetMetaSafe("CVehicleSystemData", 26);
			igMetaField _vehiclePersonalizationSets = GetFieldSafe(vehicleSystemDataMeta, "_vehiclePersonalizationSets");
			_vehiclePersonalizationSets._attributes.SetCapacity(2);
			_vehiclePersonalizationSets._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_vehiclePersonalizationSets._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			igMetaField _vehiclePersonalizationColorScheme = GetFieldSafe(vehicleSystemDataMeta, "_vehiclePersonalizationColorScheme");
			_vehiclePersonalizationColorScheme._attributes.SetCapacity(2);
			_vehiclePersonalizationColorScheme._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_vehiclePersonalizationColorScheme._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			igMetaField _vehiclePersonalizationToppers = GetFieldSafe(vehicleSystemDataMeta, "_vehiclePersonalizationToppers");
			_vehiclePersonalizationToppers._attributes.SetCapacity(2);
			_vehiclePersonalizationToppers._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_vehiclePersonalizationToppers._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			igMetaField _vehiclePersonalizationNeon = GetFieldSafe(vehicleSystemDataMeta, "_vehiclePersonalizationNeon");
			_vehiclePersonalizationNeon._attributes.SetCapacity(2);
			_vehiclePersonalizationNeon._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_vehiclePersonalizationNeon._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			igMetaField _vehiclePersonalizationTaunts = GetFieldSafe(vehicleSystemDataMeta, "_vehiclePersonalizationTaunts");
			_vehiclePersonalizationTaunts._attributes.SetCapacity(2);
			_vehiclePersonalizationTaunts._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_vehiclePersonalizationTaunts._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			igMetaField _vehiclePersonalizationBoosts = GetFieldSafe(vehicleSystemDataMeta, "_vehiclePersonalizationBoosts");
			_vehiclePersonalizationBoosts._attributes.SetCapacity(2);
			_vehiclePersonalizationBoosts._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_vehiclePersonalizationBoosts._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));

			igMetaObject virtualTagSettingsMeta = GetMetaSafe("CVirtualTagSettings", 2);
			igMetaField _virtualTagList = GetFieldSafe(virtualTagSettingsMeta, "_virtualTagList");
			_virtualTagList._attributes.SetCapacity(2);
			_virtualTagList._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_virtualTagList._attributes.Append(new igPlatformExclusionAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
		}
	}
}