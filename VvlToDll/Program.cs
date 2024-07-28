using igLibrary;
using igLibrary.Core;
using igLibrary.DotNet;
using igLibrary.Gfx;

namespace VvlToDll
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			if(args.Length == 0)
			{
				Console.WriteLine(@"
VvlToDll

Utility for converting Vvl files to DotNet Dlls so they can be viewed in an external tool.

VvlToDll -g <base game folder> -u <update.pak> -o <output directory> -p <platform> -- <...packages>
- base game folder: folder containing archives folder
- update.pak: self explanatory
- output directory: where the dlls get dumped
- platform: IG_CORE_PLATFORM name
- packages: game paths to package, can be written as ""generated/maps/UI/MainMenuBackground/MainMenuBackground"",
  can find these by looking in the packages folder of an archive.
");
				return;
			}

			string? gamePath = null;
			string? updatePath = null;
			string? outputDir = null;
			IG_CORE_PLATFORM platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT;
			List<string> packages = new List<string>();
			for(int i = 0; i < args.Length; i++)
			{
				string flag = args[i];
				switch(flag)
				{
					case "-g":
						gamePath = args[++i];
						break;
					case "-u":
						updatePath = args[++i];
						break;
					case "-o":
						outputDir = args[++i];
						break;
					case "-p":
						platform = Enum.Parse<IG_CORE_PLATFORM>(args[++i]);
						break;
					case "--":
						i++;
						for(; i < args.Length; i++)
						{
							packages.Add(args[i]);
						}
						break;
				}
			}

			if(gamePath == null)
			{
				Console.WriteLine("Missing game path");
				return;
			}
			if(updatePath == null)
			{
				Console.WriteLine("Missing update path");
				return;
			}
			if(outputDir == null)
			{
				Console.WriteLine("Missing output directory");
				return;
			}
			if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT)
			{
				Console.WriteLine("Missing platform.");
				return;
			}

			igRegistry.GetRegistry()._platform = platform;
			igRegistry.GetRegistry()._gfxPlatform = igGfx.GetGfxPlatformFromCore(platform);
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);
			ArkDllExport.Create();
			igAlchemyCore.InitializeSystems();
			igFileContext.Singleton.Initialize(gamePath);
			igFileContext.Singleton.InitializeUpdate(updatePath);

			//CClient::loadGameStartupPackages
			//CPrecacheManager._Instance.PrecachePackage("data:/archives/languagestartup", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage($"generated/packageXmls/permanent_{igAlchemyCore.GetPlatformString(platform)}", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/essentialui", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/legal", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/gamestartup", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanentdeveloper", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/SoundBankData", EMemoryPoolID.MP_DEFAULT);

			//CClient::loadDeferredPackages
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/maps/zoneinfos", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent_2015", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_Mobile", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_FrontEnd", EMemoryPoolID.MP_DEFAULT);

			for(int i = 0; i < packages.Count; i++)
			{
				Console.WriteLine("Loading script " + packages[i]);
				CPrecacheManager._Instance.PrecachePackage(packages[i], EMemoryPoolID.MP_DEFAULT);
			}


			//DotNetRuntime runtime = new DotNetRuntime();

			//bool success;
			//VvlLoader.Load("scripts:/interop/Core.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/interop/Runtime.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/interop/DebugLink.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/interop/VisualScript.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/interop/game.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/interop/DotNetAttributes.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/common.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/ui.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/behaviorHandlers.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/common_script_vs.vvl", runtime, out success);
			//VvlLoader.Load("scripts:/Characters_script_vs.vvl", runtime, out success);

			//VvlLoader.Load("scripts:/ChopChop_script.vvl", runtime, out success);

			DllExportManager.ExportAllVvls(outputDir);
		}
	}
}