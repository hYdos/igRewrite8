namespace igLibrary.Core
{
	public class igStringMetaField : igRefMetaField
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
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			uint basePos = section._sh.Tell();
			
			if(value == null)
			{
				saver.WriteRawOffset(0, section);
			}
			else
			{
				//why use string refs when you can use the string table
				//Because alchemy sucks
				if(igAlchemyCore.isPlatform64Bit(saver._platform))
				{
					int index = saver._stringList.FindIndex(x => x == (string)value);
					if(index < 0)
					{
						index = saver._stringList.Count;
						saver._stringList.Add((string)value);
					}
					saver.WriteRawOffset((ulong)index, section);
					section._runtimeFields._stringTables.Add(basePos);
				}
				else
				{
					if(saver._stringRefList.TryGetValue((string)value, out uint offset))
					{

					}
					else
					{
						igIGZSaver.SaverSection stringSection = saver.GetSaverSection(igMemoryContext.Singleton.GetMemoryPoolByName("String"));
						stringSection._sh.Seek(stringSection.FindFreeMemory((byte)((saver._version < 7) ? 1u : 2u)));
						offset = saver.SerializeOffset(stringSection._sh.Tell(), stringSection);
						saver._stringRefList.Add((string)value, offset);
						stringSection._sh.WriteString((string)value);
						stringSection.PushAlignment((saver._version < 7) ? 1u : 2u);
					}
					section._runtimeFields._stringRefs.Add(basePos);
					section._sh.WriteUInt32(offset);
				}
			}
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