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

		public Window(GameWindowSettings gws, NativeWindowSettings nws, string[] args) : base(gws, nws)
		{
			this.args = args;
			igFileContext.Singleton.Initialize(args[0]);
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);
		}
		protected override void OnLoad()
		{
			base.OnLoad();

			controller = new ImGuiController(ClientSize.X, ClientSize.Y);

			frames.Add(new ObjectManagerFrame());
			((ObjectManagerFrame)frames[0]).Initialize(args);
			frames.Add(new InspectorFrame());
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