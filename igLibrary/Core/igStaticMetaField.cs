using System.Reflection;

namespace igLibrary.Core
{
	public class igStaticMetaField : igRefMetaField
	{
		public igMetaField _storageMetaField;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			_storageMetaField.DumpArkData(saver, sh);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_storageMetaField = loader.ReadMetaField(sh);
		}
	}
}