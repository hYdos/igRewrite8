using System.Text;

namespace igLibrary.Gfx
{
	public class igMetaImageInfo : igObject
	{
		private static Lazy<igMetaImageInfo> _metaImageInfoLazy = new Lazy<igMetaImageInfo>(() => new igMetaImageInfo());
		public static igMetaImageInfo _metaImageInfo => _metaImageInfoLazy.Value;
		public igStringMetaImageHashTable _metaImagesTable = new igStringMetaImageHashTable() { _autoRehash = true };
		public static void RegisterFormat(igMetaImage metaimage)
		{
			//Console.WriteLine($"I should've added {metaimage._name}");
			_metaImageInfo._metaImagesTable.Add(metaimage._name, metaimage);
			igObjectStreamManager.Singleton.Load("metaimages").AddObject(metaimage, default, new igName(metaimage._name));
		}
		public static igMetaImage FindFormat(string name)
		{
			return _metaImageInfo._metaImagesTable[name];
		}
		public static void Debug()
		{
			StringBuilder sb = new StringBuilder();
			igObjectDirectory dir = igObjectStreamManager.Singleton.Load("metaimages");
			for(int i = 0; i < dir._objectList._count; i++)
			{
				sb.Append(dir._nameList[i]._hash.ToString("X08"));
				sb.Append(' ');
				sb.Append(dir._nameList[i]._string);
				sb.Append('\n');
			}
			File.WriteAllText("metaimages.debug.log", sb.ToString());
		}
	}
	public class igStringMetaImageHashTable : igTUHashTable<igMetaImage, string> {}
}