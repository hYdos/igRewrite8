using System.Text;

namespace igLibrary
{
	//It'll be funny to implement the cdn system here	
	public static class CArchive
	{
		public static bool s_bCacheEnabled = false;
		public static bool s_bDoPackages = true;
		public static int Open(string filePath, out igArchive? archive, EMemoryPoolID poolId, int flags)
		{
			int res = 0;
			archive = null;
			if((flags & 4) == 0 && !s_bDoPackages && true)	//replace the false with !CContentDeployment::isContentDeploymentEnabled()
			{
				res = 0;
			}
			else
			{
				string archivePath;
				if((flags & 8) == 0)
				{
					archivePath = GetArchivePath(filePath);
				}
				else
				{
					archivePath = filePath;
				}
				if(!s_bDoPackages && true)	//replace the true with !CContentDeployment::isContentDeploymentEnabled()
				{
					res = 0;
				}
				else
				{
					//CStreamingInstall stuff
				}
				if((flags & 8) == 0)
				{
					archivePath = GetArchivePath(filePath);
				}
				if(res == 0 && ((flags & 4) != 0 || s_bDoPackages))
				{
					//should implement pools
					igFilePath fp = new igFilePath();
					fp.Set(filePath);
					archive = new igArchive();
					archive._enableCache = s_bCacheEnabled;
					archive._loadNameTable = (flags & 1) != 0;
					archive._override = (flags & 2) != 0;
					archive._sequentialRead = fp._file == "loosefiles";
					archive.Open(archivePath, igBlockingType.kBlocking);
					igFileContext.Singleton._archiveManager._archiveList.Append(archive);
				}
			}
			return res;
		}
		public static void Close(igArchive archive)
		{
			archive.Close(igBlockingType.kBlocking);
		}
		public static bool CanOpen(string filePath)
		{
			return false; //???
		}
		public static string GetArchivePath(string filePath)
		{
			igFilePath fp = new igFilePath();
			fp.Set(filePath);
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("app:/archives/{0}.pak", fp._file.ToLower());
			return sb.ToString();
		}
		public static void StartAccessing(string filePath)
		{
			
		}
	}
}