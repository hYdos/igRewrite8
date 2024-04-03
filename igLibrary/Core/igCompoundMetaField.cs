using System.Reflection.Emit;
using System.Reflection;

namespace igLibrary.Core
{
	public class igCompoundMetaField : igMetaField
	{
		public igCompoundMetaFieldInfo _compoundFieldInfo;

		public override object? ReadIGZField(igIGZLoader loader)
		{
			if(_compoundFieldInfo._vTablePointer == null) return null;

			uint objectOffset = loader._stream.Tell();

			List<igMetaField> metaFields = _compoundFieldInfo._fieldList;

			object compoundData = _compoundFieldInfo.ConstructInstance(GetOutputTypeInternal());	//grab the type like this becuase igOrderedMapMetaField<T, U>

			_compoundFieldInfo.CalculateOffsetForPlatform(loader._platform);

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;
				if(!metaFields[i].IsApplicableForPlatform(loader._platform)) continue;

				if(!metaFields[i]._properties._persistent) continue;

				loader._stream.Seek(objectOffset + metaFields[i]._offsets[loader._platform]);

				object? data = metaFields[i].ReadIGZField(loader);

				FieldInfo? field = GetOutputTypeInternal().GetField(metaFields[i]._name);
				if(field != null)
				{
					field.SetValue(compoundData, data);
				}
			}

			return compoundData;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			if(_compoundFieldInfo._vTablePointer == null) return;

			uint objectOffset = section._sh.Tell();

			List<igMetaField> metaFields = _compoundFieldInfo._fieldList;

			_compoundFieldInfo.CalculateOffsetForPlatform(saver._platform);

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;
				if(!metaFields[i].IsApplicableForPlatform(saver._platform)) continue;

				object? data = null;

				if(metaFields[i]._properties._persistent)
				{
					FieldInfo? field = GetOutputTypeInternal().GetField(metaFields[i]._name);

					if(field == null) continue;

					data = field.GetValue(value);
				}
				else
				{
					data = metaFields[i].GetDefault(igMemoryContext.Singleton.GetMemoryPoolByName("Default"));
					if((metaFields[i].GetOutputType().IsValueType || metaFields[i].GetOutputType() == typeof(string)) && data == null) continue;
				}

				section._sh.Seek(objectOffset + metaFields[i]._offsets[saver._platform]);

