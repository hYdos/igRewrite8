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

			DotNetRuntime runtime = new DotNetRuntime();

			DotNetLibrary lib = VvlLoader.Load("scripts:/interop/game.vvl", runtime, out bool success);
			//DotNetLibrary lib = VvlLoader.Load("scripts:/ChopChop_script.vvl", runtime, out bool success);

			DllExporter vvlExporter = new DllExporter();
			vvlExporter.ExportLibrary(lib, "Game.dll");
		}
	}
}