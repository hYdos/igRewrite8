namespace igLibrary.Core
{
	public class igObjectStreamManager : igSingleton<igObjectStreamManager>
	{
		//change to Dictionary<igName, igObjectDirectory>
		public Dictionary<uint, igObjectDirectory> _directories = new Dictionary<uint, igObjectDirectory>();
		public void AddObjectDirectory(igObjectDirectory dir)
		{
			_directories.Add(dir._name._hash, dir);
			igObjectHandleManager.Singleton.AddDirectory(dir);
		}
		public igObjectDirectory? Load(string path)
		{
			return Load(path, new igName(Path.GetFileNameWithoutExtension(path)));
		}
		public igObjectDirectory? Load(string filePath, igName nameSpace)
		{
			Console.Write($"igObjectStreamManager was asked to load {filePath}...");
			if(_directories.ContainsKey(nameSpace._hash))
			{
				Console.Write($"was previously loaded.\n");
				return _directories[nameSpace._hash];
			}
			Console.Write($"was not previously loaded.\n");

			if(!igFileContext.Singleton.Exists(filePath, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal)) return null;

			igObjectDirectory objDir = new igObjectDirectory(filePath, nameSpace);
			AddObjectDirectory(objDir);
			objDir.ReadFile();
			igObjectHandleManager.Singleton.AddDirectory(objDir);
			return objDir;
		}
	}
}