using System;
using System.IO;
using System.Threading.Tasks;

namespace WavSplitter
{
	public class WavReader
	{
		public WavHeader Header { get; private set; }

		public bool HasMore
		{
			get { return reader.BaseStream.Position < Header.FileSize + 8; }
		}

		WavChunkHeader currentDataChunkHeader;

		readonly BinaryReader reader;

		public WavReader (Stream stream)
		{
			reader = new BinaryReader (stream);

			Header = new WavHeader ();
			Header.Read (stream);
		}

		public WavChunkHeader ReadChunkHeader (bool skipFllr = true)
		{
			currentDataChunkHeader = new WavChunkHeader ();
			currentDataChunkHeader.Read (reader);

			if (skipFllr)
			{
				while (currentDataChunkHeader.ShouldSkip)
				{
					reader.BaseStream.Seek (currentDataChunkHeader.ChunkLength, SeekOrigin.Current);
					currentDataChunkHeader = new WavChunkHeader ();
					currentDataChunkHeader.Read (reader);
				}
			}

			currentDataChunkPosition = 0;

			return currentDataChunkHeader;
		}

		private int currentDataChunkPosition;

		public async Task<int> ReadDataChunk (byte[] buffer)
		{
			if (currentDataChunkHeader == null)
			{
				throw new InvalidOperationException ("Read Data Chunk Header first");
			}

			var dataLength = currentDataChunkHeader.ChunkLength - currentDataChunkPosition;
			if (dataLength > buffer.Length)
			{
				dataLength = buffer.Length;
			}

			var blockAlignDiff = dataLength % Header.BlockAlign;
			if (blockAlignDiff != 0)
			{
				dataLength -= blockAlignDiff;
			}

			var count = await reader.BaseStream.ReadAsync (buffer, 0, dataLength).ConfigureAwait (false);

			currentDataChunkPosition += count;

			if (count != dataLength)
			{
				throw new InvalidOperationException ("Unexpected end of file");
			}

			return count;
		}
	}
}
