namespace igLibrary.Core
{
	//TODO: Define dependency providers for
	// -  igComponentDependencyProvider
	// -  CZoneInfoDependencyProvider
	// -  CWorldEntityDataDependencyProvider
	// -  CCRMSystemDataDependencyProvider
	// -  CStaticComponentDependencyProvider
	// -  CStaticEntityDataDependencyProvider
	// -  CEdcInfoDependencyProvider
	// -  CQuestInteractionCutsceneOnLoadDependencyProvider
	// -  CObjectiveDependencyProvider
	// -  CActorDataDependencyProvider
	// -  CCutsceneActorDataDependencyProvider
	// -  CQuestSystemDataDependencyProvider
	// -  CPortalMasterPerkDependencyProvider
	// -  CWorldDataDependencyProvider
	// -  CLevelBorderComponentDataDependencyProvider
	// -  CVehicleSystemDataDependencyProvider
	// -  CStaticCollisionComponentDependencyProvider
	// -  CVehiclePersonalizationDataDependencyProvider
	// -  CAudioArchiveDependencyProvider
	public class igDependencyProvider : igObject
	{
		public IG_CORE_PLATFORM _platform;
		public igObjectDirectory _directory;
		public virtual void GetFileDependencies(igObject obj, out igStringRefList? output){output = null;}
		public virtual void GetBuildDependencies(igObject obj, out igStringRefList? output){output = null;}
	}
}