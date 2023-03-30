namespace igLibrary.Core
{
	public class igArchiveMountManager : igFileWorkItemProcessor
	{
		public bool _isWorkPending;

		public override void Process(igFileWorkItem workItem)
		{
			SendToNextProcessor(workItem);
		}
		public void MountArchive(igArchive archive)
		{
			igFileWorkItem workItem = igFileContext.Singleton.AllocateWorkItem();
			workItem._buffer = archive;
			workItem._type = igFileWorkItem.WorkType.kTypeInvalid;
			workItem._priority = igFileWorkItem.Priority.kPriorityNormal;
			workItem._status = igFileWorkItem.Status.kStatusInactive;
		}
	}
}