using NAudio.MediaFoundation;
using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls.Primitives;

namespace EasyRec.Audio.FileWriters
{
	class MediaFoundationWriter : Stream, IDisposable
	{

		static MediaFoundationWriter()
		{
			MediaFoundationApi.Startup();
		}

		/// <summary>
		/// Queries the available bitrates for a given encoding output type, sample rate and number of channels
		/// </summary>
		/// <param name="audioSubtype">Audio subtype - a value from the AudioSubtypes class</param>
		/// <param name="sampleRate">The sample rate of the PCM to encode</param>
		/// <param name="channels">The number of channels of the PCM to encode</param>
		/// <returns>An array of available bitrates in average bits per second</returns>
		public static int[] GetEncodeBitrates(Guid audioSubtype, int sampleRate, int channels) => GetOutputMediaTypes(audioSubtype)
				.Where(mt => mt.SampleRate == sampleRate && mt.ChannelCount == channels)
				.Select(mt => mt.AverageBytesPerSecond * 8)
				.Distinct()
				.OrderBy(br => br)
				.ToArray();

		/// <summary>
		/// Gets all the available media types for a particular 
		/// </summary>
		/// <param name="audioSubtype">Audio subtype - a value from the AudioSubtypes class</param>
		/// <returns>An array of available media types that can be encoded with this subtype</returns>
		public static MediaType[] GetOutputMediaTypes(Guid audioSubtype)
		{
			IMFCollection availableTypes;
			try
			{
				MediaFoundationInterop.MFTranscodeGetAudioOutputAvailableTypes(
					audioSubtype, _MFT_ENUM_FLAG.MFT_ENUM_FLAG_ALL, null, out availableTypes);
			}
			catch (COMException c)
			{
				if (c.GetHResult() == MediaFoundationErrors.MF_E_NOT_FOUND)
				{
					// Don't worry if we didn't find any - just means no encoder available for this type
					return new MediaType[0];
				}
				else
				{
					throw;
				}
			}
			availableTypes.GetElementCount(out int count);
			var mediaTypes = new List<MediaType>(count);
			for (int n = 0; n < count; n++)
			{
				availableTypes.GetElement(n, out object mediaTypeObject);
				var mediaType = (IMFMediaType)mediaTypeObject;
				mediaTypes.Add(new MediaType(mediaType));
			}
			Marshal.ReleaseComObject(availableTypes);
			return mediaTypes.ToArray();
		}

		/// <summary>
		/// Tries to find the encoding media type with the closest bitrate to that specified
		/// </summary>
		/// <param name="audioSubtype">Audio subtype, a value from AudioSubtypes</param>
		/// <param name="inputFormat">Your encoder input format (used to check sample rate and channel count)</param>
		/// <param name="desiredBitRate">Your desired bitrate</param>
		/// <returns>The closest media type, or null if none available</returns>
		public static MediaType SelectMediaType(Guid audioSubtype, WaveFormat inputFormat, int desiredBitRate) => GetOutputMediaTypes(audioSubtype)
				.Where(mt => mt.SampleRate == inputFormat.SampleRate && mt.ChannelCount == inputFormat.Channels)
				.Select(mt => new { MediaType = mt, Delta = Math.Abs(desiredBitRate - mt.AverageBytesPerSecond * 8) })
				.OrderBy(mt => mt.Delta)
				.Select(mt => mt.MediaType)
				.FirstOrDefault();

		private readonly MediaType outputMediaType;
		private readonly string path;
		private readonly MediaType inputMediaType;
		private bool disposed;
		private IMFSinkWriter writer;
		private int streamIndex;
		private long position;
		private bool started;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => throw new NotImplementedException();

