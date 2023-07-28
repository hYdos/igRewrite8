using System.Reflection;
using System.Reflection.Emit;

namespace igLibrary.Core
{
	public class igMetaObject : igBaseMeta
	{
		public igMetaObject? _parent;
		public List<igMetaField> _metaFields;
		public igObjectList? _attributes;
		public Dictionary<IG_CORE_PLATFORM, ushort> _sizes = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public Dictionary<IG_CORE_PLATFORM, ushort> _alignments = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public Type _vTablePointer;
		private bool _baseFieldsInherited = false;


		public igMetaObject()
		{
			_metaFields = new List<igMetaField>();
		}
		public T? GetAttribute<T>() where T : igObject
		{
			if(_attributes == null) return null;
			for(int i = 0; i < _attributes._count; i++)
			{
				if(_attributes[i] is T attr) return attr;
			}
			return null;
		}
		public void GenerateType()
		{
			DeclareType();
			igArkCore.GeneratePendingTypes();
		}

		public override void DeclareType()
		{
			if(_vTablePointer != null) return;

			_vTablePointer = igArkCore.GetNewTypeBuilder(_name);

			igArkCore._pendingTypes.Add(this);

			for(int i = 0; i < _metaFields.Count; i++)
			{
				if(_parent._metaFields.Count <= i)                ReadyFieldDependancy(_metaFields[i]);
				else if(_metaFields[i] != _parent._metaFields[i]) ReadyFieldDependancy(_metaFields[i]);
			}

			_parent.DeclareType();
		}
		public override void DefineType()
		{
			if(!(_vTablePointer is TypeBuilder)) return;

			TypeBuilder tb = (TypeBuilder)_vTablePointer;
			Type parentType = _parent._vTablePointer;

			if(_parent._name == "igDataList")
			{
				igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];

				parentType = typeof(igTDataList<>).MakeGenericType(dataField._memType.GetOutputType());
			}
			else if(_parent._name == "igObjectList" || _parent._name == "igNonRefCountedObjectList")
			{
				igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];

				parentType = typeof(igTObjectList<>).MakeGenericType(dataField._memType.GetOutputType());
				_priority = BuildPriority.Low;
			}
			else if(_parent._name == "igHashTable")
			{
				igMemoryRefMetaField valuesField = (igMemoryRefMetaField)_metaFields[0];
				igMemoryRefMetaField keysField = (igMemoryRefMetaField)_metaFields[1];

				parentType = typeof(igTUHashTable<,>).MakeGenericType(valuesField._memType.GetOutputType(), keysField._memType.GetOutputType());
				_priority = BuildPriority.Low;
			}

			tb.SetParent(parentType);

			for(int i = _parent._metaFields.Count; i < _metaFields.Count; i++)
			{
				if(_metaFields[i] is igPropertyFieldMetaField) continue;

				FieldAttributes attrs = FieldAttributes.Public;
				if(_metaFields[i] is igStaticMetaField) attrs |= FieldAttributes.Static;

				FieldBuilder fb = tb.DefineField(_metaFields[i]._name, _metaFields[i].GetOutputType(), attrs);
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

					_parent.FinalizeType();

					if(tb.BaseType != null && tb.BaseType.IsGenericType)
					{
						if(tb.BaseType.GetGenericTypeDefinition() == typeof(igTObjectList<>))
						{
							igMemoryRefMetaField _data = (igMemoryRefMetaField)_metaFields[2];
							if(_data._memType is igObjectRefMetaField objectRefMetaField) objectRefMetaField._metaObject.FinalizeType();
						}
						else if(tb.BaseType.GetGenericTypeDefinition() == typeof(igTUHashTable<,>))
						{
							igMemoryRefMetaField memField = (igMemoryRefMetaField)_metaFields[0];
							if(memField._memType is igObjectRefMetaField objectRefMetaField) objectRefMetaField._metaObject.FinalizeType();
							memField = (igMemoryRefMetaField)_metaFields[1];
							if(memField._memType is igObjectRefMetaField objectRefMetaField2) objectRefMetaField2._metaObject.FinalizeType();
						}
					}
					

					for(int i = 0; i < _metaFields.Count; i++)
					{
						if(_parent._metaFields.Count <= i)                FinalizeFieldDependancy(_metaFields[i]);
						else if(_metaFields[i] != _parent._metaFields[i]) FinalizeFieldDependancy(_metaFields[i]);
					}

					Console.WriteLine($"Prepped {_name}");
				}

