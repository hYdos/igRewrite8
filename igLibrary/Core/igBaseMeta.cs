namespace igLibrary.Core
{
	public class igBaseMeta
	{
		public string? _name;
		internal Type _internalType;

		public virtual void PostUndump(){}

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