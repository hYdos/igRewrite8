using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igRangedQuadraticMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igRangedQuadratic data = new igRangedQuadratic();
			data._data = loader._stream.ReadBytes(0x20);
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			section._sh.WriteBytes(((igRangedQuadratic)value!)._data);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x20;
		public override Type GetOutputType() => typeof(igRangedQuadratic);
	}
}