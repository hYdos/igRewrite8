using System.Reflection.Emit;
using System.Reflection;

namespace igLibrary.Core
{
	public class igCompoundMetaField : igMetaField
	{
		public igCompoundMetaFieldInfo _compoundFieldInfo;

		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);
			//string typeName = loader.ReadString(sh);
			//_compoundFieldInfo = igArkCore.GetCompoundFieldInfo(typeName);
		}
		public override Type GetOutputType() => _compoundFieldInfo._vTablePointer;
	}

	public class igCompoundMetaFieldInfo : igBaseMeta
	{
		public List<igMetaField> _fieldList = new List<igMetaField>();
		public Type _vTablePointer;

		public override void PostUndump()
		{
			_vTablePointer = igArkCore.GetStructDotNetType(_name.Substring(0, _name.Length-9));

			//Sometimes is null, in those cases, a dynamic type will be created
		}

		public void CalculateOffsets()
		{
			IG_CORE_PLATFORM[] platforms = Enum.GetValues<IG_CORE_PLATFORM>();
			for(int i = 0; i < platforms.Length; i++)
			{
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT) continue;
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED) continue;
				if(platforms[i] == IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX) continue;

				CalculateOffsetForPlatform(platforms[i]);
			}
		}
		public void CalculateOffsetForPlatform(IG_CORE_PLATFORM platform)
		{
			igMetaField[] metaFieldsByOffset = _fieldList.OrderBy(x => x._offset).ToArray();
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
		public void TypeBuildBegin()
		{
			if(_vTablePointer != null) return;

			TypeBuilder tb = igArkCore.GetNewStructTypeBuilder(_name.Substring(0, _name.Length - 9));		//Trim off the "MetaField"
			_vTablePointer = tb;
		}
		public void TypeBuildAddFields()
		{
			if(_vTablePointer.GetType() != typeof(TypeBuilder)) return;

			TypeBuilder tb = (TypeBuilder)_vTablePointer;

			if(_name == "igOrderedMapMetaField") return;	//This one's generic!

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
			if(_vTablePointer.GetType() != typeof(TypeBuilder)) return;

			for(int i = 0; i < _fieldList.Count; i++)
			{
				if(_fieldList[i] is igCompoundMetaField compoundMetaField)
				{
					Type outputType = compoundMetaField.GetOutputType();
					if(outputType is TypeBuilder)
					{
						compoundMetaField._compoundFieldInfo.TypeBuildFinalize();
					}
				}
			}

			Type testType = ((TypeBuilder)_vTablePointer).CreateType();
			//igArkCore.AddDynamicTypeToCache(testType);
			_vTablePointer = testType;
		}
	}

	public class igStruct : Attribute{}
}