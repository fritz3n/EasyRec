using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio.FileWriters
{
	class AacFileWriter : MFFileWriter
	{
		public AacFileWriter(string path, ThroughputDescription description) : base(path, description) { }

		protected override MediaType GetMediaType(WaveFormat waveFormat) => MediaFoundationEncoder.SelectMediaType(
					AudioSubtypes.MFAudioFormat_AAC,
					waveFormat,
					128000);
		protected override string NormalizePath(string path)
		{
			if (!path.EndsWith(".aac") && !path.EndsWith(".m4a"))
				path += ".aac";
			return path;
		}
	}
}
