using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio.Conversion
{
    class PCMConversionWaveProvider : IWaveProvider
    {
        private readonly ISampleProvider provider;
        private readonly int targetBytes;

        public WaveFormat WaveFormat { get; }

        public PCMConversionWaveProvider(ISampleProvider provider, int targetBytes)
        {
            if (!(targetBytes == 1 || targetBytes == 2 || targetBytes == 4))
                throw new ArgumentException("PCMConversionWaveProvider only Supports 1, 2 and 4 target-bytes");


            WaveFormat = new WaveFormat(provider.WaveFormat.SampleRate, targetBytes * 8, provider.WaveFormat.Channels);
            this.provider = provider;

            this.targetBytes = targetBytes;
        }


        public int Read(byte[] buffer, int offset, int count)
        {
            float[] floatBuffer = new float[count / targetBytes];
            int read = provider.Read(floatBuffer, 0, count / targetBytes);
            WaveBuffer outWaveBuffer = new WaveBuffer(buffer);

            int calcOffset = offset / targetBytes;

            for (int i = 0; i < read; i++)
            {
                switch (targetBytes)
                {
                    case 1:
                        outWaveBuffer.ByteBuffer[calcOffset + i] = (byte)((floatBuffer[i] + 1) * 128f);
                        break;
                    case 2:
                        outWaveBuffer.ShortBuffer[calcOffset + i] = (short)(floatBuffer[i] * 32768f);
                        break;
                    case 4:
                        outWaveBuffer.IntBuffer[calcOffset + i] = (int)(floatBuffer[i] * 2147483648f);
                        break;
                }
            }

            return read * targetBytes;
        }
    }
}
