using igLibrary.Core;
using igLibrary.Gfx;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace igCauldron3.Conversion
{
	public static class TextureConversion
	{
		public static void Export(igImage2 image, Stream dst, string ext)
		{
			int res = image.ConvertClone(igMetaImageInfo.FindFormat("r8g8b8a8"), igMemoryContext.Singleton.GetMemoryPoolByName("Image"), out igImage2? r8g8b8a8Image);
			if(res != 0 || r8g8b8a8Image == null) return;
			Image<Rgba32> output = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(r8g8b8a8Image._data.Buffer, r8g8b8a8Image._width, r8g8b8a8Image._height);
			switch(ext)
			{
				case ".png":
					output.SaveAsPng(dst);
					break;
				case ".jpeg":
				case ".jpg":
					output.SaveAsJpeg(dst);
					break;
				case ".bmp":
					output.SaveAsBmp(dst);
					break;
				case ".gif":
					output.SaveAsGif(dst);
					break;
				case ".pbm":
					output.SaveAsPbm(dst);
					break;
				case ".qoi":
					output.SaveAsQoi(dst);
					break;
				case ".tga":
					output.SaveAsTga(dst);
					break;
				case ".tiff":
					output.SaveAsTiff(dst);
					break;
				case ".webp":
					output.SaveAsWebp(dst);
					break;
			}
		}
	}
}