using HashDepot;
using System.Text;
using System.IO.Compression;
using SevenZip.Compression.LZMA;
using K4os.Compression.LZ4;

namespace igLibrary.Core
{
	public class igArchive : igStorageDevice
	{
		//This is a wishlist, not an indicator of current support, although the ones marked with an asterisk (*) aren't supported
		public enum IG_CORE_ARCHIVE_VERSION : uint
		{
			ssa_wii_or_all_citrus_or_sg_alpha = 0x04,			//Skylanders Spyro's Adventure on Wii, or all the alchemy 3DS games, or the Skylanders Giants Alpha*
			giants_not_citrus_or_ssa_cafe_or_ssf_alpha = 0x08,	//All versions of Skylanders Giants except 3DS, or Skylanders Spyro's Adventure on Wii U, or the Skylanders Swap Force Alpha*
			sli_ssf_not_citrus = 0x0A,							//All versions of Skylanders Lost Islands and Skylanders Swap Force except 3DS
			stt_not_citrus = 0x0B,								//All versions of Skylanders Trap Team except 3DS*
			ssc_all_or_si_ps3_xenon_cafe = 0x1000000B,			//All alchemy-based versions of Skylanders Superchargers or Skylanders Imaginators on PS3, Xbox 360, or WiiU*
			si_ps4_durango = 0x2000000B,						//Skylanders Imaginators on PS4 or Xbox One*
			si_nx = 0xDEAD,										//Skylanders Imaginators on Nintendo Switch*, version number is 0xDEAD cos idk what it is
		}
		public struct IG_CORE_ARCHIVE_FILE_HEADER
		{
			public uint hash;			//Not part of the file header but it's easier to store it here
			public string name;			//See above
			public string fullName;		//See above
			public uint offset;
			public uint ordinal;	//Doesn't exist on IG_CORE_ARCHIVE_VERSION.ssa_wii_or_all_citrus_or_sg_alpha
			public uint length;
			public uint mode;
		}

		StreamHelper sh;
		IG_CORE_ARCHIVE_VERSION version;

		uint alignment;
		public IG_CORE_ARCHIVE_FILE_HEADER[] _fileHeaders;

		uint numLargeBlocks;
		uint numMediumBlocks;
		uint numSmallBlocks;

		uint[] largeBlockTable;
		ushort[] mediumBlockTable;
		byte[] smallBlockTable;

		public uint flags;

		public string _nativeMedia;
		public string _nativePath;
		public igFile _file;

		public igArchive(string path)
		{
			_path = path;
			_name = Path.GetFileName(path);
			_file = new igFile();
			_file.Open(path);
			sh = new StreamHelper(_file._file._handle);
			uint magicNumber = sh.ReadUInt32();

			if(magicNumber == 0x1A414749) sh._endianness = StreamHelper.Endianness.Little;
			else if(magicNumber == 0x4947411A) sh._endianness = StreamHelper.Endianness.Big;

			ReadHeader();
		}

		//Bypasses igFileContext, useful when not reading an archive from _root
		public igArchive(Stream stream)
		{
			sh = new StreamHelper(stream);
			uint magicNumber = sh.ReadUInt32();

			if(magicNumber == 0x1A414749) sh._endianness = StreamHelper.Endianness.Little;
			else if(magicNumber == 0x4947411A) sh._endianness = StreamHelper.Endianness.Big;

			ReadHeader();

			igFileContext.Singleton._archiveManager._archiveList.Append(this);
		}

