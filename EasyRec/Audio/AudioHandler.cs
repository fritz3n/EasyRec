using EasyRec.Audio.Buffer;
using EasyRec.Audio.FileWriters;
using EasyRec.Configuration;
using log4net;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRec.Audio
{
	class AudioHandler : IDisposable
	{
		private static ILog log = LogManager.GetLogger(nameof(AudioHandler));
		private WasapiProvider[] inputs;
		private SampleBuffer buffer;
		private FileWriter recorder;
		private SamplePusher pusher;
		private ThroughputDescription description;
		private WaveMixdown mixdown = null;

		public float Volume => mixdown?.Volume ?? 0;

		public bool VolumeActive { get => mixdown?.VolumeActive ?? false; set { if (mixdown is not null) mixdown.VolumeActive = value; } }

		public TimeSpan RecordedTime => recorder?.RecordedTime ?? TimeSpan.Zero;
		public TimeSpan BufferedTime => buffer?.BufferedTime ?? TimeSpan.Zero;

		public bool Recording { get; private set; }
		public bool Buffering { get; private set; }
		private bool active = false;
		private bool savingBuffer = false;

		public void BuildPipeline()
		{
			StopRecording();
			StopBuffer();
			Deactivate();

			log.Info("Starting audio");

			Config config = ConfigHandler.Config;

			if (config.Inputs.Length == 0)
				throw new PipelineBuildException("No Inputs in Config");

			var inputList = new List<WasapiProvider>();
			inputs = new WasapiProvider[config.Inputs.Length];
			foreach (string input in config.Inputs)
			{
				try
				{
					inputList.Add(new WasapiProvider(input));
				}
				catch (KeyNotFoundException) { } // Ignore devices which were not found
			}


			// Remove Providers with duplicate Names. Can happen if default ins/outs overlap with chosen ins/outs.
			foreach (WasapiProvider input in inputList.GroupBy(i => i.Name).Where(g => g.Count() > 1).SelectMany(g => g.Skip(1)))
			{
				input.Dispose();
				inputList.Remove(input);
			}

			inputs = inputList.ToArray();

			log.Info("Inputs:\n\t" + string.Join("\n\t", inputs.Select(i =>
			{
				return $"{i.Name} - {i.WaveFormat.ToStringBetter()}";
			})));

			var targetFormat = new WaveFormat(config.SampleRate, config.Bits, 2);

			var converter = new WaveConverter(inputs.Select(i => i.AudioStream), targetFormat);
			List<AudioStream> streams;

			if (config.MixdownType == MixdownType.OnlyOriginal)
				streams = converter.AudioStreams;
			else
				streams = (mixdown = new WaveMixdown(converter.AudioStreams, config.MixdownType == MixdownType.Both)).AudioStreams;


			log.Info("Streams:\n\t" + string.Join("\n\t", streams.Select(s =>
			{
				return $"{s.Name} - {s.WaveProvider.WaveFormat.ToStringBetter()}";
			})));

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

		public async Task<TimeSpan> SaveBuffer()
		{
			if (!Buffering)
				throw new InvalidOperationException("Buffer is not active");

			if (savingBuffer)
				return TimeSpan.Zero;
			savingBuffer = true;

			Config conf = ConfigHandler.Config;
			string path = Path.Combine(conf.BufferPath, PathFormatter.Format(conf.BufferPattern));
			var writer = FileWriter.GetFileWriter(conf.BufferWriter, path, description);
			List<AudioFragment> fragments = buffer.ReadContents();
			await writer.WriteIncremental(fragments);
			TimeSpan span = writer.RecordedTime;
			writer.Dispose();
			savingBuffer = false;
			return span;
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
