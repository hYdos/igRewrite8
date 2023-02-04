using System.Reflection;

namespace igLibrary.Core
{
	public class igEnumMetaField : igMetaField
	{
		public igMetaEnum _metaEnum;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaEnum._name);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaEnum = igArkCore.GetMetaEnum(loader.ReadString(sh));
		}
	}
	public class igEnumArrayMetaField : igEnumMetaField
	{
		short _num;
	}
}