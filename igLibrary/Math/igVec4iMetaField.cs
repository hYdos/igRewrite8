using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec4iMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec4i data = new igVec4i();
			data._x = loader._stream.ReadInt32();
			data._y = loader._stream.ReadInt32();
			data._z = loader._stream.ReadInt32();
			data._w = loader._stream.ReadInt32();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x10;
		public override Type GetOutputType() => typeof(igVec4i);
	}
	public class igVec4iArrayMetaField : igVec4iMetaField
	{
		short _num;
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