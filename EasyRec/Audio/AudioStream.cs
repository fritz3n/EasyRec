using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio
{
	struct AudioStream
	{
		public AudioStream(string name, IWaveProvider waveProvider)
		{
			Name = name;
			WaveProvider = waveProvider;
		}

		public string Name { get; }
		public IWaveProvider WaveProvider { get; }

		public StreamDescription Description => new StreamDescription(Name, WaveProvider.WaveFormat);

		public AudioStream WithProvider(IWaveProvider waveProvider) => new AudioStream(Name, waveProvider);
	}
}
