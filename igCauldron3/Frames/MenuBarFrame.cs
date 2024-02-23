using ImGuiNET;
using igCauldron3.Utils;
using igLibrary.Core;

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
						MemoryStream ms = new MemoryStream();
						igObjectDirectory target = ObjectManagerFrame._dirs[ObjectManagerFrame._currentDir];
						if(target._fd._device is igArchive arc)
						{
							target.WriteFile(ms, igRegistry.GetRegistry()._platform);
							ms.Seek(0, SeekOrigin.Begin);
							FileStream fs = File.Create("test.igz");
							ms.CopyTo(fs);
							fs.Close();
							ms.Seek(0, SeekOrigin.Begin);
							igFilePath fp = new igFilePath();
							fp.Set(target._path);
							arc.Compress(fp._path, ms);
							ms.Close();
							arc.Save($"{igFileContext.Singleton._root}/archives/{Path.GetFileName(arc._path)}");
						}
					}
					ImGui.EndMenu();
				}
				ImGui.EndMainMenuBar();
			}
		}
	}
}