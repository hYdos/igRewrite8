using igLibrary.Core;
using Mono.Cecil;

namespace VvlToDll
{
	public class ArkDllExport
	{
		public static Dictionary<igBaseMeta, TypeDefinition> _metaTypeLookup;
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
					AddField(td, metaObject, i);
				}
			}
		}
		private static void AddField(TypeDefinition target, igMetaObject meta, int index)
		{
			igMetaField field = meta._metaFields[index];
			FieldDefinition fd = new FieldDefinition(field._name, FieldAttributes.Public, module.ImportReference(typeof(object)));
			if(field is igStaticMetaField) fd.Attributes |= FieldAttributes.Static;
			target.Fields.Add(fd);
		}
	}
}