		public IG_CORE_ARCHIVE_VERSION PredictVersion()
		{
			uint rawVersion = sh.ReadUInt32(0x04);
			if(rawVersion == 0x0A)      return IG_CORE_ARCHIVE_VERSION.sli_ssf_not_citrus;
			else if(rawVersion == 0x0B)
			{
				     if(sh.ReadUInt32(0x2C) == 0) return IG_CORE_ARCHIVE_VERSION.stt_not_citrus;
				else if(sh.ReadUInt32(0x28) == 0) return IG_CORE_ARCHIVE_VERSION.ssc_all_or_si_ps3_xenon_cafe;
				else                              return IG_CORE_ARCHIVE_VERSION.si_ps4_durango;
			}
			throw new NotImplementedException($"version number {rawVersion} is not supported as of yet.");
		}
		public void ReadHeader()
		{
			version = PredictVersion();
			//Console.WriteLine($"Got version {version}");

			uint numFiles = sh.ReadUInt32(0x0C);

			//Read alignment
			alignment = 0;
			if(version == IG_CORE_ARCHIVE_VERSION.ssa_wii_or_all_citrus_or_sg_alpha)
			{
				throw new NotImplementedException("No idea what the alignment for ssa_wii_or_all_citrus_or_sg_alpha is.");
			}
			else
			{
				alignment = sh.ReadUInt32(0x10);
			}

			//Read block table counts
			numLargeBlocks = sh.ReadUInt32(0x1C);
			numMediumBlocks = sh.ReadUInt32();
			numSmallBlocks = sh.ReadUInt32();

			uint headerLength = 0x38;
			uint fileHeaderLength = 0x10;
			if(version == IG_CORE_ARCHIVE_VERSION.ssa_wii_or_all_citrus_or_sg_alpha || version == IG_CORE_ARCHIVE_VERSION.giants_not_citrus_or_ssa_cafe_or_ssf_alpha)
			{
				if(version == IG_CORE_ARCHIVE_VERSION.ssa_wii_or_all_citrus_or_sg_alpha)
				{
					fileHeaderLength = 0x0C;
				}
				headerLength = 0x34;
			}

			//Get nametable offset
			uint nametableStart = (uint)sh.ReadUInt64(0x28);

			//Initialize arrays
			//names = new string[numFiles];
			_fileHeaders = new IG_CORE_ARCHIVE_FILE_HEADER[numFiles];

			//Read stuff
			for(uint i = 0; i < numFiles; i++)
			{
				//Read nametable
				uint relativeNameOffset = sh.ReadUInt32(nametableStart + i * 0x04);
				_fileHeaders[i].fullName = sh.ReadString(nametableStart + relativeNameOffset);
				_fileHeaders[i].name = sh.ReadString();

				//Read Hash
				_fileHeaders[i].hash = sh.ReadUInt32(headerLength + i * 4);
				
				sh.Seek(headerLength + numFiles * 4 + i * fileHeaderLength);

				if(version == IG_CORE_ARCHIVE_VERSION.sli_ssf_not_citrus)
				{
					_fileHeaders[i].offset = sh.ReadUInt32();
					_fileHeaders[i].ordinal = sh.ReadUInt32();
					_fileHeaders[i].length = sh.ReadUInt32();
					_fileHeaders[i].mode = sh.ReadUInt32();
				}
				else if(version == IG_CORE_ARCHIVE_VERSION.ssc_all_or_si_ps3_xenon_cafe)
				{
					_fileHeaders[i].ordinal = sh.ReadUInt32();
					_fileHeaders[i].offset = sh.ReadUInt32();
					_fileHeaders[i].length = sh.ReadUInt32();
					_fileHeaders[i].mode = sh.ReadUInt32();
				}
				else if(version == IG_CORE_ARCHIVE_VERSION.stt_not_citrus)
				{
					_fileHeaders[i].offset = sh.ReadUInt32();
					_fileHeaders[i].ordinal = sh.ReadUInt32();
					_fileHeaders[i].length = sh.ReadUInt32();
					_fileHeaders[i].mode = sh.ReadUInt32();
				}
			}

			//Read block tables
			sh.Seek(headerLength + numFiles * (fileHeaderLength + 0x04));
			largeBlockTable = sh.ReadStructArray<uint>(numLargeBlocks);
			mediumBlockTable = sh.ReadStructArray<ushort>(numMediumBlocks);
			smallBlockTable = sh.ReadStructArray<byte>(numSmallBlocks);

			flags = sh.ReadUInt32(headerLength - 4);
		}
		public void ExtractFile(string name, Stream dst) => ExtractFile(FindFile(name), dst);
		public void ExtractFile(uint hash, Stream dst) => ExtractFile(FindFile(hash), dst);
		public void ExtractFile(int i, Stream dst)
		{
			if(i < 0) throw new FileNotFoundException("File does not exist.");
			sh.Seek(_fileHeaders[i].offset);

			if(dst is MemoryStream dsms) dsms.Capacity = (int)_fileHeaders[i].length;
			dst.Seek(0, SeekOrigin.Begin);

			if(_fileHeaders[i].mode == 0xFFFFFFFF)
			{
				dst.Write(sh.ReadBytes((int)_fileHeaders[i].length));
				dst.Seek(0, SeekOrigin.Begin);
				return;
			}

			//Start decocmpression loop, this was mainly taken from LG-RZ's igArchiveLib (https://github.com/LG-RZ/igArchiveLib/)

			bool isWriting = false;

			uint numBlocks = (_fileHeaders[i].length + 0x7FFF) >> 0xF;
			for(uint currentBlockIndex = 0; currentBlockIndex < numBlocks; currentBlockIndex++)
			{
				uint blockIndex = (_fileHeaders[i].mode & 0x0FFFFFFF) + currentBlockIndex;
				uint blockOffset = 0;
				uint blockSize = 0;
				bool isCompressed = false;
				if(0x7F * alignment < _fileHeaders[i].length)
				{
					if(0x7FFF * alignment < _fileHeaders[i].length)
					{
						//Console.WriteLine("Large blocks");
						uint block = largeBlockTable[blockIndex];
						blockOffset = (block & 0x7FFFFFFF) * alignment;
						isCompressed = (block >> 0x1F) == 1;
						blockSize = (largeBlockTable[blockIndex + 1] & 0x7FFFFFFF) * alignment - blockOffset;
					}
					else
					{
						uint block = mediumBlockTable[blockIndex];
						//Console.WriteLine($"Medium blocks, {blockIndex.ToString("X02")}, {block.ToString("X04")}");
						blockOffset = (block & 0x7FFF) * alignment;
						isCompressed = (byte)(block >> 0xf) == 1;
						blockSize = (uint)((mediumBlockTable[blockIndex + 1] & 0x7FFF) * alignment - blockOffset);
					}
				}
				else
				{
					//Console.WriteLine("Small blocks");
					uint block = smallBlockTable[blockIndex];
					blockOffset = (block & 0x7F) * alignment;
					isCompressed = (block >> 0x07) == 1;
					blockSize = (uint)(smallBlockTable[blockIndex + 1] & 0x7F) * alignment - blockOffset;
				}

				uint decompressedSize = (_fileHeaders[i].length < (currentBlockIndex + 1) * 0x8000) ? _fileHeaders[i].length & 0x7FFF : 0x8000;

				byte[] chunk;
				sh.Seek(_fileHeaders[i].offset + blockOffset);

				if(!isCompressed)
				{
					//Console.WriteLine($"Copying {decompressedSize.ToString("X08")} from {names[i]} @ {sh.BaseStream.Position.ToString("X08")}");
					chunk = sh.ReadBytes((int)decompressedSize);
					dst.Write(chunk);
					continue;
				}

				uint compressionType = _fileHeaders[i].mode >> 0x1C;
				uint compressedSize = 0;
				if(((uint)version & 0xF) < 0x0C)
				{
					compressedSize = sh.ReadUInt16(StreamHelper.Endianness.Little);
				}
				else
				{
					compressedSize = sh.ReadUInt32(StreamHelper.Endianness.Little);
				}

				//Console.WriteLine($"Decompressing {compressedSize.ToString("X08")}->{decompressedSize.ToString("X08")} bytes with compression type {compressionType} from {names[i]} @ {sh.BaseStream.Position.ToString("X08")}");
				switch(compressionType)
				{
					case 0x1:
						chunk = sh.ReadBytes((int)compressedSize);
						DeflateStream zstr = new DeflateStream(new MemoryStream(chunk), CompressionMode.Decompress);
						zstr.CopyTo(dst);
						zstr.Close();
						break;
					case 0x2:
						byte[] properties = sh.ReadBytes(5);
						chunk = sh.ReadBytes((int)compressedSize);
						SevenZip.Compression.LZMA.Decoder dec = new SevenZip.Compression.LZMA.Decoder();
						dec.SetDecoderProperties(properties);
						dec.Code(new MemoryStream(chunk), dst, compressedSize, decompressedSize, null);
						break;
					case 0x3:
						byte[] dest = new byte[decompressedSize];
						LZ4Codec.Decode(sh.ReadBytes((int)compressedSize), 0, (int)compressedSize, dest, 0, dest.Length);
						dst.Write(dest);
						break;
					default:
						throw new NotImplementedException($"Compression type 0x{compressionType.ToString("X")} is unsupported");
				}
			}
			dst.Flush();
			dst.Seek(0, SeekOrigin.Begin);
			if(_fileHeaders[i].length == 0) Console.WriteLine($"file {_fileHeaders[i].name} is empty");
		}
		//Likely unsafe
		public void Close()
		{
			sh.Close();
			sh.Dispose();
		}
		public int FindFile(string path)
		{
			string pathToHash = path;
			if((flags & 1u) != 0)
			{
				pathToHash = pathToHash.Replace('\\', '/');
				pathToHash = pathToHash.ToLower();
				//Console.WriteLine("kCaseInsensitiveHash");
			}
			if((flags & 2u) != 0) pathToHash = Path.GetFileName(pathToHash);
			pathToHash = pathToHash.TrimStart('/', '\\');
			return FindFile(igHash.Hash(pathToHash));
		}

