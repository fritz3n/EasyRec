using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio
{
	struct AudioFragment
	{
		public AudioFragment(StreamDescription streamDescription, byte[] audioData, int count)
		{
			StreamDescription = streamDescription;
			AudioData = audioData;
			Count = count;
		}

		public StreamDescription StreamDescription { get; }
		public byte[] AudioData { get; }
		public int Count { get; set; }
	}
}
