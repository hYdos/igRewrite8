namespace igLibrary.Core
{
	public class igUnsignedCharMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadByte();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteByte((byte)value);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 1;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 1;
		public override Type GetOutputType() => typeof(byte);
	}
	public class igUnsignedCharArrayMetaField : igUnsignedCharMetaField
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