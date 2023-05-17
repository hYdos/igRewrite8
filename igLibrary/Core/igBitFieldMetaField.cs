using System.Reflection;

namespace igLibrary.Core
{
	public class igBitFieldMetaField : igMetaField
	{
		public igMetaField _storageMetaField;
		public igMetaField _assignmentMetaField;
		public uint _bits;
		public uint _shift;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			sh.WriteUInt32(_shift);
			sh.WriteUInt32(_bits);
			saver.SaveString(sh, _storageMetaField._name);
			_assignmentMetaField.DumpArkData(saver, sh);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_shift = sh.ReadUInt32();
			_bits = sh.ReadUInt32();
			_storageMetaField = _parentMeta.GetFieldByName(loader.ReadString(sh));
			_assignmentMetaField = loader.ReadMetaField(sh);
		}
		//Look into this
		public override object? ReadIGZField(igIGZLoader loader)
		{
			ulong storage = (ulong)Convert.ChangeType(_storageMetaField.ReadIGZField(loader), typeof(ulong));
			storage = (storage >> (int)_shift) & (0xFFFFFFFFFFFFFFFF >> (int)(64 - _bits));

			if(_assignmentMetaField is igUnsignedIntMetaField)	return unchecked((uint)storage);
			if(_assignmentMetaField is igIntMetaField)			return unchecked((int)storage);
			if(_assignmentMetaField is igUnsignedCharMetaField)	return unchecked((byte)storage);
			if(_assignmentMetaField is igCharMetaField)			return unchecked((sbyte)storage);
			if(_assignmentMetaField is igBoolMetaField)			return storage != 0;
			if(_assignmentMetaField is igUnsignedLongMetaField)	return storage;
			if(_assignmentMetaField is igLongMetaField)			return unchecked((long)storage);
			if(_assignmentMetaField is igEnumMetaField enumMf)	return enumMf._metaEnum.GetEnumFromValue((int)storage);

			throw new NotImplementedException($"_assignmentMetaField for {_assignmentMetaField.GetType().Name} is not implemented, contact a developer.");
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			return;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0;
		public override Type GetOutputType() => _assignmentMetaField.GetOutputType();
	}
}