		public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		/// <summary>
		/// Creates a new encoder that encodes to the specified output media type
		/// </summary>
		/// <param name="outputMediaType">Desired output media type</param>
		public MediaFoundationWriter(MediaType outputMediaType, WaveFormat inputWaveFormat, string path)
		{
			if (outputMediaType == null)
				throw new ArgumentNullException("outputMediaType");
			this.outputMediaType = outputMediaType;
			this.path = path;
			inputMediaType = new MediaType(inputWaveFormat);

			if (inputWaveFormat.Encoding != WaveFormatEncoding.Pcm && inputWaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
			{
				throw new ArgumentException("Encode input format must be PCM or IEEE float");
			}
		}

		public void StartEncode(string outputFile = null)
		{
			if (started)
				return;
			outputFile ??= path;
			writer = CreateSinkWriter(outputFile);
			writer.AddStream(outputMediaType.MediaFoundationObject, out streamIndex);

			// n.b. can get 0xC00D36B4 - MF_E_INVALIDMEDIATYPE here
			writer.SetInputMediaType(streamIndex, inputMediaType.MediaFoundationObject, null);
			writer.BeginWriting();

			position = 0;
			started = true;
		}

		/// <summary>
		/// Encodes a file
		/// </summary>
		/// <param name="outputFile">Output filename (container type is deduced from the filename)</param>
		/// <param name="inputProvider">Input provider (should be PCM, some encoders will also allow IEEE float)</param>
		public override void Write(byte[] samples, int offset, int count)
		{
			if (!started)
				throw new InvalidOperationException("Encoder isn´t started");

			IMFMediaBuffer buffer = MediaFoundationApi.CreateMemoryBuffer(count);

			do
			{
				int read = ConvertOneBuffer(samples, offset, count, buffer);
				offset += read;
				count -= read;

			} while (count > 0);
		}

		public void StopEncode()
		{
			if (started)
				writer.DoFinalize();
			started = false;
		}

		private static IMFSinkWriter CreateSinkWriter(string outputFile)
		{
			// n.b. could try specifying the container type using attributes, but I think
			// it does a decent job of working it out from the file extension 
			// n.b. AAC encode on Win 8 can have AAC extension, but use MP4 in win 7
			// http://msdn.microsoft.com/en-gb/library/windows/desktop/dd389284%28v=vs.85%29.aspx
			IMFSinkWriter writer;
			IMFAttributes attributes = MediaFoundationApi.CreateAttributes(1);
			attributes.SetUINT32(MediaFoundationAttributes.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, 1);
			try
			{
				MediaFoundationInterop.MFCreateSinkWriterFromURL(outputFile, null, attributes, out writer);
			}
			catch (COMException e)
			{
				if (e.GetHResult() == MediaFoundationErrors.MF_E_NOT_FOUND)
				{
					throw new ArgumentException("Was not able to create a sink writer for this file extension");
				}
				throw;
			}
			finally
			{
				Marshal.ReleaseComObject(attributes);
			}
			return writer;
		}
		private static long BytesToNsPosition(int bytes, MediaType mediaType)
		{
			long nsPosition = (10000000L * bytes) / mediaType.AverageBytesPerSecond;
			return nsPosition;
		}

		private int ConvertOneBuffer(byte[] samples, int offset, int count, IMFMediaBuffer buffer)
		{
			IMFSample sample = MediaFoundationApi.CreateSample();
			sample.AddBuffer(buffer);

			buffer.Lock(out IntPtr ptr, out int maxLength, out int currentLength);
			long durationConverted = BytesToNsPosition(count, inputMediaType);
			Marshal.Copy(samples, offset, ptr, maxLength);
			buffer.SetCurrentLength(count);
			buffer.Unlock();
			sample.SetSampleTime(position);
			sample.SetSampleDuration(durationConverted);
			writer.WriteSample(streamIndex, sample);

			Marshal.ReleaseComObject(sample);
			Marshal.ReleaseComObject(buffer);
			position += durationConverted;
			return maxLength;
		}

		/// <summary>
		/// Disposes this instance
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (started)
				StopEncode();
			Marshal.ReleaseComObject(writer);
			Marshal.ReleaseComObject(inputMediaType.MediaFoundationObject);
			Marshal.ReleaseComObject(outputMediaType.MediaFoundationObject);
		}

		/// <summary>
		/// Disposes this instance
		/// </summary>
		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				Dispose(true);
			}
			GC.SuppressFinalize(this);
		}

		public override void Flush() => StopEncode();
		public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
		public override void SetLength(long value) => throw new NotImplementedException();

		/// <summary>
		/// Finalizer
		/// </summary>
		~MediaFoundationWriter()
		{
			Dispose(false);
		}
	}
}
