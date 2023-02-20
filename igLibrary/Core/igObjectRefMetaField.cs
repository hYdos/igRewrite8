using System.Reflection;

namespace igLibrary.Core
{
	public class igObjectRefMetaField : igRefMetaField
	{
		public igMetaObject _metaObject;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			if(_name == "_traversal")
				sh = sh;

			saver.SaveString(sh, _metaObject._name);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			if(_name == "_traversal")
				sh = sh;

			_metaObject = igArkCore.GetObjectMeta(loader.ReadString(sh));
		}
		public override object? ReadIGZField(igIGZLoader loader)
		{
			bool isExid = loader._runtimeFields._externals.Any(x => x == (ulong)loader._stream.BaseStream.Position);
			bool isOffset = loader._runtimeFields._offsets.Any(x => x == (ulong)loader._stream.BaseStream.Position);
			bool isNamedExternal = loader._runtimeFields._namedExternals.Any(x => x == (ulong)loader._stream.BaseStream.Position);
			igSizeTypeMetaField sizeTypeMetaField = new igSizeTypeMetaField();
			ulong raw = (ulong)sizeTypeMetaField.ReadIGZField(loader);
			igObject? ret = null;
			if(raw == 0) return null;
			if(isNamedExternal)
			{
				try
				{
					//ret = loader._namedExternalList[(int)(raw & 0x7FFFFFFF)];
				}
				catch(Exception e)
				{
					//Console.WriteLine($"NamedExternal Error: {e.Message} @ {igz._stream.BaseStream.Position - igCore.GetSizeOfPointer(igz._platform)}");
					ret = null;
				}
			}
			else if(isOffset)
			{
				ret = loader._offsetObjectList[loader.DeserializeOffset(raw)];
				loader._stream.Seek(loader.DeserializeOffset(raw));
			}
			else if(isExid)
			{
				//ret = igz._externalList[(int)(raw & 0x7FFFFFFF)].GetObject<T>();
			}
			else
			{
				Console.WriteLine("uhhhhhhhhhhhhhhhhh");
			}
			return ret;
			//if(ret is T t) return t;
			//else return null;
		}
		public override Type GetOutputType()
		{
			if(_metaObject._vTablePointer == typeof(igBlindObject)) return typeof(igObject);
			return _metaObject._vTablePointer;
		}
	}
	public class igObjectRefArrayMetaField : igObjectRefMetaField
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