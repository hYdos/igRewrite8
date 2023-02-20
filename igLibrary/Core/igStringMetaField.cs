namespace igLibrary.Core
{
	public class igStringMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			uint basePos = loader._stream.Tell();
			bool isRef = loader._runtimeFields._stringRefs.Any(x => x == basePos);
			bool isTable = loader._runtimeFields._stringTables.Any(x => x == basePos);

			string? data = null;
			ulong raw = loader.ReadRawOffset();

			if(isRef)
			{
				ulong offset = loader.DeserializeOffset(raw);
				loader._stream.Seek(offset);
				data = loader._stream.ReadString();
			}
			else if (isTable)
			{
				data = loader._stringList[(int)raw];
			}
			
			loader._stream.Seek(basePos + igAlchemyCore.GetPointerSize(loader._platform));

			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override Type GetOutputType() => typeof(string);
	}
	public class igStringArrayMetaField : igStringMetaField
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