namespace igLibrary.Gfx
{
	public unsafe struct igImageLevel
	{
		public byte* _imageData;
		public uint _imageSize;
		public igMetaImage _targetMeta;
		public uint _width;
		public uint _height;
		public uint _levelsImageProduct;
		public uint _levelCount;
		public uint _imageCount;
	}
}