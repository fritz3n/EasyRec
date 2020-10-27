using EasyRec.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyRec.Hotkeys
{
	public class HotkeyHandler
	{

		List<Hotkey> hotkeys;
		private readonly Startup startup;

		public HotkeyHandler(Startup startup)
		{
			Config conf = ConfigHandler.Config;
			hotkeys = conf.Hotkeys?.Select(c => new Hotkey(c.ModifierKeys, c.Key, c.Type)).ToList() ?? new List<Hotkey>();
			this.startup = startup;
		}

		public void Initialize()
		{
			foreach (Hotkey h in hotkeys)
			{
				h.Pressed += H_Pressed;
				h.Register();
			}
		}

		private void H_Pressed(object sender, EventArgs e)
		{
			HotkeyType type = (sender as Hotkey).HotkeyType;
			switch (type)
			{
				case HotkeyType.SaveBuffer:
					_ = startup.SaveBuffer();
					break;

				case HotkeyType.StartBuffer:
					startup.SetBuffering(true, true);
					break;

				case HotkeyType.StopBuffer:
					startup.SetBuffering(false, true);
					break;

				case HotkeyType.ToggleBuffer:
					startup.SetBuffering(!startup.Buffering, true);
					break;

				case HotkeyType.StartRecording:
					startup.SetRecording(true, true);
					break;

				case HotkeyType.StopRecording:
					startup.SetRecording(false, true);
					break;

				case HotkeyType.ToggleRecording:
					startup.SetRecording(!startup.Recording, true);
					break;
			}
		}

		public static bool Contains(HotkeyType type) => ConfigHandler.Config.Hotkeys.Any(c => c.Type == type);
		public static HotkeyConfig GetHotkey(HotkeyType type) => ConfigHandler.Config.Hotkeys.SingleOrDefault(c => c.Type == type);

		public void ChangeHotkey(HotkeyConfig config)
		{
			Hotkey previous = hotkeys.Where(h => h.HotkeyType == config.Type).SingleOrDefault();
			if (!(previous is null))
			{
				previous.Dispose();
				hotkeys.Remove(previous);
			}
			ConfigHandler.Config.Hotkeys.RemoveAll(c => c.Type == config.Type);

			Hotkey newHotkey = new Hotkey(config.ModifierKeys, config.Key, config.Type);
			newHotkey.Pressed += H_Pressed;
			hotkeys.Add(newHotkey);
			newHotkey.Register();
			ConfigHandler.Config.Hotkeys.Add(config);
			ConfigHandler.Save();
		}

		public void RemoveHotkey(HotkeyType type)
		{
			Hotkey previous = hotkeys.Where(h => h.HotkeyType == type).SingleOrDefault();
			if (!(previous is null))
			{
				previous.Dispose();
				hotkeys.Remove(previous);
			}
			ConfigHandler.Config.Hotkeys.RemoveAll(c => c.Type != type);
			ConfigHandler.Save();
		}
	}
}
