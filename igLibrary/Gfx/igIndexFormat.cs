namespace igLibrary.Gfx
{
	public class igIndexFormat : igObject
	{
		public IG_INDEX_TYPE _indexType;
		public IG_GFX_PLATFORM _platform;
		public uint _headerSize;
		public uint _alignment;
		public bool _hasRestartIndices;
		public bool _dynamic;
		public string _indexFormatNamespace;
		public igObjectList _indexFormats;
	}
}