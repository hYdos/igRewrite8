using System.IO;

namespace PTMTB
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			UndumpFromArkFile(args);
			//UndumpFromTemporaryFile(args);
		}
		private static void UndumpFromArkFile(string[] args)
		{
			igArkCore.ReadFromFile2(igArkCore.EGame.EV_SkylandersSuperchargers);

			List<igMetaObject> metaObjects = igArkCore._metaObjects;
			List<igMetaEnum> metaEnums = igArkCore._metaEnums;
			List<igCompoundMetaFieldInfo> compoundInfos = igArkCore._compoundFieldInfos;

			return;
		}
		private static void UndumpFromTemporaryFile(string[] args)
		{
			StreamHelper objSh = new StreamHelper(new FileStream(args[0], FileMode.Open, FileAccess.Read));
			StreamHelper enumSh = new StreamHelper(new FileStream(args[1], FileMode.Open, FileAccess.Read));

			while(true)
			{
				string metaObjectLine = objSh.ReadLine();
				string[] metaObjectInfo = metaObjectLine.Split(' ');
				if(metaObjectInfo[0] != "igMetaObject")
				{
					if(metaObjectInfo[0] == "igCompoundField")
					{
						igCompoundMetaFieldInfo compoundInfo = new igCompoundMetaFieldInfo();
						compoundInfo._fieldTypeName = igArkCore._metaObjects.Last()._name;
						while(true)
						{
							string memberLine = objSh.ReadLine();
							string[] memberInfo = memberLine.Split(' ');

							if(memberInfo[0] == "igEndCompoundField") break;
							
							int dataIndex = 0;
							igMetaField metaField = ReadFieldType(memberInfo, ref dataIndex);
							metaField._offset = ushort.Parse(memberInfo[dataIndex++].Substring(2), System.Globalization.NumberStyles.HexNumber);
							metaField._name = memberInfo[dataIndex++];
							compoundInfo._fieldList.Add(metaField);
						}
						igArkCore._compoundFieldInfos.Add(compoundInfo);
						continue;
					}
					else break;
				}
				igMetaObject meta = new igMetaObject();
				if(metaObjectLine.Length == 0) break;
				meta._name = metaObjectInfo[1];
				if(metaObjectInfo.Length > 2)
				{
					meta._parentName = metaObjectInfo[2];
				}
				while(true)
				{
					string memberLine = objSh.ReadLine();
					string[] memberInfo = memberLine.Split(' ');

					if(memberInfo[0] == "igMetaEndObject") break;
					
					int dataIndex = 0;
					igMetaField metaField = ReadFieldType(memberInfo, ref dataIndex);
					metaField._offset = ushort.Parse(memberInfo[dataIndex++].Substring(2), System.Globalization.NumberStyles.HexNumber);
					metaField._name = memberInfo[dataIndex++];
					meta._fields.Add(metaField);
				}
				
				igArkCore._metaObjects.Add(meta);
			}

			while(true)
			{
				igMetaEnum metaEnum = new igMetaEnum();
				string metaEnumLine = enumSh.ReadLine();
				if(metaEnumLine.Length == 0) break;
				metaEnum._name = metaEnumLine.Split(' ')[1];
				enumSh.ReadLine();
				while(true)
				{
					string memberLine = enumSh.ReadLine();
					if(memberLine[0] == '}') break;
					string[] memberInfo = memberLine.Substring(1).Split(' ');
					metaEnum._names.Add(memberInfo[0]);
					metaEnum._values.Add(int.Parse(memberInfo[2].TrimEnd(',')));
				}
				igArkCore._metaEnums.Add(metaEnum);
			}

			igArkCore.WriteToFile2(igArkCore.EGame.EV_SkylandersSuperchargers);

			//igArkCore.Reset();

			//.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

			return;
		}
		private static igMetaField ReadFieldType(string[] data, ref int index)
		{
			List<igMetaField> templateArgs = new List<igMetaField>();
			igMetaField? metaField = null;

			string typeName = data[index++];
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

			if(typeName.StartsWith("igMemoryRefHandle"))
			{
				metaField = new igMemoryRefHandleMetaField() { _memType = ReadFieldType(data, ref index) };
			}
			else if(typeName.StartsWith("igMemoryRef"))
			{
				metaField = new igMemoryRefMetaField() { _memType = ReadFieldType(data, ref index) };
			}
			else if(typeName.StartsWith("igObjectRef"))
			{
				metaField = new igObjectRefMetaField() { _metaObjectName = data[index++] };
			}
			else if(typeName.StartsWith("igHandle") && !typeName.StartsWith("igHandleName"))
			{
				metaField = new igHandleMetaField() { _metaObjectName = data[index++] };
			}
			else if(typeName.StartsWith("igEnum"))
			{
				string enumName = data[index++];
				metaField = new igEnumMetaField() { _metaEnumName = (enumName == "0" ? null : enumName) };
			}
			else if(typeName.StartsWith("igStatic"))
			{
				metaField = new igStaticMetaField() { _storageMetaField = ReadFieldType(data, ref index) };
			}
			else if(typeName.StartsWith("igPropertyField"))
			{
				metaField = new igPropertyFieldMetaField() { _innerMetaField = ReadFieldType(data, ref index) };
			}
			else if(typeName.StartsWith("igBitField"))
			{
				igMetaField assignmentMetaField = ReadFieldType(data, ref index);
				uint shift = uint.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);
				uint bits = uint.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);
				metaField = new igBitFieldMetaField() { _assignmentMetaField = assignmentMetaField, _shift = shift, _bits = bits };
			}
			if(metaField == null)
			{
				metaField = new igMetaField();
			}

			metaField._typeName = typeName;
			metaField._templateArgs = templateArgs;
			string possibleArray = data[index];
			if(possibleArray.StartsWith("["))
			{
				metaField._num = short.Parse(data[index++].Substring(3, 8), System.Globalization.NumberStyles.HexNumber);
			}

			return metaField;
		}
	}
}