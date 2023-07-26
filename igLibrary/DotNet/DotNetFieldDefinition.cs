namespace igLibrary.DotNet
{
	public struct DotNetFieldDefinition
	{
		public string Name;
		public EFlags Flags;
		public DotNetData Data;

		public enum EFlags
		{
			kHandle = 1,
			kConstruct = 1
		}
	}
	public class DotNetFieldDefinitionList : igTDataList<DotNetFieldDefinition>{}
}