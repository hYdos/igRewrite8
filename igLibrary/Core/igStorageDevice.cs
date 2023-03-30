namespace igLibrary.Core
{
	public abstract class igStorageDevice : igFileWorkItemProcessor
	{
		public string _name;
		public string _path;
		public uint _readMediaAlignment;
		public uint _writeMediaAlignment;
		public uint _memoryAlignment;
		public uint _randomAccessTransferSize;
		public uint _sequentialTransferSize;
		public bool _readOnly;
		public bool _removableMedia;

		public abstract void Exists(igFileWorkItem workItem);
		public abstract void Open(igFileWorkItem workItem);
		public abstract void Close(igFileWorkItem workItem);
		public abstract void Read(igFileWorkItem workItem);
		public abstract void Write(igFileWorkItem workItem);
		public abstract void Truncate(igFileWorkItem workItem);
		public abstract void Mkdir(igFileWorkItem workItem);
		public abstract void Rmdir(igFileWorkItem workItem);
		public abstract void GetFileList(igFileWorkItem workItem);
		public abstract void GetFileListWithSizes(igFileWorkItem workItem);
		public abstract void Unlink(igFileWorkItem workItem);
		public abstract void Rename(igFileWorkItem workItem);
		public abstract void Prefetch(igFileWorkItem workItem);
		public abstract void Format(igFileWorkItem workItem);
		public abstract void Commit(igFileWorkItem workItem);
	}
}