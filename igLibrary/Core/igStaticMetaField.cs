using System.Reflection;

namespace igLibrary.Core
{
	public class igStaticMetaField : igMetaField
	{
		public igMetaField _storageMetaField;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _staticPointer;
		[Obsolete("This exists for the reflection system, do not use.")] public igMetaObject? _owner;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveMetaField(sh, _storageMetaField);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_storageMetaField = loader.ReadMetaField(sh);
		}

		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0;
		public override Type GetOutputType() => _storageMetaField.GetOutputType();
	}
}