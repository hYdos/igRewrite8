using System.Diagnostics;

namespace igLibrary.Core
{
	/// <summary>
	/// List of modifications to make to the arkcore data, was used to support ssc ios before i realised it's futile
	/// </summary>
	[Obsolete]
	internal static class igArkCoreFixups
	{
		private static igMetaObject GetMetaSafe(string name, int expectedFieldNum)
		{
			igMetaObject? meta = igArkCore.GetObjectMeta(name);
			if(meta == null) throw new TypeLoadException($"Failed to load {name}");
			ErrorIfFieldCountMismatch(meta, expectedFieldNum);
			return meta;
		}
		private static igCompoundMetaFieldInfo GetCompoundSafe(string name)
		{
			igCompoundMetaFieldInfo? compound = igArkCore.GetCompoundFieldInfo(name);
			if(compound == null) throw new TypeLoadException($"Failed to load {name}");
			return compound;
		}
		private static void SetSizeAlign(igCompoundMetaFieldInfo compound, IG_CORE_PLATFORM platform, ushort size, ushort align)
		{
			igMetaFieldPlatformInfo? platformInfo = igArkCore.GetMetaFieldPlatformInfo(compound._name!);
			if(platformInfo == null) throw new TypeLoadException($"Failed to load {compound._name}");
			platformInfo._alignments[platform] = align;
			platformInfo._sizes[platform] = size;
		}
		private static void ErrorIfFieldCountMismatch(igMetaObject meta, int expectedFieldNum)
		{
			Debug.Assert(meta._metaFields.Count == expectedFieldNum, $"Expected {meta._name}._metaFields count to be {expectedFieldNum} before adding fields, instead got {meta._metaFields.Count}");
		}
		private static void ErrorIfFieldExists(igMetaObject meta, string fieldName)
		{
			if(meta.GetFieldByName(fieldName) != null) throw new InvalidOperationException($"{fieldName} was already added to the type!");
		}
		private static void ErrorIfFieldExists(igCompoundMetaFieldInfo compound, string fieldName)
		{
			if(compound.GetFieldByName(fieldName) != null) throw new InvalidOperationException($"{fieldName} was already added to the type!");
		}
		private static igMetaField GetFieldSafe(igMetaObject meta, string fieldName)
		{
			igMetaField? field = meta.GetFieldByName(fieldName);
			if(field == null) throw new InvalidOperationException($"{fieldName} was not added to the type!");
			return field;
		}
		private static T InstantiateAndAppendMetaField<T>(igCompoundMetaFieldInfo compound, int index, ushort offset, string name, object? defaultValue, params IG_CORE_PLATFORM[] platforms) where T : igMetaField, new()
		{
			ErrorIfFieldExists(compound, name);
			T metafield = new T();
			metafield._attributes.SetCapacity(platforms.Length);
			for(int i = 0; i < platforms.Length; i++)
			{
				metafield._attributes.Append(new igPlatformExclusiveAttribute(platforms[i]));
			}
			metafield._default = defaultValue;
			metafield._fieldName = name;
			metafield._offset = offset;
			metafield._parentMeta = compound;
			compound._fieldList.Insert(index, metafield);
			return metafield;
		}
		private static T InstantiateAndAppendMetaField<T>(igMetaObject meta, int index, ushort offset, string name, object? defaultValue, params IG_CORE_PLATFORM[] platforms) where T : igMetaField, new()
		{
			ErrorIfFieldExists(meta, name);
			T metafield = new T();
			metafield._attributes.SetCapacity(platforms.Length);
			for(int i = 0; i < platforms.Length; i++)
			{
				metafield._attributes.Append(new igPlatformExclusiveAttribute(platforms[i]));
			}
			metafield._default = defaultValue;
			metafield._fieldName = name;
			metafield._offset = offset;
			metafield._parentMeta = meta;
			meta.ApplyFixup(index, metafield);
			return metafield;
		}
		private static igMetaField AddPlatformExclusionAttr(igMetaObject meta, string name, params IG_CORE_PLATFORM[] platforms)
		{
			igMetaField field = GetFieldSafe(meta, name);
			if(field._attributes == null) field._attributes = new igObjectList();
			field._attributes.SetCapacity(field._attributes._capacity + platforms.Length);
			for(int i = 0; i < platforms.Length; i++)
			{
				field._attributes.Append(new igPlatformExclusionAttribute(platforms[i]));
			}
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
			_virtualDeadZoneDeflection._fieldName = "_virtualDeadZoneDeflection";
			_virtualDeadZoneDeflection._offset = 0x1D;	//in between the two surrounding fields
			_virtualDeadZoneDeflection._parentMeta = playerSystemDataMeta;
			playerSystemDataMeta.ApplyFixup(6, _virtualDeadZoneDeflection);
			ErrorIfFieldExists(playerSystemDataMeta, "_virtualWalkAndTurnStickThreshold");
			igFloatMetaField _virtualWalkAndTurnStickThreshold = new igFloatMetaField();
			_virtualWalkAndTurnStickThreshold._attributes.SetCapacity(2);
			_virtualWalkAndTurnStickThreshold._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_virtualWalkAndTurnStickThreshold._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			_virtualWalkAndTurnStickThreshold._default = 0.05f;
			_virtualWalkAndTurnStickThreshold._fieldName = "_virtualWalkAndTurnStickThreshold";
			_virtualWalkAndTurnStickThreshold._offset = 0x21;	//in between the two surrounding fields
			_virtualWalkAndTurnStickThreshold._parentMeta = playerSystemDataMeta;
			playerSystemDataMeta.ApplyFixup(8, _virtualWalkAndTurnStickThreshold);
			ErrorIfFieldExists(playerSystemDataMeta, "_virtualRunStickThreshold");
			igFloatMetaField _virtualRunStickThreshold = new igFloatMetaField();
			_virtualRunStickThreshold._attributes.SetCapacity(2);
			_virtualRunStickThreshold._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_virtualRunStickThreshold._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			_virtualRunStickThreshold._default = 0.3f;
			_virtualRunStickThreshold._fieldName = "_virtualRunStickThreshold";
			_virtualRunStickThreshold._offset = 0x25;	//in between the two surrounding fields
			_virtualRunStickThreshold._parentMeta = playerSystemDataMeta;
			playerSystemDataMeta.ApplyFixup(10, _virtualRunStickThreshold);
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

			igMetaObject guiBehaviorVehicleCustomizationMeta = GetMetaSafe("CGuiBehaviorVehicleCustomization", 111);
			ErrorIfFieldExists(guiBehaviorVehicleCustomizationMeta, "_changeCategoryText");
			igStringMetaField _changeCategoryText = new igStringMetaField();
			_changeCategoryText._attributes.SetCapacity(3);
			_changeCategoryText._attributes.Append(new igLocalizeAttribute());
			_changeCategoryText._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_changeCategoryText._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			_changeCategoryText._fieldName = "_changeCategoryText";
			_changeCategoryText._offset = 0xCD;	//in between the two surrounding fields
			_changeCategoryText._parentMeta = guiBehaviorVehicleCustomizationMeta;
			guiBehaviorVehicleCustomizationMeta.ApplyFixup(29, _changeCategoryText);
			ErrorIfFieldExists(guiBehaviorVehicleCustomizationMeta, "_dpadCycle");
			igStringMetaField _dpadCycle = new igStringMetaField();
			_dpadCycle._attributes.SetCapacity(2);
			_dpadCycle._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN));
			_dpadCycle._attributes.Append(new igPlatformExclusiveAttribute(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64));
			_dpadCycle._fieldName = "_dpadCycle";
			_dpadCycle._offset = 0x01A1;	//in between the two surrounding fields
			_dpadCycle._parentMeta = guiBehaviorVehicleCustomizationMeta;
			guiBehaviorVehicleCustomizationMeta.ApplyFixup(85, _dpadCycle);

