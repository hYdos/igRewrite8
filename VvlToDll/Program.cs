/*
	Copyright (c) 2022-2025, The VvlToDll Contributors.
	VvlToDll and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


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

VvlToDll -gg <base game folder> -up <update.pak> -od <output directory> -p <platform> -ge <game enum> -- <...packages>
- base game folder: folder containing archives folder
- update.pak: self explanatory
- output directory: where the dlls get dumped
- platform: IG_CORE_PLATFORM name
= game enum: the game to load, look in the Resources/ArkCore folder for a list of values
- packages: game paths to package, can be written as ""generated/maps/UI/MainMenuBackground/MainMenuBackground"",
  can find these by looking in the packages folder of an archive.
");
				return;
			}

			string? gamePath = null;
			string? updatePath = null;
			string? outputDir = null;
			IG_CORE_PLATFORM platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT;
			igArkCore.EGame game = igArkCore.EGame.EV_None;
			List<string> packages = new List<string>();
			for(int i = 0; i < args.Length; i++)
			{
				string flag = args[i];
				switch(flag)
				{
					case "-gp":
						gamePath = args[++i];
						break;
					case "-up":
						updatePath = args[++i];
						break;
					case "-od":
						outputDir = args[++i];
						break;
					case "-p":
						platform = Enum.Parse<IG_CORE_PLATFORM>(args[++i]);
						break;
					case "-ge":
						game = Enum.Parse<igArkCore.EGame>(args[++i]);
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
			if (game == igArkCore.EGame.EV_None)
			{
				Console.WriteLine("Missing game enum");
				return;
			}

			igRegistry.GetRegistry()._platform = platform;
			igRegistry.GetRegistry()._gfxPlatform = igGfx.GetGfxPlatformFromCore(platform);
			igArkCore.ReadFromXmlFile(game);
			ArkDllExport.Create(outputDir);
			igAlchemyCore.InitializeSystems();
			igFileContext.Singleton.Initialize(gamePath);
			igFileContext.Singleton.InitializeUpdate(updatePath);

			VvlPackagePrecacher._Instance.Intialize();

			//CClient::loadGameStartupPackages
			//CPrecacheManager._Instance.PrecachePackage("data:/archives/languagestartup", EMemoryPoolID.MP_DEFAULT);
			VvlPackagePrecacher._Instance.PrecachePackage($"generated/packageXmls/permanent_{igAlchemyCore.GetPlatformString(platform)}");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/packageXmls/essentialui");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/UI/legal");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/packageXmls/gamestartup");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/packageXmls/permanentdeveloper");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/SoundBankData");

			//CClient::loadDeferredPackages
			VvlPackagePrecacher._Instance.PrecachePackage("generated/packageXmls/permanent");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/maps/zoneinfos");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/packageXmls/permanent_2015");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_Mobile");
			VvlPackagePrecacher._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_FrontEnd");

			for(int i = 0; i < packages.Count; i++)
			{
				Console.WriteLine("Loading script " + packages[i]);
				VvlPackagePrecacher._Instance.PrecachePackage(packages[i]);
			}

			DllExportManager.ExportAllVvls(outputDir);
		}
	}
}