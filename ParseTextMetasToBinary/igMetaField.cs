namespace PTMTB
{
	public class igMetaField
	{
		public string _typeName;
		public short _num;
		public string? _name;
		public ushort _offset;
		public List<igMetaField> _templateArgs = new List<igMetaField>();
		public List<igMetaField> _fieldList = new List<igMetaField>();

		public virtual void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			saver.SaveString(sh, _typeName);
			sh.WriteInt32(_templateArgs.Count);
			for(int i = 0; i < _templateArgs.Count; i++)
			{
				saver.SaveMetaField(sh, _templateArgs[i]);
			}
			saver.SaveString(sh, _name);
			sh.WriteInt16(_num == 0 ? (short)-1 : (short)_num);
			sh.WriteUInt16(_offset);
		}
		public virtual void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			int templateArgCount = sh.ReadInt32();
			_templateArgs.Capacity = templateArgCount;
			for(int i = 0; i < templateArgCount; i++)
			{
				_templateArgs.Add(loader.ReadMetaField(sh));
			}
			_name = loader.ReadString(sh);
			_num = sh.ReadInt16();
			_offset = sh.ReadUInt16();
		}
	}
	public class igMemoryRefMetaField : igMetaField
	{
		public igMetaField _memType;

		public override void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveMetaField(sh, _memType);
		}
		public override void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_memType = loader.ReadMetaField(sh);
		}
	}
	public class igMemoryRefArrayMetaField : igMemoryRefMetaField {}
	public class igMemoryRefHandleMetaField : igMetaField
	{
		public igMetaField _memType;

		public override void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveMetaField(sh, _memType);
		}
		public override void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_memType = loader.ReadMetaField(sh);
		}
	}
	public class igMemoryRefHandleArrayMetaField : igMemoryRefHandleMetaField {}
	public class igObjectRefMetaField : igMetaField
	{
		public string _metaObjectName;
		public override void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaObjectName);
		}
		public override void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaObjectName = loader.ReadString(sh);
		}
	}
	public class igObjectRefArrayMetaField : igObjectRefMetaField {}
	public class igHandleMetaField : igMetaField
	{
		public string _metaObjectName;
		public override void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaObjectName);
		}
		public override void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaObjectName = loader.ReadString(sh);
		}
	}
	public class igHandleArrayMetaField : igHandleMetaField {}
	public class igEnumMetaField : igMetaField
	{
		public string _metaEnumName;
		public override void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaEnumName);
		}
		public override void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaEnumName = loader.ReadString(sh);
		}
	}
	public class igEnumArrayMetaField : igEnumMetaField {}
	public class igStaticMetaField : igMetaField
	{
		public igMetaField _storageMetaField;
		public override void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveMetaField(sh, _storageMetaField);
		}
		public override void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_storageMetaField = loader.ReadMetaField(sh);
		}
	}
	public class igStaticArrayMetaField : igStaticMetaField {}
	public class igPropertyFieldMetaField : igMetaField
	{
		public igMetaField _innerMetaField;

		public override void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveMetaField(sh, _innerMetaField);
		}
		public override void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_innerMetaField = loader.ReadMetaField(sh);
		}
	}
	public class igPropertyFieldArrayMetaField : igPropertyFieldMetaField {}
	public class igBitFieldMetaField : igMetaField
	{
		public igMetaField _assignmentMetaField;
		public uint _shift;
		public uint _bits;

		public override void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			sh.WriteUInt32(_shift);
			sh.WriteUInt32(_bits);
			saver.SaveMetaField(sh, _assignmentMetaField);
		}
		public override void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_shift = sh.ReadUInt32();
			_bits = sh.ReadUInt32();
			_assignmentMetaField = loader.ReadMetaField(sh);
		}
	}
	public class igBitFieldArrayMetaField : igBitFieldMetaField {}

	public class igCompoundMetaFieldInfo
	{
		public string _fieldTypeName;
		public List<igMetaField> _fieldList = new List<igMetaField>();

		public void DumpArkData(igArkCoreSaver2 saver, StreamHelper sh)
		{
			saver.SaveString(sh, _fieldTypeName);
			sh.WriteInt32(_fieldList.Count);
			for(int i = 0; i < _fieldList.Count; i++)
			{
				saver.SaveMetaField(sh, _fieldList[i]);
			}
		}
		public void UndumpArkData(igArkCoreSaver2 loader, StreamHelper sh)
		{
			_fieldTypeName = loader.ReadString(sh);
			int fieldCount = sh.ReadInt32();
			_fieldList.Capacity = fieldCount;
			for(int i = 0; i < fieldCount; i++)
			{
				_fieldList.Add(loader.ReadMetaField(sh));
			}
		}
	}
}