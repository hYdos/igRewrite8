using System.Collections;
using System.Diagnostics;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class igHashTableOverride : InspectorDrawOverride
	{
		private static Dictionary<IigHashTable, UiData> _uiData = new Dictionary<IigHashTable, UiData>();
		private class UiData
		{
			public List<object> _keys;
			public List<object> _values;
			public bool _stale;
			public List<KeyStatus> _keyStatus;
			public UiData(IigHashTable hashTable)
			{
				_keys = new List<object>(hashTable.GetHashItemCount());
				_values = new List<object>(hashTable.GetHashItemCount());
				_keyStatus = new List<KeyStatus>(hashTable.GetHashItemCount());
				_stale = false;

				IigMemory values = hashTable.GetValues();
				IigMemory keys = hashTable.GetKeys();

				for(int i = 0; i < values.GetCount(); i++)
				{
					object? value = values.GetItem(i);
					object? key = keys.GetItem(i);

					if(!hashTable.IsValidKey(key)) continue;

					_keys.Add(key!);
					_values.Add(value!);
					_keyStatus.Add(KeyStatus.Good);
				}
			}
		}
		[Flags]
		private enum KeyStatus
		{
			Good = 0,
			Duplicate = 1,
			Invalid = 2
		}
		public igHashTableOverride()
		{
			_t = typeof(IigHashTable);
		}
		public override void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta)
		{
			IigHashTable hashTable = (IigHashTable)obj;

			if(!_uiData.TryGetValue(hashTable, out UiData? uiData))
			{
				uiData = new UiData(hashTable);
				_uiData.Add(hashTable, uiData);
			}

			List<object> keys = uiData._keys!;
			List<object> values = uiData._values!;

			igMetaField valuesType = hashTable.GetValueElementType();
			igMetaField keysType = hashTable.GetKeyElementType();

			if(uiData._keyStatus.Any(x => (x & KeyStatus.Duplicate) != 0))
			{
				ImGui.TextColored(new System.Numerics.Vector4(1, 0 , 0, 1), "! HashTable contains duplicate keys and will not be saved until these are resolved !");
			}

			if(uiData._keyStatus.Any(x => (x & KeyStatus.Invalid) != 0))
			{
				ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "! HashTable contains invalid keys and will not be saved until these are resolved !");
			}

			ImGui.BeginTable(id, 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders);
			ImGui.TableSetupColumn("Keys");
			ImGui.TableSetupColumn("Values");
			ImGui.TableHeadersRow();

			ImGui.TableNextRow();
			ImGui.TableSetColumnIndex(0);

			for(int i = 0; i < values.Count; i++)
			{
				object? value = values[i];
				object? key = keys[i];

				FieldRenderer.RenderField($"{id}_keys[{i}]", string.Empty, key, keysType, (newKey) => {
					//reset old values
					for(int j = 0; j < keys.Count; j++)
					{
						if(keys[j].Equals(keys[i]))
						{
							uiData._keyStatus[j] = uiData._keyStatus[j] & ~KeyStatus.Duplicate;
						}
					}
					keys[i] = newKey!;
					//set whether or not the key's invalid
					uiData._keyStatus[i] = (uiData._keyStatus[i] & ~KeyStatus.Invalid) | (hashTable.IsValidKey(newKey) ? KeyStatus.Good : KeyStatus.Invalid);
					bool duplicateFound = false;
					//set new values
					for(int j = 0; j < keys.Count; j++)
					{
						if(i == j) continue;
						if(keys[j].Equals(newKey))
						{
							uiData._keyStatus[j] |= KeyStatus.Duplicate;
							duplicateFound = true;
						}
					}
					if(duplicateFound)
					{
						uiData._keyStatus[i] |= KeyStatus.Duplicate;
					}
					uiData._stale = true;
				});
				ImGui.TableNextColumn();
				FieldRenderer.RenderField($"{id}_values[{i}]", string.Empty, value, valuesType, (newValue) => {
					values[i] = newValue!;
					uiData._stale = true;
				});
				ImGui.TableNextColumn();
			}

			ImGui.EndTable();

			if(uiData._stale && uiData._keyStatus.All(x => x == KeyStatus.Good))
			{
				hashTable.ActivateWithExpectedCount(uiData._keys.Count);
				for(int i = 0; i < uiData._keys.Count; i++)
				{
					hashTable.Insert(uiData._keys[i], uiData._values[i]);
				}
				uiData._stale = false;
			}
		}
	}
}