using igLibrary.Core;
using Newtonsoft.Json;

namespace igCauldron3
{
	public class CauldronConfig
	{
		private const string _configFilename = "gameconfig.json";
		public static CauldronConfig _config { get; private set; }

		public List<GameConfig> _games = new List<GameConfig>();

		public class GameConfig
		{
			public string _path = string.Empty;
			public string _updatePath = string.Empty;
			public igArkCore.EGame _game = igArkCore.EGame.EV_SkylandersSuperchargers;	//This'll be replaced once multiple games work
			public IG_CORE_PLATFORM _platform;
		}

		public static void ReadConfig()
		{
			if(File.Exists(_configFilename))
			{
				string json = File.ReadAllText(_configFilename);
				_config = JsonConvert.DeserializeObject<CauldronConfig>(json);
				if(_config == null) throw new ApplicationException($"Failed to load config. Try deleting {_configFilename} and trying again.");
			}
			else
			{
				_config = new CauldronConfig();
			}
		}
		public static void WriteConfig()
		{
			string json = JsonConvert.SerializeObject(_config);
			File.WriteAllText(_configFilename, json);
		}
	}
}