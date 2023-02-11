namespace igLibrary.Core
{
	public class igIntMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			return loader._stream.ReadInt32();
		}
	}
}