using igCauldron3.Graphics;
using igCauldron3.Utils;
using igLibrary.Core;
using igLibrary.Gfx;
using igLibrary.Graphics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;

namespace igCauldron3
{
	public class igImage2Override : InspectorDrawOverride
	{
		public igImage2Override()
		{
			_t = typeof(igImage2);
		}

		public unsafe override void Draw(ObjectManagerFrame objFrame, igObject obj, igMetaObject meta)
		{
			igImage2 image = (igImage2)obj;
			
			if(image._texHandle == -1)
			{
				if(image._format == null)
				{
					Console.WriteLine($"{image._name} has Unsupported Texture Format");
					return;
				}

				igTContext<igBaseGraphicsDevice>._instance.CreateTexture(igResourceUsage.kUsageDefault, image);
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
			ImGui.Image((IntPtr)image._texHandle, new System.Numerics.Vector2(image._width, image._height), System.Numerics.Vector2.UnitY, System.Numerics.Vector2.UnitX);
		}
	}
}