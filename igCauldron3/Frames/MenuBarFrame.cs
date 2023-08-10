using ImGuiNET;
using igCauldron3.Utils;

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
						string savePath = CrossFileDialog.SaveFile("Save IGZ", ".igz;.lng");
						if(!string.IsNullOrWhiteSpace(savePath))
						{
							ObjectManagerFrame._dirs[ObjectManagerFrame._currentDir].WriteFile(savePath);
						}
					}
					ImGui.EndMenu();
				}
				ImGui.EndMainMenuBar();
			}
		}
	}
}