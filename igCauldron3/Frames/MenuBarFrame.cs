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
					if(ImGui.MenuItem("Save"))
					{
						MemoryStream ms = new MemoryStream();
						igObjectDirectory target = ObjectManagerFrame._dirs[ObjectManagerFrame._currentDir];
						target.WriteFile(ms, igRegistry.GetRegistry()._platform);
						ms.Seek(0, SeekOrigin.Begin);
#if DEBUG
						FileStream fs = File.Create("test.igz");
						ms.CopyTo(fs);
						fs.Close();
						ms.Seek(0, SeekOrigin.Begin);
#endif
						igFilePath fp = new igFilePath();
						fp.Set(target._path);
						igArchive arc = igFileContext.Singleton._archiveManager._patchArchives._count > 0 ? igFileContext.Singleton._archiveManager._patchArchives[0] : (igArchive)target._fd._device;
						arc.GetAddFile(fp._path);
						arc.Compress(fp._path, ms);
						ms.Close();
						if(arc._path[1] == ':') arc.Save(arc._path);
						else arc.Save($"{igFileContext.Singleton._root}/archives/{Path.GetFileName(arc._path)}");
					}
					if(ImGui.MenuItem("New IGZ"))
					{
						_wnd.frames.Add(new DirectoryCreatorFrame(_wnd));
					}
					ImGui.EndMenu();
				}
				ImGui.EndMainMenuBar();
			}
		}
	}
}