using ImGuiNET;
using igLibrary.Core;
using igLibrary;

namespace igCauldron3
{
	/// <summary>
	/// UI Frame for viewing files to load
	/// </summary>
	public sealed class ArchiveFrame : Frame
	{
		private bool _isChoosingArchive = false;
		private string?[]? _allowedArchives = null;
		private Dictionary<string, igArchive.FileInfo[]> _sortedFileHeaders = new Dictionary<string, igArchive.FileInfo[]>();


		private Dictionary<string, PackageUiData> _packageUiData = new Dictionary<string, PackageUiData>();

		private struct PackageUiData
		{
			public igObjectDirectory _directory;
			public Dictionary<string, igStringRefList> _files;
		}


		/// <summary>
		/// Constructor for the frame
		/// </summary>
		/// <param name="wnd">Reference to the main window object</param>
		public ArchiveFrame(Window wnd) : base(wnd){}


		/// <summary>
		/// Render function for the frame
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("igArchive Manager", ImGuiWindowFlags.HorizontalScrollbar);
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
						if(archives.Any(x => Path.GetFileName(x._path).ToLower() == _allowedArchives[i].ToLower()))
						{
							_allowedArchives[i] = null;
						}
					}
					Array.Sort(_allowedArchives);
				}
				igVector<string> packages = CPrecacheManager._Instance._packagesPerPool[(int)EMemoryPoolID.MP_DEFAULT];
				for(int i = 0; i < packages._count; i++)
				{
					RenderPackage(packages[i]);
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
						igArchive loaded = igFileContext.Singleton.LoadArchive("app:/archives/" + _allowedArchives[i]);
						for(int j = 0; j < loaded._files.Count; j++)
						{
							if(loaded._files[j]._logicalName.EndsWith("_pkg.igz"))
							{
								CPrecacheManager._Instance.PrecachePackage(loaded._files[j]._logicalName, EMemoryPoolID.MP_DEFAULT);
							}
						}
						CDotNetaManager cdnm = CDotNetaManager._Instance;
					}
				}
			}
			ImGui.End();
		}


		/// <summary>
		/// Renders the contents of a package file
		/// </summary>
		/// <param name="packageName">path to a package file</param>
		private void RenderPackage(string packageName)
		{
			if(ImGui.TreeNode(packageName))
			{
				PackageUiData uiData = GetOrCreatePackageUi(packageName);

				foreach(KeyValuePair<string, igStringRefList> kvp in uiData._files)
				{
					if(ImGui.TreeNode(kvp.Key))
					{
						for(int i = 0; i < kvp.Value._count; i++)
						{
							if(ImGui.Button(kvp.Value[i]))
							{
								DirectoryManagerFrame._instance.AddDirectory(igObjectStreamManager.Singleton.Load(kvp.Value[i])!);
							}
						}

						ImGui.TreePop();
					}
				}

				ImGui.TreePop();
			}
		}


		/// <summary>
		/// Gets or creates the ui data for a package
		/// </summary>
		/// <param name="packageName">Patch to a package file</param>
		private PackageUiData GetOrCreatePackageUi(string packageName)
		{
			PackageUiData uiData;

			if(_packageUiData.TryGetValue(packageName, out uiData))
			{
				return uiData;
			}

			uiData._directory = igObjectStreamManager.Singleton.Load(packageName)!;
			uiData._files = new Dictionary<string, igStringRefList>();

			//Only show igObjectDirectories, the rest are hidden
			uiData._files.Add(               "pkg", new igStringRefList());
			uiData._files.Add(    "character_data", new igStringRefList());
			uiData._files.Add(         "actorskin", new igStringRefList());
			//uiData._files.Add(       "havokanimdb", new igStringRefList());
			//uiData._files.Add(    "havokrigidbody", new igStringRefList());
			//uiData._files.Add("havokphysicssystem", new igStringRefList());
			uiData._files.Add(           "texture", new igStringRefList());
			uiData._files.Add(            "effect", new igStringRefList());
			uiData._files.Add(            "shader", new igStringRefList());
			uiData._files.Add(        "motionpath", new igStringRefList());
			uiData._files.Add(          "igx_file", new igStringRefList());
			uiData._files.Add("material_instances", new igStringRefList());
			uiData._files.Add(      "igx_entities", new igStringRefList());
			uiData._files.Add(       "gui_project", new igStringRefList());
			uiData._files.Add(              "font", new igStringRefList());
			uiData._files.Add(         "lang_file", new igStringRefList());
			uiData._files.Add(         "spawnmesh", new igStringRefList());
			uiData._files.Add(             "model", new igStringRefList());
			uiData._files.Add(         "sky_model", new igStringRefList());
			//uiData._files.Add(          "behavior", new igStringRefList());
			uiData._files.Add("graphdata_behavior", new igStringRefList());
			uiData._files.Add(   "events_behavior", new igStringRefList());
			//uiData._files.Add(    "asset_behavior", new igStringRefList());
			//uiData._files.Add(      "hkb_behavior", new igStringRefList());
			//uiData._files.Add(     "hkc_character", new igStringRefList());
			//uiData._files.Add(           "navmesh", new igStringRefList());
			//uiData._files.Add(            "script", new igStringRefList());

			igStringRefList packageContent = (igStringRefList)uiData._directory._objectList[0];

			for(int i = 0; i < packageContent._count; i += 2)
			{
				string type = packageContent[i];
				string file = packageContent[i+1];

				if(!uiData._files.TryGetValue(type, out igStringRefList? output))
				{
					continue;
				}

				output.Append(file);
			}

			igStringRefList emptyPackages = new igStringRefList();
			foreach(KeyValuePair<string, igStringRefList> kvp in uiData._files)
			{
				if(kvp.Value._count == 0)
				{
					emptyPackages.Append(kvp.Key);
				}
			}

			for(int i = 0; i < emptyPackages._count; i++)
			{
				uiData._files.Remove(emptyPackages[i]);
			}

			_packageUiData.Add(packageName, uiData);

			return uiData;
		}


		/// <summary>
		/// Function that renders the contents of an <c>igArchive</c>
		/// </summary>
		/// <param name="archive">The archive to render</param>
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
						DirectoryManagerFrame._instance.AddDirectory(igObjectStreamManager.Singleton.Load(fileHeaders.ElementAt(i)._logicalName)!);
					}
				}
				ImGui.TreePop();
			}
		}
	}
}