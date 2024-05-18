namespace igLibrary
{
	public class CCombatSoundData : igObject
	{
		public bool _playAttackSoundOnVictim = true; 
		public string _attackSound = null;
		public string _victimSound = "COM_Impact_Victim_Basic";
		public igHandle _attackSoundHandle;		//It's a CSound
		public igHandle _victimSoundHandle;		//It's a CSound
		public CUpgradeableValue _attackSoundUpgradeable;	//This SHOULD be CUpgradeableSound but hotdog's combat file has an instance that references a CUpgradeableVfx object
		public CUpgradeableValue _victimSoundUpgradeable;	//This SHOULD be CUpgradeableSound but hotdog's combat file has an instance that references a CUpgradeableVfx object
	}
}