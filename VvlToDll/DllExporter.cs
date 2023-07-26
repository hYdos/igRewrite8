using Mono.Cecil;
using Mono.Cecil.Cil;
using igLibrary.Core;
using igLibrary.DotNet;

namespace VvlToDll
{
	public class DllExporter
	{
		private static ModuleReference module = new ModuleReference("mscorlib");
		private static bool _initialized = false;

		private ModuleDefinition _module;
		private DotNetLibrary _library;
		private List<DotNetLibrary> _dependencies = new List<DotNetLibrary>();
		private Dictionary<igBaseMeta, TypeDefinition> _metaTypeLookup = new Dictionary<igBaseMeta, TypeDefinition>();
		private Dictionary<DotNetMethodDefinition, MethodDefinition> _methodLookup = new Dictionary<DotNetMethodDefinition, MethodDefinition>();

		private static void Init()
		{
			if(_initialized) return;
			_initialized = true;
		}
		public DllExporter(DotNetLibrary library)
		{
			_module = ModuleDefinition.CreateModule(Path.GetFileNameWithoutExtension(library._path), ModuleKind.Dll);
			_library = library;
		}
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
		public void CreateTypes()
		{
			foreach(igBaseMeta meta in _library._ownedTypes)
			{
				if(meta == null) continue;
				GetNsAndName(in meta._name, out string ns, out string name);
				TypeDefinition td = new TypeDefinition(ns, name, TypeAttributes.Public);
				if(!_metaTypeLookup.TryAdd(meta, td) && meta is not igMetaEnum) throw new ArgumentException("Type exists boss."); 	//So many enums are called just "Flags" or "Mode" that I had to make this 
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
		public void DefineEnums()
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
		public void DefineObjects()
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
		public TypeReference ImportVvlTypeRef(DotNetType dnTypeRef)
		{
			switch((ElementType)(dnTypeRef._flags & (uint)DotNetType.Flags.kTypeMask))
			{
				default:
					throw new ArgumentException("Yo what???");
				case ElementType.kElementTypeVoid:
					return _module.TypeSystem.Void;
				case ElementType.kElementTypeBoolean:
					return _module.TypeSystem.Boolean;
				case ElementType.kElementTypeChar:
					return _module.TypeSystem.Char;
				case ElementType.kElementTypeI1:
					return _module.TypeSystem.SByte;
				case ElementType.kElementTypeU1:
					return _module.TypeSystem.Byte;
				case ElementType.kElementTypeI2:
					return _module.TypeSystem.Int16;
				case ElementType.kElementTypeU2:
					return _module.TypeSystem.UInt16;
				case ElementType.kElementTypeI4:
					return _module.TypeSystem.Int32;
				case ElementType.kElementTypeU4:
					return _module.TypeSystem.UInt32;
				case ElementType.kElementTypeI8:
					return _module.TypeSystem.Int64;
				case ElementType.kElementTypeU8:
					return _module.TypeSystem.UInt64;
				case ElementType.kElementTypeR4:
					return _module.TypeSystem.Single;
				case ElementType.kElementTypeR8:
					return _module.TypeSystem.Double;
				case ElementType.kElementTypeString:
					return _module.TypeSystem.String;
				case ElementType.kElementTypeObject:
					DotNetLibrary? owner = null;
					if(dnTypeRef._baseMeta is igDotNetDynamicMetaEnum dndme)
					{
						owner = dndme._owner;
					}
					else if(dnTypeRef._baseMeta is igDotNetDynamicMetaObject dndmo)
					{
						owner = dndmo._owner;
					}
					else if(dnTypeRef._baseMeta is igMetaObject || dnTypeRef._baseMeta is igMetaEnum)
					{
						return _module.ImportReference(ArkDllExport._metaTypeLookup[dnTypeRef._baseMeta]);
					}
					if(owner == null) return _module.TypeSystem.Object;	//This is bad, this happening indicates that the vvls were not loaded properly

					if(!_dependencies.Contains(owner))
					{
						_dependencies.Add(owner);
					}

					DllExporter dependentExporter = DllExportManager._libExporterLookup[owner];
					return _module.ImportReference(dependentExporter._metaTypeLookup[dnTypeRef._baseMeta]);				
			}
		}
		public void DeclareMethods()
		{
			foreach(DotNetMethodDefinition? dnMethodDef in _library._methodDefs)
			{
				if(dnMethodDef == null) continue;

				TypeDefinition td = _metaTypeLookup[dnMethodDef._declaringType._baseMeta];
				MethodDefinition md = new MethodDefinition(dnMethodDef._name, MethodAttributes.CompilerControlled, ImportVvlTypeRef(dnMethodDef._retType));
				
				if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.Constructor) != 0) md.Attributes |= MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;
				if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.StaticMethod) != 0) md.Attributes |= MethodAttributes.Static;
				if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.AbstractMethod) != 0) md.Attributes |= MethodAttributes.Abstract;
				//if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.RuntimeImplMethod) != 0) md.Attributes |= MethodAttributes.;
				//if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.NoSpecializationCopyMethod) != 0) md.Attributes |= MethodAttributes.;

				//If it's not static then param 1 is "this"
				for(int i = md.IsStatic ? 0 : 1; i < dnMethodDef._parameters._count; i++)
				{
					ParameterDefinition pd = new ParameterDefinition(ImportVvlTypeRef(dnMethodDef._parameters[i]));
					pd.Name = dnMethodDef._methodMeta._parameters[i]._name;
					md.Parameters.Add(pd);
				}

				if(md.HasBody)
				{
					md.Body.MaxStackSize = dnMethodDef._stackHeight;
					for(int i = 0; i < dnMethodDef._locals._count; i++)
					{
						md.Body.Variables.Add(new VariableDefinition(ImportVvlTypeRef(dnMethodDef._locals[i])));
					}
					ILProcessor il = md.Body.GetILProcessor();
					il.Append(il.Create(OpCodes.Ret));
				}

				td.Methods.Add(md);
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
		public void Finish()
		{
			_module.Write($"{_module.Name}.dll");
		}
	}
}