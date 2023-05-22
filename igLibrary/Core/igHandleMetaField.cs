using System.Reflection;

namespace igLibrary.Core
{
	public class igHandleMetaField : igRefMetaField
	{
		public igMetaObject _metaObject;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaObject._name);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaObject = igArkCore.GetObjectMeta(loader.ReadString(sh));
		}
		public override object ReadIGZField(igIGZLoader loader)
		{
			if(!loader._runtimeFields._handles.Contains(loader._stream.Tell64())) return null;

			uint raw = loader._stream.ReadUInt32();
			if((raw & 0x80000000) != 0)
			{
				return loader._namedHandleList[(int)(raw & 0x3FFFFFFF)];
			}
			else
			{
				return loader._externalList[(int)(raw & 0x3FFFFFFF)];
			}
		}

		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			if(value == null) return;

			int handleIndex = saver._namedHandleList.FindIndex(x => x == value);
			if(handleIndex < 0)
			{
				handleIndex = saver._namedHandleList.Count;
				saver._namedHandleList.Add((igHandle)value);
			}
			section._runtimeFields._handles.Add(section._sh.Tell64());
			section._sh.WriteUInt32(0x80000000u | (uint)handleIndex);
		}
		public override Type GetOutputType() => typeof(igHandle);
	}
	public class igHandleArrayMetaField : igHandleMetaField
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