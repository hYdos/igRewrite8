using ImGuiNET;

namespace igCauldron3
{
	public class InspectorFrame : Frame
	{
		public override void Render()
		{
			ImGui.Begin("Inspector");
			ImGui.Text("Hi");
			ImGui.End();
		}
	}
}