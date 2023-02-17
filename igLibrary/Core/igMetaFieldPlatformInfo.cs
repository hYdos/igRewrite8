namespace igLibrary.Core
{
	public class igMetaFieldPlatformInfo
	{
		public string _name;
		public Dictionary<IG_CORE_PLATFORM, ushort> _sizes = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public Dictionary<IG_CORE_PLATFORM, ushort> _alignments = new Dictionary<IG_CORE_PLATFORM, ushort>();
	}
}