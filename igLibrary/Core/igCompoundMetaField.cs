namespace igLibrary.Core
{
	public class igCompoundMetaField : igMetaField
	{
		public igCompoundMetaFieldInfo _compoundFieldInfo;

		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);
			string typeName = loader.ReadString(sh);
			_compoundFieldInfo = igArkCore.GetCompoundFieldInfo(typeName);
		}
	}

	public class igCompoundMetaFieldInfo : igBaseMeta
	{
		public List<igMetaField> _fieldList = new List<igMetaField>();

		public void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			_name = loader.ReadString(sh);
			int fieldCount = sh.ReadInt32();
			_fieldList.Capacity = fieldCount;
			for(int i = 0; i < fieldCount; i++)
			{
				_fieldList.Add(loader.ReadMetaField(sh));
			}
		}
	}
}