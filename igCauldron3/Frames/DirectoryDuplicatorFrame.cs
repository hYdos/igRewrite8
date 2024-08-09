using System.Runtime.Serialization;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI Frame for duplicating an igObjectDirectory
	/// </summary>
	public sealed class DirectoryDuplicatorFrame : Frame
	{
		private string _path = "";
		private igObjectDirectory _source;


		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		/// <param name="source">the <c>igObjectDirectory</c> to clone</param>
		public DirectoryDuplicatorFrame(Window wnd, igObjectDirectory source) : base(wnd)
		{
			_source = source;
		}


		/// <summary>
		/// Render function for the frame
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("Copy Directory", ImGuiWindowFlags.NoDocking);

			//Render path field and check for errors
			ImGui.Text("Path");
			ImGui.SameLine();
			bool pathErrored = string.IsNullOrWhiteSpace(_path) || _path.Contains(' ');
			if(pathErrored) ImGui.PushStyleColor(ImGuiCol.FrameBg, Styles._errorBg);
			ImGui.InputText(string.Empty, ref _path, 0x100);
			if(pathErrored) ImGui.PopStyleColor();

			if(ImGui.Button("Copy") && !pathErrored)
			{
				// Duplicating igObjectDirectories is a bit hellish by virtue of the fact that they're actually literally
				// just objects when loaded, it's probably best to just save it to a file and then reload that file

				// Ideally this'd be handled with igFileContext's ram storage device but I've not implemented that yet
				string tempPath = Path.Combine(Path.GetTempPath(), "igz_dupe.igz");
				FileStream tempFs = File.Create(tempPath);

				_source.WriteFile(tempFs, igRegistry.GetRegistry()._platform);

				// Close it so igFileContext can access it
				tempFs.Close();

				igObjectDirectory duplicate = igObjectStreamManager.Singleton.Load(tempPath, new igName(Path.GetFileNameWithoutExtension(_path)))!;
				duplicate._path = _path;

				igFileContext.Singleton.Close(duplicate._fd, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);

				File.Delete(tempPath);

				DirectoryManagerFrame._instance.AddDirectory(duplicate);

				Close();
			}
			if(ImGui.Button("Close")) Close();
			ImGui.End();
		}
	}
}