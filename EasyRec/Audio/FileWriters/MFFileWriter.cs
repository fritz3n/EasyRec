using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasyRec.Audio.FileWriters
{
	abstract class MFFileWriter : FileWriter<MediaFoundationWriter>
	{
		AutoResetEvent dataAvailableEvent = new AutoResetEvent(false);
		AutoResetEvent dataProcessedEvent = new AutoResetEvent(false);
		ManualResetEvent stopEvent = new ManualResetEvent(false);
		private Thread thread;
		CancellationTokenSource tokenSource = new CancellationTokenSource();
		IList<AudioFragment> audioFragments;
		readonly List<string> paths = new List<string>();

		public MFFileWriter(string path, ThroughputDescription description) : base(path, description)
		{
			// We always need to write using the same thread for MediaFoundation to function properly
			thread = new Thread(DoWork);
			thread.Start();
			dataProcessedEvent.WaitOne();
		}

		protected override void AddSamples(IList<AudioFragment> audioFragments)
		{
			this.audioFragments = audioFragments;
			dataAvailableEvent.Set();
			dataProcessedEvent.WaitOne();
		}

		protected abstract MediaType GetMediaType(WaveFormat waveFormat);
		protected abstract string NormalizePath(string path);

		protected override MediaFoundationWriter GetEncoder(string path, WaveFormat waveFormat)
		{
			paths.Add(path);
			return null;
		}

		private void DoWork()
		{
			for (int i = 0; i < encoders.Length; i++)
			{
				string path = NormalizePath(paths[i]);
				MediaType mediaType = GetMediaType(description[i].WaveFormat);
				encoders[i] = new MediaFoundationWriter(mediaType, description[i].WaveFormat, path);
				encoders[i].StartEncode();
			}
			dataProcessedEvent.Set();

			try
			{
				while (true)
				{
					int signaled = WaitHandle.WaitAny(new WaitHandle[] { dataAvailableEvent, tokenSource.Token.WaitHandle });
					if (signaled == 1)
						break;
					for (int i = 0; i < audioFragments.Count; i++)
					{
						if (audioFragments[i].Count > 0)
							encoders[i].Write(audioFragments[i].AudioData, 0, audioFragments[i].Count);
					}
					dataProcessedEvent.Set();
				}
			}
			finally
			{
				for (int i = 0; i < encoders.Length; i++)
					encoders[i].StopEncode();
				stopEvent.Set();
			}
		}


		public override void Stop()
		{
			tokenSource.Cancel();
			stopEvent.WaitOne();
		}

	}
}
