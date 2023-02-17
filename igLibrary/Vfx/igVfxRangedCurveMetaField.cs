using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igVfxRangedCurveMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVfxRangedCurve data = new igVfxRangedCurve();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x54;
		public override Type GetOutputType() => typeof(igVfxRangedCurve);
	}
	public class igVfxRangedCurveArrayMetaField : igVfxRangedCurveMetaField
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