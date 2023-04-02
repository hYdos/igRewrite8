namespace igLibrary.Core
{
	public class igFile : igObject
	{
		public igFileDescriptor _file;
		public long _offset = 0;
		public igFileWorkItem.Priority _priority = igFileWorkItem.Priority.kPriorityNormal;
		public void Open(string path)
		{
			igFileContext.Singleton.Open(path, igFileContext.GetOpenFlags(FileAccess.Read, FileMode.Open), out _file, igBlockingType.kBlocking, _priority);
		}
	}
}