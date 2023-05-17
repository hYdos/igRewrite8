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
		public int _texHandle;
		public ulong _lockedMemory;
		public bool _oglDiscardOriginalImage;
		public static bool _makeAbstract;
		public static bool _makeConcrete;
		public igVec4f _colorScale;
		public igVec4f _colorBias;
		public igObject _graphicsHelper;
	}
}
