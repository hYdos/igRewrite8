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
			//PackagePrecacher.PrecachePackage("generated/shaders/shaders_ps3");

			//CClient::loadGameStartupPackages
			//PackagePrecacher.PrecachePackage("data:/archives/languagestartup");
			PackagePrecacher.PrecachePackage("generated/packageXmls/permanent_ps3");
			PackagePrecacher.PrecachePackage("generated/packageXmls/essentialui");
			PackagePrecacher.PrecachePackage("generated/UI/legal");
			PackagePrecacher.PrecachePackage("generated/packageXmls/gamestartup");
			PackagePrecacher.PrecachePackage("generated/packageXmls/permanentdeveloper");

			//CClient::loadDeferredPackages
			PackagePrecacher.PrecachePackage("generated/packageXmls/permanent");
			PackagePrecacher.PrecachePackage("generated/maps/zoneinfos");
			PackagePrecacher.PrecachePackage("generated/packageXmls/permanent_2015");
			PackagePrecacher.PrecachePackage("generated/UI/Domains/JuiceDomain_Mobile");
			PackagePrecacher.PrecachePackage("generated/UI/Domains/JuiceDomain_FrontEnd");

			PackagePrecacher.PrecachePackage("generated/characters/ChopChop");

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