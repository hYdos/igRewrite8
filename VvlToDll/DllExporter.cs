using Mono.Cecil;
using igLibrary.Core;
using igLibrary.DotNet;

namespace VvlToDll
{
	public class DllExporter
	{
		private static ModuleReference mscorlib;
		private static bool _initialized = false;

		private ModuleDefinition _module;
		private DotNetLibrary _library;
		private Dictionary<igBaseMeta, TypeDefinition> _metaTypeLookup = new Dictionary<igBaseMeta, TypeDefinition>();

		private static void Init()
		{
			if(_initialized) return;

			mscorlib = new ModuleReference("mscorlib");
			_initialized = true;
		}
		public DllExporter() => Init();
		public void ExportLibrary(DotNetLibrary library, string name)
		{
			_module = ModuleDefinition.CreateModule(name, ModuleKind.Dll);
			//_module.ModuleReferences.Add(ArkDllExport.module);
			_library = library;
			//module.ModuleReferences.Add(mscorlib);
			CreateTypes();
			DefineEnums();
			DefineObjects();
			_module.Write(name);
		}
		private void CreateTypes()
		{
			foreach(igBaseMeta meta in _library._ownedTypes)
			{
				if(meta == null) continue;
				GetNsAndName(in meta._name, out string ns, out string name);
				TypeDefinition td = new TypeDefinition(ns, name, TypeAttributes.Public);
				_metaTypeLookup.Add(meta, td);
				_module.Types.Add(td);
			}
		}
		public TypeReference GetTypeFromMeta(igBaseMeta meta)
		{
			//return _module.ImportReference(typeof(object));
			if(ArkDllExport._metaTypeLookup.TryGetValue(meta, out TypeDefinition td)) return _module.ImportReference(td);
			if(_metaTypeLookup.TryGetValue(meta, out td)) return td;
			throw new KeyNotFoundException("Failed to load type");
		}
		private void DefineEnums()
		{
			foreach(KeyValuePair<igBaseMeta, TypeDefinition> kvp in _metaTypeLookup)
			{
				if(kvp.Key is not igMetaEnum metaEnum) continue;
				
				TypeDefinition td = kvp.Value;
				td.BaseType = _module.ImportReference(typeof(Enum));
				td.Fields.Add(new FieldDefinition("__value", FieldAttributes.Public | FieldAttributes.SpecialName, _module.ImportReference(typeof(int))));
				for(int i = 0; i < metaEnum._names.Count; i++)
				{
					td.Fields.Add(new FieldDefinition(metaEnum._names[i], FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal, td));
					td.Fields.Last().Constant = metaEnum._values[i];
				}
			}
		}
		private void DefineObjects()
		{
			foreach(KeyValuePair<igBaseMeta, TypeDefinition> kvp in _metaTypeLookup)
			{
				if(kvp.Key is not igMetaObject metaObject) continue;

				TypeDefinition td = kvp.Value;
				if(metaObject._parent != null)
				{
					td.BaseType = GetTypeFromMeta(metaObject._parent);
				}
				else continue;

				for(int i = metaObject._parent._metaFields.Count; i < metaObject._metaFields.Count; i++)
				{
					//FieldDefinition fd = new FieldDefinition(metaObject._metaFields[i]._name, FieldAttributes.Public, _module.ImportReference(typeof(object)));
					//td.Fields.Add(fd);

					ArkDllExport.AddField(_module, td, metaObject, metaObject._metaFields[i], GetTypeFromMeta);
				}
			}
		}
		private static void GetNsAndName(in string qualifiedName, out string ns, out string name)
		{
			int checkGeneric = qualifiedName.IndexOf('`');
			int genericTestIndex = checkGeneric < 0 ? qualifiedName.Length-1 : checkGeneric;	//Poorly named
			int nsEnd = qualifiedName.LastIndexOf('.', genericTestIndex, genericTestIndex);
			if(nsEnd > 0)
			{
				ns = qualifiedName.Substring(0, nsEnd);
				name = qualifiedName.Substring(nsEnd+1);
			}
			else
			{
				ns = "-";
				name = qualifiedName;
			}
		}
	}
}