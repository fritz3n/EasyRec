using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRec.Config
{
	class Config
	{
		public string BufferPath { get; set; }
		public string RecordPath { get; set; }
		public string BufferPattern { get; set; }
		public string RecordPattern { get; set; }


		public FileWriterType BufferWriter { get; set; }
		public FileWriterType RecordWriter { get; set; }
		public string[] Inputs { get; set; }
		public MixdownType MixdownType { get; set; }
		public float BufferLength { get; set; }
	}
}