				_finishedFinalizationPrep = true;
			}

			if(!_beganFinalization && _finishedFinalizationPrep && _parent._finishedFinalization)
			{
				_beganFinalization = true;

				if(_vTablePointer is TypeBuilder tb)
				{
					Console.WriteLine($"Finalizing {_name}");
					
					Type testType = tb.CreateType();
					igArkCore.AddDynamicTypeToCache(testType);
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
			_vTablePointer = igArkCore.GetNewTypeBuilder(_name);

			_parent.GatherDependancies();

			for(int i = 0; i < _metaFields.Count; i++)
			{
				ReadyCompoundFieldDependancy(_metaFields[i]);
			}
			for(int i = 0; i < _parent._metaFields.Count; i++)
			{
				if(_metaFields[i] != _parent._metaFields[i]) ReadyFieldDependancy2(_metaFields[i]);
			}

			igArkCore._pendingTypes.Add(this);

			for(int i = _parent._metaFields.Count; i < _metaFields.Count; i++)
			{
				ReadyFieldDependancy2(_metaFields[i]);
			}

		}
		public override void DefineType2()
		{
			if(_vTablePointer is not TypeBuilder tb) return;

			Type parentType = _parent._vTablePointer;

			if(_parent._name == "igDataList")
			{
				igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];

				parentType = typeof(igTDataList<>).MakeGenericType(dataField._memType.GetOutputType());
			}
			else if(_parent._name == "igObjectList" || _parent._name == "igNonRefCountedObjectList")
			{
				igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];

				parentType = typeof(igTObjectList<>).MakeGenericType(dataField._memType.GetOutputType());
				_priority = BuildPriority.Low;
			}
			else if(_parent._name == "igHashTable")
			{
				igMemoryRefMetaField valuesField = (igMemoryRefMetaField)_metaFields[0];
				igMemoryRefMetaField keysField = (igMemoryRefMetaField)_metaFields[1];

				parentType = typeof(igTUHashTable<,>).MakeGenericType(valuesField._memType.GetOutputType(), keysField._memType.GetOutputType());
				_priority = BuildPriority.Low;
			}

			tb.SetParent(parentType);
			for(int i = _parent._metaFields.Count; i < _metaFields.Count; i++)
			{
				if(_metaFields[i] is igPropertyFieldMetaField) continue;

				FieldAttributes attrs = FieldAttributes.Public;
				if(_metaFields[i] is igStaticMetaField) attrs |= FieldAttributes.Static;

				FieldBuilder fb = tb.DefineField(_metaFields[i]._name, _metaFields[i].GetOutputType(), attrs);
			}
		}
		public override void CreateType2()
		{
			if(_vTablePointer is not TypeBuilder tb) return;

			if(!_parent._finishedFinalization) throw new TypeLoadException("Derived class being initialized before parent.");
			if(!_beganFinalization)
			{
				_beganFinalization = true;

				Console.WriteLine($"Finalizing {_name}");
				
				Type testType = tb.CreateType();
				igArkCore.AddDynamicTypeToCache(testType);
				_vTablePointer = testType;

				Console.WriteLine($"Finalized {_name}");

				_finishedFinalization = true;
			}
		}

		public override void PostUndump()
		{
			_vTablePointer = igArkCore.GetObjectDotNetType(_name);

			//Sometimes is null, in those cases, a dynamic type will be created

			if(_vTablePointer != null)
			{
				_beganFinalizationPrep = true;
				_finishedFinalizationPrep = true;
				_beganFinalization = true;
				_finishedFinalization = true;
			}
		}
		public bool CanBeAssignedTo(igMetaObject other)
		{
			igMetaObject? tester = this;
			while(tester != null)
			{
				if(tester == other) return true;
				tester = tester._parent;
			}
			return false;
		}
		public int GetFieldIndexByName(string name)
		{
			int index = _metaFields.FindIndex(x => x._name == name);
			return index;
		}
		public override igMetaField? GetFieldByName(string name)
		{
			int index = GetFieldIndexByName(name);
			if(index < 0) return null;
			return _metaFields[index];
		}
		public void ValidateAndSetField(int index, igMetaField field)
		{
			field._parentMeta = this;
			_metaFields[index] = field;
		}
		public void InheritFields()
		{
			//if(_baseFieldsInherited) return;
			//_baseFieldsInherited = true;
			if(_parent == null) return;
			//_parent.InheritFields();
			_metaFields.AddRange(_parent._metaFields);
		}
		public void AppendDynamicField(igMetaField field)
		{
			field._offset = (ushort)(_metaFields.Max(x => x._offset) + 1u);
			field._parentMeta = this;
			_metaFields.Add(field);
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
			ushort alignment = 0;
			igMetaField[] metaFieldsByOffset = _metaFields.OrderBy(x => x._offset).ToArray();
			ushort currentOffset = (ushort)(4u + igAlchemyCore.GetPointerSize(platform));
			for(int i = 0; i < metaFieldsByOffset.Length; i++)
			{
				if(metaFieldsByOffset[i] is igStaticMetaField) continue;
				if(metaFieldsByOffset[i] is igPropertyFieldMetaField) continue;
				if(metaFieldsByOffset[i] is igBitFieldMetaField bfMf)
				{
					if(!bfMf._offsets.ContainsKey(platform))
					{
						bfMf._offsets.Add(platform, bfMf._storageMetaField._offsets[platform]);
					}
					continue;
				}

				alignment = (ushort)System.Math.Max(alignment, metaFieldsByOffset[i].GetAlignment(platform));

				Align(ref currentOffset, metaFieldsByOffset[i].GetAlignment(platform));

				if(metaFieldsByOffset[i]._offsets.ContainsKey(platform))
					goto addOffsetAndContinue;

				metaFieldsByOffset[i]._offsets.Add(platform, currentOffset);

			addOffsetAndContinue:
				currentOffset += (ushort)metaFieldsByOffset[i].GetSize(platform);
			}

			if(!_alignments.ContainsKey(platform))
			{
				_alignments.Add(platform, alignment);
			}
			if(!_sizes.ContainsKey(platform))
			{
				Align(ref currentOffset, alignment);
				_sizes.Add(platform, currentOffset);
			}
		}
		private void Align(ref ushort offset, uint alignment)
		{
			offset = (ushort)(((offset + (alignment - 1)) / alignment) * alignment);
		}
		public virtual void AppendToArkCore()
		{
			igArkCore._metaObjects.Add(this);
		}
		public virtual igObject ConstructInstance(igMemoryPool memPool, bool setFields = true)
		{
			if(_vTablePointer == null)
			{
				GenerateType();
			}
			igObject obj = (igObject)Activator.CreateInstance(_vTablePointer);
			obj.internalMemoryPool = memPool;
			if(setFields) obj.ResetFields(this);
			return obj;
		}
		public void CorrectObjectMeta(igObject obj)
		{
			FieldInfo? fi = _vTablePointer.GetField("_meta");
			if(fi != null)
			{
				fi.SetValue(obj, this);
			}
		}
	}
}