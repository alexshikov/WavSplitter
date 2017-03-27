using System;
using System.IO;
using System.Text;

namespace WavSplitter
{
	// See these links as a good reference
	// https://web.archive.org/web/20141213140451/https://ccrma.stanford.edu/courses/422/projects/WaveFormat/
	// https://msdn.microsoft.com/en-us/library/ff827591.aspx
	public class WavHeader
	{
		public const int Size = 36;

		public string ChunkID { get; private set; }
		public int FileSize { get; private set; }
		public string RiffType { get; private set; }
		public string FormatChunkID { get; private set; }
		public int FormatChunkLength { get; private set; }
		public int AudioFormat { get; private set; }
		public int Channels { get; private set; }
		public int SampleRate { get; private set; }
		public int AvarageBytePerSecond { get; private set; }
		public int BlockAlign { get; private set; }
		public int BitsPerSample { get; private set; }

		public void Read (Stream stream)
		{
			var reader = new BinaryReader (stream);

			ChunkID = Encoding.UTF8.GetString (reader.ReadBytes (4), 0, 4); // "RIFF"
			FileSize = reader.ReadInt32 (); // whole file size - 8 (current and previous 8 bytes)
			RiffType = Encoding.UTF8.GetString (reader.ReadBytes (4), 0, 4); // "WAVE"
			FormatChunkID = Encoding.UTF8.GetString (reader.ReadBytes (4), 0, 4); // "fmt "
			FormatChunkLength = reader.ReadInt32 (); // 16 for PCM
			AudioFormat = reader.ReadInt16 (); // PCM=1
			Channels = reader.ReadInt16 ();
			SampleRate = reader.ReadInt32 ();
			AvarageBytePerSecond = reader.ReadInt32 ();
			BlockAlign = reader.ReadInt16 ();
			BitsPerSample = reader.ReadInt16 (); // bits per sample

			if (FormatChunkLength == 18)
			{
				throw new NotImplementedException ($"Format Chunk Lenght {FormatChunkLength} is not supported");
				// Read any extra values
				//int fmtExtraSize = reader.ReadInt16 ();
				//reader.ReadBytes (fmtExtraSize);
			}
		}
	}
}
