namespace igLibrary.Core
{
	public class igUnsignedLongMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadUInt64();
		public override uint GetAlignment(IG_CORE_PLATFORM platform)
		{
			if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN || platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX) return 4;
			return 8;
		}
		public override uint GetSize(IG_CORE_PLATFORM platform) => 8;
		public override Type GetOutputType() => typeof(ulong);
	}
	public class igUnsignedLongArrayMetaField : igUnsignedLongMetaField
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