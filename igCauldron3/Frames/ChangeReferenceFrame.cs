using ImGuiNET;
using System.Reflection;
using igLibrary.Core;

namespace igCauldron3
{
	public class ChangeReferenceFrame : Frame
	{
		private FieldInfo _target;
		public ChangeReferenceFrame(Window wnd, ref object? target, igObjectRefMetaField metadata) : base(wnd){}
		public override void Render()
		{
			ImGui.Begin("Change Object Reference");
			ImGui.Text("Hi");
			ImGui.End();
		}
	}
}