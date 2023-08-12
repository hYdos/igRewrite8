namespace igLibrary.DotNet
{
	public struct DotNetFieldDefinition
	{
		public string Name;
		public FieldDefFlags Flags;
		public DotNetData Data;

		public enum FieldDefFlags
		{
			kHandle = 1,
			kConstruct = 2
		}
	}
	public class DotNetFieldDefinitionList : igTDataList<DotNetFieldDefinition>{}
}