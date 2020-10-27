using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio.Buffer
{
	class BufferedSampleProvider : ISampleProvider
	{
		private CircularSampleBuffer circularBuffer;

		/// <summary>
		/// Creates a new buffered WaveProvider
		/// </summary>
		/// <param name="waveFormat">WaveFormat</param>
		public BufferedSampleProvider(WaveFormat waveFormat)
		{
			WaveFormat = waveFormat;
			BufferLength = waveFormat.AverageBytesPerSecond * 5;
			ReadFully = true;
		}

		/// <summary>
		/// If true, always read the amount of data requested, padding with zeroes if necessary
		/// By default is set to true
		/// </summary>
		public bool ReadFully { get; set; }

		/// <summary>
		/// Buffer length in bytes
		/// </summary>
		public int BufferLength { get; set; }

		/// <summary>
		/// Buffer duration
		/// </summary>
		public TimeSpan BufferDuration
		{
			get => TimeSpan.FromSeconds((double)BufferLength / WaveFormat.SampleRate);
			set => BufferLength = (int)(value.TotalSeconds * WaveFormat.SampleRate);
		}

		/// <summary>
		/// If true, when the buffer is full, start throwing away data
		/// if false, AddSamples will throw an exception when buffer is full
		/// </summary>
		public bool DiscardOnBufferOverflow { get; set; }

		/// <summary>
		/// The number of buffered bytes
		/// </summary>
		public int BufferedBytes => circularBuffer == null ? 0 : circularBuffer.Count;

		/// <summary>
		/// Buffered Duration
		/// </summary>
		public TimeSpan BufferedDuration => TimeSpan.FromSeconds((double)BufferedBytes / WaveFormat.SampleRate);

		/// <summary>
		/// Gets the WaveFormat
		/// </summary>
		public WaveFormat WaveFormat { get; }

		/// <summary>
		/// Adds samples. Takes a copy of buffer, so that buffer can be reused if necessary
		/// </summary>
		public void AddSamples(float[] buffer, int offset, int count)
		{
			// create buffer here to allow user to customise buffer length
			if (circularBuffer == null)
			{
				circularBuffer = new CircularSampleBuffer(BufferLength);
			}

			int written = circularBuffer.Write(buffer, offset, count);
			if (written < count && !DiscardOnBufferOverflow)
			{
				throw new InvalidOperationException("Buffer full");
			}
		}

		/// <summary>
		/// Reads from this WaveProvider
		/// Will always return count bytes, since we will zero-fill the buffer if not enough available
		/// </summary>
		public int Read(float[] buffer, int offset, int count)
		{
			int read = 0;
			if (circularBuffer != null) // not yet created
			{
				read = circularBuffer.Read(buffer, offset, count);
			}
			if (ReadFully && read < count)
			{
				// zero the end of the buffer
				Array.Clear(buffer, offset + read, count - read);
				read = count;
			}
			return read;
		}

		/// <summary>
		/// Discards all audio from the buffer
		/// </summary>
		public void ClearBuffer()
		{
			if (circularBuffer != null)
			{
				circularBuffer.Reset();
			}
		}
	}
}
