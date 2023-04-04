using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

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
	}
}