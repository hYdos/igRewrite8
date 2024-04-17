using System.Runtime.Serialization;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public sealed class DirectoryCreatorFrame : Frame
	{
		private string _path = "";
		public DirectoryCreatorFrame(Window wnd) : base(wnd){}

		public override void Render()
		{
			ImGui.Begin("New Directory");
			ImGui.Text("Path");
			ImGui.SameLine();
			ImGui.InputText(string.Empty, ref _path, 0x100);
			if(ImGui.Button("Create"))
			{
				igObjectDirectory newDir = new igObjectDirectory(_path, new igName(Path.GetFileNameWithoutExtension(_path)));
				newDir._nameList = new igNameList();
				newDir._useNameList = true;
				igObjectStreamManager.Singleton.AddObjectDirectory(newDir, _path);
				ObjectManagerFrame._dirs.Add(newDir);
				_wnd.frames.Remove(this);
			}
			ImGui.End();
		}
	}
}