			igMetaObject globalPlatformStrings = GetMetaSafe("CGlobalPlatformStrings", 221);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  53, 0x00, "_unableToConnectToCloud", "Unable to connect to iCloud. Retry or continue without saving.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  54, 0x00, "_unableToConnectToCloudTablet", "Unable to connect to iCloud. Retry or continue without iCloud.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  55, 0x00, "_continueWithoutCloudSave", "No iCloud", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  56, 0x00, "_iCloudTitle", "iCloud", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  57, 0x00, "_connectingToiCloud", "Connecting to iCloud", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  58, 0x00, "_conflictFromDeviceFormat", "From %s", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  59, 0x00, "_conflictPortalMasterRankFormat", "Portal Master Rank: %d", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  60, 0x00, "_conflictSaveSlotWithSaveFormat", "Save %d: Chapter %d", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  61, 0x00, "_conflictSaveSlotNoSaveFormat", "Save %d: -", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  62, 0x00, "_conflictPromptChooseCloud", "Use iCloud and overwrite local save?", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  63, 0x00, "_conflictPromptChooseLocal", "Use local save and overwrite iCloud?", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings,  73, 0x00, "_iCloudConflictSignInWarning", "Your iCloud account is unavailable. You need to be to signed in to save, load, and delete game files.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 203, 0x00, "_onlineMultiplayerOverCellularMessage", "Online multiplayer is not supported over cellular network.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 206, 0x00, "_contentDeploymentDownloadTitle", "Downloading Content", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 208, 0x00, "_contentDeploymentDownloadStartingTitle", "Preparing Download", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 212, 0x00, "_wifiConnectionRequiredTitle", "Network Error", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 213, 0x00, "_wifiConnectionRequiredMessage", "Wi-Fi required to download game content. (Error %d)", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 214, 0x00, "_contentDeploymentValidatingTitle", "Validating Content", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 215, 0x00, "_contentDeploymentValidatingMessage", "Validation %.2f %% complete", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 216, 0x00, "_contentDeploymentReservingTitle", "Reserving Cache Space", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 217, 0x00, "_contentDeploymentReservingMessage", "Reserve %.2f %% complete.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 218, 0x00, "_contentDeploymentOutOfDiskSpaceTitle", "Out of Storage", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 219, 0x00, "_contentDeploymentOutOfDiskSpaceMessage", "%llu more MB of storage\nrequired to install game.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 220, 0x00, "_contentDeploymentNeedMoreSpaceMessage", "There is not enough available storage to download game content. Please free up storage.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 221, 0x00, "_contentDeploymentRetryOption", "Retry", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 222, 0x00, "_contentDeploymentAbortOption", "Abort", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 223, 0x00, "_iCloudConflictSaveSlotTitle", "Save Slot Conflict!", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 224, 0x00, "_iCloudConflictOptionsTitle", "Player Stats Conflict!", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 240, 0x00, "_requiredUpdateTitle", "Update Required.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 241, 0x00, "_requiredUpdateMessage", "You must update your game before continuing.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 242, 0x00, "_useInstantTrophy", "Use Instant Trophy", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 243, 0x00, "_requiredGamepad", "Usage of ability upgrades requires an MFi game controller or Skylanders Game Controller.", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igStringMetaField>(globalPlatformStrings, 244, 0x00, "_comboOfTheDayCarouselItemName", "Daily Double", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			for(int i = 0; i < globalPlatformStrings._metaFields.Count; i++)
			{
				globalPlatformStrings._metaFields[i]._offset = (ushort)i;	//It's so complicated to manage these offsets that i might as well not bother
			}

