/*namespace igLibrary.Core
{
	public class igArkCoreSaver
	{
		private StreamHelper sh;
		private StreamHelper stringSh;
		private StreamHelper metaobjectPrepassSh;
		private StreamHelper metaobjectSh;
		private StreamHelper metaenumSh;
		private igArkCore.EGame _game;
		private List<string?> _stringTable;
		private uint savedMetaObjects = 0;
		private uint savedMetaEnums = 0;

		public igArkCoreSaver(string filePath, igArkCore.EGame game)
		{
			sh = new StreamHelper(new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite));
			stringSh = new StreamHelper(new MemoryStream());
			metaobjectPrepassSh = new StreamHelper(new MemoryStream());
			metaobjectSh = new StreamHelper(new MemoryStream());
			metaenumSh = new StreamHelper(new MemoryStream());
			_game = game;
			_stringTable = new List<string?>();
		}

		public void SaveMetaEnum(igMetaEnum metaEnum)
		{
			metaenumSh.WriteInt32(SaveString(metaEnum._name));
			metaenumSh.WriteInt32(metaEnum._names.Count);
			for(int i = 0; i < metaEnum._names.Count; i++)
			{
				metaenumSh.WriteInt32(SaveString(metaEnum._names[i]));
				metaenumSh.WriteInt32(metaEnum._values[i]);
			}
			savedMetaEnums++;
		}
		public void SaveMetaObject(igMetaObject meta)
		{
			metaobjectPrepassSh.WriteInt32(SaveString(meta._name));
			metaobjectSh.WriteInt32(SaveString((meta._parent == null ? null : meta._parent._name)));
			metaobjectSh.WriteInt32(meta._metaFields.Count);
			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				meta._metaFields[i].DumpArkData(this, metaobjectSh);
			}
			savedMetaObjects++;
		}

		public void FinishSave()
		{
			sh.WriteUInt32(igArkCore._magicCookie);
			sh.WriteUInt32(igArkCore._magicVersion);
			sh.WriteUInt32((uint)_game);

			stringSh.BaseStream.Flush();
			sh.WriteUInt32((uint)_stringTable.Count);
			sh.WriteUInt32((uint)stringSh.BaseStream.Length);
			sh.BaseStream.Write((stringSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)stringSh.BaseStream.Length);
			stringSh.BaseStream.Close();

			metaobjectPrepassSh.BaseStream.Flush();
			sh.WriteUInt32(savedMetaObjects);
			sh.BaseStream.Write((metaobjectPrepassSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)metaobjectPrepassSh.BaseStream.Length);
			metaobjectPrepassSh.BaseStream.Close();

			metaobjectSh.BaseStream.Flush();
			sh.WriteUInt32((uint)metaobjectSh.BaseStream.Length);
			sh.BaseStream.Write((metaobjectSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)metaobjectSh.BaseStream.Length);
			metaobjectSh.BaseStream.Close();

			metaenumSh.BaseStream.Flush();
			sh.WriteUInt32(savedMetaEnums);
			sh.WriteUInt32((uint)metaenumSh.BaseStream.Length);
			sh.BaseStream.Write((metaenumSh.BaseStream as MemoryStream).GetBuffer(), 0, (int)metaenumSh.BaseStream.Length);
			metaenumSh.BaseStream.Close();

			sh.Close();
		}
		public int SaveString(string? str)
		{
			if(str == null) return -1;
			int index = _stringTable.FindIndex(x => x == str);
			if(index >= 0) return index;
			else
			{
				_stringTable.Add(str);
				stringSh.WriteString(str);
				return _stringTable.Count - 1;
			}
		}
	}
}*/