namespace igLibrary.Core
{
	//Is technically a struct, labelled it a class because it's stored by reference
	public class igHandle
	{
		public static igHandle NullHandle => new igHandle();
		public ushort _refCount;
		public ushort _unk;
		public igName _namespace;
		public igName _alias;
		public igObject? _object;
		public igHandle()
		{
			_namespace._hash = 0;
			_namespace._string = null;
			_alias._hash = 0;
			_alias._string = null;
		}
		public igHandle(igHandleName name)
		{
			_alias = name._name;
			_namespace = name._ns;
		}
		public T? GetObjectAlias<T>() where T : igObject
		{
			if(_object != null) return (T)_object;

			if(_namespace._hash == 0x7054BE0F) return (T)(igObject)igArkCore.GetObjectMeta(_alias._string);
			if(_namespace._hash == 0x61B3DAD4) return (T)(igObject)igArkCore.GetFieldMetaForObject(_alias._string);

			Dictionary<uint, igObjectDirectory> dirs = igObjectStreamManager.Singleton._directories;
			if(dirs.ContainsKey(_namespace._hash))
			{
				igObjectDirectory dir = dirs[_namespace._hash];
				if(dir._useNameList == false) return null;
				for(int i = 0; i < dir._nameList._count; i++)
				{
					if(dir._nameList[i]._hash == _alias._hash)
					{
						_object = dir._objectList[i] as T;
						return (T)_object;
					}
				}
			}
			Console.WriteLine($"failed to load {_namespace._string}:/{_alias._string}");
			return null;
		}
		public override string ToString()
		{
			return $"{_namespace._string}.{_alias._string}";
		}
	}
}