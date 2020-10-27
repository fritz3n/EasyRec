using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio
{
	struct StreamDescription
	{
		public StreamDescription(string name, WaveFormat waveFormat)
		{
			Name = name;
			WaveFormat = waveFormat;
		}

		public string Name { get; }
		public WaveFormat WaveFormat { get; }
	}
}
