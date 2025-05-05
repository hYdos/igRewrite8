/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


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


		/// <summary>
		/// Default constructor
		/// </summary>
		public igHandle()
		{
			_namespace._hash = 0;
			_namespace._string = null;
			_alias._hash = 0;
			_alias._string = null;
		}


		/// <summary>
		/// Constructor taking in an igHandleName
		/// </summary>
		/// <param name="name">the igHandleName</param>
		public igHandle(igHandleName name)
		{
			_alias = name._name;
			_namespace = name._ns;
		}


		/// <summary>
		/// Constructor taking in a string
		/// </summary>
		/// <param name="name">the string</param>
		public igHandle(string name): this(new igHandleName(name))
		{
		}


		/// <summary>
		/// Gets the object referenced by this handle and attempts to cast it
		/// </summary>
		/// <typeparam name="T">The type to cast to</typeparam>
		/// <returns>The object, or null if the cast fails or if it can't be located</returns>
		public T? GetObjectAlias<T>() where T : igObject
		{
			if(_object != null) return (T)_object;

			Dictionary<uint, igObjectDirectoryList> dirLists = igObjectStreamManager.Singleton._directoriesByName;
			if(dirLists.ContainsKey(_namespace._hash))
			{
				igObjectDirectoryList dirs = dirLists[_namespace._hash];
				for(int d = 0; d < dirs._count; d++)
				{
					igObjectDirectory dir = dirs[d];
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
			}

			if (_namespace._string == null)
			{
				Logging.Warn("failed to load igHandle {0}.{1}", _namespace._hash, _alias._hash);
			}
			else
			{
				Logging.Warn("failed to load igHandle {0}.{1}", _namespace._string, _alias._string);
			}
			return null;
		}


		public static bool operator ==(igHandle? a, igHandle? b)
		{
			if(ReferenceEquals(a, b)) return true;
			if(a is null || b is null) return false;
			return a._namespace._hash == b._namespace._hash && a._alias._hash == b._alias._hash;
		}


		public static bool operator !=(igHandle? a, igHandle? b)
		{
			if(ReferenceEquals(a, b)) return false;
			if(a is null || b is null) return true;
			return a._namespace._hash != b._namespace._hash || a._alias._hash != b._alias._hash;
		}


		public override string ToString()
		{
			return $"{_namespace._string ?? _namespace._hash.ToString("x")}.{_alias._string ?? _alias._hash.ToString("x")}";
		}
	}
}