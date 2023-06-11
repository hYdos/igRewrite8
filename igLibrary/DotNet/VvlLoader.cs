using System.Runtime.InteropServices;
using System.Diagnostics;

namespace igLibrary.DotNet
{
	public static class VvlLoader
	{
		public static DotNetLibrary Load(string libName, DotNetRuntime runtime, out bool success)
		{
			//Open file
			igFileContext.Singleton.Open(libName, igFileContext.GetOpenFlags(FileAccess.Read, FileMode.Open), out igFileDescriptor fd, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
			StreamHelper sh = new StreamHelper(fd._handle, StreamHelper.Endianness.Little);

			//Endianness
			if(sh.ReadUInt32() == 0x08000000) sh._endianness = StreamHelper.Endianness.Big;

			//Read Header
			sh.Seek(0);
			VvlHeader header = sh.ReadStruct<VvlHeader>();			
			Debug.Assert(header._sizeofSize == 0x28);

			//Prepare library and resolver
			DotNetLibrary library = new DotNetLibrary();
			library._runtime = runtime;
			igDotNetLoadResolver resolver = new igDotNetLoadResolver();
			resolver._runtime = runtime;

			//Read the file's sections
			resolver._stringTable = new StreamHelper(sh.ReadBytes(header._stringTableLength));
			StreamHelper strings = resolver._stringTable;
			sh.Seek(header._novaUnkCount * 0x54, SeekOrigin.Current);
			VvlFieldDefinition[] fieldDefs = sh.ReadStructArray<VvlFieldDefinition>(header._fieldCount);
			SDotNetMethodMeta[] sMethodMetas = sh.ReadStructArray<SDotNetMethodMeta>(header._methodCount);
			VvlParameterMeta[] paramMetas = sh.ReadStructArray<VvlParameterMeta>(header._parameterCount);
			VvlAttribute[] attributeMetas = sh.ReadStructArray<VvlAttribute>(header._attributeCount);
			sh.Seek(header.unk20 * 0x04, SeekOrigin.Current);
			StreamHelper bTypeDetails = new StreamHelper(sh.ReadBytes(header._typeDetailLength), sh._endianness);

			sh.Close();
			sh.Dispose();

			VvlTypeDetailHeader typeHeader = bTypeDetails.ReadStruct<VvlTypeDetailHeader>();
			Debug.Assert(typeHeader._sizeofSize == 0x50);

			//Method metadata pass
			Debug.Assert(typeHeader._methodCount == header._methodCount);

			library._methodDefs.SetCapacity((int)header._methodCount);

			bTypeDetails.Seek(typeHeader._methodDefOffset);
			VvlMethodDef[] methodDefs = bTypeDetails.ReadStructArray<VvlMethodDef>(typeHeader._methodCount);
			for(int i = 0; i < typeHeader._methodCount; i++)
			{
				DotNetMethodDefinition methodDef = new DotNetMethodDefinition();

				methodDef._methodMeta._importTag = ReadVvlString(strings, sMethodMetas[i]._importTag);
				methodDef._methodMeta._methodName = ReadVvlString(strings, sMethodMetas[i]._methodName);
				methodDef._methodMeta._entryPoint = ReadVvlString(strings, sMethodMetas[i]._entryPoint);
				methodDef._methodMeta._return = ConvertParameterMeta(paramMetas[sMethodMetas[i]._retParamIndex], attributeMetas, strings);

				int paramCount = methodDefs[i]._paramCount;
				methodDef._methodMeta._parameters.SetCapacity(paramCount);
				for(int j = 0; j < paramCount; j++)
				{
					methodDef._methodMeta._parameters.Append(ConvertParameterMeta(paramMetas[sMethodMetas[i]._paramIndex + j], attributeMetas, strings));
				}

				library._methodDefs.Append(methodDef);
			}

			//Dereference these to hopefully free them?
			sMethodMetas = null;
			paramMetas = null;

			//Read types
			bTypeDetails.Seek(0x50);
			library._stringTable = new StreamHelper(bTypeDetails.ReadBytes(typeHeader._stringTableLength));
			library._ownedTypes.SetCapacity(typeHeader._ownedTypeCount);
			DotNetTypeDetailsList typeDetails = new DotNetTypeDetailsList();
			bTypeDetails.Seek(typeHeader._ownedTypeOffset);
			VvlTypeDetails[] vvltds = bTypeDetails.ReadStructArray<VvlTypeDetails>((uint)typeHeader._ownedTypeCount);
			bTypeDetails.Seek(typeHeader._genericTypeOffset);
			VvlGenericTypeDetails[] vvlgtds = bTypeDetails.ReadStructArray<VvlGenericTypeDetails>((uint)typeHeader._genericTypeCount);

			uint[] memberStarts = new uint[vvltds.Length + vvlgtds.Length];
			uint[] memberCounts = new uint[vvltds.Length + vvlgtds.Length];

			for(int i = 0; i < typeHeader._ownedTypeCount; i++)
			{
				memberStarts[i] = vvltds[i]._fieldStartIndex;
				memberCounts[i] = vvltds[i]._memberCount;
				DotNetTypeDetails dntd = ConvertTypeDetails(resolver, vvltds[i], strings, fieldDefs, attributeMetas, bTypeDetails);
				if(dntd != null)
				{
					typeDetails.Append(dntd);
				}
			}

			for(int i = 0; i < typeHeader._genericTypeCount; i++)
			{
				DotNetTypeDetails dntd = new DotNetTypeDetails();
				dntd._interfaceOffset = 0;
				dntd._interfaceCount = 0;
				dntd._templateParameterOffset = vvlgtds[i]._templateParameterOffset;
				dntd._templateParameterCount = vvlgtds[i]._templateParameterCount;
				string typeSpecName = ReadVvlString(strings, vvlgtds[i]._typeSpecName);
				typeSpecName = resolver._runtime._prefix + typeSpecName.TrimStart('.');
				
				memberStarts[typeHeader._ownedTypeCount + i] = 0;
				memberCounts[typeHeader._ownedTypeCount + i] = 0;
				igDotNetTypeSpecMetaObject? typeSpec = (igDotNetTypeSpecMetaObject?)igArkCore.GetObjectMeta(typeSpecName);
				if(typeSpec == null)
				{
					typeSpec = new igDotNetTypeSpecMetaObject();
					typeSpec._name = typeSpecName;
				}
				else
				{
					dntd._ownsMeta = false;
				}
				dntd._targetMeta = typeSpec;
				typeSpec._owners.Append(library);
				resolver.AppendMeta(typeSpec);
				if(typeSpec._name != typeSpecName)
				{
					resolver._aliases.TryAdd(typeSpecName, typeSpec._name);
				}
				igDotNetTypeReference baseTypeRef = new igDotNetTypeReference(resolver, vvlgtds[i]._isArray != 0, vvlgtds[i]._elementType, ReadVvlString(strings, vvlgtds[i]._someTypeReference));
				dntd._baseType = baseTypeRef;
				typeDetails.Append(dntd);
			}

			igVector<int> incompleteObjects = new igVector<int>();

			for(int i = 0; i < typeDetails._count; i++)
			{
				if(!typeDetails[i]._ownsMeta) continue;
				if(typeDetails[i]._targetMeta is igMetaObject metaObject)
				{
					incompleteObjects.Append(i);
				}
				else
				{
					library._ownedTypes.Append(typeDetails[i]._targetMeta);
				}
			}

			//TODO: Read static fields

			//Read fields of types
			for(int i = 0; i < incompleteObjects._count; i++)
			{
				int metaIndex = incompleteObjects[i];
				igMetaObject meta = (igMetaObject)typeDetails[metaIndex]._targetMeta;
				if(meta is igDotNetDynamicMetaObject dynamicMeta)
				{
					//You'd implement igPatchAttribute here

					bool importSuccess = false;
					igDotNetTypeReference typeRef = typeDetails[metaIndex]._baseType;
					DotNetType baseType = new DotNetType();
					while(!importSuccess)
					{
						importSuccess = typeRef.TryResolveObject(out baseType);
						if(!importSuccess)
						{
							int nsIndex = typeRef._name.IndexOf('.');
							if(nsIndex >= 0) typeRef._name = typeRef._name.Substring(nsIndex+1);
							//else throw new TypeLoadException($"Failed to find class {typeName}");
							else break;
						}
					}
					if(!importSuccess)
					{
						baseType._baseMeta = igArkCore.GetObjectMeta("Object");	//Should replace this with DotNet.Object._Meta when that's implemented
						baseType._flags = (uint)ElementType.kElementTypeObject;
					}
					if(baseType._baseMeta is not igMetaObject baseMetaObject)
					{
						throw new TypeLoadException($"Somehow didn't get a metaobject for the parent of {baseType._baseMeta}, instead got {baseType._baseMeta._name}");
					}
					else
					{
						if(baseMetaObject._parent == null)
						{
							incompleteObjects.Append(metaIndex);
							continue;
						}
						meta._parent = baseMetaObject;
						meta.InheritFields();
						for(int j = 0; j < memberCounts[metaIndex]; j++)
						{
							meta.AppendDynamicField(AddField(library, resolver, fieldDefs[memberStarts[metaIndex] + j], strings));
						}
						library._ownedTypes.Append(meta);
					}
				}
			}

			library._referencedTypes.SetCapacity(typeHeader._referencedTypeCount);
			for(int i = 0; i < typeHeader._referencedTypeCount; i++)
			{
				bTypeDetails.Seek(typeHeader._referencedTypeOffset + i * 0xC);
				igDotNetTypeReference dntr = new igDotNetTypeReference();
				dntr._resolver = resolver;
				dntr._elementType = (ElementType)bTypeDetails.ReadUInt32();
				dntr._isArray = bTypeDetails.ReadUInt32() == 1;
				resolver._stringTable.Seek(bTypeDetails.ReadUInt32());
				dntr._name = resolver._stringTable.ReadString();

				DotNetType dnt = new DotNetType();

				if(dntr._elementType == ElementType.kElementTypeObject && !dntr._isArray)
				{
					dntr.TryResolveObject(out dnt);
				}
				else
				{
					if(dntr._elementType != ElementType.kElementTypeObject) dnt._flags = (uint)DotNetType.Flags.kIsSimple;
					dnt._flags = ((uint)dntr._elementType) | (dntr._isArray ? (uint)DotNetType.Flags.kIsArray : 0);
				}
				library._referencedTypes.Append(dnt);
			}

			for(int i = 0; i < typeDetails._count; i++)
			{
				if(typeDetails[i]._targetMeta is not igDotNetTypeSpecMetaObject metaObject) continue;

				metaObject._templateParameters = new DotNetTypeList();
				metaObject._templateParameters.SetCapacity(typeDetails[i]._templateParameterCount);
				for(int j = 0; j < typeDetails[i]._templateParameterCount; j++)
				{
					metaObject._templateParameters.Append(library._referencedTypes[typeDetails[i]._templateParameterOffset + j]);
				}
			}
			success = true;
			//CDotNetaManager._Instance._libraries.Add(libName, library);
			return library;
		}
		private static string ReadVvlString(StreamHelper strings, uint offset)
		{
			strings.Seek(offset);
			return strings.ReadString();
		}

		public static DotNetParameterMeta ConvertParameterMeta(VvlParameterMeta paramMeta, VvlAttribute[] attributes, StreamHelper strings)
		{
			DotNetParameterMeta dnpm = new DotNetParameterMeta();
			dnpm._name = ReadVvlString(strings, paramMeta._name);
			//TODO: Read Attributes
			return dnpm;
		}
		public static DotNetTypeDetails ConvertTypeDetails(igDotNetLoadResolver resolver, VvlTypeDetails vvltd, StreamHelper strings, VvlFieldDefinition[] fields, VvlAttribute[] attributeMetas, StreamHelper bTypeDetails)
		{
			//Should probably put in checks to verify that the metaobject/metaenum is what we expect it to be

			DotNetTypeDetails dntd = new DotNetTypeDetails();
			dntd._interfaceOffset = vvltd._interfaceOffset;
			dntd._interfaceCount = vvltd._interfaceCount;
			string typeName = ReadVvlString(strings, vvltd._name).TrimStart('.');
			if(resolver._runtime._prefix != null)
			{
				typeName = resolver._runtime._prefix + typeName;
			}

			igBaseMeta? meta = null;

			if(vvltd._elementType == ElementType.kElementTypeI4)	//If enum
			{
				if((vvltd._typeFlag & 1) == 0)	//If owned
				{
					igDotNetDynamicMetaEnum dndme = new igDotNetDynamicMetaEnum();
					meta = dndme;
					dndme._flags = false;
					dndme._name = typeName;
					dndme._values.Capacity = vvltd._memberCount;
					dndme._names.Capacity = vvltd._memberCount;
					for(int i = 0; i < vvltd._memberCount; i++)
					{
						VvlFieldDefinition field = fields[vvltd._fieldStartIndex + i];
						dndme._names.Add(ReadVvlString(strings, field._name));
						dndme._values.Add(field._default);
					}
					resolver.AppendMeta(dndme);
				}
				else
				{
					dntd._ownsMeta = false;
					while(true)
					{
						meta = igArkCore.GetMetaEnum(typeName);
						if(meta == null)
						{
							int nsIndex = typeName.IndexOf('.');
							if(nsIndex >= 0) typeName = typeName.Substring(nsIndex+1);
							//else throw new TypeLoadException($"Failed to find enum {typeName}");
							else break;
							continue;
						}
						else break;
					}
				}

				if(meta != null && typeName != meta._name)
				{
					resolver._aliases.TryAdd(typeName, meta._name);
				}
			}
			else	//If metaobject
			{
				if((vvltd._typeFlag & 1) == 0)	//If owned
				{
					igDotNetDynamicMetaObject dndmo = new igDotNetDynamicMetaObject();
					dndmo._name = typeName;
					meta = dndmo;
					resolver.AppendMeta(dndmo);
				}
				else
				{
					dntd._ownsMeta = false;
					bool successfulImport = false;
					igDotNetTypeReference typeRef = new igDotNetTypeReference(resolver, false, ElementType.kElementTypeObject, typeName);
					while(!successfulImport)
					{
						successfulImport = typeRef.TryResolveObject(out DotNetType dnt);
						if(!successfulImport)
						{
							int nsIndex = typeRef._name.IndexOf('.');
							if(nsIndex >= 0) typeRef._name = typeRef._name.Substring(nsIndex+1);
							//else throw new TypeLoadException($"Failed to find class {typeName}");
							else break;
						}
						else
						{
							meta = dnt._baseMeta;
							typeName = typeRef._name;
							break;
						}
					}
				}
			}

			//if(meta == null) throw new TypeLoadException($"Somehow failed to load {typeName}, I don't really know how it got past the previous checks");
			if(meta == null)
			{
				Console.Error.WriteLine($"FAILED TO LOAD {typeName} SOMETIMES THIS IS MEANT TO HAPPEN IDK");
				return null;
			}

			dntd._targetMeta = meta;
			
			if(typeName != meta._name)
			{
				resolver._aliases.TryAdd(typeName, meta._name);
			}

			if(meta is igMetaObject metaObject)
			{
				igDotNetTypeReference baseTypeRef = new igDotNetTypeReference(resolver, vvltd._isArray != 0, vvltd._elementType, ReadVvlString(strings, vvltd._typeReferenceName));
				dntd._baseType = baseTypeRef;
				if(dntd._baseType.TryResolveObject(out DotNetType baseDnt))
				{
					metaObject._parent = (igMetaObject)baseDnt._baseMeta;
				}
			}

			//TODO: Read attributes

			return dntd;
		}

		private static unsafe igMetaField AddField(DotNetLibrary library, igDotNetLoadResolver resolver, VvlFieldDefinition field, StreamHelper strings)
		{
			igMetaField metaField = null;
			if(field._isArray != 0)
			{
				switch(field._fieldType)
				{
					case ElementType.kElementTypeBoolean:
						metaField = new igBoolArrayMetaField();
						((igBoolArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = false;
						break;
					case ElementType.kElementTypeI1:
						metaField = new igCharArrayMetaField();
						((igCharArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (sbyte)0;
						break;
					case ElementType.kElementTypeU1:
						metaField = new igUnsignedCharArrayMetaField();
						((igUnsignedCharArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (byte)0;
						break;
					case ElementType.kElementTypeI2:
						metaField = new igShortArrayMetaField();
						((igShortArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (short)0;
						break;
					case ElementType.kElementTypeU2:
						metaField = new igUnsignedShortArrayMetaField();
						((igUnsignedShortArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (ushort)0;
						break;
					case ElementType.kElementTypeI4:
					case ElementType.kElementTypeValueType:
						metaField = new igIntArrayMetaField();
						((igIntArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (int)0;
						break;
					case ElementType.kElementTypeU4:
						metaField = new igUnsignedIntArrayMetaField();
						((igUnsignedIntArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (uint)0;
						break;
					case ElementType.kElementTypeI8:
						metaField = new igLongArrayMetaField();
						((igLongArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (long)0;
						break;
					case ElementType.kElementTypeU8:
						metaField = new igUnsignedLongArrayMetaField();
						((igUnsignedLongArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (ulong)0;
						break;
					case ElementType.kElementTypeR4:
						metaField = new igFloatArrayMetaField();
						((igFloatArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (float)0;
						break;
					case ElementType.kElementTypeString:
						metaField = new igStringArrayMetaField();
						((igStringArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = null;
						break;
					case ElementType.kElementTypeClass:
					case ElementType.kElementTypeObject:
						if(field._isHandle == 0)
						{
							metaField = new igObjectRefArrayMetaField();
							((igObjectRefArrayMetaField)metaField)._num = (short)field._default;
							((igObjectRefArrayMetaField)metaField)._metaObject = igArkCore.GetObjectMeta("igObject");	//replace this with Core.igObject._Meta once that's implemented

						}
						else
						{
							metaField = new igHandleArrayMetaField();
							((igHandleArrayMetaField)metaField)._num = (short)field._default;
						}
						metaField._default = null;
						break;
				}
			}
			else
			{
				switch(field._fieldType)
				{
					case ElementType.kElementTypeBoolean:
						metaField = new igBoolMetaField();
						//metaField._default = *(bool*)&field._default;
						break;
					case ElementType.kElementTypeI1:
						metaField = new igCharMetaField();
						//metaField._default = *(sbyte*)&field._default;
						break;
					case ElementType.kElementTypeU1:
						metaField = new igUnsignedCharMetaField();
						//metaField._default = *(byte*)&field._default;
						break;
					case ElementType.kElementTypeI2:
						metaField = new igShortMetaField();
						//metaField._default = *(short*)&field._default;
						break;
					case ElementType.kElementTypeU2:
						metaField = new igUnsignedShortMetaField();
						//metaField._default = *(ushort*)&field._default;
						break;
					case ElementType.kElementTypeI4:
					case ElementType.kElementTypeValueType:
						metaField = new igIntMetaField();
						//metaField._default = (int)field._default;
						break;
					case ElementType.kElementTypeU4:
						metaField = new igUnsignedIntMetaField();
						//metaField._default = (uint)field._default;
						break;
					case ElementType.kElementTypeI8:
						metaField = new igLongMetaField();
						//metaField._default = (long)0;
						break;
					case ElementType.kElementTypeU8:
						metaField = new igUnsignedLongMetaField();
						//metaField._default = (ulong)0;
						break;
					case ElementType.kElementTypeR4:
						metaField = new igFloatMetaField();
						//metaField._default = (float)0;
						break;
					case ElementType.kElementTypeString:
						metaField = new igStringMetaField();
						//metaField._default = null;
						break;
					case ElementType.kElementTypeClass:
					case ElementType.kElementTypeObject:
						bool importSuccess = false;
						igDotNetTypeReference typeRef = new igDotNetTypeReference(resolver, field._isArray != 0, field._fieldType, ReadVvlString(strings, field._refTypeName));
						DotNetType dntRef = new DotNetType();
						while(!importSuccess)
						{
							importSuccess = typeRef.TryResolveObject(out dntRef);
							if(!importSuccess)
							{
								int nsIndex = typeRef._name.IndexOf('.');
								if(nsIndex >= 0) typeRef._name = typeRef._name.Substring(nsIndex+1);
								//else throw new TypeLoadException($"Failed to find class {typeName}");
								else break;
							}
						}
						if(!importSuccess)
						{
							dntRef._baseMeta = igArkCore.GetObjectMeta("Object");
						}

						if(dntRef._baseMeta is igMetaEnum metaEnum)
						{
							metaField = new igDotNetEnumMetaField();
							((igDotNetEnumMetaField)metaField)._definedMetaEnum = metaEnum;
						}
						else if(field._isHandle == 0)
						{
							metaField = new igObjectRefMetaField();
							((igObjectRefMetaField)metaField)._metaObject = (igMetaObject)dntRef._baseMeta;	//replace this with Core.igObject._Meta once that's implemented
							//default is handled with a handle? didn't expect that
						}
						else
						{
							metaField = new igHandleMetaField();
							((igHandleMetaField)metaField)._metaObject = (igMetaObject)dntRef._baseMeta;	//replace this with Core.igObject._Meta once that's implemented
						}
						//metaField._default = null;
						break;
				}
			}
			metaField._name = ReadVvlString(strings, field._name);
			if(metaField._name[0] == '<')	//Why would this happen??
			{
				metaField._name = metaField._name.Substring(1);
				//Ignoring the rest for now
			}

			//Read attributes
			return metaField;
		}

		[StructLayout(LayoutKind.Explicit, Size = 0x28)]
		public struct VvlHeader
		{
			[FieldOffset(0x00)] public uint unk00;
			[FieldOffset(0x04)] public uint _sizeofSize;
			[FieldOffset(0x08)] public uint _stringTableLength;
			[FieldOffset(0x0C)] public uint _novaUnkCount;
			[FieldOffset(0x10)] public uint _fieldCount;
			[FieldOffset(0x14)] public uint _methodCount;
			[FieldOffset(0x18)] public uint _parameterCount;
			[FieldOffset(0x1C)] public uint _attributeCount;
			[FieldOffset(0x20)] public uint unk20;
			[FieldOffset(0x24)] public uint _typeDetailLength;
		}
		[StructLayout(LayoutKind.Explicit, Size = 0x18)]
		public struct SDotNetMethodMeta
		{
			[FieldOffset(0x00)] public uint _sizeofSize;
			[FieldOffset(0x04)] public uint _importTag;
			[FieldOffset(0x08)] public uint _methodName;
			[FieldOffset(0x0C)] public uint _entryPoint;
			[FieldOffset(0x10)] public uint _retParamIndex;
			[FieldOffset(0x14)] public uint _paramIndex;
		}
	[StructLayout(LayoutKind.Explicit, Size = 0x54)]
	public struct VvlMethodDef
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _declaringTypeName;
		[FieldOffset(0x10)] public uint _methodName;
		[FieldOffset(0x20)] public int _paramCount;
		[FieldOffset(0x28)] public int _stackHeight;
		[FieldOffset(0x2C)] public ElementType _returnElementType;
		[FieldOffset(0x30)] public int _isReturnArray;
		[FieldOffset(0x34)] public uint _returnTypeName;
		[FieldOffset(0x40)] public int _paramStartIndex;
		[FieldOffset(0x48)] public uint _ILOffset;
		[FieldOffset(0x4C)] public uint _ILCount;
		[FieldOffset(0x50)] public short _flags;
		[FieldOffset(0x52)] public short _methodIndex;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x30)]
	public struct VvlFieldDefinition
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _name;
		[FieldOffset(0x08)] public uint _isHandle;
		[FieldOffset(0x20)] public ElementType _fieldType;
		[FieldOffset(0x24)] public uint _isArray;
		[FieldOffset(0x28)] public uint _refTypeName;
		[FieldOffset(0x18)] public int _default;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x10)]
	public struct VvlParameterMeta
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _name;
		[FieldOffset(0x08)] public uint _attributeIndex;
		[FieldOffset(0x0C)] public uint _attributeCount;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x14)]
	public struct VvlAttribute
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _name;
		[FieldOffset(0x0C)] public uint unk0C;
		[FieldOffset(0x10)] public uint unk10;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x2C)]
	public struct VvlTypeDetails
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public ElementType _elementType;
		[FieldOffset(0x08)] public uint _isArray;
		[FieldOffset(0x0C)] public uint _typeReferenceName;
		[FieldOffset(0x10)] public int _interfaceOffset;
		[FieldOffset(0x14)] public int _interfaceCount;
		[FieldOffset(0x18)] public uint _name;
		[FieldOffset(0x1C)] public ushort _typeFlag;
		[FieldOffset(0x1E)] public ushort _memberCount;
		[FieldOffset(0x20)] public uint _fieldStartIndex;
		[FieldOffset(0x24)] public uint _attributeIndex;
		[FieldOffset(0x28)] public uint _attributeCount;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x1C)]
	public struct VvlGenericTypeDetails
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _typeSpecName;
		[FieldOffset(0x08)] public ElementType _elementType;
		[FieldOffset(0x0C)] public uint _isArray;
		[FieldOffset(0x10)] public uint _someTypeReference;
		[FieldOffset(0x14)] public int _templateParameterOffset;
		[FieldOffset(0x18)] public int _templateParameterCount;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x50)]
	public struct VvlTypeDetailHeader
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _stringTableLength;
		[FieldOffset(0x0C)] public uint _someOffset;
		[FieldOffset(0x10)] public uint _ILOffset;
		[FieldOffset(0x14)] public uint _ILCount;
		[FieldOffset(0x18)] public uint _methodDefOffset;
		[FieldOffset(0x1C)] public uint _methodCount;
		[FieldOffset(0x24)] public uint _ownedTypeOffset;
		[FieldOffset(0x28)] public int _ownedTypeCount;
		[FieldOffset(0x2C)] public uint _referencedTypeOffset;
		[FieldOffset(0x30)] public int _referencedTypeCount;
		[FieldOffset(0x34)] public uint _staticFieldOffset;
		[FieldOffset(0x38)] public int _staticFieldCount;
		[FieldOffset(0x44)] public uint _genericTypeOffset;
		[FieldOffset(0x48)] public int _genericTypeCount;
	}
	}
}