using System.Reflection;

namespace igLibrary.Core
{
	public class igMemoryRefMetaField : igMetaField
	{
		public igMetaField _memType;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			_memType.DumpArkData(saver, sh);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_memType = loader.ReadMetaField(sh);
		}
	}
	public class igMemoryRefArrayMetaField : igMemoryRefMetaField
	{
		short _num;
	}
}