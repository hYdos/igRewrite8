using System.Diagnostics;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class igHashTableOverride : InspectorDrawOverride
	{
		public igHashTableOverride()
		{
			_t = typeof(IigHashTable);
		}
		public override void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta)
		{
			IigHashTable hashTable = (IigHashTable)obj;
			IigMemory values = hashTable.GetValues();
			IigMemory keys = hashTable.GetKeys();

			igMetaField valuesType = hashTable.GetValueElementType();
			igMetaField keysType = hashTable.GetKeyElementType();

 			int processed = 0;

			ImGui.Columns(2);
			ImGui.Text("Keys");
			ImGui.NextColumn();
			ImGui.Text("Values");
			ImGui.NextColumn();

			for(int i = 0; i < values.GetCount(); i++)
			{
				object? value = values.GetItem(i);
				object? key = keys.GetItem(i);

				if(!hashTable.IsValidKey(key)) continue;

				FieldRenderer.RenderField($"{id}_keys[{processed}]", string.Empty, key, keysType, (something) => {});	
				ImGui.NextColumn();
				FieldRenderer.RenderField($"{id}_values[{processed}]", string.Empty, value, valuesType, (something) => { values.SetItem(i, something); });
				ImGui.NextColumn();

				processed++;
			}

			ImGui.Columns();

			Debug.Assert(processed == hashTable.GetHashItemCount());
		}
	}
}