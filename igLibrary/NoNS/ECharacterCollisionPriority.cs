namespace igLibrary
{
	public enum ECharacterCollisionPriority
	{
		eCCP_NonGameplayCritical = 0,
		eCCP_None = 1,
		eCCP_Low = 2,
		eCCP_NormalSelfPushable = 3,
		eCCP_Normal = 4,
		eCCP_JumpingPlayer = 5,
		eCCP_High = 6,
		eCCP_Charger = 7,
		eCCP_PlayerTraversable = 8,
		eCCP_Immobile = 9,
	};
}