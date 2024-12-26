namespace igLibrary.Core
{
	/// <summary>
	/// The archive mount manager
	/// </summary>
	public class igArchiveMountManager : igFileWorkItemProcessor
	{
		public bool _isWorkPending;


		/// <summary>
		/// Process a work item
		/// </summary>
		/// <param name="workItem">the work item</param>
		public override void Process(igFileWorkItem workItem)
		{
			SendToNextProcessor(workItem);
		}


		/// <summary>
		/// Mount a specific archive
		/// </summary>
		/// <param name="archive">The archive</param>
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