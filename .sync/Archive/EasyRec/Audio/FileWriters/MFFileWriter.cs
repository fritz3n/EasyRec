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
		ManualResetEvent stopEvent = new ManualResetEvent(false);
		private MediaFoundationWriter writer;
		private Thread thread;
		CancellationTokenSource tokenSource = new CancellationTokenSource();
		IList<AudioFragment> audioFragments;

		public MFFileWriter(string path, ThroughputDescription description) : base(path, description)
		{
			thread = new Thread(DoWork);
			thread.Start();
			dataAvailableEvent.WaitOne();
		}

		public override void ReceiveSamples(IList<AudioFragment> audioFragments)
		{
			this.audioFragments = audioFragments;
			dataAvailableEvent.Set();
		}

		protected abstract MediaType GetMediaType(WaveFormat waveFormat);
		protected abstract string NormalizePath(string path);

		protected override MediaFoundationWriter GetEncoder(string path, WaveFormat waveFormat)
		{

			path = NormalizePath(path);

			MediaType mediaType = GetMediaType(waveFormat);

			MediaFoundationWriter encoder = new MediaFoundationWriter(mediaType, waveFormat, path);
			return encoder;
		}

		private void DoWork()
		{
			for (int i = 0; i < encoders.Length; i++)
				encoders[i].StartEncode();
			dataAvailableEvent.Set();

			try
			{
				while (true)
				{
					int signaled = WaitHandle.WaitAny(new WaitHandle[] { dataAvailableEvent, tokenSource.Token.WaitHandle });
					if (signaled == 1)
						throw new Exception();
					for (int i = 0; i < audioFragments.Count; i++)
					{
						encoders[i].Write(audioFragments[i].AudioData, 0, audioFragments[i].Count);
					}
				}
			}
			catch (Exception e) { }
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
