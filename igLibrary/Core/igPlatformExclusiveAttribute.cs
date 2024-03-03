namespace igLibrary.Core
{
	//This is responsible for marking fields as platform exclusive.
	// For example, the _virtualWalkAndTurnStickThreshold and _virtualRunStickThreshold fields in CPlayerSystemData
	public class igPlatformExclusiveAttribute : igObject
	{
		public IG_CORE_PLATFORM _value;
		public igPlatformExclusiveAttribute(){}
		public igPlatformExclusiveAttribute(IG_CORE_PLATFORM platform)
		{
			_value = platform;
		}
	}
}