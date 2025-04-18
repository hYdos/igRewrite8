/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class igDotNetLoadResolver : igObject
	{
		public StreamHelper _stringTable;
		public DotNetRuntime _runtime;
		public Dictionary<string, string> _aliases = new Dictionary<string, string>();
		public Dictionary<string, igBaseMeta> _pending = new Dictionary<string, igBaseMeta>();
		public void AppendMeta(igBaseMeta meta)
		{
			_pending.Add(meta._name, meta);
		}
		public void FinalizeTypes(DotNetLibrary library)
		{
			for(int i = 0; i < library._ownedTypes._count; i++)
			{
				if(library._ownedTypes[i] is igDotNetDynamicMetaObject metaObject) metaObject.FinalizeAppendToArkCore();
			}
		}
		public void AddTypes(DotNetLibrary library)
		{
			for(int i = 0; i < library._ownedTypes._count; i++)
			{
				if(library._ownedTypes[i] is igDotNetDynamicMetaObject metaObject)  metaObject.AppendToArkCore();
				else if(library._ownedTypes[i] is igDotNetDynamicMetaEnum metaEnum) metaEnum.AppendToArkCore();
			}
		}
	}
}