/*using System.Linq;
using System;

namespace igLibrary.Core
{
	public class igArkCoreLoader
	{
		private StreamHelper sh;
		private StreamHelper stringSh;
		private StreamHelper metaobjectPrepassSh;
		private StreamHelper metaobjectSh;
		private StreamHelper compoundFieldSh;
		private StreamHelper metaenumSh;
		private igArkCore.EGame _game;
		private List<string?> _stringTable;
		public uint _metaEnumCount;
		public uint _metaObjectCount;
		public uint _compoundFieldCount;
		private uint _stringTableCount;

		public igArkCoreLoader(string filePath, igArkCore.EGame game)
		{
			sh = new StreamHelper(new FileStream(filePath, FileMode.Open, FileAccess.Read));

			uint magic = sh.ReadUInt32();
			uint version = sh.ReadUInt32();
			if(magic != igArkCore._magicCookie || version != igArkCore._magicVersion) throw new InvalidDataException("Invalid magic cookie or magic version");

			sh.ReadUInt32();

			_stringTableCount = sh.ReadUInt32();
			stringSh = new StreamHelper(sh.ReadBytes(sh.ReadUInt32()));
			_metaObjectCount = sh.ReadUInt32();
			metaobjectPrepassSh = new StreamHelper(sh.ReadBytes(_metaObjectCount * 4));
			metaobjectSh = new StreamHelper(sh.ReadBytes(sh.ReadUInt32()));
			_compoundFieldCount = sh.ReadUInt32();
			compoundFieldSh = new StreamHelper(sh.ReadBytes(sh.ReadUInt32()));
			_metaEnumCount = sh.ReadUInt32();
			metaenumSh = new StreamHelper(sh.ReadBytes(sh.ReadUInt32()));
			_game = game;
			_stringTable = new List<string?>();
			LoadStringTable();
		}

		public igMetaEnum? LoadMetaEnum()
		{
			if(metaenumSh.Tell()+1 == metaenumSh.BaseStream.Length) return null;
			igMetaEnum metaEnum = new igMetaEnum();
			metaEnum._name = LoadString(metaenumSh.ReadInt32());
			int memberCount = metaenumSh.ReadInt32();
			for(int i = 0; i < memberCount; i++)
			{
				metaEnum._names.Add(LoadString(metaenumSh.ReadInt32()));
				metaEnum._values.Add(metaenumSh.ReadInt32());
			}
			return metaEnum;
		}
		public void InstantiateAndAppendMetaObjects(List<igMetaObject> output)
		{
			metaobjectPrepassSh.Seek(0);
			for(int i = 0; i < _metaObjectCount; i++)
			{
				igMetaObject meta = new igMetaObject();
				meta._name = LoadString(metaobjectPrepassSh.ReadInt32());
				output.Add(meta);
			}
		}
		public void LoadMetaObjects(List<igMetaObject> output)
		{
			InstantiateAndAppendMetaObjects(output);

			for(int i = 0; i < _metaObjectCount; i++)
			{
				LoadMetaObject(output[i]);
			}
		}
		public void LoadCompoundInfos(List<igCompoundMetaFieldInfo> output)
		{
			for(int i = 0; i < _compoundFieldCount; i++)
			{
				igCompoundMetaFieldInfo compoundInfo = new igCompoundMetaFieldInfo();
				compoundInfo.UndumpArkData(this, compoundFieldSh);
				igArkCore._compoundFieldInfos.Add(compoundInfo);
			}
		}
		public igMetaObject? LoadMetaObject(igMetaObject metaObject)
		{
			if(metaobjectSh.Tell()+1 == metaobjectSh.BaseStream.Length) return null;
			Console.WriteLine($"MetaObject @ {metaobjectSh.Tell().ToString("X08")}");
			//metaObject._name = ReadStringIndex(metaobjectSh);
			metaObject._parent = igArkCore.GetObjectMeta(ReadStringIndex(metaobjectSh));
			int fieldCount = metaobjectSh.ReadInt32();

			for(int i = 0; i < fieldCount; i++)
			{
				Console.WriteLine($"MetaField {i} @ {metaobjectSh.Tell().ToString("X08")}");
				metaObject._metaFields.Add(LoadMetaField(metaobjectSh));
			}
			return metaObject;
		}

		private void LoadStringTable()
		{
			for(int i = 0; i < _stringTableCount; i++)
			{
				_stringTable.Add(stringSh.ReadString());
			}
		}
		private string? LoadString(int index)
		{
			if(index == -1) return null;
			else return _stringTable[index];
		}
		public string? ReadStringIndex(StreamHelper sh)
		{
			return LoadString(sh.ReadInt32());
		}
		public igMetaField LoadMetaField(StreamHelper sh)
		{
			uint basePosition = sh.Tell();
			string typeName = ReadStringIndex(sh);
			int tIndex = Array.FindIndex<Type>(igArkCore.MetaFields, x => x.Name == typeName);
			Type t = tIndex < 0 ? typeof(igMetaField) : igArkCore.MetaFields[tIndex];
			igMetaField metaField = (igMetaField)Activator.CreateInstance(t);
			metaField.UndumpArkData(this, sh);
			return metaField;
		}
	}
}*/