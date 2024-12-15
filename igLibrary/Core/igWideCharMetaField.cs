namespace igLibrary.Core
{
	public class igWideCharMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadUnicodeChar();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteUnicodeChar((char)value!);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 2;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 2;
		public override Type GetOutputType() => typeof(char);
	}
}