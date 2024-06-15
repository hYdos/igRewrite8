using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class ObjectPickerFrame : Frame
	{
		private class FilteredDir
		{
			public igObjectDirectory _dir;
			public List<igObject> _objects = new List<igObject>();
			public List<igName> _names = new List<igName>();
			public FilteredDir(igObjectDirectory dir, igMetaObject filter)
			{
				_dir = dir;
				for(int i = 0; i < dir._objectList._count; i++)
				{
					if(filter._vTablePointer.IsAssignableFrom(dir._objectList[i].GetType()))
					{
						_objects.Add(dir._objectList[i]);
						_names.Add(dir._nameList![i]);
					}
				}
			}
		}
		private List<FilteredDir> _orderedDirs;
		private List<igObject> _curDirObjects;
		private List<string> _curDirNames;
		private Action<igObject?> _selectedCb;
		private igMetaObject _metaObject;
		private igObjectDirectory _dir;
		
		public ObjectPickerFrame(Window wnd, igObjectDirectory dir, igMetaObject metaObject, Action<igObject?> selectedCallback) : base(wnd)
		{
			_selectedCb = selectedCallback;
			_metaObject = metaObject;
			_dir = dir;
			IEnumerable<igObjectDirectory> unorderedDirs = igObjectStreamManager.Singleton._directoriesByPath.Values;
			IEnumerable<igObjectDirectory> orderedUnfilteredDirs = unorderedDirs.OrderBy(x => x._name._string);
			_orderedDirs = new List<FilteredDir>(orderedUnfilteredDirs.Count());
			for(int i = 0; i < orderedUnfilteredDirs.Count(); i++)
			{
				FilteredDir filteredDir = new FilteredDir(unorderedDirs.ElementAt(i), metaObject);
				if(filteredDir._objects.Count == 0) continue;
				_orderedDirs.Add(filteredDir);
			}
			List<igObject> traversedObjects = new List<igObject>();
			_curDirObjects = new List<igObject>();
			_curDirNames = new List<string>();
			for(int i = 0; i < dir._objectList._count; i++)
			{
				TraverseNode(dir._objectList[i], dir._nameList![i]._string, traversedObjects);
			}
		}
		private void TraverseNode(igObject? obj, string name, List<igObject> traversed)
		{
			if(obj == null) return;
			if(obj is igMetaObject) return;
			if(obj is igMetaField) return;
			if(obj is igMetaEnum) return;
			if(traversed.Contains(obj)) return;

			if(_metaObject._vTablePointer.IsAssignableFrom(obj.GetType()))
			{
				_curDirObjects.Add(obj);
				_curDirNames.Add(name);
			}
			traversed.Add(obj);


			igMetaObject meta = obj.GetMeta();
			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				if(meta._metaFields[i] is not igObjectRefMetaField) continue;

				if(meta._metaFields[i] is igObjectRefArrayMetaField arrMf)
				{
					//variable naming is my passion
					Array objArrObjs = (Array)meta._metaFields[i]._fieldHandle!.GetValue(obj)!;
					for(int j = 0; j < arrMf._num; j++)
					{
						TraverseNode((igObject?)objArrObjs.GetValue(j), name + "->" + meta._metaFields[i]._fieldName + "[" + i.ToString() + "]", traversed);
					}
				}
				else
				{
					TraverseNode((igObject?)meta._metaFields[i]._fieldHandle!.GetValue(obj)!, name + "->" + meta._metaFields[i]._fieldName, traversed);
				}
			}
		}

		public override void Render()
		{
			ImGui.Begin("Select Object");

			if(ImGui.Button("Use null"))
			{
				_selectedCb.Invoke(null);
				Close();
			}

			string searchTerm = "Search box goes here...";
			ImGui.PushID("search box id");
			bool refreshSearch = ImGui.InputText("", ref searchTerm, 0x100);
			ImGui.PopID();

			if(ImGui.TreeNode("this directory"))
			{
				for(int o = 0; o < _curDirObjects.Count; o++)
				{
					ImGui.PushID(o);
					bool objectSelected = ImGui.Button(_curDirNames[o]);
					ImGui.PopID();
					if(objectSelected)
					{
						_selectedCb.Invoke(_curDirObjects[o]);
						Close();
					}
				}
				ImGui.TreePop();
			}

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
							_selectedCb.Invoke(filteredDir._objects[o]);
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