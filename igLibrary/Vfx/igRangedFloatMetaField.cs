using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igRangedFloatMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igRangedFloat data = new igRangedFloat(loader._stream.ReadSingle(), loader._stream.ReadSingle());
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igRangedFloat data = (igRangedFloat)value;
			section._sh.WriteSingle(data._min);
			section._sh.WriteSingle(data._max);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x08;
		public override Type GetOutputType() => typeof(igRangedFloat);
	}
	public class igRangedFloatArrayMetaField : igRangedFloatMetaField
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