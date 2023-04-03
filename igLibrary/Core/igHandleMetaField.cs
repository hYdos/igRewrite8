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
			return null;	//Placeholder
		}

		public override Type GetOutputType() => typeof(igHandle);
		public override void ReadyOutputType()
		{
			if(_metaObject != null) _metaObject.TypeBuild();
		}
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