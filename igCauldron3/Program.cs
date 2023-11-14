using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using igLibrary.Core;

namespace igCauldron3
{
	public static class Program
	{
		[STAThread] 
		public static void Main(string[] args)
		{
			Window wnd = new Window(
				new GameWindowSettings()
				{
					IsMultiThreaded = false,
					RenderFrequency = 60,
					UpdateFrequency = 60,
				},
				new NativeWindowSettings()
				{
					Size = new Vector2i(1280, 720),
					Title = "igCauldron",
					Flags = ContextFlags.ForwardCompatible
				},
				args
			);

#if !DEBUG
			try
			{
				wnd.Run();
			}
			catch(Exception e)
			{
				System.Windows.Forms.MessageBox.Show($"{e.Message}\".\n\n{e.StackTrace}", $"Encountered \"{e.GetType().Name}\"", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
#else
			wnd.Run();
#endif
		}
		private static void ExtractSLIArchive(string[] args)
		{
			igFileContext.Singleton.Initialize(args[0]);
			igArchive arc = new igArchive();
			arc.Open(args[1], igBlockingType.kMayBlock);
			for(int i = 0; i < arc._files.Count; i++)
			{
				string path = Path.Combine("F:/SLI", arc._files[i]._name);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				FileStream fs = File.Create(path);
				arc.Decompress(arc._files[i], fs);
				fs.Close();
			}
		}
	}
}