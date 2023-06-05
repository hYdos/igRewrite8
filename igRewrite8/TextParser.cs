using igLibrary;
using igLibrary.Core;
using System.Reflection;

namespace igRewrite8.Devel
{
	//A lot of this is very poorly designed since it's meant to handle a temporary file format, perhaps i should make it a better format first lol

	public class TextParser
	{
		public Dictionary<string, igMetaObject> metaObjectLookup = new Dictionary<string, igMetaObject>();
		public Dictionary<string, igCompoundMetaFieldInfo> compoundInfoLookup = new Dictionary<string, igCompoundMetaFieldInfo>();

		public void ReadMetaEnumFile(string filePath)
		{
			StreamHelper sh = new StreamHelper(File.Open(filePath, FileMode.Open));

			while(true)
			{
				igMetaEnum metaEnum = new igMetaEnum();
				string metaEnumLine = sh.ReadLine();
				if(metaEnumLine.Length == 0) break;
				metaEnum._name = metaEnumLine.Split(' ')[1];
				sh.ReadLine();
				while(true)
				{
					string memberLine = sh.ReadLine();
					if(memberLine[0] == '}') break;
					string[] memberInfo = memberLine.Substring(1).Split(' ');
					metaEnum._names.Add(memberInfo[0]);
					metaEnum._values.Add(int.Parse(memberInfo[2].TrimEnd(',')));
				}
				igArkCore._metaEnums.Add(metaEnum);
			}
		}
		public void ReadMetaFieldFile(string filePath)
		{
			StreamHelper fieldSh = new StreamHelper(new FileStream(filePath, FileMode.Open, FileAccess.Read));

			List<IG_CORE_PLATFORM> platformLookup = new List<IG_CORE_PLATFORM>();
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX);

			while(true)
			{
				string metaFieldLine = fieldSh.ReadLine();
				if(metaFieldLine.Length == 0) break;

				string[] members = metaFieldLine.Split(' ');


				int index = 0;
				igMetaFieldPlatformInfo platformInfo = new igMetaFieldPlatformInfo();
				platformInfo._name = members[index++];
				while(true)
				{
					index++;	//Skip "psa"
					if(index > members.Length) break;
					IG_CORE_PLATFORM platform = platformLookup[int.Parse(members[index++], System.Globalization.NumberStyles.HexNumber)];
					ushort size = ushort.Parse(members[index++], System.Globalization.NumberStyles.HexNumber);
					ushort alignment = ushort.Parse(members[index++], System.Globalization.NumberStyles.HexNumber);

					platformInfo._sizes.Add(platform, size);
					platformInfo._alignments.Add(platform, alignment);
				}

				igArkCore._metaFieldPlatformInfos.Add(platformInfo);
			}
		}
		public void ReadMetaObjectFile(string filePath)
		{
			StreamHelper sh = new StreamHelper(File.Open(filePath, FileMode.Open));

			//First pass, instantiate metaobjects

			while(true)
			{
				string line = sh.ReadLine();

				if(line.Length == 0) break;

				string[] members = line.Split(' ');
				if(members[0] == "igMetaObject")
				{
					igMetaObject metaObject = (igMetaObject)Activator.CreateInstance(igArkCore.GetObjectDotNetType(members[1]));
					metaObject._name = members[2];

					metaObjectLookup.Add(metaObject._name, metaObject);
				}
				else if(members[0] == "igCompoundField")
				{
					igCompoundMetaFieldInfo compoundInfo = new igCompoundMetaFieldInfo();
					compoundInfo._name = metaObjectLookup.Last().Key;
					compoundInfoLookup.Add(compoundInfo._name, compoundInfo);
				}
			}

			//Second pass, read metaobject information
			sh.Seek(0);
			string previousMeta = null;

			for(int i = 0; i < metaObjectLookup.Count + compoundInfoLookup.Count; i++)
			{
				string metaObjectLine = sh.ReadLine();

				if(metaObjectLine.Length == 0) break;

				string[] members = metaObjectLine.Split(' ');

				if(members[0] == "igMetaObject")
				{
					igMetaObject metaObject = metaObjectLookup[members[2]];

					if(members.Length > 3)
					{
						metaObject._parent = metaObjectLookup[members[3]];
						metaObject.InheritFields();
						int parentMetaIndex = 4;
						if(metaObject._parent._name == "igDataList")
						{
							igMemoryRefMetaField _data = (igMemoryRefMetaField)metaObject._metaFields[2].CreateFieldCopy();
							_data._memType = ReadFieldType(members, ref parentMetaIndex);
							metaObject._metaFields[2] = _data;
						}
						else if(metaObject._parent._name == "igObjectList" || metaObject._parent._name == "igNonRefCountedObjectList")
						{
							igMemoryRefMetaField _data = (igMemoryRefMetaField)metaObject._metaFields[2].CreateFieldCopy();
							((igObjectRefMetaField)_data._memType)._metaObject = metaObjectLookup[members[parentMetaIndex]];
							metaObject._metaFields[2] = _data;
						}
						else if(metaObject._parent._name == "igHashTable")
						{
							igMemoryRefMetaField _values = (igMemoryRefMetaField)metaObject._metaFields[0].CreateFieldCopy();
							igMemoryRefMetaField _keys   = (igMemoryRefMetaField)metaObject._metaFields[1].CreateFieldCopy();
							_values._memType = ReadFieldType(members, ref parentMetaIndex);
							  _keys._memType = ReadFieldType(members, ref parentMetaIndex);
							metaObject._metaFields[0] = _values;
							metaObject._metaFields[1] = _keys;
						}
					}
					while(true)
					{
						string memberLine = sh.ReadLine();
						string[] memberInfo = memberLine.Split(' ');

						if(memberInfo[0] == "igMetaEndObject") break;
						
						int dataIndex = 0;
						igMetaField metaField = ReadFieldType(memberInfo, ref dataIndex, metaObject);
						metaField._offset = ushort.Parse(memberInfo[dataIndex++].Substring(2), System.Globalization.NumberStyles.HexNumber);
						metaField._name = memberInfo[dataIndex++];
						ReadFieldDefault(metaField, ref dataIndex, memberInfo);
						metaObject._metaFields.Add(metaField);
					}
					previousMeta = metaObject._name;
				}
				else if(members[0] == "igCompoundField")
				{
					igCompoundMetaFieldInfo compoundInfo = compoundInfoLookup[previousMeta];
					while(true)
					{
						string memberLine = sh.ReadLine();
						string[] memberInfo = memberLine.Split(' ');

						if(memberInfo[0] == "igEndCompoundField") break;
						
						int dataIndex = 0;
						igMetaField metaField = ReadFieldType(memberInfo, ref dataIndex, compoundInfo);
						metaField._offset = ushort.Parse(memberInfo[dataIndex++].Substring(2), System.Globalization.NumberStyles.HexNumber);
						metaField._name = memberInfo[dataIndex++];
						compoundInfo._fieldList.Add(metaField);
					}
				}
				else break;
			}

			sh.Close();
			sh.Dispose();

			igArkCore._metaObjects.AddRange(metaObjectLookup.Values);
			igArkCore._compoundFieldInfos.AddRange(compoundInfoLookup.Values);
		}

