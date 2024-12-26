using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// Renders the ImGui demo window
	/// </summary>
	public sealed class DemoWindowFrame : Frame
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wnd">Window to parent it to</param>
		public DemoWindowFrame(Window wnd) : base(wnd)
		{
		}


		/// <summary>
		/// Renders the UI
		/// </summary>
		public override void Render()
		{
			ImGui.ShowDemoWindow();
		}
	}
}