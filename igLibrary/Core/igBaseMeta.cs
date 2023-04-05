namespace igLibrary.Core
{
	public class igBaseMeta : igObject
	{
		public string? _name;
		internal Type _internalType;
		protected bool _beganFinalization = false;
		protected bool _beganFinalizationPrep = false;
		protected bool _finishedFinalization = false;
		protected bool _finishedFinalizationPrep = false;

		public virtual void PostUndump(){}

		public virtual igMetaField? GetFieldByName(string name) => null;

		//The following will be used to generate only the types that need to be loaded
		public virtual void TypeBuild()
		{
			TypeBuildDeclare();
		}
		protected virtual void TypeBuildDeclare(){}
		protected virtual void TypeBuildDefine(){}
		protected virtual void TypeBuildFinish(){}
	}
}