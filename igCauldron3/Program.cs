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