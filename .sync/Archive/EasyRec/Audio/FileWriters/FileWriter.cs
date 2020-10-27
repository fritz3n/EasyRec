using EasyRec.Configuration;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EasyRec.Audio.FileWriters
{

	abstract class FileWriter<T> : FileWriter where T : Stream
	{
		protected T[] encoders;

		public FileWriter(string path, ThroughputDescription description)
		{
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			encoders = new T[description.Count];
			for (int i = 0; i < description.Count; i++)
			{
				encoders[i] = GetEncoder(Path.Combine(path, description[i].Name), description[i].WaveFormat);
			}
		}

		public override void ReceiveSamples(IList<AudioFragment> audioFragments)
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
		public abstract void ReceiveSamples(IList<AudioFragment> audioFragments);
		public abstract void Stop();
		public abstract void Dispose();
		public static FileWriter GetFileWriter(FileWriterType type, string path, ThroughputDescription description) => type switch
		{
			FileWriterType.Aac => new AacFileWriter(path, description),
			FileWriterType.Mp3 => new Mp3FileWriter(path, description),
			FileWriterType.Wav => new WavFileWriter(path, description),
		};
	}
}
