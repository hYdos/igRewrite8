namespace igLibrary.Core
{
	public class igStructMetaField : igMetaField
	{
		public Dictionary<IG_CORE_PLATFORM, uint> _sizes;
		public Dictionary<IG_CORE_PLATFORM, uint> _alignements;

		public override object? ReadIGZField(igIGZLoader loader) => null;
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 1;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0;
		public override Type GetOutputType() => typeof(int);
	}
	public class igStructArrayMetaField : igStructMetaField
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
	}}