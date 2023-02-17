using System.Reflection;

namespace igLibrary.Core
{
	public class igRefMetaField : igMetaField
	{
		public bool _construct;
		public bool _destruct;
		public bool _reconstruct;
		public bool _refCounted;

		//TODO: Add ark data for this

		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
	}
}