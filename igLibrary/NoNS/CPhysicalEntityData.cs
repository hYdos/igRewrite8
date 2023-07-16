namespace igLibrary
{
	public class CPhysicalEntityData : CGameEntityData
	{
		public uint _physicalEntityFlags = 128;
		public int _health = 100;
		public int _healthMax = 100;
		public EVulnerability _vulnerability = EVulnerability.eV_Invulnerable;

		public CPhysicalEntityData()
		{
			_entityFlags = 2879488;
			_gameEntityFlags = 512;
		}
	}
}