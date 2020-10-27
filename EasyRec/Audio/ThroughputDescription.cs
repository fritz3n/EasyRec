using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyRec.Audio
{
	// Describes the AudioStreams being being put through the Pipeline
	struct ThroughputDescription : IList<StreamDescription>
	{
		private readonly IList<StreamDescription> streamDescriptions;

		public ThroughputDescription(IList<StreamDescription> StreamDescriptions)
		{
			streamDescriptions = StreamDescriptions;
		}

		public static ThroughputDescription FromStreams(IEnumerable<AudioStream> streams) => new ThroughputDescription(streams.Select(w => w.Description).ToList());

		public StreamDescription this[int index] { get => streamDescriptions[index]; set => streamDescriptions[index] = value; }

		public int Count => streamDescriptions.Count;

		public bool IsReadOnly => streamDescriptions.IsReadOnly;

		public void Add(StreamDescription item) => streamDescriptions.Add(item);
		public void Clear() => streamDescriptions.Clear();
		public bool Contains(StreamDescription item) => streamDescriptions.Contains(item);
		public void CopyTo(StreamDescription[] array, int arrayIndex) => streamDescriptions.CopyTo(array, arrayIndex);
		public IEnumerator<StreamDescription> GetEnumerator() => streamDescriptions.GetEnumerator();
		public int IndexOf(StreamDescription item) => streamDescriptions.IndexOf(item);
		public void Insert(int index, StreamDescription item) => streamDescriptions.Insert(index, item);
		public bool Remove(StreamDescription item) => streamDescriptions.Remove(item);
		public void RemoveAt(int index) => streamDescriptions.RemoveAt(index);
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)streamDescriptions).GetEnumerator();
	}
}
