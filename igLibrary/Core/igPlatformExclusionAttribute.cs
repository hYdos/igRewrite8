namespace igLibrary.Core
{
	//This is responsible for excluding fields from a platform.
	// For example, the _vehiclePersonalizationSets field in CVehicleSystemData
	public class igPlatformExclusionAttribute : igObject
	{
		public IG_CORE_PLATFORM _value;
		public igPlatformExclusionAttribute(){}
		public igPlatformExclusionAttribute(IG_CORE_PLATFORM platform)
		{
			_value = platform;
		}
	}
}