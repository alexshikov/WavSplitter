using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WavSplitter
{
	public class WavWriter
	{
		public long LastDataChunkLength
		{
			get { return dataChunkSize; }
		}

		readonly Stream output;
		readonly BinaryWriter writer;

		long dataChunkSize;
		long dataChunkPosition;

		public WavWriter (Stream output, WavHeader header)
		{
			this.output = output;
			writer = new BinaryWriter (output, Encoding.UTF8);

			WriteHeader (header);
		}

		private void WriteHeader (WavHeader header)
		{
			writer.Write (Encoding.UTF8.GetBytes (WavConst.Riff));
			//writer.Write (header.ChunkID);
			writer.Write (0); // wait for Flush
			writer.Write (Encoding.UTF8.GetBytes (WavConst.Wave));
			//writer.Write (header.RiffType);
			writer.Write (Encoding.UTF8.GetBytes (WavConst.Fmt));
			//writer.Write (header.FormatChunkID);
			writer.Write (header.FormatChunkLength);
			writer.Write ((short)header.AudioFormat);
			writer.Write ((short)header.Channels);
			writer.Write (header.SampleRate);
			writer.Write (header.AvarageBytePerSecond);
			writer.Write ((short)header.BlockAlign);
			writer.Write ((short)header.BitsPerSample);

			// TODO Write extra values
			if (header.FormatChunkLength == 18)
			{
				throw new NotSupportedException ("Extra values are not supported");
			}

			writer.Write (Encoding.UTF8.GetBytes (WavConst.Data));

			dataChunkPosition = writer.BaseStream.Position;

			writer.Write (0); // wait for Flush
		}

		public void SetCurrentChunkId (string chunkId)
		{
			var pos = writer.BaseStream.Position;
			writer.BaseStream.Seek (dataChunkPosition - 4, SeekOrigin.Begin);
			writer.Write (Encoding.UTF8.GetBytes (chunkId));
			writer.BaseStream.Position = pos;
		}

		public void StartNewDataChunk (string chunkId)
		{
			Flush ();

			writer.Write (Encoding.UTF8.GetBytes (chunkId));

			dataChunkPosition = writer.BaseStream.Position;
			dataChunkSize = 0;

			writer.Write (0); // wait for Flush
		}

		public async Task Write (byte[] data, int offset, int count)
		{
			if (output.Length + count > UInt32.MaxValue)
				throw new ArgumentException ("WAV file too large", nameof (count));
			await output.WriteAsync (data, offset, count).ConfigureAwait (false);
			dataChunkSize += count;
		}

		public void Flush ()
		{
			var pos = output.Position;
			UpdateHeader ();
			output.Position = pos;
		}

		private void UpdateHeader ()
		{
			writer.Flush ();

			// update Riff Size (Total Chunk size)
			writer.Seek (4, SeekOrigin.Begin);
			writer.Write ((uint)(output.Length - 8));

			// update Data Chunk Size
			writer.Seek ((int)dataChunkPosition, SeekOrigin.Begin);
			writer.Write ((uint)dataChunkSize);
		}
	}
}
