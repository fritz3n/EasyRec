using EasyRec.Audio.Conversion;
using NAudio.Gui;
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
		private VolumeMeterProvider volumeMeter;
		public float Volume => volumeMeter.StreamVolume;

		public bool VolumeActive { get => volumeMeter.Active; set => volumeMeter.Active = value; }

		public WaveMixdown(IEnumerable<AudioStream> audioStreams, bool keepOriginals)
		{

			List<ISampleProvider> inputs;

			if (keepOriginals)
			{
				inputs = new List<ISampleProvider>();
				AudioStreams = new List<AudioStream>();
				foreach (AudioStream input in audioStreams)
				{
					var splitter = new WaveSplitter(input.WaveProvider, 2);
					inputs.Add(splitter.Splits[0].ToSampleProvider().ToStereo());
					AudioStreams.Add(input.WithProvider(splitter.Splits[1]));
				}
			}
			else
			{
				AudioStreams = new List<AudioStream>();
				inputs = audioStreams.Select(a => a.WaveProvider.ToSampleProvider().ToStereo()).ToList();
			}

			var mixer = new MixingSampleProvider(inputs);
			volumeMeter = new VolumeMeterProvider(mixer)
			{
				Active = false
			};
			IWaveProvider mixerProvider;
			int targetBits = audioStreams.First().WaveProvider.WaveFormat.BitsPerSample;


			if (targetBits != 16)
				mixerProvider = new PCMConversionWaveProvider(volumeMeter, targetBits / 8);
			else
				mixerProvider = volumeMeter.ToWaveProvider16();

			AudioStreams.Add(new AudioStream("Mixdown", mixerProvider));
		}
	}
}
