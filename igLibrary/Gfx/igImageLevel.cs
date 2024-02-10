namespace igLibrary.Gfx
{
	public struct igImageLevel
	{
		public igMemory<byte> _imageData;
		public igMetaImage _targetMeta;
		public uint _width;
		public uint _height;
		public uint _levelsAndImagesSize;
		public uint _levelCount;
		public uint _imageCount;
	}
}