using igLibrary.Core;

namespace igLibrary.Gfx
{
	public class igMetaImage : igObject
	{
		public string _name;
		public igMetaImage _canonical;
		public byte _bitsPerPixel;
		public byte _properties;
		public igMetaImageList _formats;	//Technically an igNonRefCountedMetaImageList
		//public igImage2ConvertFunctionList _functions;
		public bool _isTile => 			((_properties >> 0) & 1) != 0;
		public bool _isCanonical => 	((_properties >> 1) & 1) != 0;
		public bool _isCompressed => 	((_properties >> 2) & 1) != 0;
		public bool _hasPalette => 		((_properties >> 3) & 1) != 0;
		public bool _isSrgb => 			((_properties >> 4) & 1) != 0;
		public bool _isFloatingPoint => ((_properties >> 5) & 1) != 0;
	}
	public class igMetaImageList : igTObjectList<igMetaImage>{}
}