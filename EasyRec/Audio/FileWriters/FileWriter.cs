using EasyRec.Configuration;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyRec.Audio.FileWriters
{

	abstract class FileWriter<T> : FileWriter where T : Stream
	{
		protected T[] encoders;
		protected readonly ThroughputDescription description;

		public FileWriter(string path, ThroughputDescription description)
		{
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			encoders = new T[description.Count];
			for (int i = 0; i < description.Count; i++)
			{
				encoders[i] = GetEncoder(Path.Combine(path, description[i].Name), description[i].WaveFormat);
			}

			this.description = description;
		}

		protected override void AddSamples(IList<AudioFragment> audioFragments)
		{
			for (int i = 0; i < audioFragments.Count; i++)
			{
				encoders[i].Write(audioFragments[i].AudioData, 0, audioFragments[i].Count);
			}
		}

		protected abstract T GetEncoder(string path, WaveFormat waveFormat);
		public override void Stop()
		{
			foreach (T encoder in encoders)
			{
				encoder?.Flush();
			}
		}

		public override void Dispose()
		{
			Stop();
			foreach (T encoder in encoders)
			{
				encoder?.Dispose();
			}
		}

	}

	abstract class FileWriter : ISampleReceiver, IDisposable
	{
		protected abstract bool IncrementalWrite { get; }
		public TimeSpan RecordedTime { get; set; } = new TimeSpan();
		public void ReceiveSamples(IList<AudioFragment> audioFragments)
		{
			RecordedTime += TimeSpan.FromSeconds((double)audioFragments[0].Count / audioFragments[0].StreamDescription.WaveFormat.AverageBytesPerSecond);
			AddSamples(audioFragments);
		}

		public async Task WriteIncremental(IList<AudioFragment> audioFragments) =>
			await Task.Run(() =>
			{
				if (IncrementalWrite)
				{
					byte[][] buffers = new byte[audioFragments.Count][];
					int[] position = new int[audioFragments.Count];
					var fragments = new AudioFragment[audioFragments.Count];

					for (int i = 0; i < audioFragments.Count; i++)
					{
						buffers[i] = new byte[audioFragments[i].StreamDescription.WaveFormat.AverageBytesPerSecond];
						fragments[i] = new AudioFragment(audioFragments[i].StreamDescription, buffers[i], 0);
					}
					bool dataLeft = false;

					while (true)
					{
						dataLeft = false;
						for (int i = 0; i < audioFragments.Count; i++)
						{
							int read = Math.Min(audioFragments[i].Count - position[i], audioFragments[i].StreamDescription.WaveFormat.AverageBytesPerSecond);
							if (read > 0)
							{
								dataLeft = true;
								Array.Copy(audioFragments[i].AudioData, position[i], buffers[i], 0, read);
								fragments[i].Count = read;
								position[i] += read;
							}
							else
							{
								fragments[i].Count = 0;
							}
						}
						if (dataLeft)
							ReceiveSamples(fragments);
						else
							break;
					}
				}
				else
				{
					ReceiveSamples(audioFragments);
				}
			});

		protected abstract void AddSamples(IList<AudioFragment> audioFragments);
		public abstract void Stop();
		public abstract void Dispose();
		public static FileWriter GetFileWriter(FileWriterType type, string path, ThroughputDescription description) => type switch
		{
			FileWriterType.Aac => new AacFileWriter(path, description),
			FileWriterType.Mp3 => new Mp3FileWriter(path, description),
			FileWriterType.Wav => new WavFileWriter(path, description),
			FileWriterType.Mp3Lame => new Mp3LameFileWriter(path, description),
		};
	}
}
