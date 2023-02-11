namespace igLibrary.Core
{
	public class igSizeTypeMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			if(igAlchemyCore.isPlatform64Bit(loader._platform)) return loader._stream.ReadUInt64();
			else                                                return (ulong)loader._stream.ReadUInt32();
		}
	}
}