using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio
{
	class VolumeMeterProvider : ISampleProvider
	{
		private readonly ISampleProvider source;



		public float StreamVolume { get; set; }
		public bool Active { get; set; } = true;

		/// <summary>
		/// Initialises a new instance of VolumeMeterProvider 
		/// </summary>
		/// <param name="source">source sampler provider</param>
		public VolumeMeterProvider(ISampleProvider source)
		{
			this.source = source;
		}

		/// <summary>
		/// The WaveFormat of this sample provider
		/// </summary>
		public WaveFormat WaveFormat => source.WaveFormat;

		/// <summary>
		/// Reads samples from this Sample Provider
		/// </summary>
		/// <param name="buffer">Sample buffer</param>
		/// <param name="offset">Offset into sample buffer</param>
		/// <param name="count">Number of samples required</param>
		/// <returns>Number of samples read</returns>
		public int Read(float[] buffer, int offset, int count)
		{
			int samplesRead = source.Read(buffer, offset, count);

			if (Active)
			{
				float max = 0;
				for (int index = 0; index < samplesRead; index++)
				{
					float sampleValue = Math.Abs(buffer[offset + index]);
					max = Math.Max(max, sampleValue);
				}

				if (max > StreamVolume)
					StreamVolume = max;
				else
					StreamVolume = (StreamVolume + max) / 2;
			}
			else
			{
				StreamVolume = 0;
			}

			return samplesRead;
		}
	}
}
