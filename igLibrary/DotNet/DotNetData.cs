namespace igLibrary.DotNet
{
	public struct DotNetData
	{
		public object? _data;
		public DotNetType _type;
		public DataRepresentation _representation;
		public uint _maybeRepresentation;

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