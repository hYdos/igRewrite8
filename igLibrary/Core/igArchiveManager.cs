namespace igLibrary.Core
{
	public class igArchiveManager : igFileWorkItemProcessor
	{
		public igArchiveList _archiveList = new igArchiveList();
		public igArchiveList _patchArchives = new igArchiveList();

		public override void Process(igFileWorkItem workItem)
		{
			if(workItem._type == igFileWorkItem.WorkType.kTypeFileList)
			{
				for(int i = 0; i < _patchArchives._count; i++)
				{
					igArchive archive = _patchArchives[i];
					try
					{
						if(archive._nativePath == workItem._file._path)
						{
							archive.Process(workItem);						
							return;
						}
					}
					catch(IOException){}
				}
				for(int i = 0; i < _archiveList._count; i++)
				{
					igArchive archive = _archiveList[i];
					if(archive._nativePath == workItem._file._path)
					{
						archive.Process(workItem);
						return;
					}
				}
			}
			else
			{
				if(workItem._type == igFileWorkItem.WorkType.kTypeInvalid) goto giveUp;

				for(int i = 0; i < _patchArchives._count; i++)
				{
					igArchive archive = _patchArchives[i];
					try
					{
						if(archive.HasFile(workItem._path))
						{
							archive.Process(workItem);
							return;
						}
					}
					catch(IOException){}
				}
				for(int i = 0; i < _archiveList._count; i++)
				{
					igArchive archive = _archiveList[i];
					if(archive.HasFile(workItem._path))
					{
						archive.Process(workItem);
						return;
					}
				}
			}

		giveUp:
			SendToNextProcessor(workItem);
		}
	}
}