using igLibrary.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace igCauldron3
{
	public class CauldronConfig
	{
		public static string ConfigFolder
		{
			get
			{
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NefariousTechSupport", "igCauldron");
				Directory.CreateDirectory(path);
				return path;
			}
		}
		private static string GameConfigFilePath => Path.Combine(ConfigFolder, "gameconfig.json");
		public static string ImGuiConfigFilePath => Path.Combine(ConfigFolder, "imgui.ini");
		public static CauldronConfig _config { get; private set; }
		private const int CurrentVersion = 2;

		public int _version = CurrentVersion;
		public List<GameConfig> _games = new List<GameConfig>();

		public class VersionChecker
		{
			public int _version;
		}

		public class GameConfig
		{
			public string _path = string.Empty;
			public string _updatePath = string.Empty;
			[JsonConverter(typeof(StringEnumConverter))] public igArkCore.EGame _game = igArkCore.EGame.EV_None;
			[JsonConverter(typeof(StringEnumConverter))] public IG_CORE_PLATFORM _platform;
		}

		public static void ReadConfig()
		{
			if(File.Exists(GameConfigFilePath))
			{
				string json = File.ReadAllText(GameConfigFilePath);

				int version = JsonConvert.DeserializeObject<VersionChecker>(json)._version;
				if (version == CurrentVersion)
				{
					_config = JsonConvert.DeserializeObject<CauldronConfig>(json);
				}

				if(_config == null) throw new ApplicationException($"Failed to load config. Try deleting \"{GameConfigFilePath}\" and try again.");
			}
			else
			{
				_config = new CauldronConfig();
			}
		}
		public static void WriteConfig()
		{
			string json = JsonConvert.SerializeObject(_config);
			File.WriteAllText(GameConfigFilePath, json);
		}
	}
}