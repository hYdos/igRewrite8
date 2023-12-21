using ImGuiNET;
using igLibrary.Core;
using igLibrary;

namespace igCauldron3
{
	public class ConfigFrame : Frame
	{
		public ConfigFrame(Window wnd) : base(wnd)
		{
			CauldronConfig.ReadConfig();
		}
		public override void Render()
		{
			ImGui.Begin("Configuration");

			CauldronConfig config = CauldronConfig._config;

			for(int i = 0; i < config._games.Count; i++)
			{
				CauldronConfig.GameConfig game = config._games[i];
				if(ImGui.TreeNode(i.ToString("X08"), $"Game {i}: {game._path}"))
				{
					RenderTextField("Game Path", "gp", ref game._path);
					RenderTextField("Update Path", "up", ref game._updatePath);

					ImGui.Text("Platform");
					ImGui.SameLine();
					ImGui.PushID("platform");
					int selectedItem = (int)game._platform;
					bool changed = ImGui.Combo(string.Empty, ref selectedItem, typeof(IG_CORE_PLATFORM).GetEnumNames(), (int)IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX);
					ImGui.PopID();
					if(changed)
					{
						game._platform = (IG_CORE_PLATFORM)selectedItem;
					}

					if(ImGui.Button("Load Game"))
					{
						if(!Directory.Exists(game._path)) throw new DirectoryNotFoundException("Game folder does not exist");
						if(!string.IsNullOrWhiteSpace(game._updatePath) && !File.Exists(game._updatePath)) throw new FileNotFoundException("Update file does not exist");
						if(game._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT || game._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED || game._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX) throw new ArgumentException("Invalid platform");

						CauldronConfig.WriteConfig();

						igFileContext.Singleton.Initialize(game._path);
						if(!string.IsNullOrWhiteSpace(game._updatePath))
						{
							igFileContext.Singleton.InitializeUpdate(game._updatePath);
						}
						igRegistry.GetRegistry()._platform = game._platform;
						igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);
						IG_CORE_PLATFORM platform = igRegistry.GetRegistry()._platform;

						igFileContext.Singleton.LoadArchive("data:/archives/loosefiles.pak");

						CPrecacheManager._Instance.PrecachePackage($"generated/shaders/shaders_{igAlchemyCore.GetPlatformString(platform)}", EMemoryPoolID.MP_DEFAULT);

						CPrecacheManager._Instance.PrecachePackage($"generated/packageXmls/permanent_{igAlchemyCore.GetPlatformString(platform)}", EMemoryPoolID.MP_DEFAULT);
						CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/essentialui", EMemoryPoolID.MP_DEFAULT);
						CPrecacheManager._Instance.PrecachePackage("generated/UI/legal", EMemoryPoolID.MP_DEFAULT);
						CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/gamestartup", EMemoryPoolID.MP_DEFAULT);
						CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanentdeveloper", EMemoryPoolID.MP_DEFAULT);

						CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent", EMemoryPoolID.MP_DEFAULT);
						CPrecacheManager._Instance.PrecachePackage("generated/maps/zoneinfos", EMemoryPoolID.MP_DEFAULT);
						CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent_2015", EMemoryPoolID.MP_DEFAULT);
						CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_Mobile", EMemoryPoolID.MP_DEFAULT);
						CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_FrontEnd", EMemoryPoolID.MP_DEFAULT);

						_wnd.frames.Remove(this);
						_wnd.frames.Add(new ObjectManagerFrame(_wnd));
						_wnd.frames.Add(new ArchiveFrame(_wnd));
						_wnd.frames.Add(new MenuBarFrame(_wnd));

					}
					ImGui.SameLine();
					if(ImGui.Button("Remove Game"))
					{
						config._games.RemoveAt(i);
					}

					ImGui.TreePop();
				}
			}

			if(ImGui.Button("Add Game"))
			{
				config._games.Add(new CauldronConfig.GameConfig());
			}

			ImGui.End();
		}
		private void RenderTextField(string label, string id, ref string val)
		{
			ImGui.Text(label);
			ImGui.SameLine();
			ImGui.PushID(id);
			ImGui.InputText(string.Empty, ref val, 512);
			ImGui.PopID();
		}
	}
}