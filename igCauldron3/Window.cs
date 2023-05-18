using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using igLibrary.Core;

namespace igCauldron3
{
	public class Window : GameWindow
	{
		ImGuiController controller;
		List<Frame> frames = new List<Frame>();
		string[] args;
		public static igObjectDirectory directory = null;

		public Window(GameWindowSettings gws, NativeWindowSettings nws, string[] args) : base(gws, nws)
		{
			this.args = args;
			igFileContext.Singleton.Initialize(args[0]);
			igLibrary.Gfx.igGfx.Initialize();
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);
		}
		protected override void OnLoad()
		{
			base.OnLoad();

			controller = new ImGuiController(ClientSize.X, ClientSize.Y);

			igArchive permanentArc = new igArchive("archives/permanent.pak");
			          permanentArc = new igArchive("archives/shaders_aspenHigh.pak");
			          permanentArc = new igArchive("archives/permanent_aspenHigh.pak");
			igArchive arc = new igArchive(args[1]);
			directory = igObjectStreamManager.Singleton.Load(args[2]);

			frames.Add(new ObjectManagerFrame());
			frames.Add(new InspectorFrame());
			frames.Add(new MenuBarFrame());
		}
		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);
			
			GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
			controller.WindowResized(ClientSize.X, ClientSize.Y);
		}
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (KeyboardState.IsKeyDown(Keys.Escape))
			{
				Close();
			}

			base.OnUpdateFrame(e);
		}
		protected override void OnTextInput(TextInputEventArgs e)
		{
			base.OnTextInput(e);
			controller.PressChar((char)e.Unicode);
		}
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			controller.Update(this, (float)e.Time);

			for(int i = 0; i < frames.Count; i++)
			{
				frames[i].Render();
			}

			controller.Render();

			SwapBuffers();
		}
	}
}