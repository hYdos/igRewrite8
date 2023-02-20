using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec3dMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec3d data = new igVec3d();
			data._x = loader._stream.ReadDouble();
			data._y = loader._stream.ReadDouble();
			data._z = loader._stream.ReadDouble();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x08;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x18;
		public override Type GetOutputType() => typeof(igVec3d);
	}
	public class igVec3dArrayMetaField : igVec3dMetaField
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