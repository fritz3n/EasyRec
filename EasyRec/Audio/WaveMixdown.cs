using EasyRec.Audio.Conversion;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyRec.Audio
{
	class WaveMixdown
	{
		public List<AudioStream> AudioStreams { get; private set; }

		public WaveMixdown(IEnumerable<AudioStream> audioStreams, bool keepOriginals)
		{

			List<ISampleProvider> inputs;

			if (keepOriginals)
			{
				inputs = new List<ISampleProvider>();
				AudioStreams = new List<AudioStream>();
				foreach (AudioStream input in audioStreams)
				{
					WaveSplitter splitter = new WaveSplitter(input.WaveProvider, 2);
					inputs.Add(splitter.Splits[0].ToSampleProvider().ToStereo());
					AudioStreams.Add(input.WithProvider(splitter.Splits[1]));
				}
			}
			else
			{
				AudioStreams = new List<AudioStream>();
				inputs = audioStreams.Select(a => a.WaveProvider.ToSampleProvider().ToStereo()).ToList();
			}

			MixingSampleProvider mixer = new MixingSampleProvider(inputs);

			IWaveProvider mixerProvider = mixer.ToWaveProvider16();
			int targetBits = audioStreams.First().WaveProvider.WaveFormat.BitsPerSample;
			if (mixerProvider.WaveFormat.BitsPerSample != targetBits)
				mixerProvider = new PCMConversionWaveProvider(mixerProvider.ToSampleProvider(), targetBits / 8);


			AudioStreams.Add(new AudioStream("Mixdown", mixerProvider));
		}
	}
}
