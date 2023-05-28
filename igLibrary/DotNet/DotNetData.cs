namespace igLibrary.DotNet
{
	public struct DotNetData
	{
		object _data;
		ElementType _type;
		DataRepresentation _representation;

		public enum DataRepresentation
		{
			Normal = 0,
			Complex = 1,
			Indirect = 2,
			RawIndirect = 4,
			FieldReference = 8,
		}
	}
}