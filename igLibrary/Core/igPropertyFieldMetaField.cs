using System.Reflection;

namespace igLibrary.Core
{
	public class igPropertyFieldMetaField : igRefMetaField
	{
		public igMetaField _innerMetaField;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			_innerMetaField.DumpArkData(saver, sh);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_innerMetaField = loader.ReadMetaField(sh);
		}
	}
}