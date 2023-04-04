using System.Reflection;
using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	public class ObjectManagerFrame : Frame
	{
		private static List<InspectorDrawOverride> overrides = new List<InspectorDrawOverride>();
		public igObjectDirectory directory;

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

		public void Initialize(string[] args)
		{
			igArchive arc = new igArchive(args[1]);
			directory = igObjectStreamManager.Singleton.Load(args[2]);
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
		private void RenderFieldWithName(igObject obj, igMetaField field)
		{
			Type t = obj.GetType();
			FieldInfo? fi = t.GetField(field._name);
			bool canRender = fi != null;

			if(canRender)
			{
				object? value = fi.GetValue(obj);
				RenderField(field._name, ref value, field);
				fi.SetValue(obj, value);
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
						RenderField($"Element {i}", ref arrValue, memType);
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
		}
	}
}