			igMetaObject dialogBoxInfoMeta = GetMetaSafe("CDialogBoxInfo", 19);
			igRawRefMetaField _inputCallback = InstantiateAndAppendMetaField<igRawRefMetaField>(dialogBoxInfoMeta, 10, 0x0029, "_inputCallback", null, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);

			igMetaObject guiBehaviorCollectionMeta = GetMetaSafe("CGuiBehaviorCollection", 39);
			AddPlatformExclusionAttr(guiBehaviorCollectionMeta, "_collectionArchive", IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);

			igMetaObject cameraBaseMeta = GetMetaSafe("CCameraBase", 12);
			InstantiateAndAppendMetaField<igBoolMetaField>(cameraBaseMeta, 12, 285, "_isSelected", null, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);

			igCompoundMetaFieldInfo cameraModelComp = GetCompoundSafe("CCameraModelMetaField");
			InstantiateAndAppendMetaField<igFloatMetaField>(cameraModelComp, 22, 0xB1, "_mobileShadowLodDistance", 1.0f, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igFloatMetaField>(cameraModelComp, 23, 0xB2, "_mobileShadowRange", 1000.0f, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igFloatMetaField>(cameraModelComp, 24, 0xB3, "_mobileShadowHeightOffset", 1000.0f, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			InstantiateAndAppendMetaField<igFloatMetaField>(cameraModelComp, 25, 0xB4, "_mobileShadowFarPlane", 1500.0f, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			SetSizeAlign(cameraModelComp, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, 0xD0, 0x10);
			SetSizeAlign(cameraModelComp, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64, 0xD0, 0x10);

			igMetaObject guiBehaviorUpsellBase = GetMetaSafe("CGuiBehaviorUpsellBase", 24);
			InstantiateAndAppendMetaField<igBoolMetaField>(guiBehaviorUpsellBase, 24, 99, "_isPurchasing", null, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64)._properties._persistent = false;

			//unfinished
			igMetaObject guiBehaviorToypackUpsell = GetMetaSafe("CGuiBehaviorToyPackUpsell", 35);
			InstantiateAndAppendMetaField<igObjectRefMetaField>(guiBehaviorUpsellBase, 31, 99, "_comboPackOpenAnimation", null, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64)._properties._persistent = false;
			InstantiateAndAppendMetaField<igObjectRefMetaField>(guiBehaviorUpsellBase, 31, 99, "_comboPackIdleAnimation", null, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64)._properties._persistent = false;
			InstantiateAndAppendMetaField<igObjectRefMetaField>(guiBehaviorUpsellBase, 31, 99, "_comboPackCloseAnimation", null, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64)._properties._persistent = false;
		}
	}
}