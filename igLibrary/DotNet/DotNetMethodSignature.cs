namespace igLibrary.DotNet
{
	public class DotNetMethodSignature : igObject
	{
		public uint _flags;
		public DotNetType _retType;
		public DotNetTypeList _parameters = new DotNetTypeList();
		public DotNetMethodMeta _methodMeta = new DotNetMethodMeta();
	}
}