				metaFields[i].WriteIGZField(saver, section, data);
			}
		}
		public override Type GetOutputType()
		{
			if(_compoundFieldInfo._vTablePointer == null) return typeof(object);
			else return _compoundFieldInfo._vTablePointer;
		}
		//Exists to deal with both igOrderedMap<T, U> being annoying and also needing to handle array metafields
		private Type GetOutputTypeInternal()
		{
			Type t = GetOutputType();
			if(t.IsArray) t = t.GetElementType();
			return t;
		}

		public override uint GetAlignment(IG_CORE_PLATFORM platform) => _compoundFieldInfo._platformInfo._alignments[platform];
		public override uint GetSize(IG_CORE_PLATFORM platform) => _compoundFieldInfo._platformInfo._sizes[platform];
		public override object? GetDefault(igMemoryPool pool)
		{
			return _compoundFieldInfo.ConstructInstance(GetOutputTypeInternal());
		}
	}

	public class igCompoundMetaFieldInfo : igBaseMeta
	{
		public List<igMetaField> _fieldList = new List<igMetaField>();
		public igMetaFieldPlatformInfo _platformInfo;
		public Type _vTablePointer;
		private bool _beganTypeDeclaration = false;
		private bool _beganTypeDefiniton = false;
		private bool _beganTypeFinalization = false;
		private bool _finishedTypeFinalization = false;

		public override void PostUndump()
		{
			_vTablePointer = igArkCore.GetStructDotNetType(_name.Substring(0, _name.Length-9));
			_platformInfo = igArkCore.GetMetaFieldPlatformInfo(_name);

			//Sometimes is null, in those cases, a dynamic type will be created
		}

		public override void DeclareType()
		{
			if(_vTablePointer != null) return;

			_vTablePointer = igArkCore.GetNewStructTypeBuilder(_name.Substring(0, _name.Length - 9));
			igArkCore._pendingTypes.Add(this);

			for(int i = 0; i < _fieldList.Count; i++)
			{
				ReadyFieldDependancy(_fieldList[i]);
			}
		}
		public override void DefineType()
		{
			if(!(_vTablePointer is TypeBuilder)) return;

			TypeBuilder tb = (TypeBuilder)_vTablePointer;

			tb.SetParent(typeof(ValueType));

			for(int i = 0; i < _fieldList.Count; i++)
			{
				if(_fieldList[i] is igPropertyFieldMetaField) continue;

				FieldAttributes attrs = FieldAttributes.Public;
				if(_fieldList[i] is igStaticMetaField) attrs |= FieldAttributes.Static;

				FieldBuilder fb = tb.DefineField(_fieldList[i]._name, _fieldList[i].GetOutputType(), attrs);
			}
		}
		public override void FinalizeType()
		{
			if(!_beganFinalizationPrep)
			{
				_beganFinalizationPrep = true;

				if(_vTablePointer is TypeBuilder tb)
				{
					Console.WriteLine($"Prepping {_name}");

					for(int i = 0; i < _fieldList.Count; i++)
					{
						FinalizeFieldDependancy(_fieldList[i]);
					}

					Console.WriteLine($"Prepped {_name}");
				}

				_finishedFinalizationPrep = true;
			}

			if(!_beganFinalization && _finishedFinalizationPrep)
			{
				_beganFinalization = true;

				if(_vTablePointer is TypeBuilder tb)
				{
					Console.WriteLine($"Finalizing {_name}");
					
					Type testType = tb.CreateType();
					//igArkCore.AddDynamicTypeToCache(testType);
					_vTablePointer = testType;

					Console.WriteLine($"Finalized {_name}");
				}

				_finishedFinalization = true;
			}
		}
		public override void GatherDependancies()
		{
			if(_gatheredDependancies) return;
			_gatheredDependancies = true;
			if(_vTablePointer is not null) return;
			_vTablePointer = igArkCore.GetNewStructTypeBuilder(_name);

			for(int i = 0; i < _fieldList.Count; i++)
			{
				ReadyCompoundFieldDependancy(_fieldList[i]);
			}

			igArkCore._pendingTypes.Add(this);

			for(int i = 0; i < _fieldList.Count; i++)
			{
				ReadyFieldDependancy2(_fieldList[i]);
			}
		}
		public override void DefineType2()
		{
			if(_vTablePointer is not TypeBuilder tb) return;

			for(int i = 0; i < _fieldList.Count; i++)
			{
				if(_fieldList[i] is igPropertyFieldMetaField) continue;

				FieldAttributes attrs = FieldAttributes.Public;
				if(_fieldList[i] is igStaticMetaField) attrs |= FieldAttributes.Static;

				FieldBuilder fb = tb.DefineField(_fieldList[i]._name, _fieldList[i].GetOutputType(), attrs);
			}
		}
		public override void CreateType2()
		{
			if(_vTablePointer is not TypeBuilder tb) return;

			if(!_beganFinalization)
			{
				_beganFinalization = true;

				Console.WriteLine($"Finalizing {_name}");
				
				Type testType = tb.CreateType();
				_vTablePointer = testType;

				Console.WriteLine($"Finalized {_name}");

				_finishedFinalization = true;
			}
		}

		public void CalculateOffsets()
		{
			igMetaEnum platformEnum = igArkCore.GetMetaEnum("IG_CORE_PLATFORM");
			IG_CORE_PLATFORM[] platforms = new IG_CORE_PLATFORM[platformEnum._names.Count];
			
			for(int i = 0; i < platforms.Length; i++)
			{
				platforms[i] = (IG_CORE_PLATFORM)platformEnum.GetEnumFromName(platformEnum._names[i]);

				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT) continue;
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED) continue;
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX) continue;

				CalculateOffsetForPlatform(platforms[i]);
			}
		}
		public void CalculateOffsetForPlatform(IG_CORE_PLATFORM platform)
		{
			igMetaField[] metaFieldsByOffset = _fieldList.OrderBy(x => x._offset).ToArray();
			ushort currentOffset = 0;
			for(int i = 0; i < metaFieldsByOffset.Length; i++)
			{
				if(metaFieldsByOffset[i] is igStaticMetaField) continue;
				if(metaFieldsByOffset[i] is igPropertyFieldMetaField) continue;
				if(metaFieldsByOffset[i] is igBitFieldMetaField) continue;
				if(!metaFieldsByOffset[i].IsApplicableForPlatform(platform)) continue;
				if(metaFieldsByOffset[i]._offsets.ContainsKey(platform)) continue;

				Align(ref currentOffset, metaFieldsByOffset[i].GetAlignment(platform));

				metaFieldsByOffset[i]._offsets.Add(platform, currentOffset);

				currentOffset += (ushort)metaFieldsByOffset[i].GetSize(platform);
			}
		}
		public override igMetaField? GetFieldByName(string name)
		{
			int index = _fieldList.FindIndex(x => x._name == name);
			if(index < 0) return null;
			return _fieldList[index];
		}
		private void Align(ref ushort offset, uint alignment)
		{
			offset = (ushort)(((offset + (alignment - 1)) / alignment) * alignment);
		}
		public void TypeBuildBegin()
		{
			if(_beganTypeDeclaration) return;
			if(_vTablePointer != null)
			{
				_beganTypeDeclaration = true;
				return;
			}
			_beganTypeDeclaration = true;

			TypeBuilder tb = igArkCore.GetNewStructTypeBuilder(_name.Substring(0, _name.Length - 9));		//Trim off the "MetaField"
			_vTablePointer = tb;
		}
		public void TypeBuildAddFields()
		{
			if(_beganTypeDefiniton) return;
			if(!(_vTablePointer is TypeBuilder))
			{
				_beganTypeDefiniton = true;
				return;
			}
			_beganTypeDefiniton = true;

			TypeBuilder tb = (TypeBuilder)_vTablePointer;

			if(igHash.Hash(_name) == 0x48D79D7B) return;	//if _name is "igOrderedMapMetaField" as that one's generic!

			for(int i = 0; i < _fieldList.Count; i++)
			{
				if(_fieldList[i] is igPropertyFieldMetaField) continue;

				FieldAttributes attrs = FieldAttributes.Public;
				if(_fieldList[i] is igStaticMetaField) attrs |= FieldAttributes.Static;

				FieldBuilder fb = tb.DefineField(_fieldList[i]._name, _fieldList[i].GetOutputType(), attrs);
			}
		}
		public void TypeBuildFinalize()
		{
			if(_beganTypeFinalization) return;
			else if(!(_vTablePointer is TypeBuilder))
			{
				_beganTypeFinalization = true;
				_finishedTypeFinalization = true;
				return;
			}

			_beganTypeFinalization = true;

			for(int i = 0; i < _fieldList.Count; i++)
			{
				if(_fieldList[i] is igCompoundMetaField compoundMetaField)
				{
					ReadyDependancyAndBlock(compoundMetaField._compoundFieldInfo);
				}
			}

			Type testType = ((TypeBuilder)_vTablePointer).CreateType();
			//igArkCore.AddDynamicTypeToCache(testType);
			_vTablePointer = testType;
			_finishedTypeFinalization = true;
		}
		private void ReadyDependancyAndBlock(igCompoundMetaFieldInfo dependancy)
		{
			if(dependancy._beganTypeFinalization == false)
			{
				dependancy.TypeBuildFinalize();
				return;
			}			

			while(!dependancy._finishedTypeFinalization)
			{
				Thread.Sleep(10);
			}
		}
		public object ConstructInstance(Type t, bool setFields = true)
		{
			object obj = Activator.CreateInstance(t);
			if(setFields) ResetFields(t, obj);
			return obj;
		}
		public void ResetFields(Type t, object dat)
		{
			for(int i = 0; i < _fieldList.Count; i++)
			{
				if(_fieldList[i] is igStaticMetaField || _fieldList[i] is igPropertyFieldMetaField || _fieldList[i] is igBitFieldMetaField) continue;

				//Defaults not working cos they're not in the metadata iirc
				string name = _fieldList[i]._name ?? $"_unk{i}";
				FieldInfo? field = t.GetField(name);

				object? data = _fieldList[i].GetDefault(igMemoryContext.Singleton.GetMemoryPoolByName("Default"));

				if(_fieldList[i].GetOutputType().IsValueType && data == null) continue;

				field.SetValue(dat, data);
			}
		}
	}

	public class igStruct : Attribute{}

	public class igCompoundArrayMetaField : igCompoundMetaField
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
		public override object? GetDefault(igMemoryPool pool)
		{
			Array arr = Array.CreateInstance(base.GetOutputType(), _num);
			for(int i = 0; i < _num; i++)
			{
				arr.SetValue(base.GetDefault(pool), i);
			}
			return arr;
		}
	}
}