using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec2fMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec2f data = new igVec2f();
			data._x = loader._stream.ReadSingle();
			data._y = loader._stream.ReadSingle();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x08;
		public override Type GetOutputType() => typeof(igVec2f);
	}
	public class igVec2fArrayMetaField : igVec2fMetaField
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