using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Configuration
{
	class Config
	{
		private List<HotkeyConfig> hotkeys;

		public string BufferPath { get; set; }
		public string RecordPath { get; set; }
		public string BufferPattern { get; set; }
		public string RecordPattern { get; set; }


		public FileWriterType BufferWriter { get; set; }
		public FileWriterType RecordWriter { get; set; }
		public string[] Inputs { get; set; }
		public MixdownType MixdownType { get; set; }
		public float BufferLength { get; set; }

		public int SampleRate { get; set; }
		public int Bits { get; set; }

		public bool BufferOnStart { get; set; }
		public bool RecordOnStart { get; set; }

		public List<HotkeyConfig> Hotkeys { get => hotkeys ?? new List<HotkeyConfig>(); set => hotkeys = value ?? new List<HotkeyConfig>(); }
	}
}
