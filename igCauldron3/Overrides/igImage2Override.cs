using igCauldron3.Utils;
using igLibrary.Core;
using igLibrary.Gfx;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;

namespace igCauldron3
{
	public class igImage2Override : InspectorDrawOverride
	{
		private Dictionary<igImage2, int> imageTextureLookup = new Dictionary<igImage2, int>();
		public igImage2Override()
		{
			_t = typeof(igImage2);
		}

		public unsafe override void Draw(ObjectManagerFrame objFrame, igObject obj, igMetaObject meta)
		{
			igImage2 image = (igImage2)obj;
			
			bool previouslyAdded = imageTextureLookup.TryGetValue(image, out int tex);

			if(!previouslyAdded)
			{
				if(image._format == null)
				{
					Console.WriteLine($"{image._name} has Unsupported Texture Format");
					GL.DeleteTexture(tex);
					return;
				}

				tex = GL.GenTexture();

				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, tex);

				uint offset = 0;
				int width = image._width;
				int height = image._height;
				int size = 0;
				InternalFormat compressedFormat = InternalFormat.Srgb;	//This is a default value
				bool useCompression = image._format._isCompressed;

				byte[] data = (byte[])((IigMemory)image._data).GetData();
				fixed(byte* b = data)
				{
					byte[]? uncompressedRGBAData = null;
					if(image._format._name.StartsWith("dxt1"))
					{
						size = (int)(Math.Max( 1, ((width+3)/4) ) * Math.Max(1, ( (height + 3) / 4 ) )) * 8;
						if(image._format._isSrgb) compressedFormat = InternalFormat.CompressedSrgbS3tcDxt1Ext;
						else                      compressedFormat = InternalFormat.CompressedRgbS3tcDxt1Ext;
					}
					else if(image._format._name.StartsWith("dxt3"))
					{
						size = (int)(Math.Max( 1, ((width+3)/4) ) * Math.Max(1, ( (height + 3) / 4 ) )) * 16;
						if(image._format._isSrgb) compressedFormat = InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
						else                      compressedFormat = InternalFormat.CompressedRgbaS3tcDxt3Ext;
					}
					else if(image._format._name.StartsWith("dxt5"))
					{
						size = (int)(Math.Max( 1, ((width+3)/4) ) * Math.Max(1, ( (height + 3) / 4 ) )) * 16;
						if(image._format._isSrgb) compressedFormat = InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
						else                      compressedFormat = InternalFormat.CompressedRgbaS3tcDxt5Ext;
					}
					else if(image._format._name.StartsWith("pvrtc2"))
					{
					}
					else if(image._format._name.StartsWith("pvrtc4"))
					{
					}
					else throw new Exception("invalid SimpleMetaImageFormat");

					if(useCompression)
					{
						if(compressedFormat == InternalFormat.Srgb) throw new Exception();
						GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, compressedFormat, width, height, 0, size, (IntPtr)(b + offset));
					}
					else
					{
						if(image._format._isSrgb)
						{
							GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Srgb8Alpha8, image._width, image._height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, uncompressedRGBAData);
						}
						else
						{
							GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image._width, image._height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, uncompressedRGBAData);
						}
					}
				}

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);

				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

				GL.BindTexture(TextureTarget.Texture2D, 0);

				imageTextureLookup.Add(image, tex);
			}

			objFrame.RenderFieldWithName(image, meta.GetFieldByName("_name"));
			if(ImGui.Button("Extract"))
			{
				string filePath = CrossFileDialog.SaveFile("Save Image...", ".dds");
				if(!string.IsNullOrWhiteSpace(filePath))
				{
					FileStream fs = File.Create(filePath);
					igImage2Exporter.ExportToDds(image, fs);
					fs.Close();
				}
			}
			if(ImGui.Button("Replace"))
			{
			}
			ImGui.Image((IntPtr)tex, new System.Numerics.Vector2(image._width, image._height), System.Numerics.Vector2.UnitY, System.Numerics.Vector2.UnitX);
		}
	}
}