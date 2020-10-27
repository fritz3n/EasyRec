using EasyRec.Audio.Conversion;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio
{
	class WaveConverter
	{

		public List<AudioStream> AudioStreams { get; private set; } = new List<AudioStream>();

		public WaveConverter(IEnumerable<AudioStream> audioStreams, WaveFormat targetWaveFormat)
		{
			foreach (AudioStream stream in audioStreams)
			{
				AudioStreams.Add(stream.WithProvider(new ConvertingWaveProvider(stream.WaveProvider, targetWaveFormat, true)));
			}
		}
	}
}
