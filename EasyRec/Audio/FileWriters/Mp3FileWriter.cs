using NAudio.Lame;
using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyRec.Audio.FileWriters
{
	class Mp3FileWriter : MFFileWriter
	{
		public Mp3FileWriter(string path, ThroughputDescription description) : base(path, description) { }

		protected override MediaType GetMediaType(WaveFormat waveFormat) => MediaFoundationEncoder.SelectMediaType(
					AudioSubtypes.MFAudioFormat_MP3,
					waveFormat,
					128000);
		protected override string NormalizePath(string path)
		{
			if (!path.EndsWith(".mp3"))
				path += ".mp3";
			return path;
		}
	}
}
