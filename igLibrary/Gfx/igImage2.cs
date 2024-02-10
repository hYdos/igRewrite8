using igLibrary.Core;
using igLibrary.Math;

namespace igLibrary.Gfx
{
	public class igImage2 : igNamedObject
	{
		public ushort _width;
		public ushort _height;
		public ushort _depth;
		public ushort _levelCount;
		public ushort _imageCount;
		public igMetaImage _format;
		public int _usageDeprecated;
		public ushort _paddingDeprecated;
		public igMemory<byte> _data;
		public int _lockCount;
		public int _texHandle = -1;
		public ulong _lockedMemory;
		public bool _oglDiscardOriginalImage = true;
		public static bool _makeAbstract;
		public static bool _makeConcrete;
		public igVec4f _colorScale = new igVec4f(1, 1, 1, 1);
		public igVec4f _colorBias;
		public igObject? _graphicsHelper = null;

		public uint GetTextureLevelOffset(int targetLevel, int targetImage)
		{
			if(_format == null) return 0xFFFFFFFF;
			return _format.GetTextureLevelOffset(_width, _height, _depth, _levelCount, _imageCount, targetLevel, targetImage);
		}
	}
}
