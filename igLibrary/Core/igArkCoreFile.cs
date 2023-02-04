namespace igLibrary.Core
{
	public class igArkCoreFile
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
		}

		public igArkCoreFile()
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
				igMetaEnum metaEnum = ReadMetaEnum();
				igArkCore._metaEnums.Add(metaEnum);
				_metaEnumsInFile.Add(metaEnum);
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
				compoundInfo._name = ReadString(_shs[Section.CompoundInst]);
				_compoundsInFile.Add(compoundInfo);
				igArkCore._compoundFieldInfos.Add(compoundInfo);
			}

			_shs[Section.ObjectInst].Seek(0);
			for(int i = 0; i < metaObjectCount; i++)
			{
				igMetaObject metaObject = new igMetaObject();
				metaObject._name = ReadString(_shs[Section.ObjectInst]);
				_metaObjectsInFile.Add(metaObject);
				igArkCore._metaObjects.Add(metaObject);
			}
		}
		public void SaveMetaObject(igMetaObject metaObject)
		{
			SaveString(_shs[Section.ObjectInst], metaObject._name);
			SaveString(_shs[Section.ObjectInfo], metaObject._parent != null ? metaObject._parent._name : null);
			_shs[Section.ObjectInfo].WriteInt32(metaObject._metaFields.Count);

			for(int i = 0; i < metaObject._metaFields.Count; i++)
			{
				SaveMetaField(_shs[Section.ObjectInfo], metaObject._metaFields[i]);
			}

			_metaObjectsInFile.Add(metaObject);
		}
		private void ReadMetaObject()
		{
			_shs[Section.ObjectInfo].Seek(0);
			for(int i = 0; i < _metaObjectsInFile.Count; i++)
			{
				igMetaObject metaObject = _metaObjectsInFile[i];
				string? parentName = ReadString(_shs[Section.ObjectInfo]);
				if(parentName != null)
				{
					metaObject._parent = igArkCore.GetObjectMeta(parentName);
				}
				int fieldCount = _shs[Section.ObjectInfo].ReadInt32();
				metaObject._metaFields.Capacity = fieldCount;
				for(int j = 0; j < fieldCount; j++)
				{
					metaObject._metaFields.Add(ReadMetaField(_shs[Section.ObjectInfo]));
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
			Type? t = igArkCore.GetObjectDotNetType(typeName);
			if(t == null)
			{
				t = typeof(igPlaceHolderMetaField);
			}
			igMetaField metaField = (igMetaField)Activator.CreateInstance(t);
			if(metaField is igPlaceHolderMetaField placeHolder)
			{
				placeHolder._typeName = typeName;
			}
			metaField.UndumpArkData(this, sh);
			return metaField;
		}
		public void SaveCompoundInfo(igCompoundMetaFieldInfo compoundInfo)
		{
			SaveString(_shs[Section.CompoundInst], compoundInfo._name);
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
					Console.WriteLine($"Compound {i}, metafield {j} @ {GetOffset(Section.CompoundInfo).ToString("X08")}");
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
}