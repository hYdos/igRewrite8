using System.Reflection;

namespace igLibrary.Core
{
	public class igPropertyFieldMetaField : igMetaField
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
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0;
		public override Type GetOutputType() => null;
	}
}