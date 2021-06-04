using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasyRec.Audio
{
	class SamplePusher
	{
		private static ILog log = LogManager.GetLogger(nameof(SamplePusher));
		private readonly IList<AudioStream> sources;
		public IList<ISampleReceiver> Destinations { get; }
		private Timer timer;
		private int waiting = 0;
		private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
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
			Interlocked.Increment(ref waiting);
			if (waiting > 1)
			{
				log.Debug(waiting + " pushers waiting");
			}
			semaphoreSlim.Wait();
			Interlocked.Decrement(ref waiting);
			try
			{
				if (!Running)
					return;

				for (int i = 0; i < sources.Count; i++)
				{
					fragments[i].Count = sources[i].WaveProvider.Read(fragments[i].AudioData, 0, lengths[i]);
					if (fragments[i].Count == 0)
						log.Warn($"Read 0 samples from {sources[i].Name}");
				}

				for (int i = 0; i < Destinations.Count; i++)
				{
					Destinations[i].ReceiveSamples(fragments);
				}
			}
			finally
			{
				semaphoreSlim.Release();
			}
		}
	}
}
