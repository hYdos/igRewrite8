using igLibrary.Core;

namespace igLibrary.Utils
{
	public class igVariantMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVariant data = new igVariant();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x10;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x20;
		public override Type GetOutputType() => typeof(igVariant);


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input) => false;
	}
}