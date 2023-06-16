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
				ret = loader._namedExternalList[(int)(raw & 0x7FFFFFFF)];
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

			igExternalReferenceSystem.Singleton._globalSet.MakeReference(obj, null, out igHandleName name);
			if(name._ns._hash != 0)
			{
				section._runtimeFields._namedExternals.Add(section._sh.Tell64());
				section._sh.WriteUInt32((uint)saver._namedExternalList.Count | 0x80000000);
				saver._namedExternalList.Add(new igHandle(name));
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
				}
				else
				{
					Console.WriteLine("EXNM object found, reference to " + hnd.ToString());
					section._runtimeFields._namedExternals.Add(section._sh.Tell64());
					section._sh.WriteUInt32((uint)saver._namedExternalList.Count | 0x80000000);
					saver._namedExternalList.Add(hnd);
				}
				return;
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
			if(_metaObject._vTablePointer == null) _metaObject.GatherDependancies();
			return _metaObject._vTablePointer;
		}
		public override object? GetDefault(igObject target)
		{
			if(_construct) return _metaObject.ConstructInstance(target.internalMemoryPool);
			else return _default;
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