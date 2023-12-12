using Mono.Cecil;
using Mono.Cecil.Cil;
using igLibrary.Core;
using igLibrary.DotNet;
using igLibrary;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace VvlToDll
{
	public class DllExporter
	{
		private class InstructionPlaceholder
		{
			public OpCode _opcode;
			public object? _operand;
			public InstructionPlaceholder(OpCode opcode)
			{
				_opcode = opcode;
			}
		}

		private static bool _initialized = false;

		private ModuleDefinition _module;
		private DotNetLibrary _library;
		private List<DotNetLibrary> _dependencies = new List<DotNetLibrary>();
		private Dictionary<igBaseMeta, TypeDefinition> _metaTypeLookup = new Dictionary<igBaseMeta, TypeDefinition>();
		private Dictionary<DotNetMethodDefinition, MethodDefinition> _methodLookup = new Dictionary<DotNetMethodDefinition, MethodDefinition>();
		private Dictionary<DotNetMethodDefinition, MethodReference> _methodRefLookup = new Dictionary<DotNetMethodDefinition, MethodReference>();

		private static void Init()
		{
			if(_initialized) return;
			_initialized = true;
		}
		public DllExporter(DotNetLibrary library)
		{
			_module = ModuleDefinition.CreateModule(Path.GetFileNameWithoutExtension(library._path), ModuleKind.Dll);
			_library = library;
		}
		public void ExportLibrary(DotNetLibrary library, string name)
		{
			_module = ModuleDefinition.CreateModule(name, ModuleKind.Dll);
			//_module.ModuleReferences.Add(ArkDllExport.module);
			_library = library;
			//module.ModuleReferences.Add(mscorlib);
			CreateTypes();
			DefineEnums();
			DefineObjects();
			_module.Write(name);
		}
		public void CreateTypes()
		{
			foreach(igBaseMeta meta in _library._ownedTypes)
			{
				if(meta == null) continue;
				GetNsAndName(in meta._name, out string ns, out string name);
				TypeDefinition td = new TypeDefinition(ns, name, TypeAttributes.Public);
				if(!_metaTypeLookup.TryAdd(meta, td) && meta is not igMetaEnum) throw new ArgumentException("Type exists boss."); 	//So many enums are called just "Flags" or "Mode" that I had to make this 
				_module.Types.Add(td);
			}
		}
		public TypeReference GetTypeFromMeta(igBaseMeta meta)
		{
			//return _module.ImportReference(typeof(object));
			if(ArkDllExport._metaTypeLookup.TryGetValue(meta, out TypeDefinition td)) return _module.ImportReference(td);
			if(_metaTypeLookup.TryGetValue(meta, out td)) return _module.ImportReference(td);
			else return ImportVvlClassRef(meta);
			throw new KeyNotFoundException("Failed to load type");
		}
		public void DefineEnums()
		{
			foreach(KeyValuePair<igBaseMeta, TypeDefinition> kvp in _metaTypeLookup)
			{
				if(kvp.Key is not igMetaEnum metaEnum) continue;
				
				TypeDefinition td = kvp.Value;
				td.BaseType = _module.ImportReference(typeof(Enum));
				td.Fields.Add(new FieldDefinition("__value", FieldAttributes.Public | FieldAttributes.SpecialName, _module.ImportReference(typeof(int))));
				for(int i = 0; i < metaEnum._names.Count; i++)
				{
					td.Fields.Add(new FieldDefinition(metaEnum._names[i], FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal, td));
					td.Fields.Last().Constant = metaEnum._values[i];
				}
			}
		}
		public void DefineObjects()
		{
			foreach(KeyValuePair<igBaseMeta, TypeDefinition> kvp in _metaTypeLookup)
			{
				if(kvp.Key is not igMetaObject metaObject) continue;

				TypeDefinition td = kvp.Value;
				if(metaObject._parent != null)
				{
					td.BaseType = GetTypeFromMeta(metaObject._parent);
				}
				else continue;

				for(int i = metaObject._parent._metaFields.Count; i < metaObject._metaFields.Count; i++)
				{
					//FieldDefinition fd = new FieldDefinition(metaObject._metaFields[i]._name, FieldAttributes.Public, _module.ImportReference(typeof(object)));
					//td.Fields.Add(fd);

					ArkDllExport.AddField(_module, td, metaObject, metaObject._metaFields[i], GetTypeFromMeta);
				}
			}
		}
		public MethodReference ImportVvlMethodRef(uint callToken)
		{
			DotNetMethodDefinition dnMethod = _library.LookupMethod(callToken);
			if((callToken & 1) == 0)
			{
				return _methodLookup[dnMethod];
			}
			else
			{
				return _methodRefLookup[dnMethod];
			}
		}
		public FieldReference ImportVvlFieldRef(int fieldToken)
		{
			igMetaField field = _library._fields[fieldToken];
			return _module.ImportReference(new FieldReference(field._name, ArkDllExport.GetFieldTypeRef(_module, field, GetTypeFromMeta), ImportVvlClassRef(field._parentMeta)));
		}
		public TypeReference ImportVvlTypeRef(DotNetType dnTypeRef)
		{
			switch((ElementType)(dnTypeRef._flags & (uint)DotNetType.Flags.kTypeMask))
			{
				default:
					throw new ArgumentException("Yo what???");
				case ElementType.kElementTypeVoid:
					return _module.TypeSystem.Void;
				case ElementType.kElementTypeBoolean:
					return _module.TypeSystem.Boolean;
				case ElementType.kElementTypeChar:
					return _module.TypeSystem.Char;
				case ElementType.kElementTypeI1:
					return _module.TypeSystem.SByte;
				case ElementType.kElementTypeU1:
					return _module.TypeSystem.Byte;
				case ElementType.kElementTypeI2:
					return _module.TypeSystem.Int16;
				case ElementType.kElementTypeU2:
					return _module.TypeSystem.UInt16;
				case ElementType.kElementTypeI4:
					return _module.TypeSystem.Int32;
				case ElementType.kElementTypeU4:
					return _module.TypeSystem.UInt32;
				case ElementType.kElementTypeI8:
					return _module.TypeSystem.Int64;
				case ElementType.kElementTypeU8:
					return _module.TypeSystem.UInt64;
				case ElementType.kElementTypeR4:
					return _module.TypeSystem.Single;
				case ElementType.kElementTypeR8:
					return _module.TypeSystem.Double;
				case ElementType.kElementTypeString:
					return _module.TypeSystem.String;
				case ElementType.kElementTypeObject:
					return ImportVvlClassRef(dnTypeRef._baseMeta);
			}
		}
		private TypeReference ImportVvlClassRef(igBaseMeta meta)
		{
			DotNetLibrary? owner = null;
			if(meta is igDotNetDynamicMetaEnum dndme)
			{
				owner = dndme._owner;
			}
			else if(meta is igDotNetDynamicMetaObject dndmo)
			{
				owner = dndmo._owner;
			}
			else if(meta is igMetaObject || meta is igMetaEnum)
			{
				return _module.ImportReference(ArkDllExport._metaTypeLookup[meta]);
			}
			if(owner == null) return _module.TypeSystem.Object;	//This is bad, this happening indicates that the vvls were not loaded properly

			if(!_dependencies.Contains(owner))
			{
				_dependencies.Add(owner);
			}

			DllExporter dependentExporter = DllExportManager._libExporterLookup[owner];
			return _module.ImportReference(dependentExporter._metaTypeLookup[meta]);
		}
		public void DeclareMethods()
		{
			foreach(DotNetMethodDefinition? dnMethodRef in _library._methodRefs)
			{
				if(dnMethodRef == null) continue;
				MethodReference mr = new MethodReference(dnMethodRef._name, ImportVvlTypeRef(dnMethodRef._retType), ImportVvlTypeRef(dnMethodRef._declaringType));
				_module.ImportReference(mr);
				_methodRefLookup.Add(dnMethodRef, mr);
			}
			foreach(DotNetMethodDefinition? dnMethodDef in _library._methodDefs)
			{
				if(dnMethodDef == null) continue;

				TypeDefinition td = _metaTypeLookup[dnMethodDef._declaringType._baseMeta];
				MethodDefinition md = new MethodDefinition(dnMethodDef._name, MethodAttributes.Public, ImportVvlTypeRef(dnMethodDef._retType));
				
				if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.Constructor) != 0) md.Attributes |= MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;
				if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.StaticMethod) != 0) md.Attributes |= MethodAttributes.Static;
				if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.AbstractMethod) != 0) md.Attributes |= MethodAttributes.Abstract;
				//if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.RuntimeImplMethod) != 0) md.Attributes |= MethodAttributes.;
				//if((dnMethodDef._flags & (uint)DotNetMethodSignature.FlagTypes.NoSpecializationCopyMethod) != 0) md.Attributes |= MethodAttributes.;

				//If it's not static then param 1 is "this"
				for(int i = 0; i < dnMethodDef._parameters._count; i++)
				{
					ParameterDefinition pd = new ParameterDefinition(ImportVvlTypeRef(dnMethodDef._parameters[i]));
					pd.Name = dnMethodDef._methodMeta._parameters[i]._name;
					md.Parameters.Add(pd);
				}

				if(md.HasBody)
				{
					md.Body.MaxStackSize = dnMethodDef._stackHeight;
					for(int i = 0; i < dnMethodDef._locals._count; i++)
					{
						md.Body.Variables.Add(new VariableDefinition(ImportVvlTypeRef(dnMethodDef._locals[i])));
					}
					ILProcessor il = md.Body.GetILProcessor();
					il.Append(il.Create(OpCodes.Ret));
				}

				_methodLookup.Add(dnMethodDef, md);
				td.Methods.Add(md);
			}
		}
		public void DefineMethods()
		{
			foreach(KeyValuePair<DotNetMethodDefinition, MethodDefinition> kvp in _methodLookup)
			{
				if((kvp.Value.Attributes & MethodAttributes.Abstract) == 0)
				{
					DumpOpCodes(kvp.Key._IL, kvp.Value);
				}
			}
		}
		private static void GetNsAndName(in string qualifiedName, out string ns, out string name)
		{
			int checkGeneric = qualifiedName.IndexOf('`');
			int genericTestIndex = checkGeneric < 0 ? qualifiedName.Length-1 : checkGeneric;	//Poorly named
			int nsEnd = qualifiedName.LastIndexOf('.', genericTestIndex, genericTestIndex);
			if(nsEnd > 0)
			{
				ns = qualifiedName.Substring(0, nsEnd);
				name = qualifiedName.Substring(nsEnd+1);
			}
			else
			{
				ns = "-";
				name = qualifiedName;
			}
		}
		private unsafe int GetI4FromIl(bool isLittleEndian, byte* bytecode)
		{
			if(_library._isLittleEndian == BitConverter.IsLittleEndian)
			{
				return *(int*)bytecode;
			}
			else
			{
				return (*(bytecode + 3) << 24) | (*(bytecode + 2) << 16) | (*(bytecode + 1) << 8) | (*bytecode);
			}
		}
		private unsafe uint GetU4FromIl(bool isLittleEndian, byte* bytecode) => unchecked((uint)GetI4FromIl(isLittleEndian, bytecode));
		private unsafe float GetR4FromIl(bool isLittleEndian, byte* bytecode)
		{
			int i4 = GetI4FromIl(isLittleEndian, bytecode);
			return *(float*)&i4;
		}
		private unsafe long GetI8FromIl(bool isLittleEndian, byte* bytecode)
		{
			if(_library._isLittleEndian == BitConverter.IsLittleEndian)
			{
				return *(long*)bytecode;
			}
			else
			{
				return (*(bytecode + 7) << 56) | (*(bytecode + 6) << 48) | (*(bytecode + 5) << 40) | (*(bytecode + 4) << 32) | (*(bytecode + 3) << 24) | (*(bytecode + 2) << 16) | (*(bytecode + 1) << 8) | (*bytecode);
			}
		}
		private unsafe ulong GetU8FromIl(bool isLittleEndian, byte* bytecode) => unchecked((ulong)GetI8FromIl(isLittleEndian, bytecode));
		private unsafe double GetR8FromIl(bool isLittleEndian, byte* bytecode)
		{
			long i4 = GetI8FromIl(isLittleEndian, bytecode);
			return *(double*)&i4;
		}
		private void DumpOpCodes(igVector<byte> il, MethodDefinition method)
		{
			StreamHelper ilStream = new StreamHelper(il._data.ToArray(), _library._isLittleEndian ? StreamHelper.Endianness.Little : StreamHelper.Endianness.Big);
			Dictionary<int, InstructionPlaceholder> instructions = new Dictionary<int, InstructionPlaceholder>();
			while(ilStream.Tell() < ilStream.BaseStream.Length)
			{
				int pos = (int)ilStream.Tell();
				int debugpos = pos;
				try
				{
					instructions.Add(pos, DumpOpCode(method, pos, ilStream));
				}
				catch(InvalidDataException e)
				{
					StringBuilder methodBody = new StringBuilder((int)il._count * 2);
					for(int i = 0; i < il._count; i++)
					{
						methodBody.Append(il[i].ToString("X02"));
					}
					throw new Exception($"Failed to disassemble method {method.Name} due to {e.GetType().Name} due to \"{e.Message}\"\n\nMethod Body:\n{methodBody}\n\nOpCode Position: {debugpos.ToString("X08")}", e);
				}
			}
			ILProcessor ilProcessor = method.Body.GetILProcessor();
			Dictionary<int, Instruction> realInstructions = new Dictionary<int, Instruction>();
			foreach(KeyValuePair<int, InstructionPlaceholder> kvp in instructions)
			{
				Instruction inst;
				switch(kvp.Value._opcode.OperandType)
				{
					case OperandType.InlineBrTarget:
					case OperandType.ShortInlineBrTarget:
						inst = Instruction.Create(kvp.Value._opcode, Instruction.Create(OpCodes.Nop));	//I'm so sad about this
						break;
					case OperandType.InlineField:
						inst = Instruction.Create(kvp.Value._opcode, (FieldReference)kvp.Value._operand);
						break;
					case OperandType.InlineI:
						inst = Instruction.Create(kvp.Value._opcode, (int)kvp.Value._operand);
						break;
					case OperandType.InlineI8:
						inst = Instruction.Create(kvp.Value._opcode, (long)kvp.Value._operand);
						break;
					case OperandType.InlineMethod:
						inst = Instruction.Create(kvp.Value._opcode, (MethodReference)kvp.Value._operand);
						break;
					case OperandType.InlineNone:
						inst = Instruction.Create(kvp.Value._opcode);
						break;
					case OperandType.InlineR:
						inst = Instruction.Create(kvp.Value._opcode, (double)kvp.Value._operand);
						break;
					case OperandType.InlineString:
						inst = Instruction.Create(kvp.Value._opcode, (string)kvp.Value._operand);
						break;
					case OperandType.InlineSwitch:
						int[] offsets = (int[]) kvp.Value._operand;
						Instruction[] branches = new Instruction[offsets.Length];
						inst = Instruction.Create(kvp.Value._opcode, branches);
						break;
					case OperandType.InlineTok:
					case OperandType.InlineType:
						inst = Instruction.Create(kvp.Value._opcode, (TypeReference)kvp.Value._operand);
						break;
					case OperandType.InlineVar:
					case OperandType.ShortInlineVar:
						inst = Instruction.Create(kvp.Value._opcode, (VariableDefinition)kvp.Value._operand);
						break;
					case OperandType.InlineArg:
					case OperandType.ShortInlineArg:
						inst = Instruction.Create(kvp.Value._opcode, (ParameterDefinition)kvp.Value._operand);
						break;
					case OperandType.ShortInlineI:
						inst = Instruction.Create(kvp.Value._opcode, (sbyte)kvp.Value._operand);
						break;
					case OperandType.ShortInlineR:
						inst = Instruction.Create(kvp.Value._opcode, (float)kvp.Value._operand);
						break;
					default:
						throw new NotImplementedException("Failed to create instruction");
				}
				realInstructions.Add(kvp.Key, inst);
			}
			foreach(KeyValuePair<int, Instruction> kvp in realInstructions)
			{
				switch(kvp.Value.OpCode.OperandType)
				{
					case OperandType.InlineBrTarget:
					case OperandType.ShortInlineBrTarget:
						kvp.Value.Operand = realInstructions[(int)instructions[kvp.Key]._operand];
						break;
					case OperandType.InlineSwitch:
						Instruction[] branches = (Instruction[])kvp.Value.Operand;
						for(int i = 0; i < branches.Length; i++)
						{
							branches[i] = realInstructions[((int[])instructions[kvp.Key]._operand)[i]];
						}
						break;
				}
				ilProcessor.Append(kvp.Value);
			}
		}
		private static InvalidDataException GetRemovedOpCodeException(string opcodeName) => new InvalidDataException($"{opcodeName} is not permitted in VVL IL");
		private static OpCode GetOpCode(StreamHelper ilStream)
		{
			byte op = ilStream.ReadByte();
			switch(op)
			{
				case 0x00: return OpCodes.Nop;
				case 0x01: return OpCodes.Break;
				case 0x02: return OpCodes.Ldarg_0;
				case 0x03: return OpCodes.Ldarg_1;
				case 0x04: return OpCodes.Ldarg_2;
				case 0x05: return OpCodes.Ldarg_3;
				case 0x06: return OpCodes.Ldloc_0;
				case 0x07: return OpCodes.Ldloc_1;
				case 0x08: return OpCodes.Ldloc_2;
				case 0x09: return OpCodes.Ldloc_3;
				case 0x0A: return OpCodes.Stloc_0;
				case 0x0B: return OpCodes.Stloc_1;
				case 0x0C: return OpCodes.Stloc_2;
				case 0x0D: return OpCodes.Stloc_3;
				case 0x0E: return OpCodes.Ldarg_S;
				case 0x0F: return OpCodes.Ldarga_S;
				case 0x10: return OpCodes.Starg_S;
				case 0x11: return OpCodes.Ldloc_S;
				case 0x12: return OpCodes.Ldloca_S;
				case 0x13: return OpCodes.Stloc_S;
				case 0x14: return OpCodes.Ldnull;
				case 0x15: return OpCodes.Ldc_I4_M1;
				case 0x16: return OpCodes.Ldc_I4_0;
				case 0x17: return OpCodes.Ldc_I4_1;
				case 0x18: return OpCodes.Ldc_I4_2;
				case 0x19: return OpCodes.Ldc_I4_3;
				case 0x1A: return OpCodes.Ldc_I4_4;
				case 0x1B: return OpCodes.Ldc_I4_5;
				case 0x1C: return OpCodes.Ldc_I4_6;
				case 0x1D: return OpCodes.Ldc_I4_7;
				case 0x1E: return OpCodes.Ldc_I4_8;
				case 0x1F: return OpCodes.Ldc_I4_S;
				case 0x20: return OpCodes.Ldc_I4;
				case 0x21: return OpCodes.Ldc_I8;
				case 0x22: return OpCodes.Ldc_R4;
				case 0x23: throw GetRemovedOpCodeException("ldc.r8");
				case 0x25: return OpCodes.Dup;
				case 0x26: return OpCodes.Pop;
				case 0x27: throw GetRemovedOpCodeException("jmp");
				case 0x28: return OpCodes.Call;
				case 0x29: throw GetRemovedOpCodeException("calli");
				case 0x2A: return OpCodes.Ret;
				case 0x2B: return OpCodes.Br_S;
				case 0x2C: return OpCodes.Brfalse_S;
				case 0x2D: return OpCodes.Brtrue_S;
				case 0x2E: return OpCodes.Beq_S;
				case 0x2F: return OpCodes.Bge_S;
				case 0x30: return OpCodes.Bgt_S;
				case 0x31: return OpCodes.Ble_S;
				case 0x32: return OpCodes.Blt_S;
				case 0x33: return OpCodes.Bne_Un_S;
				case 0x34: return OpCodes.Bge_Un_S;
				case 0x35: return OpCodes.Bgt_Un_S;
				case 0x36: return OpCodes.Ble_Un_S;
				case 0x37: return OpCodes.Blt_Un_S;
				case 0x38: return OpCodes.Br;
				case 0x39: return OpCodes.Brfalse;
				case 0x3A: return OpCodes.Brtrue;
				case 0x3B: return OpCodes.Beq;
				case 0x3C: return OpCodes.Bge;
				case 0x3D: return OpCodes.Bgt;
				case 0x3E: return OpCodes.Ble;
				case 0x3F: return OpCodes.Blt;
				case 0x40: return OpCodes.Bne_Un;
				case 0x41: throw GetRemovedOpCodeException("bge.un");
				case 0x42: return OpCodes.Bgt_Un;
				case 0x43: return OpCodes.Ble_Un;
				case 0x44: return OpCodes.Blt_Un;
				case 0x45: return OpCodes.Switch;
				case 0x46: return OpCodes.Ldind_I1;
				case 0x47: return OpCodes.Ldind_U1;
				case 0x48: return OpCodes.Ldind_I2;
				case 0x49: return OpCodes.Ldind_U2;
				case 0x4A: return OpCodes.Ldind_I4;
				case 0x4B: return OpCodes.Ldind_U4;
				case 0x4C: return OpCodes.Ldind_I8;
				case 0x4D: return OpCodes.Ldind_I;
				case 0x4E: return OpCodes.Ldind_R4;
				case 0x4F: throw GetRemovedOpCodeException("ldind.r8");
				case 0x50: return OpCodes.Ldind_Ref;
				case 0x51: return OpCodes.Stind_Ref;
				case 0x52: return OpCodes.Stind_I1;
				case 0x53: return OpCodes.Stind_I2;
				case 0x54: return OpCodes.Stind_I4;
				case 0x55: return OpCodes.Stind_I8;
				case 0x56: return OpCodes.Stind_R4;
				case 0x57: throw GetRemovedOpCodeException("stind.r8");
				case 0x58: return OpCodes.Add;
				case 0x59: return OpCodes.Sub;
				case 0x5A: return OpCodes.Mul;
				case 0x5B: return OpCodes.Div;
				case 0x5C: return OpCodes.Div_Un;
				case 0x5D: return OpCodes.Rem;
				case 0x5E: return OpCodes.Rem_Un;
				case 0x5F: return OpCodes.And;
				case 0x60: return OpCodes.Or;
				case 0x61: return OpCodes.Xor;
				case 0x62: return OpCodes.Shl;
				case 0x63: return OpCodes.Shr;
				case 0x64: return OpCodes.Shr_Un;
				case 0x65: return OpCodes.Neg;
				case 0x66: return OpCodes.Not;
				case 0x67: return OpCodes.Conv_I1;
				case 0x68: return OpCodes.Conv_I2;
				case 0x69: return OpCodes.Conv_I4;
				case 0x6A: return OpCodes.Conv_I8;
				case 0x6B: return OpCodes.Conv_R4;
				case 0x6C: throw GetRemovedOpCodeException("conv.r8");
				case 0x6D: return OpCodes.Conv_U4;
				case 0x6E: return OpCodes.Conv_U8;
				case 0x6F: return OpCodes.Callvirt;
				case 0x70: throw GetRemovedOpCodeException("cpobj");
				case 0x71: throw GetRemovedOpCodeException("ldobj");
				case 0x72: return OpCodes.Ldstr;
				case 0x73: return OpCodes.Newobj;
				case 0x74: return OpCodes.Castclass;
				case 0x75: return OpCodes.Isinst;
				case 0x76: throw GetRemovedOpCodeException("conv.r.un");
				case 0x79: throw GetRemovedOpCodeException("unbox");
				case 0x7A: throw GetRemovedOpCodeException("throw");
				case 0x7B: return OpCodes.Ldfld;
				case 0x7C: return OpCodes.Ldflda;
				case 0x7D: return OpCodes.Stfld;
				case 0x7E: return OpCodes.Ldsfld;
				case 0x7F: return OpCodes.Ldsflda;
				case 0x80: return OpCodes.Stsfld;
				case 0x81: throw GetRemovedOpCodeException("stobj");
				case 0x82: throw GetRemovedOpCodeException("conv.ovf.i1.un");
				case 0x83: throw GetRemovedOpCodeException("conv.ovf.i2.un");
				case 0x84: throw GetRemovedOpCodeException("conv.ovf.i4.un");
				case 0x85: throw GetRemovedOpCodeException("conv.ovf.i8.un");
				case 0x86: throw GetRemovedOpCodeException("conv.ovf.u1.un");
				case 0x87: throw GetRemovedOpCodeException("conv.ovf.u2.un");
				case 0x88: throw GetRemovedOpCodeException("conv.ovf.u4.un");
				case 0x89: throw GetRemovedOpCodeException("conv.ovf.u8.un");
				case 0x8A: throw GetRemovedOpCodeException("conv.ovf.i.un");
				case 0x8B: throw GetRemovedOpCodeException("conv.ovf.u.un");
				case 0x8C: return OpCodes.Box;									//box is effectively a nop
				case 0x8D: return OpCodes.Newarr;
				case 0x8E: return OpCodes.Ldlen;
				case 0x8F: return OpCodes.Ldelema;
				case 0x90: return OpCodes.Ldelem_I1;
				case 0x91: return OpCodes.Ldelem_U1;
				case 0x92: return OpCodes.Ldelem_I2;
				case 0x93: return OpCodes.Ldelem_U2;
				case 0x94: return OpCodes.Ldelem_I4;
				case 0x95: return OpCodes.Ldelem_U4;
				case 0x96: return OpCodes.Ldelem_I8;
				case 0x97: return OpCodes.Ldelem_I;
				case 0x98: return OpCodes.Ldelem_R4;
				case 0x99: throw GetRemovedOpCodeException("ldelem.r8");
				case 0x9A: return OpCodes.Ldelem_Ref;
				case 0x9B: return OpCodes.Stelem_I;
				case 0x9C: return OpCodes.Stelem_I1;
				case 0x9D: return OpCodes.Stelem_I2;
				case 0x9E: return OpCodes.Stelem_I4;
				case 0x9F: return OpCodes.Stelem_I8;
				case 0xA0: return OpCodes.Stelem_R4;
				case 0xA1: throw GetRemovedOpCodeException("stelem.r8");
				case 0xA2: return OpCodes.Stelem_Ref;
				case 0xA3: return OpCodes.Ldelem_Any;
				case 0xA4: return OpCodes.Stelem_Any;
				case 0xA5: return OpCodes.Unbox;								//unbox is ffectively a nop
				case 0xB3: throw GetRemovedOpCodeException("conv.ovf.i1");
				case 0xB4: throw GetRemovedOpCodeException("conv.ovf.u1");
				case 0xB5: throw GetRemovedOpCodeException("conv.ovf.i2");
				case 0xB6: throw GetRemovedOpCodeException("conv.ovf.u2");
				case 0xB7: throw GetRemovedOpCodeException("conv.ovf.i4");
				case 0xB8: throw GetRemovedOpCodeException("conv.ovf.u4");
				case 0xB9: throw GetRemovedOpCodeException("conv.ovf.i8");
				case 0xBA: throw GetRemovedOpCodeException("conv.ovf.u8");
				case 0xC2: throw GetRemovedOpCodeException("refanyval");
				case 0xC3: throw GetRemovedOpCodeException("ckfinite");
				case 0xC6: throw GetRemovedOpCodeException("mkrefany");
				case 0xD0: return OpCodes.Ldtoken;
				case 0xD1: return OpCodes.Conv_U2;
				case 0xD2: return OpCodes.Conv_U1;
				case 0xD3: throw GetRemovedOpCodeException("conv.i");
				case 0xD4: throw GetRemovedOpCodeException("conv.ovf.i");
				case 0xD5: throw GetRemovedOpCodeException("conv.ovf.u");
				case 0xD6: throw GetRemovedOpCodeException("add.ovf");
				case 0xD7: throw GetRemovedOpCodeException("add.ovf.un");
				case 0xD8: throw GetRemovedOpCodeException("mul.ovf");
				case 0xD9: throw GetRemovedOpCodeException("mul.ovf.un");
				case 0xDA: throw GetRemovedOpCodeException("sub.ovf");
				case 0xDB: throw GetRemovedOpCodeException("sub.ovf.un");
				case 0xDC: throw GetRemovedOpCodeException("endfinally");
				case 0xDD: return OpCodes.Leave;
				case 0xDE: return OpCodes.Leave_S;
				case 0xDF: return OpCodes.Stind_I;
				case 0xE0: throw GetRemovedOpCodeException("conv.u");
				//Custom instructions/aliases
				case 0xF0: throw new NotImplementedException("ldfld but with a pointer is not implemented");
				case 0xF1: return OpCodes.Ldloc_S;
				case 0xF2: return OpCodes.Ldloc_0;
				case 0xF3: return OpCodes.Ldloc_1;
				case 0xF4: return OpCodes.Ldloc_2;
				case 0xF5: return OpCodes.Ldloc_3;
				case 0xF6: //throw new NotImplementedException("add delegate is not implemented");
				case 0xF7: //throw new NotImplementedException("remove delegate is not implemented");
					ilStream.Seek(4, SeekOrigin.Current);
					return OpCodes.Nop;
				case 0xF8: throw new NotImplementedException("replace delegate is not implemented");
				//End of custom instructions/aliases
				case 0xFE:
					op = ilStream.ReadByte();
					switch(op)
					{
						case 0x00: throw GetRemovedOpCodeException("arglist");
						case 0x01: return OpCodes.Ceq;
						case 0x02: return OpCodes.Cgt;
						case 0x03: return OpCodes.Cgt_Un;
						case 0x04: return OpCodes.Clt;
						case 0x05: return OpCodes.Clt_Un;
						case 0x06: return OpCodes.Ldftn;
						case 0x07: return OpCodes.Ldvirtftn;
						case 0x09: throw GetRemovedOpCodeException("ldarg");
						case 0x0A: throw GetRemovedOpCodeException("ldarga");
						case 0x0B: return OpCodes.Starg;
						case 0x0C: return OpCodes.Ldloc;
						case 0x0D: return OpCodes.Ldloca;
						case 0x0E: return OpCodes.Stloc;
						case 0x0F: throw GetRemovedOpCodeException("localloc");
						case 0x13: throw GetRemovedOpCodeException("endfilter");
						case 0x14: throw GetRemovedOpCodeException("unaligned");
						case 0x15: throw GetRemovedOpCodeException("volatile");
						case 0x16: throw GetRemovedOpCodeException("tail");
						case 0x17: throw GetRemovedOpCodeException("initobj");
						case 0x18: return OpCodes.Constrained;						//Effectively a nop
						case 0x19: throw GetRemovedOpCodeException("no");
						case 0x1A: throw GetRemovedOpCodeException("rethrow");
						case 0x1C: throw GetRemovedOpCodeException("sizeof");
						case 0x1D: throw GetRemovedOpCodeException("refanytype");
						case 0x1E: throw GetRemovedOpCodeException("readonly");
						default: throw new InvalidDataException($"Instruction 0xFE {op.ToString("X08")} does not exist");
					}
				default: throw new InvalidDataException($"Instruction {op.ToString("X08")} does not exist");
			}
		}
		private InstructionPlaceholder DumpOpCode(MethodDefinition method, int offset, StreamHelper ilStream)
		{
			OpCode op = GetOpCode(ilStream);
			InstructionPlaceholder inst = new InstructionPlaceholder(op);
			switch(op.OperandType)
			{
				case OperandType.InlineBrTarget:
					inst._operand = (int)(ilStream.ReadInt32() + ilStream.Tell());
					break;
				case OperandType.InlineField:
					inst._operand = ImportVvlFieldRef(ilStream.ReadInt32());
					break;
				case OperandType.InlineI:
					inst._operand = ilStream.ReadInt32();
					break;
				case OperandType.InlineI8:
					inst._operand = ilStream.ReadInt64();
					break;
				case OperandType.InlineMethod:
					inst._operand = ImportVvlMethodRef(ilStream.ReadUInt32());
					break;
				case OperandType.InlineNone:
					break;
				case OperandType.InlineR:
					inst._operand = ilStream.ReadDouble();
					break;
				case OperandType.InlineString:
					inst._operand = VvlLoader.ReadVvlString(_library._stringTable, ilStream.ReadUInt32());
					break;
				case OperandType.InlineSwitch:
					int switch_length = ilStream.ReadInt32();
					int[] branches = new int[switch_length];
					int switch_base_offset = (int)ilStream.Tell() + switch_length * 4;
					for(int i = 0; i < switch_length; i++)
					{
						branches[i] = switch_base_offset + ilStream.ReadInt32();
					}
					inst._operand = branches;	//This is a hack I stole from Mono.Cecil hahah
					break;
				case OperandType.InlineTok:
				case OperandType.InlineType:
					inst._operand = ImportVvlTypeRef(_library._referencedTypes[ilStream.ReadInt32()]);
					break;
				case OperandType.InlineVar:
					inst._operand = method.Body.Variables[ilStream.ReadUInt16()];
					break;
				case OperandType.InlineArg:
					inst._operand = method.Parameters[ilStream.ReadUInt16()];
					break;
				case OperandType.ShortInlineBrTarget:
					inst._operand = (int)(ilStream.ReadSByte() + ilStream.Tell());
					break;
				case OperandType.ShortInlineI:
					if(inst._opcode == OpCodes.Ldc_I4_S) inst._operand = ilStream.ReadSByte();
					else                                 inst._operand = ilStream.ReadByte();
					break;
				case OperandType.ShortInlineR:
					inst._operand = ilStream.ReadSingle();
					break;
				case OperandType.ShortInlineVar:
					inst._operand = method.Body.Variables[ilStream.ReadByte()];
					break;
				case OperandType.ShortInlineArg:
					inst._operand = method.Parameters[ilStream.ReadByte()];
					break;
				default:
					//InlineSig doesn't need to be implemented as Calli isn't a valid instruction in a vvl
					throw new NotSupportedException($"OperandType \"{op.OperandType}\" for OpCode {op} in not supported.");
			}
			return inst;
			/*switch(op)
			{
				case 0x00:	//nop
					return Instruction.Create(OpCodes.Nop);
				case 0x01:	//break
					return Instruction.Create(OpCodes.Break);
				case 0x02: //ldarg.0
					return Instruction.Create(OpCodes.Ldarg_0);
				case 0x03: //ldarg.1
					return Instruction.Create(OpCodes.Ldarg_1);
				case 0x04: //ldarg.2
					return Instruction.Create(OpCodes.Ldarg_2);
				case 0x05: //ldarg.3
					return Instruction.Create(OpCodes.Ldarg_3);
				case 0x06: //ldloc.0
					return Instruction.Create(OpCodes.Ldloc_0);
				case 0x07: //ldloc.1
					return Instruction.Create(OpCodes.Ldloc_1);
				case 0x08: //ldloc.2
					return Instruction.Create(OpCodes.Ldloc_2);
				case 0x09: //ldloc.3
					return Instruction.Create(OpCodes.Ldloc_3);
				case 0x0A:	//stloc.0
					return Instruction.Create(OpCodes.Stloc_0);
				case 0x0B:	//stloc.1
					return Instruction.Create(OpCodes.Stloc_1);
				case 0x0C:	//stloc.2
					return Instruction.Create(OpCodes.Stloc_2);
				case 0x0D:	//stloc.3
					return Instruction.Create(OpCodes.Stloc_3);
				case 0x0E: //ldarg.s
					return Instruction.Create(OpCodes.Ldarg_S, *bytecode++);
				case 0x0F: //ldarga.s
					return Instruction.Create(OpCodes.Ldarga_S, *bytecode++);
				case 0x10: //starg.s
					return Instruction.Create(OpCodes.Starg_S, *bytecode++);
				case 0x11: //ldloc.s
					return Instruction.Create(OpCodes.Ldloc_S, *bytecode++);
				case 0x12: //ldloca.s
					return Instruction.Create(OpCodes.Ldloca_S, *bytecode++);
				case 0x13: //stloc.s
					return Instruction.Create(OpCodes.Stloc_S, *bytecode++);
				case 0x14: //ldnull
					return Instruction.Create(OpCodes.Ldnull);
				case 0x15: //ldc.i4.m1
					return Instruction.Create(OpCodes.Ldc_I4_M1);
				case 0x16: //ldc.i4.0
					return Instruction.Create(OpCodes.Ldc_I4_0);
				case 0x17: //ldc.i4.1
					return Instruction.Create(OpCodes.Ldc_I4_1);
				case 0x18: //ldc.i4.2
					return Instruction.Create(OpCodes.Ldc_I4_2);
				case 0x19: //ldc.i4.3
					return Instruction.Create(OpCodes.Ldc_I4_3);
				case 0x1A: //ldc.i4.4
					return Instruction.Create(OpCodes.Ldc_I4_4);
				case 0x1B: //ldc.i4.5
					return Instruction.Create(OpCodes.Ldc_I4_5);
				case 0x1C: //ldc.i4.6
					return Instruction.Create(OpCodes.Ldc_I4_6);
				case 0x1D: //ldc.i4.7
					return Instruction.Create(OpCodes.Ldc_I4_7);
				case 0x1E: //ldc.i4.8
					return Instruction.Create(OpCodes.Ldc_I4_8);
				case 0x1F: //ldc.i4.s
					inst = Instruction.Create(OpCodes.Ldc_I4_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x20: //ldc.i4
					inst = Instruction.Create(OpCodes.Ldc_I4, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x21: //ldc.i8
					inst = Instruction.Create(OpCodes.Ldc_I8, GetI8FromIl(_library._isLittleEndian, bytecode));
					bytecode += 8;
					return inst;
				case 0x22: //ldc.r4
					inst = Instruction.Create(OpCodes.Ldc_R4, GetR4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x23: //ldc.r8
					inst = Instruction.Create(OpCodes.Ldc_R8, GetR8FromIl(_library._isLittleEndian, bytecode));
					bytecode += 8;
					return inst;
				case 0x24: //Does not exist
				case 0x27: //jmp isn't implemented
				case 0x29: //calli isn't implemented
					throw new NotSupportedException("Failed reading IL stream");
				case 0x25: //dup
					return Instruction.Create(OpCodes.Dup);
				case 0x26: //pop
					return Instruction.Create(OpCodes.Pop);
				case 0x28: //call
					inst = Instruction.Create(OpCodes.Call, ImportVvlMethodRef(GetU4FromIl(_library._isLittleEndian, bytecode)));
					bytecode += 4;
					return inst;
				case 0x2A: //ret
					return Instruction.Create(OpCodes.Ret);
				case 0x2B: //br.s
					inst = Instruction.Create(OpCodes.Br_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x2C: //brfalse.s, brnull.s, brzero.s
					inst = Instruction.Create(OpCodes.Brfalse_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x2D: //brtrue.s, brinst.s
					inst = Instruction.Create(OpCodes.Brtrue_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x2E: //beq.s
					inst = Instruction.Create(OpCodes.Beq_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x2F: //bge.s
					inst = Instruction.Create(OpCodes.Bge_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x30: //bgt.s
					inst = Instruction.Create(OpCodes.Bgt_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x31: //ble.s
					inst = Instruction.Create(OpCodes.Ble_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x32: //blt.s
					inst = Instruction.Create(OpCodes.Blt_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x33: //bne.un.s
					inst = Instruction.Create(OpCodes.Bne_Un_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x34: //bge.un.s
					inst = Instruction.Create(OpCodes.Bge_Un_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x35: //bgt.un.s
					inst = Instruction.Create(OpCodes.Bgt_Un_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x36: //ble.un.s
					inst = Instruction.Create(OpCodes.Ble_Un_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x37: //blt.un.s
					inst = Instruction.Create(OpCodes.Blt_Un_S, *(sbyte*)bytecode);
					bytecode++;
					return inst;
				case 0x38: //br
					inst = Instruction.Create(OpCodes.Br, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x39: //brfalse
					inst = Instruction.Create(OpCodes.Brfalse, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x3A: //brtrue
					inst = Instruction.Create(OpCodes.Brtrue, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x3B: //beq
					inst = Instruction.Create(OpCodes.Beq, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x3C: //bge
					inst = Instruction.Create(OpCodes.Bge, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x3D: //bgt
					inst = Instruction.Create(OpCodes.Bgt, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x3E: //ble
					inst = Instruction.Create(OpCodes.Ble, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x3F: //blt
					inst = Instruction.Create(OpCodes.Blt, GetI4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x40: //bne.un
					inst = Instruction.Create(OpCodes.Bne_Un, GetU4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x41: //bge.un
					inst = Instruction.Create(OpCodes.Bge_Un, GetU4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x42: //bgt.un
					inst = Instruction.Create(OpCodes.Bgt_Un, GetU4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x43: //ble.un
					inst = Instruction.Create(OpCodes.Ble_Un, GetU4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x44: //blt.un
					inst = Instruction.Create(OpCodes.Blt_Un, GetU4FromIl(_library._isLittleEndian, bytecode));
					bytecode += 4;
					return inst;
				case 0x45: //switch
					inst = Instruction.Create(OpCodes.Switch);
					int switch_length = GetI4FromIl(_library._isLittleEndian, bytecode);
					int[] branches = new int[switch_length];
					int switch_base_offset = switch_length * 4;
					for(int i = 0; i < switch_length; i++)
					{
						branches[i] = GetI4FromIl(_library._isLittleEndian, bytecode);
					}
					inst.Operand = branches;	//This is a hack I stole from Mono.Cecil hahah
					bytecode += (switch_length + 1) * 4;
					return inst;
				case 0x46:
					
					break;
			}*/
		}
		public void Finish()
		{
			_module.Write($"{_module.Name}.dll");
		}
	}
}