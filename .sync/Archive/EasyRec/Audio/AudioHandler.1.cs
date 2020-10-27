using EasyRec.Audio.Buffer;
using EasyRec.Audio.FileWriters;
using EasyRec.Configuration;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EasyRec.Audio
{
	class AudioHandler : IDisposable
	{
		private WasapiProvider[] inputs;
		private SampleBuffer buffer;
		private FileWriter recorder;
		private SamplePusher pusher;
		private ThroughputDescription description;

		public bool Recording { get; private set; }
		public bool Buffering { get; private set; }
		private bool active = false;


		public void BuildPipeline()
		{
			StopRecording();
			StopBuffer();
			Deactivate();

			Config config = ConfigHandler.Config;

			if (config.Inputs.Length == 0)
				throw new PipelineBuildException("No Inputs in Config");

			inputs = new WasapiProvider[config.Inputs.Length];
			for (int i = 0; i < inputs.Length; i++)
			{
				inputs[i] = new WasapiProvider(config.Inputs[i]);
			}

			WaveFormat targetFormat = new WaveFormat(config.SampleRate, config.Bits, 2);

			WaveConverter converter = new WaveConverter(inputs.Select(i => i.AudioStream), targetFormat);
			List<AudioStream> streams;

			if (config.MixdownType == MixdownType.OnlyOriginal)
				streams = converter.AudioStreams;
			else
				streams = new WaveMixdown(converter.AudioStreams, config.MixdownType == MixdownType.Both).AudioStreams;

			description = ThroughputDescription.FromStreams(streams);
			pusher = new SamplePusher(streams, new List<ISampleReceiver>());

		}

		public void StartRecording()
		{
			if (Recording)
				return;
			Recording = true;
			Activate();
			Config conf = ConfigHandler.Config;
			string path = Path.Combine(conf.RecordPath, PathFormatter.Format(conf.RecordPattern));
			recorder = FileWriter.GetFileWriter(conf.RecordWriter, path, description);
			pusher.Destinations.Add(recorder);
		}
		public void StopRecording()
		{
			if (!Recording)
				return;
			Recording = false;
			if (!Buffering)
				Deactivate();
			pusher.Destinations.Remove(recorder);
			recorder.Dispose();
			recorder = null;
		}
		public void StartBuffer()
		{
			if (Buffering)
				return;
			Buffering = true;
			Activate();
			Config conf = ConfigHandler.Config;
			string path = Path.Combine(conf.BufferPath, PathFormatter.Format(conf.BufferPattern));
			buffer = new SampleBuffer(description, conf.BufferLength);
			pusher.Destinations.Add(buffer);
		}
		public void StopBuffer()
		{
			if (!Buffering)
				return;
			Buffering = false;
			if (!Recording)
				Deactivate();
			pusher.Destinations.Remove(buffer);
			buffer = null;
		}

		public void SaveBuffer()
		{
			if (!Buffering)
				throw new InvalidOperationException("Buffer is not active");

			Config conf = ConfigHandler.Config;
			string path = Path.Combine(conf.BufferPath, PathFormatter.Format(conf.BufferPattern));
			FileWriter writer = FileWriter.GetFileWriter(conf.BufferWriter, path, description);
			List<AudioFragment> fragments = buffer.ReadContents();
			writer.ReceiveSamples(fragments);
			writer.Dispose();
		}

		private void Activate()
		{
			if (active)
				return;
			foreach (WasapiProvider inputs in inputs)
			{
				inputs.StartRecording();
			}
			pusher.Start();
			active = true;
		}

		private void Deactivate()
		{
			if (!active)
				return;
			pusher.Stop();
			foreach (WasapiProvider inputs in inputs)
			{
				inputs.StopRecording();
			}
			active = false;
		}

		public void Dispose()
		{
			StopRecording();
			StopBuffer();
			Deactivate();

			foreach (WasapiProvider input in inputs)
				input?.Dispose();
			recorder?.Dispose();
			recorder = null;
			buffer = null;
		}
	}

	public class PipelineBuildException : Exception
	{
		public PipelineBuildException()
		{
		}

		public PipelineBuildException(string message) : base(message)
		{
		}

		public PipelineBuildException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PipelineBuildException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
