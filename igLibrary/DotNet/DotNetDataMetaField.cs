using igLibrary.Core;

namespace igLibrary.DotNet
{
	public class DotNetDataMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			DotNetData data = new DotNetData();
			//No idea
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform) * 2;
		public override Type GetOutputType() => typeof(DotNetData);
	}
	public class DotNetDataArrayMetaField : DotNetDataMetaField
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