namespace igLibrary.DotNet
{
	public class DotNetMethodSignature : igObject
	{
		public uint _flags;
		public DotNetType _retType;
		public DotNetTypeList _parameters = new DotNetTypeList();
		public DotNetMethodMeta _methodMeta = new DotNetMethodMeta();

		public enum FlagTypes
		{
			StaticMethod = 0x04,
			Constructor = 0x08,
			AbstractMethod = 0x20,
			RuntimeImplMethod = 0x40,
			NoSpecializationCopyMethod = 0x80
		}
	}
}