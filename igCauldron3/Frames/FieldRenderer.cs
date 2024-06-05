using System.Reflection;
using igLibrary.Core;
using igLibrary.DotNet;
using igLibrary.Math;
using ImGuiNET;
using OpenTK.Platform.Windows;

namespace igCauldron3
{
	public static class FieldRenderer
	{
		private delegate bool RenderFieldAction(string label, ref object? raw, igMetaField field);
		private static Dictionary<Type, RenderFieldAction> _renderFuncLookup = new Dictionary<Type, RenderFieldAction>();
		public static void Init()
		{
			_renderFuncLookup.Add(typeof(igCharMetaField), RenderField_SByte);
			_renderFuncLookup.Add(typeof(igUnsignedCharMetaField), RenderField_Byte);
			_renderFuncLookup.Add(typeof(igShortMetaField), RenderField_Short);
			_renderFuncLookup.Add(typeof(igUnsignedShortMetaField), RenderField_UShort);
			_renderFuncLookup.Add(typeof(igIntMetaField), RenderField_Int);
			_renderFuncLookup.Add(typeof(igUnsignedIntMetaField), RenderField_UInt);
			_renderFuncLookup.Add(typeof(igLongMetaField), RenderField_Long);
			_renderFuncLookup.Add(typeof(igUnsignedLongMetaField), RenderField_ULong);
			_renderFuncLookup.Add(typeof(igSizeTypeMetaField), RenderField_ULong);
			_renderFuncLookup.Add(typeof(igFloatMetaField), RenderField_Float);
			_renderFuncLookup.Add(typeof(igDoubleMetaField), RenderField_Double);
			_renderFuncLookup.Add(typeof(igVec2ucMetaField), RenderField_Vec2uc);
			_renderFuncLookup.Add(typeof(igVec2fMetaField), RenderField_Vec2f);
			_renderFuncLookup.Add(typeof(igVec3ucMetaField), RenderField_Vec3uc);
			_renderFuncLookup.Add(typeof(igVec3fMetaField), RenderField_Vec3f);
			_renderFuncLookup.Add(typeof(igVec3fAlignedMetaField), RenderField_Vec3f);
			_renderFuncLookup.Add(typeof(igVec3dMetaField), RenderField_Vec3d);
			_renderFuncLookup.Add(typeof(igVec4ucMetaField), RenderField_Vec4uc);
			_renderFuncLookup.Add(typeof(igVec4fMetaField), RenderField_Vec4f);
			_renderFuncLookup.Add(typeof(igVec4fUnalignedMetaField), RenderField_Vec4f);
			_renderFuncLookup.Add(typeof(igVec4iMetaField), RenderField_Vec4i);
			_renderFuncLookup.Add(typeof(igQuaternionfMetaField), RenderField_Quaternionf);
			_renderFuncLookup.Add(typeof(igMatrix44fMetaField), RenderField_Matrix44f);
			_renderFuncLookup.Add(typeof(igStringMetaField), RenderField_String);
			_renderFuncLookup.Add(typeof(igBoolMetaField), RenderField_Bool);
			_renderFuncLookup.Add(typeof(igMemoryRefMetaField), RenderField_MemoryRef);
			_renderFuncLookup.Add(typeof(igMemoryRefHandleMetaField), RenderField_MemoryRef);
			_renderFuncLookup.Add(typeof(igBitFieldMetaField), RenderField_BitField);
			_renderFuncLookup.Add(typeof(igObjectRefMetaField), RenderField_Object);
			_renderFuncLookup.Add(typeof(igHandleMetaField), RenderField_Handle);
		}
		public static bool RenderField(string label, ref object? value, igMetaField field)
		{
			if(field is igStaticMetaField) return false;
			if(field is igPropertyFieldMetaField) return false;
			ImGui.Text(label);
			ImGui.SameLine();
			return RenderFieldNoLabel(label, ref value, field);
		}
		private static bool RenderFieldNoLabel(string label, ref object? value, igMetaField field)
		{
			if(_renderFuncLookup.TryGetValue(field.GetType(), out RenderFieldAction? renderFunc))
			{
				return renderFunc.Invoke(label, ref value, field);
			}
			else
			{
				ImGui.Text($"{field.GetType().Name} is unimplemented.");
				return false;
			}
		}

#region Primitive Numeric Renderers
		private static bool RenderField_PrimitiveNumber(string label, ref object? raw, ElementType type)
		{
			string val = raw!.ToString()!;
			ImGui.PushID(label);
			ImGui.PushItemWidth(128);
			bool changed = ImGui.InputText(string.Empty, ref val, 128);
			ImGui.PopItemWidth();
			ImGui.PopID();
			if(changed)
			{
				MethodInfo convertFunc;
				switch (type)
				{
					case ElementType.kElementTypeI1: convertFunc = ((Func<string, sbyte>)Convert.ToSByte).Method; break;
					case ElementType.kElementTypeU1: convertFunc = ((Func<string, byte>)Convert.ToByte).Method; break;
					case ElementType.kElementTypeI2: convertFunc = ((Func<string, short>)Convert.ToInt16).Method; break;
					case ElementType.kElementTypeU2: convertFunc = ((Func<string, ushort>)Convert.ToUInt16).Method; break;
					case ElementType.kElementTypeI4: convertFunc = ((Func<string, int>)Convert.ToInt32).Method; break;
					case ElementType.kElementTypeU4: convertFunc = ((Func<string, uint>)Convert.ToUInt32).Method; break;
					case ElementType.kElementTypeI8: convertFunc = ((Func<string, long>)Convert.ToInt64).Method; break;
					case ElementType.kElementTypeU8: convertFunc = ((Func<string, ulong>)Convert.ToUInt64).Method; break;
					case ElementType.kElementTypeR4: convertFunc = ((Func<string, float>)Convert.ToSingle).Method; break;
					case ElementType.kElementTypeR8: convertFunc = ((Func<string, double>)Convert.ToDouble).Method; break;
					default: throw new ArgumentException($"Element Type {type} is not a primitive type");
				}
				try
				{
					raw = convertFunc.Invoke(null, new object?[]{val});
				}
				catch(Exception){ changed = false; }	//change nothing
			}
			return changed;
		}
		private static bool RenderField_SByte(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeI1);
		private static bool RenderField_Byte(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeU1);
		private static bool RenderField_Short(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeI2);
		private static bool RenderField_UShort(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeU2);
		private static bool RenderField_Int(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeI4);
		private static bool RenderField_UInt(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeU4);
		private static bool RenderField_Long(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeI8);
		private static bool RenderField_ULong(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeU8);
		private static bool RenderField_Float(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeR4);
		private static bool RenderField_Double(string label, ref object? raw, igMetaField field) => RenderField_PrimitiveNumber(label, ref raw, ElementType.kElementTypeR8);
#endregion
#region Math Structure Renderers
		private static bool RenderField_Vec2f(string label, ref object? raw, igMetaField field)
		{
			igVec2f value = (igVec2f)raw!;
			bool changed = false;
			object? worker = value._x; if(RenderField_Float(label + " _x", ref worker, igFloatMetaField._MetaField)) { changed = true; value._x = (float)worker!; } ImGui.SameLine();
			        worker = value._y; if(RenderField_Float(label + " _y", ref worker, igFloatMetaField._MetaField)) { changed = true; value._y = (float)worker!; }
			return changed;
		}
		private static bool RenderField_Vec3f(string label, ref object? raw, igMetaField field)
		{
			igVec3f value = (igVec3f)raw!;
			bool changed = false;
			object? worker = value._x; if(RenderField_Float(label + " _x", ref worker, igFloatMetaField._MetaField)) { changed = true; value._x = (float)worker!; } ImGui.SameLine();
			        worker = value._y; if(RenderField_Float(label + " _y", ref worker, igFloatMetaField._MetaField)) { changed = true; value._y = (float)worker!; } ImGui.SameLine();
			        worker = value._z; if(RenderField_Float(label + " _z", ref worker, igFloatMetaField._MetaField)) { changed = true; value._z = (float)worker!; }
			return changed;
		}
		private static bool RenderField_Vec3d(string label, ref object? raw, igMetaField field)
		{
			igVec3d value = (igVec3d)raw!;
			bool changed = false;
			object? worker = value._x; if(RenderField_Double(label + " _x", ref worker, igDoubleMetaField._MetaField)) { changed = true; value._x = (double)worker!; } ImGui.SameLine();
			        worker = value._y; if(RenderField_Double(label + " _y", ref worker, igDoubleMetaField._MetaField)) { changed = true; value._y = (double)worker!; } ImGui.SameLine();
			        worker = value._z; if(RenderField_Double(label + " _z", ref worker, igDoubleMetaField._MetaField)) { changed = true; value._z = (double)worker!; }
			return changed;
		}
		private static bool RenderField_Vec4f(string label, ref object? raw, igMetaField field)
		{
			igVec4f value = (igVec4f)raw!;
			bool changed = false;
			object? worker = value._x; if(RenderField_Float(label + " _x", ref worker, igFloatMetaField._MetaField)) { changed = true; value._x = (float)worker!; } ImGui.SameLine();
			        worker = value._y; if(RenderField_Float(label + " _y", ref worker, igFloatMetaField._MetaField)) { changed = true; value._y = (float)worker!; } ImGui.SameLine();
			        worker = value._z; if(RenderField_Float(label + " _z", ref worker, igFloatMetaField._MetaField)) { changed = true; value._z = (float)worker!; } ImGui.SameLine();
			        worker = value._w; if(RenderField_Float(label + " _w", ref worker, igFloatMetaField._MetaField)) { changed = true; value._w = (float)worker!; }
			return changed;
		}
		private static bool RenderField_Vec2uc(string label, ref object? raw, igMetaField field)
		{
			igVec2uc value = (igVec2uc)raw!;
			bool changed = false;
			object? worker = value._x; if(RenderField_Byte(label + " _x", ref worker, igCharMetaField._MetaField)) { changed = true; value._x = (byte)worker!; } ImGui.SameLine();
			        worker = value._y; if(RenderField_Byte(label + " _y", ref worker, igCharMetaField._MetaField)) { changed = true; value._y = (byte)worker!; }
			return changed;
		}
		private static bool RenderField_Vec3uc(string label, ref object? raw, igMetaField field)
		{
			igVec3uc value = (igVec3uc)raw!;
			bool changed = false;
			object? worker = value._x; if(RenderField_Byte(label + " _x", ref worker, igCharMetaField._MetaField)) { changed = true; value._x = (byte)worker!; } ImGui.SameLine();
			        worker = value._y; if(RenderField_Byte(label + " _y", ref worker, igCharMetaField._MetaField)) { changed = true; value._y = (byte)worker!; } ImGui.SameLine();
			        worker = value._z; if(RenderField_Byte(label + " _z", ref worker, igCharMetaField._MetaField)) { changed = true; value._z = (byte)worker!; }
			return changed;
		}
		private static bool RenderField_Vec4uc(string label, ref object? raw, igMetaField field)
		{
			igVec4uc value = (igVec4uc)raw!;
			bool changed = false;
			object? worker = value._r; if(RenderField_Byte(label + " _r", ref worker, igCharMetaField._MetaField)) { changed = true; value._r = (byte)worker!; } ImGui.SameLine();
			        worker = value._g; if(RenderField_Byte(label + " _g", ref worker, igCharMetaField._MetaField)) { changed = true; value._g = (byte)worker!; } ImGui.SameLine();
			        worker = value._b; if(RenderField_Byte(label + " _b", ref worker, igCharMetaField._MetaField)) { changed = true; value._b = (byte)worker!; } ImGui.SameLine();
			        worker = value._a; if(RenderField_Byte(label + " _a", ref worker, igCharMetaField._MetaField)) { changed = true; value._a = (byte)worker!; }
			return changed;
		}
		private static bool RenderField_Vec4i(string label, ref object? raw, igMetaField field)
		{
			igVec4i value = (igVec4i)raw!;
			bool changed = false;
			object? worker = value._x; if(RenderField_Byte(label + " _x", ref worker, igIntMetaField._MetaField)) { changed = true; value._x = (int)worker!; } ImGui.SameLine();
			        worker = value._y; if(RenderField_Byte(label + " _y", ref worker, igIntMetaField._MetaField)) { changed = true; value._y = (int)worker!; } ImGui.SameLine();
			        worker = value._z; if(RenderField_Byte(label + " _z", ref worker, igIntMetaField._MetaField)) { changed = true; value._z = (int)worker!; } ImGui.SameLine();
			        worker = value._w; if(RenderField_Byte(label + " _w", ref worker, igIntMetaField._MetaField)) { changed = true; value._w = (int)worker!; }
			return changed;
		}
		private static bool RenderField_Quaternionf(string label, ref object? raw, igMetaField field)
		{
			igQuaternionf value = (igQuaternionf)raw!;
			bool changed = false;
			object? worker = value._x; if(RenderField_Float(label + " _x", ref worker, igFloatMetaField._MetaField)) { changed = true; value._x = (float)worker!; } ImGui.SameLine();
			        worker = value._y; if(RenderField_Float(label + " _y", ref worker, igFloatMetaField._MetaField)) { changed = true; value._y = (float)worker!; } ImGui.SameLine();
			        worker = value._z; if(RenderField_Float(label + " _z", ref worker, igFloatMetaField._MetaField)) { changed = true; value._z = (float)worker!; } ImGui.SameLine();
			        worker = value._w; if(RenderField_Float(label + " _w", ref worker, igFloatMetaField._MetaField)) { changed = true; value._w = (float)worker!; }
			return changed;
		}
		private static bool RenderField_Matrix44f(string label, ref object? raw, igMetaField field)
		{
			igMatrix44f value = (igMatrix44f)raw!;
			bool changed = false;
			object? worker = value._m11; if(RenderField_Float(label + " _m11", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m11 = (float)worker!; } ImGui.SameLine();
			        worker = value._m12; if(RenderField_Float(label + " _m12", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m12 = (float)worker!; } ImGui.SameLine();
			        worker = value._m13; if(RenderField_Float(label + " _m13", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m13 = (float)worker!; } ImGui.SameLine();
			        worker = value._m14; if(RenderField_Float(label + " _m14", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m14 = (float)worker!; }
			        worker = value._m21; if(RenderField_Float(label + " _m21", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m21 = (float)worker!; } ImGui.SameLine();
			        worker = value._m22; if(RenderField_Float(label + " _m22", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m22 = (float)worker!; } ImGui.SameLine();
			        worker = value._m23; if(RenderField_Float(label + " _m23", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m23 = (float)worker!; } ImGui.SameLine();
			        worker = value._m24; if(RenderField_Float(label + " _m24", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m24 = (float)worker!; }
			        worker = value._m31; if(RenderField_Float(label + " _m31", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m31 = (float)worker!; } ImGui.SameLine();
			        worker = value._m32; if(RenderField_Float(label + " _m32", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m32 = (float)worker!; } ImGui.SameLine();
			        worker = value._m33; if(RenderField_Float(label + " _m33", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m33 = (float)worker!; } ImGui.SameLine();
			        worker = value._m34; if(RenderField_Float(label + " _m34", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m34 = (float)worker!; }
			        worker = value._m41; if(RenderField_Float(label + " _m41", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m41 = (float)worker!; } ImGui.SameLine();
			        worker = value._m42; if(RenderField_Float(label + " _m42", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m42 = (float)worker!; } ImGui.SameLine();
			        worker = value._m43; if(RenderField_Float(label + " _m43", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m43 = (float)worker!; } ImGui.SameLine();
			        worker = value._m44; if(RenderField_Float(label + " _m44", ref worker, igFloatMetaField._MetaField)) { changed = true; value._m44 = (float)worker!; }
			return changed;
		}
#endregion
		private static bool RenderField_String(string label, ref object? raw, igMetaField field)
		{
			ImGui.PushID(label);
			string value = (string)raw ?? string.Empty;
			bool changed = ImGui.InputText(string.Empty, ref value, ushort.MaxValue);
			if(changed) raw = value;
			return changed;
		}
		private static bool RenderField_Bool(string label, ref object? raw, igMetaField field)
		{
			ImGui.PushID(label);
			bool value = (bool)raw!;
			bool changed = ImGui.Checkbox(string.Empty, ref value);
			if(changed) raw = value;
			return changed;
		}
#region Array Rendereres
		public static bool RenderArrayField(string label, ref object? value, igMetaField field)
		{
			FieldInfo? fi = field.GetType().GetField("_num");
			if(fi != null)
			{
				short num = (short)fi.GetValue(field)!;
				Array values = (Array)value!;
				for(int i = 0; i < num; i++)
				{
					object? currValue = values.GetValue(i);
					bool changed = RenderField($"{i}_{label}", ref currValue, field);
					if(changed) values.SetValue(currValue, i);
				}
				return false;	//The array itself never changes, only the contents of it, so we always return false
			}
			else return RenderField(label, ref value, field);	//Here it's different though
		}
		private static bool RenderField_Vector(string label, ref object? raw, igMetaField field)
		{
			if(ImGui.TreeNode(label, "Data"))
			{
				igVectorCommon vector = (igVectorCommon)raw!;
				IigMemory memValue = vector.GetData();
				igMetaField memType = ((igVectorMetaField)field).GetTemplateParameter(0);

				Array data = memValue.GetData();
				if(data != null)
				{
					//ADD REMOVE BUTTON
					for(int i = 0; i < vector.GetCount(); i++)
					{
						object? arrValue = vector.GetItem(i);
						bool changed = RenderField($"Element {i}", ref arrValue, memType);
						if(changed) vector.SetItem(i, arrValue);
					}
					if(ImGui.Button("+"))
					{
						vector.SetCapacity((int)vector.GetCount() + 1);
						vector.SetCount(vector.GetCount() + 1u);
					}
				}
				ImGui.TreePop();
			}
			return false;
		}
		private static bool RenderField_MemoryRef(string label, ref object? raw, igMetaField field)
		{
			bool memChanged = false;
			if(ImGui.TreeNode(label, "Data"))
			{
				IigMemory memValue = (IigMemory)raw!;
				igMetaField memType;
				if(field is igMemoryRefMetaField memoryRefMetaField)
				{
					memType = memoryRefMetaField._memType;
				}
				else if(field is igMemoryRefHandleMetaField memoryRefHandleMetaField)
				{
					memType = memoryRefHandleMetaField._memType;
				}
				else throw new NotImplementedException($"yo you forgot to implement {field.GetType().Name} into this func");

				Array data = memValue.GetData();
				if(data != null)
				{
					int remove = -1;
					for(int i = 0; i < data.Length; i++)
					{
						ImGui.PushID(label + field.GetHashCode().ToString("X08") + i.ToString("X08"));
						if(ImGui.Button("-"))
						{
							remove = i;
						}
						ImGui.PopID();
						ImGui.SameLine();
						object? arrValue = data.GetValue(i);
						bool changed = RenderField($"Element {i}", ref arrValue, memType);
						if(changed) data.SetValue(arrValue, i);
					}
					if(remove >= 0)
					{
						Array newData = Array.CreateInstance(data.GetType().GetElementType()!, data.Length - 1);
						for(int r = 0, w = 0; r < data.Length; r++)
						{
							if(r == remove) continue;
							newData.SetValue(data.GetValue(r), w);
							w++;
						}
						memValue.SetData(newData);
						memChanged = true;
					}
					if(ImGui.Button("+"))
					{
						memValue.Realloc(data.Length + 1);
						memChanged = true;
					}
				}
				ImGui.TreePop();
			}
			return memChanged;
		}
#endregion
		public static bool RenderField_BitField(string label, ref object? raw, igMetaField field)
		{
			igBitFieldMetaField bfmf = (igBitFieldMetaField)field;
			return RenderFieldNoLabel(label, ref raw, bfmf._assignmentMetaField);
		}
		public static bool RenderField_Object(string label, ref object? raw, igMetaField field)
		{
			DirectoryManagerFrame._instance.RenderObject(label, (igObject?)raw);
			return false;
		}
		public static bool RenderField_Handle(string label, ref object? raw, igMetaField field)
		{
			string display = "NullHandle";
			if(raw != null)
			{
				display = raw.ToString()!;
			}
			ImGui.PushID(label);
			bool shouldEdit = ImGui.Selectable(display);
			ImGui.PopID();
			if(shouldEdit)
			{
				DirectoryManagerFrame._instance.AddChild(new HandlePickerFrame(Window._instance, (handle) => Console.WriteLine("Selected " + handle.ToString())));
			}
			//I know it should only be done when the field is edited but this is the easiest way to set it up, in the future, a better ui implementation should be thought up
			return true;
		}
	}
}