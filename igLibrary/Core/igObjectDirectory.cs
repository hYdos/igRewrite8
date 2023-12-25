namespace igLibrary.Core
{
	public class igObjectDirectory : igObject
	{
		public string _path;
		public igName _name;
		public List<igObjectDirectory> _dependancies = new List<igObjectDirectory>();
		public igObjectList _objectList = new igObjectList();
		public bool _useNameList = false;
		public igNameList? _nameList = null;
		public igIGZLoader _loader;
		public igFileDescriptor _fd;
		public static Func<string, igName, igBlockingType, igObjectDirectory?> _loadDependancyFunction = igObjectDirectory.LoadDependancyDefault;

		public enum FileType : uint
		{
			kAuto,
			kIGB,
			kIGX,
			kDataStream,
			kIGZ,
			kInvalid,	//This isn't real
		}

		FileType type;

		public igObjectDirectory(){}
		public igObjectDirectory(string path, igName nameSpace)
		{
			_path = path;
			_name = nameSpace;
		}
		public igObjectDirectory(string path)
		{
			_path = path;
			_name = new igName(Path.GetFileNameWithoutExtension(path).ToLower());
		}
		public void ReadFile()
		{
			igObjectLoader loader = igObjectLoader.FindLoader(_path);
			loader.ReadFile(this, _path, igBlockingType.kMayBlock);
		}
		public void WriteFile(Stream dst, IG_CORE_PLATFORM platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
		{
			if(type == FileType.kIGZ)
			{
				igIGZSaver saver = new igIGZSaver();
				if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
				{
					saver.WriteFile(this, dst, _loader._platform);
				}
				else
				{
					saver.WriteFile(this, dst, platform);
				}
			}
		}
		public void WriteFile(string path, IG_CORE_PLATFORM platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
		{
			FileStream fs = File.Create(path);
			WriteFile(path, platform);
			fs.Close();
		}
		public void AddObject(igObject obj, igName ns, igName name)
		{
			_objectList.Append(obj);
			if(_useNameList)
			{
				_nameList.Append(name);
			}
		}
		public static igObjectDirectory? LoadDependancyDefault(string path, igName name, igBlockingType idk)
		{
			return igObjectStreamManager.Singleton.Load(path, name);
		}
		public static igObjectDirectory? LoadDependancyDefault(string filePath, igName nameSpace)
		{
			return igObjectStreamManager.Singleton.Load(filePath, nameSpace);
		}
		public static FileType GetLoader(string filePath)
		{
			igFilePath path = new igFilePath();
			path.Set(filePath);
			switch(path._extension.ToString())
			{
				case "igz":
				case "lng":
				case "pak":	//not to be confused with the archive extension
				case "bld":	//not to be confused with the archive extension
					return FileType.kIGZ;
				default:
					return FileType.kInvalid;
					//throw new InvalidOperationException($"Invalid filetype {path._fileExtension}");
			}
		}
		public void BuildIGZ(string path)
		{
			
		}
		public igObject? GetObjectByType(Type t)
		{
			for(int i = 0; i < _objectList._count; i++)
			{
				if(_objectList[i].GetType().IsAssignableTo(t)) return _objectList[i];
			}
			return null;
		}
		public T? GetObjectByType<T>() where T : igObject
		{
			for(int i = 0; i < _objectList._count; i++)
			{
				if(_objectList[i] is T) return (T)_objectList[i];
			}
			return null;
		}
	}
	public class igObjectDirectoryList : igTObjectList<igObjectDirectory> {}
}