		public int FindFile(uint hash) => Array.FindIndex<IG_CORE_ARCHIVE_FILE_HEADER>(_fileHeaders, x => x.hash == hash);
		public bool HasFile(string path) => FindFile(path) >= 0;
		public bool HasFile(uint hash) => FindFile(hash) >= 0;
		public string GetFileName(string path) => GetFileName(FindFile(path));
		public string GetFileName(uint hash) => GetFileName(FindFile(hash));
		public string GetFileName(int i)
		{
			string volumeName = Path.GetFileName((sh.BaseStream as FileStream).Name);
			return $"{volumeName}:/{_fileHeaders[i].name}";
		}

		public override void Process(igFileWorkItem workItem)
		{
			switch(workItem._type)
			{
				case igFileWorkItem.WorkType.kTypeExists:
					Exists(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeOpen:
					Open(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeClose:
					Close(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeRead:
					Read(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeWrite:
					//Write(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeTruncate:
					Truncate(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeMkdir:
					Mkdir(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeRmdir:
					Rmdir(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeFileList:
					GetFileList(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeFileListWithSizes:
					GetFileListWithSizes(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeUnlink:
					Unlink(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeRename:
					Rename(workItem);
					break;
				case igFileWorkItem.WorkType.kTypePrefetch:
					workItem.SetStatus(igFileWorkItem.Status.kStatusUnsupported);
					break;
				case igFileWorkItem.WorkType.kTypeFormat:
					Format(workItem);
					break;
				case igFileWorkItem.WorkType.kTypeCommit:
					Commit(workItem);
					break;
			}
		}

		public override void Exists(igFileWorkItem workItem)
		{
			if(HasFile(workItem._path))
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
			}
			else
			{
				workItem.SetStatus(igFileWorkItem.Status.kStatusInvalidPath);
			}
		}

		public override void Open(igFileWorkItem workItem)
		{
			int fileId = FindFile(workItem._path);
			workItem._file._path = workItem._path;
			workItem._file._size = _fileHeaders[fileId].length;
			workItem._file._position = 0;
			workItem._file._handle = new MemoryStream((int)workItem._file._size);
			workItem._file._device = this;
			ExtractFile(fileId, (MemoryStream)workItem._file._handle);
			workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
		}
		public override void Close(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Read(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Write(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Truncate(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Mkdir(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Rmdir(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void GetFileList(igFileWorkItem workItem)
		{
			igStringRefList nameList = (igStringRefList)workItem._buffer;
			nameList.SetCapacity(_fileHeaders.Length);
			for(int i = 0; i < _fileHeaders.Length; i++)
			{
				nameList.Append(_fileHeaders[i].name);
			}
			workItem.SetStatus(igFileWorkItem.Status.kStatusComplete);
		}

		public override void GetFileListWithSizes(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Unlink(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Rename(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Prefetch(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Format(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}

		public override void Commit(igFileWorkItem workItem)
		{
			throw new NotImplementedException();
		}
	}
	public class igArchiveList : igTObjectList<igArchive>{}
}