using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igRangedFloatMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igRangedFloat data = new igRangedFloat();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x08;
		public override Type GetOutputType() => typeof(igRangedFloat);
	}
	public class igRangedFloatArrayMetaField : igRangedFloatMetaField
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