using System.Reflection;

namespace igLibrary.Core
{
	public class igVectorMetaField : igRefMetaField
	{
		public igMetaField _memType;

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

			igMemoryRefMetaField memoryRefMetaField = new igMemoryRefMetaField();
			memoryRefMetaField._memType = _memType;
			IigMemory memory = (IigMemory)memoryRefMetaField.ReadIGZField(loader);

			igVectorCommon vector = (igVectorCommon)Activator.CreateInstance(GetOutputType());

			vector.SetData(memory);
			vector.SetCount(count);

			return vector;
		}
		public override Type GetOutputType()
		{
			return typeof(igVector<>).MakeGenericType(_memType.GetOutputType());
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