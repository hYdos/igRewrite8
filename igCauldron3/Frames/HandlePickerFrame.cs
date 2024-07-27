using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class HandlePickerFrame : Frame
	{
		private class FilteredDir
		{
			public igObjectDirectory _dir;
			public List<igObject> _objects = new List<igObject>();
			public List<igName> _names = new List<igName>();
			public FilteredDir(igObjectDirectory dir, igMetaObject filter)
			{
				if(filter == null) throw new ArgumentNullException("Filter cannot be null!");

				_dir = dir;
				for(int i = 0; i < dir._objectList._count; i++)
				{
					//Use metaobject stuff for this cos _vTablePointer isn't guaranteed to be assigned
					if(dir._objectList[i].GetMeta().CanBeAssignedTo(filter))
					{
						_objects.Add(dir._objectList[i]);
						_names.Add(dir._nameList![i]);
					}
				}
			}
		}
		private List<FilteredDir> _orderedDirs;
		private Action<igHandle?> _selectedCb;
		private igMetaObject _metaObject;
		
		public HandlePickerFrame(Window wnd, igMetaObject metaObject, Action<igHandle?> selectedCallback) : base(wnd)
		{
			_selectedCb = selectedCallback;
			_metaObject = metaObject;
			IEnumerable<igObjectDirectory> unorderedDirs = igObjectStreamManager.Singleton._directoriesByPath.Values;
			IEnumerable<igObjectDirectory> orderedUnfilteredDirs = unorderedDirs.OrderBy(x => x._name._string);
			_orderedDirs = new List<FilteredDir>(orderedUnfilteredDirs.Count());
			for(int i = 0; i < orderedUnfilteredDirs.Count(); i++)
			{
				if(orderedUnfilteredDirs.ElementAt(i)._nameList == null) continue;
				FilteredDir filteredDir = new FilteredDir(unorderedDirs.ElementAt(i), metaObject);
				if(filteredDir._objects.Count == 0) continue;
				_orderedDirs.Add(filteredDir);
			}
		}

		public override void Render()
		{
			ImGui.Begin("Select Handle");

			if(ImGui.Button("Use NullHandle"))
			{
				_selectedCb.Invoke(null);
				Close();
			}

			string searchTerm = "Search box goes here...";
			ImGui.PushID("search box id");
			bool refreshSearch = ImGui.InputText("", ref searchTerm, 0x100);
			ImGui.PopID();

			for(int i = 0; i < _orderedDirs.Count; i++)
			{
				FilteredDir filteredDir = _orderedDirs[i];
				igObjectDirectory dir = filteredDir._dir;

				ImGui.PushID(i);
				bool openDir = ImGui.TreeNode(dir._name._string);
				ImGui.PopID();
				if(openDir)
				{
					for(int o = 0; o < filteredDir._objects.Count; o++)
					{
						ImGui.PushID(dir._name._string + filteredDir._names[o]._string);
						bool objectSelected = ImGui.Button(filteredDir._names[o]._string);
						ImGui.PopID();
						if(objectSelected)
						{
							_selectedCb.Invoke(igObjectHandleManager.Singleton._objectToHandleTable[filteredDir._objects[o]]);
							Close();
						}
					}
					ImGui.TreePop();
				}
			}

			ImGui.End();
		}
	}
}