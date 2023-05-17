namespace igLibrary.Core
{
	public class igFileContext : igSingleton<igFileContext>
	{
		private List<igFileDescriptor> _fileDescriptorPool = new List<igFileDescriptor>();
		public List<igFileWorkItem> _fileWorkItemPool = new List<igFileWorkItem>();		//TODO: Make this an igObjectPool
		private List<igStorageDevice> _devices = new List<igStorageDevice>();
		private igFileWorkItemProcessor _processorStack;
		public igArchiveMountManager _archiveMountManager = new igArchiveMountManager();
		public igArchiveManager _archiveManager = new igArchiveManager();

		Dictionary<string, string> _virtualDevices = new Dictionary<string, string>()
		{
			{"actors", "actors"},
			{"anims", "anims"},
			{"behavior_events", "behavior_events"},
			{"animation_events", "animation_events"},
			{"behaviors", "behaviors"},
			{"cutscene", "cutscene"},
			{"data", ""},
			{"fonts", "fonts"},
			{"graphs", "graphs"},
			{"vsc", "vsc"},
			{"loosetextures", "loosetextures"},
			{"luts", "loosetextures/luts"},
			{"maps", "maps"},
			{"materials", "materialInstances"},
			{"models", "models"},
			{"motionpaths", "motionpaths"},
			{"renderer", "renderer"},
			{"scripts", "scripts"},
			{"shaders", "shaders"},
			{"sky", "sky"},
			{"sounds", "sounds"},
			{"spawnmeshes", "spawnmeshes"},
			{"textures", "textures"},
			{"ui", "ui"},
			{"vfx", "vfx"},
			{"cwd", ""},
		};

		public string _root { get; private set;}

		public igFileContext()
		{
			_root = string.Empty;

			//Not Accurate
			//_processorStack = new igWin32StorageDevice();
			//_processorStack._nextProcessor = _archiveMountManager;

			//Accurate
			_processorStack = _archiveMountManager;

			_archiveMountManager._nextProcessor = _archiveManager;
			_archiveManager._nextProcessor = new igWin32StorageDevice();
		}

		//Rename to GetVirtualStorageDevicePath
		public string GetMediaDirectory(string media)
		{
			string lower = media.ToLower();
			if(_virtualDevices.ContainsKey(lower))
			{
				return _virtualDevices[lower];
			}
			else return media;
		}
		private void CreateWorkItem(igFileDescriptor fd, igFileWorkItem.WorkType workType, object buffer, ulong offset, ulong size, uint flags, string path, igBlockingType blockingType, igFileWorkItem.Priority priority, Action callback, object[] callbackData)
		{
			igFileWorkItem workItem = AllocateWorkItem();

			workItem._file = fd;
			workItem._buffer = buffer;
			workItem._offset = offset;
			workItem._size = (uint)size;
			workItem._flags = flags;
			workItem._path = path;
			workItem._type = workType;
			workItem._blocking = blockingType;
			workItem._priority = priority;

			//Didn't see this happening but it felt right
			workItem._status = igFileWorkItem.Status.kStatusActive;

			_processorStack.Process(workItem);
		}

		public igFileWorkItem AllocateWorkItem()
		{
			igFileWorkItem workItem;
			//int index = _fileWorkItemPool.FindIndex(x => x._status != igFileWorkItem.Status.kStatusActive);
			int index = -1;
			if(index < 0)
			{
				workItem = new igFileWorkItem();
				_fileWorkItemPool.Add(workItem);
			}
			else
			{
				workItem = _fileWorkItemPool[index];
			}
			return workItem;
		}

		public void Initialize(string root)
		{
			_root = root.TrimEnd('/');
			_root = _root.TrimEnd('\\');
		}

		public void AddStorageDevice(igStorageDevice device)
		{
			_devices.Add(device);
		}

		public igFileDescriptor Open(string path)
		{
			igFilePath fp = new igFilePath();
			fp.Set(path);
			int fdIndex = _fileDescriptorPool.FindIndex(x => x._path == fp._path);

			if(fdIndex >= 0) return _fileDescriptorPool[fdIndex];
			else
			{
				Stream? ms = null;
				if(File.Exists($"{_root}/{fp._path}"))
				{
					ms = File.Open(_root + "/" + fp._path, FileMode.Open);
				}
				else
				{
					uint hash = igHash.Hash(fp._path);
					for(int i = 0; i < _devices.Count; i++)
					{
						if(_devices[i] is igArchive iga)
						{
							if(iga.HasFile(hash))
							{
								ms = new MemoryStream();
								iga.ExtractFile(hash, ms);
							}
						}
					}
					if(ms == null) throw new FileNotFoundException($"Failed to find file {fp._path}");
				}
				_fileDescriptorPool.Add(new igFileDescriptor(ms, fp._path));
				return _fileDescriptorPool.Last();
			}
		}
		public static uint GetOpenFlags(FileAccess access, FileMode mode)
		{
			return ((uint)mode << 8) | (uint)access;
		}
		private igFileDescriptor Prepare(string path, uint flags)
		{
			igFileDescriptor fd = new igFileDescriptor();
			_fileDescriptorPool.Add(fd);
			igFilePath fp = new igFilePath();
			fp.Set(path);
			fd._path = fp._path;
			return fd;
		}
		public void Open(string path, uint flags, out igFileDescriptor fd, igBlockingType blockingType, igFileWorkItem.Priority priority)
		{
			fd = Prepare(path, flags);
			CreateWorkItem(fd, igFileWorkItem.WorkType.kTypeOpen, null, 0, 0, flags, fd._path, blockingType, priority, null, null);
		}
		public bool Exists(string path) => _fileDescriptorPool.Any(x => x._path == path);
	}
}