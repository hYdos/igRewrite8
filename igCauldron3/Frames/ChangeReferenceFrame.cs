using ImGuiNET;
using System.Reflection;
using igLibrary.Core;

namespace igCauldron3
{
	public class ChangeReferenceFrame : Frame
	{
		private FieldInfo _target;
		public ChangeReferenceFrame(ref object? target, igObjectRefMetaField metadata)
		{

		}
		public override void Render()
		{
			ImGui.Begin("Change Object Reference");
			ImGui.Text("Hi");
			ImGui.End();
		}
	}
}