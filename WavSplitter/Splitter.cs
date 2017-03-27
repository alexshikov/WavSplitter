using System;
using System.IO;
using System.Threading.Tasks;

namespace WavSplitter
{
	public class Splitter
	{
		readonly WavReader reader;
		readonly int fileSizeLimit; // bytes

		private WavChunkHeader currentDataChunkHeader;
		private int bytesToReadFromDataChunk;
		//private long position;

		private readonly byte[] buffer;

		public Splitter (Stream input, int fileSizeLimit)
		{
			this.reader = new WavReader (input);
			this.fileSizeLimit = fileSizeLimit;

			buffer = new byte[fileSizeLimit];

			currentDataChunkHeader = reader.ReadChunkHeader ();
			bytesToReadFromDataChunk = currentDataChunkHeader.ChunkLength;

			//position = reader.Header.HeaderSize;
		}

		// returns TRUE if has more to read
		public async Task<bool> ReadNext (Stream output)
		{
			var header = reader.Header;
			var writer = new WavWriter (output, header);

			long size = WavHeader.Size + WavChunkHeader.Size;

			bool hasMore = true;

			do
			{
				var count = await reader.ReadDataChunk (buffer).ConfigureAwait (false);
				size += count;

				writer.Write (buffer, 0, count);

				bytesToReadFromDataChunk -= count;

				hasMore = reader.HasMore;

				if (bytesToReadFromDataChunk == 0 && hasMore)
				{
					currentDataChunkHeader = reader.ReadChunkHeader ();
					bytesToReadFromDataChunk = currentDataChunkHeader.ChunkLength;
				}

			} while (size < fileSizeLimit && hasMore);

			writer.Flush ();

			return hasMore;
		}
	}
}
