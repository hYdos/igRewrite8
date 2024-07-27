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
			ImGui.Begin("New Directory", ImGuiWindowFlags.NoDocking);

			ImGui.Text("Path");
			ImGui.SameLine();
			bool pathErrored = string.IsNullOrWhiteSpace(_path) || _path.Contains(' ');
			if(pathErrored) ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles._errorBg);
			ImGui.InputText(string.Empty, ref _path, 0x100);
			if(pathErrored) ImGui.PopStyleColor();

			if(ImGui.Button("Create") && !pathErrored)
			{
				igObjectDirectory newDir = new igObjectDirectory(_path, new igName(Path.GetFileNameWithoutExtension(_path)));
				newDir._nameList = new igNameList();
				newDir._useNameList = true;
				igObjectStreamManager.Singleton.AddObjectDirectory(newDir, _path);
				DirectoryManagerFrame._instance.AddDirectory(newDir);
				Close();
			}
			if(ImGui.Button("Close")) Close();
			ImGui.End();
		}
	}
}