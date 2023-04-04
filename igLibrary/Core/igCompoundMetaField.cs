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

			object compoundData = Activator.CreateInstance(_compoundFieldInfo._vTablePointer);

			_compoundFieldInfo.CalculateOffsetForPlatform(loader._platform);

			for(int i = 0; i < metaFields.Count; i++)
			{
				if(metaFields[i] is igStaticMetaField) continue;
				if(metaFields[i] is igPropertyFieldMetaField) continue;

				loader._stream.Seek(objectOffset + metaFields[i]._offsets[loader._platform]);

				object? data = metaFields[i].ReadIGZField(loader);

				FieldInfo? field = _compoundFieldInfo._vTablePointer.GetField(metaFields[i]._name);
				if(field != null)
				{
					field.SetValue(compoundData, data);
				}
			}

			return compoundData;
		}

		public override Type GetOutputType()
		{
			if(_compoundFieldInfo._vTablePointer == null) return typeof(object);
			else return _compoundFieldInfo._vTablePointer;
		}

		public override uint GetAlignment(IG_CORE_PLATFORM platform) => _compoundFieldInfo._platformInfo._alignments[platform];
		public override uint GetSize(IG_CORE_PLATFORM platform) => _compoundFieldInfo._platformInfo._sizes[platform];
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
	}

	public class igStruct : Attribute{}
}