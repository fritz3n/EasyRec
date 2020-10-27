using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio.FileWriters
{
	class WavFileWriter : FileWriter<WaveFileWriter>
	{
		public WavFileWriter(string path, ThroughputDescription description) : base(path, description)
		{

		}

		protected override WaveFileWriter GetEncoder(string path, WaveFormat waveFormat)
		{
			if (!path.EndsWith(".wav"))
				path += ".wav";
			return new WaveFileWriter(path, waveFormat);
		}
	}
}
