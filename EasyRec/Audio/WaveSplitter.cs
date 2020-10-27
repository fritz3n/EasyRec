using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio
{
	class WaveSplitter
	{
		private readonly IWaveProvider input;
		private SplitProvider[] splitters;
		private byte[] readBuffer;

		public IReadOnlyList<IWaveProvider> Splits => splitters;

		public WaveSplitter(IWaveProvider input, int count)
		{
			this.input = input;
			splitters = new SplitProvider[count];
			for (int i = 0; i < count; i++)
			{
				splitters[i] = new SplitProvider(this);
			}
			readBuffer = new byte[input.WaveFormat.AverageBytesPerSecond];
		}

		private void Read(int count)
		{
			int read = input.Read(readBuffer, 0, count);
			for (int i = 0; i < splitters.Length; i++)
				splitters[i].waveBuffer.AddSamples(readBuffer, 0, read);
		}

		private class SplitProvider : IWaveProvider
		{
			private readonly WaveSplitter parent;
			public BufferedWaveProvider waveBuffer;

			public SplitProvider(WaveSplitter parent)
			{
				this.parent = parent;
				waveBuffer = new BufferedWaveProvider(WaveFormat) { BufferLength = WaveFormat.AverageBytesPerSecond };
			}

			public WaveFormat WaveFormat => parent.input.WaveFormat;

			public int Read(byte[] buffer, int offset, int count)
			{
				if (waveBuffer.BufferedBytes < count - offset)
					parent.Read((count - offset) - waveBuffer.BufferedBytes);
				return waveBuffer.Read(buffer, offset, count);
			}
		}
	}
}
