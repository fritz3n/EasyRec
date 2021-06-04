using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRec.Audio
{
	class WasapiProvider : IWaveProvider, IDisposable
	{
		private static MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
		private WasapiCapture capture;
		private BufferedWaveProvider buffer;

		public WaveFormat WaveFormat => buffer.WaveFormat;

		public AudioStream AudioStream => new AudioStream(Name, this);
		public string Name { get; }

		/// <summary>
		/// Creates a new IWaveProvider using a Wasapi Capture device
		/// </summary>
		/// <param name="id">The ID of the Wasapi Device</param>
		/// <param name="inputLatency">Length of Wasapi buffer in ms, or -1 for automatic value</param>
		/// <param name="bufferLatency">Length of Wavebuffer in ms, or -1 for automatic value</param>
		public WasapiProvider(string id, int inputLatency = -1, int bufferLatency = -1)
		{

			MMDevice device = null;
			if (id == "<default>")
			{
				device = WasapiCapture.GetDefaultCaptureDevice();
			}
			else if (id == "<defaultLoopback>")
			{
				device = WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice();
			}
			else
			{
				foreach (MMDevice dev in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
				{
					if (dev.ID == id)
					{
						device = dev;
						break;
					}
				}
			}

			if (device == null)
				throw new KeyNotFoundException($"Device with ID '{id}' not found or inactive");

			Name = (device.DataFlow == DataFlow.Capture ? "In " : "Out ") + device.FriendlyName;

			if (device.DataFlow == DataFlow.Capture)
			{
				if (inputLatency == -1)
					capture = new WasapiCapture(device);
				else
					capture = new WasapiCapture(device, false, inputLatency);
			}
			else
			{
				capture = new WasapiLoopbackCapture(device);
			}


			if (bufferLatency == -1)
				buffer = new BufferedWaveProvider(capture.WaveFormat) { DiscardOnBufferOverflow = true };
			else
				buffer = new BufferedWaveProvider(capture.WaveFormat) { DiscardOnBufferOverflow = true, BufferDuration = TimeSpan.FromMilliseconds(bufferLatency) };

			capture.DataAvailable += Capture_DataAvailable;
		}

		public void StartRecording() => capture.StartRecording();
		public void StopRecording() => capture.StopRecording();

		public int Read(byte[] buffer, int offset, int count) => ((IWaveProvider)this.buffer).Read(buffer, offset, count);
		private void Capture_DataAvailable(object sender, WaveInEventArgs e) => buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
		public void Dispose() => ((IDisposable)capture).Dispose();
	}
}
