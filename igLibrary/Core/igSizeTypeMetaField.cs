namespace igLibrary.Core
{
	public class igSizeTypeMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			if(igAlchemyCore.isPlatform64Bit(loader._platform)) return loader._stream.ReadUInt64();
			else                                                return (ulong)loader._stream.ReadUInt32();
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override Type GetOutputType() => typeof(ulong);
	}
	public class igSizeTypeArrayMetaField : igSizeTypeMetaField
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