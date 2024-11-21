using igLibrary.Core;

namespace igLibrary.DotNet
{
	public class DotNetTypeMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			ulong baseOffset = loader._stream.Tell64();
			DotNetType data = new DotNetType();

			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 8;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x18 + (igAlchemyCore.isPlatform64Bit(platform) ? 8u : 0u);
		public override Type GetOutputType() => typeof(DotNetType);


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			// This is incorrect
			DotNetType type = new DotNetType();
			if (!Enum.TryParse(typeof(ElementType), "kElementType" + input, out object? elementType))
			{
				return false;
			}
			type._elementType = (ElementType)elementType!;
			return true;
		}
	}
}