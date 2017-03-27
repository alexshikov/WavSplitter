using System;
using NUnit.Framework;
using System.IO;
using WavSplitter;
using System.Threading.Tasks;

namespace WavSplitter.Test
{
	[TestFixture]
	public class WriterTest
	{
		private WavHeader GetHeader ()
		{
			using (var input = Helper.GetAudioStream ())
			{
				var reader = new WavReader (input);
				return reader.Header;
			}
		}

		[Test]
		public void Header ()
		{
			WavHeader header = GetHeader ();

			using (var memory = new MemoryStream ())
			{
				var writer = new WavWriter (memory, header);
				writer.ToString (); // we just need it to process header

				Assert.AreEqual (WavHeader.Size + WavChunkHeader.Size, memory.Position);
			}
		}

		[Test]
		public async Task FlushUpdatesSizes ()
		{
			WavHeader header = GetHeader ();

			using (var memory = new MemoryStream ())
			{
				var writer = new WavWriter (memory, header);

				var data = new byte[12];
				await writer.Write (data, 0, data.Length);
				writer.Flush ();

				// read data, that has been written above
				memory.Position = 0;
				var reader = new WavReader (memory);
				var h = reader.ReadChunkHeader ();

				Assert.AreEqual (WavHeader.Size + WavChunkHeader.Size - 8 + data.Length, reader.Header.FileSize);
				Assert.AreEqual (data.Length, h.ChunkLength);
			}
		}
	}
}
