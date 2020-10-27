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
		private MediaFoundationResampler resampler = null;

		public ConvertingWaveProvider(IWaveProvider waveProvider, WaveFormat targetFormat, bool ignoreChannels = false)
		{

			if (waveProvider.WaveFormat.SampleRate != targetFormat.SampleRate)
			{
				if (waveProvider.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
					provider = resampler = new MediaFoundationResampler(waveProvider, targetFormat);
				else
					provider = resampler = new MediaFoundationResampler(new Wave16ToFloatProvider(waveProvider), targetFormat);
			}
			else
			{
				provider = waveProvider;
			}

			if (provider.WaveFormat.Encoding != targetFormat.Encoding)
			{
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
				provider = new PCMConversionWaveProvider(provider.ToSampleProvider(), targetFormat.BitsPerSample / 8);
			}

			if (!ignoreChannels && provider.WaveFormat.Channels != targetFormat.Channels)
			{
				if (targetFormat.Channels == 1 & provider.WaveFormat.Channels == 2)
					provider = new StereoToMonoProvider16(provider);
				else if (targetFormat.Channels == 2)
					provider = new MonoToStereoProvider16(provider);
				else
					throw new InvalidWaveFormatException($"Couldn´t find a suitable conversion from {provider.WaveFormat.Channels} to {targetFormat.Channels} Channels");
			}
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


