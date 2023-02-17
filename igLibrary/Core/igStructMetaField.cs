namespace igLibrary.Core
{
	public class igStructMetaField : igMetaField
	{
		public Dictionary<IG_CORE_PLATFORM, uint> _sizes;
		public Dictionary<IG_CORE_PLATFORM, uint> _alignements;
	}
	public class igStructArrayMetaField : igStructMetaField
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
	}}