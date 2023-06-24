namespace igLibrary.Core
{
	public class igIGZSaver
	{
		//public Dictionary<igObject, ulong> _objectOffsetList = new Dictionary<igObject, ulong>();
		public List<igMetaObject> _vTableList = new List<igMetaObject>();
		public List<string> _stringList = new List<string>();
		public Dictionary<string, uint> _stringRefList = new Dictionary<string, uint>();
		public List<Tuple<ulong, ulong>> _thumbnails = new List<Tuple<ulong, ulong>>();
		private List<SaverSection> _sections = new List<SaverSection>();
		public List<igHandle> _namedHandleList = new List<igHandle>();
		public List<igHandle> _namedExternalList = new List<igHandle>();
		public List<igHandle> _externalList = new List<igHandle>();
		public List<(string, string)> _buildDependancies = new List<(string, string)>();
		public IG_CORE_PLATFORM _platform;
		private StreamHelper _stream;
		public uint _version;
		private uint _fixupCount;
		private uint _fixupSize;
		private ulong _nameListOffset = 0;

		public class SaverSection
		{
			public StreamHelper _sh;
			public igMemoryPool _pool;
			public igRuntimeFields _runtimeFields;
			public Dictionary<igObject, ulong> _objectOffsetList;
			public uint _fileOffset;
			public uint _fileSize;
			public uint _index;
			public uint _alignment;
			private IG_CORE_PLATFORM _platform;
			public SaverSection(igMemoryPool pool, IG_CORE_PLATFORM platform)
			{
				if(pool == null) throw new ArgumentNullException("Memory pool cannot be null!");
				_sh = new StreamHelper(new MemoryStream(), igAlchemyCore.isPlatformBigEndian(platform) ? StreamHelper.Endianness.Big: StreamHelper.Endianness.Little);
				_pool = pool;
				_platform = platform;
				_runtimeFields = new igRuntimeFields();
				_objectOffsetList = new Dictionary<igObject, ulong>();
			}
			public ulong FindFreeMemory(ushort alignment) => Align((uint)_sh.BaseStream.Length, alignment);
			public ulong Align(ulong input, uint alignment)
			{
				return (ulong)(((input + (alignment - 1)) / alignment) * alignment);
			}
			public ulong Malloc(uint size) => MallocAligned(size, 1);
			public ulong MallocAligned(uint size, ushort alignment)
			{
				ulong offset = FindFreeMemory(alignment);
				_sh.Seek(offset);
				for(int i = 0; i < size; i++)
				{
					_sh.WriteByte(0);
				}
				return offset;
			}
			public void PushAlignment(uint alignment)
			{
				if(_alignment < alignment) _alignment = alignment;
			}
		}
		public void WriteFile(igObjectDirectory dir, string path, IG_CORE_PLATFORM platform)
		{
			_platform = platform;
			_version = 0x09;
			_stream = new StreamHelper(File.Create(path), igAlchemyCore.isPlatformBigEndian(platform) ? StreamHelper.Endianness.Big : StreamHelper.Endianness.Little);

			SaverSection rootSection = GetSaverSection(dir._objectList.internalMemoryPool);
			rootSection._runtimeFields._objectLists.Add(SaveObject(dir._objectList));
			if(dir._useNameList)
			{
				_nameListOffset = SaveObject(dir._nameList);
			}

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

			offset = section.MallocAligned(meta._sizes[_platform], meta._alignments[_platform]);
			section._sh.Seek(offset);

			section._objectOffsetList.Add(obj, offset);
			WriteVTable(meta, section);
			section._sh.WriteUInt32(0);

			section._sh.Seek(offset);

			obj.WriteIGZFields(this, section);

			return offset | (section._index << (_version >= 7 ? 0x1B : 0x18));
		}
		public void RefObject(igObject? obj)
		{
			SaverSection section = GetSaverSection(obj.internalMemoryPool);
			ulong offset = section._objectOffsetList[obj];
			section._sh.Seek(offset + igAlchemyCore.GetPointerSize(_platform));
			uint refCount = section._sh.ReadUInt32();
			section._sh.Seek(offset + igAlchemyCore.GetPointerSize(_platform));
			section._sh.WriteUInt32(refCount + 1);
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
				uint sectionSize = (uint)_sections[i]._sh.BaseStream.Length;
				_stream.WriteUInt32(sectionSize);
				_stream.WriteUInt32(_sections[i]._alignment);

				_stream.Seek(memoryOffset);
				_sections[i]._sh.BaseStream.Flush();
				_sections[i]._sh.Seek(0);
				_sections[i]._sh.BaseStream.CopyTo(_stream.BaseStream);

				memPoolNameOffset += _sections[i]._pool._name.Length + 1;	//Don't forget the null byte!
				memoryOffset += sectionSize;

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
			section.PushAlignment(igAlchemyCore.GetPointerSize(_platform));
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
				ret._index = (uint)_sections.Count;
				_sections.Add(ret);
			}
			else
			{
				ret = _sections[index];
			}
			return ret;
		}
		public uint SerializeOffset(uint offset, SaverSection section) => SerializeOffset(offset, section._index);
		public uint SerializeOffset(uint offset, uint index)
		{
			if(_version <= 0x06) return offset | (index << 0x18);
			else                 return offset | (index << 0x1B);
		}
		public void WriteFixupSections(igObjectDirectory dir)
		{
			ulong endOffset = 0x800;
			ulong startOffset = 0x800;
			_stream.Seek(startOffset);

			ulong dependancyStartOffset = 0x800;

			//Dependancy list can't be autogenerated, cry
			if(dir._dependancies.Count > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x50454454);
				_stream.WriteInt32(dir._dependancies.Count);
				_stream.WriteInt32(0);
				_stream.WriteInt32(0x10);

				for(int j = 0; j < dir._dependancies.Count; j++)
				{
					_stream.WriteString(dir._dependancies[j]._name._string);
					_stream.WriteString(dir._dependancies[j]._path);
				}

				endOffset = Align(_stream.Tell(), 4);
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(endOffset - startOffset));
				_stream.Seek(endOffset);
				dependancyStartOffset = endOffset;
				_fixupCount++;
			}
			/*if(_namedExternalList.Count > 0)
			{
				dir._dependancies.Clear();
				for(int i = 0; i < _namedExternalList.Count; i++)
				{
					if(igObjectHandleManager.Singleton.IsSystemObject(_namedExternalList[i]))
					{
						continue;
					}
					igObjectDirectory dependantDir = igObjectStreamManager.Singleton._directories[_namedExternalList[i]._namespace._hash];
					dir._dependancies.Add(dependantDir);
				}

				startOffset = endOffset;
				_stream.WriteUInt32(0x50454454);
				_stream.WriteInt32(dir._dependancies.Count);
				_stream.WriteInt32(0);
				_stream.WriteInt32(0x10);

				for(int j = 0; j < dir._dependancies.Count; j++)
				{
					_stream.WriteString(dir._dependancies[j]._name._string);
					_stream.WriteString(dir._dependancies[j]._path);
				}

				endOffset = Align(_stream.Tell(), 4);
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(endOffset - startOffset));
				_stream.Seek(endOffset);
				dependancyStartOffset = endOffset;
			}*/

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

			if(_externalList.Count > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x44495845);
				_stream.WriteInt32(_externalList.Count);
				_stream.WriteUInt32((uint)_externalList.Count * 8 + 0x10);
				_stream.WriteUInt32(0x10);
				for(int j = 0; j < _externalList.Count; j++)
				{
					_stream.WriteUInt32(_externalList[j]._alias._hash);
					_stream.WriteUInt32(_externalList[j]._namespace._hash);
				}
				endOffset = startOffset + 0x10u + (uint)_externalList.Count * 8u;
				_stream.Seek(endOffset);
				_fixupCount += 1;
			}

			if(_namedHandleList.Count > 0 || _namedExternalList.Count > 0)
			{
				startOffset = endOffset;
				int externalCount = _namedHandleList.Count + _namedExternalList.Count;
				_stream.WriteUInt32(0x4D4E5845);
				_stream.WriteInt32(externalCount);
				uint alignedDataStart = (uint)Align(_stream.Tell()+8 - (uint)startOffset, igAlchemyCore.GetPointerSize(_platform));
				_stream.WriteUInt32((uint)externalCount * igAlchemyCore.GetPointerSize(_platform) * 2u + alignedDataStart);
				_stream.WriteUInt32(alignedDataStart);
				_stream.Seek(startOffset + alignedDataStart);

				//Write named handles
				for(int j = 0; j < _namedHandleList.Count; j++)
				{
					_stream.WriteUInt32((uint)AddString(_namedHandleList[j]._namespace._string) | 0x80000000);
					_stream.WriteUInt32((uint)AddString(_namedHandleList[j]._alias._string));
				}
				//Write named externals
				for(int j = 0; j < _namedExternalList.Count; j++)
				{
					_stream.WriteUInt32((uint)AddString(_namedExternalList[j]._namespace._string));
					_stream.WriteUInt32((uint)AddString(_namedExternalList[j]._alias._string));
				}

				endOffset = _stream.Tell();
				_stream.Seek(endOffset);
				_fixupCount += 1;
			}

			//TSTR
			if(_stringList.Count > 0)
			{
				startOffset = dependancyStartOffset;
				_stream.Seek(startOffset);
				byte[] otherFixups = new byte[endOffset - startOffset];
				_stream.Read(otherFixups);
				_stream.Seek(startOffset);
				for(int i = 0; i < otherFixups.Length; i++)
				{
					_stream.WriteByte(0);
				}
				_stream.Seek(startOffset);

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
				ulong tempEndOffset = Align(_stream.Tell(), 4);
				_stream.Seek(startOffset + 8);
				_stream.WriteUInt32((uint)(tempEndOffset - startOffset));
				_stream.Seek(tempEndOffset);
				_stream.BaseStream.Write(otherFixups);				
				endOffset = _stream.Tell64();
				//_stream.Seek(endOffset + tempEndOffset - 0x800);
				_fixupCount++;
			}

			if(_thumbnails.Count > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x4E484D54);
				_stream.WriteInt32(_thumbnails.Count);
				uint alignedDataStart = (uint)Align(_stream.Tell()+8 - (uint)startOffset, igAlchemyCore.GetPointerSize(_platform));
				_stream.WriteUInt32((uint)_thumbnails.Count * igAlchemyCore.GetPointerSize(_platform) * 2u + alignedDataStart);
				_stream.WriteUInt32(alignedDataStart);
				for(int j = 0; j < _thumbnails.Count; j++)
				{
					if(igAlchemyCore.isPlatform64Bit(_platform))
					{
						_stream.WriteUInt64(_thumbnails[j].Item1);
						_stream.WriteUInt64(_thumbnails[j].Item2);
					}
					else
					{
						_stream.WriteUInt32((uint)_thumbnails[j].Item1);
						_stream.WriteUInt32((uint)_thumbnails[j].Item2);
					}
				}
				endOffset = _stream.Tell();
				_stream.Seek(endOffset);
				_fixupCount += 1;
			}

			WriteRuntimeFixup(0x42545652, ref startOffset, ref endOffset, x => x._runtimeFields._vtables);			//RVTB
			WriteRuntimeFixup(0x52545352, ref startOffset, ref endOffset, x => x._runtimeFields._stringRefs);		//RSTR
			WriteRuntimeFixup(0x54545352, ref startOffset, ref endOffset, x => x._runtimeFields._stringTables);		//RSTT
			WriteRuntimeFixup(0x53464F52, ref startOffset, ref endOffset, x => x._runtimeFields._offsets);			//ROFS
			WriteRuntimeFixup(0x44495052, ref startOffset, ref endOffset, x => x._runtimeFields._poolIds);			//RPID
			WriteRuntimeFixup(0x54584552, ref startOffset, ref endOffset, x => x._runtimeFields._externals);		//REXT
			WriteRuntimeFixup(0x444E4852, ref startOffset, ref endOffset, x => x._runtimeFields._handles);			//RHND
			WriteRuntimeFixup(0x58454E52, ref startOffset, ref endOffset, x => x._runtimeFields._namedExternals);	//RNEX
			WriteRuntimeFixup(0x4E484D52, ref startOffset, ref endOffset, x => x._runtimeFields._memoryHandles);	//RMHN
			WriteRuntimeFixup(0x544F4F52, ref startOffset, ref endOffset, x => x._runtimeFields._objectLists);		//ROOT

			_stream.Seek(endOffset);

			if(_nameListOffset > 0)
			{
				startOffset = endOffset;
				_stream.WriteUInt32(0x4D414E4F);
				_stream.WriteInt32(1);
				_stream.WriteInt32(0x14);
				_stream.WriteInt32(0x10);
				_stream.WriteUInt32((uint)_nameListOffset);
	
				endOffset = startOffset + 0x14u;
				_stream.Seek(endOffset);
				_fixupCount += 1;
			}

			_fixupSize = (uint)endOffset - 0x800u;
		}
		private int AddString(string value)
		{
			int index = _stringList.FindIndex(x => x == value);
			if(index < 0)
			{
				index = _stringList.Count;
				_stringList.Add(value);
			}
			return index;
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
			endOffset = Align(startOffset + 0x10 + size, 4);
			_stream.WriteUInt32((uint)(endOffset - startOffset));
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