using System.Reflection;
using igLibrary.Core;
using ImGuiNET;

using static igCauldron3.Window;

namespace igCauldron3
{
	public class ObjectManagerFrame : Frame
	{
		private static List<InspectorDrawOverride> overrides = new List<InspectorDrawOverride>();

		public ObjectManagerFrame() : base()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type[] types = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsAssignableTo(typeof(InspectorDrawOverride))).ToArray();
			for(uint i = 0; i < types.Length; i++)
			{
				if(!types[i].IsAbstract)
				{
					InspectorDrawOverride drawOverride = (InspectorDrawOverride)Activator.CreateInstance(types[i]);
					overrides.Add(drawOverride);
				}
			}

		}

		public override void Render()
		{
			ImGui.Begin("Object Manager");
			if(ImGui.TreeNode("Objects"))
			{
				for(int i = 0; i < directory._objectList._count; i++)
				{
					string name;
					if(directory._useNameList) name = directory._nameList[i]._string;
					else                       name = $"Object {i}";

					RenderObject(name, directory._objectList[i]);
				}
				ImGui.TreePop();
			}
			ImGui.End();
		}
		private void RenderObject(string label, igObject? obj)
		{
			if(obj == null)
			{
				ImGui.Text($"{label}: null");
				return;
			}
			igMetaObject meta = obj.GetMeta();
			if(ImGui.TreeNode(obj.GetHashCode().ToString("X08"), $"{label}: {meta._name}"))
			{
				int i = 0;

				int overrideIndex = overrides.FindIndex(x => meta._vTablePointer.IsAssignableTo(typeof(IigDataList)));
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
		}
		public void RenderObjectFields(igObject obj, igMetaObject meta, int i = 0)
		{
			for(; i < meta._metaFields.Count; i++)
			{
				RenderFieldWithName(obj, meta._metaFields[i]);
			}
		}
		private void RenderFieldWithName(object obj, igMetaField field)
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
				ImGui.InputInt(label, ref intValue);
				value = (int)Math.Clamp(intValue, int.MinValue, int.MaxValue);
			}
			else if(field is igUnsignedShortMetaField)
			{
				int intValue = (ushort)value;
				ImGui.InputInt(label, ref intValue);
				value = (ushort)Math.Clamp(intValue, ushort.MinValue, ushort.MaxValue);
			}
			else if(field is igShortMetaField)
			{
				int intValue = (short)value;
				ImGui.InputInt(label, ref intValue);
				value = (short)Math.Clamp(intValue, short.MinValue, short.MaxValue);
			}
			else if(field is igStringMetaField)
			{
				string strValue = (string)value;
				if(strValue == null) strValue = "";
				ImGui.InputText(label, ref strValue, (uint)short.MaxValue);
				if(value != null || strValue.Length > 1)
				{
					value = strValue;
				}
			}
			else if(field is igUnsignedCharMetaField)
			{
				int byteValue = (byte)value;
				ImGui.InputInt(label, ref byteValue);
				value = (byte)Math.Clamp(byteValue, byte.MinValue, byte.MaxValue);
			}
			else if(field is igCharMetaField)
			{
				int byteValue = (sbyte)value;
				ImGui.InputInt(label, ref byteValue);
				value = (sbyte)Math.Clamp(byteValue, sbyte.MinValue, sbyte.MaxValue);
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
					for(int i = 0; i < data.Length; i++)
					{
						object? arrValue = data.GetValue(i);
						RenderArrayField($"Element {i}", ref arrValue, memType);
						data.SetValue(arrValue, i);
					}
					ImGui.TreePop();
				}
			}
			else if(field is igObjectRefMetaField)
			{
				RenderObject(label, (igObject?)value);
			}
			else if(field is igBitFieldMetaField bitFieldMetaField)
			{
				RenderField(label, ref value, bitFieldMetaField._assignmentMetaField);
			}
			else if(field is igEnumMetaField enumMetaField)
			{
				ImGui.Text($"{label}: {value.ToString()}");
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
		}
	}
}