using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WavSplitter.Test
{
	[TestFixture]
	public class ReaderTest
	{
		[Test]
		public void Header ()
		{
			using (var input = Helper.GetAudioStream ())
			{
				var reader = new WavReader (input);

				Assert.AreEqual (WavConst.Riff, reader.Header.ChunkID, "ChunkID");
				Assert.AreEqual (WavConst.Wave, reader.Header.RiffType, "RiffType");
				Assert.AreEqual (WavConst.Fmt, reader.Header.FormatChunkID, "FormatChunkID");
				Assert.AreEqual (16, reader.Header.FormatChunkLength, "FormatChunkLength");
				Assert.AreEqual (1, reader.Header.AudioFormat, "AudioFormat");
				Assert.AreEqual (1, reader.Header.Channels, "Channels");
				Assert.AreEqual (8000, reader.Header.SampleRate, "SampleRate");
				Assert.AreEqual (16000, reader.Header.AvarageBytePerSecond, "AvarageBytePerSecond");
				Assert.AreEqual (2, reader.Header.BlockAlign, "BlockAlign");
				Assert.AreEqual (16, reader.Header.BitsPerSample, "BitsPerSample");
			}
		}

		[Test]
		public void HeaderSize ()
		{
			using (var input = Helper.GetAudioStream ())
			{
				var reader = new WavReader (input);
				reader.ToString (); // we don't need the reader, but we need it's initialization operation

				Assert.AreEqual (WavHeader.Size, input.Position);
			}
		}

		[Test]
		public void ContentSize ()
		{
			using (var input = Helper.GetAudioStream ())
			{
				var reader = new WavReader (input);

				Assert.AreEqual (70026, reader.Header.FileSize, "FileSize");
			}
		}

		[Test]
		public void FllrChunkHeader ()
		{
			using (var input = Helper.GetAudioStream ())
			{
				var reader = new WavReader (input);
				var chunkHeader = reader.ReadChunkHeader (skipFllr: false);

				Assert.AreEqual (WavConst.Fllr, chunkHeader.ChunkId, "ChunkID");
				Assert.AreEqual (4044, chunkHeader.ChunkLength, "ChunkLength");
			}
		}

		[Test]
		public void DataChunkHeader ()
		{
			using (var input = Helper.GetAudioStream ())
			{
				var reader = new WavReader (input);
				var chunkHeader = reader.ReadChunkHeader ();

				Assert.AreEqual (WavConst.Data, chunkHeader.ChunkId, "ChunkID");
				Assert.AreEqual (65938, chunkHeader.ChunkLength, "ChunkLength");
			}
		}

		[Test]
		public async Task ReadToEnd ()
		{
			using (var input = Helper.GetAudioStream ())
			{
				var reader = new WavReader (input);
				var chunkHeader = reader.ReadChunkHeader ();

				var buffer = new byte[chunkHeader.ChunkLength];
				var count = await reader.ReadDataChunk (buffer);

				Assert.AreEqual (count, buffer.Length, "buffer.Length");
				Assert.AreEqual (false, reader.HasMore, "reader.HasMore");
			}
		}
	}
}
