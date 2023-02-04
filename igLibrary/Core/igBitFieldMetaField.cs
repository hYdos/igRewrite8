using System.Reflection;

namespace igLibrary.Core
{
	public class igBitFieldMetaField : igMetaField
	{
		public igMetaField _assignmentMetaField;
		public uint _bits;
		public uint _shift;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			sh.WriteUInt32(_shift);
			sh.WriteUInt32(_bits);
			_assignmentMetaField.DumpArkData(saver, sh);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_shift = sh.ReadUInt32();
			_bits = sh.ReadUInt32();
			_assignmentMetaField = loader.ReadMetaField(sh);
		}
	}
}