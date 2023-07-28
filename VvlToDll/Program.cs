using igLibrary.Core;
using igLibrary.DotNet;

namespace VvlToDll
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);
			ArkDllExport.Create();
			igFileContext.Singleton.Initialize(args[0]);
			igFileContext.Singleton.LoadArchive("archives/permanent.pak");
			igFileContext.Singleton.LoadArchive("archives/permanent_ps3.pak");
			igFileContext.Singleton.LoadArchive("archives/permanentdeveloper.pak");
			igFileContext.Singleton.LoadArchive("archives/gamestartup.pak");
			igFileContext.Singleton.LoadArchive("archives/ChopChop.pak");

			DotNetRuntime runtime = new DotNetRuntime();

			bool success;
			VvlLoader.Load("scripts:/interop/Core.vvl", runtime, out success);
			VvlLoader.Load("scripts:/interop/Runtime.vvl", runtime, out success);
			VvlLoader.Load("scripts:/interop/DebugLink.vvl", runtime, out success);
			VvlLoader.Load("scripts:/interop/VisualScript.vvl", runtime, out success);
			VvlLoader.Load("scripts:/interop/game.vvl", runtime, out success);
			VvlLoader.Load("scripts:/interop/DotNetAttributes.vvl", runtime, out success);
			VvlLoader.Load("scripts:/common.vvl", runtime, out success);
			VvlLoader.Load("scripts:/ui.vvl", runtime, out success);
			VvlLoader.Load("scripts:/behaviorHandlers.vvl", runtime, out success);
			VvlLoader.Load("scripts:/common_script_vs.vvl", runtime, out success);
			VvlLoader.Load("scripts:/Characters_script_vs.vvl", runtime, out success);

			VvlLoader.Load("scripts:/ChopChop_script.vvl", runtime, out success);

			DllExportManager.ExportAllVvls();
		}
	}
}