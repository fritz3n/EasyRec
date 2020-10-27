using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyRec.Configuration
{
	static class ConfigHandler
	{
		private const string configPath = "config.json";
		public static Config Config { get; set; } = null;

		static ConfigHandler()
		{
			Load();
		}

		private static void Load()
		{
			if (!File.Exists(configPath))
			{
				SetDefault();
				Save();
			}
			else
			{
				string json = File.ReadAllText(configPath);
				Config = JsonConvert.DeserializeObject<Config>(json);
			}
		}

		public static void Save()
		{
			string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
			try
			{
				File.WriteAllText(configPath, json);
			}
			catch (UnauthorizedAccessException) { }
		}

		private static void SetDefault()
		{
			string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "EasyRec");

			Config = new Config()
			{
				Inputs = new string[] { "<default>", "<defaultLoopback>" },
				MixdownType = MixdownType.Both,
				BufferLength = 60,
				BufferPath = Path.Combine(basePath, "Highlights"),
				BufferPattern = "Highlight %yyyy-MM-dd HH-mm%",
				BufferWriter = FileWriterType.Mp3,
				RecordPath = Path.Combine(basePath, "Recordings"),
				RecordPattern = "Recording %yyyy-MM-dd HH-mm%",
				RecordWriter = FileWriterType.Mp3,
				SampleRate = 44100,
				Bits = 16,
				Hotkeys = new List<HotkeyConfig>()
			};
		}
	}
}
