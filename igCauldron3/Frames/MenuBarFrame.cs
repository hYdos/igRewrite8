using ImGuiNET;

namespace igCauldron3
{
	public class MenuBarFrame : Frame
	{
		public MenuBarFrame(Window wnd) : base(wnd){}
		public override void Render()
		{
			if(ImGui.BeginMainMenuBar())
			{
				if(ImGui.BeginMenu("File"))
				{
					if(ImGui.MenuItem("Save As"))
					{
						ObjectManagerFrame._dirs[ObjectManagerFrame._currentDir].WriteFile("test.igz");
					}
					ImGui.EndMenu();
				}
				ImGui.EndMainMenuBar();
			}
		}
	}
}