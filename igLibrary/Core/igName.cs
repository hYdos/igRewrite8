namespace igLibrary.Core
{
	[igStruct]
	public struct igName
	{
		public string _string = string.Empty;
		public uint _hash = 0;
		public igName(){}
		public igName(string name)
		{
			SetString(name);
		}
		public void SetString(string newString)
		{
			_string = newString;
			_hash = igHash.Hash(newString.ToLower());
		}
	}
}