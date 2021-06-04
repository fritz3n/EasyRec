using log4net;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRec.Audio.Conversion
{
	class ConvertingWaveProvider : IWaveProvider, IDisposable
	{
		IWaveProvider provider;
		private static ILog log = LogManager.GetLogger(nameof(ConvertingWaveProvider));
		private MediaFoundationResampler resampler = null;

		public ConvertingWaveProvider(IWaveProvider waveProvider, WaveFormat targetFormat, bool ignoreChannels = false)
		{
			int chain = 0;
			log.Info($"New ConvertingWaveProvider for '{waveProvider}' from '{waveProvider.WaveFormat.ToStringBetter()}' to '{targetFormat.ToStringBetter()}'");

			if (waveProvider.WaveFormat.SampleRate != targetFormat.SampleRate)
			{
				chain++;
				log.Info($"-> using MediaFoundationResampler");
				if (waveProvider.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
					provider = new WrappedProvider(resampler = new MediaFoundationResampler(waveProvider, targetFormat));
				else
					provider = new WrappedProvider(resampler = new MediaFoundationResampler(new Wave16ToFloatProvider(waveProvider), targetFormat));
			}
			else
			{
				provider = waveProvider;
			}

			if (provider.WaveFormat.Encoding != targetFormat.Encoding)
			{
				chain++;
				provider = (provider.WaveFormat.Encoding, targetFormat.Encoding) switch
				{
					(WaveFormatEncoding.Pcm, WaveFormatEncoding.IeeeFloat) => provider.ToSampleProvider().ToWaveProvider(),
					(WaveFormatEncoding.IeeeFloat, WaveFormatEncoding.Pcm) when targetFormat.BitsPerSample == 16 => new WaveFloatTo16Provider(provider),
					(WaveFormatEncoding.IeeeFloat, WaveFormatEncoding.Pcm) => new PCMConversionWaveProvider(provider.ToSampleProvider(), targetFormat.BitsPerSample / 8),
					_ => throw new InvalidWaveFormatException($"Couldn´t find a suitable conversion from {provider.WaveFormat.Channels} to {targetFormat.Channels} Channels")
				};
			}

			if (provider.WaveFormat.Encoding == WaveFormatEncoding.Pcm && provider.WaveFormat.BitsPerSample != targetFormat.BitsPerSample)
			{
				chain++;
				provider = new PCMConversionWaveProvider(provider.ToSampleProvider(), targetFormat.BitsPerSample / 8);
			}

			if (!ignoreChannels && provider.WaveFormat.Channels != targetFormat.Channels)
			{
				chain++;
				if (targetFormat.Channels == 1 & provider.WaveFormat.Channels == 2)
					provider = new StereoToMonoProvider16(provider);
				else if (targetFormat.Channels == 2)
					provider = new MonoToStereoProvider16(provider);
				else
					throw new InvalidWaveFormatException($"Couldn´t find a suitable conversion from {provider.WaveFormat.Channels} to {targetFormat.Channels} Channels");
			}
			log.Info($"-> Conversion successfull using {chain} links");
		}

		public WaveFormat WaveFormat => provider.WaveFormat;

		public void Dispose() => resampler?.Dispose();
		public int Read(byte[] buffer, int offset, int count) => provider.Read(buffer, offset, count);
	}

	public class InvalidWaveFormatException : Exception
	{
		public InvalidWaveFormatException(string message) : base(message)
		{
		}

		public InvalidWaveFormatException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}


