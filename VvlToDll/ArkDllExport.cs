using igLibrary.Core;
using igLibrary.DotNet;
using Mono.Cecil;

namespace VvlToDll
{
	public class ArkDllExport
	{
		public static Dictionary<igBaseMeta, TypeDefinition> _metaTypeLookup;
		public static Dictionary<igBaseMeta, TypeDefinition> _dynamicMetaTypeLookup = new Dictionary<igBaseMeta, TypeDefinition>();
		public static ModuleDefinition module;

		public static void Create()
		{
			if(module != null) return;
			module = ModuleDefinition.CreateModule("Ark.dll", ModuleKind.Dll);
			_metaTypeLookup = new Dictionary<igBaseMeta, TypeDefinition>();
			InstantiateTypes();
			DefineEnums();
			DefineObjects();
			module.Write("Ark.dll");
		}
		private static void InstantiateTypes()
		{
			foreach(igMetaEnum metaEnum in igArkCore._metaEnums)
			{
				TypeDefinition td = new TypeDefinition("Enums", metaEnum._name, TypeAttributes.Public | TypeAttributes.Sealed);
				_metaTypeLookup.Add(metaEnum, td);
				module.Types.Add(td);
			}
			foreach(igMetaObject metaObject in igArkCore._metaObjects)
			{
				TypeDefinition td = new TypeDefinition("Classes", metaObject._name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.AnsiClass);
				_metaTypeLookup.Add(metaObject, td);
				module.Types.Add(td);
			}
		}
		private static void DefineEnums()
		{
			foreach(KeyValuePair<igBaseMeta, TypeDefinition> kvp in _metaTypeLookup)
			{
				if(kvp.Key is not igMetaEnum metaEnum) break;

				TypeDefinition td = kvp.Value;
				td.BaseType = module.ImportReference(typeof(Enum));
				td.Fields.Add(new FieldDefinition("__value", FieldAttributes.Public | FieldAttributes.SpecialName, module.ImportReference(typeof(int))));
				for(int i = 0; i < metaEnum._names.Count; i++)
				{
					FieldDefinition fd = new FieldDefinition(metaEnum._names[i], FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal, td);
					fd.Constant = metaEnum._values[i];
					td.Fields.Add(fd);
				}
			}
		}
		private static void DefineObjects()
		{
			foreach(KeyValuePair<igBaseMeta, TypeDefinition> kvp in _metaTypeLookup)
			{
				if(kvp.Key is not igMetaObject metaObject) continue;

				TypeDefinition td = kvp.Value;
				if(metaObject._parent != null)
				{
					td.BaseType = _metaTypeLookup[metaObject._parent];
				}
				else continue;

				for(int i = metaObject._parent._metaFields.Count; i < metaObject._metaFields.Count; i++)
				{
					AddField(module, td, metaObject, metaObject._metaFields[i], x => x == null ? module.ImportReference(typeof(int)) : _metaTypeLookup[x]);
				}
			}
		}
		public static void AddField(ModuleDefinition md, TypeDefinition target, igMetaObject meta, igMetaField metaField, Func<igBaseMeta, TypeReference> resolver)
		{
			FieldDefinition fd = new FieldDefinition(metaField._name, FieldAttributes.Public, GetFieldTypeRef(md, metaField, resolver));
			if(metaField is igStaticMetaField)
			{
				fd.Attributes |= FieldAttributes.Static;
			}
			target.Fields.Add(fd);
		}
		public static TypeReference GetFieldTypeRef(ModuleDefinition md, igMetaField field, Func<igBaseMeta, TypeReference> resolver)
		{
			     if(field is igIntMetaField)                   return md.ImportReference(typeof(int));
			else if(field is igUnsignedIntMetaField)           return md.ImportReference(typeof(uint));
			else if(field is igShortMetaField)                 return md.ImportReference(typeof(short));
			else if(field is igUnsignedShortMetaField)         return md.ImportReference(typeof(ushort));
			else if(field is igLongMetaField)                  return md.ImportReference(typeof(long));
			else if(field is igUnsignedLongMetaField)          return md.ImportReference(typeof(ulong));
			else if(field is igCharMetaField)                  return md.ImportReference(typeof(sbyte));
			else if(field is igUnsignedCharMetaField)          return md.ImportReference(typeof(byte));
			else if(field is igFloatMetaField)                 return md.ImportReference(typeof(float));
			else if(field is igDoubleMetaField)                return md.ImportReference(typeof(double));
			else if(field is igIntPtrMetaField)                return md.ImportReference(typeof(IntPtr));
			else if(field is igUnsignedIntPtrMetaField)        return md.ImportReference(typeof(UIntPtr));
			else if(field is igBoolMetaField)                  return md.ImportReference(typeof(bool));
			else if(field is igStringMetaField)                return md.ImportReference(typeof(string));
			else if(field is igDotNetEnumMetaField dnemf)      return resolver.Invoke(dnemf._definedMetaEnum);
			else if(field is igEnumMetaField emf)              return resolver.Invoke(emf._metaEnum);
			else if(field is igMemoryRefMetaField mrmf)        return CreateMemoryRefType(md, GetFieldTypeRef(md, mrmf._memType, resolver));
			else if(field is igMemoryRefHandleMetaField mrhmf) return CreateMemoryRefType(md, GetFieldTypeRef(md, mrhmf._memType, resolver));
			else if(field is igObjectRefMetaField ormf)        return resolver.Invoke(ormf._metaObject);
			else if(field is igHandleMetaField hmf)            return CreateHandleType(md, resolver.Invoke(hmf._metaObject));
			else                                               return md.ImportReference(typeof(object));
		}
		private static TypeReference CreateMemoryRefType(ModuleDefinition md, TypeReference typeRef)
		{
			GenericInstanceType memInst = new GenericInstanceType(md.ImportReference(typeof(igMemory<>)));
			memInst.GenericArguments.Add(typeRef);
			return memInst;
		}
		private static TypeReference CreateHandleType(ModuleDefinition md, TypeReference typeRef)
		{
			GenericInstanceType hndInst = new GenericInstanceType(md.ImportReference(typeof(igTHandle<>)));
			hndInst.GenericArguments.Add(typeRef);
			return hndInst;
		}
	}
}