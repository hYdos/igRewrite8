namespace igLibrary.Core
{
	public class igArchiveManager : igFileWorkItemProcessor
	{
		public Dictionary<string, igArchive> archives = new Dictionary<string, igArchive>();
		public igArchiveList _archiveList = new igArchiveList();

		public bool ExistsInOpenArchives(string path)
		{
			igFilePath fp = new igFilePath();
			fp.Set(path);

			bool fileFound = false;
			foreach(KeyValuePair<string, igArchive> archive in archives)
			{
				if(archive.Value.HasFile(fp._path))
				{
					fileFound = true;
					break;
				}
			}
			return fileFound;
		}

		public void AddArchiveToPool(string archivePath)
		{
			igFilePath fp = new igFilePath();
			fp.Set(archivePath);
			if(fp._directory == string.Empty)
			{
				fp.Set($"archives/{archivePath}");
			}
			if(!archives.ContainsKey(archivePath))
			{
				try
				{
					igArchive archive = new igArchive(fp._path);
					archives.Add(Path.GetFileNameWithoutExtension(fp._path), archive);
				}
				catch(FileNotFoundException)		//There's a chance a pak is missing on the ios ports of ssc
				{
					Console.WriteLine($"WARNING: {archivePath} IS MISSING");
				}
			}
		}

		public void GetFile(string filePath, Stream output)
		{
			igFilePath fp = new igFilePath();
			fp.Set(filePath);

			bool fileFound = false;
			foreach(KeyValuePair<string, igArchive> archive in archives)
			{
				if(archive.Value.HasFile(fp._path))
				{
					archive.Value.ExtractFile(fp._path, output);
					fileFound = true;
					break;
				}
			}
			if(!fileFound) throw new FileNotFoundException($"COULD NOT FIND {filePath}");

			output.Flush();
		}

		public override void Process(igFileWorkItem workItem)
		{
			if(workItem._type == igFileWorkItem.WorkType.kTypeFileList)
			{
				for(int i = 0; i < _archiveList._count; i++)
				{
					igArchive archive = (igArchive)_archiveList[i];
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

				for(int i = 0; i < _archiveList._count; i++)
				{
					igArchive archive = (igArchive)_archiveList[i];
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