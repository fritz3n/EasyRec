using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRec.Audio
{
	static class FormatExtensions
	{
		public static string ToStringBetter(this WaveFormat format)
		{
			return $"{format.Encoding} {format.BitsPerSample}bit {format.SampleRate}Hz {format.Channels}ch";
		}
	}
}
