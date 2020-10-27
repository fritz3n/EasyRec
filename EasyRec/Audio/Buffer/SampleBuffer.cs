using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio.Buffer
{
	class SampleBuffer : ISampleReceiver
	{
		BufferedWaveProvider[] buffers;
		byte[] discardBuffer = new byte[1000];
		private readonly ThroughputDescription description;
		public TimeSpan BufferedTime => buffers[0].BufferedDuration;
		public SampleBuffer(ThroughputDescription desc, float length)
		{
			buffers = new BufferedWaveProvider[desc.Count];
			for (int i = 0; i < desc.Count; i++)
			{
				buffers[i] = new BufferedWaveProvider(desc[i].WaveFormat)
				{
					BufferDuration = TimeSpan.FromSeconds(length)
				};
			}

			description = desc;
		}

		public void ReceiveSamples(IList<AudioFragment> audioFragments)
		{
			for (int i = 0; i < audioFragments.Count; i++)
			{
				int read = audioFragments[i].Count - (buffers[i].BufferLength - buffers[i].BufferedBytes);
				if (read > discardBuffer.Length)
					discardBuffer = new byte[read];
				if (read > 0)
					buffers[i].Read(discardBuffer, 0, read);
				buffers[i].AddSamples(audioFragments[i].AudioData, 0, audioFragments[i].Count);
			}
		}

		public List<AudioFragment> ReadContents()
		{
			List<AudioFragment> output = new List<AudioFragment>();
			for (int i = 0; i < buffers.Length; i++)
			{
				int doRead = buffers[i].BufferedBytes;
				byte[] buffer = new byte[doRead];
				int read = buffers[i].Read(buffer, 0, doRead);
				output.Add(new AudioFragment(description[i], buffer, read));
			}
			return output;
		}
	}
}
