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

		public override object? ReadIGZField(igIGZLoader loader)
		{
			int raw = loader._stream.ReadInt32();
			if(_metaEnum != null) return Enum.ToObject(_metaEnum._internalType, raw);
			else                  return raw;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			if(_metaEnum == null) section._sh.WriteInt32((int)value);

			int raw = _metaEnum.GetValueFromEnum(value);
			section._sh.WriteInt32(raw);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 4;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 4;
		public override Type GetOutputType() => _metaEnum == null ? typeof(int) : _metaEnum._internalType;
		//I don't wanna deal with these two
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpDefault(saver, sh);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpDefault(loader, sh);
		}
	}
	public class igEnumArrayMetaField : igEnumMetaField
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
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			Array data = (Array)value;
			for(int i = 0; i < _num; i++)
			{
				base.WriteIGZField(saver, section, data.GetValue(i));
			}
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