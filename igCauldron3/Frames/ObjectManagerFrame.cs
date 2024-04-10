using System.Reflection;
using System.Linq;
using igLibrary.Core;
using ImGuiNET;

using igLibrary.Math;

namespace igCauldron3
{
	public class ObjectManagerFrame : Frame
	{
		private static List<InspectorDrawOverride>? overrides;
		public static List<igObjectDirectory> _dirs = new List<igObjectDirectory>();
		private bool renderChangeReference = false;
		public static int _currentDir;

		public ObjectManagerFrame(Window wnd) : base(wnd)
		{
			if(overrides == null)
			{
				overrides = new List<InspectorDrawOverride>();
				Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableTo(typeof(InspectorDrawOverride))).ToArray();
				for(uint i = 0; i < types.Length; i++)
				{
					if(!types[i].IsAbstract)
					{
						InspectorDrawOverride drawOverride = (InspectorDrawOverride)Activator.CreateInstance(types[i]);
						overrides.Add(drawOverride);
					}
				}
			}
		}

		public override void Render()
		{
			ImGui.Begin("Object Manager");
			if(ImGui.BeginTabBar("Object Directories"))
			{
				for(int i = 0; i < _dirs.Count; i++)
				{
					if(ImGui.BeginTabItem(_dirs[i]._name._string))
					{
						_currentDir = i;
						RenderDir(_dirs[i]);
						ImGui.EndTabItem();
					}
				}
				ImGui.EndTabBar();
			}
			ImGui.End();
		}
		public void RenderDir(igObjectDirectory dir)
		{
			igObjectList list = dir._objectList;
			if(ImGui.TreeNode("Dependancies"))
			{
				for(int i = 0; i < dir._dependancies.Count; i++)
				{
					ImGui.Text(dir._dependancies[i]._path);					
				}
				ImGui.TreePop();
			}
			if(ImGui.TreeNode("Objects"))
			{
				for(int i = 0; i < dir._objectList._count; i++)
				{
					string name;
					if(dir._useNameList) name = dir._nameList[i]._string;
					else                       name = $"Object {i}";

					RenderObject(name, dir._objectList[i]);
				}
				if(ImGui.Button("+"))
				{
					_wnd.frames.Add(new CreateObjectFrame(_wnd, _dirs[_currentDir], igArkCore.GetObjectMeta("igObject")));
				}
				ImGui.TreePop();
			}
			if(renderChangeReference)
			{
				RenderObjectReferenceWindow();
			}
		}
		private void RenderObject(string label, igObject? obj)
		{
			if(obj == null)
			{
				ImGui.Text($"{label}: null");
				return;
			}
			else if(obj is igMetaField metaField)
			{
				ImGui.Text($"{label}: metafield.{metaField._parentMeta._name}::{metaField._name}");
				return;
			}
			else if(obj is igMetaObject metaObject)
			{
				ImGui.Text($"{label}: metaobject.{metaObject._name}");
				return;
			}
			igMetaObject meta = obj.GetMeta();
			string objKey = obj.GetHashCode().ToString("X08");
			if(ImGui.TreeNode(objKey, $"{label}: {meta._name}"))
			{
				int i = 0;

				int overrideIndex = overrides.FindIndex(x => meta._vTablePointer.IsAssignableTo(x._t));
				if(overrideIndex < 0)
				{
					RenderObjectFields(obj, meta, i);
				}
				else
				{
					overrides[overrideIndex].Draw(this, obj, meta);
				}
				
				ImGui.TreePop();
			}
			if(ImGui.BeginPopupContextItem(objKey))
			{
				if(ImGui.Selectable("Change Reference"))
				{
					renderChangeReference = true;
				}
				ImGui.EndPopup();
			}
		}
		private void RenderObjectReferenceWindow()
		{
			ImGui.BeginMenu("Change Object Reference");
			ImGui.Text("UwU");
			ImGui.EndMenu();
		}
		public void RenderObjectFields(igObject obj, igMetaObject meta, int i = 0)
		{
			for(; i < meta._metaFields.Count; i++)
			{
				//if(meta._metaFields[i]._properties._persistent)
				{
					RenderFieldWithName(obj, meta._metaFields[i]);
				}
			}
		}
		public void RenderFieldWithName(object obj, igMetaField field)
		{
			Type t = obj.GetType();
			FieldInfo? fi = t.GetField(field._name);
			bool canRender = fi != null;

			if(canRender)
			{
				object? value = fi.GetValue(obj);
				RenderArrayField(field._name, ref value, field);
				fi.SetValue(obj, value);
			}
		}
		public void RenderArrayField(string label, ref object? value, igMetaField field)
		{
			FieldInfo? fi = field.GetType().GetField("_num");
			if(fi != null)
			{
				short num = (short)fi.GetValue(field);
				Array values = (Array)value;
				for(int i = 0; i < num; i++)
				{
					object? currValue = values.GetValue(i);
					RenderField($"{i}_{label}", ref currValue, field);
					values.SetValue(currValue, i);
				}
			}
			else
			{
				RenderField(label, ref value, field);
			}

		}
		public void RenderField(string label, ref object? value, igMetaField field)
		{
			if(field is igIntMetaField)
			{
				int intValue = (int)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputInt(string.Empty, ref intValue);
				ImGui.PopID();
				value = (int)Math.Clamp(intValue, int.MinValue, int.MaxValue);
			}
			else if(field is igUnsignedShortMetaField)
			{
				int intValue = (ushort)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputInt(string.Empty, ref intValue);
				ImGui.PopID();
				value = (ushort)Math.Clamp(intValue, ushort.MinValue, ushort.MaxValue);
			}
			else if(field is igShortMetaField)
			{
				int intValue = (short)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputInt(string.Empty, ref intValue);
				ImGui.PopID();
				value = (short)Math.Clamp(intValue, short.MinValue, short.MaxValue);
			}
			else if(field is igUnsignedLongMetaField || field is igSizeTypeMetaField)
			{
				ulong ulongValue = (ulong)value;
				string strValue = ulongValue.ToString();
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputText(string.Empty, ref strValue, (uint)short.MaxValue);
				ImGui.PopID();
				if(ulong.TryParse(strValue, out ulong temp))
				{
					value = temp;
				}

			}
			else if(field is igLongMetaField)
			{
				long longValue = (long)value;
				string strValue = longValue.ToString();
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputText(string.Empty, ref strValue, (uint)short.MaxValue);
				ImGui.PopID();
				if(long.TryParse(strValue, out long temp))
				{
					value = temp;
				}
			}
			else if(field is igStringMetaField)
			{
				string strValue = (string)value;
				if(strValue == null) strValue = "";
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputText(string.Empty, ref strValue, (uint)short.MaxValue);
				ImGui.PopID();
				if(value != null || strValue.Length > 1)
				{
					value = strValue;
				}
			}
			if(field is igFloatMetaField)
			{
				float floatValue = (float)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputFloat(string.Empty, ref floatValue);
				ImGui.PopID();
				value = floatValue;
			}
			else if(field is igUnsignedCharMetaField)
			{
				int byteValue = (byte)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputInt(string.Empty, ref byteValue);
				ImGui.PopID();
				value = (byte)Math.Clamp(byteValue, byte.MinValue, byte.MaxValue);
			}
			else if(field is igCharMetaField)
			{
				int byteValue = (sbyte)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputInt(string.Empty, ref byteValue);
				ImGui.PopID();
				value = (sbyte)Math.Clamp(byteValue, sbyte.MinValue, sbyte.MaxValue);
			}
			else if(field is igBoolMetaField)
			{
				bool boolValue = (bool)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.Checkbox(string.Empty, ref boolValue);
				ImGui.PopID();
				value = boolValue;
			}
			else if(field is igMemoryRefMetaField || field is igMemoryRefHandleMetaField || field is igVectorMetaField)
			{
				if(ImGui.TreeNode(label))
				{
					IigMemory memValue = null;
					igMetaField memType = null;
					if(field is igMemoryRefMetaField memoryRefMetaField)
					{
						memValue = (IigMemory)value;
						memType = memoryRefMetaField._memType;
					}
					else if(field is igMemoryRefHandleMetaField memoryRefHandleMetaField)
					{
						memValue = (IigMemory)value;
						memType = memoryRefHandleMetaField._memType;
					}
					else if(field is igVectorMetaField vectorMetaField)
					{
						igVectorCommon vector = (igVectorCommon)value;
						memValue = vector.GetData();
						memType = vectorMetaField.GetTemplateParameter(0);
					}

					Array data = memValue.GetData();
					if(data != null)
					{
						for(int i = 0; i < data.Length; i++)
						{
							object? arrValue = data.GetValue(i);
							RenderArrayField($"Element {i}", ref arrValue, memType);
							data.SetValue(arrValue, i);
						}
					}
					ImGui.TreePop();
				}
			}
			else if(field is igObjectRefMetaField)
			{
				RenderObject(label, (igObject?)value);
			}
			else if(field is igHandleMetaField hndMetaField)
			{
				igHandle hnd = (igHandle)value;
				ImGui.Text(label);
				ImGui.SameLine();
				if(hnd != null) ImGui.Text(hnd.ToString());
				else            ImGui.Text("null");
				/*int selectedNs = directory._dependancies.FindIndex(x => x._name._hash == hnd._namespace._hash);
				int selectedName = -1;

				string[] dependancyNames = new string[directory._dependancies.Count];
				for(int i = 0; i < directory._dependancies.Count; i++)
				{
					dependancyNames[i] = directory._dependancies[i]._name._string;
				}
				
				string[] objectNames = new string[directory._dependancies[selectedNs]._objectList._count];
				for(int i = 0; i < directory._dependancies[selectedNs]._nameList._count; i++)
				{
					objectNames[i] = directory._nameList[i]._string;
					if(directory._dependancies[selectedNs]._nameList[i]._hash == hnd._alias._hash)
					{
						selectedName = i;
					}
				}

				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID($"1_{label}");
				bool NSChanged = ImGui.Combo(string.Empty, ref selectedNs, dependancyNames, dependancyNames.Length);
				ImGui.PopID();
				ImGui.SameLine();
				ImGui.PushID($"2_{label}");
				bool NameChanged = ImGui.Combo(string.Empty, ref selectedName, objectNames, objectNames.Length);
				ImGui.PopID();
				if(NSChanged || NameChanged)
				{
					value = new igHandle(new igHandleName(){ _name = new igName(objectNames[selectedName]), _ns = new igName(dependancyNames[selectedNs]) });
				}*/
			}
			else if(field is igBitFieldMetaField bitFieldMetaField)
			{
				RenderField(label, ref value, bitFieldMetaField._assignmentMetaField);
			}
			else if(field is igEnumMetaField enumMetaField)
			{
				if(enumMetaField._metaEnum != null)
				{
					string valueName = value.ToString();
					int selectedItem = enumMetaField._metaEnum._names.FindIndex(x => x == valueName);
					ImGui.Text(label);
					ImGui.SameLine();
					ImGui.PushID(label);
					bool changed = ImGui.Combo(string.Empty, ref selectedItem, enumMetaField._metaEnum._names.ToArray(), enumMetaField._metaEnum._names.Count);
					ImGui.PopID();
					if(changed)
					{
						value = enumMetaField._metaEnum.GetEnumFromName(enumMetaField._metaEnum._names[selectedItem]);
					}
				}
				else
				{
					int intValue = (int)value;
					ImGui.Text(label);
					ImGui.SameLine();
					ImGui.PushID(label);
					ImGui.InputInt(string.Empty, ref intValue);
					ImGui.PopID();
					value = (int)Math.Clamp(intValue, int.MinValue, int.MaxValue);
				}
			}
			else if(field is igCompoundMetaField compoundMetaField)
			{
				if(ImGui.TreeNode(label))
				{
					List<igMetaField> fieldList = compoundMetaField._compoundFieldInfo._fieldList;
					for(int i = 0; i < fieldList.Count; i++)
					{
						RenderFieldWithName(value, fieldList[i]);
					}
					ImGui.TreePop();
				}
			}
			else if(field is igVec2fMetaField vec2fmf)
			{
				System.Numerics.Vector2 vec2fValue = (igVec2f)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputFloat2(string.Empty, ref vec2fValue);
				ImGui.PopID();
				value = (igVec2f)vec2fValue;
			}
			else if(field is igVec3fMetaField)
			{
				System.Numerics.Vector3 vec3fValue = (igVec3f)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputFloat3(string.Empty, ref vec3fValue);
				ImGui.PopID();
				value = (igVec3f)vec3fValue;
			}
			else if(field is igVec4fMetaField)
			{
				System.Numerics.Vector4 vec4fValue = (igVec4f)value;
				ImGui.Text(label);
				ImGui.SameLine();
				ImGui.PushID(label);
				ImGui.InputFloat4(string.Empty, ref vec4fValue);
				ImGui.PopID();
				value = (igVec4f)vec4fValue;
			}
		}
	}
}