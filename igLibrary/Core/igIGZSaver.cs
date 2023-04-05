namespace igLibrary.Core
{
	public class igIGZSaver
	{
		//public Dictionary<igObject, ulong> _objectOffsetList = new Dictionary<igObject, ulong>();
		public List<igMetaObject> _vTableList = new List<igMetaObject>();
		public List<string> _stringList = new List<string>();
		private List<SaverSection> _sections = new List<SaverSection>();
		public IG_CORE_PLATFORM _platform;
		private StreamHelper _stream;
		private uint _version;
		private uint _fixupCount;
		private uint _fixupSize;

		public class SaverSection
		{
			public StreamHelper _sh;
			public igMemoryPool _pool;
			public igRuntimeFields _runtimeFields;
			public Dictionary<igObject, ulong> _objectOffsetList;
			public uint _fileOffset;
			public uint _fileSize;
			public SaverSection(igMemoryPool pool, IG_CORE_PLATFORM platform)
			{
				_sh = new StreamHelper(new MemoryStream(), igAlchemyCore.isPlatformBigEndian(platform) ? StreamHelper.Endianness.Big: StreamHelper.Endianness.Little);
				_pool = pool;
				_runtimeFields = new igRuntimeFields();
				_objectOffsetList = new Dictionary<igObject, ulong>();
			}
			public ulong FindFreeMemory(byte alignment)
			{
				return (ulong)(_sh.BaseStream.Length & ~0b111) + 8;
			}
			public ulong Malloc(uint size)
			{
				ulong offset = FindFreeMemory(8);
				_sh.Seek(offset);
				for(int i = 0; i < size; i++)
				{
					_sh.WriteByte(0);
				}
				return offset;
			}
		}
		public void WriteFile(igObjectDirectory dir, string path)
		{
			_platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3;
			_version = 0x09;
			_stream = new StreamHelper(File.Create(path), StreamHelper.Endianness.Big);

			SaverSection rootSection = GetSaverSection(dir._objectList.internalMemoryPool);
			rootSection._runtimeFields._objectLists.Add(SaveObject(dir._objectList));

			WriteFixupSections(dir);
			WriteHeader();

			WriteOutSections();

			_stream.BaseStream.Close();
		}

		public ulong SaveObject(igObject? obj)
		{
			if(obj == null) return 0;

			SaverSection section = GetSaverSection(obj.internalMemoryPool);

			bool previouslyWritten = section._objectOffsetList.TryGetValue(obj, out ulong offset);
			if(previouslyWritten) return offset;

			igMetaObject meta = obj.GetMeta();

			offset = section.Malloc(meta._sizes[_platform]);
			section._sh.Seek(offset);

			WriteVTable(meta, section);
			section._sh.WriteUInt32(obj.refCount);

			section._sh.Seek(offset);

			obj.WriteIGZFields(this, section);

			return offset;
		}

		private void WriteHeader()
		{
			_stream.Seek(0);
			_stream.WriteUInt32(igIGZLoader._littleMagicCookie);
			_stream.WriteUInt32(_version);
			_stream.WriteUInt32(0);			//SerializableFieldsHash
			_stream.WriteInt32(igArkCore.GetMetaEnum("IG_CORE_PLATFORM").GetValueFromEnum(_platform));
			_stream.WriteUInt32(_fixupCount);
		}
		private void WriteOutSections()
		{
			int memPoolNameOffset = 0;
			uint memoryOffset = 0x800 + _fixupSize;

			//Write out fixup section
			_stream.Seek(0x14);
			_stream.WriteUInt32(0);
			_stream.WriteUInt32(0x800);
			_stream.WriteUInt32(_fixupSize);
			_stream.WriteUInt32(0x800);

			for(int i = 0; i < _sections.Count; i++)
			{
				_stream.Seek(0x24 + i * 0x10);
				uint unknown = 0;
				_stream.WriteUInt32((uint)(memPoolNameOffset & 0xFFFF) | ((unknown << 16) & 0xFFFF));
				_stream.WriteUInt32(memoryOffset);

				_stream.Seek(memoryOffset);
				_sections[i]._sh.BaseStream.Flush();
				_sections[i]._sh.Seek(0);
				_sections[i]._sh.BaseStream.CopyTo(_stream.BaseStream);

				memPoolNameOffset += _sections[i]._pool._name.Length;
				memoryOffset += (uint)_sections[i]._sh.BaseStream.Length;

				_sections[i]._sh.BaseStream.Close();
			}

			_stream.Seek(0x24 + 0x10 * 0x20);
			for(int i = 0; i < _sections.Count; i++)
			{
				_stream.WriteString(_sections[i]._pool._name);
			}
		}
		private void WriteVTable(igMetaObject meta, SaverSection section)
		{
			int index = _vTableList.FindIndex(x => x == meta);
			if(index < 0)
			{
				index = _vTableList.Count;
				_vTableList.Add(meta);
			}
			section._runtimeFields._vtables.Add(section._sh.Tell());
			WriteRawOffset((ulong)index, section);
		}

		public SaverSection GetSaverSection(igMemoryPool pool)
		{
			int index = _sections.FindIndex(x => x._pool == pool);
			SaverSection ret;
			if(index < 0)
			{
				ret = new SaverSection(pool, _platform);
				_sections.Add(ret);
			}
			else
			{
				ret = _sections[index];
			}
			return ret;
		}
		public void WriteFixupSections(igObjectDirectory dir)
		{
			ulong endOffset = 0x800;
			ulong startOffset = 0x800;
			_stream.Seek(startOffset);

			//TSTR
			if(_stringList.Count > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x52545354);
				_stream.WriteInt32(_stringList.Count);
				_stream.WriteInt32(0);
				_stream.WriteInt32(0x10);
				for(int j = 0; j < _stringList.Count; j++)
				{
					long basePos = _stream.BaseStream.Position;
					_stream.WriteString(_stringList[j]);

					int bits = (_version > 7) ? 2 : 1;
					_stream.Seek(basePos + bits + (_stringList[j].Length & (uint)(-bits)));
				}
				endOffset = Align(_stream.Tell(), 4);
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(endOffset - startOffset));
				_stream.Seek(endOffset);
				_fixupCount++;
			}

			//TMET
			if(_vTableList.Count > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x54454D54);
				_stream.WriteInt32(_vTableList.Count);
				_stream.WriteInt32(0);
				_stream.WriteInt32(0x10);
				for(int j = 0; j < _vTableList.Count; j++)
				{
					long basePos = _stream.BaseStream.Position;
					_stream.WriteString(_vTableList[j]._name);

					int bits = (_version > 7) ? 2 : 1;
					_stream.Seek(basePos + bits + (_vTableList[j]._name.Length & (uint)(-bits)));
				}
				endOffset = Align(_stream.Tell(), 4);
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(endOffset - startOffset));
				_stream.Seek(endOffset);

				startOffset = endOffset;
				_stream.WriteUInt32(0x5A53544D);
				_stream.WriteInt32(_vTableList.Count);
				_stream.WriteInt32(_vTableList.Count * 4 + 0x10);
				_stream.WriteInt32(0x10);
				for(int j = 0; j < _vTableList.Count; j++)
				{
					_stream.WriteUInt32(_vTableList[j]._sizes[_platform]);
				}
				endOffset = startOffset + 0x10u + (uint)_vTableList.Count * 4u;
				_stream.Seek(endOffset);
				_fixupCount += 2;
			}

			WriteRuntimeFixup(0x42545652, ref startOffset, ref endOffset, x => x._runtimeFields._vtables);
			WriteRuntimeFixup(0x544F4F52, ref startOffset, ref endOffset, x => x._runtimeFields._objectLists);
			WriteRuntimeFixup(0x52545352, ref startOffset, ref endOffset, x => x._runtimeFields._stringRefs);
			WriteRuntimeFixup(0x54545352, ref startOffset, ref endOffset, x => x._runtimeFields._stringTables);
			WriteRuntimeFixup(0x53464F52, ref startOffset, ref endOffset, x => x._runtimeFields._offsets);

			_stream.Seek(endOffset);
			_fixupSize = (uint)endOffset - 0x800u;
		}
		private void WriteRuntimeFixup(uint magic, ref ulong startOffset, ref ulong endOffset, Func<SaverSection, List<ulong>> getRuntimeFunc)
		{
			startOffset = endOffset;
			byte[] compressedData = GetRuntimeFixupData(getRuntimeFunc, out uint size, out uint count);
			if(count == 0) return;
			_stream.WriteUInt32(magic);
			_stream.WriteInt32(0);
			_stream.WriteInt32(0);
			_stream.WriteInt32(0x10);
			_stream.BaseStream.Write(compressedData);
			_stream.Seek(startOffset + 4);
			_stream.WriteUInt32(count);
			_stream.WriteUInt32(size + 0x10);
			endOffset = Align(startOffset + 0x10 + size, 4);
			_stream.Seek(endOffset);
			_fixupCount++;
		}
		private byte[] GetRuntimeFixupData(Func<SaverSection, List<ulong>> getRuntimeFunc, out uint size, out uint count)
		{
			List<ulong> offsets = new List<ulong>();
			for(int i = 0; i < _sections.Count; i++)
			{
				List<ulong> sectionOffsets = getRuntimeFunc.Invoke(_sections[i]);
				offsets.Capacity += sectionOffsets.Count;
				sectionOffsets.Sort();
				for(int j = 0; j < sectionOffsets.Count; j++)
				{
					offsets.Add(sectionOffsets[j] + (ulong)(i << 0x1B));
				}
			}
			count = (uint)offsets.Count;
			byte[] compressedData = PackUncompressedIntegers(count, offsets);
			size = (uint)compressedData.Length;
			return compressedData;
		}
		public void WriteRawOffset(ulong data, SaverSection section)
		{
			if(igAlchemyCore.isPlatform64Bit(_platform)) section._sh.WriteUInt64(data);
			                                             section._sh.WriteUInt32((uint)data);
		}
		private ulong Align(ulong input, uint alignment)
		{
			return (ulong)(((input + (alignment - 1)) / alignment) * alignment);
		}
		private byte[] PackUncompressedIntegers(uint count, List<ulong> offsets)
		{
			List<byte> compressedData = new List<byte>();

			ulong previousInt = 0x00;
			bool shiftMoveOrMask = false;
			byte currentByte = 0x00;
			int shiftAmount = 0x00;

			for(int i = 0; i < count; i++)
			{
				bool firstPass = true;
				ulong deltaInt = (offsets[i] - previousInt) / 4 - (_version < 0x09 ? 1u : 0u);
				previousInt = offsets[i];
				while(true)
				{
					byte delta = (byte)((deltaInt >> shiftAmount) & 0b0111);
					ulong remaining = ((deltaInt >> shiftAmount) & ~0b0111u);
					if(remaining > 0 || delta > 0 || firstPass)
					{
						if(remaining != 0)
						{
							delta |= 0x08;
						}
						shiftAmount += 3;
						if(shiftMoveOrMask)
						{
							currentByte |= (byte)(delta << 4);
							compressedData.Add(currentByte);
							currentByte = 0x00;
						}
						else
						{
							currentByte |= delta;
						}
						firstPass = false;
						shiftMoveOrMask = !shiftMoveOrMask;
					}
					else
					{
						shiftAmount = 0;
						previousInt = offsets[i];
						break;
					}
				}
			}
			if(shiftMoveOrMask)
			{
				compressedData.Add(currentByte);
			}

			return compressedData.ToArray();
		}
	}
}