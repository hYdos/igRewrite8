using System.Reflection;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class DirectoryManagerFrame : Frame
	{
		private static List<InspectorDrawOverride> _overrides = null!;
		public igObjectDirectoryList _dirs = new igObjectDirectoryList();

		public DirectoryManagerFrame(Window wnd) : base(wnd)
		{
			if(_overrides == null)
			{
				_overrides = new List<InspectorDrawOverride>();
				Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableTo(typeof(InspectorDrawOverride))).ToArray();
				for(uint i = 0; i < types.Length; i++)
				{
					if(!types[i].IsAbstract)
					{
						InspectorDrawOverride drawOverride = (InspectorDrawOverride)Activator.CreateInstance(types[i])!;
						_overrides.Add(drawOverride!);
					}
				}
			}

			_dirs.Append(igObjectStreamManager.Singleton.Load("LooseData/GuiSystemData.igz")!);
		}
		public override void Render()
		{
			ImGui.Begin("Directory Manager");
			RenderDirectory(_dirs[0]);
			ImGui.End();
		}
		private void RenderDirectory(igObjectDirectory dir)
		{
			if(ImGui.TreeNode("Objects"))
			{
				for(int i = 0; i < dir._objectList._count; i++)
				{
					string name;
					if(dir._useNameList) name = dir._nameList![i]._string;
					else                        name = $"Object {i}";

					RenderObject(name, dir._objectList[i]);
				}
			}
		}
		private void RenderObject(string label, igObject obj)
		{
			if(obj == null)
			{
				ImGui.Text($"{label}: null");
				return;
			}

			//TODO: ADD METAFIELD AND METAOBJECT CASES

			igMetaObject meta = obj.GetMeta();
			string objKey = obj.GetHashCode().ToString("X08") + label;
			if(ImGui.TreeNode(objKey, $"{label}: {meta._name}"))
			{
				int overrideIndex = _overrides.FindIndex(x => meta._vTablePointer.IsAssignableTo(x._t));
				if(overrideIndex < 0)
				{
					RenderObjectFields(obj, meta);
				}
				else
				{
					_overrides[overrideIndex].Draw2(this, obj, meta);
				}
				ImGui.TreePop();
			}
		}
		private void RenderObjectFields(igObject obj, igMetaObject meta)
		{
			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				FieldInfo fi = meta._metaFields[i]._fieldHandle!;
				object? raw = fi.GetValue(obj);
				bool changed = FieldRenderer.RenderField(meta._metaFields[i]._fieldName!, ref raw, meta._metaFields[i]);
				if(changed) fi.SetValue(obj, raw);
			}
		}
	}
}