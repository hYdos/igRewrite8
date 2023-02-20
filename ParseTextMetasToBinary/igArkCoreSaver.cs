namespace PTMTB
{
	/*
		MetaFields require:
		- Type Name
		- Offset
		- Field Name
		- Template Parameters
		- If:
			- ObjectRef: require MetaObject
			- Handle: require MetaObject
			- MemoryRef: require Memory Type
			- MemoryRefHandle: require Memory Type
			- BitField: require shift, bits, and assignment MetaField
			- Enum: require MetaEnum
			- Static: require storage MetaField
			- Property: require inner MetaField
			- Array: require num
			- Compound: require some sort of lookup for compound fields

		Compounds require:
		- Name for looking up
		- Fields

		MetaObjects require:
		- Parent
		- Fields
		MetaObjects will have to be instantiated beforehand

		Should go:
		1) MetaObject instantiate pass
		2) CompoundField instantiate pass
		2) CompoundField pass
		3) MetaObject pass
	*/

	public class igArkCoreSaver2
	{
		private StreamHelper _sh;
		private List<string?> _stringTable;
		public List<igMetaObject> _metaObjectsInFile;
		public List<igCompoundMetaFieldInfo> _compoundsInFile;
		public List<igMetaEnum> _metaEnumsInFile;
		private Dictionary<Section, StreamHelper> _shs;
		private Dictionary<Section, uint> _offsets;

		private enum Section : int
		{
			Strings,
			CompoundInst,
			ObjectInst,
			CompoundInfo,
			ObjectInfo,
			EnumInfo,
			PlatformInfo,
		}

		public igArkCoreSaver2()
		{
			_stringTable = new List<string?>();
			_metaObjectsInFile = new List<igMetaObject>();
			_compoundsInFile = new List<igCompoundMetaFieldInfo>();
			_metaEnumsInFile = new List<igMetaEnum>();
			_shs = new Dictionary<Section, StreamHelper>(6);
			_offsets = new Dictionary<Section, uint>(6);
			_shs.Add(Section.Strings, null);
			_shs.Add(Section.CompoundInst, null);
			_shs.Add(Section.ObjectInst, null);
			_shs.Add(Section.CompoundInfo, null);
			_shs.Add(Section.ObjectInfo, null);
			_shs.Add(Section.EnumInfo, null);
			_shs.Add(Section.PlatformInfo, null);
		}

		uint GetOffset(Section section)
		{
			return _offsets[section] + _shs[section].Tell();
		}
		public void BeginSave(string filePath)
		{
			_sh = new StreamHelper(new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite));
			_shs[Section.Strings] = new StreamHelper(new MemoryStream());
			_shs[Section.CompoundInst] = new StreamHelper(new MemoryStream());
			_shs[Section.ObjectInst] = new StreamHelper(new MemoryStream());
			_shs[Section.CompoundInfo] = new StreamHelper(new MemoryStream());
			_shs[Section.ObjectInfo] = new StreamHelper(new MemoryStream());
			_shs[Section.EnumInfo] = new StreamHelper(new MemoryStream());
			_shs[Section.PlatformInfo] = new StreamHelper(new MemoryStream());
		}

		public void ReadFile(string filePath)
		{
			_sh = new StreamHelper(new FileStream(filePath, FileMode.Open, FileAccess.Read));

			_sh.Seek(0);
			uint cookie = _sh.ReadUInt32();
			uint version = _sh.ReadUInt32();
			_sh.Seek(0x10);
			int stringCount = _sh.ReadInt32();
			int compoundCount = _sh.ReadInt32();
			int metaObjectCount = _sh.ReadInt32();
			int metaEnumCount = _sh.ReadInt32();

			Section[] sections = typeof(Section).GetEnumValues().Cast<Section>().ToArray();
			for(int i = 0; i < sections.Length; i++)
			{
				_sh.Seek(0x20 + i * 8);

				uint offset = _sh.ReadUInt32();
				uint length = _sh.ReadUInt32();

				_sh.Seek(offset);
				_shs[sections[i]] = new StreamHelper(new MemoryStream(_sh.ReadBytes(length)));
				_shs[sections[i]].Seek(0);
				_offsets.Add(sections[i], offset);
			}

			ReadStringTable(stringCount);
			InstantiateMetas(compoundCount, metaObjectCount);
			_metaEnumsInFile.Capacity = metaEnumCount;
			for(int i = 0; i < metaEnumCount; i++)
			{
				_metaEnumsInFile.Add(ReadMetaEnum());
			}
			ReadCompounds();
			ReadMetaObject();
		}
		private void ReadStringTable(int stringCount)
		{
			_shs[Section.Strings].Seek(0);
			_stringTable.Capacity = stringCount;
			for(int i = 0; i < stringCount; i++)
			{
				_stringTable.Add(_shs[Section.Strings].ReadString());
			}
		}

		private void InstantiateMetas(int compoundCount, int metaObjectCount)
		{
			_shs[Section.CompoundInst].Seek(0);
			for(int i = 0; i < compoundCount; i++)
			{
				igCompoundMetaFieldInfo compoundInfo = new igCompoundMetaFieldInfo();
				compoundInfo._fieldTypeName = ReadString(_shs[Section.CompoundInst]);
				_compoundsInFile.Add(compoundInfo);
			}

			_shs[Section.ObjectInst].Seek(0);
			for(int i = 0; i < metaObjectCount; i++)
			{
				igMetaObject metaObject = new igMetaObject();
				metaObject._name = ReadString(_shs[Section.ObjectInst]);
				_metaObjectsInFile.Add(metaObject);
			}
		}
		public void SaveMetaObject(igMetaObject metaObject)
		{
			SaveString(_shs[Section.ObjectInst], metaObject._name);
			SaveString(_shs[Section.ObjectInfo], metaObject._parentName);
			_shs[Section.ObjectInfo].WriteInt32(metaObject._fields.Count);

			for(int i = 0; i < metaObject._fields.Count; i++)
			{
				SaveMetaField(_shs[Section.ObjectInfo], metaObject._fields[i]);
			}

			_metaObjectsInFile.Add(metaObject);
		}
		private void ReadMetaObject()
		{
			_shs[Section.ObjectInfo].Seek(0);
			for(int i = 0; i < _metaObjectsInFile.Count; i++)
			{
				igMetaObject metaObject = _metaObjectsInFile[i];
				metaObject._parentName = ReadString(_shs[Section.ObjectInfo]);
				int fieldCount = _shs[Section.ObjectInfo].ReadInt32();
				metaObject._fields.Capacity = fieldCount;
				for(int j = 0; j < fieldCount; j++)
				{
					metaObject._fields.Add(ReadMetaField(_shs[Section.ObjectInfo]));
				}
			}
		}
		public void SaveMetaEnum(igMetaEnum metaEnum)
		{
			SaveString(_shs[Section.EnumInfo], metaEnum._name);
			_shs[Section.EnumInfo].WriteInt32(metaEnum._names.Count);
			for(int i = 0; i < metaEnum._names.Count; i++)
			{
				SaveString(_shs[Section.EnumInfo], metaEnum._names[i]);
				_shs[Section.EnumInfo].WriteInt32(metaEnum._values[i]);
			}
			_metaEnumsInFile.Add(metaEnum);
		}
		public igMetaEnum ReadMetaEnum()
		{
			igMetaEnum metaEnum = new igMetaEnum();
			metaEnum._name = ReadString(_shs[Section.EnumInfo]);
			int memberCount = _shs[Section.EnumInfo].ReadInt32();
			metaEnum._names.Capacity = memberCount;
			metaEnum._values.Capacity = memberCount;
			for(int i = 0; i < memberCount; i++)
			{
				metaEnum._names.Add(ReadString(_shs[Section.EnumInfo]));
				metaEnum._values.Add(_shs[Section.EnumInfo].ReadInt32());
			}
			return metaEnum;
		}
		public void SaveMetaField(StreamHelper sh, igMetaField? metaField)
		{
			if(metaField == null) sh.WriteInt32(-1);
			else                  metaField.DumpArkData(this, sh);
		}
		public igMetaField? ReadMetaField(StreamHelper sh)
		{
			string? typeName = ReadString(sh);
			if(typeName == null) return null;
			typeName = "PTMTB." + typeName;
			Type t = Type.GetType(typeName);
			if(t == null)
			{
				t = typeof(igMetaField);
			}
			igMetaField metaField = (igMetaField)Activator.CreateInstance(t);
			metaField._typeName = typeName;
			metaField.UndumpArkData(this, sh);
			return metaField;
		}
		public void SaveCompoundInfo(igCompoundMetaFieldInfo compoundInfo)
		{
			SaveString(_shs[Section.CompoundInst], compoundInfo._fieldTypeName);
			_shs[Section.CompoundInfo].WriteInt32(compoundInfo._fieldList.Count);

			for(int i = 0; i < compoundInfo._fieldList.Count; i++)
			{
				SaveMetaField(_shs[Section.CompoundInfo], compoundInfo._fieldList[i]);
			}

			_compoundsInFile.Add(compoundInfo);
		}
		private void ReadCompounds()
		{
			_shs[Section.CompoundInfo].Seek(0);
			for(int i = 0; i < _compoundsInFile.Count; i++)
			{
				igCompoundMetaFieldInfo compoundInfo = _compoundsInFile[i];
				int fieldCount = _shs[Section.CompoundInfo].ReadInt32();
				compoundInfo._fieldList.Capacity = fieldCount;
				for(int j = 0; j < fieldCount; j++)
				{
					compoundInfo._fieldList.Add(ReadMetaField(_shs[Section.CompoundInfo]));
				}
			}
		}
		public void FinishSave()
		{
			_sh.Seek(0);
			_sh.WriteUInt32(igArkCore._magicCookie);
			_sh.WriteUInt32(igArkCore._magicVersion);
			_sh.Seek(0x10);
			_sh.WriteInt32(_stringTable.Count);
			_sh.WriteInt32(_compoundsInFile.Count);
			_sh.WriteInt32(_metaObjectsInFile.Count);
			_sh.WriteInt32(_metaEnumsInFile.Count);

			Section[] sections = typeof(Section).GetEnumValues().Cast<Section>().ToArray();
			for(int i = 0; i < sections.Length; i++)
			{
				_sh.Seek(0x20 + i * 8);

				_shs[sections[i]].BaseStream.Flush();

				uint offset = (uint)_sh.BaseStream.Length;
				uint length = (uint)_shs[sections[i]].BaseStream.Length;
				if(offset < 0x80) offset = 0x80;

				_sh.WriteUInt32(offset);
				_sh.WriteUInt32(length);

				_sh.Seek(offset);
				_shs[sections[i]].Seek(0);
				_shs[sections[i]].BaseStream.CopyTo(_sh.BaseStream);

				_sh.BaseStream.Flush();
			}
		}
		public void SaveString(StreamHelper sh, string? data)
		{
			int index = -1;

			if(data != null)
			{
				index = _stringTable.FindIndex(x => x == data);
				if(index < 0)
				{
					index = _stringTable.Count;
					_stringTable.Add(data);
					_shs[Section.Strings].WriteString(data);
				}
			}

			sh.WriteInt32(index);
		}
		public string? ReadString(StreamHelper sh)
		{
			int index = sh.ReadInt32();
			if(index < 0) return null;
			return _stringTable[index];
		}
	}
	public class igArkCoreSaver
	{
		private StreamHelper sh;
		private StreamHelper stringSh;
		private StreamHelper metaobjectPrepassSh;
		private StreamHelper metaobjectSh;
		private StreamHelper compoundFieldSh;
		private StreamHelper metaenumSh;
		private igArkCore.EGame _game;
		private List<string?> _stringTable;
		private uint savedMetaObjects = 0;
		private uint savedMetaEnums = 0;
		private uint savedCompoundFields = 0;

		public igArkCoreSaver(string filePath, igArkCore.EGame game)
		{
			sh = new StreamHelper(new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite));
			stringSh = new StreamHelper(new MemoryStream());
			metaobjectPrepassSh = new StreamHelper(new MemoryStream());
			metaobjectSh = new StreamHelper(new MemoryStream());
			compoundFieldSh = new StreamHelper(new MemoryStream());
			metaenumSh = new StreamHelper(new MemoryStream());
			_game = game;
			_stringTable = new List<string?>();
		}

		public void SaveMetaEnum(igMetaEnum metaEnum)
		{
			metaenumSh.WriteInt32(SaveString(metaEnum._name));
			metaenumSh.WriteInt32(metaEnum._names.Count);
			for(int i = 0; i < metaEnum._names.Count; i++)
			{
				metaenumSh.WriteInt32(SaveString(metaEnum._names[i]));
				metaenumSh.WriteInt32(metaEnum._values[i]);
			}
			savedMetaEnums++;
		}
		public void SaveMetaObject(igMetaObject meta)
		{
			metaobjectPrepassSh.WriteInt32(SaveString(meta._name));
			metaobjectSh.WriteInt32(SaveString(meta._parentName));
			metaobjectSh.WriteInt32(meta._fields.Count);
			for(int i = 0; i < meta._fields.Count; i++)
			{
				//meta._fields[i].DumpArkData(this, metaobjectSh);
			}
			savedMetaObjects++;
		}
		public void SaveCompoundMetaFieldFieldList(igCompoundMetaFieldInfo dumper)
		{
			//dumper.DumpArkData(this, compoundFieldSh);
			savedCompoundFields++;
		}

		public void FinishSave()
		{
			sh.WriteUInt32(igArkCore._magicCookie);
			sh.WriteUInt32(igArkCore._magicVersion);
			sh.WriteUInt32((uint)_game);

			stringSh.BaseStream.Flush();
			sh.WriteUInt32((uint)_stringTable.Count);
			sh.WriteUInt32((uint)stringSh.BaseStream.Length);
			sh.BaseStream.Write((stringSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)stringSh.BaseStream.Length);
			stringSh.BaseStream.Close();

			metaobjectPrepassSh.BaseStream.Flush();
			sh.WriteUInt32(savedMetaObjects);
			sh.BaseStream.Write((metaobjectPrepassSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)metaobjectPrepassSh.BaseStream.Length);
			metaobjectPrepassSh.BaseStream.Close();

			metaobjectSh.BaseStream.Flush();
			sh.WriteUInt32((uint)metaobjectSh.BaseStream.Length);
			sh.BaseStream.Write((metaobjectSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)metaobjectSh.BaseStream.Length);
			metaobjectSh.BaseStream.Close();

			compoundFieldSh.BaseStream.Flush();
			sh.WriteUInt32(savedCompoundFields);
			sh.WriteUInt32((uint)compoundFieldSh.BaseStream.Length);
			sh.BaseStream.Write((compoundFieldSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)compoundFieldSh.BaseStream.Length);
			compoundFieldSh.BaseStream.Close();

			metaenumSh.BaseStream.Flush();
			sh.WriteUInt32(savedMetaEnums);
			sh.WriteUInt32((uint)metaenumSh.BaseStream.Length);
			sh.BaseStream.Write((metaenumSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)metaenumSh.BaseStream.Length);
			metaenumSh.BaseStream.Close();

			sh.Close();
		}
		public int SaveString(string? str)
		{
			if(str == null) return -1;
			int index = _stringTable.FindIndex(x => x == str);
			if(index >= 0) return index;
			else
			{
				_stringTable.Add(str);
				stringSh.WriteString(str);
				return _stringTable.Count - 1;
			}
		}
	}
}