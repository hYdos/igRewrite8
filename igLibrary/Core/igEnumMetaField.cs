using System.Reflection;

namespace igLibrary.Core
{
	//Should define DotNet::igDotNetEnumMetaField but it's not too important as it doesn't appear in igzs
	public class igEnumMetaField : igMetaField
	{
		public igMetaEnum _metaEnum;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaEnum == null ? null : _metaEnum._name);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaEnum = igArkCore.GetMetaEnum(loader.ReadString(sh));
		}
	}
	public class igEnumArrayMetaField : igEnumMetaField
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