using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using igLibrary.Core;

namespace igCauldron3
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			ParseArgs(args);
			Window wnd = new Window(
				new GameWindowSettings()
				{
					IsMultiThreaded = false
				},
				new NativeWindowSettings()
				{
					Size = new Vector2i(1280, 720),
					Title = "igCauldron",
					Flags = ContextFlags.ForwardCompatible
				},
				args
			);

			wnd.Run();
		}
		private static void ParseArgs(string[] args)
		{
			igRegistry registry = igRegistry.GetRegistry();
			
			for(int i = 0; i < args.Length;)
			{
				switch(args[i].ToLower())
				{
					case "-p":
						i++;
						switch(args[i].ToLower())
						{
							case "ps3":
							case "playstation3":
								registry._platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3;
								break;
							case "aspen":
							case "aspen32":
							case "aspenlow":
							case "ios32":
							case "iosold":
							case "ioslow":
								registry._platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN;
								break;
							case "aspen64":
							case "aspenhigh":
							case "ios64":
							case "iosnew":
							case "ioshigh":
								registry._platform = IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64;
								break;
						}
						i++;
						break;
					case "-c":
						i++;
						igFileContext.Singleton.Initialize(args[i]);
						i++;
						break;
					case "-u":
						i++;
						igFileContext.Singleton.InitializeUpdate(args[i]);
						i++;
						break;
					case "-a":
						i++;
						igArchive arc = new igArchive(args[i]);
						i++;
						break;
					case "-f":
						i++;
						if(Window.targetIgz != null) throw new ArgumentException("Multiple files specified");
						Window.targetIgz = args[i];
						i++;
						break;
				}
			}
		}

		private static void ExtractSLIArchive(string[] args)
		{
			igFileContext.Singleton.Initialize(args[0]);
			igArchive arc = new igArchive(args[1]);
			for(int i = 0; i < arc.fileHeaders.Length; i++)
			{
				string path = Path.Combine("F:/SLI", arc.fileHeaders[i].fullName);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				FileStream fs = File.Create(path);
				arc.ExtractFile(i, fs);
				fs.Close();
			}
		}
	}
}