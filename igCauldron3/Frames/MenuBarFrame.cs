using ImGuiNET;

namespace igCauldron3
{
	public class MenuBarFrame : Frame
	{
		public override void Render()
		{
			if(ImGui.BeginMainMenuBar())
			{
				if(ImGui.BeginMenu("File"))
				{
					if(ImGui.MenuItem("Save As"))
					{
						Window.directory.WriteFile("test.igz");
					}
					ImGui.EndMenu();
				}
				ImGui.EndMainMenuBar();
			}
		}
	}
}