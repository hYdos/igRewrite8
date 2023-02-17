namespace igLibrary.Core
{
	//Is technically a struct, labelled it a class because it's stored by reference
	public class igHandle
	{
		public ushort _refCount;
		public ushort _unk;
		public igName _namespace;
		public igName _alias;
		public igObject _object;
	}
}