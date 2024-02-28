using System.Reflection;

namespace igLibrary.Core
{
	public class igVectorMetaField : igRefMetaField
	{
		public igMetaField _memType;
		public int _initialCapacity;		//Todo: Dump this from the game

		public override void SetTemplateParameter(uint index, igMetaField meta)
		{
			if(index != 0) throw new IndexOutOfRangeException("igVector only has 1 template argument");

			_memType = meta;
		}
		public override void SetTemplateParameterCount(uint count)
		{
			if(count != 1) throw new IndexOutOfRangeException("igVector can only has 1 template argument");
		}
		public override igMetaField GetTemplateParameter(uint index)
		{
			if(index != 0) throw new IndexOutOfRangeException("igVector only has 1 template argument");

			return _memType;
		}
		public override uint GetTemplateParameterCount() => 1;
		public override object? ReadIGZField(igIGZLoader loader)
		{
			uint count = 0;
			if(loader._version == 0x09 && igAlchemyCore.isPlatform64Bit(loader._platform))
			{
				count = (uint)loader._stream.ReadUInt64();
			}
			else
			{
				count = loader._stream.ReadUInt32();
			}

			igVectorCommon vector = (igVectorCommon)Activator.CreateInstance(GetOutputType());

			IigMemory memory;

			if(count != 0)
			{
				igMemoryRefMetaField memoryRefMetaField = new igMemoryRefMetaField();
				memoryRefMetaField._memType = _memType;
				memory = (IigMemory)memoryRefMetaField.ReadIGZField(loader);
				vector.SetData(memory);
				vector.SetCount(count);
			}

			return vector;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVectorCommon vector = (igVectorCommon)value;

			if(saver._version == 0x09 && igAlchemyCore.isPlatform64Bit(saver._platform))
			{
				section._sh.WriteUInt64(vector.GetCount());
			}
			else
			{
				section._sh.WriteUInt32(vector.GetCount());
			}

			igMemoryRefMetaField memoryRefMetaField = new igMemoryRefMetaField();
			memoryRefMetaField._memType = _memType;
			memoryRefMetaField.WriteIGZField(saver, section, vector.GetData());
		}
		public override Type GetOutputType()
		{
			return typeof(igVector<>).MakeGenericType(_memType.GetOutputType());
		}
		public override object? GetDefault(igMemoryPool pool)
		{
			igVectorCommon vector = (igVectorCommon)Activator.CreateInstance(GetOutputType());
			if(_initialCapacity > 0)
			{
				vector.SetCapacity(_initialCapacity);
			}
			IigMemory memory = vector.GetData();
			memory.SetMemoryPool(pool);
			vector.SetData(memory);
			return vector;
		}
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform) * 3;	//May cause issues for 64 bit platforms
	}
	public class igVectorArrayMetaField : igVectorMetaField
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