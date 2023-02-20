using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec4ucMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec4uc data = new igVec4uc();
			data._r = loader._stream.ReadByte();
			data._g = loader._stream.ReadByte();
			data._b = loader._stream.ReadByte();
			data._a = loader._stream.ReadByte();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x01;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x04;
		public override Type GetOutputType() => typeof(igVec4uc);
	}
	public class igVec4ucArrayMetaField : igVec4ucMetaField
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