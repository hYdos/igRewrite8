using ImGuiNET;

namespace igCauldron3
{
	public sealed class DemoWindowFrame : Frame
	{
		public DemoWindowFrame(Window wnd) : base(wnd)
		{
		}

		public override void Render()
		{
			ImGui.ShowDemoWindow();
		}
	}
}