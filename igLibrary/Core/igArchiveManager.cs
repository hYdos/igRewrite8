namespace igLibrary.Core
{
	public class igArchiveManager : igFileWorkItemProcessor
	{
		public igArchiveList _archiveList = new igArchiveList();
		public igArchiveList _patchArchives = new igArchiveList();

		public igArchive LoadArchive(string path)
		{
			if(TryGetArchive(path, out igArchive? loaded)) return loaded;
			loaded = new igArchive();
			loaded.Open(path, igBlockingType.kMayBlock);
			_archiveList.Append(loaded);
			return loaded;
		}
		public bool TryGetArchive(string path, out igArchive? archive)
		{
			igFilePath fp = new igFilePath();
			fp.Set(path);
			for(int i = 0; i < _archiveList._count; i++)
			{
				if(_archiveList[i]._path.ToLower() == fp._path.ToLower())
				{
					archive = _archiveList[i];
					return true;
				}
			}
			archive = null;
			return false;
		}

		public override void Process(igFileWorkItem workItem)
		{
			if(workItem._type == igFileWorkItem.WorkType.kTypeFileList)
			{
				uint pathHash = igHash.Hash(workItem._file._path);
				for(int i = 0; i < _patchArchives._count; i++)
				{
					igArchive archive = _patchArchives[i];
					if(igHash.Hash(archive._nativePath) == pathHash)
					{
						archive.Process(workItem);
						return;
					}
				}
				for(int i = 0; i < _archiveList._count; i++)
				{
					igArchive archive = _archiveList[i];
					if(igHash.Hash(archive._nativePath) == pathHash)
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
					archive.Process(workItem);
					if(workItem._status == igFileWorkItem.Status.kStatusComplete) return;
				}
				for(int i = 0; i < _archiveList._count; i++)
				{
					igArchive archive = _archiveList[i];
					archive.Process(workItem);
					if(workItem._status == igFileWorkItem.Status.kStatusComplete) return;
				}
			}

		giveUp:
			SendToNextProcessor(workItem);
		}
	}
}