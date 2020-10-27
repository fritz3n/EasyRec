using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Audio
{
	interface ISampleReceiver
	{
		/// <summary>
		/// Receive multiple AudioFragments of audio data, one for each AudioStream
		/// </summary>
		/// <param name="audioFragments">One AudioFragment od audio data for each AudioStream</param>
		void ReceiveSamples(IList<AudioFragment> audioFragments);

		//void Init(ThroughputDescription formats);
	}
}
