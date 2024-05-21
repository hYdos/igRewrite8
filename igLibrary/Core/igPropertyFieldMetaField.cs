using System.Reflection;

namespace igLibrary.Core
{
	public class igPropertyFieldMetaField : igMetaField
	{
		public igMetaField _innerMetaField;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _setCallbackFunction;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _getCallbackFunction;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveMetaField(sh, _innerMetaField);
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