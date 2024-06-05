using System.Drawing.Design;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class HandlePickerFrame : Frame
	{
		private List<igObjectDirectory> _orderedDirs;
		private Action<igHandle?> _selectedCb;
		
		public HandlePickerFrame(Window wnd, Action<igHandle?> selectedCallback) : base(wnd)
		{
			IEnumerable<igObjectDirectory> unorderedDirs = igObjectStreamManager.Singleton._directoriesByPath.Values;
			_orderedDirs = unorderedDirs.OrderBy(x => x._name._string).ToList();
			_selectedCb = selectedCallback;
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
				igObjectDirectory dir = _orderedDirs[i];

				ImGui.PushID(i);
				bool openDir = ImGui.TreeNode(dir._name._string);
				ImGui.PopID();
				if(openDir)
				{
					for(int o = 0; o < dir._objectList._count; o++)
					{
						ImGui.PushID(dir._name._string + dir._nameList![o]._string);
						bool objectSelected = ImGui.Button(dir._nameList![o]._string);
						ImGui.PopID();
						if(objectSelected)
						{
							_selectedCb.Invoke(igObjectHandleManager.Singleton._objectToHandleTable[dir._objectList[o]]);
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