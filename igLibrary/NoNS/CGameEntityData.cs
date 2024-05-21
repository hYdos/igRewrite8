namespace igLibrary
{
	public class CGameEntityData : CEntityData
	{
		public uint _gameEntityFlags = 512;
		public DistanceCullImportance _distanceCullImportance = DistanceCullImportance.kMedium; 
		public ETeamFilterLayers _collisionLayer = ETeamFilterLayers.eTFL_Entity;
		public ECharacterCollisionPriority _collisionPriority = ECharacterCollisionPriority.eCCP_None;
		public string _modelName;
		public string _skinName;
		public igObject _materialOverrides;
		public igHandle _collisionMaterial;
		public bool _castsShadows = true;
		public EMobileShadowState _mobileShadowState;
		public float _lifetime;
		public float _lifetimeModifier;
		public EMemoryPoolID _cachedAssetPool = EMemoryPoolID.MP_MAX_POOL;
		public EMaterial _material;	//technically a property but that doesn't work well here
	}
}