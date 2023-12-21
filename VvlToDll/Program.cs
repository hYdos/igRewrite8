using igLibrary;
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
			igAlchemyCore.InitializeSystems();
			igFileContext.Singleton.Initialize(args[0]);

			//CRenderBase<CRender>::initializeEarly
			CPrecacheManager._Instance.PrecachePackage("generated/shaders/shaders_ps3", EMemoryPoolID.MP_DEFAULT);

			//CClient::loadGameStartupPackages
			//CPrecacheManager._Instance.PrecachePackage("data:/archives/languagestartup", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent_ps3", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/essentialui", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/legal", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/gamestartup", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanentdeveloper", EMemoryPoolID.MP_DEFAULT);

			//CClient::loadDeferredPackages
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/maps/zoneinfos", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent_2015", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_Mobile", EMemoryPoolID.MP_DEFAULT);
			CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_FrontEnd", EMemoryPoolID.MP_DEFAULT);

			CPrecacheManager._Instance.PrecachePackage("generated/characters/ChopChop", EMemoryPoolID.MP_DEFAULT);

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

			DllExportManager.ExportAllVvls();
		}
	}
}