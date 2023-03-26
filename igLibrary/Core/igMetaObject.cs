using System.Reflection;
using System.Reflection.Emit;

namespace igLibrary.Core
{
	public class igMetaObject : igBaseMeta
	{
		public igMetaObject? _parent;
		public List<igMetaField> _metaFields;
		public Type _vTablePointer;

		private bool _beganTypeDeclaration = false;
		private bool _beganTypeDefiniton = false;
		private bool _beganTypeFinalization = false;
		private bool _finishedTypeFinalization = false;
		private static MethodInfo GetCompoundFieldInfoMethodInfo = typeof(igArkCore).GetMethod("GetCompoundFieldInfo");
		private static FieldInfo CompoundFieldInfoFieldInfo = typeof(igCompoundMetaField).GetField("_compoundFieldInfo");
		private static ConstructorInfo CompoundFieldConstructor = typeof(igCompoundMetaField).GetConstructor(BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes);

		public igMetaObject()
		{
			_metaFields = new List<igMetaField>();
		}

		public override void PostUndump()
		{
			_vTablePointer = igArkCore.GetObjectDotNetType(_name);

			//Sometimes is null, in those cases, a dynamic type will be created
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
		public void InheritFields()
		{
			if(_parent == null) return;
			_metaFields.AddRange(_parent._metaFields);
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
			igMetaField[] metaFieldsByOffset = _metaFields.OrderBy(x => x._offset).ToArray();
			ushort currentOffset = (ushort)(4u + igAlchemyCore.GetPointerSize(platform));
			for(int i = 0; i < metaFieldsByOffset.Length; i++)
			{
				if(metaFieldsByOffset[i] is igStaticMetaField) continue;
				if(metaFieldsByOffset[i] is igPropertyFieldMetaField) continue;
				if(metaFieldsByOffset[i] is igBitFieldMetaField) continue;
				if(metaFieldsByOffset[i]._offsets.ContainsKey(platform)) continue;

				Align(ref currentOffset, metaFieldsByOffset[i].GetAlignment(platform));

				metaFieldsByOffset[i]._offsets.Add(platform, currentOffset);

				currentOffset += (ushort)metaFieldsByOffset[i].GetSize(platform);
			}
		}
		private void Align(ref ushort offset, uint alignment)
		{
			offset = (ushort)(((offset + (alignment - 1)) / alignment) * alignment);
		}

		protected override void TypeBuildDeclare()
		{
			if(_beganTypeDeclaration) return;
			if(_vTablePointer != null)
			{
				_beganTypeDeclaration = true;
				return;
			}
			_beganTypeDeclaration = true;

			igArkCore._pendingTypes.Push(this);
		}
		protected override void TypeBuildDefine()
		{
			if(_beganTypeDefiniton) return;
			if(!(_vTablePointer is TypeBuilder))
			{
				_beganTypeDefiniton = true;
				return;
			}
			_beganTypeDefiniton = true;

			this._parent.TypeBuild();

			for(int i = 0; i < _metaFields.Count; i++)
			{
				_metaFields[i].ReadyOutputType();
			}
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

			_vTablePointer = igArkCore.GetNewTypeBuilder(_name);
		}
		//It'll probably be a good idea to add safety checks for the parent class, but __internalObjectBase doesn't every deal with this so yeah
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

			Type parentType = _parent._vTablePointer;

			if(_parent._name == "igDataList")
			{
				igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];

				parentType = typeof(igTDataList<>).MakeGenericType(dataField._memType.GetOutputType());
			}
			else if(_parent._name == "igObjectList")
			{
				igMemoryRefMetaField dataField = (igMemoryRefMetaField)_metaFields[2];

				parentType = typeof(igTObjectList<>).MakeGenericType(dataField._memType.GetOutputType());
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

			ReadyDependancyAndBlock(_parent);

			uint parentNameHash = igHash.Hash(_parent._name);
			if(_vTablePointer.BaseType.IsGenericType)
			{
				//If _parent._name is "igObjectList" or "igNonRefCountedObjectList"
				if((_parent._name.Length == 12 && parentNameHash == 0x84D79B42) || (_parent._name.Length == 25 && parentNameHash == 0xD3BD6FDE))
				{
					ReadyDependancyAndBlock(((igObjectRefMetaField)((igMemoryRefMetaField)_metaFields[2])._memType)._metaObject);
				}
			}

			TypeBuilder tb = (TypeBuilder)_vTablePointer;

			//if _parent._name is "igCompoundMetaField"
			if(_parent._name.Length == 19 && parentNameHash == 0x9AEC2E67)
			{
				ConstructorBuilder cb = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
				ILGenerator ilGen = cb.GetILGenerator();
				ilGen.Emit(OpCodes.Ldarg_0);																			//Load "this"
				ilGen.Emit(OpCodes.Call, CompoundFieldConstructor);				//Call the base constructor
				ilGen.Emit(OpCodes.Nop);
				ilGen.Emit(OpCodes.Nop);
				ilGen.Emit(OpCodes.Ldarg_0);									//Load "this"
				ilGen.Emit(OpCodes.Ldstr, _name);								//Load the name of this class as a string
				ilGen.Emit(OpCodes.Call, GetCompoundFieldInfoMethodInfo);		//Get the igCompoundFieldInfo
				ilGen.Emit(OpCodes.Stfld, CompoundFieldInfoFieldInfo);			//Assign the value
				ilGen.Emit(OpCodes.Ret);										//Return
			}
			else
			{
				tb.DefineDefaultConstructor(MethodAttributes.Public);
			}

			Type testType = ((TypeBuilder)_vTablePointer).CreateType();
			igArkCore.AddDynamicTypeToCache(testType);
			_vTablePointer = testType;
			_finishedTypeFinalization = true;
		}
		private void ReadyDependancyAndBlock(igMetaObject dependancy)
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
	}
}