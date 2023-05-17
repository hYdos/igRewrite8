using igLibrary.Core;

namespace igLibrary.Gfx
{
	public static class igGfx
	{
		private enum Props
		{
			Tile			= 0b000001,
			Canonical		= 0b000010,
			Compressed		= 0b000100,
			Palette			= 0b001000,
			Srgb			= 0b010000,
			FloatingPoint	= 0b100000
		}
		public static void Initialize()
		{
			igObjectDirectory metaimages = new igObjectDirectory();
			metaimages._name = new igName("metaimages");
			metaimages._useNameList = true;
			metaimages._nameList = new igNameList();

			AppendMetaImage(metaimages, "dxt1", 4, Props.Compressed);
			AppendMetaImage(metaimages, "dxt1_tile_big_ps3", 4, Props.Compressed | Props.Tile);
			AppendMetaImage(metaimages, "dxt1_big_ps3", 4, Props.Compressed);
			AppendMetaImage(metaimages, "dxt1_srgb_big_ps3", 4, Props.Compressed | Props.Srgb);
			AppendMetaImage(metaimages, "dxt1_srgb_tile_big_ps3", 4, Props.Compressed | Props.Srgb | Props.Tile);

			AppendMetaImage(metaimages, "dxt3", 8, Props.Compressed);
			AppendMetaImage(metaimages, "dxt3_tile_big_ps3", 8, Props.Compressed | Props.Tile);
			AppendMetaImage(metaimages, "dxt3_big_ps3", 8, Props.Compressed);
			AppendMetaImage(metaimages, "dxt3_srgb_big_ps3", 8, Props.Compressed | Props.Srgb);
			AppendMetaImage(metaimages, "dxt3_srgb_tile_big_ps3", 8, Props.Compressed | Props.Srgb | Props.Tile);

			AppendMetaImage(metaimages, "dxt5", 8, Props.Compressed);
			AppendMetaImage(metaimages, "dxt5_tile_big_ps3", 8, Props.Compressed | Props.Tile);
			AppendMetaImage(metaimages, "dxt5_big_ps3", 8, Props.Compressed);
			AppendMetaImage(metaimages, "dxt5_srgb_big_ps3", 8, Props.Compressed | Props.Srgb);
			AppendMetaImage(metaimages, "dxt5_srgb_tile_big_ps3", 8, Props.Compressed | Props.Srgb | Props.Tile);

			igObjectStreamManager.Singleton.AddObjectDirectory(metaimages);
			igObjectHandleManager.Singleton.AddSystemNamespace("metaimages");
		}
		private static igMetaImage AppendMetaImage(igObjectDirectory metaimages, string name, byte bpp, Props properties)
		{
			igMetaImage meta = new igMetaImage();
			meta._name = name;
			meta._bitsPerPixel = bpp;
			meta._properties = (byte)properties;

			metaimages._objectList.Append(meta);
			metaimages._nameList.Append(new igName(name));

			return meta;
		}
	}
}