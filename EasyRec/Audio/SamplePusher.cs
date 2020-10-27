using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasyRec.Audio
{
	class SamplePusher
	{
		private readonly IList<AudioStream> sources;
		public IList<ISampleReceiver> Destinations { get; }
		private Timer timer;
		AudioFragment[] fragments = null;

		int[] lengths;

		public bool Running { get; private set; } = false;
		public float PusherPeriod { get; }

		public SamplePusher(IList<AudioStream> sources, IList<ISampleReceiver> destinations, float pusherPeriod = 0.05f)
		{
			this.sources = sources;
			Destinations = destinations;
			PusherPeriod = pusherPeriod;
			timer = new Timer(PushSamples);

			fragments = new AudioFragment[sources.Count];
			lengths = new int[sources.Count];

			for (int i = 0; i < sources.Count; i++)
			{
				lengths[i] = (int)(sources[i].Description.WaveFormat.AverageBytesPerSecond * PusherPeriod);
				fragments[i] = new AudioFragment(sources[i].Description, new byte[lengths[i]], 0);
			}
		}

		public void Start()
		{
			if (Running)
				return;
			timer.Change(TimeSpan.FromSeconds(PusherPeriod * 4), TimeSpan.FromSeconds(PusherPeriod));
			Running = true;
		}

		public void Stop()
		{
			if (!Running)
				return;
			timer.Change(0, Timeout.Infinite);
			Running = false;
		}

		private void PushSamples(object state)
		{
			if (!Running)
				return;

			for (int i = 0; i < sources.Count; i++)
			{
				fragments[i].Count = sources[i].WaveProvider.Read(fragments[i].AudioData, 0, lengths[i]);
			}

			for (int i = 0; i < Destinations.Count; i++)
			{
				Destinations[i].ReceiveSamples(fragments);
			}
		}
	}
}
