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

		public virtual void PostUndump(){}

		public virtual igMetaField? GetFieldByName(string name) => null;

		//The following will be used to generate only the types that need to be loaded
		public virtual void DeclareType(){}
		public virtual void DefineType(){}
		public virtual void FinalizeType(){}
		protected void ReadyFieldDependancy(igMetaField field)
		{
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
			if(field is igObjectRefMetaField objField) objField._metaObject.FinalizeType();
			else if(field is igHandleMetaField hndField) hndField._metaObject.FinalizeType();
			else if(field is igMemoryRefMetaField memField) FinalizeFieldDependancy(memField._memType);
			else if(field is igCompoundMetaField compoundField) compoundField._compoundFieldInfo.FinalizeType();
			else if(field is igMemoryRefHandleMetaField memHndField) FinalizeFieldDependancy(memHndField._memType);
			else if(field is igStaticMetaField staticField) FinalizeFieldDependancy(staticField._storageMetaField);

			for(uint i = 0; i < field.GetTemplateParameterCount(); i++)
			{
				FinalizeFieldDependancy(field.GetTemplateParameter(i));
			}
		}
	}
}