		private igMetaField ReadFieldType(string[] data, ref int index, igBaseMeta? meta = null)
		{
			List<igMetaField> templateArgs = new List<igMetaField>();
			igMetaField? metaField = null;

			string typeName = data[index++];
			uint properties = uint.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);
			int templateCount = int.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);

			for(int i = 0; i < templateCount; i++)
			{
				if(data[index] == "0")
				{
					index++;
					templateArgs.Add(null);
				}
				else
				{
					templateArgs.Add(ReadFieldType(data, ref index));
				}
			}

			Type? t = igArkCore.GetObjectDotNetType(typeName);
			if(t == null)
			{
				t = typeof(igPlaceHolderMetaField);
			}
			metaField = (igMetaField)Activator.CreateInstance(t);
			metaField._parentMeta = meta;

			if(metaField is igRefMetaField refMetaField)
			{
				string refData = data[index++];
				refMetaField._construct = refData[0] == '1';
				refMetaField._destruct = refData[1] == '1';
				refMetaField._reconstruct = refData[2] == '1';
				refMetaField._refCounted = refData[3] == '1';
			}

			if(typeName.StartsWith("igMemoryRefHandle"))
			{
				(metaField as igMemoryRefHandleMetaField)._memType = ReadFieldType(data, ref index);
			}
			else if(typeName.StartsWith("igMemoryRef"))
			{
				(metaField as igMemoryRefMetaField)._memType = ReadFieldType(data, ref index);
			}
			else if(typeName.StartsWith("igObjectRef"))
			{
				(metaField as igObjectRefMetaField)._metaObject = metaObjectLookup[data[index++]];
			}
			else if(typeName.StartsWith("igHandle") && !typeName.StartsWith("igHandleName"))
			{
				(metaField as igHandleMetaField)._metaObject = metaObjectLookup[data[index++]];
			}
			else if(typeName.StartsWith("igStruct"))
			{
				(metaField as igStructMetaField)._sizes.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT, ushort.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber));
				(metaField as igStructMetaField)._alignments.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT, ushort.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber));
			}
			else if(typeName.StartsWith("igEnum"))
			{
				string enumName = data[index++];
				//metaField = new igEnumMetaField() { _metaEnumName = (enumName == "0" ? null : enumName) };
				(metaField as igEnumMetaField)._metaEnum = (enumName == "0" ? null : igArkCore.GetMetaEnum(enumName));
			}
			else if(typeName.StartsWith("igStatic"))
			{
				(metaField as igStaticMetaField)._storageMetaField = ReadFieldType(data, ref index);
			}
			else if(typeName.StartsWith("igPropertyField"))
			{
				(metaField as igPropertyFieldMetaField)._innerMetaField = ReadFieldType(data, ref index);
			}
			else if(typeName.StartsWith("igBitField"))
			{
				igBitFieldMetaField bfMetaField = metaField as igBitFieldMetaField;
				bfMetaField._storageMetaField = bfMetaField._parentMeta.GetFieldByName(data[index++]);
				igMetaField assignmentMetaField = ReadFieldType(data, ref index);
				uint shift = uint.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);
				uint bits = uint.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);
				bfMetaField._assignmentMetaField = assignmentMetaField;
				bfMetaField._shift = shift;
				bfMetaField._bits = bits;
			}

			if(metaField is igPlaceHolderMetaField placeholder)
			{
				placeholder._platformInfo = igArkCore.GetMetaFieldPlatformInfo(typeName);
			}

			metaField._properties._storage = properties;

			metaField.SetTemplateParameterCount((uint)templateCount);
			for(int i= 0; i < templateCount; i++)
			{
				metaField.SetTemplateParameter((uint)i, templateArgs[i]);
			}

			if(typeName.EndsWith("ArrayMetaField"))
			{
				string possibleArray = data[index];
				if(possibleArray.StartsWith("["))
				{
					FieldInfo? num = metaField.GetType().GetField("_num");
					num.SetValue(metaField, short.Parse(data[index++].Substring(3, 8), System.Globalization.NumberStyles.HexNumber));
				}
			}

			return metaField;
		}
		private void ReadFieldDefault(igMetaField field, ref int dataIndex, string[] memberInfo)
		{
			if(memberInfo.Length <= dataIndex) return;

			string typeName = field.GetType().Name;
			string[] defaultInfo = memberInfo[dataIndex].Split(',');

			     if(typeName.StartsWith("igIntPtr"))			field._default = long.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igInt"))				field._default = int.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedIntPtr"))	field._default = ulong.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedInt"))		field._default = uint.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igLong"))				field._default = long.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedLong"))		field._default = ulong.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igShort"))				field._default = short.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedShort"))		field._default = ushort.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igSizeType"))			field._default = ulong.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igBool"))				field._default = byte.Parse(memberInfo[dataIndex]) == 1;
			else if(typeName.StartsWith("igChar"))				field._default = unchecked((sbyte)byte.Parse(memberInfo[dataIndex]));
			else if(typeName.StartsWith("igUnsignedChar"))		field._default = byte.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igFloat"))				field._default = float.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igVec2f"))				field._default = new igLibrary.Math.igVec2f(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]));
			else if(typeName.StartsWith("igVec2uc"))			field._default = new igLibrary.Math.igVec2uc(byte.Parse(defaultInfo[0]), byte.Parse(defaultInfo[1]));
			else if(typeName.StartsWith("igVec3fAligned"))		field._default = new igLibrary.Math.igVec3fAligned(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]));
			else if(typeName.StartsWith("igVec3f"))				field._default = new igLibrary.Math.igVec3f(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]));
			else if(typeName.StartsWith("igVec3d"))				field._default = new igLibrary.Math.igVec3d(double.Parse(defaultInfo[0]), double.Parse(defaultInfo[1]), double.Parse(defaultInfo[2]));
			else if(typeName.StartsWith("igVec4fUnaligned"))	field._default = new igLibrary.Math.igVec4fUnaligned(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]), float.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igVec4f"))				field._default = new igLibrary.Math.igVec4f(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]), float.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igQuaternionf"))		field._default = new igLibrary.Math.igQuaternionf(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]), float.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igVec4uc"))			field._default = new igLibrary.Math.igVec4uc(byte.Parse(defaultInfo[0]), byte.Parse(defaultInfo[1]), byte.Parse(defaultInfo[2]), byte.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igVec4i"))				field._default = new igLibrary.Math.igVec4i(int.Parse(defaultInfo[0]), int.Parse(defaultInfo[1]), int.Parse(defaultInfo[2]), int.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igMatrix44f"))			field._default = new igLibrary.Math.igMatrix44f(new float[16] {float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]), float.Parse(defaultInfo[3]), float.Parse(defaultInfo[4]), float.Parse(defaultInfo[5]), float.Parse(defaultInfo[6]), float.Parse(defaultInfo[7]), float.Parse(defaultInfo[8]), float.Parse(defaultInfo[9]), float.Parse(defaultInfo[10]), float.Parse(defaultInfo[11]), float.Parse(defaultInfo[12]), float.Parse(defaultInfo[13]), float.Parse(defaultInfo[14]), float.Parse(defaultInfo[15])});
			else if(typeName.StartsWith("igString"))
			{
				string hexString = memberInfo[dataIndex];
				string data = System.Text.Encoding.ASCII.GetString(Convert.FromHexString(hexString));
			}
		}
	}
}