namespace igLibrary.Core
{
	public class igArkCoreFile
	{
		private StreamHelper _sh;
		private List<string?> _stringTable;
		public List<igMetaObject> _metaObjectsInFile;
		public List<igCompoundMetaFieldInfo> _compoundsInFile;
		public List<igMetaEnum> _metaEnumsInFile;
		public List<igMetaFieldPlatformInfo> _metaFieldPlatformInfosInFile;
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

		public igArkCoreFile()
		{
			_stringTable = new List<string?>();
			_metaObjectsInFile = new List<igMetaObject>();
			_compoundsInFile = new List<igCompoundMetaFieldInfo>();
			_metaEnumsInFile = new List<igMetaEnum>();
			_metaFieldPlatformInfosInFile = new List<igMetaFieldPlatformInfo>();
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
			int metaFieldPlatformCount = _sh.ReadInt32();

			Section[] sections = typeof(Section).GetEnumValues().Cast<Section>().ToArray();
			for(int i = 0; i < sections.Length; i++)
			{
				_sh.Seek(0x40 + i * 8);

				uint offset = _sh.ReadUInt32();
				uint length = _sh.ReadUInt32();

				_sh.Seek(offset);
				_shs[sections[i]] = new StreamHelper(new MemoryStream(_sh.ReadBytes(length)));
				_shs[sections[i]].Seek(0);
				_offsets.Add(sections[i], offset);
			}

			ReadStringTable(stringCount);
			InstantiateCompounds(compoundCount);
			InstantiateObjectMetas(metaObjectCount);
			_metaEnumsInFile.Capacity = metaEnumCount;
			for(int i = 0; i < metaEnumCount; i++)
			{
				igMetaEnum metaEnum = ReadMetaEnum();
				if(metaEnum == null) continue;
				igArkCore._metaEnums.Add(metaEnum);
				_metaEnumsInFile.Add(metaEnum);
			}
			ReadPlatformInfos(metaFieldPlatformCount);
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

		private void InstantiateCompounds(int compoundCount)
		{
			_shs[Section.CompoundInst].Seek(0);
			for(int i = 0; i < compoundCount; i++)
			{
				igCompoundMetaFieldInfo compoundInfo = new igCompoundMetaFieldInfo();
				compoundInfo._name = ReadString(_shs[Section.CompoundInst]);
				_compoundsInFile.Add(compoundInfo);
				igArkCore._compoundFieldInfos.Add(compoundInfo);
			}
		}

		private void InstantiateObjectMetas(int metaObjectCount)
		{
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
			
			int fieldCount = metaObject._metaFields.Count;
			int firstField = 0;
			if(metaObject._parent != null)
			{
				if(metaObject._parent._name == "igDataList")
				{
					igMemoryRefMetaField _data = (igMemoryRefMetaField)metaObject._metaFields[2];
					SaveMetaField(_shs[Section.ObjectInfo], _data._memType);
				}
				else if(metaObject._parent._name == "igObjectList" || metaObject._parent._name == "igNonRefCountedObjectList")
				{
					igMemoryRefMetaField _data = (igMemoryRefMetaField)metaObject._metaFields[2];
					igMetaObject _metaObject  = ((igObjectRefMetaField)_data._memType)._metaObject;
					SaveString(_shs[Section.ObjectInfo], _metaObject._name);
				}
				else if(metaObject._parent._name == "igHashTable")
				{
					igMemoryRefMetaField _values = (igMemoryRefMetaField)metaObject._metaFields[0];
					igMemoryRefMetaField _keys = (igMemoryRefMetaField)metaObject._metaFields[1];
					SaveMetaField(_shs[Section.ObjectInfo], _values._memType);
					SaveMetaField(_shs[Section.ObjectInfo], _keys._memType);
				}
				firstField = metaObject._parent._metaFields.Count;
			}

			_shs[Section.ObjectInfo].WriteInt32(fieldCount - firstField);

			for(int i = firstField; i < fieldCount; i++)
			{
				SaveMetaField(_shs[Section.ObjectInfo], metaObject._metaFields[i]);
			}

			_metaObjectsInFile.Add(metaObject);
		}
		private void ReadPlatformInfos(int platformInfoCount)
		{
			_shs[Section.PlatformInfo].Seek(0);
			for(int i = 0; i < platformInfoCount; i++)
			{
				igMetaFieldPlatformInfo metaFieldPlatformInfo = new igMetaFieldPlatformInfo();
				metaFieldPlatformInfo._name = ReadString(_shs[Section.PlatformInfo]);
				uint platformCount = _shs[Section.PlatformInfo].ReadUInt32();
				for(int j = 0; j < platformCount; j++)
				{
					string platformName = ReadString(_shs[Section.PlatformInfo]);
					ushort size = _shs[Section.PlatformInfo].ReadUInt16();
					ushort align = _shs[Section.PlatformInfo].ReadUInt16();
					IG_CORE_PLATFORM platformValue = Enum.Parse<IG_CORE_PLATFORM>(platformName);
					metaFieldPlatformInfo._sizes.Add(platformValue, size);
					metaFieldPlatformInfo._alignments.Add(platformValue, align);
				}
				_metaFieldPlatformInfosInFile.Add(metaFieldPlatformInfo);
				igArkCore._metaFieldPlatformInfos.Add(metaFieldPlatformInfo);
			}
		}
		public void SaveMetaFieldPlatformInfo(igMetaFieldPlatformInfo metaFieldPlatformInfo)
		{
			SaveString(_shs[Section.PlatformInfo], metaFieldPlatformInfo._name);
			_shs[Section.PlatformInfo].WriteInt32(metaFieldPlatformInfo._sizes.Count);
			for(int j = 0; j < metaFieldPlatformInfo._sizes.Count; j++)
			{
				string platformName = metaFieldPlatformInfo._sizes.ElementAt(j).Key.ToString();
				SaveString(_shs[Section.PlatformInfo], platformName);

				ushort size = metaFieldPlatformInfo._sizes.ElementAt(j).Value;
				_shs[Section.PlatformInfo].WriteUInt16(size);

				ushort align = metaFieldPlatformInfo._alignments.ElementAt(j).Value;
				_shs[Section.PlatformInfo].WriteUInt16(align);
			}
			_metaFieldPlatformInfosInFile.Add(metaFieldPlatformInfo);
		}
		private void ReadMetaObject()
		{
			_shs[Section.ObjectInfo].Seek(0);
			for(int i = 0; i < _metaObjectsInFile.Count; i++)
			{
				if(_metaObjectsInFile[i]._name == "igTraversalInstance") 
					_shs = _shs;
				igMetaObject metaObject = _metaObjectsInFile[i];
				string? parentName = ReadString(_shs[Section.ObjectInfo]);
				if(parentName != null)
				{
					metaObject._parent = igArkCore.GetObjectMeta(parentName);
					metaObject.InheritFields();
					if(metaObject._parent._name == "igDataList")
					{
						igMemoryRefMetaField _data = (igMemoryRefMetaField)metaObject._metaFields[2].CreateFieldCopy();
						_data._memType = ReadMetaField(_shs[Section.ObjectInfo]);
						metaObject._metaFields[2] = _data;
					}
					else if(metaObject._parent._name == "igObjectList" || metaObject._parent._name == "igNonRefCountedObjectList")
					{
						igMemoryRefMetaField _data = (igMemoryRefMetaField)metaObject._metaFields[2].CreateFieldCopy();
						Console.WriteLine($"igObjectList child at {GetOffset(Section.ObjectInfo).ToString("X08")}");
						((igObjectRefMetaField)_data._memType)._metaObject = igArkCore.GetObjectMeta(ReadString(_shs[Section.ObjectInfo]));
						metaObject._metaFields[2] = _data;
					}
					else if(metaObject._parent._name == "igHashTable")
					{
						igMemoryRefMetaField _values = (igMemoryRefMetaField)metaObject._metaFields[0].CreateFieldCopy();
						igMemoryRefMetaField _keys   = (igMemoryRefMetaField)metaObject._metaFields[1].CreateFieldCopy();
						_values._memType = ReadMetaField(_shs[Section.ObjectInfo]);
						  _keys._memType = ReadMetaField(_shs[Section.ObjectInfo]);
						metaObject._metaFields[0] = _values;
						metaObject._metaFields[1] = _keys;
					}
				}
				int fieldCount = _shs[Section.ObjectInfo].ReadInt32();
				metaObject._metaFields.Capacity += fieldCount;
				for(int j = 0; j < fieldCount; j++)
				{
					metaObject._metaFields.Add(ReadMetaField(_shs[Section.ObjectInfo]));
				}
				metaObject.PostUndump();
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
			if(metaEnum._name == null || metaEnum._name.Length == 0)
				return null;
			metaEnum.PostUndump();
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
			igCompoundMetaFieldInfo? compoundFieldInfo = null;
			if(t == null)
			{
				compoundFieldInfo = igArkCore.GetCompoundFieldInfo(typeName);
				if(compoundFieldInfo != null)
				{
					t = typeof(igCompoundMetaField);
				}
				else
				{
					t = typeof(igPlaceHolderMetaField);
				}
			}
			igMetaField metaField = (igMetaField)Activator.CreateInstance(t);
			if(metaField is igPlaceHolderMetaField placeHolder)
			{
				placeHolder._platformInfo = igArkCore.GetMetaFieldPlatformInfo(typeName);
			}
			else if(metaField is igCompoundMetaField compoundField)
			{
				compoundField._compoundFieldInfo = compoundFieldInfo;
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
					//Console.WriteLine($"Compound {i}, metafield {j} @ {GetOffset(Section.CompoundInfo).ToString("X08")}");
					compoundInfo._fieldList.Add(ReadMetaField(_shs[Section.CompoundInfo]));
				}
				compoundInfo.PostUndump();
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
			_sh.WriteInt32(_metaFieldPlatformInfosInFile.Count);

			Section[] sections = typeof(Section).GetEnumValues().Cast<Section>().ToArray();
			for(int i = 0; i < sections.Length; i++)
			{
				_sh.Seek(0x40 + i * 8);

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
			if(index == 7599)
				index = index;
			return _stringTable[index];
		}
		public void Dispose()
		{
			Section[] sections = typeof(Section).GetEnumValues().Cast<Section>().ToArray();
			for(int i = 0; i < sections.Length; i++)
			{
				_shs[sections[i]].Close();
				_shs[sections[i]].Dispose();
			}
			_sh.Close();
			_sh.Dispose();
		}
	}
}