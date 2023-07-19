using ImGuiNET;
using igLibrary.Core;

namespace igCauldron3
{
	public class ArchiveFrame : Frame
	{
		public ArchiveFrame(Window wnd) : base(wnd){}
		public override void Render()
		{
			ImGui.Begin("igArchive Manager");
			if(ImGui.Button("Add Archive"))
			{
			}
			igArchiveList archives = igFileContext.Singleton._archiveManager._archiveList;
			for(int i = 0; i < archives._count; i++)
			{
				RenderArchive(archives[i]);
			}
			ImGui.End();
		}
		private void RenderArchive(igArchive archive)
		{
			if(ImGui.TreeNode(archive._path))
			{
				for(int i = 0; i < archive._fileHeaders.Length; i++)
				{
					string name = archive._fileHeaders[i].name;
					if(!(name.EndsWith(".igz") || name.EndsWith(".lng"))) continue;
					if(ImGui.Button(name))
					{
						ObjectManagerFrame._dirs.Add(igObjectStreamManager.Singleton.Load(archive._fileHeaders[i].name));
					}
				}
				ImGui.TreePop();
			}
		}
	}
}