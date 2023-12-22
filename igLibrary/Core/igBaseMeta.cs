namespace igLibrary.Core
{
	public class igBaseMeta : igObject
	{
		public string? _name;
		internal Type _internalType;
		protected bool _beganFinalization = false;
		protected bool _beganFinalizationPrep = false;
		protected bool _finishedFinalization = false;
		protected bool _finishedFinalizationPrep = false;
		protected bool _gatheredDependancies = false;
		public BuildPriority _priority = BuildPriority.Normal;
		public enum BuildPriority
		{
			Low,
			Normal,
			High
		}
		public virtual void PostUndump(){}

		public virtual igMetaField? GetFieldByName(string name) => null;

		//The following will be used to generate only the types that need to be loaded
		public virtual void DeclareType(){}
		public virtual void DefineType(){}
		public virtual void FinalizeType(){}
		protected void ReadyFieldDependancy2(igMetaField field)
		{
			if(field is igObjectRefMetaField objField) objField._metaObject.GatherDependancies();
			else if(field is igMemoryRefMetaField memField) ReadyFieldDependancy2(memField._memType);
			else if(field is igMemoryRefHandleMetaField memHndField) ReadyFieldDependancy2(memHndField._memType);
			else if(field is igStaticMetaField staticField) ReadyFieldDependancy2(staticField._storageMetaField);
		}
		protected void ReadyCompoundFieldDependancy(igMetaField field)
		{
			if(field is igOrderedMapMetaField omField)
			{
				ReadyCompoundFieldDependancy(omField._t);
				ReadyFieldDependancy2(omField._t);
				ReadyCompoundFieldDependancy(omField._u);
				ReadyFieldDependancy2(omField._u);
			}
			else if(field is igCompoundMetaField compoundField) compoundField._compoundFieldInfo.GatherDependancies();
			else if(field is igMemoryRefMetaField memField) ReadyCompoundFieldDependancy(memField._memType);
			else if(field is igMemoryRefHandleMetaField memHndField) ReadyCompoundFieldDependancy(memHndField._memType);
			else if(field is igStaticMetaField staticField) ReadyCompoundFieldDependancy(staticField._storageMetaField);
		}
		protected void ReadyFieldDependancy(igMetaField field)
		{
			if(field == null) return;
			if(field is igObjectRefMetaField objField) objField._metaObject.DeclareType();
			else if(field is igHandleMetaField hndField) hndField._metaObject.DeclareType();
			else if(field is igMemoryRefMetaField memField) ReadyFieldDependancy(memField._memType);
			else if(field is igCompoundMetaField compoundField) compoundField._compoundFieldInfo.DeclareType();
			else if(field is igMemoryRefHandleMetaField memHndField) ReadyFieldDependancy(memHndField._memType);
			else if(field is igStaticMetaField staticField) ReadyFieldDependancy(staticField._storageMetaField);

			for(uint i = 0; i < field.GetTemplateParameterCount(); i++)
			{
				ReadyFieldDependancy(field.GetTemplateParameter(i));
			}
		}
		protected void FinalizeFieldDependancy(igMetaField field)
		{
			//if(field is igObjectRefMetaField objField) objField._metaObject.FinalizeType();
			//else if(field is igHandleMetaField hndField) hndField._metaObject.FinalizeType();		//Not needed since the field is igHandle
			if(field is igMemoryRefMetaField memField) FinalizeFieldDependancy(memField._memType);
			else if(field is igCompoundMetaField compoundField) compoundField._compoundFieldInfo.FinalizeType();
			else if(field is igMemoryRefHandleMetaField memHndField) FinalizeFieldDependancy(memHndField._memType);
			else if(field is igStaticMetaField staticField) FinalizeFieldDependancy(staticField._storageMetaField);

			for(uint i = 0; i < field.GetTemplateParameterCount(); i++)
			{
				FinalizeFieldDependancy(field.GetTemplateParameter(i));
			}
		}
		public virtual void GatherDependancies(){}
		public virtual void DefineType2(){}
		public virtual void CreateType2(){}
		//https://stackoverflow.com/questions/74616/how-to-detect-if-type-is-another-generic-type/1075059#1075059
		public static bool IsAssignableToGenericType(Type type, Type genericType)
		{
			return (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
				||  (type.BaseType != null && IsAssignableToGenericType(type.BaseType, genericType));
		}
 	}
	public class igBaseMetaList : igTObjectList<igBaseMeta>{}
}