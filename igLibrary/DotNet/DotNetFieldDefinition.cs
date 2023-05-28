namespace igLibrary.DotNet
{
	public struct DotNetFieldDefinition
	{
		public string Name;
		public int Flags;
		public DotNetData Data;
	}
	public class DotNetFieldDefinitionList : igTDataList<DotNetFieldDefinition>{}
}