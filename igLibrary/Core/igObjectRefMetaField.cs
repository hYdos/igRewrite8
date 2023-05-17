using System.Reflection;

namespace igLibrary.Core
{
	public class igObjectRefMetaField : igRefMetaField
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
				ret = loader._offsetObjectList[raw];
			}
			else if(isExid)
			{
				ret = loader._externalList[(int)(raw & 0x7FFFFFFF)].GetObjectAlias<igObject>();
			}
			else
			{
				Console.WriteLine("uhhhhhhhhhhhhhhhhh");
			}
			return ret;
			//if(ret is T t) return t;
			//else return null;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			ulong baseOffset = section._sh.Tell64();
			igObject? obj = (igObject?)value;

			if(obj == null)
			{
				saver.WriteRawOffset(0, section);
				return;
			}
			
			igHandle hnd = igObjectHandleManager.Singleton.GetHandle(obj);
			if(hnd != null)
			{
				if(igObjectHandleManager.Singleton.IsSystemObject(hnd))
				{
					Console.WriteLine("EXID object found, reference to " + hnd.ToString());
					section._runtimeFields._externals.Add(section._sh.Tell64());
					section._sh.WriteUInt32((uint)saver._externalList.Count | 0x80000000);
					saver._externalList.Add(hnd);
					return;
				}
				else
				{
					Console.WriteLine("EXNM object found, reference to " + hnd.ToString());
					section._runtimeFields._externals.Add(section._sh.Tell64());
					section._sh.WriteUInt32((uint)saver._externalList.Count | 0x80000000);
					saver._externalList.Add(hnd);
				}
			}
			//Should add stuff to check for externals

			ulong objectOffset = saver.SaveObject(obj);
			section._sh.Seek(baseOffset);
			saver.WriteRawOffset(objectOffset, section);
			section._runtimeFields._offsets.Add(baseOffset);
			if(_refCounted && obj != null)
			{
				saver.RefObject(obj);
			}
		}
		public override Type GetOutputType()
		{
			//if(_metaObject._vTablePointer == typeof(igBlindObject)) return typeof(igObject);
			if(_metaObject._vTablePointer == null) _metaObject.DeclareType();
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