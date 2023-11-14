using ImGuiNET;
using igLibrary.Core;
using igLibrary;

namespace igCauldron3
{
	public class ArchiveFrame : Frame
	{
		private bool _isChoosingArchive = false;
		private string?[]? _allowedArchives = null;
		private Dictionary<string, igArchive.FileInfo[]> _sortedFileHeaders = new Dictionary<string, igArchive.FileInfo[]>();
		public ArchiveFrame(Window wnd) : base(wnd){}
		public override void Render()
		{
			ImGui.Begin("igArchive Manager");
			if(!_isChoosingArchive)
			{
				igArchiveList archives = igFileContext.Singleton._archiveManager._archiveList;
				if(ImGui.Button("Add Archive"))
				{
					_isChoosingArchive = true;
					_allowedArchives = Directory.GetFiles(Path.Combine(igFileContext.Singleton._root, "archives"), "*.pak");
					for(int i = 0; i < _allowedArchives.Length; i++)
					{
						_allowedArchives[i] = Path.GetFileName(_allowedArchives[i]);
						if(archives.Any(x => x._path.ToLower() == _allowedArchives[i].ToLower()))
						{
							_allowedArchives[i] = null;
						}
					}
				}
				for(int i = 0; i < archives._count; i++)
				{
					RenderArchive(archives[i]);
				}
			}
			else
			{
				if(_allowedArchives == null) _isChoosingArchive = false;
				if(ImGui.Button("Cancel"))
				{
					_isChoosingArchive = false;
				}
				for(int i = 0; i < _allowedArchives.Length; i++)
				{
					if(_allowedArchives[i] == null) continue;
					if(ImGui.Button(_allowedArchives[i]))
					{
						_isChoosingArchive = false;
						igArchive loaded = igFileContext.Singleton.LoadArchive("data:/archives/" + _allowedArchives[i]);
						for(int j = 0; j < loaded._files.Count; j++)
						{
							if(loaded._files[j]._logicalName.EndsWith("_pkg.igz"))
							{
								PackagePrecacher.PrecachePackage(loaded._files[j]._logicalName);
							}
						}
						CDotNetaManager cdnm = CDotNetaManager._Instance;
					}
				}
			}
			ImGui.End();
		}
		private void RenderArchive(igArchive archive)
		{
			igArchive.FileInfo[]? fileHeaders;
			if(!_sortedFileHeaders.TryGetValue(archive._path, out fileHeaders))
			{
				fileHeaders = archive._files.OrderBy(x => x._logicalName).ToArray();
				_sortedFileHeaders.Add(archive._path, fileHeaders);
			}
			if(ImGui.TreeNode(archive._path))
			{
				for(int i = 0; i < fileHeaders.Count(); i++)
				{
					string name = fileHeaders.ElementAt(i)._logicalName;
					if(!(name.EndsWith(".igz") || name.EndsWith(".lng"))) continue;
					if(ImGui.Button(name))
					{
						ObjectManagerFrame._dirs.Add(igObjectStreamManager.Singleton.Load(fileHeaders.ElementAt(i)._logicalName));
					}
				}
				ImGui.TreePop();
			}
		}
	}
}