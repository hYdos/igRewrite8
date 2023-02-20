using igLibrary.Core;

namespace igLibrary.Math
{
	public class igMatrix44fMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igMatrix44f data = new igMatrix44f();
			data._m11 = loader._stream.ReadSingle();
			data._m12 = loader._stream.ReadSingle();
			data._m13 = loader._stream.ReadSingle();
			data._m14 = loader._stream.ReadSingle();
			data._m21 = loader._stream.ReadSingle();
			data._m22 = loader._stream.ReadSingle();
			data._m23 = loader._stream.ReadSingle();
			data._m24 = loader._stream.ReadSingle();
			data._m31 = loader._stream.ReadSingle();
			data._m32 = loader._stream.ReadSingle();
			data._m33 = loader._stream.ReadSingle();
			data._m34 = loader._stream.ReadSingle();
			data._m41 = loader._stream.ReadSingle();
			data._m42 = loader._stream.ReadSingle();
			data._m43 = loader._stream.ReadSingle();
			data._m44 = loader._stream.ReadSingle();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x10;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x40;
		public override Type GetOutputType() => typeof(igMatrix44f);
	}
	public class igMatrix44fArrayMetaField : igMatrix44fMetaField
	{
		public short _num;
		public override object? ReadIGZField(igIGZLoader loader)
		{
			Array data = Array.CreateInstance(base.GetOutputType(), _num);
			for(int i = 0; i < _num; i++)
			{
				data.SetValue(base.ReadIGZField(loader), i);
			}
			return data;
		}
		public override uint GetSize(IG_CORE_PLATFORM platform)
		{
			return base.GetSize(platform) * (uint)_num;
		}
		public override Type GetOutputType()
		{
			return base.GetOutputType().MakeArrayType();
		}
	}
}