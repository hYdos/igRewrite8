using ImGuiNET;
using igLibrary.Core;

namespace igCauldron3
{
	public class ArchiveFrame : Frame
	{
		private bool _isChoosingArchive = false;
		private string?[]? _allowedArchives = null;
		private Dictionary<string, igArchive.IG_CORE_ARCHIVE_FILE_HEADER[]> _sortedFileHeaders = new Dictionary<string, igArchive.IG_CORE_ARCHIVE_FILE_HEADER[]>();
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
						if(archives.Any(x => x._name.ToLower() == _allowedArchives[i].ToLower()))
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
						igArchive loaded = igFileContext.Singleton.LoadArchive("archives/" + _allowedArchives[i]);
						for(int j = 0; j < loaded._fileHeaders.Length; j++)
						{
							if(loaded._fileHeaders[j].name.EndsWith("_pkg.igz"))
							{
								PackagePrecacher.PrecachePackage(loaded._fileHeaders[j].name);
							}
						}
					}
				}
			}
			ImGui.End();
		}
		private void RenderArchive(igArchive archive)
		{
			igArchive.IG_CORE_ARCHIVE_FILE_HEADER[]? fileHeaders;
			if(!_sortedFileHeaders.TryGetValue(archive._name, out fileHeaders))
			{
				fileHeaders = archive._fileHeaders.OrderBy(x => x.name).ToArray();
				_sortedFileHeaders.Add(archive._name, fileHeaders);
			}
			if(ImGui.TreeNode(archive._path))
			{
				for(int i = 0; i < fileHeaders.Count(); i++)
				{
					string name = fileHeaders.ElementAt(i).name;
					if(!(name.EndsWith(".igz") || name.EndsWith(".lng"))) continue;
					if(ImGui.Button(name))
					{
						ObjectManagerFrame._dirs.Add(igObjectStreamManager.Singleton.Load(fileHeaders.ElementAt(i).name));
					}
				}
				ImGui.TreePop();
			}
		}
	}
}