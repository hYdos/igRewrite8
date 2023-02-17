using System.Reflection;

namespace igLibrary.Core
{
	public class igMemoryRefMetaField : igMetaField
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
			igSizeTypeMetaField metaField = new igSizeTypeMetaField();	//Should implement that static _MetaField field
			ulong size = (ulong)metaField.ReadIGZField(loader) & 0x07FFFFFF;
			ulong offset = (ulong)metaField.ReadIGZField(loader);
			ulong end = loader._stream.Tell64();
			igMemoryPool pool = loader.GetMemoryPoolFromSerializedOffset(offset);
			offset = loader.DeserializeOffset(offset);
			Type memoryType = GetOutputType();
			IigMemory memory = (IigMemory)Activator.CreateInstance(memoryType);
			memory.SetMemoryPool(pool);
			Array objects = Array.CreateInstance(_memType.GetOutputType(), (int)(size / _memType.GetSize(loader._platform)));

			for(int i = 0; i < objects.Length; i++)
			{
				objects.SetValue(_memType.ReadIGZField(loader), i);
			}

			memory.SetData(objects);
			
			return memory;
		}
		public override Type GetOutputType()
		{
			return typeof(igMemory<>).MakeGenericType(_memType.GetOutputType());
		}
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform) * 2;
	}
	public class igMemoryRefArrayMetaField : igMemoryRefMetaField
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