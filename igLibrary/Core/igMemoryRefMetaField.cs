using System.Reflection;

namespace igLibrary.Core
{
	public class igMemoryRefMetaField : igRefMetaField
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
			ulong size = loader.ReadRawOffset() & 0x07FFFFFF;
			ulong raw = loader.ReadRawOffset();
			ulong end = loader._stream.Tell64();

			igMemoryPool pool = loader.GetMemoryPoolFromSerializedOffset(raw);
			ulong offset = loader.DeserializeOffset(raw);
			Type memoryType;
			try
			{
				memoryType = GetOutputType();
			}
			catch(NotImplementedException)
			{
				return null;
			}
			uint memSize = _memType.GetSize(loader._platform);
			IigMemory memory = (IigMemory)Activator.CreateInstance(memoryType);
			memory.SetMemoryPool(pool);
			Array objects = Array.CreateInstance(_memType.GetOutputType(), (int)(size / memSize));

			for(int i = 0; i < objects.Length; i++)
			{
				loader._stream.Seek((long)offset + memSize * i);
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
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);

		public override igMetaField CreateFieldCopy()
		{
			igMemoryRefMetaField field = (igMemoryRefMetaField)base.CreateFieldCopy();
			field._memType = field._memType.CreateFieldCopy();
			return field;
		}
		public override void ReadyOutputType()
		{
			if(_memType != null)
			{
				_memType.ReadyOutputType();
			}
		}	}
	public class igMemoryRefArrayMetaField : igMemoryRefMetaField
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