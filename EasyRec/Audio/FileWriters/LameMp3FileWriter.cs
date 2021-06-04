using log4net;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRec.Audio.FileWriters
{
	class Mp3LameFileWriter : FileWriter<LameMP3FileWriter>
	{
		private static ILog log = LogManager.GetLogger(nameof(Mp3LameFileWriter));
		public Mp3LameFileWriter(string path, ThroughputDescription description) : base(path, description)
		{

		}
		protected override bool IncrementalWrite => false;

		protected override LameMP3FileWriter GetEncoder(string path, WaveFormat waveFormat)
		{
			if (!path.EndsWith(".mp3"))
				path += ".mp3";
			var encoder = new LameMP3FileWriter(path, waveFormat, LAMEPreset.VBR_90);
			encoder.OnProgress += Encoder_OnProgress;
			encoder.SetDebugFunction(m => log.Debug("Debug: " + m));
			encoder.SetErrorFunction(m => log.Debug("Error: " + m));
			encoder.SetMessageFunction(m => log.Debug("Message: " + m));
			return encoder;
		}

		private void Encoder_OnProgress(object writer, long inputBytes, long outputBytes, bool finished)
		{
			log.Debug($"Progress: {inputBytes} of {outputBytes}, finished: {finished}");
		}

	}
}
