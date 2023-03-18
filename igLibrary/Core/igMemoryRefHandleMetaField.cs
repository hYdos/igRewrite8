using System.Reflection;

namespace igLibrary.Core
{
	public class igMemoryRefHandleMetaField : igRefMetaField
	{
		public igMetaField _memType;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			_memType.DumpArkData(saver, sh);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_memType = loader.ReadMetaField(sh);
		}
		public override object? ReadIGZField(igIGZLoader loader)
		{
			return null;
		}

		public override Type GetOutputType()
		{
			return typeof(igMemory<>).MakeGenericType(_memType.GetOutputType());
		}
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override igMetaField CreateFieldCopy()
		{
			igMemoryRefMetaField field = (igMemoryRefMetaField)base.CreateFieldCopy();
			field._memType = field._memType.CreateFieldCopy();
			return field;
		}
	}
	public class igMemoryRefHandleArrayMetaField : igMemoryRefHandleMetaField
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