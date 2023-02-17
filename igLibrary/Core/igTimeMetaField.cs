namespace igLibrary.Core
{
	public class igTimeMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadUInt32();
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 4;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 4;
		public override Type GetOutputType() => typeof(uint);	//Should be changed
	}
	public class igTimeArrayMetaField : igTimeMetaField
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