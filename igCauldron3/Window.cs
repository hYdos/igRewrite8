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
			igAlchemyCore.InitializeSystems();
			igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);
		}
		protected override void OnLoad()
		{
			base.OnLoad();

			controller = new ImGuiController(ClientSize.X, ClientSize.Y);

			igArchive permanentArc = new igArchive("archives/permanent.pak");
			          permanentArc = new igArchive("archives/gamestartup.pak");
			          permanentArc = new igArchive("archives/shaders_ps3.pak");
			          permanentArc = new igArchive("archives/permanent_ps3.pak");
			igArchive arc = new igArchive(args[1]);

			igObjectLoader scriptLoader = igObjectLoader._loaders["DotNet"];
			scriptLoader.ReadFile("scripts:/interop/Runtime.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/interop/Core.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/interop/DotNetAttributes.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/interop/game.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/interop/VisualScript.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/interop/DebugLink.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/common.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/ui.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/behaviorHandlers.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/common_script_vs.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/Characters_script_vs.vvl", igBlockingType.kMayBlock);
			//scriptLoader.ReadFile("scripts:/ChopChop_script.vvl", igBlockingType.kMayBlock);
			scriptLoader.ReadFile("scripts:/Legs_Template_script.vvl", igBlockingType.kMayBlock);

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