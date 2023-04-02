namespace igLibrary.Core
{
	public class igFileCache : igFileWorkItemProcessor
	{
		public override void Process(igFileWorkItem workItem)
		{
			SendToNextProcessor(workItem);
		}
	}
}