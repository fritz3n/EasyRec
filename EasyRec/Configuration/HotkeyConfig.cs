using EasyRec.Hotkeys;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace EasyRec.Configuration
{
	public class HotkeyConfig
	{
		public HotkeyConfig(ModifierKeys modifierKeys, Keys key, HotkeyType type)
		{
			ModifierKeys = modifierKeys;
			Key = key;
			Type = type;
		}

		[JsonConverter(typeof(StringEnumConverter))]
		public ModifierKeys ModifierKeys { get; }

		[JsonConverter(typeof(StringEnumConverter))]
		public Keys Key { get; }

		[JsonConverter(typeof(StringEnumConverter))]
		public HotkeyType Type { get; }
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum HotkeyType
	{
		SaveBuffer,
		StartBuffer,
		StopBuffer,
		ToggleBuffer,

		StartRecording,
		StopRecording,
		ToggleRecording,
	}
}
