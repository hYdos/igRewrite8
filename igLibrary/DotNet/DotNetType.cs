using igLibrary.Core;

namespace igLibrary.DotNet
{
	public struct DotNetType
	{
		public igBaseMeta? _baseMeta;
		public uint _flags;
		//public ElementType _elementType;

		public enum Flags : uint
		{
			kIsSimple = 0x40000000,
			kIsArray  = 0x80000000,
			kTypeMask = 0x000000FF,
		}

		public DotNetType()
		{
			_baseMeta = null;
			_flags = (uint)ElementType.kElementTypeVoid;
		}
	}
	public class DotNetTypeList : igTDataList<DotNetType>{}
}