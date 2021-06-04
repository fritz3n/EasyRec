using log4net;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRec.Audio.Conversion
{
	class WrappedProvider : IWaveProvider
	{
		private static ILog log = LogManager.GetLogger(nameof(WrappedProvider));
		private readonly IWaveProvider source;
		private int consecutiveThrows = 0;
		private const int maxConsecutiveThrows = 10;

		public WrappedProvider(IWaveProvider source)
		{
			this.source = source;
			log.Info("New wrapped provider for " + source);
		}

		public WaveFormat WaveFormat => source.WaveFormat;

		public int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				int result = source.Read(buffer, offset, count);
				consecutiveThrows = 0;
				return result;
			}
			catch (Exception e)
			{
				log.Info("Exception in wrapped provider for " + source + ":\n" + e);
				consecutiveThrows++;
				if (consecutiveThrows >= maxConsecutiveThrows)
					throw;
				return 0;
			}
		}
	}
}
