using igLibrary.Core;
using igLibrary.DotNet;

namespace igCauldron3
{
	public static class PackagePrecacher
	{
		public static void PrecachePackage(string fileName)
		{
			string fixedFileName = fileName;
			if(!fixedFileName.StartsWith("packages")) fixedFileName = "packages/" + fixedFileName;
			if(!fixedFileName.EndsWith("_pkg.igz")) fixedFileName += "_pkg.igz";

			string archiveName = Path.GetFileNameWithoutExtension(fixedFileName);
			archiveName = "archives/" + archiveName.Substring(0, archiveName.Length - 4) + ".pak";

			igArchive loaded = igFileContext.Singleton.LoadArchive(archiveName);
			igObjectDirectory pkgDir = igObjectStreamManager.Singleton.Load(fixedFileName);
			igStringRefList pkgList = (igStringRefList)pkgDir._objectList[0];
			igObjectLoader scriptLoader = igObjectLoader._loaders["DotNet"];
			for(int i = 0; i < pkgList._count; i += 2)
			{
				     if(pkgList[i] == "script") scriptLoader.ReadFile(pkgList[i+1], igBlockingType.kBlocking);
				else if(pkgList[i] == "pkg")    PrecachePackage(pkgList[i+1]);
			}
		}
	}
}